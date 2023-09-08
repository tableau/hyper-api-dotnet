using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

namespace Tableau.HyperAPI.Test
{
    public class SqlTypeTests
    {
        internal class ComparerWithOperators : TestUtil.IComparerWithOperators<SqlType>
        {
            public bool Eq(SqlType x, SqlType y)
            {
                return x == y;
            }

            public bool Ge(SqlType x, SqlType y)
            {
                return x >= y;
            }

            public bool Gt(SqlType x, SqlType y)
            {
                return x > y;
            }

            public bool Le(SqlType x, SqlType y)
            {
                return x <= y;
            }

            public bool Lt(SqlType x, SqlType y)
            {
                return x < y;
            }

            public bool Ne(SqlType x, SqlType y)
            {
                return x != y;
            }
        }

        private void CheckComparison(SqlType lhs, SqlType rhs)
        {
            ComparerWithOperators comparer = new ComparerWithOperators();
            TestUtil.CheckComparison(lhs, rhs, comparer);
        }

        [Test]
        public void TestComparison()
        {
            Assert.AreNotEqual(null, SqlType.Double());
            Assert.AreNotEqual(SqlType.Double(), null);
            Assert.IsFalse(SqlType.Double() == null);
            Assert.IsFalse(null == SqlType.Double());
            Assert.IsFalse((SqlType)null == SqlType.Double());
            Assert.IsTrue(SqlType.Double() != null);
            Assert.IsTrue(null != SqlType.Double());
            Assert.IsTrue((SqlType)null != SqlType.Double());

            Assert.AreEqual(SqlType.Double(), SqlType.Double());
            Assert.AreEqual(0, SqlType.Double().CompareTo(SqlType.Double()));

            Assert.IsTrue(SqlType.Double() == SqlType.Double());
            Assert.IsTrue(SqlType.Double() <= SqlType.Double());
            Assert.IsTrue(SqlType.Double() >= SqlType.Double());
            Assert.IsFalse(SqlType.Double() != SqlType.Double());
            Assert.IsFalse(SqlType.Double() < SqlType.Double());
            Assert.IsFalse(SqlType.Double() > SqlType.Double());

            CheckComparison(SqlType.BigInt(), SqlType.Text());
            CheckComparison(SqlType.BigInt(), SqlType.Numeric(18, 2));
            CheckComparison(SqlType.Numeric(17, 2), SqlType.Numeric(18, 2));
            CheckComparison(SqlType.Numeric(18, 2), SqlType.Numeric(18, 3));
            CheckComparison(SqlType.Varchar(10), SqlType.Char(8));
            CheckComparison(SqlType.Varchar(8), SqlType.Char(10));
            CheckComparison(SqlType.Char(10), SqlType.Char(11));
        }
    }
}
