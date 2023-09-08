using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;

using Tableau.HyperAPI;
using Tableau.HyperAPI.Test;

namespace Benchmark
{
    class Options
    {
        public bool discardInHyper;
        public bool quiet;
    }

    class Program
    {
        const int mb1 = 1024 * 1024;
        const int mb10 = 10 * mb1;
        const int mb100 = 100 * mb1;
        const int gb1 = 1024 * mb1;

        class SingleColumnBenchmark
        {
            public Column Column;
            public string Name;
        }

        static void BenchmarkSingleColumn(HyperProcess instance, Options options, int size, int[] intColumnWidths, List<BenchmarkResult> results)
        {
            string sizeSuffix = $" {size / mb1}Mb";

            List<SingleColumnBenchmark> benchmarks = new List<SingleColumnBenchmark>(new[]
            {
                new SingleColumnBenchmark{ Column = new IntervalColumn(), Name = "Interval" },
                new SingleColumnBenchmark{ Column = new TimeColumn(), Name = "Time" },
                new SingleColumnBenchmark{ Column = new DateColumn(), Name = "Date" },
                new SingleColumnBenchmark{ Column = new BooleanColumn(), Name = "Boolean" },
                new SingleColumnBenchmark{ Column = new ShortColumn(), Name = "Short" },
                new SingleColumnBenchmark{ Column = new LongColumn(), Name = "Long" },
                new SingleColumnBenchmark{ Column = new StringColumn(0, 20), Name = "String(0, 20)" },
                new SingleColumnBenchmark{ Column = new StringColumn(100, 100), Name = "String(100, 100)" },
                new SingleColumnBenchmark{ Column = new StringColumn(0, 10000), Name = "String(0, 10000)" },
                new SingleColumnBenchmark{ Column = new DoubleColumn(), Name = "Double" },
                new SingleColumnBenchmark{ Column = new TimestampColumn(), Name = "Timestamp" },
            });

            foreach (int w in intColumnWidths)
                benchmarks.Add(new SingleColumnBenchmark { Column = new IntColumn(w), Name = $"Int({w})" });

            foreach (SingleColumnBenchmark b in benchmarks)
            {
                results.Add(new Benchmark(new Column[] { b.Column }, b.Name + sizeSuffix, size).Run(instance, options));
            }
        }

        static void BenchmarkMixed(HyperProcess instance, Options options, int[] sizes, List<BenchmarkResult> results)
        {
            Column[] columns = new Column[]
            {
                new IntervalColumn(),
                new TimeColumn(),
                new DateColumn(),
                new BooleanColumn(),
                new ShortColumn(),
                new IntColumn(1),
                new LongColumn(),
                new DoubleColumn(),
                new StringColumn(0, 20),
                new StringColumn(100, 100),
                new StringColumn(0, 10000),
            };
            foreach (int size in sizes)
            {
                results.Add(new Benchmark(columns, $"Mixed {size / mb1}Mb", size).Run(instance, options));
            }
        }

        static void RunBenchmarks(bool quick, bool quiet, bool mixedOnly, bool singleOnly)
        {
            int singleColumnSize;
            int[] mixedSizes;
            int[] intColumnWidths;
            if (quick)
            {
                mixedSizes = new[] { mb1, mb10 };
                singleColumnSize = mb1;
                intColumnWidths = new[] { 1, 10 };
            }
            else
            {
                mixedSizes = new[] { mb1, mb100, gb1 };
                singleColumnSize = mb100;
                intColumnWidths = new[] { 1, 10, 100, 1000, 10000 };
            }

            List<BenchmarkResult> results = new List<BenchmarkResult>();

            using (TestHyperEnvironment testEnv = new TestHyperEnvironment(TestFlags.StartServer, "benchmark"))
            {
                HyperProcess instance = testEnv.Server;
                Options options = new Options { discardInHyper = false, quiet = quiet };
                if (!singleOnly)
                    BenchmarkMixed(instance, options, mixedSizes, results);
                if (!mixedOnly)
                    BenchmarkSingleColumn(instance, options, singleColumnSize, intColumnWidths, results);
            }

            if (!quiet)
            {
                DateTime now = DateTime.Now;
                Console.WriteLine(now.ToString());
                Console.WriteLine(BenchmarkResult.GetCSVHeader());
                foreach (BenchmarkResult result in results)
                    Console.WriteLine(result.GetCSV(now));
            }
        }

        static void Main(string[] args)
        {
            CommandLineApplication app = new CommandLineApplication();
            CommandOption quick = app.Option("--quick", "Do it quickly.", CommandOptionType.NoValue);
            CommandOption quiet = app.Option("--quiet", "Do it quietly.", CommandOptionType.NoValue);
            CommandOption mixedOnly = app.Option("--mixed-only", "Run only Mixed benchmark.", CommandOptionType.NoValue);
            CommandOption singleOnly = app.Option("--single-only", "Run only Single-column benchmark.", CommandOptionType.NoValue);

            app.HelpOption("-? | -h | --help");
            app.OnExecute(() =>
            {
                RunBenchmarks(quick.HasValue(), quiet.HasValue(), mixedOnly.HasValue(), singleOnly.HasValue());
                return 0;
            });

            app.Execute(args);
        }
    }
}
