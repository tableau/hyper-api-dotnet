using System;

using NUnit.Framework;

namespace Tableau.HyperAPI.Test
{
    public class DateTimeTests
    {
        private void CheckDate(int year, int month, int day)
        {
            Date date = new Date(year, month, day);
            Assert.AreEqual(year, date.Year);
            Assert.AreEqual(month, date.Month);
            Assert.AreEqual(day, date.Day);
        }

        private void CheckDateRepresentableAsDateTime(int year, int month, int day)
        {
            CheckDate(year, month, day);
            Date date = new Date(year, month, day);
            DateTime dateTime = (DateTime)date;
            Assert.AreEqual(date, Date.FromDateTime(dateTime));
            Assert.AreEqual(dateTime, (DateTime)date);
            Assert.AreEqual(year, date.Year);
            Assert.AreEqual(month, date.Month);
            Assert.AreEqual(day, date.Day);
        }

        private void CheckDateNotRepresentableAsDateTime(int year, int month, int day)
        {
            CheckDate(year, month, day);

            CheckDateRepresentableAsDateTime(1, month, day);

            Date date = new Date(year, month, day);
            Assert.AreEqual(year, date.Year);
            Assert.AreEqual(month, date.Month);
            Assert.AreEqual(day, date.Day);
#pragma warning disable 8073
            Assert.Throws<ArgumentOutOfRangeException>(() => { if ((DateTime)date != null) return; });
#pragma warning restore 8073
        }

        [Test]
        public void BasicDateTest()
        {
            CheckDateRepresentableAsDateTime(1, 1, 1);
            CheckDateRepresentableAsDateTime(2019, 6, 13);
            CheckDateRepresentableAsDateTime(9999, 12, 31);
            CheckDateNotRepresentableAsDateTime(0, 1, 1);
            CheckDateNotRepresentableAsDateTime(-1000, 1, 1);
            CheckDateNotRepresentableAsDateTime(100000, 1, 1);
        }

        private class DateComparerWithOperators : TestUtil.IComparerWithOperators<Date>
        {
            public bool Eq(Date x, Date y)
            {
                return x == y;
            }

            public bool Ge(Date x, Date y)
            {
                return x >= y;
            }

            public bool Gt(Date x, Date y)
            {
                return x > y;
            }

            public bool Le(Date x, Date y)
            {
                return x <= y;
            }

            public bool Lt(Date x, Date y)
            {
                return x < y;
            }

            public bool Ne(Date x, Date y)
            {
                return x != y;
            }
        }

        private void CheckComparison(Date lhs, Date rhs)
        {
            TestUtil.CheckComparison(lhs, rhs, new DateComparerWithOperators());
        }

        [Test]
        public void TestDateComparison()
        {
            Assert.AreEqual(new Date(1, 1, 1), new Date(1, 1, 1));
            Assert.AreEqual(0, new Date(1, 1, 1).CompareTo(new Date(1, 1, 1)));

            Assert.IsTrue(new Date(1, 1, 1) == new Date(1, 1, 1));
            Assert.IsTrue(new Date(1, 1, 1) <= new Date(1, 1, 1));
            Assert.IsTrue(new Date(1, 1, 1) >= new Date(1, 1, 1));
            Assert.IsFalse(new Date(1, 1, 1) != new Date(1, 1, 1));
            Assert.IsFalse(new Date(1, 1, 1) < new Date(1, 1, 1));
            Assert.IsFalse(new Date(1, 1, 1) > new Date(1, 1, 1));

            CheckComparison(new Date(1, 1, 1), new Date(1, 2, 3));
            CheckComparison(new Date(-1, 1, 1), new Date(1, 2, 3));
        }

        [Test]
        public void TestDateFormatting()
        {
            Assert.AreEqual("2019-01-01", new Date(2019, 01, 01).ToString());
            Assert.AreEqual("2019-06-13", new Date(2019, 06, 13).ToString());
            Assert.AreEqual("2019-12-31", new Date(2019, 12, 31).ToString());
            Assert.AreEqual("-1000-01-01", new Date(-1000, 01, 01).ToString());
            Assert.AreEqual("-1000-12-31", new Date(-1000, 12, 31).ToString());
        }

