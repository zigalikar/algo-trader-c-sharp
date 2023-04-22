using AlgoTrader.Algos;
using AlgoTrader.Core.Model.Order;
using AlgoTrader.Backtesting;
using AlgoTrader.Core.Extensions;
using AlgoTrader.Core.Model.Backtest;
using AlgoTrader.Exchanges;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using AlgoTrader.Core.Model;
using AlgoTrader.Backtest.Helpers.Optimise;
using AlgoTrader.Backtest.Helpers.Backtest;
using AlgoTrader.Core.Interfaces;
using AlgoTrader.Backtest.Helpers;

namespace AlgoTrader.Backtest
{
    class Program
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            RunBacktest();
            //RunOptimise();

            while (true)
                Console.ReadLine();
        }

        private static async Task RunOptimise()
        {
            try
            {
                var optimiser = new DBPMOptimiserUI();
                await optimiser.Run();
            }
            catch (Exception ex)
            {

            }
        }

        private static async Task RunBacktest()
        {
            try
            {
                var backtester = new DBPMBacktesterUI();
                await backtester.Run();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
