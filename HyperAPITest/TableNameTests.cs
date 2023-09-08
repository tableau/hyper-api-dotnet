using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using NUnit.Framework;

namespace Tableau.HyperAPI.Test
{
    class TableNameTests
    {
        private void CheckTableName(TableName tableName, string[] components)
        {
            if (tableName.SchemaName != null)
            {
                if (tableName.DatabaseName != null)
                    Assert.IsTrue(tableName.IsFullyQualified());
                else
                    Assert.IsFalse(tableName.IsFullyQualified());

                Assert.IsTrue(components.Length > 1);
                CheckSchemaName(tableName.SchemaName, components.Take(components.Length - 1).ToArray());
            }
            else
            {
                Assert.IsTrue(components.Length == 1);
            }

            Assert.AreEqual(components[components.Length - 1], tableName.Name.Unescaped);
            string expectedEscaped = string.Join(".", components.Select(c => new Name(c).ToString()));
            Assert.AreEqual(expectedEscaped, tableName.ToString());
        }

        private void CheckSchemaName(SchemaName schemaName, string[] components)
        {
            if (schemaName.DatabaseName != null)
            {
                Assert.IsTrue(components.Length == 2);
                Assert.AreEqual(components[0], schemaName.DatabaseName.Unescaped);
                Assert.AreEqual(new Name(components[0]), schemaName.DatabaseName.Name);
            }
            else
            {
                Assert.IsTrue(components.Length == 1);
            }
            Assert.AreEqual(components[components.Length - 1], schemaName.Name.Unescaped);
            string expectedEscaped = string.Join(".", components.Select(c => new Name(c).ToString()));
            Assert.AreEqual(expectedEscaped, schemaName.ToString());
        }

        [Test]
        public void TestBasic()
        {
            CheckTableName(new TableName("a"), new[] { "a" });
            CheckTableName(new TableName("a.b"), new[] { "a.b" });
            CheckTableName(new TableName("a", "b"), new[] { "a", "b" });
            CheckTableName(new TableName("a", "b", "c"), new[] { "a", "b", "c" });
            CheckTableName(new TableName("a", "x.y", "c"), new[] { "a", "x.y", "c" });
            CheckTableName(new TableName("a", "\"x.y\"", "c"), new[] { "a", "\"x.y\"", "c" });
        }

        private class ComparerWithOperators : TestUtil.IComparerWithOperators<TableName>
        {
            public bool Eq(TableName x, TableName y)
            {
                return x == y;
            }

            public bool Ge(TableName x, TableName y)
            {
                return x >= y;
            }

            public bool Gt(TableName x, TableName y)
            {
                return x > y;
            }

            public bool Le(TableName x, TableName y)
            {
                return x <= y;
            }

            public bool Lt(TableName x, TableName y)
            {
                return x < y;
            }

            public bool Ne(TableName x, TableName y)
            {
                return x != y;
            }
        }

        [Test]
        public void TestComparison()
        {
            var comparer = new ComparerWithOperators();
            TestUtil.CheckComparison(new TableName("a"), new TableName("b"), comparer);
            TestUtil.CheckComparison(new TableName("b"), new TableName("a", "b"), comparer);
            TestUtil.CheckComparison(new TableName("a"), new TableName("a", "b"), comparer);
            TestUtil.CheckComparison(new TableName("a", "b"), new TableName("b", "a"), comparer);
            TestUtil.CheckComparison(new TableName("a", "b"), new TableName("a", "c"), comparer);
            TestUtil.CheckComparison(new TableName("x", "y"), new TableName("a", "b", "c"), comparer);
            TestUtil.CheckComparison(new TableName("a", "b", "c"), new TableName("x", "y", "z"), comparer);
            TestUtil.CheckComparison(new TableName("x", "b", "c"), new TableName("x", "y", "z"), comparer);
            TestUtil.CheckComparison(new TableName("x", "y", "c"), new TableName("x", "y", "z"), comparer);
            TestUtil.CheckComparison(new TableName("y", "z"), new TableName("x", "y", "z"), comparer);
        }
    }
}
