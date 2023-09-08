using System;
using System.Collections.Generic;
using System.Text;

using Tableau.HyperAPI.Raw;

namespace Tableau.HyperAPI
{
    /// <summary>
    /// Hyper TIMESTAMP and TIMESTAMP_TZ values - combination of date and time of day.
    /// </summary>
    /// <remarks>
    /// <c>Timestamp</c> is almost identical to <c>DateTime</c>, except for two things:
    /// <list type="bullet">
    /// <item><description><c>Timestamp</c> supports a greater range of dates.</description></item>
    /// <item><description><c>DateTime</c> resolution is .NET tick, which equals to 100 nanoseconds, while
    /// <c>Timestamp</c> resolution is 1 microsecond (10 ticks).</description></item>
    /// </list>
    /// <para>
    /// The <see cref="Kind"/> property indicates if a <see cref="Timestamp"/> represents UTC, local time,
    /// or is unspecified. However, a timestamp does not store an explicit time zone offset. In a time-zone-aware
    /// application, you must rely on some external mechanism to determine the time zone in which
    /// a <see cref="Timestamp"/> object was created.
    /// </para>
    /// </remarks>
    public struct Timestamp : IComparable, IComparable<Timestamp>, IEquatable<Timestamp>
    {
        internal ulong Value { get; }

        internal Timestamp(ulong value, DateTimeKind kind)
        {
            Value = value;
            Kind = kind;
        }

        /// <summary>
        /// Gets a value that indicates whether the time represented by this instance is based on local time,
        /// Coordinated Universal Time (UTC), or neither.
        /// </summary>
        /// <property name="DateTimeKind">
        /// One of the enumeration values that indicates what the current time represents. The default is
        /// <see cref="DateTimeKind.Unspecified"/>.
        /// </property>
        /// <remarks>
        /// You can explicitly set the <see cref="Kind"/> property of a new <see cref="Timestamp"/> value to
        /// a particular <see cref="DateTimeKind"/> value by calling the <see cref="SpecifyKind"/> method.
        /// <para>
        /// The <see cref="Kind"/> property allows a <see cref="Timestamp"/> value to clearly reflect either
        /// Coordinated Universal Time (UTC) or the local time. In contrast, the <c>DateTimeOffset</c>
        /// structure can unambiguously reflect any time in any time zone as a single point in time.
        /// </para>
        /// </remarks>
        public DateTimeKind Kind { get; }

        /// <summary>
        /// Creates a new <see cref="Timestamp"/> object that has the same date and time as the specified
        /// <see cref="Timestamp"/>, but is designated as either local time, Coordinated Universal Time (UTC),
        /// or neither, as indicated by the specified <see cref="DateTimeKind"/> value.
        /// </summary>
        /// <param name="value">A Timestamp.</param>
        /// <param name="kind">One of the enumeration values that indicates whether the new object represents
        /// local time, UTC, or neither.</param>
        /// <returns>
        /// A new object that has the same date and time as the object represented by the <c>value</c> parameter
        /// and the <see cref="DateTimeKind"/> value specified by the <c>kind</c> parameter.
        /// </returns>
        /// <remarks>
        /// <para>
        /// A <see cref="Timestamp"/> object consists of a Kind field that indicates whether the time value is
        /// based on local time, Coordinated Universal Time (UTC), or neither, and a date time components.
        /// The <see cref="SpecifyKind"/> method creates a new <see cref="Timestamp"/> object using the specified
        /// <c>kind</c> parameter and the original date time components.
        /// </para>
        /// <para>
        /// The returned <see cref="Timestamp"/> value may not represent the same instant in time as the
        /// <c>value</c> parameter, and <see cref="SpecifyKind"/> is not a time zone conversion method. Instead,
        /// it leaves the date time components specified by the <c>value</c> parameter unchanged, and sets the
        /// <see cref="Kind"/> property to <c>kind</c>.
        /// </para>
        /// <para>
        /// The <see cref="SpecifyKind"/> method is useful in interoperability scenarios where you receive a
        /// <see cref="Timestamp"/> object with an unspecified Kind field, but you can determine by independent
        /// means that the datetime components represent local time or UTC.
        /// </para>
        /// </remarks>
        public static Timestamp SpecifyKind(Timestamp value, DateTimeKind kind)
        {
            return new Timestamp(value.Value, kind);
        }

