using System;
using System.Collections.Generic;
using System.Text;

namespace Tableau.HyperAPI.Test
{
    class IntervalUtil
    {
        private const long MicrosecondsPerSecond = 1_000_000;
        private const long MicrosecondsPerMinute = 60 * MicrosecondsPerSecond;
        private const long MicrosecondsPerHour = 60 * MicrosecondsPerMinute;

        /// <summary>
        /// Constructs an <c>Interval</c> from the given number of days.
        /// </summary>
        /// <param name="days">Number of days.</param>
        internal static Interval FromDays(int days)
        {
            return new Interval(0, days, 0);
        }

        /// <summary>
        /// Constructs an <c>Interval</c> from the given number of years and months.
        /// </summary>
        /// <param name="years">Number of years.</param>
        /// <param name="months">Number of months.</param>
        internal static Interval FromYearsAndMonths(int years, int months)
        {
            return new Interval(12 * years + months, 0, 0);
        }

        /// <summary>
        /// Constructs an <c>Interval</c> from the given number of hours, minutes, seconds, and microseconds.
        /// </summary>
        /// <param name="hours">Number of hours.</param>
        /// <param name="minutes">Number of minutes.</param>
        /// <param name="seconds">Number of seconds.</param>
        /// <param name="microseconds">Number of microseconds.</param>
        internal static Interval FromTime(int hours, int minutes = 0, int seconds = 0, int microseconds = 0)
        {
            return new Interval(0, 0,
                microseconds + seconds * MicrosecondsPerSecond +
                minutes * MicrosecondsPerMinute + hours * MicrosecondsPerHour);
        }

        /// <summary>
        /// Constructs an <c>Interval</c> from the given number of seconds and microseconds.
        /// </summary>
        /// <param name="seconds">Number of seconds.</param>
        /// <param name="microseconds">Number of microseconds.</param>
        internal static Interval FromSeconds(int seconds, int microseconds = 0)
        {
            return FromTime(0, 0, seconds, microseconds);
        }

        /// <summary>
        /// Constructs an <c>Interval</c> from the given number of seconds.
        /// </summary>
        /// <param name="seconds">Number of seconds.</param>
        internal static Interval FromSeconds(double seconds)
        {
            return new Interval(0, 0, (long)(seconds * MicrosecondsPerSecond));
        }
    }
}
