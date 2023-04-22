using System.Threading.Tasks;

using AlgoTrader.Algos;
using AlgoTrader.Exchanges;
using AlgoTrader.Core.Model;
using AlgoTrader.Backtesting;
using AlgoTrader.Core.Model.Backtest;

namespace AlgoTrader.Chart.Model
{
    public static class BacktesterWrapper
    {
        public static async Task<BacktestResult> Backtest()
        {
            //var backtester = new Backtester<DBPM, Binance>("C:\\Users\\Žiga\\Desktop\\exchange-csv\\quantconnect\\EUR-USD-1D.csv", new BacktestOptions
            //var backtester = new Backtester<DBPM, Binance>("C:\\Users\\Žiga\\Desktop\\exchange-csv\\quantconnect\\printed-from-console\\eur-usd.csv", new BacktestOptions
            var backtester = new Backtester<SMAAlgo, Binance>("C:\\Users\\Žiga\\Desktop\\exchange-csv\\yahoo\\BTC-USD-1D.csv", new BacktestOptions
            //var backtester = new Backtester<DBPM, Binance>("C:\\Users\\Žiga\\Desktop\\exchange-csv\\ftx script\\output.csv", new BacktestOptions
            //var backtester = new Backtester<DBPM, Binance>("C:\\Users\\Žiga\\Desktop\\exchange-csv\\custom\\ftx\\btcusd", TimeFrameEnum.Minute1, new BacktestOptions
            {
                AlgoParams = new object[] { 11, 6 }
            });
            
            return await backtester.Run();
        }
    }
}
