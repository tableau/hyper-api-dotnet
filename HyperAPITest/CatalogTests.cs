using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace Tableau.HyperAPI.Test
{
    [TestFixture]
    class CatalogTests : HyperTest
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            SetUpTest(TestFlags.StartServer);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            TearDownTest();
        }

        [Test]
        public void TestCreateAndDropDatabase()
        {
            string dbPath = Path.Combine(TestEnv.DbDir, "testdb.hyper");

            if (File.Exists(dbPath))
            {
                using (Connection connection = new Connection(Server.Endpoint))
                    connection.Catalog.DropDatabase(dbPath);
                Assert.IsFalse(File.Exists(dbPath));
            }

            using (Connection connection = new Connection(Server.Endpoint))
                connection.Catalog.CreateDatabase(dbPath);

            Assert.IsTrue(File.Exists(dbPath));

            using (Connection connection = new Connection(Server.Endpoint, dbPath))
            {
                TableDefinition tableDef = new TableDefinition("foo")
                    .AddColumn("x", SqlType.Text());
                connection.Catalog.CreateTable(tableDef);
            }

            using (Connection connection = new Connection(Server.Endpoint, dbPath))
                connection.ExecuteCommand("SELECT * FROM foo");

            using (Connection connection = new Connection(Server.Endpoint))
                connection.Catalog.DropDatabase(dbPath);

            Assert.IsFalse(File.Exists(dbPath));

            using (Connection connection = new Connection(Server.Endpoint))
            {
                connection.Catalog.CreateDatabase(dbPath);
                connection.Catalog.AttachDatabase(dbPath, "db");
                connection.Catalog.CreateTable(new TableDefinition(new TableName("db", "public", "foo")));
                connection.Catalog.DetachDatabase("db");
            }

            Assert.IsTrue(File.Exists(dbPath));
            using (Connection connection = new Connection(Server.Endpoint))
                Assert.Throws<HyperException>(() => connection.Catalog.CreateDatabase(dbPath));

            using (Connection connection = new Connection(Server.Endpoint))
            {
                connection.Catalog.CreateDatabaseIfNotExists(dbPath);
                connection.Catalog.AttachDatabase(dbPath, "db");
                connection.ExecuteCommand($"SELECT * FROM {new TableName("db", "public", "foo")}");
                connection.Catalog.DetachDatabase("db");
                connection.Catalog.DropDatabase(dbPath);
                Assert.IsFalse(File.Exists(dbPath));

                connection.Catalog.CreateDatabaseIfNotExists(dbPath);
                Assert.IsTrue(File.Exists(dbPath));
                connection.Catalog.AttachDatabase(dbPath, "db");
                Assert.Throws<HyperException>(() => connection.ExecuteCommand($"SELECT * FROM {new TableName("db", "public", "foo")}"));
                connection.Catalog.DetachDatabase("db");

                connection.Catalog.DropDatabaseIfExists(dbPath);
                Assert.IsFalse(File.Exists(dbPath));
                connection.Catalog.DropDatabaseIfExists(dbPath);
            }
        }

        [Test]
        public void TestDropConnectedDatabase()
        {
            string dbPath = Path.Combine(TestEnv.DbDir, "testdb.hyper");

            using (new Connection(Server.Endpoint, dbPath, CreateMode.CreateAndReplace))
            {
            }

            Assert.IsTrue(File.Exists(dbPath));

            using (Connection connection = new Connection(Server.Endpoint, dbPath))
            {
                Assert.Throws<HyperException>(() => connection.Catalog.DropDatabase(dbPath));
            }

            Assert.IsTrue(File.Exists(dbPath));
        }

        [Test]
        public void TestAttachAndDetach()
        {
            string dbPath = Path.Combine(TestEnv.DbDir, "attachdetach.hyper");
            string dbName = Path.GetFileNameWithoutExtension(dbPath);

            using (new Connection(Server.Endpoint, dbPath, CreateMode.CreateAndReplace))
            {
            }

            using (Connection connection = new Connection(Server.Endpoint))
            {
                Assert.Throws<HyperException>(() => connection.Catalog.GetSchemaNames("attachdetach"));

                connection.Catalog.AttachDatabase(dbPath);
                Assert.Throws<HyperException>(() => connection.Catalog.GetSchemaNames(dbPath));
                CheckGetSchemaNames(connection, "attachdetach", new[] { "public" });
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    Assert.Throws<IOException>(() => File.Delete(dbPath));

                connection.Catalog.DetachDatabase("attachdetach");
                Assert.Throws<HyperException>(() => connection.Catalog.GetSchemaNames(dbName));
                Assert.Throws<HyperException>(() => connection.Catalog.GetSchemaNames("attachdetach"));

                connection.Catalog.AttachDatabase(dbPath, "x");
                Assert.Throws<HyperException>(() => connection.Catalog.GetSchemaNames(dbName));
                CheckGetSchemaNames(connection, "x", new[] { "public" });
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    Assert.Throws<IOException>(() => File.Delete(dbPath));

                connection.Catalog.DetachAllDatabases();
                Assert.Throws<HyperException>(() => connection.Catalog.GetSchemaNames(dbName));
                Assert.Throws<HyperException>(() => connection.Catalog.GetSchemaNames("attachdetach"));
                Assert.Throws<HyperException>(() => connection.Catalog.GetSchemaNames("x"));

                File.Delete(dbPath);
            }
        }

        [Test]
        public void TestDetachConnectedDatabase()
        {
            string dbPath = Path.Combine(TestEnv.DbDir, "testdb.hyper");
            string dbName = Path.GetFileNameWithoutExtension(dbPath);

            using (var connection = new Connection(Server.Endpoint, dbPath, CreateMode.CreateAndReplace))
            {
                connection.ExecuteCommand("CREATE TABLE foo ()");
                connection.ExecuteCommand("SELECT * FROM foo");
            }

            Assert.IsTrue(File.Exists(dbPath));

            using (Connection connection = new Connection(Server.Endpoint, dbPath))
            {
                connection.ExecuteCommand("SELECT * FROM foo");
                connection.Catalog.DetachDatabase(dbName);
                Assert.Throws<HyperException>(() => connection.ExecuteCommand("SELECT * FROM foo"));
                File.Delete(dbPath);
            }
        }

        private void CheckGetSchemaNames(Connection connection, string databasePath, IEnumerable<string> expectedNames)
        {
            TestUtil.AssertEqualSets(expectedNames.Select(s => databasePath != null ? new SchemaName(databasePath, s) : new SchemaName(s)),
                connection.Catalog.GetSchemaNames(databasePath != null ? new DatabaseName(databasePath) : null));
        }

        private void CheckGetTableNames(Connection connection, SchemaName schema, IEnumerable<string> expectedNames)
        {
            TestUtil.AssertEqualSets(expectedNames.Select(s => new TableName(schema, s)), connection.Catalog.GetTableNames(schema));
        }

        [Test]
        public void TestQueryMethods()
        {
            string dbPath = Path.Combine(TestEnv.DbDir, "testdb.hyper");
            string dbName = Path.GetFileNameWithoutExtension(dbPath);

            var fooDef = new TableDefinition("foo")
                .AddColumn("x", SqlType.Text())
                .AddColumn("y", SqlType.Text(), "en_US_CI", Nullability.NotNullable);

            var barDef = new TableDefinition(new TableName("bar", "bar"))
                .AddColumn("x", SqlType.Text())
                .AddColumn("y", SqlType.Text(), "en_US_CI", Nullability.NotNullable);

            using (Connection connection = new Connection(Server.Endpoint, dbPath, CreateMode.CreateAndReplace))
            {
                connection.Catalog.CreateTable(fooDef);
                connection.ExecuteCommand("CREATE SCHEMA bar");
                connection.Catalog.CreateTable(barDef);
            }

            using (Connection connection = new Connection(Server.Endpoint, dbPath))
            {
                Assert.IsTrue(connection.Catalog.HasTable("foo"));
                Assert.IsTrue(connection.Catalog.HasTable(new TableName("public", "foo")));
                Assert.IsFalse(connection.Catalog.HasTable("bar"));
                Assert.IsTrue(connection.Catalog.HasTable(new TableName("bar", "bar")));
                Assert.IsFalse(connection.Catalog.HasTable(new TableName("bar", "blah")));
                Assert.IsFalse(connection.Catalog.HasTable(new TableName("blah", "bar")));
                Assert.IsTrue(connection.Catalog.HasTable(new TableName(dbName, "bar", "bar")));

                fooDef.TableName = new TableName(dbName, "public", "foo");
                AssertEqualTableDefinitions(fooDef, connection.Catalog.GetTableDefinition(new TableName("foo")));
                barDef.TableName = new TableName(dbName, "bar", "bar");
                AssertEqualTableDefinitions(barDef, connection.Catalog.GetTableDefinition(new TableName("bar", "bar")));
                AssertEqualTableDefinitions(barDef, connection.Catalog.GetTableDefinition(new TableName(dbName, "bar", "bar")));
                Assert.Throws<HyperException>(() => connection.Catalog.GetTableDefinition(new TableName("blah")));

                CheckGetSchemaNames(connection, null, new[] { "public", "bar" });
                CheckGetSchemaNames(connection, dbName, new[] { "public", "bar" });

                CheckGetTableNames(connection, "public", new[] { "foo" });
                CheckGetTableNames(connection, "bar", new[] { "bar" });
                CheckGetTableNames(connection, new SchemaName(dbName, "bar"), new[] { "bar" });
                CheckGetTableNames(connection, "blah", new string[0]);
            }
        }

        [Test]
        public void TestCreateTable()
        {
            using (Connection connection = new Connection(Server.Endpoint, "blah.hyper", CreateMode.CreateAndReplace))
            {
                var oneColumn = new TableDefinition("foo")
                    .AddColumn("x", SqlType.Text());

                var twoColumns = new TableDefinition("foo")
                    .AddColumn("x", SqlType.Text())
                    .AddColumn("y", SqlType.Text(), "en_US_CI", Nullability.NotNullable);

                connection.Catalog.CreateTable(oneColumn);
                Assert.Throws<HyperException>(() => connection.ExecuteCommand("SELECT y FROM foo"));

                connection.Catalog.CreateTableIfNotExists(twoColumns);
                Assert.Throws<HyperException>(() => connection.ExecuteCommand("SELECT y FROM foo"));

                connection.ExecuteCommand("DROP TABLE foo");

                connection.Catalog.CreateTableIfNotExists(twoColumns);
                connection.ExecuteCommand("SELECT y FROM foo");
            }
        }

        [Test]
        public void TestCreateSchema()
        {
            using (Connection connection = new Connection(Server.Endpoint, "db1.hyper", CreateMode.CreateAndReplace))
            {
                // Create and attach a db2.hyper database
                string dbPath = Path.Combine(TestEnv.DbDir, "db2.hyper");
                connection.Catalog.CreateDatabase(dbPath);
                connection.Catalog.AttachDatabase(dbPath, "db2");

                connection.ExecuteCommand("SET schema_search_path=db1");
                Assert.IsFalse(connection.Catalog.GetSchemaNames("db1").Contains(new SchemaName("db1", "foo")));
                Assert.IsFalse(connection.Catalog.GetSchemaNames("db2").Contains(new SchemaName("db2", "foo")));
                Assert.IsFalse(connection.Catalog.GetSchemaNames("db1").Contains(new SchemaName("db1", "bar\"☃")));
                Assert.IsFalse(connection.Catalog.GetSchemaNames("db2").Contains(new SchemaName("db2", "bar\"☃")));

                connection.Catalog.CreateSchema("foo");
                Assert.IsTrue(connection.Catalog.GetSchemaNames("db1").Contains(new SchemaName("db1", "foo")));
                Assert.IsFalse(connection.Catalog.GetSchemaNames("db2").Contains(new SchemaName("db2", "foo")));

                connection.Catalog.CreateSchemaIfNotExists("foo");
                Assert.Throws<HyperException>(() => connection.Catalog.CreateSchema("foo"));
                Assert.IsTrue(connection.Catalog.GetSchemaNames("db1").Contains(new SchemaName("db1", "foo")));
                Assert.IsFalse(connection.Catalog.GetSchemaNames("db2").Contains(new SchemaName("db2", "foo")));

                connection.Catalog.CreateSchema(new SchemaName("db2", "bar\"☃"));
                Assert.IsFalse(connection.Catalog.GetSchemaNames("db1").Contains(new SchemaName("db1", "bar\"☃")));
                Assert.IsTrue(connection.Catalog.GetSchemaNames("db2").Contains(new SchemaName("db2", "bar\"☃")));
            }
        }

        // Not a test, just a piece of code to make sure it compiles. When changing it, copy-paste it
        // to the docs.
        public void TestExample1()
        {
            using (var hyper = new HyperProcess(Telemetry.SendUsageDataToTableau, "myapp"))
            {
                using (var connection = new Connection(hyper.Endpoint, "mydb.hyper", CreateMode.Create))
                {
                    connection.Catalog.CreateSchema("Extract");

                    var tableSchema = new TableDefinition(new TableName("Extract", "Extract"))
                        .AddColumn("a", SqlType.Text(), "en_us_ci", Nullability.NotNullable)
                        .AddColumn("b", SqlType.Bool());

                    connection.Catalog.CreateTable(tableSchema);

                    using (var inserter = new Inserter(connection, tableSchema))
                    {
                        inserter.Add("x");
                        inserter.Add(true);
                        inserter.EndRow();
                        inserter.Add("y");
                        inserter.AddNull();
                        inserter.EndRow();
                        inserter.Execute();
                    }
                }
            }
        }

        public void TestExample2()
        {
            HyperProcess hyper = null;

            /////////////////////////////////////////

            using (var connection = new Connection(hyper.Endpoint, "mydb.hyper"))
            {
                var tableName = new TableName("Extract", "Extract");
                using (var inserter = new Inserter(connection, tableName))
                {
                    inserter.Add("z");
                    inserter.Add(false);
                    inserter.EndRow();
                    inserter.Execute();
                }
            }

            /////////////////////////////////////////

            using (var connection = new Connection(hyper.Endpoint, "mydb.hyper"))
            {
                var tableName = new TableName("Extract", "Extract");

                using (Result result = connection.ExecuteQuery($"SELECT * FROM {tableName}"))
                {
                    while (result.NextRow())
                    {
                        string v1 = result.GetString(0);
                        bool? v2 = result.IsNull(1) ? null : (bool?)result.GetBool(1);
                        // ...
                    }
                }

                int max = connection.ExecuteScalarQuery<int>($"SELECT MAX(b) FROM {tableName}");

                // Delete the table
                connection.ExecuteCommand($"DROP TABLE {tableName}");
            }
        }
    }
}
