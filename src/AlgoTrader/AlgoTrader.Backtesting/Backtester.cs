using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

using AlgoTrader.Feeds;
using AlgoTrader.Core.DTO;
using AlgoTrader.Core.Model;
using AlgoTrader.Core.Helpers;
using AlgoTrader.Feeds.Helpers;
using AlgoTrader.Exchanges.Core;
using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Model.Algo;
using AlgoTrader.Exchanges.Backtest;
using AlgoTrader.Core.Model.Backtest;
using AlgoTrader.Core.Model.Attributes;

namespace AlgoTrader.Backtesting
{
    /// <summary>
    /// Used for backtesting algos
    /// </summary>
    /// <typeparam name="A">Algo to backtest</typeparam>
    /// <typeparam name="E">Exchange to backtest on</typeparam>
    public class Backtester<A, E> : Backtester where A : IBacktestAlgo where E : IExchange
    {
        /// <summary>
        /// Initiates a new instance of the backtester with candle close CSV data
        /// </summary>
        /// <param name="csvPath">Path to the .csv file with backtesting data</param>
        /// <param name="options">Backtest options</param>
        /// <param name="algoOptions">Algo options</param>
        public Backtester(string csvPath, BacktestOptions backtestOptions = null, AlgoOptions algoOptions = null) : base(csvPath, typeof(A), typeof(E), backtestOptions, algoOptions) { }

        /// <summary>
        /// Initiates a new instance of the backtester with price tick CSV data
        /// </summary>
        /// <param name="folderPath">Path to the folder of .csv files with price tick data</param>
        /// <param name="timeFrame">Time frame to group ticks and test on</param>
        /// <param name="options">Backtest options</param>
        /// <param name="algoOptions">Algo options</param>
        public Backtester(string folderPath, TimeFrameEnum timeFrame, BacktestOptions backtestOptions = null, AlgoOptions algoOptions = null) : base(folderPath, typeof(A), typeof(E), timeFrame, backtestOptions, algoOptions) { }
    }

    public class Backtester
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly BacktestOptions _options;
        private readonly AlgoOptions _algoOptions;
        private readonly IFeed _feed;
        private readonly IBacktestExchange _exchange;
        private readonly IBacktestAlgo _algo;

        private TaskCompletionSource<BacktestResult> _backtestTcs;

        /// <summary>
        /// Backtester constructor for candle close CSV data
        /// </summary>
        /// <param name="csvPath">Path to the backtesting data</param>
        /// <param name="algoType">Type of algo to backtest on</param>
        /// <param name="exchangeType">Type of the exchange to backtest on</param>
        /// <param name="options">Backtest options</param>
        /// <param name="algoOptions">Algo options</param>
        public Backtester(string csvPath, Type algoType, Type exchangeType, BacktestOptions options = null, AlgoOptions algoOptions = null) : this(options, algoOptions)
        {
            var feed = new CSVCandleCloseFeed(csvPath);
            _feed = feed;

            // init exchange
            var currencyPair = new BacktesterCurrencyPair();
            var exchangeConfig = GetExchangeConfig(exchangeType);
            var exchange = new BacktestCandleCloseExchange(currencyPair, exchangeConfig.MakerFee, exchangeConfig.TakerFee, _options);
            _exchange = exchange;
            _exchange.Initialize().Wait();
            feed.Subscribe(exchange.BeforeOnData);

            // init algo
            _algo = GetAlgo(algoType, _feed, currencyPair);

            feed.Subscribe(exchange.OnData);

            logger.Trace("Constructor");
        }

        /// <summary>
        /// Backtester constructor for price tick CSV data
        /// </summary>
        /// <param name="folderPath">Path to the folder of .csv files with price tick data</param>
        /// <param name="algoType">Type of algo to backtest on</param>
        /// <param name="exchangeType">Type of the exchange to backtest on</param>
        /// <param name="timeFrame">Time frame to group ticks and test on</param>
        /// <param name="options">Backtest options</param>
        /// <param name="algoOptions">Algo options</param>
        public Backtester(string folderPath, Type algoType, Type exchangeType, TimeFrameEnum timeFrame, BacktestOptions options = null, AlgoOptions algoOptions = null) : this(options, algoOptions)
        {
            var feed = new CSVPriceTickFeed(folderPath);
            _feed = feed;

            // init exchange
            var currencyPair = new BacktesterCurrencyPair();
            var exchangeConfig = GetExchangeConfig(exchangeType);
            var exchange = new BacktestPriceTickExchange(currencyPair, timeFrame, exchangeConfig.MakerFee, exchangeConfig.TakerFee, _options);
            _exchange = exchange;
            _exchange.Initialize().Wait();
            feed.Subscribe(exchange.OnPriceTickData);

            // init algo
            _algo = GetAlgo(algoType, exchange.GetCandleCloseFeed(), currencyPair);

            logger.Trace("Constructor");
        }

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="options">Backtest options</param>
        /// <param name="algoOptions">Algo options</param>
        private Backtester(BacktestOptions options = null, AlgoOptions algoOptions = null)
        {
            _options = options ?? new BacktestOptions();
            _algoOptions = algoOptions ?? new AlgoOptions();
        }

        private IBacktestAlgo GetAlgo(Type algoType, IFeed algoFeed, ICurrencyPair currencyPair)
        {
            // init algo with parameters
            var algoParams = new List<object> { algoFeed, _exchange, currencyPair, null, _algoOptions };
            if (_options.AlgoParams != null && _options.AlgoParams.Count() > 0)
                algoParams = algoParams.Concat(_options.AlgoParams).ToList();

            // find constructor
            var constr = algoType.GetConstructors().First(c => c.GetParameters().Count() >= algoParams.Count);

            // add constructor parameters
            var constrParams = constr.GetParameters();
            for (var i = algoParams.Count; i < constrParams.Count(); i++)
            {
                var param = constrParams[i];
                if (param.HasDefaultValue)
                    algoParams.Add(param.DefaultValue);
                else
                    throw new ArgumentException("No parameter value supplied for a parameter with no default value - supply values for backtesting/optimisation in the options object in the backtesting/optimisation function", param.Name, null);
            }
            
            return Activator.CreateInstance(algoType, algoParams.ToArray()) as IBacktestAlgo;
        }

        private ExchangeConfig GetExchangeConfig(Type exchangeType)
        {
            var exchangeAttribute = ReflectionHelper.GetTypesWithAttribute<Exchange>(string.Join(".", new string[] { nameof(AlgoTrader), nameof(AlgoTrader.Exchanges) }), Assembly.GetAssembly(typeof(ExchangeBase))).Select(x => x.GetCustomAttribute<Exchange>()).First(x => x.ImplementationClass == exchangeType);
            return exchangeAttribute.GetConfig();
        }

        /// <summary>
        /// Runs the backtester
        /// </summary>
        public Task<BacktestResult> Run()
        {
            if (_backtestTcs != null && _backtestTcs.Task.IsCompleted == false)
                _backtestTcs.SetCanceled();
            _backtestTcs = new TaskCompletionSource<BacktestResult>();

            Task.Run(() =>
            {
                //// subscribe
                //_feed.Subscribe(_exchange.OnData);

                // start
                _feed.Start();

                //// unsubscribe
                //_feed.Unsubscribe(_exchange.OnData);

                // return final backtesting result
                var result = _exchange.GetResult(_algo);
                result.AlgoOptions = _algoOptions;
                _backtestTcs.SetResult(result);
            });

            return _backtestTcs.Task;
        }
        
        private class BacktesterCurrencyPair : ICurrencyPair
        {
            public string Base => "Base";
            public string Quote => "Quote";
        }
    }
}
