using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using NUnit.Framework;

namespace Tableau.HyperAPI.Test
{
    class DatabaseNameTests
    {
        private class ComparerWithOperators : TestUtil.IComparerWithOperators<DatabaseName>
        {
            public bool Eq(DatabaseName x, DatabaseName y)
            {
                return x == y;
            }

            public bool Ge(DatabaseName x, DatabaseName y)
            {
                return x >= y;
            }

            public bool Gt(DatabaseName x, DatabaseName y)
            {
                return x > y;
            }

            public bool Le(DatabaseName x, DatabaseName y)
            {
                return x <= y;
            }

            public bool Lt(DatabaseName x, DatabaseName y)
            {
                return x < y;
            }

            public bool Ne(DatabaseName x, DatabaseName y)
            {
                return x != y;
            }
        }

        private void CheckDatabaseName(string unescaped, string escaped)
        {
            DatabaseName name = new DatabaseName(unescaped);
            Assert.AreEqual(unescaped, name.Unescaped);
            Assert.AreEqual(Sql.EscapeName(unescaped), name.Name);
            Assert.AreEqual(escaped, name.ToString());
        }

        [Test]
        public void TestBasic()
        {
            CheckDatabaseName("x", "\"x\"");
            CheckDatabaseName("x.y", "\"x.y\"");
            CheckDatabaseName("\"", "\"\"\"\"");
        }

        [Test]
        public void TestComparison()
        {
            ComparerWithOperators comparer = new ComparerWithOperators();
            TestUtil.CheckComparison(new DatabaseName("x"), new DatabaseName("y"), comparer);
            TestUtil.CheckComparison(new DatabaseName("x.y"), new DatabaseName("y"), comparer);
            TestUtil.CheckComparison(new DatabaseName("x"), new DatabaseName("y.z"), comparer);
        }
    }
}
