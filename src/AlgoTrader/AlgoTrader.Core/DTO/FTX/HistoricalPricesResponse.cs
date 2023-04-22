using System;
using System.Collections.Generic;

using AlgoTrader.Core.Model;
using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Extensions;
using AlgoTrader.Core.JsonConverters;

using Newtonsoft.Json;

namespace AlgoTrader.Core.DTO.FTX
{
    /// <summary>
    /// Response DTO for FTX historical  prices
    /// </summary>
    public class HistoricalPricesResponse : ResponseBase<List<HistoricalPricesResponseData>> { }

    public class HistoricalPricesResponseData
    {
        [JsonProperty("close")]
        public double ClosePrice { get; set; }

        [JsonProperty("high")]
        public double HighPrice { get; set; }

        [JsonProperty("low")]
        public double LowPrice { get; set; }

        [JsonProperty("open")]
        public double OpenPrice { get; set; }

        [JsonProperty("startTime")]
        [JsonConverter(typeof(DateTimeToUtcConverter))]
        public DateTime OpenTime { get; set; }

        [JsonProperty("volume")]
        public double Volume { get; set; }
    }

    public class HistoricalPricesResponseDataWrapper : HistoricalPricesResponseData, ICandlestick
    {
        public DateTime CloseTime { get; set; }
        
        public HistoricalPricesResponseDataWrapper(HistoricalPricesResponseData data, TimeFrameEnum timeFrame)
        {
            ClosePrice = data.ClosePrice;
            HighPrice = data.HighPrice;
            LowPrice = data.LowPrice;
            OpenPrice = data.OpenPrice;
            OpenTime = data.OpenTime;
            Volume = data.Volume;

            CloseTime = data.OpenTime.Add(timeFrame.ToTimeSpan()).Subtract(TimeSpan.FromTicks(1));
        }
    }
}
