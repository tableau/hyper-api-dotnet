using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

using Tableau.HyperAPI.Raw;

namespace Tableau.HyperAPI.Test
{
    public class ConversionTests
    {
        [Test]
        public void BasicDateTest()
        {
            DateTime[] dates =
            {
                new DateTime(1, 1, 1),
                new DateTime(2019, 3, 1),
                new DateTime(9999, 12, 31),
            };

            foreach (DateTime date in dates)
            {
                uint v = Conversions.TicksToDateValue(date.Ticks);
                DateTime roundtripped = new DateTime(Conversions.DateValueToTicks(v));
                Assert.AreEqual(date, roundtripped);
            }
        }

        [Test]
        public void BasicTimeTest()
        {
            TimeSpan[] times =
            {
                new TimeSpan(0, 0, 0),
                new TimeSpan(1, 2, 3),
                new TimeSpan(0, 23, 59, 59, 999),
            };

            foreach (TimeSpan time in times)
            {
                ulong v = Conversions.TimeSpanToTimeValue(time);
                TimeSpan roundtripped = Conversions.TimeValueToTimeSpan(v);
                Assert.AreEqual(time, roundtripped);
            }
        }

        [Test]
        public void BasicTimestampTest()
        {
            DateTime[] timestamps =
            {
                new DateTime(2019, 3, 1),
                new DateTime(2020, 5, 5),
                new DateTime(2020, 5, 5, 23, 59, 59, 999),
            };

            foreach (DateTime timestamp in timestamps)
            {
                ulong v = Conversions.TicksToTimestampValue(timestamp.Ticks);
                DateTime roundtripped = new DateTime(Conversions.TimestampValueToTicks(v));
                Assert.AreEqual(timestamp, roundtripped);
            }
        }

        [Test]
        public void DateTimeToTimestampTest()
        {
            DateTime[] dateTimes =
            {
                new DateTime(2020, 5, 5, 23, 59, 59, DateTimeKind.Unspecified),
                new DateTime(2020, 5, 5, 23, 59, 59, 999, DateTimeKind.Unspecified),
                new DateTime(2020, 5, 5, 23, 59, 59, DateTimeKind.Utc),
                new DateTime(2020, 5, 5, 23, 59, 59, 999, DateTimeKind.Utc),
                new DateTime(2020, 5, 5, 23, 59, 59, DateTimeKind.Local),
                new DateTime(2020, 5, 5, 23, 59, 59, 999, DateTimeKind.Local),
            };

            foreach (DateTime dateTime in dateTimes)
            {
                Timestamp timestamp = (Timestamp)dateTime;
                DateTime roundtripped = (DateTime)timestamp;
                Assert.AreEqual(dateTime, roundtripped);
            }
        }

        [Test]
        public void TimestampToDateTimeTest()
        {
            Timestamp[] timestamps =
            {
                new Timestamp(2020, 5, 5, DateTimeKind.Unspecified),
                new Timestamp(2020, 5, 5, 23, 59, 59, DateTimeKind.Unspecified),
                new Timestamp(2020, 5, 5, 23, 59, 59, 999, DateTimeKind.Unspecified),
                new Timestamp(2020, 5, 5, DateTimeKind.Utc),
                new Timestamp(2020, 5, 5, 23, 59, 59, DateTimeKind.Utc),
                new Timestamp(2020, 5, 5, 23, 59, 59, 999, DateTimeKind.Utc),
                new Timestamp(2020, 5, 5, DateTimeKind.Local),
                new Timestamp(2020, 5, 5, 23, 59, 59, DateTimeKind.Local),
                new Timestamp(2020, 5, 5, 23, 59, 59, 999, DateTimeKind.Local),
            };

            foreach (Timestamp timestamp in timestamps)
            {
                DateTime dateTime = (DateTime)timestamp;
                Timestamp roundtripped = (Timestamp)dateTime;
                Assert.AreEqual(timestamp, roundtripped);
            }
        }

        [Test]
        public void TimestampToDateTimeOffsetTest()
        {
            DateTime[] dateTimes =
            {
                new DateTime(2020, 5, 5, 23, 59, 59, DateTimeKind.Unspecified),
                new DateTime(2020, 5, 5, 23, 59, 59, 999, DateTimeKind.Unspecified),
                new DateTime(2020, 5, 5, 23, 59, 59, DateTimeKind.Utc),
                new DateTime(2020, 5, 5, 23, 59, 59, 999, DateTimeKind.Utc),
                new DateTime(2020, 5, 5, 23, 59, 59, DateTimeKind.Local),
                new DateTime(2020, 5, 5, 23, 59, 59, 999, DateTimeKind.Local),
            };

            foreach (DateTime dateTime in dateTimes)
            {
                Timestamp timestamp = (Timestamp)dateTime;
                DateTimeOffset fromTimestamp = (DateTimeOffset)timestamp;
                DateTimeOffset fromDateTime = dateTime;
                Assert.AreEqual(fromTimestamp, fromDateTime);
            }
        }