        /// <summary>
        /// Constructs a <see cref="Timestamp"/> from the year, month, and day.
        /// </summary>
        /// <param name="year">Year.</param>
        /// <param name="month">Month (1 through 12).</param>
        /// <param name="day">Day of month (1 through the number of days in the month).</param>
        /// <param name="kind">One of the enumeration values that indicates whether year, month, day specify a
        /// local time, Coordinated Universal Time (UTC), or neither.</param>
        public Timestamp(int year, int month, int day, DateTimeKind kind = DateTimeKind.Unspecified)
            : this(year, month, day, 0, 0, 0, 0, kind)
        {
        }

        /// <summary>
        /// Constructs a <see cref="Timestamp"/> from the year, month, day, hours, minutes, and seconds.
        /// </summary>
        /// <param name="year">Year.</param>
        /// <param name="month">Month (1 through 12).</param>
        /// <param name="day">Day of month (1 through the number of days in the month).</param>
        /// <param name="hour">Hours (0 through 23).</param>
        /// <param name="minute">Minutes (0 through 59).</param>
        /// <param name="second">Seconds (0 through 59).</param>
        /// <param name="kind">One of the enumeration values that indicates whether year, month, day, hours, minutes,
        /// and seconds specify a local time, Coordinated Universal Time (UTC), or neither.</param>
        public Timestamp(int year, int month, int day, int hour, int minute, int second, DateTimeKind kind = DateTimeKind.Unspecified)
            : this(year, month, day, hour, minute, second, 0, kind)
        {
        }

        /// <summary>
        /// Constructs a <see cref="Timestamp"/> from the year, month, day, hours, minutes, seconds, and microseconds.
        /// </summary>
        /// <param name="year">Year.</param>
        /// <param name="month">Month (1 through 12).</param>
        /// <param name="day">Day of month (1 through the number of days in the month).</param>
        /// <param name="hour">Hours (0 through 23).</param>
        /// <param name="minute">Minutes (0 through 59).</param>
        /// <param name="second">Seconds (0 through 59).</param>
        /// <param name="microsecond">Microseconds (0 through 999999). NOTE, this is
        /// not milliseconds like in the <c>DateTime</c> constructor.</param>
        /// <param name="kind">One of the enumeration values that indicates whether year, month, day, hour, minute,
        /// second, and microsecond specify a local time, Coordinated Universal Time (UTC), or neither.</param>
        public Timestamp(int year, int month, int day, int hour, int minute, int second, int microsecond, DateTimeKind kind = DateTimeKind.Unspecified)
        {
            Value = Conversions.MicrosecondsPerDay * Conversions.EncodeDate(year, (short)month, (short)day) +
                Conversions.EncodeTime((ushort)hour, (ushort)minute, (ushort)second, (uint)microsecond);
            Kind = kind;
        }

        /// <summary>
        /// Converts a <c>DateTime</c> value to a <see cref="Timestamp"/>.
        /// </summary>
        /// <param name="dateTime"><c>DateTime</c> value to convert.</param>
        /// <remarks>
        /// <c>DateTime</c> resolution is a .NET tick, which equals to 100 nanoseconds, while
        /// <see cref="Timestamp"/> resolution is 1 microsecond (10 ticks), so the resulting value
        /// is rounded to 10 ticks.
        /// <para>
        /// The <see cref="Timestamp.Kind"/> property of the resulting <see cref="Timestamp"/> object
        /// matches the <c>DateTime.Kind</c> property of <c>dateTime</c>.
        /// </para>
        /// </remarks>
        public static explicit operator Timestamp(DateTime dateTime)
        {
            return new Timestamp(Conversions.TicksToTimestampValue(dateTime.Ticks), dateTime.Kind);
        }

        /// <summary>
        /// Converts a <see cref="Date"/> value to <see cref="Timestamp"/>.
        /// </summary>
        /// <param name="date"><see cref="Date"/> value to convert.</param>
        public static explicit operator Timestamp(Date date)
        {
            return new Timestamp(Conversions.MicrosecondsPerDay * date.Value, DateTimeKind.Unspecified);
        }

        /// <summary>
        /// Converts a <see cref="Timestamp"/> value to a <c>DateTime</c>.
        /// </summary>
        /// <remarks>
        /// <see cref="Timestamp"/> supports a greater range of dates, and the conversion
        /// throws an exception if the date is not representable by <c>DateTime</c>.
        /// <para>
        /// The <c>DateTime.Kind</c> property of the resulting <c>DateTime</c> object matches the
        /// <see cref="Timestamp.Kind"/> property of <c>timestamp</c>.
        /// </para>
        /// </remarks>
        /// <param name="timestamp"><see cref="Timestamp"/> value to convert.</param>
        public static explicit operator DateTime(Timestamp timestamp)
        {
            return new DateTime(Conversions.TimestampValueToTicks(timestamp.Value), timestamp.Kind);
        }

