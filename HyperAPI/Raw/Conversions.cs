using System;
using System.Collections.Generic;
using System.Text;

using Tableau.HyperAPI.Impl;

namespace Tableau.HyperAPI.Raw
{
    internal static class Conversions
    {
        public const ulong MicrosecondsPerSecond = 1_000_000;
        public const ulong MicrosecondsPerMinute = 60 * MicrosecondsPerSecond;
        public const ulong MicrosecondsPerHour = 60 * MicrosecondsPerMinute;
        public const ulong MicrosecondsPerDay = 24 * MicrosecondsPerHour;

        private const ulong TicksPerMicrosecond = TimeSpan.TicksPerSecond / MicrosecondsPerSecond;
        private const long EpochOffsetDays = 1_721_426;
        private const long EpochOffsetTicks = EpochOffsetDays * TimeSpan.TicksPerDay;

        public static uint TicksToDateValue(long ticks)
        {
            return (uint)((ticks / TimeSpan.TicksPerDay) + EpochOffsetDays);
        }

        public static long DateValueToTicks(uint value)
        {
            return (value - EpochOffsetDays) * TimeSpan.TicksPerDay;
        }

        public static ulong TimeSpanToTimeValue(TimeSpan time)
        {
            long ticks = time.Ticks;
            Util.Verify(ticks >= 0);
            return (ulong)ticks / 10;
        }

        private static long TimeValueToTicks(Dll.hyper_time_components_t comps)
        {
            return comps.hour * TimeSpan.TicksPerHour + comps.minute * TimeSpan.TicksPerMinute
                + comps.second * TimeSpan.TicksPerSecond + comps.microsecond * (long)TicksPerMicrosecond;
        }

        public static TimeSpan TimeValueToTimeSpan(ulong value)
        {
            // `time` is the number of microseconds since midnight.
            return new TimeSpan((long)(value * TicksPerMicrosecond));
        }

        public static ulong TicksToTimestampValue(long ticks)
        {
            return (ulong)(ticks + EpochOffsetTicks) / TicksPerMicrosecond;
        }

        public static long TimestampValueToTicks(ulong value)
        {
            return (long)(value * TicksPerMicrosecond) - EpochOffsetTicks;
        }

        public static Dll.hyper_date_components_t DecodeDate(uint v)
        {
            // Convert from Julian day to Gregorian date.
            // Note that most countries adapted their calendars between 1582 and 1583.
            // Algorithm from the [Calendar FAQ 2.15.1](https://people.inf.elte.hu/csa/200405.1/calender_faq.htm).

            uint a = v + 32044;
            uint b = (4 * a + 3) / 146097;
            uint c = a - ((146097 * b) / 4);
            uint d = (4 * c + 3) / 1461;
            uint e = c - ((1461 * d) / 4);
            uint m = (5 * e + 2) / 153;

            uint uyear = (100 * b) + d + (m / 10);

            return new Dll.hyper_date_components_t
            {
                day = (short)(e - ((153 * m + 2) / 5) + 1),
                month = (short)(m + 3 - (12 * (m / 10))),
                year = (int)uyear - 4800,
            };
        }

        public static Dll.hyper_date_components_t DecodeDatePart(ulong timestamp)
        {
            return DecodeDate((uint)(timestamp / MicrosecondsPerDay));
        }

        public static Dll.hyper_time_components_t DecodeTime(ulong v)
        {
            // `time` is the number of microseconds since midnight.
            Dll.hyper_time_components_t comps;
            comps.microsecond = (int)(v % MicrosecondsPerSecond);
            v /= MicrosecondsPerSecond;
            comps.second = (byte)(v % 60);
            v /= 60;
            comps.minute = (byte)(v % 60);
            v /= 60;
            comps.hour = (byte)v;
            return comps;
        }

        public static Dll.hyper_time_components_t DecodeTimePart(ulong timestamp)
        {
            return DecodeTime(timestamp % MicrosecondsPerDay);
        }

        public static uint EncodeDate(int year, short month, short day)
        {
            // Convert from Gregorian to Julian day.
            uint a = (uint)((14 - month) / 12);
            uint y = (uint)((year + 4800) - a);
            uint m = (uint)(month + (12 * a) - 3);
            return (uint)(day + ((153 * m + 2) / 5) + (365 * y) + (y / 4) - (y / 100) + (y / 400) - 32045);
        }

        public static ulong EncodeTime(ushort hour, ushort minute, ushort second, uint microsecond)
        {
            return microsecond + MicrosecondsPerSecond * second + MicrosecondsPerMinute * minute + MicrosecondsPerHour * hour;
        }
    }
}
