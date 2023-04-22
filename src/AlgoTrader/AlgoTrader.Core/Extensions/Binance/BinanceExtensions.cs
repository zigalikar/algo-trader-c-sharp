using System;
using System.Linq;
using System.Collections.Generic;

using AlgoTrader.Core.Model;
using AlgoTrader.Core.Model.Order;

namespace AlgoTrader.Core.Extensions.Binance
{
    public static class BinanceExtensions
    {
        private static readonly IDictionary<string, TimeFrameEnum> _intervalsTimeFrames = new Dictionary<string, TimeFrameEnum>
        {
            { "1m", TimeFrameEnum.Minute1 },
            { "15m", TimeFrameEnum.Minute15 },
            { "30m", TimeFrameEnum.Minute30 },
            { "1h", TimeFrameEnum.Hour1 },
            { "4h", TimeFrameEnum.Hour4 },
            { "12h", TimeFrameEnum.Hour12 },
            { "1d", TimeFrameEnum.Day1 },
            { "1w", TimeFrameEnum.Week1 },
            { "1M", TimeFrameEnum.Month1 }
        };

        public static string ToBinanceTimeFrameInterval(this TimeFrameEnum tf)
        {
            var i = _intervalsTimeFrames.ToList().FirstOrDefault(x => x.Value == tf);
            if (string.IsNullOrWhiteSpace(i.Key) == false)
                return i.Key;
            throw new NotImplementedException(string.Format("Time frame '{0}' not supported in {0}.", tf.ToString(), nameof(ToBinanceTimeFrameInterval)));
        }

        public static TimeFrameEnum FromBinanceIntervalToTimeFrameEnum(this string i)
        {
            if (_intervalsTimeFrames.TryGetValue(i, out TimeFrameEnum tf))
                return tf;
            throw new NotImplementedException(string.Format("Interval '{0}' not supported in {1}.", i, nameof(FromBinanceIntervalToTimeFrameEnum)));
        }

        public static string ToBinanceOrderSide(this OrderSide side)
        {
            if (side == OrderSide.Buy)
                return "BUY";
            else if (side == OrderSide.Sell)
                return "SELL";

            throw new NotImplementedException(string.Format("Side '{0}' not supported in ToBinanceOrderSide.", side.ToString()));
        }

        public static string ToBinanceOrderType(this OrderType type)
        {
            if (type == OrderType.Market)
                return "MARKET";
            else if (type == OrderType.StopMarket)
                return "STOP_LOSS";
            else if (type == OrderType.Limit)
                return "LIMIT";
            else if (type == OrderType.StopLimit)
                return "STOP_LOSS_LIMIT";

            throw new NotImplementedException(string.Format("Type '{0}' not supported in ToBinanceOrderType.", type.ToString()));
        }
    }
}
