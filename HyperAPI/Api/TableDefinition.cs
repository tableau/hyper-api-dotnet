using System;
using System.Collections.Generic;
using System.Linq;

using Tableau.HyperAPI.Raw;
using Tableau.HyperAPI.Impl;

namespace Tableau.HyperAPI
{
    /// <summary>
    /// Constants which define whether a column may contain NULL values.
    /// </summary>
    public enum Nullability
    {
        /// <summary>
        /// Column is not nullable, i.e. it may not contain NULL values.
        /// </summary>
        NotNullable = 0,

        /// <summary>
        /// Column is nullable, i.e. it may contain NULL values.
        /// </summary>
        Nullable = 1,
    }

    /// <summary>
    /// Table persistence mode.
    /// </summary>
    public enum Persistence
    {
        /// <summary>
        /// Table is saved in the database on disk.
        /// </summary>
        Permanent = Dll.HYPER_PERMANENT,

        /// <summary>
        /// Table is temporary.
        /// </summary>
        Temporary = Dll.HYPER_TEMPORARY,
    }

    /// <summary>
    /// Schema of a database table.
    /// </summary>
    public class TableDefinition
    {
        /// <summary>
        /// Column of a database table.
        /// </summary>
        public class Column
        {
            /// <summary>
            /// Column name.
            /// </summary>
            public Name Name { get; }

            /// <summary>
            /// Column type.
            /// </summary>
            public SqlType Type { get; }

            /// <summary>
            /// Can this column contain NULL values?
            /// </summary>
            public Nullability Nullability { get; }

            /// <summary>
            /// Collation of a text column. By default it is the binary collation.
            /// </summary>
            public string Collation { get; }

            /// <summary>
            /// Creates a <see cref="Column"/> object.
            /// </summary>
            /// <param name="name">Column name.</param>
            /// <param name="type">Column type.</param>
            /// <param name="nullability">Can this column contain NULL values? <see cref="Nullability.Nullable"/> by default.</param>
            public Column(string name, SqlType type, Nullability nullability = Nullability.Nullable)
                : this(name, type, null, nullability)
            {
            }

            /// <summary>
            /// Creates a <see cref="Column"/> object.
            /// </summary>
            /// <param name="name">Column name.</param>
            /// <param name="type">Column type.</param>
            /// <param name="collation">Text column collation. <c>null</c> means binary collation.</param>
            /// <param name="nullability">Can this column contain NULL values? <see cref="Nullability.Nullable"/> by default.</param>
            public Column(string name, SqlType type, string collation, Nullability nullability = Nullability.Nullable)
            {
                Util.CheckArgument(!string.IsNullOrEmpty(name), "name may not be null or empty");
                Util.CheckArgument(string.IsNullOrEmpty(collation) || type.Tag == TypeTag.Char || type.Tag == TypeTag.Varchar || type.Tag == TypeTag.Text,
                    "collation may be used only with text types");
                Name = name;
                Type = type;
                Collation = collation;
                Nullability = nullability;
            }
        }

        /// <summary>
        /// Table name.
        /// </summary>
        public TableName TableName { get; set; }

        /// <summary>
        /// Table persistence mode.
        /// </summary>
        public Persistence Persistence { get; set; }

        private List<Column> columns = new List<Column>();

        /// <summary>
        /// Constructs a TableDefinition object.
        /// </summary>
        /// <param name="name">Table name.</param>
        /// <param name="persistence">Table persistence mode, <see cref="Persistence.Permanent"/> by default.</param>
        public TableDefinition(TableName name, Persistence persistence = Persistence.Permanent)
            : this(name, null, persistence)
        {
        }

        /// <summary>
        /// Constructs a TableDefinition object.
        /// </summary>
        /// <param name="name">Table name.</param>
        /// <param name="columns">Optional list of table columns.</param>
        /// <param name="persistence">Table persistence mode, <see cref="Persistence.Permanent"/> by default.</param>
        public TableDefinition(TableName name, IEnumerable<Column> columns, Persistence persistence = Persistence.Permanent)
        {
            Util.CheckArgument(name != null, "name must be specified");
            TableName = name;
            Persistence = persistence;
            if (columns != null)
                this.columns.AddRange(columns);
        }

        internal TableDefinition(TableDefinitionHandle raw)
        {
            string databaseName = raw.DatabaseName;
            string schemaName = raw.SchemaName;
            string tableName = raw.TableName;

            if (!string.IsNullOrEmpty(databaseName))
                TableName = new TableName(databaseName, schemaName, tableName);
            else if (!string.IsNullOrEmpty(schemaName))
                TableName = new TableName(schemaName, tableName);
            else
                TableName = new TableName(tableName);

            for (int i = 0; i < raw.ColumnCount; ++i)
            {
                string collation = raw.ColumnCollation(i);
                int tag = raw.ColumnTypeTag(i);

                columns.Add(new Column(raw.ColumnName(i), new SqlType(
                    (TypeTag)raw.ColumnTypeTag(i), raw.ColumnTypeModifier(i), (uint)raw.ColumnTypeOid(i)),
                    collation, raw.ColumnIsNullable(i) ? Nullability.Nullable : Nullability.NotNullable));
            }
        }

