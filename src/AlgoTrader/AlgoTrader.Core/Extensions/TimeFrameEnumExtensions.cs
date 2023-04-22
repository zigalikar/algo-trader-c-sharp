using System;

using AlgoTrader.Core.Model;

namespace AlgoTrader.Core.Extensions
{
    /// <summary>
    /// Provides extensions for working with time frames
    /// </summary>
    public static class TimeFrameEnumExtensions
    {
        /// <summary>
        /// Represents the specified time frame as a TimeSpan object
        /// </summary>
        /// <param name="tf">Time frame to convert</param>
        /// <param name="current">Date the time frame is at (used for monthly time frames on leap years)</param>
        /// <returns>TimeSpan representing the specified time frame</returns>
        public static TimeSpan ToTimeSpan(this TimeFrameEnum tf, DateTime? current = null)
        {
            if (current.HasValue == false)
                current = DateTime.UtcNow;

            if (tf == TimeFrameEnum.Minute1)
                return TimeSpan.FromMinutes(1);
            else if (tf == TimeFrameEnum.Minute15)
                return TimeSpan.FromMinutes(15);
            else if (tf == TimeFrameEnum.Minute30)
                return TimeSpan.FromMinutes(30);
            else if (tf == TimeFrameEnum.Hour1)
                return TimeSpan.FromHours(1);
            else if (tf == TimeFrameEnum.Hour4)
                return TimeSpan.FromHours(4);
            else if (tf == TimeFrameEnum.Hour12)
                return TimeSpan.FromHours(12);
            else if (tf == TimeFrameEnum.Day1)
                return TimeSpan.FromDays(1);
            else if (tf == TimeFrameEnum.Week1)
                return TimeSpan.FromDays(7);
            else if (tf == TimeFrameEnum.Month1)
                return TimeSpan.FromDays(DateTime.DaysInMonth(current.Value.Year, current.Value.Month));

            throw new NotImplementedException(string.Format("Interval '{0}' not supported in {1}.", tf.ToString(), nameof(ToTimeSpan)));
        }

        /// <summary>
        /// Returns the next candle open of the timeframe
        /// </summary>
        /// <param name="tf">Timeframe of the candle</param>
        /// <param name="time">Timestamp to check for next open (defaults to UtcNow if not provided)</param>
        /// <returns>Next candle open of the timeframe</returns>
        public static DateTime GetNextCandleOpen(this TimeFrameEnum tf, DateTime? time = null)
        {
            var now = time ?? DateTime.UtcNow.ClearSecondsAndMilliseconds();

            if (tf == TimeFrameEnum.Minute1)
                now = now.RoundDown(tf.ToTimeSpan()).AddMinutes(1);
            else if (tf == TimeFrameEnum.Minute15)
                now = now.RoundDown(tf.ToTimeSpan()).AddMinutes(15);
            else if (tf == TimeFrameEnum.Minute30)
                now = now.RoundDown(tf.ToTimeSpan()).AddMinutes(30);
            else if (tf == TimeFrameEnum.Hour1)
                now = now.RoundDown(tf.ToTimeSpan()).AddHours(1);
            else if (tf == TimeFrameEnum.Hour4)
                now = now.RoundDown(tf.ToTimeSpan()).AddHours(4);
            else if (tf == TimeFrameEnum.Hour12)
                now = now.RoundDown(tf.ToTimeSpan()).AddHours(12);
            else if (tf == TimeFrameEnum.Day1)
                now = now.RoundDown(tf.ToTimeSpan()).AddDays(1);
            else if (tf == TimeFrameEnum.Week1)
                now = now.RoundDown(tf.ToTimeSpan()).AddDays(7);
            else if (tf == TimeFrameEnum.Month1)
                now = now.Subtract(TimeSpan.FromHours(now.Hour)).Subtract(TimeSpan.FromMinutes(now.Minute)).Subtract(TimeSpan.FromDays(now.Day - 1)).AddMonths(1);
            else
                throw new NotImplementedException(string.Format("Interval '{0}' not supported in {1}.", tf.ToString(), nameof(GetNextCandleOpen)));
            
            return now;
        }

        public static DateTime RoundDown(this DateTime dt, TimeSpan accuracy)
        {
            return new DateTime(dt.Ticks / accuracy.Ticks * accuracy.Ticks, dt.Kind);
        }
    }
}
