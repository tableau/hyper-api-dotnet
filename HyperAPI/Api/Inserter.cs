using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tableau.HyperAPI.Impl;
using Tableau.HyperAPI.Raw;

namespace Tableau.HyperAPI
{
    /// <summary>
    /// Writes data into a database table.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>Inserter</c> is used to append data to a database table. To do it, first create
    /// an inserter, then add data to it, then call <see cref="Execute"/>. Data is
    /// added row by row, with <c>Add</c> methods such as <see cref="Add(int)"/> to push individual values,
    /// and <see cref="EndRow"/> call after each row.
    /// </para>
    /// <para>
    /// Instead of individual <c>Add</c> methods one can use <see cref="AddRow(object[])"/> or
    /// <see cref="AddRows(IEnumerable{object[]})"/>. In this case <c>null</c> value corresponds
    /// to database NULL. This is slightly less performant since it involves boxing and unboxing values.
    /// <see cref="EndRow"/> does not need to be called after <see cref="AddRow(object[])"/>.
    /// </para>
    /// <para>
    /// If <see cref="Execute"/> is not called or if it fails, then the table data remains
    /// unchanged.
    /// </para>
    /// <para>
    /// <c>Inserter</c> object is open until it is closed with <see cref="Close"/>, <see cref="Dispose()"/>,
    /// or <see cref="Execute"/> method. An open <c>Inserter</c> object keeps the connection busy,
    /// and no query methods can be called on it until the inserter is closed. Similarly, two <c>Inserter</c>
    /// objects can not be used on the same connection simultaneously.
    /// </para>
    /// </remarks>
    public sealed class Inserter : IDisposable
    {
        private const ulong DEFAULT_CHUNK_SIZE = 16 * 1024 * 1024;
        // Buffer is initially DEFAULT_CHUNK_SIZE + DEFAULT_EXTRA_SIZE big, and we flush when the size
        // is >= ChunkSize, this way we lower the probability of reallocation.
        private const ulong DEFAULT_EXTRA_SIZE = 10 * 1024;

        private Raw.InserterHandle nativeHandle;
        private RowBuffer buffer;
        private bool flushedAnything;

        private TableDefinition streamDefinition;
        private string selectList;

        // These are for testing
        internal ulong ChunkSize { get; set; }
        internal int FlushCount { get; private set; }

        /// <summary>
        /// Internal property, do not use it. It may change or be removed in the future releases.
        /// </summary>
        // Setting it to true forces bulk insert mode. It's public because we need it for a bunch of tests in
        // HyperAPITest, HyperAPIInternalsTest is not enough.
        public bool InternalFB { get; set; }

        private class ColumnInfo
        {
            public Name Name;
            public SqlType ColType;
            public bool IsNullable;

            public ColumnInfo(TableDefinition.Column column)
            {
                Name = column.Name;
                ColType = column.Type;
                IsNullable = column.Nullability == Nullability.Nullable;
            }
        }

         /// <summary>
         /// Defines how target column is mapped or computed from inserter definition.
         /// </summary>
        public class ColumnMapping
        {

            /// <summary>
            /// Column name
            /// </summary>
            public Name ColumnName { get; private set; }
            /// <summary>
            /// Expression
            /// <p>
            /// The SQL expression specified for the column is used to transform or compute values on the fly during insertion.
            /// If not set, the data for this column must be provided using Add() calls during insertion
            /// </p>
            /// </summary>
            public string Expression { get; private set; }

            /// <summary>
            /// Creates a ColumnMapping
            /// </summary>
            /// <param name="columnName">Name of the column</param>
            /// <param name="expression">Expression </param>
            public ColumnMapping(Name columnName, string expression = null)
            {
               Util.CheckArgument(columnName != null, "name must be specified");
               ColumnName = columnName;
               Expression = expression;
            }

            /// <summary>
            /// Converts a <see cref="Name"/> to a <see cref="ColumnMapping"/>.
            /// </summary>
            /// <param name="identifier">Identifier to convert.</param>
            public static implicit operator ColumnMapping(Name identifier)
            {
               return new ColumnMapping(identifier);
            }

            /// <summary>
            /// Returns the fully escaped and quoted expression. For e.g CAST("text_column" AS TIMESTAMP) as "timestamp_column";
            /// </summary>
            internal string AsSelectListExpression()
            {
               if (Expression != null) {
                  return Expression + " AS " + ColumnName.ToString();
               } else {
                  return ColumnName.ToString();
               }
            }
        }

        private int columnCount;
        private ColumnInfo[] columns;
        private int currentColumn;

