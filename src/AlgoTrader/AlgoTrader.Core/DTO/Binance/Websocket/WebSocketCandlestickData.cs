using System;

using AlgoTrader.Core.Model;
using AlgoTrader.Core.JsonConverters;
using AlgoTrader.Core.JsonConverters.Binance;

using Newtonsoft.Json;

namespace AlgoTrader.Core.DTO.Binance.Websocket
{
    public class WebSocketCandlestickData : WebSocketMessage
    {
        [JsonProperty("t")]
        [JsonConverter(typeof(MillisecondsToDateTimeConverter))]
        public DateTime StartTime { get; set; }

        [JsonProperty("T")]
        [JsonConverter(typeof(MillisecondsToDateTimeConverter))]
        public DateTime CloseTime { get; set; }

        [JsonProperty("i")]
        [JsonConverter(typeof(IntervalToTimeFrameConverter))]
        public TimeFrameEnum TimeFrame { get; set; }

        [JsonProperty("f")]
        public long FirstTradeId { get; set; }

        [JsonProperty("L")]
        public long LastTradeId { get; set; }

        [JsonProperty("o")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double OpenPrice { get; set; }

        [JsonProperty("c")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double ClosePrice { get; set; }

        [JsonProperty("h")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double HighPrice { get; set; }

        [JsonProperty("l")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double LowPrice { get; set; }

        [JsonProperty("v")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double BaseVolume { get; set; }

        [JsonProperty("q")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double QuoteVolume { get; set; }

        [JsonProperty("n")]
        public int NumberOfTrades { get; set; }

        [JsonProperty("x")]
        public bool Closed { get; set; }

        [JsonProperty("V")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double TakerBuyBaseAssetVolume { get; set; }

        [JsonProperty("Q")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double TakerBuyQuoteAssetVolume { get; set; }

        /// <summary>
        /// Converts the data to Binance candlestick data response object
        /// </summary>
        /// <returns>Candlestick data response object</returns>
        public CandlestickDataResponse ToCandlestickDataResponse()
        {
            if (!Closed)
                throw new Exception(string.Format("Cannot construct {0} if the candlestick has not closed yet.", nameof(CandlestickDataResponse)));

            return CandlestickDataResponse.FromValues(StartTime, OpenPrice, HighPrice, LowPrice, ClosePrice, BaseVolume, CloseTime, QuoteVolume, NumberOfTrades, TakerBuyBaseAssetVolume, TakerBuyQuoteAssetVolume);
        }
    }
}