        private void CheckEncodeDecodeTime(ulong v)
        {
            Dll.hyper_time_components_t comps1 = Conversions.DecodeTime(v);
            Dll.hyper_time_components_t comps2 = Dll.hyper_decode_time(v);
            Assert.AreEqual(comps2.hour, comps1.hour);
            Assert.AreEqual(comps2.minute, comps1.minute);
            Assert.AreEqual(comps2.second, comps1.second);
            Assert.AreEqual(comps2.microsecond, comps1.microsecond);

            ulong v2 = Conversions.EncodeTime(comps1.hour, comps1.minute, comps1.second, (uint)comps1.microsecond);
            Assert.AreEqual(v, v2);

            ulong v3 = Dll.hyper_encode_time(comps1);
            Assert.AreEqual(v, v3);
        }

        private void CheckEncodeDecodeTime(ushort hour, ushort minute, ushort second, uint microsecond)
        {
            CheckEncodeDecodeTime(Conversions.EncodeTime(hour, minute, second, microsecond));
        }

        [Test]
        public void TestEncodeDecodeTime()
        {
            Assert.AreEqual(0, Conversions.EncodeTime(0, 0, 0, 0));
            Assert.AreEqual(Conversions.MicrosecondsPerHour, Conversions.EncodeTime(1, 0, 0, 0));
            Assert.AreEqual(Conversions.MicrosecondsPerMinute, Conversions.EncodeTime(0, 1, 0, 0));
            Assert.AreEqual(Conversions.MicrosecondsPerSecond, Conversions.EncodeTime(0, 0, 1, 0));
            Assert.AreEqual(Conversions.MicrosecondsPerDay, Conversions.EncodeTime(24, 0, 0, 0));
            CheckEncodeDecodeTime(0);
            CheckEncodeDecodeTime(Conversions.MicrosecondsPerDay / 2);
            CheckEncodeDecodeTime(Conversions.MicrosecondsPerDay - 1);
            CheckEncodeDecodeTime(Conversions.MicrosecondsPerDay);

            for (ulong i = 0; i < Conversions.MicrosecondsPerDay; i += 987_765)
                CheckEncodeDecodeTime(i);

            CheckEncodeDecodeTime(0, 0, 0, 0);
            CheckEncodeDecodeTime(24, 0, 0, 0);
            CheckEncodeDecodeTime(23, 59, 59, 999999);
            CheckEncodeDecodeTime(11, 55, 23, 123456);
        }

        private void CheckEncodeDecodeDate(uint v)
        {
            Dll.hyper_date_components_t comps1 = Conversions.DecodeDate(v);
            Dll.hyper_date_components_t comps2 = Dll.hyper_decode_date(v);
            Assert.AreEqual(comps2.year, comps1.year);
            Assert.AreEqual(comps2.month, comps1.month);
            Assert.AreEqual(comps2.day, comps1.day);

            ulong v2 = Conversions.EncodeDate(comps1.year, comps1.month, comps1.day);
            Assert.AreEqual(v, v2);

            ulong v3 = Dll.hyper_encode_date(comps1);
            Assert.AreEqual(v, v3);
        }

        private void CheckEncodeDecodeDate(int year, short month, short day)
        {
            Dll.hyper_date_components_t z1 = new Dll.hyper_date_components_t { year = year, month = month, day = day };
            Dll.hyper_date_components_t z2 = Dll.hyper_decode_date(Dll.hyper_encode_date(z1));
            Assert.AreEqual(z1.year, z2.year);
            Assert.AreEqual(z1.month, z2.month);
            Assert.AreEqual(z1.day, z2.day);

            CheckEncodeDecodeDate(Conversions.EncodeDate(year, month, day));
            Dll.hyper_date_components_t comps = Conversions.DecodeDate(Conversions.EncodeDate(year, month, day));
            Assert.AreEqual(year, comps.year);
            Assert.AreEqual(month, comps.month);
            Assert.AreEqual(day, comps.day);
        }

        [Test]
        public void TestEncodeDecodeDate()
        {
            for (uint i = 0; i < 100; ++i)
                CheckEncodeDecodeDate(i);

            for (uint i = 0; i < 10_000; i += 101)
                CheckEncodeDecodeDate(i);

            for (uint i = 0; i < 110_000_000; i += 9_876)
                CheckEncodeDecodeDate(i);

            CheckEncodeDecodeDate(-4000, 1, 1);
            CheckEncodeDecodeDate(-4000, 12, 31);
            CheckEncodeDecodeDate(1, 1, 1);
            CheckEncodeDecodeDate(1, 12, 31);
            CheckEncodeDecodeDate(2019, 6, 13);
        }
    }
}