        /// <summary>
        /// Converts a <see cref="Timestamp"/> value to a <c>DateTimeOffset</c>.
        /// </summary>
        /// <remarks>
        /// <see cref="Timestamp"/> supports a greater range of dates, and the conversion
        /// throws an exception if the date is not representable by <c>DateTimeOffset</c>.
        /// <para>
        /// The offset of the resulting <c>DateTimeOffset</c> object depends on the value of the
        /// <see cref="Timestamp.Kind"/> property of the <c>timestamp</c> parameter:
        /// </para>
        /// <para>
        /// If the value of the <see cref="Timestamp.Kind"/> property is <see cref="DateTimeKind.Utc"/>,
        /// the date and time of the <c>DateTimeOffset</c> object is set equal to <c>timestamp</c>,
        /// and its <c>Offset</c> property is set equal to 0.
        /// </para>
        /// <para>
        /// If the value of the <see cref="Timestamp.Kind"/> property is <see cref="DateTimeKind.Local"/> or
        /// <see cref="DateTimeKind.Unspecified"/>, the date and time of the <c>DateTimeOffset</c> object is
        /// set equal to <c>timestamp</c>, and its <c>Offset</c> property is set equal to the offset of the
        /// local system's current time zone.
        /// </para>
        /// </remarks>
        /// <param name="timestamp"><see cref="Timestamp"/> value to convert.</param>
        public static explicit operator DateTimeOffset(Timestamp timestamp)
        {
            return (DateTime)timestamp;
        }

        private DateTime nearestValidDateTime(long ticks)
        {
            if (ticks > DateTime.MaxValue.Ticks)
                ticks = DateTime.MaxValue.Ticks;
            else if (ticks < DateTime.MinValue.Ticks)
                ticks = DateTime.MinValue.Ticks;
            return new DateTime(ticks);
        }

        /// <summary>
        /// Converts the value of the current <see cref="Timestamp"/> object to local time.
        /// </summary>
        /// <returns>
        /// An object whose <see cref="Kind"/> property is <see cref="DateTimeKind.Local"/>, and whose value is the local time
        /// equivalent to the value of the current <see cref="Timestamp"/> object.
        /// </returns>
        /// <remarks>
        /// If the <see cref="Kind"/> property is set to <see cref="DateTimeKind.Utc"/>, the current <see cref="Timestamp"/>
        /// object is converted to local time. If the <see cref="Kind"/> property is set to <see cref="DateTimeKind.Unspecified"/>
        /// the current <see cref="Timestamp"/> object is assumed to be a UTC time, and the conversion is
        /// performed as if <see cref="Kind"/> were <see cref="DateTimeKind.Utc"/>. No conversion is applied if the
        /// <see cref="Kind"/> property is set to <see cref="DateTimeKind.Local"/>.
        /// </remarks>
        public Timestamp ToLocalTime()
        {
            if (Kind == DateTimeKind.Local) {
                return this;
            }
            long ticks = Conversions.TimestampValueToTicks(Value);
            TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(nearestValidDateTime(ticks));
            return new Timestamp(Conversions.TicksToTimestampValue(ticks + offset.Ticks), DateTimeKind.Local);
        }

        /// <summary>
        /// Converts the value of the current <see cref="Timestamp"/> object to Coordinated Universal Time (UTC).
        /// </summary>
        /// <returns>
        /// An object whose <see cref="Kind"/> property is <see cref="DateTimeKind.Utc"/>, and whose value is the UTC equivalent to the value
        /// of the current <see cref="Timestamp"/> object.
        /// </returns>
        /// <remarks>
        /// If the <see cref="Kind"/> property is set to <see cref="DateTimeKind.Local"/>, the current <see cref="Timestamp"/>
        /// object is converted to UTC. If the <see cref="Kind"/> property is set to <see cref="DateTimeKind.Unspecified"/>
        /// the current <see cref="Timestamp"/> object is assumed to be a local time, and the conversion is
        /// performed as if <see cref="Kind"/> were <see cref="DateTimeKind.Local"/>. No conversion is applied if the
        /// <see cref="Kind"/> property is set to <see cref="DateTimeKind.Utc"/>.
        /// </remarks>
        public Timestamp ToUniversalTime()
        {
            if (Kind == DateTimeKind.Utc) {
                return this;
            }
            long ticks = Conversions.TimestampValueToTicks(Value);
            TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(nearestValidDateTime(ticks));
            return new Timestamp(Conversions.TicksToTimestampValue(ticks - offset.Ticks), DateTimeKind.Utc);
        }

