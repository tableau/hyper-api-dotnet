using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Tableau.HyperAPI.Test
{
    [Flags]
    public enum TestFlags
    {
        None = 0,
        StartServer = 1,
        CreateDatabase = 2,
    }

    public static class TestConstants
    {
        public const string DEFAULT_USER_AGENT = "dotnet-test";
    }

    public class TestHyperEnvironment : IDisposable
    {
        public string TempDir { get; }
        public string DbDir { get; }
        public string LogDir { get; }
        public string HyperDir { get; }

        public HyperProcess Server { get; private set; }

        public string Database { get; }

        public TestHyperEnvironment(TestFlags flags = TestFlags.None, string userAgent = TestConstants.DEFAULT_USER_AGENT)
        {
            TempDir = Path.Combine(Path.GetTempPath(), "HyperAPITest", Path.GetRandomFileName());
            DbDir = Path.Combine(TempDir, "db");
            LogDir = Path.Combine(TempDir, "log");
            Directory.CreateDirectory(DbDir);
            Directory.CreateDirectory(LogDir);

            var parameters = new Dictionary<string, string>
            {
                { "database_directory", DbDir },
                { "restrict_database_directory", "f" },
                { "log_dir", LogDir },
                { "default_database_version", "3" },
            };

            if (flags.HasFlag(TestFlags.StartServer) || flags.HasFlag(TestFlags.CreateDatabase))
                Server = new HyperProcess(HyperDir, Telemetry.SendUsageDataToTableau, userAgent, parameters);

            if (flags.HasFlag(TestFlags.CreateDatabase))
            {
                Database = Path.Combine(DbDir, "testdb.hyper");
                using (new Connection(Server.Endpoint, Database, CreateMode.Create))
                {
                }
            }
        }

        public void StartServer(Dictionary<string, string> parameters)
        {
            if (Server != null)
            {
                Server.Close();
            }

            parameters["database_directory"] = DbDir;
            parameters["restrict_database_directory"] = "f";
            parameters["log_dir"] = LogDir;
            parameters["default_database_version"] = "3";

            Server = new HyperProcess(HyperDir, Telemetry.SendUsageDataToTableau, TestConstants.DEFAULT_USER_AGENT, parameters);
        }

        public static void DeleteWithRetries(string path)
        {
            int maxAttempts = 10;
            for (int i = 0; i < maxAttempts; ++i)
            {
                try
                {
                    var attrs = File.GetAttributes(path);
                    if ((attrs & FileAttributes.Directory) != 0)
                        Directory.Delete(path, true);
                    else
                        File.Delete(path);
                    return;
                }
                catch (FileNotFoundException)
                {
                    return;
                }
                catch (DirectoryNotFoundException)
                {
                    return;
                }
                catch
                {
                    if (i + 1 == maxAttempts)
                        throw;
                    System.Threading.Thread.Sleep(100);
                }
            }
        }

        public void Dispose()
        {
            if (Server != null)
            {
                Server.Close();
                Server = null;
            }

            DeleteWithRetries(TempDir);
        }
    }
}