        /// <summary>
        /// Table columns.
        /// </summary>
        public IEnumerable<Column> Columns => columns;

        /// <summary>
        /// Gets the number of columns.
        /// </summary>
        public int ColumnCount => columns.Count;

        /// <summary>
        /// Gets the column by its position.
        /// </summary>
        /// <param name="pos">Column position.</param>
        /// <returns>The column.</returns>
        public Column GetColumn(int pos)
        {
            Util.CheckArgument<IndexOutOfRangeException>(pos >= 0 && pos < columns.Count,
                () => new IndexOutOfRangeException($"index must be between 0 and {columns.Count - 1}"));
            return columns[pos];
        }

        /// <summary>
        /// Gets the column by its name, return <c>null</c> if the column does not exist.
        /// </summary>
        /// <param name="name">Column name.</param>
        /// <returns>The column or <c>null</c> if it does not exist.</returns>
        public Column GetColumnByName(Name name)
        {
            return columns.FirstOrDefault(c => c.Name.Equals(name));
        }

        /// <summary>
        /// Gets the position of the column with the given name, or <c>-1</c> if it does not exist.
        /// </summary>
        /// <param name="name">Column name.</param>
        /// <returns>Column position or <c>-1</c> if it does not exist.</returns>
        public int GetColumnPosByName(Name name)
        {
            return columns.FindIndex(col => col.Name.Equals(name));
        }

        /// <summary>
        /// Adds a column.
        /// </summary>
        /// <param name="column">Column to add.</param>
        /// <returns>The <c>TableDefinition</c> object itself for chaining methods.</returns>
        public TableDefinition AddColumn(Column column)
        {
            Util.CheckArgument(column != null, "column must not be null");
            columns.Add(column);
            return this;
        }

        /// <summary>
        /// Adds a column.
        /// </summary>
        /// <param name="name">Column name.</param>
        /// <param name="type">Column type.</param>
        /// <param name="nullability">Can this column contain NULL values? <see cref="Nullability.Nullable"/> by default.</param>
        /// <returns>The <c>TableDefinition</c> object itself for chaining methods.</returns>
        public TableDefinition AddColumn(string name, SqlType type, Nullability nullability = Nullability.Nullable)
        {
            return AddColumn(new Column(name, type, nullability));
        }

        /// <summary>
        /// Adds a column.
        /// </summary>
        /// <param name="name">Column name.</param>
        /// <param name="type">Column type.</param>
        /// <param name="collation">Text column collation. <c>null</c> means binary collation.</param>
        /// <param name="nullability">Can this column contain NULL values? <see cref="Nullability.Nullable"/> by default.</param>
        /// <returns>The <c>TableDefinition</c> object itself for chaining methods.</returns>
        public TableDefinition AddColumn(string name, SqlType type, string collation, Nullability nullability = Nullability.Nullable)
        {
            return AddColumn(new Column(name, type, collation, nullability));
        }

        internal TableDefinition ForColumnList(IEnumerable<string> columnNames)
        {
            TableDefinition newSchema = new TableDefinition(TableName, Persistence);

            foreach (string name in columnNames)
            {
                Column col = GetColumnByName(name);
                if (col == null)
                    throw new ArgumentException($"Column '{name}' does not exist", "columnNames");
                if (newSchema.GetColumnByName(name) != null)
                    throw new ArgumentException($"Duplicate column '{name}'");
                newSchema.AddColumn(col);
            }
            Util.CheckArgument(newSchema.ColumnCount != 0, "Column Mappings cannot be empty");
            return newSchema;
        }

        // Build a subset of the current TableDefinition from columnMappings
        internal TableDefinition ForColumnMappings(IEnumerable<Inserter.ColumnMapping> columnMappings)
        {
            TableDefinition newSchema = new TableDefinition(TableName, Persistence);

            foreach (Inserter.ColumnMapping columnMapping in columnMappings)
            {
                Column col = GetColumnByName(columnMapping.ColumnName.Unescaped);
                if (col == null)
                {
                    throw new ArgumentException($"Column '{columnMapping.ColumnName}' does not exist", "columnMappings");
                }
                if (newSchema.GetColumnByName(columnMapping.ColumnName) != null)
                {
                    throw new ArgumentException($"Duplicate column '{columnMapping.ColumnName}' in columnMappings");
                }
                newSchema.AddColumn(col);
            }

            return newSchema;
        }

        internal MutableTableDefinitionHandle CreateNativeTableDefinition()
        {
            MutableTableDefinitionHandle raw = null;
            try
            {
                TableName.GetComponents(out string databaseName, out string schemaName, out string tableName);
                raw = new MutableTableDefinitionHandle(databaseName, schemaName, tableName, Persistence);
                foreach (Column col in columns)
                    raw.AddColumn(col.Name.Unescaped, (int)col.Type.Tag, col.Type.InternalTypeModifier, col.Collation, col.Nullability == Nullability.Nullable);
                return raw;
            }
            catch
            {
                if (raw != null)
                    raw.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Makes a copy of this object.
        /// </summary>
        public TableDefinition Clone()
        {
            return new TableDefinition(TableName, Columns, Persistence);
        }
    }
}
