using System;
using System.Runtime.InteropServices;

namespace Tableau.HyperAPI
{
    /// <summary>
    /// Time interval - an object which encodes a Hyper INTERVAL value.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An interval consists of three logically independent components: number of months, number of days,
    /// and number of seconds (with microsecond precision). It can encode relative time offsets
    /// like "1 year" (equivalent to "12 months", but takes variable number of days depending on whether
    /// the year is a leap year), "1 month" (variable number of days depending on the month and whether
    /// the year is a leap year), "2 days" (which may or may not be equivalent to 48 hours depending on
    /// when daylight savings change happens), "3 hours 20 minutes".
    /// </para>
    /// <para>
    /// These components are independent, and there is no automatic conversion. For example an interval
    /// of one day is not equal to an interval of <c>24*60*60</c> seconds.
    /// </para>
    /// <para>
    /// Do not convert <see cref="Interval"/> to <c>TimeSpan</c> and back, except when the interval
    /// consists only of the microseconds component.
    /// </para>
    /// <para>
    /// Arithmetic operations are performed independently on the month, day, and second parts.
    /// </para>
    /// </remarks>
    [StructLayout(LayoutKind.Explicit)]
    public struct Interval : IComparable, IComparable<Interval>, IEquatable<Interval>, IFormattable
    {
        private const long MicrosecondsPerSecond = 1_000_000;
        private const long MicrosecondsPerMinute = 60 * MicrosecondsPerSecond;
        private const long MicrosecondsPerHour = 60 * MicrosecondsPerMinute;
        // .NET tick is 100ns
        private const long TicksPerMicrosecond = 10;
        private const long TicksPerDay = 24 * MicrosecondsPerHour * TicksPerMicrosecond;

        // field layout is fixed, it's what hyper binary format has

        [FieldOffset(0)]
        private long microseconds;
        [FieldOffset(8)]
        private int days;
        [FieldOffset(12)]
        private int months;

        /// <summary>
        /// Constructs an <c>Interval</c> from the given values of months, days, and microseconds.
        /// </summary>
        /// <param name="months"></param>
        /// <param name="days"></param>
        /// <param name="microseconds"></param>
        public Interval(int months, int days, long microseconds)
        {
            this.months = months;
            this.days = days;
            this.microseconds = microseconds;
        }

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

        /// <summary>
        /// Logical months part of the <c>Interval</c> value. It is independent of <see cref="Days"/> and <see cref="Microseconds"/>.
        /// </summary>
        public int Months => months;

        /// <summary>
        /// Logical days part of the <c>Interval</c> value. It is independent of <see cref="Months"/> and <see cref="Microseconds"/>.
        /// </summary>
        public int Days => days;

        /// <summary>
        /// Logical microseconds part of the <c>Interval</c> value. It is independent of <see cref="Months"/> and <see cref="Days"/>.
        /// </summary>
        public long Microseconds => microseconds;

        /// <summary>
        /// Convenience property which returns years part in the (years, months) representation: <see cref="Months"/> = <see cref="YearPart"/>
        /// years and <see cref="MonthPart"/> months.
        /// </summary>
        internal int YearPart => months / 12;

        /// <summary>
        /// Convenience property which returns months part in the (years, months) representation: <see cref="Months"/> = <see cref="YearPart"/>
        /// years and <see cref="MonthPart"/> months.
        /// </summary>
        internal int MonthPart => months % 12;

        /// <summary>
        /// Equivalent to <see cref="Days"/>.
        /// </summary>
        internal int DayPart => days;

        /// <summary>
        /// Convenience property which returns hours part in the (hours, minutes, seconds, microseconds) representation:
        /// <see cref="Microseconds"/> = <see cref="HourPart"/> hours, <see cref="MinutePart"/> minutes, <see cref="SecondPart"/> seconds,
        /// and <see cref="MicrosecondPart"/> microseconds.
        /// </summary>
        internal int HourPart => (int)((microseconds / MicrosecondsPerHour) % 24);

        /// <summary>
        /// Convenience property which returns minutes part in the (hours, minutes, seconds, microseconds) representation:
        /// <see cref="Microseconds"/> = <see cref="HourPart"/> hours, <see cref="MinutePart"/> minutes, <see cref="SecondPart"/> seconds,
        /// and <see cref="MicrosecondPart"/> microseconds.
        /// </summary>
        internal int MinutePart => (int)((microseconds / MicrosecondsPerMinute) % 60);

