using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace Tableau.HyperAPI.Test
{
    [TestFixture]
    public class DataTypeTests : HyperTest
    {
        private Connection connection;

        [OneTimeSetUp]
        public void SetUp()
        {
            SetUpTest(TestFlags.CreateDatabase);
            ResetConnection();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }
            TearDownTest();
        }

        private void ResetConnection()
        {
            if (connection != null)
                connection.Close();
            connection = new Connection(Server.Endpoint, Database);
            connection.ExecuteCommand("SET timezone TO 'PST8PDT'");
        }

        class TestQuery
        {
            public string Query;
            public object[][] Result;
        }

        class TestData
        {
            public TableDefinition Schema;
            public object[][] Rows;
            public object[][] RowsRead;
            public TestQuery[] Queries;
        }

        private void InsertValue(Inserter inserter, object value, TableDefinition.Column column)
        {
            if (value == null)
            {
                Assert.IsTrue(column.Nullability == Nullability.Nullable);
                inserter.AddNull();
                return;
            }

            switch (column.Type.Tag)
            {
                case TypeTag.BigInt:
                    inserter.Add((long)value);
                    break;
                case TypeTag.Bool:
                    inserter.Add((bool)value);
                    break;
                case TypeTag.Bytes:
                    inserter.Add((byte[])value);
                    break;
                case TypeTag.Date:
                    inserter.Add((Date)value);
                    break;
                case TypeTag.Double:
                    inserter.Add((double)value);
                    break;
                case TypeTag.Int:
                    inserter.Add((int)value);
                    break;
                case TypeTag.Char:
                    inserter.Add((string)value);
                    break;
                case TypeTag.Json:
                    inserter.Add((string)value);
                    break;
                case TypeTag.Text:
                    inserter.Add((string)value);
                    break;
                case TypeTag.Varchar:
                    inserter.Add((string)value);
                    break;
                case TypeTag.SmallInt:
                    inserter.Add((short)value);
                    break;
                case TypeTag.Time:
                    inserter.Add((TimeSpan)value);
                    break;
                case TypeTag.Timestamp:
                case TypeTag.TimestampTZ:
                    inserter.Add((Timestamp)value);
                    break;
                case TypeTag.Interval:
                    inserter.Add((Interval)value);
                    break;
                case TypeTag.Numeric:
                    inserter.Add((decimal)value);
                    break;
                case TypeTag.Geography:
                    inserter.Add((byte[])value);
                    break;
                default:
                    Assert.Fail($"unsupported data type {column.Type.Tag}");
                    break;
            }
        }

        private void InsertRow(Inserter inserter, object[] row, TableDefinition schema)
        {
            int i = 0;
            foreach (var column in schema.Columns)
            {
                InsertValue(inserter, row[i], column);
                ++i;
            }
            inserter.EndRow();
            Assert.AreEqual(i, row.Length);
        }

        private object ReadValue(Result result, int idx, ISchemaColumn column)
        {
            if (column.Nullability == Nullability.Nullable)
            {
                if (result.IsNull(idx))
                    return null;
            }
            else
            {
                Assert.IsFalse(result.IsNull(idx));
            }

            unsafe
            {
                byte[] raw = result.GetRaw(idx);
                byte* p = result.GetRawUnsafe(idx, out ulong size);
                Assert.AreEqual(raw.Length, size);
                byte[] copy = new byte[size];
                Marshal.Copy((IntPtr)p, copy, 0, (int)size);
                Assert.AreEqual(raw, copy);
            }

            switch (column.Type.Tag)
            {
                case TypeTag.BigInt:
                    {
                        long v1 = result.GetLong(idx);
                        long v2 = (long)result.GetValue(idx);
                        Assert.AreEqual(v1, v2);
                        Assert.AreEqual(BitConverter.GetBytes(v1), result.GetRaw(idx));
                        return v1;
                    }

                case TypeTag.Bool:
                    {
                        bool v1 = result.GetBool(idx);
                        bool v2 = (bool)result.GetValue(idx);
                        Assert.AreEqual(v1, v2);
                        Assert.AreEqual(BitConverter.GetBytes(v1), result.GetRaw(idx));
                        return v1;
                    }

                case TypeTag.Bytes:
                case TypeTag.Geography:
                    {
                        byte[] v1 = result.GetBytes(idx);
                        byte[] v2 = (byte[])result.GetValue(idx);
                        Assert.AreEqual(v1, v2);
                        Assert.AreEqual(v1, result.GetRaw(idx));
                        return v1;
                    }

                case TypeTag.Date:
                    {
                        Date v1 = result.GetDate(idx);
                        Date v2 = (Date)result.GetValue(idx);
                        Assert.AreEqual(v1, v2);
                        return v1;
                    }

                case TypeTag.Double:
                    {
                        double v1 = result.GetDouble(idx);
                        double v2 = (double)result.GetValue(idx);
                        Assert.AreEqual(v1, v2);
                        Assert.AreEqual(BitConverter.GetBytes(v1), result.GetRaw(idx));
                        return v1;
                    }

                case TypeTag.Int:
                    {
                        int v1 = result.GetInt(idx);
                        int v2 = (int)result.GetValue(idx);
                        Assert.AreEqual(v1, v2);
                        Assert.AreEqual(BitConverter.GetBytes(v1), result.GetRaw(idx));
                        return v1;
                    }

                case TypeTag.Interval:
                    {
                        Interval v1 = result.GetInterval(idx);
                        Interval v2 = (Interval)result.GetValue(idx);
                        Assert.AreEqual(v1, v2);
                        return v1;
                    }

                case TypeTag.Char:
                case TypeTag.Json:
                case TypeTag.Text:
                case TypeTag.Varchar:
                    {
                        string v1 = result.GetString(idx);
                        string v2 = (string)result.GetValue(idx);
                        Assert.AreEqual(v1, v2);
                        return v1;
                    }

                case TypeTag.SmallInt:
                    {
                        short v1 = result.GetShort(idx);
                        short v2 = (short)result.GetValue(idx);
                        Assert.AreEqual(v1, v2);
                        Assert.AreEqual(BitConverter.GetBytes(v1), result.GetRaw(idx));
                        return v1;
                    }

                case TypeTag.Time:
                    {
                        TimeSpan v1 = result.GetTime(idx);
                        TimeSpan v2 = (TimeSpan)result.GetValue(idx);
                        Assert.AreEqual(v1, v2);
                        return v1;
                    }

                case TypeTag.Timestamp:
                case TypeTag.TimestampTZ:
                    {
                        Timestamp v1 = result.GetTimestamp(idx);
                        Timestamp v2 = (Timestamp)result.GetValue(idx);
                        Assert.AreEqual(v1, v2);
                        return v1;
                    }

                case TypeTag.Numeric:
                    {
                        decimal v1 = result.GetDecimal(idx);
                        decimal v2 = (decimal)result.GetValue(idx);
                        Assert.AreEqual(v1, v2);
                        return v1;
                    }

                case TypeTag.Unsupported:
                    {
                        byte[] v1 = result.GetRaw(idx);
                        byte[] v2 = (byte[])result.GetValue(idx);
                        Assert.AreEqual(v1, v2);
                        return v1;
                    }

                default:
                    throw new NotImplementedException($"unsupported data type {column.Type.Tag}");
            }
        }

        private object[] ReadRow(Result rowset, Schema schema)
        {
            object[] row = new object[schema.Columns.Count];
            for (int i = 0; i < schema.Columns.Count; ++i)
                row[i] = ReadValue(rowset, i, schema.Columns[i]);
            return row;
        }

        private List<object[]> ExecuteQuery(Connection connection, string query, TableDefinition schema)
        {
            List<object[]> result = new List<object[]>();
            using (Result rowset = connection.ExecuteQuery(query))
            {
                while (rowset.NextRow())
                    result.Add(ReadRow(rowset, new Schema(schema)));
            }
            return result;
        }

        private List<object[]> ExecuteQuery(Connection connection, string query, out ResultSchema querySchema)
        {
            List<object[]> result = new List<object[]>();
            using (Result rowset = connection.ExecuteQuery(query))
            {
                querySchema = rowset.Schema;
                while (rowset.NextRow())
                    result.Add(ReadRow(rowset, new Schema(querySchema)));
            }
            return result;
        }

        private List<object[]> ExecuteQuery(Connection connection, string query)
        {
            return ExecuteQuery(connection, query, out ResultSchema querySchema);
        }

        private void RunTest(TestData data, bool forceBulkInsert)
        {
            connection.ExecuteCommand($"DROP TABLE IF EXISTS {data.Schema.TableName}");

            CreateTable(connection, data.Schema, inserter =>
            {
                inserter.InternalFB = forceBulkInsert;

                foreach (object[] row in data.Rows)
                    InsertRow(inserter, row, data.Schema);
            });

            TableDefinition roundTripped = connection.Catalog.GetTableDefinition(data.Schema.TableName);
            TableDefinition dataSchema = data.Schema.Clone();
            dataSchema.TableName = new TableName(Path.GetFileNameWithoutExtension(Database), "public", data.Schema.TableName.Name);
            AssertEqualTableDefinitions(dataSchema, roundTripped);

            List<object[]> rows = ExecuteQuery(connection, $"SELECT * FROM {data.Schema.TableName}", data.Schema);

            DataValidator.CheckData(data.RowsRead ?? data.Rows, rows, new Schema(data.Schema));

            if (data.Queries != null)
            {
                foreach (TestQuery tq in data.Queries)
                {
                    ResultSchema querySchema;
                    rows = ExecuteQuery(connection, tq.Query, out querySchema);
                    DataValidator.CheckData(tq.Result, rows, new Schema(querySchema));
                }
            }
        }

        private void RunTest(TestData data)
        {
            RunTest(data, false);
            RunTest(data, true);
        }

        [Test]
        public void StringTest()
        {
            RunTest(new TestData
            {
                Schema = new TableDefinition("foo")
                    .AddColumn("text", SqlType.Text(), Nullability.NotNullable)
                    .AddColumn("textn", SqlType.Text())
                    ,
                Rows = new[]
                {
                    new object[]{ "", "" },
                    new object[]{ "a", "b" },
                    new object[]{ "abcdefgh", "ijklmno" },
                    new object[]{ "abc", null },
                    new object[]{ "€𐐷𤭢¢ह𐍈", "€𐐷𤭢" },
                }
            });
        }

        [Test]
        public void CollationTest()
        {
            RunTest(new TestData
            {
                Schema = new TableDefinition("foo")
                    .AddColumn("ci", SqlType.Text(), "en_US_CI")
                    .AddColumn("cs", SqlType.Text())
                    .AddColumn("cich", SqlType.Char(2), "en_US_CI")
                    .AddColumn("csch", SqlType.Char(2))
                    ,
                Rows = new[]
                {
                    new object[]{ "A", "A", "A", "A" },
                    new object[]{ "a", "a", "a", "a" },
                },
                RowsRead = new[]
                {
                    new object[]{ "A", "A", "A ", "A " },
                    new object[]{ "a", "a", "a ", "a " },
                },
                Queries = new[]
                {
                    new TestQuery
                    {
                        Query = "SELECT DISTINCT ci FROM foo",
                        Result = new[]
                        {
                            new object[]{ "a" },
                        },
                    },
                    new TestQuery
                    {
                        Query = "SELECT DISTINCT cs FROM foo",
                        Result = new[]
                        {
                            new object[]{ "A" },
                            new object[]{ "a" },
                        },
                    },
                    new TestQuery
                    {
                        Query = "SELECT DISTINCT cich FROM foo",
                        Result = new[]
                        {
                            new object[]{ "a " },
                        },
                    },
                    new TestQuery
                    {
                        Query = "SELECT DISTINCT csch FROM foo",
                        Result = new[]
                        {
                            new object[]{ "A " },
                            new object[]{ "a " },
                        },
                    },
                }
            });
        }

        [Test]
        public void CharTest()
        {
            RunTest(new TestData
            {
                Schema = new TableDefinition("foo")
                    .AddColumn("c3n", SqlType.Char(3))
                    .AddColumn("c3", SqlType.Char(3), Nullability.NotNullable)
                    ,
                Rows = new[]
                {
                    new object[]{ "", "" },
                    new object[]{ null, "a" },
                    new object[]{ "ab", "a" },
                    new object[]{ "abc", "abc"},
                    //new object[]{ "abcd", "abcd" },
                },
                RowsRead = new[]
                {
                    new object[]{ "   ", "   " },
                    new object[]{ null, "a  " },
                    new object[]{ "ab ", "a  " },
                    new object[]{ "abc", "abc" },
                    //new object[]{ "abc", "abc" },
                },
            });
        }

        [Test]
        public void Char1Test()
        {
            RunTest(new TestData
            {
                Schema = new TableDefinition("foo")
                    .AddColumn("c", SqlType.Char(1), Nullability.NotNullable)
                    .AddColumn("cn", SqlType.Char(1))
                    ,
                Rows = new[]
                {
                    // TFSID 922866: space is broken
                    //new object[]{ "", " " },
                    new object[]{ "\u0002", null },
                    new object[]{ "A", "Ø" },
                    new object[]{ "∀", "𐌰" },
                    new object[]{ "💡", "🤖" },
                },
                RowsRead = new[]
                {
                    // TFSID 922866: space is broken
                    //new object[]{ " ", " " },
                    new object[]{ "\u0002", null },
                    new object[]{ "A", "Ø" },
                    new object[]{ "∀", "𐌰" },
                    new object[]{ "💡", "🤖" },
                },
                Queries = new[]
                {
                    new TestQuery
                    {
                        Query = "SELECT c::text, cn::text FROM foo",
                        Result = new[]
                        {
                            //new object[]{ " ", " " },
                            new object[]{ "\u0002", null },
                            new object[]{ "A", "Ø" },
                            new object[]{ "∀", "𐌰" },
                            new object[]{ "💡", "🤖" },
                        },
                    },
                }
            });
        }

        [Test]
        public void BoolTest()
        {
            RunTest(new TestData
            {
                Schema = new TableDefinition("foo")
                    .AddColumn("bool", SqlType.Bool(), Nullability.NotNullable)
                    .AddColumn("booln", SqlType.Bool())
                    ,
                Rows = new[]
                {
                    new object[]{ true, true },
                    new object[]{ false, false },
                    new object[]{ true, null },
                    new object[]{ true, false },
                    new object[]{ false, null },
                },
                Queries = new[]
                {
                    new TestQuery
                    {
                        Query = "select \"bool\"::text as a, booln::text as b from foo",
                        Result = new[]
                        {
                            new object[]{ "t", "t" },
                            new object[]{ "f", "f" },
                            new object[]{ "t", null },
                            new object[]{ "t", "f" },
                            new object[]{ "f", null },
                        }
                    },
                },
            });
        }

        [Test]
        public void ShortTest()
        {
            RunTest(new TestData
            {
                Schema = new TableDefinition("foo")
                    .AddColumn("short", SqlType.SmallInt(), Nullability.NotNullable)
                    .AddColumn("shortn", SqlType.SmallInt())
                    ,
                Rows = new[]
                {
                    new object[]{ (short)0, (short)1 },
                    new object[]{ (short)2, (short)3 },
                    new object[]{ short.MinValue, null },
                    new object[]{ short.MaxValue, short.MinValue },
                },
                Queries = new[]
                {
                    new TestQuery
                    {
                        Query = "select \"short\"::text as a, shortn::text as b from foo",
                        Result = new[]
                        {
                            new object[]{ "0", "1" },
                            new object[]{ "2", "3" },
                            new object[]{ short.MinValue.ToString(), null },
                            new object[]{ short.MaxValue.ToString(), short.MinValue.ToString() },
                        }
                    },
                },
            });
        }

        [Test]
        public void IntTest()
        {
            RunTest(new TestData
            {
                Schema = new TableDefinition("foo")
                    .AddColumn("intn", SqlType.Int())
                    .AddColumn("int", SqlType.Int(), Nullability.NotNullable)
                    ,
                Rows = new[]
                {
                    new object[]{ 4, 5 },
                    new object[]{ 6, 7 },
                    new object[]{ null, int.MinValue },
                    new object[]{ int.MinValue, int.MaxValue },
                },
                Queries = new[]
                {
                    new TestQuery
                    {
                        Query = "select \"intn\"::text as a, \"int\"::text as b from foo",
                        Result = new[]
                        {
                            new object[]{ "4", "5" },
                            new object[]{ "6", "7" },
                            new object[]{ null, int.MinValue.ToString() },
                            new object[]{ int.MinValue.ToString(), int.MaxValue.ToString() },
                        }
                    },
                },
            });
        }

        [Test]
        public void LongTest()
        {
            RunTest(new TestData
            {
                Schema = new TableDefinition("foo")
                    .AddColumn("longn", SqlType.BigInt())
                    .AddColumn("long", SqlType.BigInt(), Nullability.NotNullable)
                    ,
                Rows = new[]
                {
                    new object[]{ 4L, 5L },
                    new object[]{ 6L, 7L },
                    new object[]{ null, long.MinValue },
                    new object[]{ long.MinValue, long.MaxValue },
                },
                Queries = new[]
                {
                    new TestQuery
                    {
                        Query = "select \"longn\"::text as a, \"long\"::text as b from foo",
                        Result = new[]
                        {
                            new object[]{ "4", "5" },
                            new object[]{ "6", "7" },
                            new object[]{ null, long.MinValue.ToString() },
                            new object[]{ long.MinValue.ToString(), long.MaxValue.ToString() },
                        }
                    },
                },
            });
        }

        [Test]
        public void DoubleTest()
        {
            RunTest(new TestData
            {
                Schema = new TableDefinition("foo")
                    .AddColumn("doublen", SqlType.Double())
                    .AddColumn("double", SqlType.Double(), Nullability.NotNullable)
                    ,
                Rows = new[]
                {
                    new object[]{ 4.0, 5.0 },
                    new object[]{ 6.0, 7.0 },
                    new object[]{ 1.2345678901234567890, 3.1415927 },
                    new object[]{ null, double.MinValue },
                    new object[]{ double.MinValue, double.MaxValue },
                    new object[]{ double.NaN, double.NegativeInfinity },
                    new object[]{ double.PositiveInfinity, double.NegativeInfinity },
                },
                Queries = new[]
                {
                    new TestQuery
                    {
                        Query = "select \"doublen\"::text as a, \"double\"::text as b from foo",
                        Result = new[]
                        {
                            new object[]{ "4", "5" },
                            new object[]{ "6", "7" },
                            new object[]{ "1.2345678901234567", "3.1415926999999999" },
                            new object[]{ null, "-1.7976931348623157e+308" },
                            new object[]{ "-1.7976931348623157e+308", "1.7976931348623157e+308" },
                            new object[]{ double.NaN.ToString(), "-Infinity" },
                            new object[]{ "Infinity", "-Infinity" },
                        }
                    },
                },
            });
        }

        [Test]
        public void NumericTest()
        {
            RunTest(new TestData
            {
                Schema = new TableDefinition("foo")
                    .AddColumn("numericn", SqlType.Numeric(18, 3))
                    .AddColumn("numeric", SqlType.Numeric(18, 3), Nullability.NotNullable)
                    .AddColumn("_6_3", SqlType.Numeric(6, 3), Nullability.NotNullable)
                    ,
                Rows = new[]
                {
                    // TFSID 931724: figure out the limits
                    new object[]{ 0.001M, -0.001M, -999.999M },
                    new object[]{ null, 7.0M, 999.999M },
                    new object[]{ 1.234M, 3.142M, 123.456M },
                    new object[]{ 999_999_999_999_999M, -999_999_999_999_999M, 0.001M },
                    new object[]{ 999_999_999_999_999.999M, -999_999_999_999_999.999M, -0.001M },
                },
                Queries = new[]
                {
                    new TestQuery
                    {
                        Query = "select \"numericn\"::text as a, \"numeric\"::text, \"_6_3\"::text as c from foo",
                        Result = new[]
                        {
                            new object[]{ "0.001", "-0.001", "-999.999" },
                            new object[]{ null, "7.000", "999.999" },
                            new object[]{ "1.234", "3.142", "123.456" },
                            new object[]{ "999999999999999.000", "-999999999999999.000", "0.001" },
                            new object[]{ "999999999999999.999", "-999999999999999.999", "-0.001" },
                        }
                    },
                },
            });
        }

        [Test]
        public void DateTimeTest()
        {
            // microsecond resolution
            long maxTicks = TimeSpan.TicksPerDay - (TimeSpan.TicksPerMillisecond / 1000);

            RunTest(new TestData
            {
                Schema = new TableDefinition("foo")
                    .AddColumn("daten", SqlType.Date())
                    .AddColumn("date", SqlType.Date(), Nullability.NotNullable)
                    .AddColumn("timen", SqlType.Time())
                    .AddColumn("time", SqlType.Time(), Nullability.NotNullable)
                    ,
                Rows = new[]
                {
                    new object[]{ new Date(-4712, 1, 1), new Date(294276, 12, 31), new TimeSpan(0), new TimeSpan(maxTicks) },
                    new object[]{ new Date(1, 1, 1), new Date(9999, 12, 31), new TimeSpan(0), new TimeSpan(maxTicks) },
                    new object[]{ new Date(1, 1, 1), new Date(9999, 12, 31), new TimeSpan(0), new TimeSpan(TimeSpan.TicksPerDay) },
                    new object[]{ null, new Date(2019, 5, 5), null, new TimeSpan(0, 1, 2, 3, 999) },
                },
                Queries = new[]
                {
                    new TestQuery
                    {
                        Query = "select \"daten\"::text, \"date\"::text, \"timen\"::text, \"time\"::text from foo",
                        Result = new[]
                        {
                            new object[]{ "4713-01-01 BC", "294276-12-31", "00:00:00", "23:59:59.999999" },
                            new object[]{ "0001-01-01", "9999-12-31", "00:00:00", "23:59:59.999999" },
                            new object[]{ "0001-01-01", "9999-12-31", "00:00:00", "24:00:00" },
                            new object[]{ null, "2019-05-05", null, "01:02:03.999" },
                        }
                    },
                },
            });
        }

        [Test]
        public void TimestampTest()
        {
            RunTest(new TestData
            {
                Schema = new TableDefinition("foo")
                    .AddColumn("timestampn", SqlType.Timestamp())
                    .AddColumn("timestamp", SqlType.Timestamp(), Nullability.NotNullable)
                    .AddColumn("timestamptzn", SqlType.TimestampTZ())
                    .AddColumn("timestamptz", SqlType.TimestampTZ(), Nullability.NotNullable)
                    ,
                Rows = new[]
                {
                    new object[]{ new Timestamp(-4712, 1, 1), new Timestamp(294276, 12, 31, 23, 59, 59, 999999),
                        new Timestamp(0, 0, 0, DateTimeKind.Utc), new Timestamp(0, 0, 0, 0, 0, 0, 1, DateTimeKind.Utc) },
                    new object[]{ new Timestamp(1, 1, 1), new Timestamp(9999, 12, 31, 23, 59, 59, 999999),
                        new Timestamp(2019, 3, 2, 1, 2, 3, 876, DateTimeKind.Utc), new Timestamp(2019, 5, 5, 13, 57, 3, 574, DateTimeKind.Utc) },
                    new object[]{ null, new Timestamp(9999, 12, 31, 23, 59, 59, 999999),
                        null, new Timestamp(2019, 5, 5, 13, 57, 3, 574, DateTimeKind.Utc) },
                },
                Queries = new[]
                {
                    new TestQuery
                    {
                        Query = "select \"timestampn\"::text, \"timestamp\"::text, \"timestamptzn\"::text, \"timestamptz\"::text from foo",
                        Result = new[]
                        {
                            new object[]{ "4713-01-01 00:00:00 BC", "294276-12-31 23:59:59.999999", "0002-11-29 16:00:00 BC-08", "0002-11-29 16:00:00.000001 BC-08" },
                            new object[]{ "0001-01-01 00:00:00", "9999-12-31 23:59:59.999999", "2019-03-01 17:02:03.000876-08", "2019-05-05 06:57:03.000574-07" },
                            new object[]{ null, "9999-12-31 23:59:59.999999", null, "2019-05-05 06:57:03.000574-07" },
                        }
                    },
                },
            });
        }

        [Test]
        public void IntervalTest()
        {
            RunTest(new TestData
            {
                Schema = new TableDefinition("foo")
                    .AddColumn("intervaln", SqlType.Interval())
                    .AddColumn("interval", SqlType.Interval(), Nullability.NotNullable)
                    ,
                Rows = new[]
                {
                    // TODO: figure out why this fails on linux: it returns 0 instead of -2147483648 for days
                    //new object[]{ Interval.FromYearsAndMonths(-178000000, -11) + Interval.FromDays(int.MinValue) + Interval.FromTime(-10000),
                    //    Interval.FromYearsAndMonths(178000000, 11) + Interval.FromDays(int.MaxValue) },
                    new object[]{ IntervalUtil.FromYearsAndMonths(-178000000, -11) + IntervalUtil.FromDays(-100000) + IntervalUtil.FromTime(-10000),
                        IntervalUtil.FromYearsAndMonths(178000000, 11) + IntervalUtil.FromDays(100000) },
                    new object[]{ IntervalUtil.FromYearsAndMonths(1, 2), IntervalUtil.FromDays(3) },
                    new object[]{ IntervalUtil.FromYearsAndMonths(1, 0) + IntervalUtil.FromDays(3), IntervalUtil.FromTime(1, 0, 0, 123456) },
                    new object[]{ new Interval(), IntervalUtil.FromTime(4, 5, 6, 789789) },
                    new object[]{ null, IntervalUtil.FromYearsAndMonths(1, 2) + IntervalUtil.FromDays(3) },
                    new object[]{ null, IntervalUtil.FromTime(-1, -2, -3, -456789) },
                    new object[]{ null, IntervalUtil.FromYearsAndMonths(1, 0) + IntervalUtil.FromTime(1, 0, 0, 0) },
                },
                Queries = new[]
                {
                    new TestQuery
                    {
                        Query = "select to_char(\"intervaln\", 'YYYY MM DD HH24 MI SS US'), to_char(\"interval\", 'YYYY MM DD HH24 MI SS US') from foo",
                        Result = new[]
                        {
                            new object[]{ "-178000000 -11 -100000 -10000 00 00 000000", "178000000 11 100000 00 00 00 000000" },
                            new object[]{ "0001 02 00 00 00 00 000000", "0000 00 03 00 00 00 000000" },
                            new object[]{ "0001 00 03 00 00 00 000000", "0000 00 00 01 00 00 123456" },
                            new object[]{ "0000 00 00 00 00 00 000000", "0000 00 00 04 05 06 789789" },
                            new object[]{ null, "0000 00 00 -1 -2 -3 -456789" },
                            new object[]{ null, "0001 02 03 00 00 00 000000" },
                            new object[]{ null, "0001 00 00 01 00 00 000000" },
                        }
                    },
                },
            });
        }

        private byte[] EncodeArray(int[] array, bool prependLength)
        {
            List<int> list = new List<int>(array.Length + 4);
            if (prependLength)
                list.Add(0);

            list.Add(1);
            list.Add(1);
            list.Add(array.Length);
            foreach (int x in array)
                list.Add(x);

            if (prependLength)
                list[0] = 4 * (list.Count - 1) + 1;

            Assert.IsTrue(array.Length <= 8);
            return list.SelectMany(x => BitConverter.GetBytes(x))
                // null flags - 1 byte per each 8 values
                .Append((byte)0)
                .ToArray();
        }

        [Test]
        public void UnsupportedTest()
        {
            try
            {
                connection.ExecuteCommand("DROP TABLE IF EXISTS foo");
                connection.ExecuteCommand("CREATE TEMP TABLE foo (arr INT ARRAY NOT NULL, arrn INT ARRAY)");

                TableDefinition schema = connection.Catalog.GetTableDefinition("foo");
                Assert.AreEqual(TypeTag.Unsupported, schema.GetColumn(0).Type.Tag);
                Assert.AreEqual(TypeTag.Unsupported, schema.GetColumn(1).Type.Tag);
                Assert.AreEqual(Nullability.NotNullable, schema.GetColumn(0).Nullability);
                Assert.AreEqual(Nullability.Nullable, schema.GetColumn(1).Nullability);

                connection.ExecuteCommand("INSERT INTO FOO VALUES (array[1,2], array[3,4]), (array[2,3], null)");

                object[][] intData = new[]
                {
                    new object[]{ new int[] { 1, 2 }, new int[] { 3, 4 } },
                    new object[]{ new int[] { 2, 3 }, null },
                };

                object[][] selectFirst = intData.Select(row => row.Select(x =>
                {
                    return x != null ? (object)((int[])x)[0] : null;
                }).ToArray()).ToArray();

                object[][] outputData = intData.Select(row => row.Select(x =>
                {
                    return x != null ? EncodeArray((int[])x, false) : null;
                }).ToArray()).ToArray();

                using (var result = connection.ExecuteQuery("SELECT * FROM foo"))
                {
                    int row = 0;

                    while (result.NextRow())
                    {
                        Assert.AreEqual(outputData[row], result.GetValues());

                        for (int col = 0; col < 2; ++col)
                        {
                            if (result.IsNull(col))
                                continue;

                            byte[] expectedValue = (byte[])outputData[row][col];

                            Assert.AreEqual(expectedValue, result.GetRaw(col));

                            unsafe
                            {
                                byte* ptr = result.GetRawUnsafe(col, out ulong size);
                                Assert.AreEqual(expectedValue.Length, size);
                                byte[] buf = new byte[size];
                                Marshal.Copy((IntPtr)ptr, buf, 0, (int)size);
                                Assert.AreEqual(expectedValue, buf);
                            }
                        }

                        row++;
                    }

                    Assert.AreEqual(2, row);
                }

                ResultSchema querySchema;
                var rows = ExecuteQuery(connection, "SELECT * FROM foo", out querySchema);
                DataValidator.CheckData(outputData, rows, new Schema(querySchema));

                rows = ExecuteQuery(connection, "SELECT arr[1], arrn[1] FROM foo", out querySchema);
                DataValidator.CheckData(selectFirst, rows, new Schema(querySchema));
            }
            finally
            {
                ResetConnection();
            }
        }
    }
}
