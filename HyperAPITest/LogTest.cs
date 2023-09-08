using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using NUnit.Framework;

namespace Tableau.HyperAPI.Test
{
    public class LogTests
    {
        [Test]
        public void TestLogCreated()
        {
            var previousWorkingDir = Directory.GetCurrentDirectory();
            using (TestHyperEnvironment testEnv = new TestHyperEnvironment())
            {
                try
                {
                    // Change the working dir since tests should not do stuff in the default working directory
                    Directory.SetCurrentDirectory(testEnv.TempDir);

                    var hyperDir = testEnv.HyperDir;
                    var hyperdLogPath = Path.Combine(Directory.GetCurrentDirectory(), "hyperd.log");

                    if (File.Exists(hyperdLogPath))
                    {
                        File.Delete(hyperdLogPath);
                    }

                    Assert.IsFalse(File.Exists(hyperdLogPath));
                    using (var server = new HyperProcess(hyperDir, Telemetry.SendUsageDataToTableau, "")) { };
                    Assert.IsTrue(File.Exists(hyperdLogPath));
                }
                finally
                {
                    Directory.SetCurrentDirectory(previousWorkingDir);
                }
            }
        }

        [Test]
        public void TestChangeLogDir()
        {
            var previousWorkingDir = Directory.GetCurrentDirectory();
            using (TestHyperEnvironment testEnv = new TestHyperEnvironment())
            {
                try
                {
                    // Change the working dir since tests should not do stuff in the default working directory
                    Directory.SetCurrentDirectory(testEnv.TempDir);

                    var newLogDir = Path.Combine(testEnv.TempDir, "new_log_dir");
                    Directory.CreateDirectory(newLogDir);

                    var hyperDir = testEnv.HyperDir;
                    var hyperdLogPathDefault = Path.Combine(Directory.GetCurrentDirectory(), "hyperd.log");
                    var hyperdLogPathChanged = Path.Combine(newLogDir, "hyperd.log");

                    if (File.Exists(hyperdLogPathDefault))
                    {
                        File.Delete(hyperdLogPathDefault);
                    }

                    Assert.IsFalse(File.Exists(hyperdLogPathDefault));
                    Assert.IsFalse(File.Exists(hyperdLogPathChanged));

                    var parameters = new Dictionary<string, string>
                    {
                        { "log_dir", newLogDir},
                    };
                    using (var server = new HyperProcess(hyperDir, Telemetry.SendUsageDataToTableau, "", parameters)) { }

                    Assert.IsFalse(File.Exists(hyperdLogPathDefault));
                    Assert.IsTrue(File.Exists(hyperdLogPathChanged));
                }
                finally
                {
                    Directory.SetCurrentDirectory(previousWorkingDir);
                }
            }
        }
    }
}
