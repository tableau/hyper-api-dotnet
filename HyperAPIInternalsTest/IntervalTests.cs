using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

namespace Tableau.HyperAPI.Test
{
    public class IntervalTests
    {
        [Test]
        public void TestFormatting()
        {
            Interval ti = new Interval();
            Assert.AreEqual("PT0S", ti.ToString());
            ti = Interval.FromYearsAndMonths(1, 0);
            Assert.AreEqual("P1Y", ti.ToString());
            ti = -ti;
            Assert.AreEqual("P-1Y", ti.ToString());
            ti = Interval.FromYearsAndMonths(1, 2);
            Assert.AreEqual("P1Y2M", ti.ToString());
            ti = -ti;
            Assert.AreEqual("P-1Y-2M", ti.ToString());
            ti = Interval.FromYearsAndMonths(1, 2) + Interval.FromDays(3);
            Assert.AreEqual("P1Y2M3D", ti.ToString());
            ti = -ti;
            Assert.AreEqual("P-1Y-2M-3D", ti.ToString());
            ti = Interval.FromYearsAndMonths(1, 0) + Interval.FromDays(3);
            Assert.AreEqual("P1Y3D", ti.ToString());
            ti = -ti;
            Assert.AreEqual("P-1Y-3D", ti.ToString());
            ti = Interval.FromYearsAndMonths(0, 1);
            Assert.AreEqual("P1M", ti.ToString());
            ti = -ti;
            Assert.AreEqual("P-1M", ti.ToString());
            ti = Interval.FromYearsAndMonths(0, 1) + Interval.FromDays(2);
            Assert.AreEqual("P1M2D", ti.ToString());
            ti = -ti;
            Assert.AreEqual("P-1M-2D", ti.ToString());
            ti = Interval.FromDays(3);
            Assert.AreEqual("P3D", ti.ToString());
            ti = -ti;
            Assert.AreEqual("P-3D", ti.ToString());
            ti = Interval.FromTime(1, 2, 3, 456);
            Assert.AreEqual("PT1H2M3.000456S", ti.ToString());
            ti = -ti;
            Assert.AreEqual("PT-1H-2M-3.000456S", ti.ToString());
            ti = Interval.FromTime(2, 0, 3, 456);
            Assert.AreEqual("PT2H3.000456S", ti.ToString());
            ti = -ti;
            Assert.AreEqual("PT-2H-3.000456S", ti.ToString());
            ti = Interval.FromTime(0, 2, 3, 456);
            Assert.AreEqual("PT2M3.000456S", ti.ToString());
            ti = -ti;
            Assert.AreEqual("PT-2M-3.000456S", ti.ToString());
            ti = Interval.FromTime(0, 0, 3, 456);
            Assert.AreEqual("PT3.000456S", ti.ToString());
            ti = -ti;
            Assert.AreEqual("PT-3.000456S", ti.ToString());
            ti = Interval.FromYearsAndMonths(1, 0) + Interval.FromTime(0, 0, 3, 456);
            Assert.AreEqual("P1YT3.000456S", ti.ToString());
            ti = -ti;
            Assert.AreEqual("P-1YT-3.000456S", ti.ToString());
            ti = Interval.FromYearsAndMonths(0, 1) + Interval.FromTime(0, 0, 3, 456);
            Assert.AreEqual("P1MT3.000456S", ti.ToString());
            ti = -ti;
            Assert.AreEqual("P-1MT-3.000456S", ti.ToString());
        }

        private class ComparerWithOperators : TestUtil.IComparerWithOperators<Interval>
        {
            public bool Eq(Interval x, Interval y)
            {
                return x == y;
            }

            public bool Ge(Interval x, Interval y)
            {
                return x >= y;
            }

            public bool Gt(Interval x, Interval y)
            {
                return x > y;
            }

            public bool Le(Interval x, Interval y)
            {
                return x <= y;
            }

            public bool Lt(Interval x, Interval y)
            {
                return x < y;
            }

            public bool Ne(Interval x, Interval y)
            {
                return x != y;
            }
        }

        private void CheckComparison(Interval lhs, Interval rhs)
        {
            TestUtil.CheckComparison(lhs, rhs, new ComparerWithOperators());
        }

        [Test]
        public void TestComparison()
        {
            Assert.AreEqual(new Interval(1, 0, 0), new Interval(1, 0, 0));
            Assert.AreEqual(0, new Interval(1, 0, 0).CompareTo(new Interval(1, 0, 0)));

            Assert.IsTrue(new Interval(1, 0, 0) == new Interval(1, 0, 0));
            Assert.IsTrue(new Interval(1, 0, 0) <= new Interval(1, 0, 0));
            Assert.IsTrue(new Interval(1, 0, 0) >= new Interval(1, 0, 0));
            Assert.IsFalse(new Interval(1, 0, 0) != new Interval(1, 0, 0));
            Assert.IsFalse(new Interval(1, 0, 0) < new Interval(1, 0, 0));
            Assert.IsFalse(new Interval(1, 0, 0) > new Interval(1, 0, 0));

            CheckComparison(new Interval(1, 0, 0), new Interval(2, 0, 0));
            CheckComparison(new Interval(-1, 0, 0), new Interval(0, 0, 0));
            CheckComparison(new Interval(0, 1, 0), new Interval(0, 2, 0));
            CheckComparison(new Interval(0, -1, 0), new Interval(0, 0, 0));
            CheckComparison(new Interval(0, 0, 1), new Interval(0, 0, 2));
            CheckComparison(new Interval(0, 0, -1), new Interval(0, 0, 0));
            CheckComparison(new Interval(0, 2, 0), new Interval(1, 0, 0));
            CheckComparison(new Interval(0, 0, 2), new Interval(0, 1, 0));
            CheckComparison(new Interval(0, 0, 2), new Interval(1, 0, 0));
        }

        [Test]
        public void TestMisc()
        {
            Assert.AreEqual(Interval.FromSeconds(1.5), Interval.FromSeconds(1, 500_000));
        }
    }
}