        private void CheckTimestamp(int year, int month, int day, int hour, int minute, int second, int microsecond, DateTimeKind kind)
        {
            Timestamp timestamp = new Timestamp(year, month, day, hour, minute, second, microsecond, kind);
            Assert.AreEqual(year, timestamp.Year);
            Assert.AreEqual(month, timestamp.Month);
            Assert.AreEqual(day, timestamp.Day);
            Assert.AreEqual(hour, timestamp.Hour);
            Assert.AreEqual(minute, timestamp.Minute);
            Assert.AreEqual(second, timestamp.Second);
            Assert.AreEqual(microsecond, timestamp.Microsecond);
            Assert.AreEqual(kind, timestamp.Kind);

            Timestamp noMicrosecondExpected = new Timestamp(year, month, day, hour, minute, second, 0, kind);
            Timestamp noMicrosecondActual = new Timestamp(year, month, day, hour, minute, second, kind);
            Assert.AreEqual(noMicrosecondExpected, noMicrosecondActual);
            Assert.AreEqual(noMicrosecondExpected.Kind, noMicrosecondActual.Kind);

            Timestamp onlyDateExpected = new Timestamp(year, month, day, 0, 0, 0, 0, kind);
            Timestamp onlyDateActual = new Timestamp(year, month, day, kind);
            Assert.AreEqual(onlyDateExpected, onlyDateActual);
            Assert.AreEqual(onlyDateExpected.Kind, onlyDateActual.Kind);
        }

        private void CheckTimestampRepresentableAsDateTime(int year, int month, int day, int hour, int minute, int second, int microsecond, DateTimeKind kind)
        {
            CheckTimestamp(year, month, day, hour, minute, second, microsecond, kind);

            int millisecond = microsecond / 1000;
            DateTime dateTime = new DateTime(year, month, day, hour, minute, second, millisecond, kind);
            Timestamp timestamp = new Timestamp(year, month, day, hour, minute, second, millisecond * 1000, kind);
            Assert.AreEqual(timestamp, (Timestamp)dateTime);
            Assert.AreEqual(dateTime, (DateTime)timestamp);
            Assert.AreEqual(dateTime.Year, timestamp.Year);
            Assert.AreEqual(dateTime.Month, timestamp.Month);
            Assert.AreEqual(dateTime.Day, timestamp.Day);
            Assert.AreEqual(dateTime.Hour, timestamp.Hour);
            Assert.AreEqual(dateTime.Minute, timestamp.Minute);
            Assert.AreEqual(dateTime.Second, timestamp.Second);
            Assert.AreEqual(dateTime.Millisecond, timestamp.Microsecond / 1000);
            Assert.AreEqual(dateTime.Kind, timestamp.Kind);
            Assert.AreEqual(year, timestamp.Year);
            Assert.AreEqual(month, timestamp.Month);
            Assert.AreEqual(day, timestamp.Day);
            Assert.AreEqual(hour, timestamp.Hour);
            Assert.AreEqual(minute, timestamp.Minute);
            Assert.AreEqual(second, timestamp.Second);
            Assert.AreEqual(millisecond, timestamp.Microsecond / 1000);
            Assert.AreEqual(kind, timestamp.Kind);
        }

        private void CheckTimestampNotRepresentableAsDateTime(int year, int month, int day, int hour, int minute, int second, int microsecond, DateTimeKind kind)
        {
            CheckTimestamp(year, month, day, hour, minute, second, microsecond, kind);

            CheckTimestampRepresentableAsDateTime(1, month, day, hour, minute, second, microsecond, kind);

            Timestamp timestamp = new Timestamp(year, month, day, hour, minute, second, microsecond, kind);
            Assert.AreEqual(year, timestamp.Year);
            Assert.AreEqual(month, timestamp.Month);
            Assert.AreEqual(day, timestamp.Day);
            Assert.AreEqual(hour, timestamp.Hour);
            Assert.AreEqual(minute, timestamp.Minute);
            Assert.AreEqual(second, timestamp.Second);
            Assert.AreEqual(microsecond, timestamp.Microsecond);
            Assert.AreEqual(kind, timestamp.Kind);
#pragma warning disable 8073
            Assert.Throws<ArgumentOutOfRangeException>(() => { if ((DateTime)timestamp != null) return; });
#pragma warning restore 8073
        }