        /// <summary>
        /// Gets the current date.
        /// </summary>
        public static Timestamp Today => (Timestamp)DateTime.Today;

        /// <summary>
        /// Gets the current time.
        /// </summary>
        public static Timestamp Now => (Timestamp)DateTime.Now;

        /// <summary>
        /// Year.
        /// </summary>
        public int Year
        {
            get
            {
                Dll.hyper_date_components_t comps = Conversions.DecodeDatePart(Value);
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
                Dll.hyper_date_components_t comps = Conversions.DecodeDatePart(Value);
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
                Dll.hyper_date_components_t comps = Conversions.DecodeDatePart(Value);
                return comps.day;
            }
        }

        /// <summary>
        /// Hour, from 0 to 23.
        /// </summary>
        public int Hour
        {
            get
            {
                Dll.hyper_time_components_t comps = Conversions.DecodeTimePart(Value);
                return comps.hour;
            }
        }

        /// <summary>
        /// Minute, from 0 to 59.
        /// </summary>
        public int Minute
        {
            get
            {
                Dll.hyper_time_components_t comps = Conversions.DecodeTimePart(Value);
                return comps.minute;
            }
        }

        /// <summary>
        /// Seconds, from 0 to 59.
        /// </summary>
        public int Second
        {
            get
            {
                Dll.hyper_time_components_t comps = Conversions.DecodeTimePart(Value);
                return comps.second;
            }
        }

        /// <summary>
        /// Microseconds, from 0 to 999999.
        /// </summary>
        public int Microsecond
        {
            get
            {
                Dll.hyper_time_components_t comps = Conversions.DecodeTimePart(Value);
                return comps.microsecond;
            }
        }

        /// <summary>
        /// Formats a <see cref="Timestamp"/> value as string.
        /// </summary>
        public override string ToString()
        {
            Dll.hyper_date_components_t dateComps = Conversions.DecodeDatePart(Value);
            Dll.hyper_time_components_t timeComps = Conversions.DecodeTimePart(Value);

            if (timeComps.microsecond == 0)
                return $"{dateComps.year}-{dateComps.month:d02}-{dateComps.day:d02} {timeComps.hour:d02}:{timeComps.minute:d02}:{timeComps.second:d02}";
            else
                return $"{dateComps.year}-{dateComps.month:d02}-{dateComps.day:d02} {timeComps.hour:d02}:{timeComps.minute:d02}:{timeComps.second:d02}.{timeComps.microsecond:d06}";
        }

        /// <summary>
        /// Compares the value of this instance to a specified <see cref="Timestamp"/> value and indicates whether this
        /// instance is earlier than, the same as, or later than the specified <see cref="Timestamp"/> value.
        /// </summary>
        /// <remarks>
        /// When determining the relationship of the current instance to <code>other</code>, the <see cref="CompareTo"/> method
        /// ignores their <see cref="Kind"/> property. Before comparing <see cref="Timestamp"/> objects, make sure that the objects
        /// represent times in the same time zone. You can do this by comparing the values of their <see cref="Kind"/> properties.
        /// </remarks>
        public int CompareTo(Timestamp other)
        {
            return Value.CompareTo(other.Value);
        }

        int IComparable.CompareTo(object other)
        {
            switch (other)
            {
                case null:
                    return 1;

                case Timestamp date:
                    return CompareTo(date);

                default:
                    throw new ArgumentException("Object is not a Timestamp");
            }
        }

