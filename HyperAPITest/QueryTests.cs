using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

namespace Tableau.HyperAPI.Test
{
    [TestFixture]
    public class QueryTests : HyperTest
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            SetUpTest(TestFlags.CreateDatabase);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            TearDownTest();
        }

        [Test]
        public void TestBasic()
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                CreateSampleTable(connection, "foo");

                Assert.AreEqual(3, connection.ExecuteScalarQuery<long>("SELECT COUNT(*) FROM foo"));
                Assert.Throws<HyperException>(() => connection.ExecuteScalarQuery<int>("SELECT * FROM foo"));
                Assert.AreEqual(1, connection.ExecuteScalarQuery<long>("SELECT COUNT(*) FROM foo WHERE text = 'x'"));
                Assert.Throws<HyperException>(() => connection.ExecuteScalarQuery<int>("SELECT * FROM foo WHERE text = 'x'"));

                using (Result result = connection.ExecuteQuery("SELECT * FROM foo"))
                {
                    AssertEqualResultSchemas(GetSampleSchema("foo"), result.Schema);

                    while (result.NextRow())
                    {
                    }
                }

                connection.ExecuteCommand("DROP TABLE foo");
                Assert.Throws<HyperException>(() => connection.ExecuteQuery("SELECT * FROM foo"));
            }
        }

        [Test]
        public void TestZeroRows()
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                CreateSampleTable(connection, "foo");

                using (Result result = connection.ExecuteQuery("SELECT * FROM foo WHERE FALSE"))
                {
                    AssertEqualResultSchemas(GetSampleSchema("foo"), result.Schema);
                    Assert.IsFalse(result.NextRow());
                }

                connection.ExecuteCommand("DROP TABLE foo");
            }
        }

        [Test]
        public void TestSingleRow()
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                CreateSampleTable(connection, "foo");

                using (Result result = connection.ExecuteQuery("SELECT * FROM foo WHERE \"text\" = 'x'"))
                {
                    AssertEqualResultSchemas(GetSampleSchema("foo"), result.Schema);
                    Assert.IsTrue(result.NextRow());
                    Assert.IsFalse(result.NextRow());
                }

                Assert.AreEqual(1, connection.ExecuteScalarQuery<int>("SELECT \"int\" FROM foo WHERE \"text\" = 'x'"));

                connection.ExecuteCommand("DROP TABLE foo");
            }
        }

        [Test]
        public void TestQuoteFunctions()
        {
            Assert.AreEqual("\"foo\"", Sql.EscapeName("foo").ToString());
            Assert.AreEqual("\"\"\"foo\"\"\"", Sql.EscapeName("\"foo\"").ToString());

            Assert.AreEqual("''", Sql.EscapeStringLiteral(""));
            Assert.AreEqual("'foo'", Sql.EscapeStringLiteral("foo"));
            Assert.AreEqual("'''foo'''", Sql.EscapeStringLiteral("'foo'"));
        }

        private void TestZeroColumns(string query, int expectedRows)
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                using (Result result = connection.ExecuteQuery(query))
                {
                    for (int i = 0; i < expectedRows; ++i)
                    {
                        Assert.IsTrue(result.NextRow());
                        Assert.AreEqual(new object[0], result.GetValues());
                    }

                    Assert.IsFalse(result.NextRow());
                }
            }
        }

        [Test]
        public void TestZeroColumnsZeroRows()
        {
            TestZeroColumns("SELECT WHERE FALSE", 0);
        }

        [Test]
        public void TestZeroColumnsNonZeroRows()
        {
            TestZeroColumns("SELECT FROM generate_series(1, 10)", 10);
        }

        [Test]
        public void TestReadNullUnsupported()
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                using (Result result = connection.ExecuteQuery("SELECT NULL"))
                {
                    Assert.AreEqual(1, result.Schema.ColumnCount);
                    Assert.AreEqual(TypeTag.Text, result.Schema.GetColumn(0).Type.Tag);
                    Assert.IsTrue(result.NextRow());
                    Assert.IsTrue(result.IsNull(0));
                    Assert.AreEqual(null, result.GetValue(0));
                }
            }
        }

        [Test]
        public void TestGetRaw()
        {
            using (var connection = new Connection(Server.Endpoint, Database))
            {
                connection.ExecuteCommand("DROP TABLE IF EXISTS foo");

                var schema = new TableDefinition("foo").AddColumn("x", SqlType.Int());
                connection.Catalog.CreateTable(schema);

                int value = 987654321;

                using (var inserter = new Inserter(connection, schema))
                {
                    inserter.Add(value);
                    inserter.EndRow();
                    inserter.Execute();
                }

                using (Result result = connection.ExecuteQuery("SELECT * FROM foo"))
                {
                    Assert.IsTrue(result.NextRow());
                    Assert.AreEqual(BitConverter.GetBytes(value), result.GetRaw(0));
                    Assert.IsFalse(result.NextRow());
                }
            }
        }
    }
}