        /// <summary>
        /// Convenience property which returns seconds part in the (hours, minutes, seconds, microseconds) representation:
        /// <see cref="Microseconds"/> = <see cref="HourPart"/> hours, <see cref="MinutePart"/> minutes, <see cref="SecondPart"/> seconds,
        /// and <see cref="MicrosecondPart"/> microseconds.
        /// </summary>
        internal int SecondPart => (int)((microseconds / MicrosecondsPerSecond) % 60);

        /// <summary>
        /// Convenience property which returns microseconds part in the (hours, minutes, seconds, microseconds) representation:
        /// <see cref="Microseconds"/> = <see cref="HourPart"/> hours, <see cref="MinutePart"/> minutes, <see cref="SecondPart"/> seconds,
        /// and <see cref="MicrosecondPart"/> microseconds.
        /// </summary>
        internal int MicrosecondPart => (int)(microseconds % MicrosecondsPerSecond);

        /// <summary>
        /// Compares two <see cref="Interval"/> values. It performs lexicographic comparison of <c>(Months, Days, Microseconds)</c> tuples,
        /// which does not always make sense as comparison of actual time intervals. Do not use this to decide whether one time interval
        /// is longer than another. For example, "48 hours" is smaller than "1 day" when compared with this method.
        /// </summary>
        public int CompareTo(Interval other)
        {
            int cmp = months.CompareTo(other.months);
            if (cmp != 0)
                return cmp;
            cmp = days.CompareTo(other.days);
            if (cmp != 0)
                return cmp;
            return microseconds.CompareTo(other.microseconds);
        }

        int IComparable.CompareTo(object obj)
        {
            switch (obj)
            {
                case null:
                    return 1;

                case Interval interval:
                    return CompareTo(interval);

                default:
                    throw new ArgumentException("Object is not an Interval");
            }
        }

        /// <summary>
        /// Compares two <see cref="Interval"/> values. It performs lexicographic comparison of <c>(Months, Days, Microseconds)</c> tuples,
        /// which does not always make sense as comparison of actual time intervals. Do not use this to decide whether one time interval
        /// equals another. For example, "1 day" is not equal "24 hours" when compared with this method.
        /// </summary>
        /// <param name="other">Interval to compare with.</param>
        /// <returns>Whether this interval compares equal to <c>other</c>.</returns>
        public bool Equals(Interval other)
        {
            return months == other.months && days == other.days && microseconds == other.microseconds;
        }

        /// <summary>
        /// Formats an <see cref="Interval"/> value as string. Result is a string like <c>"P1Y"</c>, <c>"P2D"</c>, <c>"PT1H2M3.456S"</c>.
        /// </summary>
        public override string ToString()
        {
            string result = "P";

            if (months != 0)
            {
                int yearPart = YearPart;
                if (yearPart != 0)
                    result += $"{yearPart}Y";
                int monthPart = MonthPart;
                if (monthPart != 0)
                    result += $"{monthPart}M";
            }

            if (days != 0)
            {
                result += $"{days}D";
            }

            if (microseconds != 0)
            {
                result += "T";
                int hourPart = HourPart;
                int minutePart = MinutePart;
                int secondPart = SecondPart;
                int microsecondPart = MicrosecondPart;
                if (hourPart != 0)
                    result += $"{hourPart}H";
                if (minutePart != 0)
                    result += $"{minutePart}M";
                if (secondPart != 0 || microsecondPart != 0)
                    result += $"{secondPart + ((double)microsecondPart / MicrosecondsPerSecond)}S";
            }
            else if (result.Length == 1)
            {
                result += "T0S";
            }

            return result;
        }

        string IFormattable.ToString(string format, IFormatProvider formatProvider)
        {
            return ToString();
        }

        /// <summary>
        /// Compares two <see cref="Interval"/> values. It performs lexicographic comparison of <c>(Months, Days, Microseconds)</c> tuples,
        /// which does not always make sense as comparison of actual time intervals. Do not use this to decide whether one time interval
        /// equals another. For example, "1 day" is not equal "24 hours" when compared with this method.
        /// </summary>
        /// <param name="obj">Object to compare with.</param>
        /// <returns>Whether this object compares equal to <c>obj</c>.</returns>
        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case Interval interval:
                    return Equals(interval);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Computes the hash code.
        /// </summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode()
        {
            return Impl.HashCode.Combine(months, days, microseconds);
        }

        /// <summary>
        /// Unary plus operator.
        /// </summary>
        /// <param name="interval">The value.</param>
        public static Interval operator +(Interval interval)
        {
            return interval;
        }

