using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace Tableau.HyperAPI.Test
{
    public interface ISchemaColumn
    {
        Nullability Nullability { get; }
        SqlType Type { get; }
    }

    public class TableSchemaColumn : ISchemaColumn
    {
        private TableDefinition.Column column;

        public TableSchemaColumn(TableDefinition.Column column)
        {
            this.column = column;
        }

        public Nullability Nullability => column.Nullability;
        public SqlType Type => column.Type;
    }

    public class ResultSchemaColumn : ISchemaColumn
    {
        private ResultSchema.Column column;

        public ResultSchemaColumn(ResultSchema.Column column)
        {
            this.column = column;
        }

        public Nullability Nullability => Nullability.Nullable;
        public SqlType Type => column.Type;
    }

    public class Schema
    {
        public List<ISchemaColumn> Columns { get; private set; }

        public Schema(TableDefinition tableDefinition)
        {
            Columns = tableDefinition.Columns.Select(c => new TableSchemaColumn(c)).Cast<ISchemaColumn>().ToList();
        }

        public Schema(ResultSchema resultSchema)
        {
            Columns = resultSchema.Columns.Select(c => new ResultSchemaColumn(c)).Cast<ISchemaColumn>().ToList();
        }
    }

    /// <summary>
    /// Super class for hyper test classes. Subclasses must have <c>TestFixture</c>
    /// attribute and call <c>SetUpTest</c>/<c>TearDownTest</c> from their respective
    /// <c>SetUp</c> and <c>TearDown</c> methods.
    /// </summary>
    public class HyperTest
    {
        public TestHyperEnvironment TestEnv { get; private set; }

        public HyperProcess Server => TestEnv.Server;
        public string Database => TestEnv.Database;

        public void SetUpTest(TestFlags flags = TestFlags.None)
        {
            TestEnv = new TestHyperEnvironment(flags);
        }

        public void TearDownTest()
        {
            if (TestEnv != null)
            {
                TestEnv.Dispose();
                TestEnv = null;
            }
        }

        public void CreateTable(Connection connection, TableDefinition schema, Action<Inserter> insertData)
        {
            connection.Catalog.CreateTable(schema);
            using (var inserter = new Inserter(connection, schema))
            {
                insertData(inserter);
                inserter.Execute();
            }
        }

        public void CreateTable(Connection connection, TableDefinition schema, IEnumerable<object[]> data)
        {
            CreateTable(connection, schema, inserter => inserter.AddRows(data));
        }

        public TableDefinition GetSampleSchema(TableName tableName)
        {
            return new TableDefinition(tableName)
                .AddColumn("bool", SqlType.Bool())
                .AddColumn("int", SqlType.Int())
                .AddColumn("text", SqlType.Text(), "en_US_CI");
        }

        public object[][] GetSampleData()
        {
            return new[] {
                new object[]{ true, 1, "x" },
                new object[]{ false, null, "y" },
                new object[]{ true, 2, null },
            };
        }

        public void CreateSampleTable(Connection connection, TableName tableName)
        {
            connection.ExecuteCommand($"DROP TABLE IF EXISTS {tableName}");
            TableDefinition schema = GetSampleSchema(tableName);
            CreateTable(connection, schema, GetSampleData());
        }

        public void AssertEqualTableDefinitions(TableDefinition expected, TableDefinition actual)
        {
            Assert.AreEqual(expected.TableName, actual.TableName);
            List<TableDefinition.Column> expectedColumns = expected.Columns.ToList();
            List<TableDefinition.Column> actualColumns = actual.Columns.ToList();
            Assert.AreEqual(expectedColumns.Count, actualColumns.Count);
            for (int i = 0; i < expectedColumns.Count; ++i)
            {
                TableDefinition.Column colExpected = expectedColumns[i];
                TableDefinition.Column colActual = actualColumns[i];
                Assert.AreEqual(colExpected.Name, colActual.Name);
                Assert.AreEqual(colExpected.Type.Tag, colActual.Type.Tag);
                Assert.AreEqual(colExpected.Type.InternalOid, colActual.Type.InternalOid);
                Assert.AreEqual(colExpected.Collation ?? "", colActual.Collation ?? "");
                Assert.AreEqual(colExpected.Nullability, colActual.Nullability);
                Assert.AreEqual(colExpected.Type.Precision, colActual.Type.Precision);
                Assert.AreEqual(colExpected.Type.Scale, colActual.Type.Scale);
                Assert.AreEqual(colExpected.Type.MaxLength, colActual.Type.MaxLength);
            }
        }

        public void AssertEqualResultSchemas(TableDefinition expected, ResultSchema actual)
        {
            List<TableDefinition.Column> expectedColumns = new List<TableDefinition.Column>(expected.Columns);
            List<ResultSchema.Column> actualColumns = new List<ResultSchema.Column>(actual.Columns);
            Assert.AreEqual(expectedColumns.Count, actualColumns.Count);
            for (int i = 0; i < expectedColumns.Count; ++i)
            {
                TableDefinition.Column colExpected = expectedColumns[i];
                ResultSchema.Column colActual = actualColumns[i];
                Assert.AreEqual(colExpected.Name, colActual.Name);
                Assert.AreEqual(colExpected.Type.Tag, colActual.Type.Tag);
            }
        }

        public TableDefinition GetAllDataTypesSchema(TableName tableName)
        {
            return new TableDefinition(tableName)
                .AddColumn("bool", SqlType.Bool(), Nullability.NotNullable)
                .AddColumn("booln", SqlType.Bool())
                .AddColumn("short", SqlType.SmallInt(), Nullability.NotNullable)
                .AddColumn("shortn", SqlType.SmallInt())
                .AddColumn("int", SqlType.Int(), Nullability.NotNullable)
                .AddColumn("intn", SqlType.Int())
                .AddColumn("long", SqlType.BigInt(), Nullability.NotNullable)
                .AddColumn("longn", SqlType.BigInt())
                .AddColumn("double", SqlType.Double(), Nullability.NotNullable)
                .AddColumn("doublen", SqlType.Double())
                .AddColumn("oid", SqlType.Oid(), Nullability.NotNullable)
                .AddColumn("oidn", SqlType.Oid())
                .AddColumn("text", SqlType.Text(), Nullability.NotNullable)
                .AddColumn("textn", SqlType.Text())
                .AddColumn("bytes", SqlType.Bytes(), Nullability.NotNullable)
                .AddColumn("bytesn", SqlType.Bytes())
                .AddColumn("time", SqlType.Time(), Nullability.NotNullable)
                .AddColumn("timen", SqlType.Time())
                .AddColumn("date", SqlType.Date(), Nullability.NotNullable)
                .AddColumn("daten", SqlType.Date())
                .AddColumn("timestamp", SqlType.Timestamp(), Nullability.NotNullable)
                .AddColumn("timestampn", SqlType.Timestamp())
                .AddColumn("timestamptz", SqlType.TimestampTZ(), Nullability.NotNullable)
                .AddColumn("timestamptzn", SqlType.TimestampTZ())
                .AddColumn("interval", SqlType.Interval(), Nullability.NotNullable)
                .AddColumn("intervaln", SqlType.Interval())
                .AddColumn("char", SqlType.Char(6), Nullability.NotNullable)
                .AddColumn("charn", SqlType.Char(6))
                .AddColumn("char1", SqlType.Char(1), Nullability.NotNullable)
                .AddColumn("char1n", SqlType.Char(1))
                .AddColumn("varchar", SqlType.Varchar(6), Nullability.NotNullable)
                .AddColumn("varcharn", SqlType.Varchar(6))
                .AddColumn("numeric", SqlType.Numeric(18, 2), Nullability.NotNullable)
                .AddColumn("numericn", SqlType.Numeric(18, 2))
                .AddColumn("numeric128", SqlType.Numeric(38, 20), Nullability.NotNullable)
                .AddColumn("numeric128n", SqlType.Numeric(38, 20))
                .AddColumn("json", SqlType.Json(), Nullability.NotNullable)
                .AddColumn("jsonn", SqlType.Json())
                .AddColumn("geography", SqlType.Geography(), Nullability.NotNullable)
                .AddColumn("geographyn", SqlType.Geography())
                ;
        }
    }
}
