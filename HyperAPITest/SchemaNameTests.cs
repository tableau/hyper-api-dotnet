using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using NUnit.Framework;

namespace Tableau.HyperAPI.Test
{
    class SchemaNameTests
    {
        private void CheckSchemaName(SchemaName name, string[] components)
        {
            if (name.DatabaseName != null)
            {
                Assert.IsTrue(components.Length == 2);
                Assert.IsTrue(name.IsFullyQualified());
                Assert.AreEqual(components[0], name.DatabaseName.Unescaped);
                Assert.AreEqual(new Name(components[0]), name.DatabaseName.Name);
            }
            else
            {
                Assert.IsTrue(components.Length == 1);
            }

            Assert.AreEqual(components[components.Length - 1], name.Name.Unescaped);
            string expectedEscaped = string.Join(".", components.Select(c => new Name(c).ToString()));
            Assert.AreEqual(expectedEscaped, name.ToString());
        }

        [Test]
        public void TestBasic()
        {
            CheckSchemaName(new SchemaName("a"), new[] { "a" });
            CheckSchemaName(new SchemaName("a.b"), new[] { "a.b" });
            CheckSchemaName(new SchemaName("a", "b"), new[] { "a", "b" });
            CheckSchemaName(new SchemaName("a", "x.y"), new[] { "a", "x.y" });
            CheckSchemaName(new SchemaName("a", "\"x.y\""), new[] { "a", "\"x.y\"" });
        }

        private class ComparerWithOperators : TestUtil.IComparerWithOperators<SchemaName>
        {
            public bool Eq(SchemaName x, SchemaName y)
            {
                return x == y;
            }

            public bool Ge(SchemaName x, SchemaName y)
            {
                return x >= y;
            }

            public bool Gt(SchemaName x, SchemaName y)
            {
                return x > y;
            }

            public bool Le(SchemaName x, SchemaName y)
            {
                return x <= y;
            }

            public bool Lt(SchemaName x, SchemaName y)
            {
                return x < y;
            }

            public bool Ne(SchemaName x, SchemaName y)
            {
                return x != y;
            }
        }

        [Test]
        public void TestComparison()
        {
            var comparer = new ComparerWithOperators();
            TestUtil.CheckComparison(new SchemaName("a"), new SchemaName("b"), comparer);
            TestUtil.CheckComparison(new SchemaName("b"), new SchemaName("a", "b"), comparer);
            TestUtil.CheckComparison(new SchemaName("a"), new SchemaName("a", "b"), comparer);
            TestUtil.CheckComparison(new SchemaName("a", "b"), new SchemaName("b", "a"), comparer);
            TestUtil.CheckComparison(new SchemaName("a", "b"), new SchemaName("a", "c"), comparer);
        }
    }
}
