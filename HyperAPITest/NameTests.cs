using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

namespace Tableau.HyperAPI.Test
{
    class NameTests
    {
        private void CheckName(string unescaped, string escaped)
        {
            Name name = unescaped;
            Assert.AreEqual(unescaped, name.Unescaped);
            Assert.AreEqual(Sql.EscapeName(unescaped), name);
            Assert.AreEqual(escaped, name.ToString());
        }

        [Test]
        public void TestBasic()
        {
            CheckName("x", "\"x\"");
            CheckName("x.y", "\"x.y\"");
            CheckName("\"", "\"\"\"\"");
        }

        private class ComparerWithOperators : TestUtil.IComparerWithOperators<Name>
        {
            public bool Eq(Name x, Name y)
            {
                return x == y;
            }

            public bool Ge(Name x, Name y)
            {
                return x >= y;
            }

            public bool Gt(Name x, Name y)
            {
                return x > y;
            }

            public bool Le(Name x, Name y)
            {
                return x <= y;
            }

            public bool Lt(Name x, Name y)
            {
                return x < y;
            }

            public bool Ne(Name x, Name y)
            {
                return x != y;
            }
        }

        [Test]
        public void TestComparison()
        {
            ComparerWithOperators comparer = new ComparerWithOperators();
            TestUtil.CheckComparison(new Name("x"), new Name("y"), comparer);
            TestUtil.CheckComparison(new Name("x.y"), new Name("y"), comparer);
            TestUtil.CheckComparison(new Name("x"), new Name("y.z"), comparer);
        }
    }
}
