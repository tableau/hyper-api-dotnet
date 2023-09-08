using System;
using System.IO;

using Microsoft.Extensions.CommandLineUtils;

using Tableau.HyperAPI;

namespace Benchmark
{
    internal class CSVReader : IDisposable
    {
        // Input stream from which to read lines
        StreamReader inputStream;
        // Delimiter of fields in a CSV row
        const Char delim = '|';

        public CSVReader(string csvdir, string filename)
        {
            string pathToCsv = Path.Join(csvdir, filename);
            inputStream = new StreamReader(pathToCsv);
        }

        public String[] Next()
        {
            if (inputStream.EndOfStream)
            {
                return null;
            }
            return inputStream.ReadLine().Split(delim);
        }

        public void Dispose()
        {
            inputStream.Dispose();
        }
    };

    public class Program
    {
        public static void CreateExtract(Connection connection)
        {
            string tableName = "lineitem";
            {
                TableDefinition schema = new TableDefinition(tableName);
                schema.AddColumn("orderkey", SqlType.Int());
                schema.AddColumn("partkey", SqlType.Int());
                schema.AddColumn("suppkey", SqlType.Int());
                schema.AddColumn("linenumber", SqlType.Int());
                schema.AddColumn("quantity", SqlType.Double());
                schema.AddColumn("extendedprice", SqlType.Double());
                schema.AddColumn("discount", SqlType.Double());
                schema.AddColumn("tax", SqlType.Double());
                schema.AddColumn("returnflag", SqlType.Text());
                schema.AddColumn("linestatus", SqlType.Text());
                schema.AddColumn("shipdate", SqlType.Date());
                schema.AddColumn("commitdate", SqlType.Date());
                schema.AddColumn("receiptdate", SqlType.Date());
                schema.AddColumn("shipinstruct", SqlType.Text());
                schema.AddColumn("shipmode", SqlType.Text());
                schema.AddColumn("comment", SqlType.Text());
                connection.Catalog.CreateTable(schema);
            }

            tableName = "part";
            {
                TableDefinition schema = new TableDefinition(tableName);
                schema.AddColumn("partkey", SqlType.Int());
                schema.AddColumn("name", SqlType.Text());
                schema.AddColumn("mfgr", SqlType.Text());
                schema.AddColumn("brand", SqlType.Text());
                schema.AddColumn("type", SqlType.Text());
                schema.AddColumn("size", SqlType.Int());
                schema.AddColumn("container", SqlType.Text());
                schema.AddColumn("retailprice", SqlType.Double());
                schema.AddColumn("comment", SqlType.Text());
                connection.Catalog.CreateTable(schema);
            }

            tableName = "supplier";
            {
                TableDefinition schema = new TableDefinition(tableName);
                schema.AddColumn("suppkey", SqlType.Int());
                schema.AddColumn("name", SqlType.Text());
                schema.AddColumn("address", SqlType.Text());
                schema.AddColumn("nationkey", SqlType.Int());
                schema.AddColumn("phone", SqlType.Text());
                schema.AddColumn("acctbal", SqlType.Double());
                schema.AddColumn("comment", SqlType.Text());
                connection.Catalog.CreateTable(schema);
            }

            tableName = "partsupp";
            {
                TableDefinition schema = new TableDefinition(tableName);
                schema.AddColumn("partkey", SqlType.Int());
                schema.AddColumn("suppkey", SqlType.Int());
                schema.AddColumn("availqty", SqlType.Int());
                schema.AddColumn("supplycost", SqlType.Double());
                schema.AddColumn("comment", SqlType.Text());
                connection.Catalog.CreateTable(schema);
            }

            tableName = "customer";
            {
                TableDefinition schema = new TableDefinition(tableName);
                schema.AddColumn("custkey", SqlType.Int());
                schema.AddColumn("name", SqlType.Text());
                schema.AddColumn("address", SqlType.Text());
                schema.AddColumn("nationkey", SqlType.Int());
                schema.AddColumn("phone", SqlType.Text());
                schema.AddColumn("acctbal", SqlType.Double());
                schema.AddColumn("mktsegment", SqlType.Text());
                schema.AddColumn("comment", SqlType.Text());
                connection.Catalog.CreateTable(schema);
            }

            tableName = "nation";
            {
                TableDefinition schema = new TableDefinition(tableName);
                schema.AddColumn("nationkey", SqlType.Int());
                schema.AddColumn("name", SqlType.Text());
                schema.AddColumn("regionkey", SqlType.Int());
                schema.AddColumn("comment", SqlType.Text());
                connection.Catalog.CreateTable(schema);
            }

            tableName = "region";
            {
                TableDefinition schema = new TableDefinition(tableName);
                schema.AddColumn("regionkey", SqlType.Int());
                schema.AddColumn("name", SqlType.Text());
                schema.AddColumn("comment", SqlType.Text());
                connection.Catalog.CreateTable(schema);
            }

            tableName = "orders";
            {
                TableDefinition schema = new TableDefinition(tableName);
                schema.AddColumn("orderkey", SqlType.Int());
                schema.AddColumn("custkey", SqlType.Int());
                schema.AddColumn("orderstatus", SqlType.Text());
                schema.AddColumn("totalprice", SqlType.Double());
                schema.AddColumn("orderdate", SqlType.Date());
                schema.AddColumn("orderpriority", SqlType.Text());
                schema.AddColumn("clerk", SqlType.Text());
                schema.AddColumn("shippriority", SqlType.Int());
                schema.AddColumn("comment", SqlType.Text());
                connection.Catalog.CreateTable(schema);
            }

        }

