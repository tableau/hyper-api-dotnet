using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

using Tableau.HyperAPI.Impl;

namespace Tableau.HyperAPI.Test
{
    public class LoggerTests
    {
        private class LoggerImpl : ILogger
        {
            public List<Tuple<LogLevel, string, string>> events = new List<Tuple<LogLevel, string, string>>();

            public void LogEvent(LogLevel level, string topic, string jsonValue)
            {
                events.Add(new Tuple<LogLevel, string, string>(level, topic, jsonValue));
            }
        }

        [Test]
        public void Test()
        {
            LoggerImpl logger = new LoggerImpl();

            Assert.IsNull(Logging.SetLogger(logger));
            Assert.AreEqual(LogLevel.Info, Logging.SetLogLevel(LogLevel.Warning));

            Logger.Info("blah");
            Assert.IsTrue(logger.events.Count == 0);

            Logger.Warn("blah");
            Assert.AreEqual(new List<Tuple<LogLevel, string, string>> { new Tuple<LogLevel, string, string>(LogLevel.Warning, "hyper-api-dotnet", "{\"msg\":\"blah\"}") }, logger.events);

            Assert.AreEqual(LogLevel.Warning, Logging.SetLogLevel(LogLevel.Info));
            logger.events.Clear();
            Logger.Info("blah");
            Assert.AreEqual(new List<Tuple<LogLevel, string, string>> { new Tuple<LogLevel, string, string>(LogLevel.Info, "hyper-api-dotnet", "{\"msg\":\"blah\"}") }, logger.events);

            Assert.AreEqual(logger, Logging.SetLogger(null));

            Logger.Info("blah");
        }
    }
}
