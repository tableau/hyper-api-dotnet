using System;

using NUnit.Framework;

namespace Tableau.HyperAPI.Test
{
    [TestFixture]
    class InserterInternalsTests : HyperTest
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
        public void TestBigWrite()
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                connection.ExecuteCommand("DROP TABLE IF EXISTS \"table\"");

                var tableDefinition = new TableDefinition("table")
                    .AddColumn("bool", SqlType.Bool(), Nullability.NotNullable)
                    .AddColumn("booln", SqlType.Bool())
                    .AddColumn("short", SqlType.SmallInt(), Nullability.NotNullable)
                    .AddColumn("shortn", SqlType.SmallInt())
                    .AddColumn("int", SqlType.Int(), Nullability.NotNullable)
                    .AddColumn("intn", SqlType.Int())
                    .AddColumn("long", SqlType.BigInt(), Nullability.NotNullable)
                    .AddColumn("longn", SqlType.BigInt())
                    .AddColumn("double", SqlType.Double(), Nullability.NotNullable)
                    .AddColumn("doublen", SqlType.Double())
                    .AddColumn("text", SqlType.Text(), Nullability.NotNullable)
                    .AddColumn("textn", SqlType.Text())
                    .AddColumn("bytes", SqlType.Bytes(), Nullability.NotNullable)
                    .AddColumn("bytesn", SqlType.Bytes())
                    .AddColumn("time", SqlType.Time(), Nullability.NotNullable)
                    .AddColumn("timen", SqlType.Time())
                    .AddColumn("date", SqlType.Date(), Nullability.NotNullable)
                    .AddColumn("daten", SqlType.Date())
                    .AddColumn("timestamp", SqlType.Timestamp(), Nullability.NotNullable)
                    .AddColumn("timestampn", SqlType.Timestamp())
                    ;

                CreateTable(connection, tableDefinition, inserter =>
                {
                    inserter.ChunkSize = 1024;
                    for (int i = 0; i < 1000; ++i)
                    {
                        inserter.Add(true);
                        inserter.Add(true);
                        inserter.Add((short)1);
                        inserter.Add((short)2);
                        inserter.Add(3);
                        inserter.Add(4);
                        inserter.Add((long)5);
                        inserter.Add((long)6);
                        inserter.Add((double)7.0);
                        inserter.Add((double)8.0);
                        inserter.Add("a");
                        inserter.Add("b");
                        inserter.Add(new byte[] { 0, 1, 2, 3, 4 });
                        inserter.Add(new byte[] { 5, 6, 7, 8, 9 });
                        inserter.Add(new TimeSpan(1, 2, 3, 4, 5));
                        inserter.Add(new TimeSpan(2, 3, 4, 5, 6));
                        inserter.Add(new Date(2019, 3, 1));
                        inserter.Add(new Date(2020, 5, 5));
                        inserter.Add(new Timestamp(2019, 3, 1, 1, 2, 3, 4));
                        inserter.Add(new Timestamp(2020, 5, 5, 2, 3, 4, 5));
                        inserter.EndRow();
                    }

                    Assert.AreNotEqual(0, inserter.FlushCount);
                });

                connection.ExecuteCommand("DROP TABLE \"table\"");
            }
        }

        [Test]
        public void TestCancel()
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                connection.ExecuteCommand("DROP TABLE IF EXISTS \"foo\"");

                var tableDefinition = new TableDefinition("foo")
                    .AddColumn("text", SqlType.Text(), Nullability.NotNullable)
                    ;

                connection.Catalog.CreateTable(tableDefinition);
                using (Inserter inserter = new Inserter(connection, tableDefinition))
                {
                    inserter.Add("a");
                    inserter.EndRow();
                    inserter.Add("b");
                    inserter.EndRow();
                    inserter.Add("c");
                    inserter.EndRow();
                    // Do not call Execute()
                }

                Assert.AreEqual(0, connection.ExecuteScalarQuery<long>("SELECT COUNT(*) FROM foo"));
                connection.ExecuteCommand("DROP TABLE IF EXISTS \"foo\"");

                // Make sure some data is flushed to server before canceling
                connection.Catalog.CreateTable(tableDefinition);
                using (Inserter inserter = new Inserter(connection, tableDefinition))
                {
                    inserter.ChunkSize = 1024;
                    for (int i = 0; i < 1000; ++i)
                    {
                        inserter.Add("abcdefghijklmnop");
                        inserter.EndRow();
                    }

                    Assert.AreNotEqual(0, inserter.FlushCount);

                    // Do not call Execute()
                }

                Assert.AreEqual(0, connection.ExecuteScalarQuery<long>("SELECT COUNT(*) FROM foo"));

                connection.ExecuteCommand("DROP TABLE \"foo\"");
            }
        }
    }
}
