using System;
using System.Linq;

using AlgoTrader.Algos;
using AlgoTrader.Feeds;
using AlgoTrader.Optimise;
using AlgoTrader.Evaluate;
using AlgoTrader.Exchanges;
using AlgoTrader.Core.Model;
using AlgoTrader.Backtesting;
using AlgoTrader.Core.Model.Algo;
using AlgoTrader.Core.Extensions;
using AlgoTrader.Core.DTO.Binance;
using AlgoTrader.Core.Model.Order;
using AlgoTrader.Core.Model.Backtest;
using AlgoTrader.Core.Model.Optimise;

using CryptoTools;

using Newtonsoft.Json;

using NLog;

namespace AlgoTrader.Console
{
    class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static string TabDivider => "\t\t\t";

        static void Main(string[] args)
        {
            //Optimise();
            //Backtest();
            //TwitterAnalyse();
            //EstimateSlippageFromOrderbooks();
            RunExchangeFeed();

            //var now = DateTime.UtcNow;
            //var min1 = TimeFrameEnum.Minute1.GetNextCandleOpen();
            //var min15 = TimeFrameEnum.Minute15.GetNextCandleOpen();
            //var h1 = TimeFrameEnum.Hour1.GetNextCandleOpen();
            //var h4 = TimeFrameEnum.Hour4.GetNextCandleOpen();
            //var h12 = TimeFrameEnum.Hour12.GetNextCandleOpen();
            //var day1 = TimeFrameEnum.Day1.GetNextCandleOpen();
            //var week1 = TimeFrameEnum.Week1.GetNextCandleOpen();
            //var month1 = TimeFrameEnum.Month1.GetNextCandleOpen();

            while (true)
            {
                var cmd = System.Console.ReadLine();
            }
        }

        private static void Optimise()
        {
            var evaluator = new SharpeRatioEvaluator(3.636);

            var optimiser = new BruteForceOptimiser<DBPM, Binance>(
                "C:\\Users\\Žiga\\Desktop\\exchange-csv\\yahoo\\BTC-USD-1D.csv", // training
                "C:\\Users\\Žiga\\Desktop\\exchange-csv\\yahoo\\BTC-USD-1D.csv", // test
                evaluator,
                GetDBPMBruteForceOptimisationParameters,
                new OptimisationOptions
                {
                    WorkerCount = 4,
                    BacktestOptions = new BacktestOptions
                    {
                        LogOrders = false
                    }
                }
            );

            var task = optimiser.Run();
            task.Wait();
            var res = task.Result;
            var ratios = evaluator.GetAdditionalBacktestData()[0] as SharpeRatioBacktestData;
            
            System.Console.WriteLine(string.Format("{0,-25}|{1,-26}|{2,-25}", "Parameters", " Sharpe (Training)", " Sharpe (Test)"));
            System.Console.WriteLine(string.Format("--------------------------------------------------------------------------------"));
            for (var i = 0; i < res.Results.Count; i++)
            {
                var par = res.Results[i].Training.Options.AlgoParams; // Training and Test AlgoParams are the same
                var training = ratios.TrainingSharpeRatios[i];
                var test = ratios.TestSharpeRatios[i];

                System.Console.WriteLine(string.Format("{0,-25}| {1,-25}| {2,-25}", JsonConvert.SerializeObject(par), training, test));
            }
        }

        private static object[][] GetDBPMBruteForceOptimisationParameters()
        {
            return new object[][]
            {
                //Enumerable.Range(20, 30).Select(x => x as object).ToArray(),
                //Enumerable.Range(40, 45).Select(x => x as object).ToArray()
                Enumerable.Range(10, 10).Select(x => x as object).ToArray(),
                Enumerable.Range(5, 10).Select(x => x as object).ToArray()
            };
        }

        private static object[][] GetSMABruteForceOptimisationParameters()
        {
            return new object[][]
            {
                //Enumerable.Range(20, 30).Select(x => x as object).ToArray(),
                //Enumerable.Range(40, 45).Select(x => x as object).ToArray()
                Enumerable.Range(20, 13).Select(x => x as object).ToArray(),
                Enumerable.Range(68, 13).Select(x => x as object).ToArray()
            };
        }

        private static void Backtest()
        {
            var backtester = new Backtester<SMAAlgo, Binance>("C:\\Users\\Žiga\\Desktop\\exchange-csv\\yahoo\\BTC-USD-1D.csv", new BacktestOptions
            {
                LogOrders = true,
                AlgoParams = new object[] { 21, 69 }
            }, new AlgoOptions
            {
                TrailStops = false
            });
            
            var task = backtester.Run();
            task.Wait();
            var res = task.Result;
        }

        private static async void EstimateSlippageFromOrderbooks()
        {
            var estimator = new OrderbookSlippageEstimator("orderbooks", 100000, OrderSide.Buy);
            var slippage = await estimator.Run();
        }

        private static void RunExchangeFeed()
        {
            //var b = new Binance();
            //var feed = b.GetCandlestickFeed(Core.DTO.Binance.CurrencyPairs.BTCUSDT, TimeFrameEnum.Minute1).Result;
            ////var feed = new ExchangeCandleCloseFeed<Binance>(Core.DTO.Binance.CurrencyPairs.BTCUSDT, TimeFrameEnum.Minute1);
            //feed.Subscribe((s, e) =>
            //{
            //    logger.Debug(string.Format("GOT DATA {0}: {1} ", DateTime.UtcNow.ToString("o"), JsonConvert.SerializeObject(e)));
            //});

            //feed.Start();


            var feed = new ExchangeCandleCloseFeed<Binance>(Core.DTO.Binance.CurrencyPairs.BTCUSDT, TimeFrameEnum.Minute1);
            feed.Subscribe((s, e) =>
            {
                logger.Debug(string.Format("GOT DATA {0}: {1} ", DateTime.UtcNow.ToString("o"), JsonConvert.SerializeObject(e)));
            });
            feed.Start();
        }
    }
}
