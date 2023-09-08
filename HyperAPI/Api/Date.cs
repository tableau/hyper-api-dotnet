using System;
using System.Collections.Generic;
using System.Text;

using Tableau.HyperAPI.Raw;

namespace Tableau.HyperAPI
{
    /// <summary>
    /// Hyper DATE value - a calendar date.
    /// </summary>
    /// <remarks>
    /// <c>Date</c> is almost identical to <c>DateTime</c>, except for two things:
    /// <list type="bullet">
    /// <item><description><c>Date</c> supports a greater range of dates</description>.</item>
    /// <item><description><c>DateTime</c> resolution is .NET tick, which equals to 100 nanoseconds, while
    /// <c>Date</c> resolution is 1 microsecond (10 ticks).</description></item>
    /// </list>
    /// </remarks>
    public struct Date : IComparable, IComparable<Date>, IEquatable<Date>
    {
        internal uint Value { get; }

        internal Date(uint value)
        {
            Value = value;
        }

        /// <summary>
        /// Constructs a <see cref="Date"/> from the specified year, month, and day.
        /// </summary>
        /// <param name="year">Year.</param>
        /// <param name="month">Month (1 through 12).</param>
        /// <param name="day">Day (1 through the number of days in the month).</param>
        public Date(int year, int month, int day)
        {
            Value = Conversions.EncodeDate(year, (short)month, (short)day);
        }

        /// <summary>
        /// Converts a <c>DateTime</c> value to <see cref="Date"/>.
        /// </summary>
        /// <param name="dateTime"><c>DateTime</c> value to convert.</param>
        /// <remarks>
        /// <c>DateTime</c> resolution is a .NET tick, which equals to 100 nanoseconds, while
        /// <see cref="Date"/> resolution is 1 microsecond (10 ticks), so the resulting value
        /// is rounded to 10 ticks.
        /// <para>
        /// <c>DateTime.Kind</c> value is ignored.
        /// </para>
        /// </remarks>
        internal static Date FromDateTime(DateTime dateTime)
        // To be enabled post-v1
        //public static explicit operator Date(DateTime dateTime)
        {
            return new Date(Conversions.TicksToDateValue(dateTime.Ticks));
        }

        /// <summary>
        /// Converts a <see cref="Date"/> to <c>DateTime</c> with <c>Kind=DateTimeKind.Unspecified</c>.
        /// </summary>
        /// <param name="date">Date value to convert.</param>
        /// <remarks>
        /// <see cref="Date"/> supports a greater range of dates, and the conversion
        /// throws an exception if the date is not representable by <c>DateTime</c>.
        /// </remarks>
        public static explicit operator DateTime(Date date)
        {
            return new DateTime(Conversions.DateValueToTicks(date.Value));
        }

        /// <summary>
        /// Gets the current date.
        /// </summary>
        internal static Date Today => FromDateTime(DateTime.Today);

        /// <summary>
        /// Year.
        /// </summary>
        public int Year
        {
            get
            {
                Dll.hyper_date_components_t comps = Conversions.DecodeDate(Value);
                return comps.year;
            }
        }

        /// <summary>
        /// Month, from 1 to 12.
        /// </summary>
        public int Month
        {
            get
            {
                Dll.hyper_date_components_t comps = Conversions.DecodeDate(Value);
                return comps.month;
            }
        }

        /// <summary>
        /// Day, from 1 to the number of days in the month.
        /// </summary>
        public int Day
        {
            get
            {
                Dll.hyper_date_components_t comps = Conversions.DecodeDate(Value);
                return comps.day;
            }
        }

        /// <summary>
        /// Formats a <see cref="Date"/> value as string.
        /// </summary>
        public override string ToString()
        {
            Dll.hyper_date_components_t comps = Conversions.DecodeDate(Value);
            return $"{comps.year}-{comps.month:d02}-{comps.day:d02}";
        }

        /// <summary>
        /// Compares two <see cref="Date"/> values.
        /// </summary>
        public int CompareTo(Date other)
        {
            return Value.CompareTo(other.Value);
        }

        int IComparable.CompareTo(object other)
        {
            switch (other)
            {
                case null:
                    return 1;

                case Date date:
                    return CompareTo(date);

                default:
                    throw new ArgumentException("Object is not a Date");
            }
        }

        /// <summary>
        /// Compares two <see cref="Date"/> values.
        /// </summary>
        /// <param name="other">Value to compare with.</param>
        /// <returns>Whether this value compares equal to <c>other</c>.</returns>
        public bool Equals(Date other)
        {
            return Value == other.Value;
        }

        /// <summary>
        /// Compares two <see cref="Date"/> values.
        /// </summary>
        /// <param name="obj">Object to compare with.</param>
        /// <returns>Whether this object compares equal to <c>obj</c>.</returns>
        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case Date date:
                    return Equals(date);

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
            return Value.GetHashCode();
        }

        /// <summary>
        /// Compares two <see cref="Date"/> values.
        /// </summary>
        /// <param name="date1">Value to compare.</param>
        /// <param name="date2">Value to compare.</param>
        public static bool operator ==(Date date1, Date date2)
        {
            return date1.Equals(date2);
        }

        /// <summary>
        /// Compares two <see cref="Date"/> values.
        /// </summary>
        /// <param name="date1">Value to compare.</param>
        /// <param name="date2">Value to compare.</param>
        public static bool operator !=(Date date1, Date date2)
        {
            return !date1.Equals(date2);
        }

        /// <summary>
        /// Compares two <see cref="Date"/> values.
        /// </summary>
        /// <param name="date1">Value to compare.</param>
        /// <param name="date2">Value to compare.</param>
        public static bool operator <(Date date1, Date date2)
        {
            return date1.CompareTo(date2) < 0;
        }

        /// <summary>
        /// Compares two <see cref="Date"/> values.
        /// </summary>
        /// <param name="date1">Value to compare.</param>
        /// <param name="date2">Value to compare.</param>
        public static bool operator >(Date date1, Date date2)
        {
            return date1.CompareTo(date2) > 0;
        }

        /// <summary>
        /// Compares two <see cref="Date"/> values.
        /// </summary>
        /// <param name="date1">Value to compare.</param>
        /// <param name="date2">Value to compare.</param>
        public static bool operator <=(Date date1, Date date2)
        {
            return date1.CompareTo(date2) <= 0;
        }

        /// <summary>
        /// Compares two <see cref="Date"/> values.
        /// </summary>
        /// <param name="date1">Value to compare.</param>
        /// <param name="date2">Value to compare.</param>
        public static bool operator >=(Date date1, Date date2)
        {
            return date1.CompareTo(date2) >= 0;
        }
    }
}
