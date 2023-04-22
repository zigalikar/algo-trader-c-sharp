using System;
using System.Linq;
using System.Collections.Generic;

using AlgoTrader.Core.Model;
using AlgoTrader.Core.JsonConverters;

using Newtonsoft.Json;

namespace AlgoTrader.Core.DTO.FTX.Websocket
{
    public class WebSocketOrderbookData
    {
        [JsonProperty("action")]
        public string Action { get; set; }
        
        [JsonProperty("bids")]
        public List<List<double>> Bids { get; set; }

        [JsonProperty("asks")]
        public List<List<double>> Asks { get; set; }

        [JsonProperty("checksum")]
        public long Checksum { get; set; }

        [JsonProperty("time")]
        [JsonConverter(typeof(SecondsToDateTimeConverter))]
        public DateTime Timestamp { get; set; }

        public Orderbook ToOrderbook()
        {
            var transform = new Func<List<double>, OrderbookItem>(x => new OrderbookItem(x[0], x[1]));
            var bids = Bids.Select(transform);
            var asks = Asks.Select(transform);
            return new Orderbook(Timestamp, bids, asks);
        }
    }
}
