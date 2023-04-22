using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using AlgoTrader.Core.Model;
using AlgoTrader.Core.Model.Order;

using Newtonsoft.Json;

namespace CryptoTools
{
    /// <summary>
    /// Simulates market buys/sells on orderbook data
    /// </summary>
    public class OrderbookSlippageEstimator
    {
        private readonly string _dataFolder;
        private readonly double _size;
        private readonly OrderSide _side;

        /// <summary>
        /// Creates an orderbook slippage estimator object
        /// </summary>
        /// <param name="dataFolder">Folder of the orderbook data</param>
        /// <param name="size">Size of the simulated trade (quote if market buy, base if market sell)</param>
        /// <param name="side">Type of the market order</param>
        public OrderbookSlippageEstimator(string dataFolder, double size, OrderSide side)
        {
            _dataFolder = dataFolder;
            _size = size;
            _side = side;
        }

        /// <summary>
        /// Estimates the slippage
        /// </summary>
        /// <returns>Average estimated slippage in percentage</returns>
        public async Task<double> Run()
        {
            var di = new DirectoryInfo(_dataFolder);
            
            var slippage = 0d;
            var i = 0;
            foreach (var file in di.EnumerateFiles())
            {
                using (var stream = file.OpenRead())
                using (var reader = new StreamReader(stream))
                {
                    var content = await reader.ReadToEndAsync();
                    var json = JsonConvert.DeserializeObject<Orderbook>(content);

                    var j = 0;
                    var avgPrice = 0d;
                    if (_side == OrderSide.Buy)
                    {
                        var depth = json.Asks.Sum(x => x.Price * x.Quantity);
                        var buys = new List<(double price, double quantity)>();

                        var toBuy = _size;
                        while (toBuy > 0)
                        {
                            double bought = 0d;
                            var ask = json.Asks.ElementAt(j);
                            if (toBuy > ask.Price * ask.Quantity)
                            {
                                bought = ask.Price * ask.Quantity;
                                toBuy -= bought;

                                buys.Add((ask.Price, ask.Quantity));
                            }
                            else
                            {
                                bought = toBuy;
                                toBuy -= bought;

                                buys.Add((ask.Price, bought / ask.Price));
                            }
                            j++;
                        }
                        
                        avgPrice = buys.Sum(x => x.price * x.quantity) / buys.Sum(x => x.quantity);
                    }
                    else if (_side == OrderSide.Sell)
                    {
                        var depth = json.Bids.Sum(x => x.Quantity);
                        var sells = new List<(double price, double quantity)>();

                        var toSell = _size;
                        while (toSell > 0)
                        {
                            double sold = 0d;
                            var bid = json.Bids.ElementAt(j);
                            if (toSell > bid.Quantity)
                            {
                                sold = bid.Quantity;
                                toSell -= sold;

                                sells.Add((bid.Price, bid.Quantity));
                            }
                            else
                            {
                                sold = toSell;
                                toSell -= sold;

                                sells.Add((bid.Price, sold));
                            }
                            j++;
                        }

                        avgPrice = sells.Sum(x => x.price * x.quantity) / sells.Sum(x => x.quantity);
                    }

                    var midPrice = (json.Asks.First().Price + json.Bids.First().Price) / 2; // TODO: get last trade - that's the market price
                    var test = avgPrice / midPrice;
                    var fileSlippage = Math.Abs(avgPrice / midPrice - 1);
                    slippage = (slippage * i + fileSlippage) / ++i;
                }
            }

            return slippage;
        }
    }
}
