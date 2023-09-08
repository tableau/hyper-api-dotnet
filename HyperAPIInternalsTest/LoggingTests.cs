using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

namespace Tableau.HyperAPI.Test
{
    [TestFixture]
    class LoggingTests : HyperTest
    {
        private static ILogger previousLogger;
        private static LogLevel previousLogLevel;
        private static bool wroteSomething;

        [OneTimeSetUp]
        public void SetUp()
        {
            previousLogLevel = Logging.SetLogLevel(LogLevel.Trace);
            previousLogger = Logging.SetLogger(new Logger());
            SetUpTest(TestFlags.CreateDatabase);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            TearDownTest();
            Logging.SetLogger(previousLogger);
            Logging.SetLogLevel(previousLogLevel);
        }

        [Test]
        public void Test()
        {
            wroteSomething = false;

            using (var connection = new Connection(Server.Endpoint, Database))
            {
                var schema = new TableDefinition("foo").AddColumn("x", SqlType.Text());
                connection.Catalog.CreateTable(schema);
                using (var inserter = new Inserter(connection, schema))
                {
                    inserter.AddRow(new[] { "a" });
                    inserter.AddRow(new[] { "b" });
                    inserter.AddRow(new[] { "c" });
                    inserter.Execute();
                }

                connection.ExecuteCommand("SELECT * FROM foo");
            }

            Assert.IsTrue(wroteSomething);
        }

        private class Logger : ILogger
        {
            public void LogEvent(LogLevel level, string topic, string jsonMessage)
            {
                Console.Write(topic);
                Console.Write(": ");
                Console.WriteLine(jsonMessage);
                wroteSomething = true;
            }
        }
    }
}
