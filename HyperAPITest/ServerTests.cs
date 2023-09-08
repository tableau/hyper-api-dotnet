using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

namespace Tableau.HyperAPI.Test
{
    [TestFixture]
    public class ServerTests : HyperTest
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            SetUpTest();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            TearDownTest();
        }

        private Dictionary<string, string> StartupParams => new Dictionary<string, string>
                {
                    { "database_directory", TestEnv.DbDir },
                    { "restrict_database_directory", "f" },
                    { "log_dir", TestEnv.DbDir },
                };

        // TODO: once it can start hyperd without specifying path to it, test it

        [Test]
        public void TestInvalidHyperPath()
        {
            Assert.Throws<HyperException>(() => new HyperProcess(@"X:\no\such\path", Telemetry.SendUsageDataToTableau, "dotnet-test", StartupParams));
        }

        [Test]
        public void TestStartAndStop()
        {
            var server = new HyperProcess(TestEnv.HyperDir, Telemetry.SendUsageDataToTableau, "dotnet-test", StartupParams);
            Endpoint endpoint = server.Endpoint;

            using (Connection connection = new Connection(endpoint, "mydb.hyper", CreateMode.CreateIfNotExists))
            {
                string version = connection.ExecuteScalarQuery<string>("SHOW server_version");
                Assert.IsTrue(!string.IsNullOrEmpty(version));
            }

            server.Shutdown();

            // Server should be stopped at this point and we should not be able to connect anymore
            Assert.Throws<HyperException>(() => new Connection(endpoint, "mydb.hyper", CreateMode.CreateIfNotExists));
            Assert.Throws<ObjectDisposedException>(() => string.IsNullOrEmpty(server.Endpoint.ConnectionDescriptor));
        }

        [Test]
        public void TestUsing()
        {
            Endpoint endpoint;

            using (HyperProcess server = new HyperProcess(TestEnv.HyperDir, Telemetry.SendUsageDataToTableau, "dotnet-test", StartupParams))
            {
                endpoint = server.Endpoint;
                using (Connection connection = new Connection(endpoint, "mydb.hyper", CreateMode.CreateIfNotExists))
                {
                    string version = connection.ExecuteScalarQuery<string>("SHOW server_version");
                    Assert.IsTrue(!string.IsNullOrEmpty(version));
                }
            }

            // Server should be stopped at this point and we should not be able to connect anymore
            Assert.Throws<HyperException>(() => new Connection(endpoint, "mydb.hyper", CreateMode.CreateIfNotExists));
        }
    }
}