        /// <summary>
        /// Add two intervals. It adds months, days, and microseconds components independently.
        /// </summary>
        /// <param name="interval1">Value to add.</param>
        /// <param name="interval2">Value to add.</param>
        public static Interval operator +(Interval interval1, Interval interval2)
        {
            return new Interval(interval1.months + interval2.months, interval1.days + interval2.days, interval1.microseconds + interval2.microseconds);
        }

        /// <summary>
        /// Unary minus operator, negates all components of the interval.
        /// </summary>
        /// <param name="interval">The value.</param>
        public static Interval operator -(Interval interval)
        {
            return new Interval(-interval.months, -interval.days, -interval.microseconds);
        }

        /// <summary>
        /// Subtracts two intervals. It subtracts months, days, and microseconds components independently.
        /// </summary>
        /// <param name="interval1">Value to subtract from.</param>
        /// <param name="interval2">Value to subtract.</param>
        public static Interval operator -(Interval interval1, Interval interval2)
        {
            return new Interval(interval1.months - interval2.months, interval1.days - interval2.days, interval1.microseconds - interval2.microseconds);
        }

        /// <summary>
        /// Compares two <see cref="Interval"/> values. It performs lexicographic comparison of <c>(Months, Days, Microseconds)</c> tuples,
        /// which does not always make sense as comparison of actual time intervals. Do not use this to decide whether one time interval
        /// equals another. For example, "1 day" is not equal "24 hours" when compared with this method.
        /// </summary>
        /// <param name="interval1">Value to compare.</param>
        /// <param name="interval2">Value to compare.</param>
        public static bool operator ==(Interval interval1, Interval interval2)
        {
            return interval1.Equals(interval2);
        }

        /// <summary>
        /// Compares two <see cref="Interval"/> values. It performs lexicographic comparison of <c>(Months, Days, Microseconds)</c> tuples,
        /// which does not always make sense as comparison of actual time intervals. Do not use this to decide whether one time interval
        /// equals another. For example, "1 day" is not equal "24 hours" when compared with this method.
        /// </summary>
        /// <param name="interval1">Value to compare.</param>
        /// <param name="interval2">Value to compare.</param>
        public static bool operator !=(Interval interval1, Interval interval2)
        {
            return !interval1.Equals(interval2);
        }

        /// <summary>
        /// Compares two <see cref="Interval"/> values. It performs lexicographic comparison of (months, days, microseconds) tuples,
        /// which does not always make sense as comparison of actual time intervals. Do not use this to decide whether one time interval
        /// is longer than another. For example, "48 hours" is smaller than "1 day" when compared with this method.
        /// </summary>
        /// <param name="interval1">Value to compare.</param>
        /// <param name="interval2">Value to compare.</param>
        public static bool operator <(Interval interval1, Interval interval2)
        {
            return interval1.CompareTo(interval2) < 0;
        }

        /// <summary>
        /// Compares two <see cref="Interval"/> values. It performs lexicographic comparison of <c>(Months, Days, Microseconds)</c> tuples,
        /// which does not always make sense as comparison of actual time intervals. Do not use this to decide whether one time interval
        /// is longer than another. For example, "48 hours" is smaller than "1 day" when compared with this method.
        /// </summary>
        /// <param name="interval1">Value to compare.</param>
        /// <param name="interval2">Value to compare.</param>
        public static bool operator >(Interval interval1, Interval interval2)
        {
            return interval1.CompareTo(interval2) > 0;
        }

        /// <summary>
        /// Compares two <see cref="Interval"/> values. It performs lexicographic comparison of <c>(Months, Days, Microseconds)</c> tuples,
        /// which does not always make sense as comparison of actual time intervals. Do not use this to decide whether one time interval
        /// is longer than another. For example, "48 hours" is smaller than "1 day" when compared with this method.
        /// </summary>
        /// <param name="interval1">Value to compare.</param>
        /// <param name="interval2">Value to compare.</param>
        public static bool operator <=(Interval interval1, Interval interval2)
        {
            return interval1.CompareTo(interval2) <= 0;
        }

        /// <summary>
        /// Compares two <see cref="Interval"/> values. It performs lexicographic comparison of <c>(Months, Days, Microseconds)</c> tuples,
        /// which does not always make sense as comparison of actual time intervals. Do not use this to decide whether one time interval
        /// is longer than another. For example, "48 hours" is smaller than "1 day" when compared with this method.
        /// </summary>
        /// <param name="interval1">Value to compare.</param>
        /// <param name="interval2">Value to compare.</param>
        public static bool operator >=(Interval interval1, Interval interval2)
        {
            return interval1.CompareTo(interval2) >= 0;
        }
    }
}
