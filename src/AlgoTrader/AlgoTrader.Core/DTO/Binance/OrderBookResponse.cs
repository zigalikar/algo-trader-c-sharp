using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

using AlgoTrader.Core.Model;

using Newtonsoft.Json;

namespace AlgoTrader.Core.DTO.Binance
{
    public class OrderbookResponse
    {
        [JsonProperty("lastUpdateId")]
        public long LastUpdateId { get; set; }

        [JsonProperty("bids")]
        public List<List<string>> Bids { get; set; }

        [JsonProperty("asks")]
        public List<List<string>> Asks { get; set; }
        
        public Orderbook ToOrderbook()
        {
            var transform = new Func<List<string>, OrderbookItem>(x =>
            {
                var price = double.Parse(x[0], CultureInfo.InvariantCulture);
                var quantity = double.Parse(x[1], CultureInfo.InvariantCulture);
                return new OrderbookItem(price, quantity);
            });

            var bids = Bids.Select(transform);
            var asks = Asks.Select(transform);
            return new Orderbook(DateTime.UtcNow, bids, asks); // wrong timestamp
        }
    }
}