        [Test]
        public void BasicTimestampTest()
        {
            DateTimeKind[] kinds = new[] { DateTimeKind.Unspecified, DateTimeKind.Utc, DateTimeKind.Local };
            foreach (DateTimeKind kind in kinds)
            {
                CheckTimestampRepresentableAsDateTime(1, 1, 1, 0, 0, 0, 0, kind);
                CheckTimestampRepresentableAsDateTime(1, 2, 3, 1, 0, 0, 0, kind);
                CheckTimestampRepresentableAsDateTime(2019, 6, 13, 0, 1, 0, 0, kind);
                CheckTimestampRepresentableAsDateTime(1, 1, 1, 0, 0, 1, 0, kind);
                CheckTimestampRepresentableAsDateTime(1, 1, 1, 0, 0, 0, 1, kind);
                CheckTimestampRepresentableAsDateTime(9999, 12, 31, 23, 59, 59, 999999, kind);
                CheckTimestampNotRepresentableAsDateTime(-1, 1, 1, 0, 0, 0, 0, kind);
                CheckTimestampNotRepresentableAsDateTime(-10, 1, 1, 1, 0, 0, 0, kind);
                CheckTimestampNotRepresentableAsDateTime(-1000, 1, 1, 0, 1, 0, 0, kind);
                CheckTimestampNotRepresentableAsDateTime(-4712, 1, 1, 0, 0, 1, 0, kind);
                CheckTimestampNotRepresentableAsDateTime(100000, 1, 1, 0, 0, 0, 1, kind);
            }
        }

        [Test]
        public void TimestampSpecifyKindTest()
        {
            Timestamp timestamp = new Timestamp(2019, 6, 13, 13, 14, 15, 1001);
            Timestamp timestamp2 = new Timestamp(2019, 6, 13, 13, 14, 15);
            Timestamp timestamp3 = new Timestamp(2019, 6, 13);
            Assert.AreEqual(DateTimeKind.Unspecified, timestamp.Kind);
            Assert.AreEqual(DateTimeKind.Unspecified, timestamp2.Kind);
            Assert.AreEqual(DateTimeKind.Unspecified, timestamp3.Kind);
            Assert.AreEqual(DateTimeKind.Unspecified, Timestamp.SpecifyKind(timestamp, DateTimeKind.Unspecified).Kind);
            Assert.AreEqual(DateTimeKind.Local, Timestamp.SpecifyKind(timestamp, DateTimeKind.Local).Kind);
            Assert.AreEqual(DateTimeKind.Utc, Timestamp.SpecifyKind(timestamp, DateTimeKind.Utc).Kind);

            Timestamp timestampUnspecified = new Timestamp(2019, 6, 13, 13, 14, 15, 1001, DateTimeKind.Unspecified);
            Assert.AreEqual(timestampUnspecified, Timestamp.SpecifyKind(timestamp, DateTimeKind.Unspecified));
            Assert.AreEqual(Timestamp.SpecifyKind(timestampUnspecified, DateTimeKind.Unspecified), timestamp);

            Timestamp timestampUtc = new Timestamp(2019, 6, 13, 13, 14, 15, 1001, DateTimeKind.Utc);
            Assert.AreEqual(timestampUtc, Timestamp.SpecifyKind(timestamp, DateTimeKind.Utc));
            Assert.AreEqual(Timestamp.SpecifyKind(timestampUtc, DateTimeKind.Unspecified), timestamp);

            Timestamp timestampLocal = new Timestamp(2019, 6, 13, 13, 14, 15, 1001, DateTimeKind.Local);
            Assert.AreEqual(timestampLocal, Timestamp.SpecifyKind(timestamp, DateTimeKind.Local));
            Assert.AreEqual(Timestamp.SpecifyKind(timestampLocal, DateTimeKind.Unspecified), timestamp);
        }

        [Test]
        public void TimestampConversionTest()
        {
            Timestamp timestamp = new Timestamp(2019, 6, 13, 13, 14, 15, DateTimeKind.Unspecified);
            Timestamp timestamp2 = new Timestamp(2019, 6, 13, 13, 14, 15, DateTimeKind.Local);
            Timestamp timestamp3 = new Timestamp(2019, 6, 13, 13, 14, 15, DateTimeKind.Utc);
            // ToLocalTime()
            Assert.AreEqual(timestamp2.ToLocalTime(), timestamp2);
            Assert.AreEqual(timestamp.ToLocalTime(), timestamp3.ToLocalTime());
            // ToUniversalTime()
            Assert.AreEqual(timestamp3.ToUniversalTime(), timestamp3);
            Assert.AreEqual(timestamp.ToUniversalTime(), timestamp2.ToUniversalTime());
            // Roundtrip
            Assert.AreEqual(timestamp2.ToUniversalTime().ToLocalTime(), timestamp2);
            Assert.AreEqual(timestamp3.ToLocalTime().ToUniversalTime(), timestamp3);

            DateTime dateTime = new DateTime(2019, 6, 13, 13, 14, 15, DateTimeKind.Unspecified);
            DateTime dateTime2 = new DateTime(2019, 6, 13, 13, 14, 15, DateTimeKind.Local);
            DateTime dateTime3 = new DateTime(2019, 6, 13, 13, 14, 15, DateTimeKind.Utc);
            // ToLocalTime()
            Assert.AreEqual(timestamp.ToLocalTime(), (Timestamp)dateTime.ToLocalTime());
            Assert.AreEqual(timestamp2.ToLocalTime(), (Timestamp)dateTime2.ToLocalTime());
            Assert.AreEqual(timestamp3.ToLocalTime(), (Timestamp)dateTime3.ToLocalTime());
            // ToUniversalTime()
            Assert.AreEqual(timestamp.ToUniversalTime(), (Timestamp)dateTime.ToUniversalTime());
            Assert.AreEqual(timestamp2.ToUniversalTime(), (Timestamp)dateTime2.ToUniversalTime());
            Assert.AreEqual(timestamp3.ToUniversalTime(), (Timestamp)dateTime3.ToUniversalTime());
        }