        public static void StoreDate(Inserter inserter, String[] line, int index)
        {
            String date = line[index];
            inserter.AddDate(new Date(Int32.Parse(date.Substring(0, 4)), Int32.Parse(date.Substring(5, 2)), Int32.Parse(date.Substring(8, 2))));
        }

        public static void PopulateExtract(Connection connection, string csvdir)
        {
            string tableName = "lineitem";
            {
                TableDefinition schema = connection.Catalog.GetTableDefinition(tableName);
                using (Inserter inserter = new Inserter(connection, schema))
                {
                    using (CSVReader csv = new CSVReader(csvdir, tableName + ".tbl"))
                    {
                        String[] line;
                        while ((line = csv.Next()) != null)
                        {
                            inserter.AddInt(Int32.Parse(line[0]));
                            inserter.AddInt(Int32.Parse(line[1]));
                            inserter.AddInt(Int32.Parse(line[2]));
                            inserter.AddInt(Int32.Parse(line[3]));
                            inserter.AddDouble(Double.Parse(line[4]));
                            inserter.AddDouble(Double.Parse(line[5]));
                            inserter.AddDouble(Double.Parse(line[6]));
                            inserter.AddDouble(Double.Parse(line[7]));
                            inserter.AddText(line[8]);
                            inserter.AddText(line[9]);
                            StoreDate(inserter, line, 10);
                            StoreDate(inserter, line, 11);
                            StoreDate(inserter, line, 12);
                            inserter.AddText(line[13]);
                            inserter.AddText(line[14]);
                            inserter.AddText(line[15]);
                            inserter.EndRow();
                        }
                    }
                    inserter.Execute();
                }
            }

            tableName = "part";
            {
                TableDefinition schema = connection.Catalog.GetTableDefinition(tableName);
                using (Inserter inserter = new Inserter(connection, schema))
                {
                    using (CSVReader csv = new CSVReader(csvdir, tableName + ".tbl"))
                    {
                        String[] line;
                        while ((line = csv.Next()) != null)
                        {
                            inserter.AddInt(Int32.Parse(line[0]));
                            inserter.AddText(line[1]);
                            inserter.AddText(line[2]);
                            inserter.AddText(line[3]);
                            inserter.AddText(line[4]);
                            inserter.AddInt(Int32.Parse(line[5]));
                            inserter.AddText(line[6]);
                            inserter.AddDouble(Double.Parse(line[7]));
                            inserter.AddText(line[8]);
                            inserter.EndRow();
                        }
                    }
                    inserter.Execute();
                }
            }

            tableName = "supplier";
            {
                TableDefinition schema = connection.Catalog.GetTableDefinition(tableName);
                using (Inserter inserter = new Inserter(connection, schema))
                {
                    using (CSVReader csv = new CSVReader(csvdir, tableName + ".tbl"))
                    {
                        String[] line;
                        while ((line = csv.Next()) != null)
                        {
                            inserter.AddInt(Int32.Parse(line[0]));
                            inserter.AddText(line[1]);
                            inserter.AddText(line[2]);
                            inserter.AddInt(Int32.Parse(line[3]));
                            inserter.AddText(line[4]);
                            inserter.AddDouble(Double.Parse(line[5]));
                            inserter.AddText(line[6]);
                            inserter.EndRow();
                        }
                    }
                    inserter.Execute();
                }
            }

            tableName = "partsupp";
            {
                TableDefinition schema = connection.Catalog.GetTableDefinition(tableName);
                using (Inserter inserter = new Inserter(connection, schema))
                {
                    using (CSVReader csv = new CSVReader(csvdir, tableName + ".tbl"))
                    {
                        String[] line;
                        while ((line = csv.Next()) != null)
                        {
                            inserter.AddInt(Int32.Parse(line[0]));
                            inserter.AddInt(Int32.Parse(line[1]));
                            inserter.AddInt(Int32.Parse(line[2]));
                            inserter.AddDouble(Double.Parse(line[3]));
                            inserter.AddText(line[4]);
                            inserter.EndRow();
                        }
                    }
                    inserter.Execute();
                }
            }

            tableName = "customer";
            {
                TableDefinition schema = connection.Catalog.GetTableDefinition(tableName);
                using (Inserter inserter = new Inserter(connection, schema))
                {
                    using (CSVReader csv = new CSVReader(csvdir, tableName + ".tbl"))
                    {
                        String[] line;
                        while ((line = csv.Next()) != null)
                        {
                            inserter.AddInt(Int32.Parse(line[0]));
                            inserter.AddText(line[1]);
                            inserter.AddText(line[2]);
                            inserter.AddInt(Int32.Parse(line[3]));
                            inserter.AddText(line[4]);
                            inserter.AddDouble(Double.Parse(line[5]));
                            inserter.AddText(line[6]);
                            inserter.AddText(line[7]);
                            inserter.EndRow();
                        }
                    }
                    inserter.Execute();
                }
            }

            tableName = "nation";
            {
                TableDefinition schema = connection.Catalog.GetTableDefinition(tableName);
                using (Inserter inserter = new Inserter(connection, schema))
                {
                    using (CSVReader csv = new CSVReader(csvdir, tableName + ".tbl"))
                    {
                        String[] line;
                        while ((line = csv.Next()) != null)
                        {
                            inserter.AddInt(Int32.Parse(line[0]));
                            inserter.AddText(line[1]);
                            inserter.AddInt(Int32.Parse(line[2]));
                            inserter.AddText(line[3]);
                            inserter.EndRow();
                        }
                    }
                    inserter.Execute();
                }
            }

            tableName = "region";
            {
                TableDefinition schema = connection.Catalog.GetTableDefinition(tableName);
                using (Inserter inserter = new Inserter(connection, schema))
                {
                    using (CSVReader csv = new CSVReader(csvdir, tableName + ".tbl"))
                    {
                        String[] line;
                        while ((line = csv.Next()) != null)
                        {
                            inserter.AddInt(Int32.Parse(line[0]));
                            inserter.AddText(line[1]);
                            inserter.AddText(line[2]);
                            inserter.EndRow();
                        }
                    }
                    inserter.Execute();
                }
            }

            tableName = "orders";
            {
                TableDefinition schema = connection.Catalog.GetTableDefinition(tableName);
                using (Inserter inserter = new Inserter(connection, schema))
                {
                    using (CSVReader csv = new CSVReader(csvdir, tableName + ".tbl"))
                    {
                        String[] line;
                        while ((line = csv.Next()) != null)
                        {
                            inserter.AddInt(Int32.Parse(line[0]));
                            inserter.AddInt(Int32.Parse(line[1]));
                            inserter.AddText(line[2]);
                            inserter.AddDouble(Double.Parse(line[3]));
                            StoreDate(inserter, line, 4); ;
                            inserter.AddText(line[5]);
                            inserter.AddText(line[6]);
                            inserter.AddInt(Int32.Parse(line[7]));
                            inserter.AddText(line[8]);
                            inserter.EndRow();
                        }
                    }
                    inserter.Execute();
                }
            }

        }