        /// <summary>
        /// Creates an inserter to append data to a table.
        /// </summary>
        /// <param name="connection">Connection to use.</param>
        /// <param name="schema">Table schema. It must match exactly the target table schema. Use the overload which
        /// takes the table name as a parameter if you don't have the schema.</param>
        /// <param name="columnNames">If specified, list of the names of the columns for which values will be inserted.</param>
        /// <remarks>
        /// Sequence of <c>Add</c> methods must match the <c>columns</c> argument if it's specified, or
        /// the columns in the table, in that exact order. Values may not be skipped. Use <see cref="AddNull"/>
        /// to insert a NULL value.
        /// </remarks>
        public Inserter(Connection connection, TableDefinition schema, IEnumerable<string> columnNames = null)
        {
            if (columnNames != null)
                schema = schema.ForColumnList(columnNames);

            streamDefinition = schema;
            Raw.MutableTableDefinitionHandle streamDefHandle = streamDefinition.CreateNativeTableDefinition();
            try
            {
                nativeHandle = connection.NativeHandle.CreateInserter(streamDefHandle);
            }
            catch
            {
                streamDefHandle.Dispose();
                throw;
            }

            try
            {
                InitBufferAndInserterColumns();
                // Build the selectList required for bulk insert
                selectList = string.Join(", ", schema.Columns.Select( c => c.Name.ToString()));
            }
            catch
            {
                nativeHandle.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Creates an inserter to append data to a table.
        /// </summary>
        /// <param name="connection">Connection to use.</param>
        /// <param name="tableName">Table name.</param>
        /// <param name="columns">If specified, list of the columns for which values will be inserted.</param>
        /// <remarks>
        /// Sequence of <c>Add</c> methods must match the <c>columns</c> argument if it's specified, or
        /// the columns in the table, in that exact order. Values may not be skipped. Use <see cref="AddNull"/>
        /// to insert a NULL value.
        /// </remarks>
        public Inserter(Connection connection, TableName tableName, IEnumerable<string> columns = null)
            : this(connection, connection.Catalog.GetTableDefinition(tableName), columns)
        {
        }

        /// <summary>
        /// Creates an inserter to append data to a table.
        /// Note: SQL expression provided during insertion are used without any modification during insertion and hence vulnerable to SQL injection attacks.
        /// Applications should prevent end-users from providing expressions directly during insertion
        /// </summary>
        /// <param name="connection">The connection to the Hyper instance containing the table.</param>
        /// <param name="tableName">The table name into which the data is inserted.</param>
        /// <param name="columnMappings">The set of columns in which to insert. The columns have to exist in the table. The columnMappings cannot be empty.
        /// Columns not present in columMappings will be set to their default value.
        /// A columnMapping can optionally contain a valid SQL expression.
        /// The SQL expression specified for the column is used to transform or compute values on the fly during insertion.
        /// The SQL expression can depend on columns that exist in the table.
        /// The SQL expression can also depend on values or columns that do not exist in the table, but must be specified in the `inserterDefinition`.</param>
        /// <param name="inserterDefinition"> The definition of columns to which values are provided.
        /// The column definition for all the columns without SQL expression must be specified in `inserterDefinition`.
        /// For a column without SQL expression the column definition provided in `inserterDefinition` must match the actual definition of the column in the table.
        /// All columns that SQL expressions specified in `columnMappings` must be in `inserterDefinition`.</param>
        ///
        /// <remarks>
        /// <p>
        /// Sequence of <c>Add</c> methods must match the <c>inserterDefinition</c> argument in that exact order.
        /// Values may not be skipped. Use <see cref="AddNull"/> to insert a NULL value.
        /// Consider the following pseudo-code on how to transform or compute values on the fly during insertion:
        /// TableDefinition    : [TableName="example", Columns=["ColumnA" as INT, "ColumnB" as BIG_INT]]
        /// ColumnMapping      : [[Name: "ColumnA"], [Name:"ColumnB", Expression:"ColumnA"*"ColumnC"]]
        /// InserterDefinition : ["ColumnA" integer, "ColumnC" integer]
        /// </p>
        /// <p>
        /// Notice that "ColumnA" does not specify an expression and "ColumnB" is a product of "ColumnA" and "ColumnC", but "ColumnC" is not part of the table.
        /// The InserterDefinition contains "ColumnA" and "ColumnC"
        /// "ColumnA" since it is not computed on the fly and has to be provided to the inserter
        /// "ColumnC" since it is specified in the SQL expression that computes "ColumnB" on the fly
        /// </p>
        /// <p>
        /// try (Inserter inserter(conn, "example", columnMapping, inserterDefinition)) {
        ///    inserter.add(2).add(3).endRow();
        ///    inserter.execute();
        /// }
        /// The insertion code snippet above inserts 2 into "ColumnA" and 6 into "ColumnB" (product of 2 and 3)
        /// </p>
        /// </remarks>
        public Inserter(Connection connection, TableName tableName, IEnumerable<Inserter.ColumnMapping> columnMappings, IEnumerable<TableDefinition.Column> inserterDefinition)
            : this(connection, connection.Catalog.GetTableDefinition(tableName), columnMappings, inserterDefinition)
        {
        }

        /// <summary>
        /// Creates an inserter to append data to a table.
        /// This overload allows user to specify SQL expressions for some or all of the columns during insertion 
        /// Note: SQL expression provided during insertion are used without any modification during insertion and hence vulnerable to SQL injection attacks.
        /// Applications should prevent end-users from providing expressions directly during insertion
        /// </summary>
        /// <param name="connection">The connection to the Hyper instance containing the table.</param>
        /// <param name="schema">The table definition for the table into which the data is inserted.</param>
        /// <param name="columnMappings">The set of columns in which to insert. The columns have to exist in the table. The columnMappings cannot be empty.
        /// Columns not present in columMappings will be set to their default value.
        /// A columnMapping can optionally contain a valid SQL expression.
        /// The SQL expression specified for the column is used to transform or compute values on the fly during insertion.
        /// The SQL expression can depend on columns that exist in the table.
        /// The SQL expression can also depend on values or columns that do not exist in the table, but must be specified in the `inserterDefinition`.</param>
        /// <param name="inserterDefinition"> The definition of columns to which values are provided.
        /// The column definition for all the columns without SQL expression must be specified in `inserterDefinition`.
        /// For a column without SQL expression the column definition provided in `inserterDefinition` must match the actual definition of the column in the table.
        /// All columns that SQL expressions specified in `columnMappings` must be in `inserterDefinition`.</param>
        ///
        /// <remarks>
        /// <p>
        /// Sequence of <c>Add</c> methods must match the <c>inserterDefinition</c> argument in that exact order.
        /// Values may not be skipped. Use <see cref="AddNull"/> to insert a NULL value.
        /// Consider the following pseudo-code on how to transform or compute values on the fly during insertion:
        /// TableDefinition    : [TableName="example", Columns=["ColumnA" as INT, "ColumnB" as BIG_INT]]
        /// ColumnMapping      : [[Name: "ColumnA"], [Name:"ColumnB", Expression:"ColumnA"*"ColumnC"]]
        /// InserterDefinition : ["ColumnA" integer, "ColumnC" integer]
        /// </p>
        /// <p>
        /// Notice that "ColumnA" does not specify an expression and "ColumnB" is a product of "ColumnA" and "ColumnC", but "ColumnC" is not part of the table.
        /// The InserterDefinition contains "ColumnA" and "ColumnC"
        /// "ColumnA" since it is not computed on the fly and has to be provided to the inserter
        /// "ColumnC" since it is specified in the SQL expression that computes "ColumnB" on the fly
        /// </p>
        /// <p>
        /// try (Inserter inserter(conn, "example", columnMapping, inserterDefinition)) {
        ///    inserter.add(2).add(3).endRow();
        ///    inserter.execute();
        /// }
        /// The insertion code snippet above inserts 2 into "ColumnA" and 6 into "ColumnB" (product of 2 and 3)
        /// </p>
        /// </remarks>
        public Inserter(Connection connection, TableDefinition schema, IEnumerable<Inserter.ColumnMapping> columnMappings, IEnumerable<TableDefinition.Column> inserterDefinition)
        {
            Util.CheckArgument(columnMappings != null && columnMappings.Count() != 0, "columnMappings cannot be empty");

            schema = schema.ForColumnMappings(columnMappings);

            streamDefinition = new TableDefinition(schema.TableName, inserterDefinition, schema.Persistence);

            VerifyColumnMappingInputs(columnMappings, schema);

            Raw.MutableTableDefinitionHandle rawTableDef = schema.CreateNativeTableDefinition();

            try
            {
                nativeHandle = connection.NativeHandle.CreateInserter(rawTableDef);
            }
            catch
            {
                rawTableDef.Dispose();
                throw;
            }

            Raw.MutableTableDefinitionHandle streamDefHandle = streamDefinition.CreateNativeTableDefinition();
            try
            {
                InitBufferAndInserterColumns();

                // Build the selectList required for bulk insert
                selectList = string.Join(", ", columnMappings.Select( c => c.AsSelectListExpression()));
                nativeHandle.InitBulkInsert(streamDefHandle, selectList);
                streamDefHandle.Dispose();
            }
            catch
            {
                nativeHandle.Dispose();
                streamDefHandle.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the inserter has not been closed.
        /// </summary>
        public bool IsOpen => nativeHandle != null;

        private void CheckIsOpen()
        {
            if (!IsOpen)
                throw new ObjectDisposedException("inserter", "Inserter object has been closed");
        }

        private void InitBufferAndInserterColumns()
        {
            buffer = new RowBuffer((int)(DEFAULT_CHUNK_SIZE + DEFAULT_EXTRA_SIZE));
            ChunkSize = DEFAULT_CHUNK_SIZE;
            columnCount = streamDefinition.ColumnCount;
            currentColumn = 0;
            columns = streamDefinition.Columns.Select(c => new ColumnInfo(c)).ToArray();
        }

        private void VerifyColumnMappingInputs(IEnumerable<Inserter.ColumnMapping> columnMappings, TableDefinition schema)
        {
            // Input Values for non-expression columns must be provided by the user
            // Verify that all non-expression columns are specified in the inserter definition
            // Verify that definition for non-expression column provided in the inserter definition matches the actual column definition
            int index=0;
            foreach (Inserter.ColumnMapping columnMapping in columnMappings)
            {
                if (columnMapping.Expression == null)
                {
                    TableDefinition.Column inserterColumn = streamDefinition.GetColumnByName(columnMapping.ColumnName);
                    Util.CheckArgument(inserterColumn != null, $"ColumnMapping {columnMapping.ColumnName} must contain an expression or defined in the inserter definition");
                    TableDefinition.Column targetColumn = schema.GetColumn(index);
                    if (targetColumn.Type != inserterColumn.Type || targetColumn.Nullability != inserterColumn.Nullability)
                    {
                        throw new ArgumentException($"Column definition for {columnMapping.ColumnName} does not match the definition provided in the inserter definition");
                    }
                }
                index++;
            }
        }

        /// <summary>
        /// Executes the insertion operation, i.e. actually inserts the data into the table. Without this
        /// call the data provided with <c>Add</c> methods will be discarded. This method closes the inserter,
        /// and the inserter can't be used afterwards, even if this method fails.
        /// </summary>
        public void Execute()
        {
            CheckIsOpen();

            Exception executeException = null;

            if (columnCount == 0 && !String.IsNullOrEmpty(selectList))
            {
                // No input values were provided by the user
                // Data to be inserted is computed by the expressions
                // For e.g `INSERT INTO table(A) SELECT generate_series(1,10)`
                try
                {
                    nativeHandle.InsertComputedExpressions(selectList);
                }
                catch(Exception ex)
                {
                    executeException = ex;
                }
            }
            else
            {

               try
               {
                   Flush();
               }
               catch (Exception ex)
               {
                   executeException = ex;
               }
            }

            try
            {
                nativeHandle.Close(insertData: true, inFinalizer: false);
            }
            catch (Exception ex)
            {
                if (executeException == null)
                    executeException = ex;
            }

            nativeHandle = null;

            Close();

            if (executeException != null)
                throw executeException;
        }

        /// <summary>
        /// Sends collected so far data to Hyper.
        /// </summary>
        private void Flush()
        {
            if (buffer.Size > 0)
            {
                if (!flushedAnything && (InternalFB || buffer.Size >= ChunkSize))
                {
                    Raw.MutableTableDefinitionHandle streamDefHandle = streamDefinition.CreateNativeTableDefinition();
                    try
                    {
                        nativeHandle.InitBulkInsert(streamDefHandle, selectList);
                        streamDefHandle.Dispose();
                    }
                    catch (Exception ex)
                    {
                        streamDefHandle.Dispose();
                        throw ex;
                    }

                }

                flushedAnything = true;
                nativeHandle.InsertChunk(buffer.Start, buffer.Size);
                buffer.Reset();
                FlushCount++;
            }
        }

        /// <summary>
        /// Call this after adding values for all columns in the current row.
        /// </summary>
        /// <remarks>
        /// Values must be added for all columns before calling this. Use <see cref="AddNull"/>
        /// to insert NULL values.
        /// </remarks>
        public void EndRow()
        {
            CheckIsOpen();

            try
            {
                if (currentColumn != columnCount)
                    throw new InvalidOperationException("Not all values have been written for the current row");

                if (buffer.Size >= ChunkSize)
                    Flush();
            }
            catch
            {
                Close();
                throw;
            }

            currentColumn = 0;
        }

        private Exception TypeMismatchException(string type, ColumnInfo columnInfo)
        {
            return new ArgumentException(
                $"attempted to insert value of type {type} into column '{columnInfo.Name}' of type " +
                $"{columnInfo.ColType.Tag.ToString()}");
        }

        private ColumnInfo GetCurrentColumnChecked()
        {
            CheckIsOpen();

            if (currentColumn >= columnCount)
            {
                Close();
                throw new InvalidOperationException("attempted to add more values to the current row than there are columns");
            }

            return columns[currentColumn];
        }

        /// <summary>
        /// Adds a NULL value.
        /// </summary>
        /// <returns>Inserter for method chaining</returns>
        public Inserter AddNull()
        {
            ColumnInfo columnInfo = GetCurrentColumnChecked();

            try
            {
                if (columnInfo.IsNullable)
                    buffer.WriteNull();
                else
                    throw new InvalidOperationException($"Attempted to insert NULL into a non-nullable column '{columnInfo.Name}'");
            }
            catch
            {
                Close();
                throw;
            }

            ++currentColumn;
            return this;
        }

        /// <summary>
        /// Adds a <c>bool</c> value.
        /// </summary>
        /// <param name="value">Value to add.</param>
        /// <returns>Inserter for method chaining</returns>
        /// <remarks>
        /// This method throws if the column is not of type <see cref="TypeTag.Bool"/>.
        /// </remarks>
        public Inserter Add(bool value)
        {
            ColumnInfo columnInfo = GetCurrentColumnChecked();

            try
            {
                switch (columnInfo.ColType.Tag)
                {
                    case TypeTag.Bool:
                        buffer.WriteBool(value, columnInfo.IsNullable);
                        break;

                    default:
                        throw TypeMismatchException("bool", columnInfo);
                }
            }
            catch
            {
                Close();
                throw;
            }

            ++currentColumn;
            return this;
        }

        /// <summary>
        /// Adds a <c>short</c> value.
        /// </summary>
        /// <param name="value">Value to add.</param>
        /// <returns>Inserter for method chaining</returns>
        /// <remarks>
        /// This method throws if the column is not of type <see cref="TypeTag.SmallInt"/>, <see cref="TypeTag.Int"/>, <see cref="TypeTag.BigInt"/>, <see cref="TypeTag.Numeric"/> or <see cref="TypeTag.Double"/>.
        /// </remarks>
        public Inserter Add(short value)
        {
            ColumnInfo columnInfo = GetCurrentColumnChecked();

            try
            {
                switch (columnInfo.ColType.Tag)
                {
                    case TypeTag.SmallInt:
                        buffer.WriteShort(value, columnInfo.IsNullable);
                        break;

                    case TypeTag.Int:
                        buffer.WriteInt(value, columnInfo.IsNullable);
                        break;

                    case TypeTag.BigInt:
                        buffer.WriteLong(value, columnInfo.IsNullable);
                        break;

                    case TypeTag.Numeric:
                        WriteNumeric(buffer, value, columnInfo);
                        break;

                    case TypeTag.Double:
                        buffer.WriteDouble(value, columnInfo.IsNullable);
                        break;

                    default:
                        throw TypeMismatchException("short", columnInfo);
                }
            }
            catch
            {
                Close();
                throw;
            }

            ++currentColumn;
            return this;
        }

        /// <summary>
        /// Adds an <c>int</c> value.
        /// </summary>
        /// <param name="value">Value to add.</param>
        /// <returns>Inserter for method chaining</returns>
        /// <remarks>
        /// This method throws if the column is not of type <see cref="TypeTag.SmallInt"/>, <see cref="TypeTag.Int"/>, <see cref="TypeTag.BigInt"/>, <see cref="TypeTag.Numeric"/> or <see cref="TypeTag.Double"/>.
        /// </remarks>
        public Inserter Add(int value)
        {
            ColumnInfo columnInfo = GetCurrentColumnChecked();

            try
            {
                switch (columnInfo.ColType.Tag)
                {
                    case TypeTag.Int:
                        buffer.WriteInt(value, columnInfo.IsNullable);
                        break;

                    case TypeTag.BigInt:
                        buffer.WriteLong(value, columnInfo.IsNullable);
                        break;

                    case TypeTag.Numeric:
                        WriteNumeric(buffer, value, columnInfo);
                        break;

                    case TypeTag.Double:
                        buffer.WriteDouble(value, columnInfo.IsNullable);
                        break;

                    default:
                        throw TypeMismatchException("int", columnInfo);
                }
            }
            catch
            {
                Close();
                throw;
            }

            ++currentColumn;
            return this;
        }

        /// <summary>
        /// Adds a <c>long</c> value.
        /// </summary>
        /// <param name="value">Value to add.</param>
        /// <returns>Inserter for method chaining</returns>
        /// <remarks>
        /// This method throws if the column is not of type <see cref="TypeTag.SmallInt"/>, <see cref="TypeTag.Int"/>, <see cref="TypeTag.BigInt"/>, <see cref="TypeTag.Numeric"/> or <see cref="TypeTag.Double"/>.
        /// </remarks>
        public Inserter Add(long value)
        {
            ColumnInfo columnInfo = GetCurrentColumnChecked();

            try
            {
                switch (columnInfo.ColType.Tag)
                {
                    case TypeTag.BigInt:
                        buffer.WriteLong(value, columnInfo.IsNullable);
                        break;

                    case TypeTag.Numeric:
                        WriteNumeric(buffer, value, columnInfo);
                        break;

                    case TypeTag.Double:
                        buffer.WriteDouble(value, columnInfo.IsNullable);
                        break;

                    default:
                        throw TypeMismatchException("long", columnInfo);
                }
            }
            catch
            {
                Close();
                throw;
            }

            ++currentColumn;
            return this;
        }

        /// <summary>
        /// Adds a <c>uint</c> value.
        /// </summary>
        /// <param name="value">Value to add.</param>
        /// <returns>Inserter for method chaining</returns>
        /// <remarks>
        /// This method throws if the column is not of type <see cref="TypeTag.Oid"/>.
        /// </remarks>
        public Inserter Add(uint value)
        {
            ColumnInfo columnInfo = GetCurrentColumnChecked();

            try
            {
                switch (columnInfo.ColType.Tag)
                {
                    case TypeTag.Oid:
                        buffer.WriteUint(value, columnInfo.IsNullable);
                        break;

                    default:
                        throw TypeMismatchException("uint", columnInfo);
                }
            }
            catch
            {
                Close();
                throw;
            }

            ++currentColumn;
            return this;
        }

        /// <summary>
        /// Adds a <c>decimal</c> value.
        /// </summary>
        /// <param name="value">Value to add.</param>
        /// <remarks>
        /// This method throws if the column is not of type <see cref="TypeTag.Numeric"/> or <see cref="TypeTag.Double"/>.
        /// </remarks>
        public Inserter Add(decimal value)
        {
            ColumnInfo columnInfo = GetCurrentColumnChecked();

            try
            {
                switch (columnInfo.ColType.Tag)
                {
                    case TypeTag.Numeric:
                        WriteNumeric(buffer, value, columnInfo);
                        break;

                    case TypeTag.Double:
                        buffer.WriteDouble((double)value, columnInfo.IsNullable);
                        break;

                    default:
                        throw TypeMismatchException("decimal", columnInfo);
                }
            }
            catch
            {
                Close();
                throw;
            }

            ++currentColumn;
            return this;
        }

        /// <summary>
        /// Adds a <c>double</c> value.
        /// </summary>
        /// <param name="value">Value to add.</param>
        /// <returns>Inserter for method chaining</returns>
        /// <remarks>
        /// This method throws if the column is not of type <see cref="TypeTag.Double"/>.
        /// </remarks>
        public Inserter Add(double value)
        {
            ColumnInfo columnInfo = GetCurrentColumnChecked();

            try
            {
                switch (columnInfo.ColType.Tag)
                {
                    case TypeTag.Double:
                        buffer.WriteDouble(value, columnInfo.IsNullable);
                        break;

                    default:
                        throw TypeMismatchException("double", columnInfo);
                }
            }
            catch
            {
                Close();
                throw;
            }

            ++currentColumn;
            return this;
        }

        /// <summary>
        /// Adds a  <see cref="Date"/> value.
        /// </summary>
        /// <param name="value">Value to add.</param>
        /// <returns>Inserter for method chaining</returns>
        /// <remarks>
        /// This method throws if the column is not of type <see cref="TypeTag.Date"/>.
        /// </remarks>
        public Inserter Add(Date value)
        {
            ColumnInfo columnInfo = GetCurrentColumnChecked();

            try
            {
                switch (columnInfo.ColType.Tag)
                {
                    case TypeTag.Date:
                        buffer.WriteDate(value, columnInfo.IsNullable);
                        break;

                    default:
                        throw TypeMismatchException("Date", columnInfo);
                }
            }
            catch
            {
                Close();
                throw;
            }

            ++currentColumn;
            return this;
        }

        /// <summary>
        /// Adds a TimeSpan value.
        /// </summary>
        /// <param name="value">Value to add.</param>
        /// <returns>Inserter for method chaining</returns>
        /// <remarks>
        /// This method throws if the column is not of type <see cref="TypeTag.Time"/>.
        /// </remarks>
        public Inserter Add(TimeSpan value)
        {
            ColumnInfo columnInfo = GetCurrentColumnChecked();

            try
            {
                switch (columnInfo.ColType.Tag)
                {
                    case TypeTag.Time:
                        buffer.WriteTime(value, columnInfo.IsNullable);
                        break;

                    default:
                        throw TypeMismatchException("TimeSpan", columnInfo);
                }
            }
            catch
            {
                Close();
                throw;
            }

            ++currentColumn;
            return this;
        }

        /// <summary>
        /// Adds a  <see cref="Timestamp"/>  value.
        /// </summary>
        /// <param name="value">Value to add.</param>
        /// <returns>Inserter for method chaining</returns>
        /// <remarks>
        /// <para>
        /// <c>Timestamp</c> does not contain time zone information. However, the <c>Kind</c> property
        /// indicates whether the value represents local time, Coordinated Universal Time (UTC), or neither.
        /// For a <c>TIMESTAMP_TZ</c> column and <c>Kind</c> set to <c>Utc</c>, the given value is
        /// interpreted as UTC and stored as is. When <c>Kind</c> is set to <c>Local</c> or <c>Unspecified</c>
        /// the value is interpreted in the local time zone, converted to UTC, and stored.
        /// For a <c>TIMESTAMP</c> column, the value is stored as is regardless of <c>Kind</c>.
        /// </para>
        /// <para>
        /// This method throws if the column is not of type <see cref="TypeTag.Timestamp"/> or <see cref="TypeTag.TimestampTZ"/>.
        /// </para>
        /// </remarks>
        public Inserter Add(Timestamp value)
        {
            ColumnInfo columnInfo = GetCurrentColumnChecked();

            try
            {
                switch (columnInfo.ColType.Tag)
                {
                    case TypeTag.Timestamp:
                        buffer.WriteTimestamp(value, columnInfo.IsNullable);
                        break;
                    case TypeTag.TimestampTZ:
                        buffer.WriteTimestamp(value.ToUniversalTime(), columnInfo.IsNullable);
                        break;

                    default:
                        throw TypeMismatchException("Timestamp", columnInfo);
                }
            }
            catch
            {
                Close();
                throw;
            }

            ++currentColumn;
            return this;
        }

        /// <summary>
        /// Adds a  <see cref="DateTime"/>  value.
        /// </summary>
        /// <param name="value">Value to add.</param>
        /// <returns>Inserter for method chaining</returns>
        /// <remarks>
        /// <para>
        /// <c>DateTime</c> does not contain time zone information. However, the <c>Kind</c> property
        /// indicates whether the value represents local time, Coordinated Universal Time (UTC), or neither.
        /// For a <c>TIMESTAMP_TZ</c> column and <c>Kind</c> set to <c>Utc</c>, the given value is
        /// interpreted as UTC and stored as is. When <c>Kind</c> is set to <c>Local</c> or <c>Unspecified</c>
        /// the value is interpreted in the local time zone, converted to UTC, and stored.
        /// For a <c>TIMESTAMP</c> column, the value is stored as is regardless of <c>Kind</c>.
        /// </para>
        /// <para>
        /// This method throws if the column is not of type <see cref="TypeTag.Timestamp"/> or <see cref="TypeTag.TimestampTZ"/>.
        /// </para>
        /// </remarks>
        public Inserter Add(DateTime value)
        {
            ColumnInfo columnInfo = GetCurrentColumnChecked();

            try
            {
                switch (columnInfo.ColType.Tag)
                {
                    case TypeTag.Timestamp:
                        buffer.WriteTimestamp((Timestamp)value, columnInfo.IsNullable);
                        break;
                    case TypeTag.TimestampTZ:
                        buffer.WriteTimestamp((Timestamp)value.ToUniversalTime(), columnInfo.IsNullable);
                        break;

                    default:
                        throw TypeMismatchException("DateTime", columnInfo);
                }
            }
            catch
            {
                Close();
                throw;
            }

            ++currentColumn;
            return this;
        }

        /// <summary>
        /// Adds a  <see cref="DateTimeOffset"/>  value.
        /// </summary>
        /// <param name="value">Value to add.</param>
        /// <returns>Inserter for method chaining</returns>
        /// <remarks>
        /// <para>
        /// <c>DateTimeOffset</c> is time-zone-aware. It represents a date and time value together with a time zone offset.
        /// Thus, the value always unambiguously identifies a single point in time.
        /// For a <c>TIMESTAMP_TZ</c> column, the value is converted to UTC using its time zone offset and stored.
        /// </para>
        /// <para>
        /// This method throws if the column is not of type <see cref="TypeTag.TimestampTZ"/>.
        /// </para>
        /// </remarks>
        public Inserter Add(DateTimeOffset value)
        {
            ColumnInfo columnInfo = GetCurrentColumnChecked();

            try
            {
                switch (columnInfo.ColType.Tag)
                {
                    case TypeTag.TimestampTZ:
                        buffer.WriteTimestamp((Timestamp)value.UtcDateTime, columnInfo.IsNullable);
                        break;

                    default:
                        throw TypeMismatchException("DateTimeOffset", columnInfo);
                }
            }
            catch
            {
                Close();
                throw;
            }

            ++currentColumn;
            return this;
        }

        /// <summary>
        /// Adds a <see cref="Interval"/> value.
        /// </summary>
        /// <param name="value">Value to add.</param>
        /// <returns>Inserter for method chaining</returns>
        /// <remarks>
        /// This method throws if the column is not of type <see cref="TypeTag.Interval"/>.
        /// </remarks>
        public Inserter Add(Interval value)
        {
            ColumnInfo columnInfo = GetCurrentColumnChecked();

            try
            {
                switch (columnInfo.ColType.Tag)
                {
                    case TypeTag.Interval:
                        buffer.WriteInterval(value, columnInfo.IsNullable);
                        break;

                    default:
                        throw TypeMismatchException("Interval", columnInfo);
                }
            }
            catch
            {
                Close();
                throw;
            }

            ++currentColumn;
            return this;
        }

        /// <summary>
        /// Adds a <c>string</c> value.
        /// </summary>
        /// <param name="value">String to insert. <c>null</c> value is not allowed, use
        /// <see cref="AddNull()"/> to insert a NULL value.</param>
        /// <param name="start">Copy the string starting from this character. <c>0</c> by default.</param>
        /// <param name="count">Copy this many characters. Negative value means copy to the end of the string.
        /// <c>-1</c> by default.</param>
        /// <returns>Inserter for method chaining</returns>
        /// <remarks>
        /// This method throws if the column is not of type <see cref="TypeTag.Char"/>, <see cref="TypeTag.Varchar"/>, <see cref="TypeTag.Text"/> or <see cref="TypeTag.Json"/>
        /// </remarks>
        public Inserter Add(string value, int start = 0, int count = -1)
        {
            ColumnInfo columnInfo = GetCurrentColumnChecked();

            try
            {
                switch (columnInfo.ColType.Tag)
                {
                    case TypeTag.Char:
                    case TypeTag.Varchar:
                    case TypeTag.Text:
                    case TypeTag.Json:
                        WriteString(buffer, value, start, count, columnInfo.IsNullable);
                        break;

                    default:
                        throw TypeMismatchException("string", columnInfo);
                }
            }
            catch
            {
                Close();
                throw;
            }

            ++currentColumn;
            return this;
        }

        /// <summary>
        /// Adds a byte[] value.
        /// </summary>
        /// <param name="value">Byte array to copy data from. <c>null</c> value is not allowed, use
        /// <see cref="AddNull()"/> to insert a NULL value.</param>
        /// <param name="start">Copy starting from this position. <c>0</c> by default.</param>
        /// <param name="count">Copy this many bytes. Negative value means copy to the end of the
        /// array. <c>-1</c> by default.</param>
        /// <returns>Inserter for method chaining</returns>
        /// <remarks>
        /// This method throws if the column is not of type <see cref="TypeTag.Bytes"/>, <see cref="TypeTag.Geography"/>, <see cref="TypeTag.Char"/>, <see cref="TypeTag.Varchar"/>, <see cref="TypeTag.Text"/> or <see cref="TypeTag.Json"/>
        /// This method throws if the column is of type <c> Char(1) </c>
        /// </remarks>
        public Inserter Add(byte[] value, int start = 0, int count = -1)
        {
            ColumnInfo columnInfo = GetCurrentColumnChecked();

            try
            {
                switch (columnInfo.ColType.Tag)
                {
                    case TypeTag.Char:
                    case TypeTag.Varchar:
                    case TypeTag.Text:
                    case TypeTag.Json:
                    case TypeTag.Bytes:
                    case TypeTag.Geography:
                        WriteBytes(buffer, value, start, count, columnInfo.IsNullable);
                        break;

                    default:
                        throw TypeMismatchException("byte[]", columnInfo);
                }
            }
            catch
            {
                Close();
                throw;
            }

            ++currentColumn;
            return this;
        }

        /// <summary>
        /// Adds a value.
        /// </summary>
        /// <param name="value">Value to add. <c>null</c> corresponds to the NULL database value.
        /// If not <c>null</c> then the value must have the correct type for the current column.
        /// See <see cref="TypeTag"/> documentation for how Hyper data types map to C# types.</param>
        /// <returns>Inserter for method chaining</returns>
        public Inserter Add(object value)
        {
            if (value == null)
                return AddNull();

            return AddNonNullValue(value);
        }

        /// <summary>
        /// Adds a row of data represented as an array of objects.
        /// </summary>
        /// <param name="row">Values to add.</param>
        /// <returns>Inserter for method chaining</returns>
        /// <remarks>
        /// <c>null</c> values correspond to NULL database values.
        /// If not <c>null</c> then the values must have correct types for the corresponding columns.
        /// <para>
        /// <see cref="EndRow"/> does not need to be called after this method.
        /// </para>
        /// </remarks>
        public void AddRow(params object[] row)
        {
            AddRow(row, 0);
        }

        /// <summary>
        /// Adds a row of data represented as a subarray of an array of objects.
        /// </summary>
        /// <param name="row">Values to add.</param>
        /// <param name="start">Add values starting from this position.</param>
        /// <returns>Inserter for method chaining</returns>
        /// <remarks>
        /// <c>null</c> values correspond to NULL database values.
        /// If not <c>null</c> then the values must have correct types for the corresponding columns.
        /// <para>
        /// <see cref="EndRow"/> does not need to be called after this method.
        /// </para>
        /// </remarks>
        public void AddRow(object[] row, int start)
        {
            try
            {
                Util.CheckArgument(row != null, "row must not be null");
                Util.CheckArgument(start >= 0, "starting position must be non-negative");
                Util.CheckArgument(start + columnCount <= row.Length, "there are not enough values to insert");

                if (currentColumn != 0)
                    throw new InvalidOperationException("AddRow() may not be mixed with individual Add*() methods.");

                for (int i = start; i < start + columnCount; ++i)
                    Add(row[i]);

                EndRow();
            }
            catch
            {
                Close();
                throw;
            }
        }

        /// <summary>
        /// Adds rows of data. See <seealso cref="AddRow(object[])"/>.
        /// </summary>
        /// <param name="rows">Rows to add.</param>
        /// <returns>Inserter for method chaining</returns>
        public void AddRows(IEnumerable<object[]> rows)
        {
            foreach (object[] row in rows)
                AddRow(row);
        }

        private static void WriteNumeric(RowBuffer buffer, decimal value, ColumnInfo columnInfo)
        {
            buffer.WriteNumeric(value, columnInfo.ColType, columnInfo.IsNullable);
        }

        private static void CheckAndPrepareStringParams(string v, int start, ref int count)
        {
            Util.CheckArgument(v != null, "string must not be null");
            Util.CheckArgument(start >= 0 && start <= v.Length, "start must not be negative or greater than the string length");

            if (count < 0)
            {
                count = v.Length - start;
            }
            else
            {
                Util.CheckArgument(count <= v.Length && v.Length - count >= start, "start + count must not be greater than the string length");
            }
        }

        private static void CheckAndPrepareBytesParams(byte[] v, int start, ref int count)
        {
            Util.CheckArgument(v != null, "buffer must not be null");
            Util.CheckArgument(start >= 0 && start <= v.Length, "start must not be negative or greater than the buffer size");

            if (count < 0)
            {
                count = v.Length - start;
            }
            else
            {
                Util.CheckArgument(count <= v.Length && v.Length - count >= start, "start + count must not be greater than the buffer size");
            }
        }

        private static void WriteBytes(RowBuffer buffer, byte[] v, int start, int count, bool isNullable)
        {
            CheckAndPrepareBytesParams(v, start, ref count);
            buffer.WriteBytes(v, start, count, isNullable);
        }

        private static void WriteString(RowBuffer buffer, string v, int start, int count, bool isNullable)
        {
            CheckAndPrepareStringParams(v, start, ref count);
            buffer.WriteString(v, start, count, isNullable);
        }

        private static void WriteValue(RowBuffer buffer, object value, ColumnInfo columnInfo)
        {
            switch (columnInfo.ColType.Tag)
            {
                case TypeTag.SmallInt:
                    buffer.WriteShort(DataConverter.ToShort(value), columnInfo.IsNullable);
                    break;

                case TypeTag.Int:
                    buffer.WriteInt(DataConverter.ToInt(value), columnInfo.IsNullable);
                    break;

                case TypeTag.BigInt:
                    buffer.WriteLong(DataConverter.ToLong(value), columnInfo.IsNullable);
                    break;

                case TypeTag.Double:
                    buffer.WriteDouble(DataConverter.ToDouble(value), columnInfo.IsNullable);
                    break;

                case TypeTag.Oid:
                    buffer.WriteUint(DataConverter.ToUint(value), columnInfo.IsNullable);
                    break;

                case TypeTag.Bool:
                    buffer.WriteBool(DataConverter.ToBool(value), columnInfo.IsNullable);
                    break;

                case TypeTag.Bytes:
                case TypeTag.Geography:
                    WriteBytes(buffer, DataConverter.ToBytes(value), 0, -1, columnInfo.IsNullable);
                    break;

                case TypeTag.Date:
                    buffer.WriteDate(DataConverter.ToDate(value), columnInfo.IsNullable);
                    break;

                case TypeTag.Interval:
                    buffer.WriteInterval(DataConverter.ToInterval(value), columnInfo.IsNullable);
                    break;

                case TypeTag.Json:
                case TypeTag.Text:
                case TypeTag.Varchar:
                case TypeTag.Char:
                    WriteString(buffer, DataConverter.ToString(value), 0, -1, columnInfo.IsNullable);
                    break;

                case TypeTag.Time:
                    buffer.WriteTime(DataConverter.ToTime(value), columnInfo.IsNullable);
                    break;

                case TypeTag.Timestamp:
                    buffer.WriteTimestamp(DataConverter.ToTimestamp(value), columnInfo.IsNullable);
                    break;

                case TypeTag.TimestampTZ:
                    buffer.WriteTimestamp(DataConverter.ToTimestampTz(value).ToUniversalTime(), columnInfo.IsNullable);
                    break;

                case TypeTag.Numeric:
                    WriteNumeric(buffer, DataConverter.ToNumeric(value), columnInfo);
                    break;

                default:
                    throw new NotImplementedException($"unhandled column type {columnInfo.ColType.Tag}");
            }
        }

        private Inserter AddNonNullValue(object value)
        {
            ColumnInfo columnInfo = GetCurrentColumnChecked();

            try
            {
                WriteValue(buffer, value, columnInfo);
            }
            catch
            {
                Close();
                throw;
            }

            ++currentColumn;
            return this;
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~Inserter()
        {
            Dispose(false);
        }

        /// <summary>
        /// Cleans up resources held by inserter. If <see cref="Execute"/> has not been called then the inserted
        /// data is discarded.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Closes the inserter, it discards written data if <see cref="Execute"/> has not been called.
        /// </summary>
        public void Close()
        {
            if (nativeHandle != null)
            {
                try
                {
                    nativeHandle.Close(insertData: false, inFinalizer: false);
                }
                catch (Exception ex)
                {
                    // This should not ever happen
                    Logger.Error("Error in InserterHandle.Close()", ex);
                }

                nativeHandle = null;
            }

            if (buffer != null)
            {
                buffer.Dispose();
                buffer = null;
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposing && nativeHandle != null)
                Util.WarnNotDisposedUserObject("Inserter object has not been closed. Close it with Execute() or Close() or use it in a 'using' statement.");

            if (disposing)
            {
                Close();
            }
        }
    }
}