        private class TimestampComparerWithOperators : TestUtil.IComparerWithOperators<Timestamp>
        {
            public bool Eq(Timestamp x, Timestamp y)
            {
                return x == y;
            }

            public bool Ge(Timestamp x, Timestamp y)
            {
                return x >= y;
            }

            public bool Gt(Timestamp x, Timestamp y)
            {
                return x > y;
            }

            public bool Le(Timestamp x, Timestamp y)
            {
                return x <= y;
            }

            public bool Lt(Timestamp x, Timestamp y)
            {
                return x < y;
            }

            public bool Ne(Timestamp x, Timestamp y)
            {
                return x != y;
            }
        }

        private void CheckComparison(Timestamp lhs, Timestamp rhs)
        {
            TestUtil.CheckComparison(lhs, rhs, new TimestampComparerWithOperators());
        }

        [Test]
        public void TestTimestampComparison()
        {
            Assert.AreEqual(new Timestamp(1, 1, 1, 0, 0, 0, 1), new Timestamp(1, 1, 1, 0, 0, 0, 1));
            Assert.AreEqual(0, new Timestamp(1, 1, 1, 0, 0, 0, 1).CompareTo(new Timestamp(1, 1, 1, 0, 0, 0, 1)));

            Assert.IsTrue(new Timestamp(1, 1, 1, 0, 0, 0, 1) == new Timestamp(1, 1, 1, 0, 0, 0, 1));
            Assert.IsTrue(new Timestamp(1, 1, 1, 0, 0, 0, 1) <= new Timestamp(1, 1, 1, 0, 0, 0, 1));
            Assert.IsTrue(new Timestamp(1, 1, 1, 0, 0, 0, 1) >= new Timestamp(1, 1, 1, 0, 0, 0, 1));
            Assert.IsFalse(new Timestamp(1, 1, 1, 0, 0, 0, 1) != new Timestamp(1, 1, 1, 0, 0, 0, 1));
            Assert.IsFalse(new Timestamp(1, 1, 1, 0, 0, 0, 1) < new Timestamp(1, 1, 1, 0, 0, 0, 1));
            Assert.IsFalse(new Timestamp(1, 1, 1, 0, 0, 0, 1) > new Timestamp(1, 1, 1, 0, 0, 0, 1));

            CheckComparison(new Timestamp(1, 1, 1, 0, 0, 0, 1), new Timestamp(1, 1, 1, 0, 0, 0, 2));
            CheckComparison(new Timestamp(-1, 1, 1), new Timestamp(1, 2, 3));
        }

        [Test]
        public void TestTimestampFormatting()
        {
            Assert.AreEqual("-1000-01-01 00:00:00", new Timestamp(-1000, 01, 01).ToString());
            Assert.AreEqual("2019-06-13 00:00:00", new Timestamp(2019, 06, 13).ToString());
            Assert.AreEqual("2019-06-13 13:23:34", new Timestamp(2019, 06, 13, 13, 23, 34).ToString());
            Assert.AreEqual("2019-06-13 13:23:34.000765", new Timestamp(2019, 06, 13, 13, 23, 34, 765).ToString());
            Assert.AreEqual("2019-06-13 13:23:34.765000", new Timestamp(2019, 06, 13, 13, 23, 34, 765000).ToString());
            Assert.AreEqual("2019-06-13 13:23:34.765765", new Timestamp(2019, 06, 13, 13, 23, 34, 765765).ToString());
        }
    }
}
