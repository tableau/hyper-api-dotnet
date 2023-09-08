using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

namespace Tableau.HyperAPI.Test
{
    [TestFixture]
    class ResultTests : HyperTest
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

        /// Tests column type mismatch.
        ///
        /// Generate a result that has one column of type `columnType`. Try to deserialize the value using calback
        /// that operates on a different type. Expect an InvalidCastException.
        private void TestTypeMismatch(SqlType columnType, Func<Result, object> callback)
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                using (Result result = connection.ExecuteQuery("SELECT null::" + GetSQLType(columnType) + " AS test"))
                {
                    Assert.True(result.IsOpen);
                    Assert.True(result.NextRow());
                    InvalidCastException ex = Assert.Throws<InvalidCastException>(() => callback(result));
                    Assert.True(ex.Message.Contains("attempted to get value of type"));
                    ex = Assert.Throws<InvalidCastException>(() => result.GetRaw(0));
                    Assert.True(ex.Message.Contains("attempted to convert a NULL value to type"));
                }
            }
        }

        /// Tests type conversions
        ///
        /// Insert value into a table that has one column of `columnType` using `addCallback`.
        /// Try to deserialize the value using the `readCallback`
        /// Add corresponding asserts in `readCallback` to verify read was successful
        private void TestTypeConversion(SqlType columnType, Action<Inserter> addCallback, Action<Result> readCallback)
        {
            using (Connection connection = new Connection(Server.Endpoint, Database))
            {
                connection.ExecuteCommand("DROP TABLE IF EXISTS \"lenient_cast_result\"");
                TableDefinition schema = new TableDefinition("lenient_cast_result");
                schema.AddColumn("test", columnType);
                connection.Catalog.CreateTable(schema);

                using (Inserter inserter = new Inserter(connection, schema))
                {
                    addCallback(inserter);
                    inserter.Execute();
                }

                using (Result result = connection.ExecuteQuery("SELECT test FROM \"lenient_cast_result\""))
                {
                    Assert.True(result.IsOpen);
                    Assert.True(result.NextRow());

                    readCallback(result);
                }

                connection.ExecuteCommand("DROP TABLE \"lenient_cast_result\"");
            }
        }

        private string GetSQLType(SqlType type)
        {
            switch (type.Tag)
            {
                case TypeTag.Bool:
                    return "boolean";
                case TypeTag.BigInt:
                    return "bigint";
                case TypeTag.SmallInt:
                    return "smallint";
                case TypeTag.Int:
                    return "int";
                case TypeTag.Numeric:
                    return "numeric(" + type.Precision + ", " + type.Scale + ")";
                case TypeTag.Double:
                    return "double precision";
                case TypeTag.Oid:
                    return "oid";
                case TypeTag.Bytes:
                    return "bytea";
                case TypeTag.Text:
                    return "text";
                case TypeTag.Varchar:
                    return "varchar(" + type.MaxLength + ")";
                case TypeTag.Char:
                    return "char(" + type.MaxLength + ")";
                case TypeTag.Json:
                    return "json";
                case TypeTag.Date:
                    return "date";
                case TypeTag.Interval:
                    return "interval";
                case TypeTag.Time:
                    return "time";
                case TypeTag.Timestamp:
                    return "timestamp";
                case TypeTag.TimestampTZ:
                    return "timestamptz";
                case TypeTag.Geography:
                    return "geography";
                default:
                    throw new NotImplementedException($"unsupported data type {type.Tag}");
            }
        }

        /// Tests column type mismatch.
        [Test]
        public void TestDeserializationErrorBoolean()
        {
            TestTypeMismatch(SqlType.BigInt(), (result) => result.GetBool(0));
        }

        /// Tests column type mismatch.
        [Test]
        public void TestDeserializationErrorLong()
        {
            TestTypeMismatch(SqlType.Json(), (result) => result.GetLong(0));
        }

        /// Tests column type mismatch.
        [Test]
        public void TestDeserializationErrorShort()
        {
            TestTypeMismatch(SqlType.Int(), (result) => result.GetShort(0));
        }

        /// Tests column type mismatch.
        [Test]
        public void TestDeserializationErrorInteger()
        {
            TestTypeMismatch(SqlType.Numeric(18, 3), (result) => result.GetInt(0));
        }

        /// Tests column type mismatch.
        [Test]
        public void TestDeserializationErrorDecimal()
        {
            TestTypeMismatch(SqlType.Double(), (result) => result.GetDecimal(0));
        }

        /// Tests column type mismatch
        [Test]
        public void TestDeserializationErrorString()
        {
            TestTypeMismatch(SqlType.Int(), (result) => result.GetString(0));
        }

        /// Tests column type mismatch.
        [Test]
        public void TestDeserializationErrorDate()
        {
            TestTypeMismatch(SqlType.Time(), (result) => result.GetDate(0));
        }

        /// Tests column type mismatch.
        [Test]
        public void TestDeserializationTimeSpan()
        {
            TestTypeMismatch(SqlType.Interval(), (result) => result.GetTime(0));
        }

        /// Tests column type mismatch.
        [Test]
        public void TestDeserializationErrorInterval()
        {
            TestTypeMismatch(SqlType.Timestamp(), (result) => result.GetInterval(0));
        }

        /// Tests column type mismatch.
        [Test]
        public void TestDeserializationTimestamp()
        {
            TestTypeMismatch(SqlType.Geography(), (result) => result.GetTimestamp(0));
        }

        /// Tests column type mismatch.
        [Test]
        public void TestDeserializationByteArray()
        {
            TestTypeMismatch(SqlType.Bool(), (result) => result.GetBytes(0));
        }

        /// Test type conversion for SmallInt column
        [Test]
        public void TestTypeConversionSmallInt()
        {
            Action<Result> smallIntLenientCast = (result) =>
            {
                short v1 = result.GetShort(0);
                int v2 = result.GetInt(0);
                long v3 = result.GetLong(0);
                decimal v4 = result.GetDecimal(0);
                double v5 = result.GetDouble(0);
                Assert.AreEqual(v1, v2);
                Assert.AreEqual(v1, v3);
                Assert.AreEqual(v1, v4);
                Assert.AreEqual(v1, v5);
            };

            TestTypeConversion(SqlType.SmallInt(), (inserter) => inserter.Add((short)123), smallIntLenientCast);
        }

        /// Test type conversion for Int column
        [Test]
        public void TestTypeConversionInt()
        {
            Action<Result> intLenientCast = (result) =>
            {
                int v1 = result.GetInt(0);
                long v2 = result.GetLong(0);
                decimal v3 = result.GetDecimal(0);
                double v4 = result.GetDouble(0);
                Assert.AreEqual(v1, v2);
                Assert.AreEqual(v1, v3);
                Assert.AreEqual(v1, v4);
            };

            TestTypeConversion(SqlType.Int(), (inserter) => inserter.Add(123456), intLenientCast);
        }

        /// Test type conversion for BigInt column
        [Test]
        public void TestTypeConversionBigInt()
        {
            Action<Result> bigIntLenientCast = (result) =>
            {
                long v1 = result.GetLong(0);
                decimal v2 = result.GetDecimal(0);
                double v3 = result.GetDouble(0);
                Assert.AreEqual(v1, v2);
                Assert.AreEqual(v1, v3);
            };

            TestTypeConversion(SqlType.BigInt(), (inserter) => inserter.Add(123456789L), bigIntLenientCast);
        }

        /// Test type conversion for Oid column
        [Test]
        public void TestTypeConversionOid()
        {
            Action<Result> oidLenientCast = (result) =>
            {
                uint v1 = result.GetUint(0);
                long v2 = result.GetLong(0);
                decimal v3 = result.GetDecimal(0);
                double v4 = result.GetDouble(0);
                Assert.AreEqual(v1, v2);
                Assert.AreEqual(v1, v3);
                Assert.AreEqual(v1, v4);
            };

            TestTypeConversion(SqlType.Oid(), (inserter) => inserter.Add((uint)123456), oidLenientCast);
        }

        /// Test type conversion for Numeric column
        [Test]
        public void TestTypeConversionNumeric()
        {
            Action<Result> numericLenientCast = (result) =>
            {
                decimal v1 = result.GetDecimal(0);
                double v3 = result.GetDouble(0);
                Assert.AreEqual(v1, v3);
            };

            TestTypeConversion(SqlType.Numeric(10, 3), (inserter) => inserter.Add((decimal)3.1415), numericLenientCast);
        }

        private static readonly byte[] bytesDigits = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
        private const string strDigits = "1234567890";
        private static readonly byte[] bytesDigitsUtf8 = Encoding.UTF8.GetBytes(strDigits);

        private TableDefinition PrepareTableWithBytes()
        {
            using (var connection = new Connection(Server.Endpoint, Database))
            {
                var tableDef = new TableDefinition("test")
                    .AddColumn("bytes", SqlType.Bytes())
                    .AddColumn("text", SqlType.Text())
                    .AddColumn("char1", SqlType.Char(1))
                    .AddColumn("char10", SqlType.Char(10))
                    .AddColumn("varchar", SqlType.Varchar(10))
                    .AddColumn("json", SqlType.Json())
                    ;
                connection.ExecuteCommand($"DROP TABLE IF EXISTS {tableDef.TableName}");
                connection.Catalog.CreateTable(tableDef);
                using (var inserter = new Inserter(connection, tableDef))
                {
                    inserter.AddRow(bytesDigits, strDigits, "1", strDigits, strDigits, strDigits);
                    inserter.Execute();
                }

                return tableDef;
            }
        }

        private void TestGetBytes(Result result, int column, byte[] expectedValue)
        {
            Assert.AreEqual(10, expectedValue.Length);
            Assert.AreEqual(expectedValue, result.GetBytes(column));
            byte[] dest = new byte[10];
            Assert.AreEqual(10, result.GetBytes(column, 0, dest, 0, 10));
            Assert.AreEqual(expectedValue, dest);
            dest = new byte[10];
            Assert.AreEqual(10, result.GetBytes(column, 0, dest, 0, 20));
            Assert.AreEqual(expectedValue, dest);
            dest = new byte[10];
            Assert.AreEqual(5, result.GetBytes(column, 5, dest, 5, 10));
            Assert.AreEqual(new byte[] { 0, 0, 0, 0, 0, expectedValue[5], expectedValue[6], expectedValue[7], expectedValue[8], expectedValue[9] }, dest);
            dest = new byte[3];
            Assert.AreEqual(3, result.GetBytes(column, 0, dest, 0, 3));
            Assert.AreEqual(new byte[] { expectedValue[0], expectedValue[1], expectedValue[2] }, dest);
            Assert.AreEqual(0, result.GetBytes(column, 10, dest, 0, 3));
        }

        [Test]
        public void TestGetBytes()
        {
            TableDefinition tableDef = PrepareTableWithBytes();
            using (var connection = new Connection(Server.Endpoint, Database))
            {
                using (Result result = connection.ExecuteQuery($"SELECT * FROM {tableDef.TableName}"))
                {
                    Assert.True(result.NextRow());
                    ResultSchema rs = result.Schema;

                    int colBytes = rs.GetColumnPosByName("bytes");
                    int colText = rs.GetColumnPosByName("text");
                    int colChar1 = rs.GetColumnPosByName("char1");
                    int colChar10 = rs.GetColumnPosByName("char10");
                    int colVarchar = rs.GetColumnPosByName("varchar");
                    int colJson = rs.GetColumnPosByName("json");

                    TestGetBytes(result, colBytes, bytesDigits);

                    foreach (int col in new[] { colText, colChar10, colVarchar, colJson })
                        TestGetBytes(result, col, bytesDigitsUtf8);

                    Assert.AreEqual(new byte[] { (byte)'1' }, result.GetBytes(colChar1));
                }
            }
        }

        [Test]
        public void TestGetColumnByName()
        {
            using (var connection = new Connection(Server.Endpoint, Database))
            {
                using (Result result = connection.ExecuteQuery($"SELECT 1 AS a"))
                {
                    Assert.True(result.NextRow());
                    ResultSchema rs = result.Schema;

                    Assert.AreEqual(rs.GetColumnByName("a").Name.Unescaped, "a");
                    Assert.AreEqual(rs.GetColumnByName(new Name("a")).Name.Unescaped,"a");
                    Assert.AreEqual(rs.GetColumnByName("b"), null);
                    Assert.AreEqual(rs.GetColumnByName(new Name("b")), null);
                }
            }
        }

        [Test]
        public void TestGetColumnPosByName()
        {
            using (var connection = new Connection(Server.Endpoint, Database))
            {
                using (Result result = connection.ExecuteQuery($"SELECT 1 AS a"))
                {
                    Assert.True(result.NextRow());
                    ResultSchema rs = result.Schema;

                    int colPosByString = rs.GetColumnPosByName("a");
                    int colPosByName = rs.GetColumnPosByName(new Name("a"));

                    Assert.AreEqual(colPosByString, 0);
                    Assert.AreEqual(colPosByString, colPosByName);
                    Assert.AreEqual(rs.GetColumnPosByName("b"), -1);
                    Assert.AreEqual(rs.GetColumnPosByName(new Name("b")), -1);
                }
            }
        }

        [Test]
        public void TestGetBigNumeric()
        {
            using (var connection = new Connection(Server.Endpoint, Database))
            {
                using (Result result = connection.ExecuteQuery($"SELECT '123456.123456'::NUMERIC(30,6)," +
                                                               $" '-123456.123456'::NUMERIC(30,6), " +
                                                               $"'0.0000000000000000000000000001'::NUMERIC(38,28), " +
                                                               $"'-1'::NUMERIC(30,0), " +
                                                               $"'-39614081257132168796771975168'::NUMERIC(38,0), " +
                                                               $"'79228162514264337593543950335'::NUMERIC(38,0), " +
                                                               $"'-79228162514264337593543950335'::NUMERIC(38,0)"))
                {
                    Assert.True(result.NextRow());
                    Assert.AreEqual(Decimal.Parse("123456.123456"), result.GetDecimal(0));
                    Assert.AreEqual(Decimal.Parse("-123456.123456"), result.GetDecimal(1));
                    Assert.AreEqual(Decimal.Parse("0.0000000000000000000000000001"), result.GetDecimal(2));
                    Assert.AreEqual(Decimal.Parse("-1"), result.GetDecimal(3));
                    Assert.AreEqual(Decimal.Parse("-39614081257132168796771975168"), result.GetDecimal(4));
                    Assert.AreEqual(Decimal.Parse("79228162514264337593543950335"), result.GetDecimal(5));
                    Assert.AreEqual(Decimal.Parse("-79228162514264337593543950335"), result.GetDecimal(6));
                }
            }
        }
    }
}
