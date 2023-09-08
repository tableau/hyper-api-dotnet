using System;
using System.Linq;
using System.Diagnostics;

using Tableau.HyperAPI;

namespace Benchmark
{
    internal class BenchmarkResult
    {
        public string schema;
        public string language;
        public double sendTimeMs;
        public double readTimeMs;
        public long targetSize;

        public static string GetCSVHeader()
        {
            return "Schema,SendTimeMs,ReadTimeMs,SizeMb,type,ts";
        }

        public string GetCSV(DateTime ts)
        {
            return $"{schema},{sendTimeMs},{readTimeMs},{targetSize / (1024.0 * 1024.0)},{language},{ts}";
        }
    }

    internal class Benchmark
    {
        private readonly Column[] columns;
        private readonly long targetSize;
        private readonly string name;

        public Benchmark(Column[] columns, string name, long targetSize)
        {
            this.columns = columns;
            this.name = name;
            this.targetSize = targetSize;
        }

        private long CalculateRowSize()
        {
            long size = 0;
            foreach (Column col in columns)
                size += col.RowSize;
            return size;
        }

        private void GenerateRandomData(int rowCount, bool willRead)
        {
            foreach (Column col in columns)
                col.GenerateRandomData(rowCount, willRead);
        }

        private TableDefinition GetSchema(string tableName)
        {
            var schema = new TableDefinition(tableName);
            for (int i = 0; i < columns.Length; ++i)
            {
                string name = $"A{i}";
                columns[i].AddColumnDefinitions(schema, name);
            }
            return schema;
        }

        private void InsertRow(Inserter inserter, int row)
        {
            foreach (Column col in columns)
                col.InsertValue(inserter, row);
            inserter.EndRow();
        }

        private long ReadData(Result result)
        {
            int rowCount = 0;
            while (result.NextRow())
            {
                int tableColumn = 0;
                foreach (Column col in columns)
                {
                    col.ReadRow(result, tableColumn, rowCount);
                    tableColumn += col.TableColumnCount;
                }
                ++rowCount;
            }
            return rowCount;
        }

        private void ValidateData(int rowCount)
        {
            foreach (Column col in columns)
                col.ValidateData(rowCount);
        }

        private string GetColumnSchemaString()
        {
            return $"|{string.Join("|", columns.Select(col => col.Name))}|";
        }

        public BenchmarkResult Run(HyperProcess instance, Options options)
        {
            if (!options.quiet)
                Console.WriteLine(name);

            Stopwatch swBench = new Stopwatch();
            swBench.Start();

            BenchmarkResult benchmarkResult = new BenchmarkResult
            {
                language = options.discardInHyper ? "C#_discard" : "C#",
                targetSize = targetSize,
                schema = GetColumnSchemaString(),
            };

            using (Connection connection = new Connection(instance.Endpoint, "benchmark.hyper", CreateMode.CreateIfNotExists))
            {
                int rowCount = (int)(targetSize / CalculateRowSize());
                if (!options.quiet)
                    Console.WriteLine($"generating {rowCount} rows of random data...");
                GenerateRandomData(rowCount, !options.discardInHyper);

                connection.ExecuteCommand("DROP TABLE IF EXISTS foo");

                if (!options.quiet)
                    Console.WriteLine("sending...");
                Stopwatch sw = new Stopwatch();
                sw.Start();

                TableDefinition schema = GetSchema("foo");
                connection.Catalog.CreateTable(schema);
                using (Inserter inserter = new Inserter(connection, schema))
                {
                    for (int i = 0; i < rowCount; ++i)
                        InsertRow(inserter, i);

                    inserter.Execute();
                }

                sw.Stop();

                benchmarkResult.sendTimeMs = sw.ElapsedMilliseconds;
                double targetSizeMb = targetSize / (1024.0 * 1024.0);
                double writeTime = sw.ElapsedMilliseconds / 1000.0;
                double writeSpeed = targetSizeMb / writeTime;
                if (!options.quiet)
                    Console.WriteLine($"Sent {targetSizeMb}Mb in {writeTime}s ({writeSpeed:F3} Mb/s)");

                if (!options.discardInHyper)
                {
                    if (!options.quiet)
                        Console.WriteLine("reading...");
                    sw.Restart();
                    long readRows;
                    using (Result result = connection.ExecuteQuery("SELECT * FROM FOO"))
                        readRows = ReadData(result);
                    sw.Stop();

                    benchmarkResult.readTimeMs = sw.ElapsedMilliseconds;
                    double readTime = sw.ElapsedMilliseconds / 1000.0;
                    double readSpeed = targetSizeMb / readTime;
                    if (!options.quiet)
                        Console.WriteLine($"Read {targetSizeMb}Mb in {readTime}s ({readSpeed:F3} Mb/s)");

                    if (readRows != rowCount)
                        throw new Exception($"read unexpected number of rows {readRows}, expected {rowCount}");

                    if (!options.quiet)
                        Console.WriteLine("validating...");
                    ValidateData(rowCount);
                }
            }

            swBench.Stop();
            if (!options.quiet)
                Console.WriteLine($"{name}: {swBench.ElapsedMilliseconds / 1000.0}s");

            return benchmarkResult;
        }
    }
}
