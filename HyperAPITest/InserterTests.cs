using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using NUnit.Framework;

using Tableau.HyperAPI.Raw;

namespace Tableau.HyperAPI.Test
{
    [TestFixture]
    class InserterTests : HyperTest
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            SetUpTest(TestFlags.CreateDatabase);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            TearDownTest();
        }

        [Test]
        public void TestInserter()
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                connection.ExecuteCommand("DROP TABLE IF EXISTS foo");

                CreateTable(connection, GetSampleSchema("foo"), inserter =>
                {
                    inserter.Add(true).Add(1).Add("x").EndRow();
                    inserter.AddRow(new object[] { false, null, "y" });
                    inserter.AddRow(new object[] { true, 2, null });
                });

                Assert.AreEqual(3, connection.ExecuteScalarQuery<long>("SELECT COUNT(*) FROM foo"));

                using (Inserter inserter = new Inserter(connection, "foo"))
                {
                    inserter.AddRow(new object[] { false, 3, "z" });
                    inserter.Execute();
                }

                Assert.AreEqual(4, connection.ExecuteScalarQuery<long>("SELECT COUNT(*) FROM foo"));

                connection.ExecuteCommand("DROP TABLE foo");
            }
        }

        private void ValidateData(Result result, object[][] expected)
        {
            List<object[]> actual = new List<object[]>();
            while (result.NextRow())
                actual.Add(result.GetValues());

            Assert.AreEqual(expected.Length, actual.Count);
            for (int i = 0; i < expected.Length; ++i)
                Assert.AreEqual(expected[i], actual[i]);
        }

        private void TestAllDataTypes(bool forceBulkInsert)
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                connection.ExecuteCommand("DROP TABLE IF EXISTS foo");

                TableDefinition schema = GetAllDataTypesSchema("foo");

                CreateTable(connection, schema, inserter =>
                {
                    inserter.InternalFB = forceBulkInsert;

                    inserter.Add(true);
                    inserter.Add(true);
                    inserter.Add((short)1);
                    inserter.Add((short)2);
                    inserter.Add(3);
                    inserter.Add(4);
                    inserter.Add((long)5);
                    inserter.Add((long)6);
                    inserter.Add((double)7.0);
                    inserter.Add((double)8.0);
                    inserter.Add((uint)9);
                    inserter.Add((uint)10);
                    inserter.Add("");
                    inserter.Add("b");
                    inserter.Add(new byte[] { 0, 1, 2, 3, 4 });
                    inserter.Add(new byte[] { 5, 6, 7, 8, 9 });
                    inserter.Add(new TimeSpan(0, 1, 2, 3, 4));
                    inserter.Add(new TimeSpan(0, 3, 4, 5, 6));
                    inserter.Add(new Date(2019, 3, 1));
                    inserter.Add(new Date(2020, 5, 5));
                    inserter.Add(new Timestamp(2019, 3, 1, 1, 2, 3, 4));
                    inserter.Add(new Timestamp(2020, 5, 5, 2, 3, 4, 5));
                    inserter.Add(new Timestamp(2019, 3, 1, 1, 2, 3, 4, DateTimeKind.Utc));
                    inserter.Add(new Timestamp(2020, 5, 5, 2, 3, 4, 5, DateTimeKind.Utc));
                    inserter.Add(new Interval(1, 0, 0));
                    inserter.Add(new Interval(0, 1, 0));
                    inserter.Add("abc");
                    inserter.Add("");
                    inserter.Add("a");
                    inserter.Add("b");
                    inserter.Add("");
                    inserter.Add("xyz");
                    inserter.Add((decimal)1.23);
                    inserter.Add((decimal)3.14);
                    inserter.Add(Decimal.Parse("12.123456"));
                    inserter.Add(Decimal.Parse("-12.123456"));
                    inserter.Add("null");
                    inserter.Add("8");
                    inserter.Add(new byte[] { 3, 4, 5, 6, 7 });
                    inserter.Add(new byte[] { 8, 9, 10, 11, 12 });
                    inserter.EndRow();

                    inserter.Add(false);
                    inserter.AddNull();
                    inserter.Add((short)9);
                    inserter.AddNull();
                    inserter.Add(10);
                    inserter.AddNull();
                    inserter.Add((long)11);
                    inserter.AddNull();
                    inserter.Add((double)12.0);
                    inserter.AddNull();
                    inserter.Add((uint)13);
                    inserter.AddNull();
                    inserter.Add("c");
                    inserter.AddNull();
                    inserter.Add(new byte[] { 10, 11, 12, 13, 14 });
                    inserter.AddNull();
                    inserter.Add(new TimeSpan(0, 2, 3, 4, 5));
                    inserter.AddNull();
                    inserter.Add(new Date(2019, 5, 5));
                    inserter.AddNull();
                    inserter.Add(new Timestamp(2021, 2, 3, 4, 5, 6, 7));
                    inserter.AddNull();
                    inserter.Add(new Timestamp(2022, 3, 4, 5, 6, 7, 8, DateTimeKind.Utc));
                    inserter.AddNull();
                    inserter.Add(new Interval(0, 0, 1));
                    inserter.AddNull();
                    inserter.Add("def");
                    inserter.AddNull();
                    inserter.Add("c");
                    inserter.AddNull();
                    inserter.Add("xftgyh");
                    inserter.AddNull();
                    inserter.Add((decimal)1234567890123456.78);
                    inserter.AddNull();
                    inserter.Add(Decimal.Parse("0.12345678901234567890"));
                    inserter.AddNull();
                    inserter.Add("\"a\"");
                    inserter.AddNull();
                    inserter.Add(new byte[] { 4, 5, 6, 7, 8 });
                    inserter.AddNull();
                    inserter.EndRow();
                });

                Assert.AreEqual(2, connection.ExecuteScalarQuery<long>("SELECT COUNT(*) FROM foo"));

                TableDefinition roundtrippedSchema = connection.Catalog.GetTableDefinition("foo");
                roundtrippedSchema.TableName = schema.TableName;
                AssertEqualTableDefinitions(schema, roundtrippedSchema);

                object[][] data = new[] {
                    new object[]
                    {
                        true, true, (short)1, (short)2, 3, 4, (long)5, (long)6, 7.0, 8.0, (uint)9, (uint)10, "", "b",
                        new byte[] { 0, 1, 2, 3, 4 }, new byte[] { 5, 6, 7, 8, 9 },
                        new TimeSpan(0, 1, 2, 3, 4), new TimeSpan(0, 3, 4, 5, 6),
                        new Date(2019, 3, 1), new Date(2020, 5, 5),
                        new Timestamp(2019, 3, 1, 1, 2, 3, 4), new Timestamp(2020, 5, 5, 2, 3, 4, 5),
                        new Timestamp(2019, 3, 1, 1, 2, 3, 4, DateTimeKind.Utc), new Timestamp(2020, 5, 5, 2, 3, 4, 5, DateTimeKind.Utc),
                        new Interval(1, 0, 0), new Interval(0, 1, 0),
                        "abc   ", "      ",
                        "a", "b",
                        "", "xyz", (decimal)1.23, (decimal)3.14, Decimal.Parse("12.123456"), Decimal.Parse("-12.123456"),
                        "null", "8", new byte[] { 3, 4, 5, 6, 7 }, new byte[] { 8, 9, 10, 11, 12 },
                    },
                    new object[]
                    {
                        false, null, (short)9, null, 10, null, (long)11, null, 12.0, null, (uint)13, null, "c", null,
                        new byte[] { 10, 11, 12, 13, 14 }, null,
                        new TimeSpan(0, 2, 3, 4, 5), null,
                        new Date(2019, 5, 5), null,
                        new Timestamp(2021, 2, 3, 4, 5, 6, 7), null,
                        new Timestamp(2022, 3, 4, 5, 6, 7, 8, DateTimeKind.Utc), null,
                        new Interval(0, 0, 1), null,
                        "def   ", null,
                        "c", null,
                        "xftgyh", null, (decimal)1234567890123456.78, null, Decimal.Parse("0.12345678901234567890"), null,
                        "\"a\"", null, new byte[] { 4, 5, 6, 7, 8 }, null,
                    },
                };

                object[] extraRow = new object[]
                {
                    true, true, (short)15, (short)16, 17, 18, (long)19, (long)20, 21.0, 22.0, (uint)23, (uint)24, "x", "y",
                    new byte[] { 10, 11, 12, 13, 14 }, null,
                    new TimeSpan(0, 2, 3, 4, 5), null,
                    new Date(2019, 5, 5), null,
                    new Timestamp(2021, 2, 3, 4, 5, 6, 7), null,
                    new Timestamp(2022, 3, 4, 5, 6, 7, 8, DateTimeKind.Utc), new DateTimeOffset(2022, 3, 4, 5, 6, 7, 8, new TimeSpan()),
                    new Interval(1, 1, 1), null,
                    "efg", null,
                    "k", null,
                    "poiuyp", null, (decimal)0.15, null, Decimal.Parse("-0.12345678901234567890"), null,
                    "[1]", null, new byte[] { 6, 7, 8, 9, 10 }, null,
                };

                object[] extraRowResult = new object[]
                {
                    true, true, (short)15, (short)16, 17, 18, (long)19, (long)20, 21.0, 22.0, (uint)23, (uint)24, "x", "y",
                    new byte[] { 10, 11, 12, 13, 14 }, null,
                    new TimeSpan(0, 2, 3, 4, 5), null,
                    new Date(2019, 5, 5), null,
                    new Timestamp(2021, 2, 3, 4, 5, 6, 7), null,
                    new Timestamp(2022, 3, 4, 5, 6, 7, 8, DateTimeKind.Utc), new Timestamp(2022, 3, 4, 5, 6, 7, 8000, DateTimeKind.Utc),
                    new Interval(1, 1, 1), null,
                    "efg   ", null,
                    "k", null,
                    "poiuyp", null, (decimal)0.15, null, Decimal.Parse("-0.12345678901234567890"), null,
                    "[1]", null, new byte[] { 6, 7, 8, 9, 10 }, null,
                };

                using (Result result = connection.ExecuteQuery("SELECT * FROM foo"))
                {
                    ValidateData(result, data);
                }

                using (Inserter inserter = new Inserter(connection, "foo"))
                {
                    inserter.AddRow(extraRow);
                    inserter.Execute();
                }

                using (Result result = connection.ExecuteQuery("SELECT * FROM foo"))
                {
                    object[][] expected = (object[][])Array.CreateInstance(typeof(object[]), data.Length + 1);
                    Array.Copy(data, expected, data.Length);
                    expected[data.Length] = extraRowResult;
                    ValidateData(result, expected);
                }

                connection.ExecuteCommand("DROP TABLE foo");
            }
        }

        [Test]
        public void TestAllDataTypes()
        {
            TestAllDataTypes(false);
            TestAllDataTypes(true);
        }

        private void TestLenientCast(SqlType columnType, Func<Inserter, Inserter> callback)
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                connection.ExecuteCommand("DROP TABLE IF EXISTS \"lenient_casts\"");
                TableName tableName = new TableName("lenient_casts");
                TableDefinition schema = new TableDefinition(tableName);
                schema.AddColumn("test", columnType);
                connection.Catalog.CreateTable(schema);
                using (Inserter inserter = new Inserter(connection, schema))
                {
                    callback(inserter);
                    inserter.EndRow();
                    inserter.Execute();
                }
                using (Result result = connection.ExecuteQuery("SELECT count(*)::int FROM " + tableName))
                {
                    Assert.True(result.NextRow());
                    Assert.AreEqual(1, result.GetInt(0));
                }
                connection.ExecuteCommand("DROP TABLE \"lenient_casts\"");
            }
        }

        private void TestTypeMismatch<T>(SqlType columnType, Func<Inserter, Inserter> callback)
            where T : Exception
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                connection.ExecuteCommand("DROP TABLE IF EXISTS \"type_mismatch\"");
                TableDefinition schema = new TableDefinition("type_mismatch");
                schema.AddColumn("test", columnType);
                connection.Catalog.CreateTable(schema);
                using (Inserter inserter = new Inserter(connection, schema))
                {
                    Assert.Throws<T>(() => callback(inserter));
                }
                connection.ExecuteCommand("DROP TABLE \"type_mismatch\"");
            }
        }

        private void TestTypeMismatch(SqlType columnType, Func<Inserter, Inserter> callback)
        {
            TestTypeMismatch<ArgumentException>(columnType, callback);
        }

        /// Test lenient cast.
        [Test]
        public void TestLenientCastShortToInt()
        {
            TestLenientCast(SqlType.Int(), (inserter) => inserter.Add((short)123));
        }

        /// Test lenient cast.
        [Test]
        public void TestLenientCastShortToBigInt()
        {
            TestLenientCast(SqlType.BigInt(), (inserter) => inserter.Add((short)123));
        }

        /// Test lenient cast.
        [Test]
        public void TestLenientCastShortToNumeric()
        {
            TestLenientCast(SqlType.Numeric(18, 3), (inserter) => inserter.Add((short)123));
        }

        /// Test lenient cast.
        [Test]
        public void TestLenientCastShortToDouble()
        {
            TestLenientCast(SqlType.Double(), (inserter) => inserter.Add((short)123));
        }

        /// Test lenient cast.
        [Test]
        public void TestLenientCastIntToBigInt()
        {
            TestLenientCast(SqlType.BigInt(), (inserter) => inserter.Add(123456));
        }

        /// Test lenient cast.
        [Test]
        public void TestLenientCastIntToNumeric()
        {
            TestLenientCast(SqlType.Numeric(18, 3), (inserter) => inserter.Add(123456));
        }

        /// Test lenient cast.
        [Test]
        public void TestLenientCastIntToDouble()
        {
            TestLenientCast(SqlType.Double(), (inserter) => inserter.Add(123456));
        }

        /// Test lenient cast.
        [Test]
        public void TestLenientCastLongToBigInt()
        {
            TestLenientCast(SqlType.BigInt(), (inserter) => inserter.Add(123456L));
        }

        /// Test lenient cast.
        [Test]
        public void TestLenientCastLongToNumeric()
        {
            TestLenientCast(SqlType.Numeric(18, 3), (inserter) => inserter.Add(123456789L));
        }

        /// Test lenient cast.
        [Test]
        public void TestLenientCastLongToDouble()
        {
            TestLenientCast(SqlType.Double(), (inserter) => inserter.Add(123456789L));
        }

        /// Test lenient cast.
        [Test]
        public void TestLenientCastDecimalToDouble()
        {
            TestLenientCast(SqlType.Double(), (inserter) => inserter.Add((decimal)3.1415));
        }

        /// Test lenient cast
        [Test]
        public void TestLenientCastStringToVarchar()
        {
            TestLenientCast(SqlType.Varchar(20), (inserter) => inserter.Add("Lorem Ipsum"));
        }

        /// Test lenient cast.
        [Test]
        public void TestLenientCastStringToChar()
        {
            TestLenientCast(SqlType.Char(40), (inserter) => inserter.Add(" Ipsum"));
        }

        /// Test lenient cast.
        [Test]
        public void TestLenientCastStringToJson()
        {
            TestLenientCast(SqlType.Json(), (inserter) => inserter.Add("{\"key\": 42}"));
        }

        /// Test lenient cast
        [Test]
        public void TestLenientCastBytesToVarchar()
        {
            TestLenientCast(SqlType.Varchar(20), (inserter) => inserter.Add(Encoding.UTF8.GetBytes("Lorem Ipsum")));
        }

        /// Test lenient cast.
        [Test]
        public void TestLenientCastBytesToChar()
        {
            TestLenientCast(SqlType.Char(40), (inserter) => inserter.Add(Encoding.UTF8.GetBytes(" Ipsum")));
        }

        /// Test lenient cast.
        [Test]
        public void TestLenientCastBytesToJson()
        {
            TestLenientCast(SqlType.Json(), (inserter) => inserter.Add(Encoding.UTF8.GetBytes("{\"key\": 42}")));
        }

        /// Test type mismatch
        [Test]
        public void TestTypeMismatchIntToSmallInt()
        {
            TestTypeMismatch(SqlType.SmallInt(), (inserter) => inserter.Add(123));
        }

        /// Test type mismatch*/
        [Test]
        public void TestTypeMismatchIntToSmallInt2()
        {
            TestTypeMismatch(SqlType.SmallInt(), (inserter) => inserter.Add(123456));
        }

        /// Test type mismatch
        [Test]
        public void TestTypeMismatchLongToSmallInt()
        {
            TestTypeMismatch(SqlType.SmallInt(), (inserter) => inserter.Add(123L));
        }

        /// Test type mismatch
        [Test]
        public void TestTypeMismatchLongToSmallInt2()
        {
            TestTypeMismatch(SqlType.SmallInt(), (inserter) => inserter.Add(123456789L));
        }

        /// Test type mismatch
        [Test]
        public void TestTypeMismatchLongToInt()
        {
            TestTypeMismatch(SqlType.Int(), (inserter) => inserter.Add(123456789123L));
        }

        /// Test type mismatch
        [Test]
        public void TestTypeMismatchShortMaxValueBoundaryToSmallInt()
        {
            TestTypeMismatch(SqlType.SmallInt(), (inserter) => inserter.Add((int)short.MaxValue + 1));
        }

        /// Test type mismatch
        [Test]
        public void TestTypeMismatchShortMinValueBoundaryToSmallInt()
        {
            TestTypeMismatch(SqlType.SmallInt(), (inserter) => inserter.Add((int)short.MinValue - 1));
        }

        /// Test type mismatch
        [Test]
        public void TestTypeMismatchIntMaxValueBoundaryToInt()
        {
            TestTypeMismatch(SqlType.Int(), (inserter) => inserter.Add((long)int.MaxValue + 1));
        }

        /// Test type mismatch
        [Test]
        public void TestTypeMismatchIntMinValueBoundaryToInt()
        {
            TestTypeMismatch(SqlType.Int(), (inserter) => inserter.Add((long)int.MinValue - 1));
        }

        /// Test type mismatch
        [Test]
        public void TestTypeMismatchDecimalToSmallInt()
        {
            TestTypeMismatch(SqlType.SmallInt(), (inserter) => inserter.Add((decimal)3.1425));
        }

        /// Test type mismatch
        [Test]
        public void TestTypeMismatchDoubleToSmallInt()
        {
            TestTypeMismatch(SqlType.SmallInt(), (inserter) => inserter.Add(2.718281828));
        }

        /// Test type mismatch
        [Test]
        public void TestTypeMismatchDecimalToInt()
        {
            TestTypeMismatch(SqlType.Int(), (inserter) => inserter.Add((decimal)3.1425));
        }

        /// Test type mismatch
        [Test]
        public void TestTypeMismatchDoubleToInt()
        {
            TestTypeMismatch(SqlType.Int(), (inserter) => inserter.Add(2.718281828));
        }

        /// Test type mismatch
        [Test]
        public void TestTypeMismatchDoubleToBigInt()
        {
            TestTypeMismatch(SqlType.BigInt(), (inserter) => inserter.Add(2.718281828));
        }

        /// Test type mismatch
        [Test]
        public void TestTypeMismatchDoubleToNumeric()
        {
            TestTypeMismatch(SqlType.Numeric(18, 3), (inserter) => inserter.Add(2.718281828));
        }

        [Test]
        public void TestTypeMismatchDateTime()
        {
            TestTypeMismatch(SqlType.Date(), (inserter) => inserter.Add(DateTime.UtcNow));
            TestTypeMismatch(SqlType.Date(), (inserter) => inserter.Add(DateTimeOffset.UtcNow));
            TestTypeMismatch(SqlType.Time(), (inserter) => inserter.Add(DateTime.UtcNow));
            TestTypeMismatch(SqlType.Time(), (inserter) => inserter.Add(DateTimeOffset.UtcNow));
            TestTypeMismatch(SqlType.Timestamp(), (inserter) => inserter.Add(DateTimeOffset.UtcNow));

            TestTypeMismatch<InvalidCastException>(SqlType.Date(), (inserter) => inserter.Add((object)DateTime.UtcNow));
            TestTypeMismatch<InvalidCastException>(SqlType.Date(), (inserter) => inserter.Add((object)DateTimeOffset.UtcNow));
            TestTypeMismatch<InvalidCastException>(SqlType.Time(), (inserter) => inserter.Add((object)DateTime.UtcNow));
            TestTypeMismatch<InvalidCastException>(SqlType.Time(), (inserter) => inserter.Add((object)DateTimeOffset.UtcNow));
            TestTypeMismatch<InvalidCastException>(SqlType.Timestamp(), (inserter) => inserter.Add((object)DateTimeOffset.UtcNow));
        }

        [Test]
        public void TestCancel()
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                connection.ExecuteCommand("DROP TABLE IF EXISTS \"foo\"");

                var tableDefinition = new TableDefinition("foo")
                    .AddColumn("text", SqlType.Text(), Nullability.NotNullable)
                    ;

                connection.Catalog.CreateTable(tableDefinition);
                using (Inserter inserter = new Inserter(connection, "foo"))
                {
                    inserter.Add("a");
                    inserter.EndRow();
                    inserter.Add("b");
                    inserter.EndRow();
                    inserter.Add("c");
                    inserter.EndRow();
                    // Do not call Execute()
                }

                Assert.AreEqual(0, connection.ExecuteScalarQuery<long>("SELECT COUNT(*) FROM foo"));
                connection.ExecuteCommand("DROP TABLE \"foo\"");
            }
        }

        private void TestDifferentSchema(bool forceBulkInsert)
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                connection.ExecuteCommand("DROP TABLE IF EXISTS foo");

                var tableDefinition = new TableDefinition("foo")
                    .AddColumn("a", SqlType.Text())
                    .AddColumn("b", SqlType.Text(), Nullability.NotNullable)
                    ;

                connection.Catalog.CreateTable(tableDefinition);
                using (Inserter inserter = new Inserter(connection, "foo", new[] { "b" }))
                {
                    inserter.InternalFB = forceBulkInsert;

                    inserter.Add("a");
                    inserter.EndRow();
                    inserter.Add("b");
                    inserter.EndRow();
                    inserter.Add("c");
                    inserter.EndRow();
                    inserter.Execute();
                }

                using (Result result = connection.ExecuteQuery("SELECT * FROM foo"))
                {
                    ValidateData(result, new[]
                    {
                        new object[]{ null, "a" },
                        new object[]{ null, "b" },
                        new object[]{ null, "c" },
                    });
                }

                connection.ExecuteCommand("DROP TABLE foo");
            }
        }

        [Test]
        public void TestDifferentSchema()
        {
            TestDifferentSchema(false);
            TestDifferentSchema(true);
        }

        private void TestInserterSuccess(SqlType columnType, Nullability nullability, Action<Connection> beforeInsertionAction, Action<Inserter> inserterAction, Action<Connection> beforeResultAction, Action<Result> resultAction)
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                TableDefinition tableDef = new TableDefinition("foo").AddColumn("x", columnType, nullability);
                connection.ExecuteCommand($"DROP TABLE IF EXISTS {tableDef.TableName}");
                connection.Catalog.CreateTable(tableDef);

                beforeInsertionAction.Invoke(connection);
                using (Inserter inserter = new Inserter(connection, tableDef))
                {
                    inserterAction.Invoke(inserter);
                    inserter.Execute();
                }

                beforeResultAction.Invoke(connection);
                using (Result result = connection.ExecuteQuery($"SELECT * FROM {tableDef.TableName}"))
                {
                    resultAction.Invoke(result);
                    Assert.IsFalse(result.NextRow());
                }
            }
        }

        private void TestInserterActionSuccess(SqlType columnType, Nullability nullability, object[] expectedValues, Action<Inserter>[] actions)
        {
            Assert.AreEqual(expectedValues.Length, actions.Length);
            TestInserterSuccess(columnType, nullability, new Action<Connection> (connection => {}), new Action<Inserter> (inserter => {
                foreach (var action in actions)
                    {
                        action.Invoke(inserter);
                        inserter.EndRow();
                    }
                }), new Action<Connection> (connection => {}), new Action<Result> (result => {
                for (int i = 0; i < expectedValues.Length; ++i)
                    {
                        Assert.IsTrue(result.NextRow());
                        Assert.AreEqual(expectedValues[i], result.GetValue(0));
                    }
                }));
        }

        private void TestInserterActionFailure(SqlType columnType, Nullability nullability, Type expectedExceptionType, Action<Inserter> action)
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                TableDefinition tableDef = new TableDefinition("foo").AddColumn("x", columnType, nullability);
                connection.ExecuteCommand($"DROP TABLE IF EXISTS {tableDef.TableName}");
                connection.Catalog.CreateTable(tableDef);

                using (Inserter inserter = new Inserter(connection, tableDef))
                {
                    Assert.Throws(expectedExceptionType, () => action.Invoke(inserter));
                    Assert.IsFalse(inserter.IsOpen);
                }
            }
        }

        [Test]
        public void TestStringParams()
        {
            Nullability[] nullabilities = new[] { Nullability.Nullable, Nullability.NotNullable };

            SqlType[] textTypes = new[] { SqlType.Text(), SqlType.Json(), SqlType.Varchar(1), SqlType.Varchar(10) };
            foreach (SqlType type in textTypes)
            {
                foreach (Nullability nullability in nullabilities)
                {
                    TestInserterActionSuccess(type, nullability, new[] { "", "", "", "", "1", "1", "1", "1", "", "", "" },
                        new Action<Inserter>[] {
                            inserter => inserter.Add(""),
                            inserter => inserter.Add("", 0),
                            inserter => inserter.Add("", 0, -1),
                            inserter => inserter.Add("", 0, 0),
                            inserter => inserter.Add("1"),
                            inserter => inserter.Add("1", 0),
                            inserter => inserter.Add("1", 0, -1),
                            inserter => inserter.Add("1", 0, 1),
                            inserter => inserter.Add("x", 1),
                            inserter => inserter.Add("x", 1, -1),
                            inserter => inserter.Add("x", 1, 0),
                        });
                }
            }

            foreach (Nullability nullability in nullabilities)
            {
                TestInserterActionSuccess(SqlType.Char(1), nullability, new[] { " ", " ", " ", " ", "x", "x", "x", "x", " ", " ", " " },
                    new Action<Inserter>[] {
                            inserter => inserter.Add(""),
                            inserter => inserter.Add("", 0),
                            inserter => inserter.Add("", 0, -1),
                            inserter => inserter.Add("", 0, 0),
                            inserter => inserter.Add("x"),
                            inserter => inserter.Add("x", 0),
                            inserter => inserter.Add("x", 0, -1),
                            inserter => inserter.Add("x", 0, 1),
                            inserter => inserter.Add("x", 1),
                            inserter => inserter.Add("x", 1, -1),
                            inserter => inserter.Add("x", 1, 0),
                    });
            }

            foreach (Nullability nullability in nullabilities)
            {
                TestInserterActionSuccess(SqlType.Char(2), nullability, new[] { "  ", "  ", "  ", "  ", "x ", "x ", "x ", "x ", "  ", "  ", "  ", "xy" },
                    new Action<Inserter>[] {
                        inserter => inserter.Add(""),
                        inserter => inserter.Add("", 0),
                        inserter => inserter.Add("", 0, -1),
                        inserter => inserter.Add("", 0, 0),
                        inserter => inserter.Add("x"),
                        inserter => inserter.Add("x", 0),
                        inserter => inserter.Add("x", 0, -1),
                        inserter => inserter.Add("x", 0, 1),
                        inserter => inserter.Add("x", 1),
                        inserter => inserter.Add("x", 1, -1),
                        inserter => inserter.Add("x", 1, 0),
                        inserter => inserter.Add("xy"),
                    });
            }

            SqlType[] allTextTypes = new[] { SqlType.Text(), SqlType.Json(), SqlType.Varchar(1), SqlType.Varchar(10), SqlType.Char(1), SqlType.Char(2) };
            foreach (SqlType type in allTextTypes)
            {
                TestInserterActionFailure(type, Nullability.Nullable, typeof(ArgumentException), inserter => inserter.Add("", -1, 0));
                TestInserterActionFailure(type, Nullability.Nullable, typeof(ArgumentException), inserter => inserter.Add("", 0, 1));
                TestInserterActionFailure(type, Nullability.Nullable, typeof(ArgumentException), inserter => inserter.Add("", 1, 0));
                TestInserterActionFailure(type, Nullability.Nullable, typeof(ArgumentException), inserter => inserter.Add("x", 0, 2));
                TestInserterActionFailure(type, Nullability.Nullable, typeof(ArgumentException), inserter => inserter.Add("x", 1, 1));
                TestInserterActionFailure(type, Nullability.Nullable, typeof(ArgumentException), inserter => inserter.Add("x", 2, 0));
                TestInserterActionFailure(type, Nullability.Nullable, typeof(ArgumentException), inserter => inserter.Add((string)null));
            }
        }

        [Test]
        public void TestUtf8StringParams()
        {
            Nullability[] nullabilities = new[] { Nullability.Nullable, Nullability.NotNullable };

            byte[] utf8Empty = new byte[0];
            byte[] utf8One = new byte[] { (byte)'1' };
            byte[] utf8X = new byte[] { (byte)'x' };
            byte[] utf8XY = new byte[] { (byte)'x', (byte)'y' };

            string[] funnyStrings = new[] { "€", "𐐷", "𤭢", "¢", "ह", "𐍈", "💡", "🤖", "𐌰" };
            byte[][] funnyBytes = funnyStrings.Select(s => Encoding.UTF8.GetBytes(s)).ToArray();

            SqlType[] textTypes = new[] { SqlType.Text(), SqlType.Json(), SqlType.Varchar(1), SqlType.Varchar(10) };
            foreach (SqlType type in textTypes)
            {
                foreach (Nullability nullability in nullabilities)
                {
                    TestInserterActionSuccess(type, nullability,
                        new[] { "", "", "", "", "1", "1", "1", "1", "", "", "" },
                        new Action<Inserter>[] {
                            inserter => inserter.Add(utf8Empty),
                            inserter => inserter.Add(utf8Empty, 0),
                            inserter => inserter.Add(utf8Empty, 0, -1),
                            inserter => inserter.Add(utf8Empty, 0, 0),
                            inserter => inserter.Add(utf8One),
                            inserter => inserter.Add(utf8One, 0),
                            inserter => inserter.Add(utf8One, 0, -1),
                            inserter => inserter.Add(utf8One, 0, 1),
                            inserter => inserter.Add(utf8X, 1),
                            inserter => inserter.Add(utf8X, 1, -1),
                            inserter => inserter.Add(utf8X, 1, 0),
                        });
                }
            }

            SqlType[] nonJsonTextTypes = new[] { SqlType.Text(), SqlType.Varchar(1), SqlType.Varchar(10) };
            foreach (SqlType type in nonJsonTextTypes)
            {
                foreach (Nullability nullability in nullabilities)
                {
                    string[] expectedValues = funnyStrings.SelectMany(s => new[] { s, s, s }).ToArray();
                    Action<Inserter>[] actions = funnyBytes.SelectMany(b =>
                    new Action<Inserter>[] {
                        inserter => inserter.Add(b),
                        inserter => inserter.Add(b, 0),
                        inserter => inserter.Add(b, 0, b.Length),
                    }).ToArray();
                    TestInserterActionSuccess(type, nullability, expectedValues, actions);
                }
            }

            foreach (Nullability nullability in nullabilities)
            {
                string[] expectedValues = (new[] { " ", " ", " ", " ", "x", "x", "x", "x", " ", " ", " " })
                    .Concat(funnyStrings).ToArray();
                Action<Inserter>[] actions = new Action<Inserter>[] {
                            inserter => inserter.Add(utf8Empty),
                            inserter => inserter.Add(utf8Empty, 0),
                            inserter => inserter.Add(utf8Empty, 0, -1),
                            inserter => inserter.Add(utf8Empty, 0, 0),
                            inserter => inserter.Add(utf8X),
                            inserter => inserter.Add(utf8X, 0),
                            inserter => inserter.Add(utf8X, 0, -1),
                            inserter => inserter.Add(utf8X, 0, 1),
                            inserter => inserter.Add(utf8X, 1),
                            inserter => inserter.Add(utf8X, 1, -1),
                            inserter => inserter.Add(utf8X, 1, 0),
                    }.Concat(funnyBytes.Select(b => (Action<Inserter>)(inserter => inserter.Add(b)))).ToArray();
                TestInserterActionSuccess(SqlType.Char(1), nullability, expectedValues, actions);
            }

            foreach (Nullability nullability in nullabilities)
            {
                TestInserterActionSuccess(SqlType.Char(2), nullability, new[] { "  ", "  ", "  ", "  ", "x ", "x ", "x ", "x ", "  ", "  ", "  ", "xy" },
                    new Action<Inserter>[] {
                        inserter => inserter.Add(utf8Empty),
                        inserter => inserter.Add(utf8Empty, 0),
                        inserter => inserter.Add(utf8Empty, 0, -1),
                        inserter => inserter.Add(utf8Empty, 0, 0),
                        inserter => inserter.Add(utf8X),
                        inserter => inserter.Add(utf8X, 0),
                        inserter => inserter.Add(utf8X, 0, -1),
                        inserter => inserter.Add(utf8X, 0, 1),
                        inserter => inserter.Add(utf8X, 1),
                        inserter => inserter.Add(utf8X, 1, -1),
                        inserter => inserter.Add(utf8X, 1, 0),
                        inserter => inserter.Add(utf8XY),
                    });
            }

            SqlType[] allTextTypes = new[] { SqlType.Text(), SqlType.Json(), SqlType.Varchar(1), SqlType.Varchar(10), SqlType.Char(1), SqlType.Char(2) };
            foreach (SqlType type in allTextTypes)
            {
                TestInserterActionFailure(type, Nullability.Nullable, typeof(ArgumentException), inserter => inserter.Add(utf8Empty, -1, 0));
                TestInserterActionFailure(type, Nullability.Nullable, typeof(ArgumentException), inserter => inserter.Add(utf8Empty, 0, 1));
                TestInserterActionFailure(type, Nullability.Nullable, typeof(ArgumentException), inserter => inserter.Add(utf8Empty, 1, 0));
                TestInserterActionFailure(type, Nullability.Nullable, typeof(ArgumentException), inserter => inserter.Add(utf8X, 0, 2));
                TestInserterActionFailure(type, Nullability.Nullable, typeof(ArgumentException), inserter => inserter.Add(utf8X, 1, 1));
                TestInserterActionFailure(type, Nullability.Nullable, typeof(ArgumentException), inserter => inserter.Add(utf8X, 2, 0));
                TestInserterActionFailure(type, Nullability.Nullable, typeof(ArgumentException), inserter => inserter.Add((byte[])null));
            }
        }

        [Test]
        public void TestBytesParams()
        {
            Nullability[] nullabilities = new[] { Nullability.Nullable, Nullability.NotNullable };

            byte[] bufEmpty = new byte[0];
            byte[] buf1 = new byte[] { 1 };
            byte[] buf12 = new byte[] { 1, 2 };
            byte[] buf123 = new byte[] { 1, 2, 3 };
            byte[] buf23 = new byte[] { 2, 3 };
            byte[] buf2 = new byte[] { 2 };

            SqlType[] types = new[] { SqlType.Bytes(), SqlType.Geography() };
            foreach (SqlType type in types)
            {
                foreach (Nullability nullability in nullabilities)
                {
                    TestInserterActionSuccess(type, nullability,
                        new[] {
                            bufEmpty, bufEmpty, bufEmpty, bufEmpty, buf1, buf1, buf1, buf1,
                            bufEmpty, bufEmpty, bufEmpty, buf123, buf23, buf2, bufEmpty,
                        },
                        new Action<Inserter>[] {
                            inserter => inserter.Add(bufEmpty),
                            inserter => inserter.Add(bufEmpty, 0),
                            inserter => inserter.Add(bufEmpty, 0, -1),
                            inserter => inserter.Add(bufEmpty, 0, 0),
                            inserter => inserter.Add(buf1),
                            inserter => inserter.Add(buf1, 0),
                            inserter => inserter.Add(buf1, 0, -1),
                            inserter => inserter.Add(buf1, 0, 1),
                            inserter => inserter.Add(buf1, 1),
                            inserter => inserter.Add(buf1, 1, -1),
                            inserter => inserter.Add(buf1, 1, 0),
                            inserter => inserter.Add(buf123),
                            inserter => inserter.Add(buf123, 1),
                            inserter => inserter.Add(buf123, 1, 1),
                            inserter => inserter.Add(buf123, 3),
                        });
                }
            }

            foreach (SqlType type in types)
            {
                TestInserterActionFailure(type, Nullability.Nullable, typeof(ArgumentException), inserter => inserter.Add(bufEmpty, -1, 0));
                TestInserterActionFailure(type, Nullability.Nullable, typeof(ArgumentException), inserter => inserter.Add(bufEmpty, 0, 1));
                TestInserterActionFailure(type, Nullability.Nullable, typeof(ArgumentException), inserter => inserter.Add(bufEmpty, 1, 0));
                TestInserterActionFailure(type, Nullability.Nullable, typeof(ArgumentException), inserter => inserter.Add(buf1, 0, 2));
                TestInserterActionFailure(type, Nullability.Nullable, typeof(ArgumentException), inserter => inserter.Add(buf1, 1, 1));
                TestInserterActionFailure(type, Nullability.Nullable, typeof(ArgumentException), inserter => inserter.Add(buf1, 2, 0));
                TestInserterActionFailure(type, Nullability.Nullable, typeof(ArgumentException), inserter => inserter.Add((byte[])null));
            }
        }

        // Inserting and retrieving from a TIMESTAMP column applies no conversions
        [Test]
        public void TestTimestamp()
        {
            Timestamp dst = new Timestamp(2019, 6, 7, 8, 9, 10);
            Timestamp std = new Timestamp(2019, 12, 13, 14, 15, 16);
            Timestamp dstUtc = Timestamp.SpecifyKind(dst, DateTimeKind.Utc);
            Timestamp stdUtc = Timestamp.SpecifyKind(std, DateTimeKind.Utc);
            Timestamp dstLocal = Timestamp.SpecifyKind(dst, DateTimeKind.Local);
            Timestamp stdLocal = Timestamp.SpecifyKind(std, DateTimeKind.Local);
            IEnumerable<Timestamp> timestamps = new[] { dst, std, dstUtc, stdUtc, dstLocal, stdLocal };
            IEnumerable<DateTime> dateTimes = timestamps.Select(timestamp => (DateTime)timestamp);
            TestInserterSuccess(SqlType.Timestamp(), Nullability.Nullable, new Action<Connection> (connection => {
                connection.ExecuteCommand("SET TIME ZONE 'Europe/Sofia'");
            }), new Action<Inserter> (inserter => {
                // Add(object)
                foreach (object timestamp in timestamps)
                    inserter.Add(timestamp).EndRow();
                // Add(Timestamp)
                foreach (Timestamp timestamp in timestamps)
                    inserter.Add(timestamp).EndRow();
                // Add(DateTime)
                foreach (DateTime dateTime in dateTimes)
                    inserter.Add(dateTime).EndRow();
            }), new Action<Connection> (connection => {
                connection.ExecuteCommand("SET TIME ZONE 'US/Eastern'");
            }), new Action<Result> (result => {
                int numExpected = 2 * timestamps.Count() + dateTimes.Count();
                for (int i = 0; i < numExpected; ++i)
                {
                    Assert.IsTrue(result.NextRow());
                    // GetValue() and GetTimestamp()
                    Assert.IsTrue(result.GetTimestamp(0).Kind == DateTimeKind.Unspecified);
                    Timestamp expectedTimestamp = (i % 2 == 0) ? dst : std;
                    Assert.AreEqual(expectedTimestamp, result.GetValue(0));
                    Assert.AreEqual(expectedTimestamp, result.GetTimestamp(0));
                    // GetDateTime()
                    Assert.IsTrue(result.GetDateTime(0).Kind == DateTimeKind.Unspecified);
                    DateTime expectedDateTime = (DateTime)expectedTimestamp;
                    Assert.AreEqual(expectedDateTime, result.GetDateTime(0));
                }
            }));
        }

        // Inserting and retrieving from a TIMESTAMP_TZ column applies conversions for DateTime and DateTimeOffset
        [Test]
        public void TestTimestampTZ()
        {
            Timestamp dst = new Timestamp(2019, 6, 7, 8, 9, 10);
            Timestamp std = new Timestamp(2019, 12, 13, 14, 15, 16);
            Timestamp dstUtc = dst.ToUniversalTime();
            Timestamp stdUtc = std.ToUniversalTime();
            Timestamp dstLocal = Timestamp.SpecifyKind(dst, DateTimeKind.Local);
            Timestamp stdLocal = Timestamp.SpecifyKind(std, DateTimeKind.Local);
            IEnumerable<Timestamp> timestamps = new[] { dst, std, dstUtc, stdUtc, dstLocal, stdLocal };
            IEnumerable<DateTime> dateTimes = timestamps.Select(timestamp => (DateTime)timestamp);
            IEnumerable<DateTimeOffset> dateTimeOffsets = timestamps.Select(timestamp => (DateTimeOffset)timestamp);

            TestInserterSuccess(SqlType.TimestampTZ(), Nullability.Nullable, new Action<Connection> (connection => {
                connection.ExecuteCommand("SET TIME ZONE 'Europe/Sofia'");
            }), new Action<Inserter> (inserter => {
                // Add(object)
                foreach (object timestamp in timestamps)
                    inserter.Add(timestamp).EndRow();
                // Add(Timestamp)
                foreach (Timestamp timestamp in timestamps)
                    inserter.Add(timestamp).EndRow();
                // Add(DateTime)
                foreach (DateTime dateTime in dateTimes)
                    inserter.Add(dateTime).EndRow();
                // Add(DateTimeOffset)
                foreach (DateTimeOffset dateTimeOffset in dateTimeOffsets)
                    inserter.Add(dateTimeOffset).EndRow();
            }), new Action<Connection> (connection => {
                connection.ExecuteCommand("SET TIME ZONE 'US/Eastern'");
            }), new Action<Result> (result => {
                int numExpected = 2 * timestamps.Count() + dateTimes.Count() + dateTimeOffsets.Count();
                for (int i = 0; i < numExpected; ++i)
                {
                    Assert.IsTrue(result.NextRow());
                    // GetValue() and GetTimestamp()
                    Timestamp expectedTimestamp = (i % 2 == 0) ? dstUtc : stdUtc;
                    Assert.IsTrue(result.GetTimestamp(0).Kind == DateTimeKind.Utc);
                    Assert.AreEqual(expectedTimestamp, result.GetValue(0));
                    Assert.AreEqual(expectedTimestamp, result.GetTimestamp(0));
                    // GetDateTime()
                    Assert.IsTrue(result.GetDateTime(0).Kind == DateTimeKind.Utc);
                    DateTime expectedDateTime = (DateTime)expectedTimestamp;
                    Assert.AreEqual(expectedDateTime, result.GetDateTime(0));
                    // GetDateTimeOffset()
                    DateTimeOffset expectedDateTimeOffset = (DateTimeOffset)expectedTimestamp;
                    Assert.AreEqual(expectedDateTimeOffset, result.GetDateTimeOffset(0));
                }
            }));
        }

        // Inserting with expressions
        [Test]
        public void TestExpressionInsert()
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                TableName tableName = new TableName("testExpressionInsert");
                connection.ExecuteCommand($"DROP TABLE IF EXISTS {tableName}");
                TableDefinition tableDefinition = new TableDefinition(tableName)
                                                      .AddColumn("a", SqlType.Int())
                                                      .AddColumn("b", SqlType.Int())
                                                      .AddColumn("sum", SqlType.Int());

                connection.Catalog.CreateTable(tableDefinition);
                List<Inserter.ColumnMapping> columnMappings = new List<Inserter.ColumnMapping>();
                columnMappings.Add(new Inserter.ColumnMapping("a"));
                columnMappings.Add(new Inserter.ColumnMapping("b"));
                columnMappings.Add(new Inserter.ColumnMapping("sum", "\"a\" + \"b\""));

                List<TableDefinition.Column> inserterDefinition = tableDefinition.Columns.Take(2).ToList();

                using (Inserter inserter = new Inserter(connection, tableDefinition, columnMappings, inserterDefinition))
                {
                    inserter.Add(1).Add(2).EndRow();
                    inserter.Add(3).Add(4).EndRow();
                    inserter.Execute();
                }

                Assert.AreEqual(2, connection.ExecuteScalarQuery<long>($"SELECT COUNT(*) FROM {tableName}"));

                object[][] data = new[] {
                    new object[] {1, 2, 3},
                    new object[] {3, 4, 7}
                };

                using (Result result = connection.ExecuteQuery($"SELECT * FROM {tableName}"))
                {
                    ValidateData(result, data);
                }
                connection.ExecuteCommand($"DROP TABLE {tableName}");
            }
        }

        // Inserting using Expressions on Intermediate columns
        [Test]
        public void TestExpressionInsertIntermediateColumns()
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                TableName tableName = new TableName("testExpressionInsertIntermediateColumns");
                connection.ExecuteCommand($"DROP TABLE IF EXISTS {tableName}");
                TableDefinition tableDefinition = new TableDefinition(tableName)
                                                      .AddColumn("b", SqlType.Int());

                connection.Catalog.CreateTable(tableDefinition);
                List<Inserter.ColumnMapping> columnMappings = new List<Inserter.ColumnMapping>();
                columnMappings.Add(new Inserter.ColumnMapping("b", "\"ib1\" * \"ib2\""));

                List<TableDefinition.Column> inserterDefinition = new List<TableDefinition.Column>();
                inserterDefinition.Add(new TableDefinition.Column("ib1", SqlType.Int()));
                inserterDefinition.Add(new TableDefinition.Column("ib2", SqlType.Int()));

                using (Inserter inserter = new Inserter(connection, tableDefinition, columnMappings, inserterDefinition))
                {
                    inserter.Add(7).Add(6).EndRow();
                    inserter.Add(21).Add(2).EndRow();
                    inserter.Execute();
                }

                Assert.AreEqual(2, connection.ExecuteScalarQuery<long>($"SELECT COUNT(*) FROM {tableName}"));

                object[][] data = new[] {
                    new object[] {42},
                    new object[] {42}
                };

                using (Result result = connection.ExecuteQuery($"SELECT * FROM {tableName}"))
                {
                    ValidateData(result, data);
                }
                connection.ExecuteCommand($"DROP TABLE {tableName}");
            }
        }

        // Inserting using CASE expression
        [Test]
        public void TestExpressionInsertWithCASE()
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                TableName tableName = new TableName("testExpressionInsertWithCASE");
                connection.ExecuteCommand($"DROP TABLE IF EXISTS {tableName}");
                TableDefinition tableDefinition = new TableDefinition(tableName)
                                                      .AddColumn("b", SqlType.Int());

                connection.Catalog.CreateTable(tableDefinition);
                List<Inserter.ColumnMapping> columnMappings = new List<Inserter.ColumnMapping>();
                columnMappings.Add(new Inserter.ColumnMapping("b", "CASE \"b_text\" WHEN \'one\' THEN 1 WHEN \'two\' THEN 2 ELSE 0 END "));

                List<TableDefinition.Column> inserterDefinition = new List<TableDefinition.Column>();
                inserterDefinition.Add(new TableDefinition.Column("b_text", SqlType.Text()));

                using (Inserter inserter = new Inserter(connection, tableDefinition, columnMappings, inserterDefinition))
                {
                    inserter.Add("one").EndRow();
                    inserter.Add("two").EndRow();
                    inserter.Add("three").EndRow();
                    inserter.Execute();
                }

                Assert.AreEqual(3, connection.ExecuteScalarQuery<long>($"SELECT COUNT(*) FROM {tableName}"));

                object[][] data = new[] {
                    new object[] {1},
                    new object[] {2},
                    new object[] {0}
                };

                using (Result result = connection.ExecuteQuery($"SELECT * FROM {tableName}"))
                {
                    ValidateData(result, data);
                }
                connection.ExecuteCommand($"DROP TABLE {tableName}");
            }
        }

        // Inserting using Collate
        [Test]
        public void TestExpressionInsertWithCollate()
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                TableName tableName = new TableName("testExpressionInsertWithCollate");
                connection.ExecuteCommand($"DROP TABLE IF EXISTS {tableName}");
                TableDefinition tableDefinition = new TableDefinition(tableName)
                                                      .AddColumn("b", SqlType.Text(), "en_US");

                connection.Catalog.CreateTable(tableDefinition);
                List<Inserter.ColumnMapping> columnMappings = new List<Inserter.ColumnMapping>();
                columnMappings.Add(new Inserter.ColumnMapping("b", "upper(\"b_ci\" COLLATE \"en_US\")"));

                List<TableDefinition.Column> inserterDefinition = new List<TableDefinition.Column>();
                inserterDefinition.Add(new TableDefinition.Column("b_ci", SqlType.Text(), "en_US_CI"));

                using (Inserter inserter = new Inserter(connection, tableDefinition, columnMappings, inserterDefinition))
                {
                    inserter.Add("one").EndRow();
                    inserter.Add("two").EndRow();
                    inserter.Execute();
                }

                Assert.AreEqual(2, connection.ExecuteScalarQuery<long>($"SELECT COUNT(*) FROM {tableName}"));

                object[][] data = new[] {
                    new object[] {"ONE"},
                    new object[] {"TWO"}
                };

                using (Result result = connection.ExecuteQuery($"SELECT * FROM {tableName}"))
                {
                    ValidateData(result, data);
                }
                connection.ExecuteCommand($"DROP TABLE {tableName}");
            }
        }

        // Inserting Geography column using expression
        [Test]
        public void TestExpressionInsertWKT()
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                TableName tableName = new TableName("testExpressionInsertWKT");
                connection.ExecuteCommand($"DROP TABLE IF EXISTS {tableName}");
                TableDefinition tableDefinition = new TableDefinition(tableName)
                                                      .AddColumn("City", SqlType.Text())
                                                      .AddColumn("geo", SqlType.Geography());

                connection.Catalog.CreateTable(tableDefinition);
                List<Inserter.ColumnMapping> columnMappings = new List<Inserter.ColumnMapping>();
                columnMappings.Add(new Inserter.ColumnMapping("City"));
                columnMappings.Add(new Inserter.ColumnMapping("geo", "\"geo_as_text\""));

                List<TableDefinition.Column> inserterDefinition = new List<TableDefinition.Column>();
                inserterDefinition.Add(new TableDefinition.Column("City", SqlType.Text()));
                inserterDefinition.Add(new TableDefinition.Column("geo_as_text", SqlType.Text()));

                using (Inserter inserter = new Inserter(connection, tableDefinition, columnMappings, inserterDefinition))
                {
                    inserter.Add("Seattle").Add("POINT(-122.338083 47.647528)").EndRow();
                    inserter.Add("Munich").Add("POINT(11.584329 48.139257)").EndRow();
                    inserter.Execute();
                }

                Assert.AreEqual(2, connection.ExecuteScalarQuery<long>($"SELECT COUNT(*) FROM {tableName}"));

                object[][] data = new[] {
                    new object[] {"Munich", "POINT(11.5843290 48.1392570)"},
                    new object[] {"Seattle", "POINT(-122.3380830 47.6475280)"}
                };

                using (Result result = connection.ExecuteQuery($"SELECT \"City\", \"geo\"::text FROM {tableName} ORDER BY \"City\""))
                {
                    ValidateData(result, data);
                }
                connection.ExecuteCommand($"DROP TABLE {tableName}");
            }
        }

        // Inserting With Columns reordered
        [Test]
        public void TestColumnReorderedInsert()
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                TableName tableName = new TableName("testColumnReorderedInsert");
                connection.ExecuteCommand($"DROP TABLE IF EXISTS {tableName}");
                TableDefinition tableDefinition = new TableDefinition(tableName)
                                                      .AddColumn("a", SqlType.BigInt())
                                                      .AddColumn("b", SqlType.Double());

                connection.Catalog.CreateTable(tableDefinition);

                using (Inserter inserter = new Inserter(connection, tableName, new String[]{"b", "a"}))
                {
                    inserter.Add((double)17.0).Add(18).EndRow();
                    inserter.Add((double)19.2).Add(19).EndRow();
                    inserter.Execute();
                }

                Assert.AreEqual(2, connection.ExecuteScalarQuery<long>($"SELECT COUNT(*) FROM {tableName}"));

                object[][] data = new[] {
                    new object[] {18, 17.0},
                    new object[] {19, 19.2}
                };

                using (Result result = connection.ExecuteQuery($"SELECT * FROM {tableName} \"a\""))
                {
                    ValidateData(result, data);
                }
                connection.ExecuteCommand($"DROP TABLE {tableName}");
            }
        }

        // Inserter with Non-Existent Column
        [Test]
        public void TestNonExistentColumn()
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                TableName tableName = new TableName("testNonExistentColumn");
                connection.ExecuteCommand($"DROP TABLE IF EXISTS {tableName}");
                TableDefinition tableDefinition = new TableDefinition(tableName)
                                                      .AddColumn("a", SqlType.BigInt())
                                                      .AddColumn("b", SqlType.Double());

                connection.Catalog.CreateTable(tableDefinition);

                List<Inserter.ColumnMapping> columnMappings = new List<Inserter.ColumnMapping>();
                columnMappings.Add(new Inserter.ColumnMapping("b"));
                columnMappings.Add(new Inserter.ColumnMapping("c", "a"));

                List<TableDefinition.Column> inserterDefinition = tableDefinition.Columns.ToList();

                Assert.Throws<ArgumentException>(() => new Inserter(connection, tableName, columnMappings, inserterDefinition));

                connection.ExecuteCommand($"DROP TABLE {tableName}");
            }
        }

        // Column definition mismatch
        [Test]
        public void TestColumnMismatch()
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                TableName tableName = new TableName("testNonExistentColumn");
                connection.ExecuteCommand($"DROP TABLE IF EXISTS {tableName}");
                TableDefinition tableDefinition = new TableDefinition(tableName)
                                                      .AddColumn("a", SqlType.BigInt())
                                                      .AddColumn("b", SqlType.BigInt());

                connection.Catalog.CreateTable(tableDefinition);

                List<Inserter.ColumnMapping> columnMappings = new List<Inserter.ColumnMapping>();
                columnMappings.Add(new Inserter.ColumnMapping("a"));
                columnMappings.Add(new Inserter.ColumnMapping("b", "1"));

                List<TableDefinition.Column> inserterDefinition = new List<TableDefinition.Column>();
                inserterDefinition.Add(new TableDefinition.Column("a", SqlType.Text()));

                Assert.Throws<ArgumentException>(() => {new Inserter(connection, tableName, columnMappings, inserterDefinition);});

                connection.ExecuteCommand($"DROP TABLE {tableName}");
            }
        }

        // Column not defined in Inserter Definition
        [Test]
        public void TestColumnNotDefined()
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                TableName tableName = new TableName("testColumnNotDefined");
                connection.ExecuteCommand($"DROP TABLE IF EXISTS {tableName}");
                TableDefinition tableDefinition = new TableDefinition(tableName)
                                                      .AddColumn("a", SqlType.BigInt())
                                                      .AddColumn("b", SqlType.BigInt())
                                                      .AddColumn("c", SqlType.BigInt());

                connection.Catalog.CreateTable(tableDefinition);

                List<Inserter.ColumnMapping> columnMappings = new List<Inserter.ColumnMapping>();
                columnMappings.Add(new Inserter.ColumnMapping("a"));
                columnMappings.Add(new Inserter.ColumnMapping("b", "1"));
                columnMappings.Add(new Inserter.ColumnMapping("c"));

                List<TableDefinition.Column> inserterDefinition = new List<TableDefinition.Column>();
                inserterDefinition.Add(new TableDefinition.Column("a", SqlType.BigInt()));

                Assert.Throws<ArgumentException>(() => {new Inserter(connection, tableDefinition, columnMappings, inserterDefinition);});

                connection.ExecuteCommand($"DROP TABLE {tableName}");
            }
        }

        private void TestBigNumericInsertion(uint scale, String value, String expectation)
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                TableDefinition tableDef = new TableDefinition("foo").AddColumn("x", SqlType.Numeric(38,scale));
                connection.ExecuteCommand($"DROP TABLE IF EXISTS {tableDef.TableName}");
                connection.Catalog.CreateTable(tableDef);

                // Insert the value
                using (Inserter inserter = new Inserter(connection, tableDef))
                {
                    inserter.AddRow(decimal.Parse(value));
                    inserter.Execute();
                }

                // Check that the value is in the database
                Assert.AreEqual(expectation, connection.ExecuteScalarQuery<String>("SELECT x::TEXT FROM FOO"));
            }
        }

        // Test 128-bit numeric insertion into a column with scale of zero
        [Test]
        public void TestBigNumericInsertionScale0()
        {
            // Correct scale
            TestBigNumericInsertion(0, "0", "0");
            TestBigNumericInsertion(0, "-1", "-1");
            TestBigNumericInsertion(0, "1", "1");
            TestBigNumericInsertion(0, "8923648236465", "8923648236465");
            TestBigNumericInsertion(0, "-8923648236465", "-8923648236465");
            // Scale down
            TestBigNumericInsertion(0, "-1.00000000000000000000000000", "-1");
            TestBigNumericInsertion(0, "1.00000000000000000000000000", "1");
            // Scale down (truncate)
            TestBigNumericInsertion(0, "-1.60000000000000000000000000", "-1");
            TestBigNumericInsertion(0, "1.60000000000000000000000000", "1");
            // Bounds of c# decimal
            TestBigNumericInsertion(0, "79228162514264337593543950335", "79228162514264337593543950335");
            TestBigNumericInsertion(0, "-79228162514264337593543950335", "-79228162514264337593543950335");
            TestBigNumericInsertion(0, "792281625142643375935439503.00", "792281625142643375935439503");
            TestBigNumericInsertion(0, "-792281625142643375935439503.00", "-792281625142643375935439503");
        }

        // Test 128-bit numeric insertion into a column with scale other than zero
        [Test]
        public void TestBigNumericInsertionScaleNonZero()
        {
            // Correct scale
            TestBigNumericInsertion(3, "0.001", "0.001");
            TestBigNumericInsertion(3, "-0.001", "-0.001");
            TestBigNumericInsertion(20, "-7.00000400000005000001", "-7.00000400000005000001");
            TestBigNumericInsertion(20, "7.00000400000005000001", "7.00000400000005000001");
            // Scale down (exact)
            TestBigNumericInsertion(3, "0.00100000", "0.001");
            TestBigNumericInsertion(3, "-0.00100000", "-0.001");
            TestBigNumericInsertion(20, "-7.00000400000005000001000", "-7.00000400000005000001");
            TestBigNumericInsertion(20, "7.00000400000005000001000", "7.00000400000005000001");
            // Scale down (truncate)
            TestBigNumericInsertion(3, "0.00153000", "0.001");
            TestBigNumericInsertion(3, "-0.00153000", "-0.001");
            TestBigNumericInsertion(20, "-7.00000400000005000001570", "-7.00000400000005000001");
            TestBigNumericInsertion(20, "7.00000400000005000001570", "7.00000400000005000001");
            // Scale up
            TestBigNumericInsertion(3, "0.1", "0.100");
            TestBigNumericInsertion(3, "-0.1", "-0.100");
            TestBigNumericInsertion(20, "0.1", "0.10000000000000000000");
            TestBigNumericInsertion(20, "-0.1", "-0.10000000000000000000");
            TestBigNumericInsertion(20, "12.123456", "12.12345600000000000000");
            TestBigNumericInsertion(20, "-12.123456", "-12.12345600000000000000");
            // Scale up beyond what a C# decimal could represent
            TestBigNumericInsertion(37, "0.1", "0.1000000000000000000000000000000000000");
            TestBigNumericInsertion(37, "-0.1", "-0.1000000000000000000000000000000000000");
        }
    }
}