        /// <summary>
        /// Returns a value indicating whether the value of this instance is equal to the value of the specified <see cref="Timestamp"/> instance.
        /// </summary>
        /// <param name="other">Value to compare with.</param>
        /// <returns>Whether this value compares equal to <c>other</c>.</returns>
        /// <remarks>
        /// The <see cref="Kind"/> property values of the current instance and <code>other</code> are not considered in the test for equality.
        /// </remarks>
        public bool Equals(Timestamp other)
        {
            return Value == other.Value;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">Object to compare with.</param>
        /// <returns>Whether this object compares equal to <c>obj</c>.</returns>
        /// <remarks>
        /// The <see cref="Kind"/> property values of the current instance and <code>obj</code> are not considered in the test for equality.
        /// </remarks>
        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case Timestamp timestamp:
                    return Equals(timestamp);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Computes the hash code.
        /// </summary>
        /// <returns>Hash code.</returns>
        /// <remarks>
        /// The <see cref="Kind"/> property value of the current instance is not considered when computing the hash code.
        /// </remarks>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="Timestamp"/> are equal.
        /// </summary>
        /// <param name="date1">Value to compare.</param>
        /// <param name="date2">Value to compare.</param>
        /// <remarks>
        /// When determining whether two <see cref="Timestamp"/> values are equal, the <see cref="Timestamp.op_Equality(Timestamp, Timestamp)"/> operator ignores
        /// their <see cref="Kind"/> property. Before comparing <see cref="Timestamp"/> objects, make sure that the objects
        /// represent times in the same time zone. You can do this by comparing the values of their <see cref="Kind"/> properties.
        /// </remarks>
        public static bool operator ==(Timestamp date1, Timestamp date2)
        {
            return date1.Equals(date2);
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="Timestamp"/> are not equal.
        /// </summary>
        /// <param name="date1">Value to compare.</param>
        /// <param name="date2">Value to compare.</param>
        /// <remarks>
        /// When determining whether two <see cref="Timestamp"/> values are not equal, the <see cref="Timestamp.op_Inequality(Timestamp, Timestamp)"/> operator ignores
        /// their <see cref="Kind"/> property. Before comparing <see cref="Timestamp"/> objects, make sure that the objects
        /// represent times in the same time zone. You can do this by comparing the values of their <see cref="Kind"/> properties.
        /// </remarks>
        public static bool operator !=(Timestamp date1, Timestamp date2)
        {
            return !date1.Equals(date2);
        }

        /// <summary>
        /// Determines whether one specified <see cref="Timestamp"/> is earlier than another specified <see cref="Timestamp"/>.
        /// </summary>
        /// <param name="date1">Value to compare.</param>
        /// <param name="date2">Value to compare.</param>
        /// <remarks>
        /// When determining the relationship between two <see cref="Timestamp"/> values, the <see cref="Timestamp.op_LessThan(Timestamp, Timestamp)"/> operator ignores
        /// their <see cref="Kind"/> property. Before comparing <see cref="Timestamp"/> objects, make sure that the objects
        /// represent times in the same time zone. You can do this by comparing the values of their <see cref="Kind"/> properties.
        /// </remarks>
        public static bool operator <(Timestamp date1, Timestamp date2)
        {
            return date1.CompareTo(date2) < 0;
        }

        /// <summary>
        /// Determines whether one specified <see cref="Timestamp"/> is later than another specified <see cref="Timestamp"/>.
        /// </summary>
        /// <param name="date1">Value to compare.</param>
        /// <param name="date2">Value to compare.</param>
        /// <remarks>
        /// When determining the relationship between two <see cref="Timestamp"/> values, the <see cref="Timestamp.op_GreaterThan(Timestamp, Timestamp)"/> operator ignores
        /// their <see cref="Kind"/> property. Before comparing <see cref="Timestamp"/> objects, make sure that the objects
        /// represent times in the same time zone. You can do this by comparing the values of their <see cref="Kind"/> properties.
        /// </remarks>
        public static bool operator >(Timestamp date1, Timestamp date2)
        {
            return date1.CompareTo(date2) > 0;
        }

        /// <summary>
        /// Determines whether one specified <see cref="Timestamp"/> represents a date and time that is the same as or earlier than another specified <see cref="Timestamp"/>.
        /// </summary>
        /// <param name="date1">Value to compare.</param>
        /// <param name="date2">Value to compare.</param>
        /// <remarks>
        /// When determining the relationship between two <see cref="Timestamp"/> values, the <see cref="Timestamp.op_LessThanOrEqual(Timestamp, Timestamp)"/> operator ignores
        /// their <see cref="Kind"/> property. Before comparing <see cref="Timestamp"/> objects, make sure that the objects
        /// represent times in the same time zone. You can do this by comparing the values of their <see cref="Kind"/> properties.
        /// </remarks>
        public static bool operator <=(Timestamp date1, Timestamp date2)
        {
            return date1.CompareTo(date2) <= 0;
        }

        /// <summary>
        /// Determines whether one specified <see cref="Timestamp"/> represents a date and time that is the same as or later than another specified <see cref="Timestamp"/>.
        /// </summary>
        /// <param name="date1">Value to compare.</param>
        /// <param name="date2">Value to compare.</param>
        /// <remarks>
        /// When determining the relationship between two <see cref="Timestamp"/> values, the <see cref="Timestamp.op_GreaterThanOrEqual(Timestamp, Timestamp)"/> operator ignores
        /// their <see cref="Kind"/> property. Before comparing <see cref="Timestamp"/> objects, make sure that the objects
        /// represent times in the same time zone. You can do this by comparing the values of their <see cref="Kind"/> properties.
        /// </remarks>
        public static bool operator >=(Timestamp date1, Timestamp date2)
        {
            return date1.CompareTo(date2) >= 0;
        }
    }
}
