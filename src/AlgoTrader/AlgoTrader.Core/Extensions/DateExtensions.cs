using System;

namespace AlgoTrader.Core.Extensions
{
    /// <summary>
    /// Provides extensions for working with dates
    /// </summary>
    public static class DateExtensions
    {
        private static DateTime Zero => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Returns a DateTime object that is the specified amount of seconds from 1970
        /// </summary>
        /// <param name="seconds">Amount of seconds away from 1970</param>
        /// <returns>DateTime representing the specified time</returns>
        public static DateTime ToDateTime(this long seconds) => Zero.AddSeconds(seconds);

        /// <summary>
        /// Returns the number of seconds the specified DateTime is from 1970+
        /// </summary>
        /// <param name="date">DateTime object to convert</param>
        /// <returns>Long number representing the specified time</returns>
        public static long ToSecondsFromEpoch(this DateTime date) => (long) date.Subtract(Zero).TotalSeconds;

        /// <summary>
        /// Returns the number of milliseconds the specified DateTime is from 1970+
        /// </summary>
        /// <param name="date">DateTime object to convert</param>
        /// <returns>Long number representing the specified time</returns>
        public static long ToMillisecondsFromEpoch(this DateTime date) => (long) date.Subtract(Zero).TotalMilliseconds;

        /// <summary>
        /// Clears the seconds and milliseconds properties of the date
        /// </summary>
        /// <param name="date">DateTime object to clear</param>
        /// <returns>Cleared DateTime object</returns>
        public static DateTime ClearSecondsAndMilliseconds(this DateTime date) => date.AddSeconds(-date.Second).AddMilliseconds(-date.Millisecond);

        /// <summary>
        /// Converts the date to ISO format time string
        /// </summary>
        /// <param name="date">DateTime object to convert</param>
        /// <returns>ISO format of the specified date</returns>
        public static string ToISOString(this DateTime date) => date.ToString("o");
    }
}
