using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace Tableau.HyperAPI.Test
{
    [TestFixture]
    class ConnectionTests : HyperTest
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
        public void TestConnectToNonExistingDatabase()
        {
            Assert.Throws<HyperException>(() => new Connection(Server.Endpoint, "/no/such/database.hyper"));
        }

        [Test]
        public void TestCreateMode()
        {
            string dbPath = Path.Combine(TestEnv.DbDir, "x.hyper");
            if (File.Exists(dbPath))
                File.Delete(dbPath);

            Assert.Throws<HyperException>(() => new Connection(Server.Endpoint, dbPath));
            Assert.Throws<HyperException>(() => new Connection(Server.Endpoint, dbPath, CreateMode.None));
            Assert.IsFalse(File.Exists(dbPath));

            using (Connection connection = new Connection(Server.Endpoint, dbPath, CreateMode.CreateIfNotExists))
            {
                Assert.IsTrue(File.Exists(dbPath));
                connection.ExecuteCommand("CREATE SCHEMA foo");
            }

            using (Connection connection = new Connection(Server.Endpoint, dbPath, CreateMode.CreateIfNotExists))
            {
                TestUtil.AssertEqualSets(new[] { "public", "foo" }.Select(s => new SchemaName(s)), connection.Catalog.GetSchemaNames());
            }

            using (Connection connection = new Connection(Server.Endpoint, dbPath, CreateMode.CreateAndReplace))
            {
                List<SchemaName> schemaNames = new List<SchemaName>(connection.Catalog.GetSchemaNames());
                List<SchemaName> expected = new[] { "public" }.Select(s => new SchemaName(s)).ToList();
                Assert.AreEqual(expected, schemaNames);
            }

            Assert.Throws<HyperException>(() => new Connection(Server.Endpoint, dbPath, CreateMode.Create));

            File.Delete(dbPath);
            using (new Connection(Server.Endpoint, dbPath, CreateMode.Create))
            {
            }
            Assert.IsTrue(File.Exists(dbPath));

            File.Delete(dbPath);
            using (new Connection(Server.Endpoint, dbPath, CreateMode.CreateAndReplace))
            {
            }
            Assert.IsTrue(File.Exists(dbPath));

            File.Delete(dbPath);
        }

        // Spec says alias is supposed to be the file path's stem, but it's not. Bug?
        [Test]
        public void TestConnectedDatabaseAlias()
        {
            string dbPath = Path.Combine(TestEnv.DbDir, "blah.hyper");
            string dbName = Path.GetFileNameWithoutExtension(dbPath);
            using (var connection = new Connection(Server.Endpoint, dbPath, CreateMode.CreateAndReplace))
            {
                connection.ExecuteCommand($"CREATE TABLE {new TableName(dbName, "public", "foo")} ()");
                connection.ExecuteCommand($"SELECT * FROM {new TableName(dbName, "public", "foo")}");
            }
        }

        private void CheckBusy(Action action)
        {
            TestUtil.AssertThrowsWithContextId(action, 0xe9bec0b1);
        }

        private void CheckBusy(Connection connection)
        {
            CheckBusy(() => connection.Catalog.CreateTable(GetSampleSchema("bar")));
            CheckBusy(() => connection.Catalog.GetTableDefinition("foo"));
            CheckBusy(() => connection.Catalog.GetTableNames("public"));
            CheckBusy(() => connection.Catalog.GetSchemaNames());
            CheckBusy(() => new Inserter(connection, "foo"));
            CheckBusy(() => connection.ExecuteQuery("SELECT * FROM foo"));
            CheckBusy(() => connection.ExecuteCommand("DROP TABLE foo"));

            string noSuchDb = Path.Combine(TestEnv.DbDir, "nosuchdb.hyper");
            if (File.Exists(noSuchDb))
                File.Delete(noSuchDb);
            CheckBusy(() => connection.Catalog.CreateDatabase(Path.Combine(TestEnv.DbDir, noSuchDb)));
        }

        [Test]
        public void TestBusyConnection()
        {
            string dbPath = Path.Combine(TestEnv.DbDir, "testdb.hyper");

            using (Connection connection = new Connection(Server.Endpoint, dbPath, CreateMode.CreateAndReplace))
            {
                CreateSampleTable(connection, "foo");

                using (Result result = connection.ExecuteQuery("SELECT * FROM foo"))
                {
                    CheckBusy(connection);

                    // Reading all rows frees the connection
                    while (result.NextRow())
                    {
                    }

                    connection.ExecuteCommand("SELECT * FROM foo");
                }

                using (Inserter inserter = new Inserter(connection, "foo"))
                {
                    CheckBusy(connection);

                    // Execute() frees the connection
                    inserter.Execute();

                    connection.ExecuteCommand("SELECT * FROM foo");
                }

                connection.ExecuteCommand("SELECT * FROM foo");
            }

            using (Connection connection = new Connection(Server.Endpoint))
                connection.Catalog.DropDatabase(dbPath);
        }

        [Test]
        public void ConnectCompatibility()
        {
            String versionToConnect = "0.0";
            using (Connection connection = new Connection(Server.Endpoint, new Dictionary<String, String>() { ["hyper_service_version"] = versionToConnect}))
            {
                Assert.AreEqual(connection.HyperServiceVersion().ToString(), versionToConnect);
            }
        }

        [Test]
        public void ConnectCompatibilityUnsupported()
        {
            Assert.Throws<HyperException>(() => new Connection(Server.Endpoint, new Dictionary<String, String>() { ["hyper_service_version"] = "200.3"} ));
        }

        [Test]
        public void testCompatibilityActiveCapabilityFlag() {
            var dict = new Dictionary<String, String>() { ["capability_flag_for_compatibility"] = "1"};
            TestEnv.StartServer(dict);

            using (Connection connection = new Connection(Server.Endpoint, new Dictionary<String, String>() { ["capability_flag_for_compatibility"] = "on"}))
            {
                Assert.AreEqual(connection.IsCapabilityActive("capability_flag_for_compatibility"), true);
            }
        }

        [Test]
        public void testCompatibilityInactiveCapabilityFlag() {
            var dict = new Dictionary<String, String>() { ["capability_flag_for_compatibility"] = "1"};
            TestEnv.StartServer(dict);

            using (Connection connection = new Connection(Server.Endpoint))
            {
                Assert.AreEqual(connection.IsCapabilityActive("capability_flag_for_compatibility"), false);
            }
        }

        [Test]
        public void TestCompatibilityVersionRange()
        {
            var result = Connection.QuerySupportedHyperServiceVersionRange(Server.Endpoint);
            List<string> testVersions = new List<String>() { "0.0" };
            Assert.AreEqual(testVersions.Count, result.Count);
            for(int i = 0; i < testVersions.Count; ++i) {
               Assert.AreEqual(testVersions[i], result[i].ToString());
            }
        }

        [Test]
        public void TestCompatibilityVersionRangeError()
        {
            Assert.Throws<HyperException>(() => Connection.QuerySupportedHyperServiceVersionRange(new Endpoint("tab.tcp://127.0.0.1:124", "UserAgentHAPITest")));
        }
    }
}