        private static void PrintException(Exception ex)
        {
            Console.WriteLine($"<{ex.GetType().Name}> {ex.Message}");
            if (ex is HyperException)
            {
                HyperException hex = (HyperException)ex;
                Console.WriteLine($"  Detail: {hex.Detail}");
                Console.WriteLine($"  Hint: {hex.Hint}");
                Console.WriteLine($"  Category: {hex.Category}");
                Console.WriteLine($"  Code: {hex.Code}");
                Console.WriteLine($"  SqlState: {hex.SqlState}");
            }
            if (ex.InnerException != null)
            {
                Console.Write("Caused by: ");
                PrintException(ex.InnerException);
            }
        }

        public static void Main(string[] args)
        {
            CommandLineApplication app = new CommandLineApplication
            {
                Name = "load-tpch",
                FullName = "Load TPC-H",
                Description = "HAPI Benchmark to load TPC-H data from CSV files",
            };
            app.HelpOption("-h | --help");

            var filenameOpt = app.Option("-f|--filename <FILENAME>", "FILENAME of the extract containing the imported TPC-H data.\nThis will be created or overwritten if it already exists.\n(default='tpch.hyper')", CommandOptionType.SingleValue);
            var csvdirOpt = app.Option("--csvdir <PATH>", "Path to directory containing CSV files generated by TPC-H's `dbgen` tool (*.tbl files) [REQUIRED].", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                string filename = filenameOpt.HasValue() ? filenameOpt.Value() : "tpch.hyper";
                if (!csvdirOpt.HasValue())
                {
                    throw new ArgumentException("missing option --csvdir");
                }
                string csvdir = csvdirOpt.Value();

                // Start the Hyper process with telemetry enabled and connect to new TPC-H database
                using (HyperProcess hyper = new HyperProcess(Telemetry.SendUsageDataToTableau))
                {
                    using (Connection connection = new Connection(hyper.Endpoint, filename, CreateMode.CreateAndReplace))
                    {
                        CreateExtract(connection);
                        PopulateExtract(connection, csvdir);
                    }
                }

                return 0;
            });

            try
            {
                app.Execute(args);
            }
            catch (Exception e)
            {
                PrintException(e);
            }
        }
    }
}
