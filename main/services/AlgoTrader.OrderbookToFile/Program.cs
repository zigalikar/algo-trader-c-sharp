using System;
using System.Collections.Generic;

using AlgoTrader.Exchanges;
using AlgoTrader.Core.Interfaces;

using CryptoTools;

namespace AlgoTrader.OrderbookToFile
{
    class Program
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static readonly IDictionary<string, IExchange> _exchanges = new Dictionary<string, IExchange>
        {
            { "binance", new Binance() },
            { "ftx", new FTX() }
        };

        static void Main(string[] args)
        {
            if (args.Length >= 3)
            {
                var path = args[0];
                var exchange = args[1];
                var market = args[2];

                FetchOrderbooks(path, exchange, market);
            }
            else
                logger.Fatal("Invalid arguments: [save path] [exchange] [market]");

            while (true)
            {
                var cmd = Console.ReadLine();
            }
        }

        private static void FetchOrderbooks(string path, string exchange, string market)
        {
            if (_exchanges.ContainsKey(exchange))
            {
                var ex = _exchanges[exchange];
                var currencyPair = ex.GetCurrencyPair(market);
                if (currencyPair != null)
                {
                    logger.Info(string.Format("Listening to '{0}' trades on '{1}' and saving to path '{2}'.", market, exchange, path));

                    var feed = ex.GetOrderbookFeed(currencyPair).Result;
                    var fetcher = new CryptoTools.OrderbookToFile(path, feed);
                    fetcher.Start();
                }
                else
                    Console.WriteLine("Invalid market parameter");
            }
            else
                Console.WriteLine(string.Format("Invalid exchange parameter, available exchanges: {0}", string.Join(", ", _exchanges.Keys)));
        }
    }
}
