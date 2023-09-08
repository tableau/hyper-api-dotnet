using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Tableau.HyperAPI.Impl;

namespace Tableau.HyperAPI
{
    /// <summary>
    /// Class which is responsible for querying and manipulating metadata.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Connection"/>'s <see cref="Connection.Catalog"/> property to get <c>Catalog</c> instances.
    /// </remarks>
    public sealed class Catalog
    {
        /// <summary>
        /// Underlying connection.
        /// </summary>
        public Connection Connection { get; }

        internal Catalog(Connection connection)
        {
            Connection = connection;
        }

        /// <summary>
        /// Get the underlying native native connection handle, throw if the connection is closed.
        /// </summary>
        private Raw.ConnectionHandle CheckedConnectionHandle
        {
            get
            {
                Connection.CheckIsOpen();
                return Connection.NativeHandle;
            }
        }

        /// <summary>
        /// Does a table with the specified name exist?
        /// </summary>
        /// <param name="table">Table name.</param>
        /// <returns>Whether the table exists.</returns>
        public bool HasTable(TableName table)
        {
            table.GetComponents(out string databaseName, out string schemaName, out string tableName);
            return CheckedConnectionHandle.HasTable(databaseName, schemaName, tableName);
        }

        /// <summary>
        /// Gets a table definition. Throws an exception if the table does not exist.
        /// </summary>
        /// <param name="table">Table name.</param>
        /// <returns>Table definition.</returns>
        public TableDefinition GetTableDefinition(TableName table)
        {
            table.GetComponents(out string databaseName, out string schemaName, out string tableName);

            using (Raw.MutableTableDefinitionHandle raw = CheckedConnectionHandle.GetTableDefinition(databaseName, schemaName, tableName))
            {
                return new TableDefinition(raw);
            }
        }

        private void CreateTableImpl(TableDefinition tableDefinition, bool failIfExists)
        {
            using (Raw.MutableTableDefinitionHandle rawTableDef = tableDefinition.CreateNativeTableDefinition())
            {
                CheckedConnectionHandle.CreateTable(rawTableDef, failIfExists);
            }
        }

        /// <summary>
        /// Creates a table. Throws an exception if it already exists.
        /// </summary>
        /// <param name="tableDefinition">Table definition.</param>
        public void CreateTable(TableDefinition tableDefinition)
        {
            CreateTableImpl(tableDefinition, true);
        }

        /// <summary>
        /// Creates a table. If it already exists, does nothing.
        /// </summary>
        /// <param name="tableDefinition">Table definition.</param>
        public void CreateTableIfNotExists(TableDefinition tableDefinition)
        {
            CreateTableImpl(tableDefinition, false);
        }

        /// <summary>
        /// Creates a schema. Throws an exception if it already exists.
        /// </summary>
        /// <param name="schema">Schema name.</param>
        public void CreateSchema(SchemaName schema)
        {
            CheckedConnectionHandle.CreateSchema(schema, true);
        }

        /// <summary>
        /// Creates a schema. If it already exists, does nothing.
        /// </summary>
        /// <param name="schema">Schema name.</param>
        public void CreateSchemaIfNotExists(SchemaName schema)
        {
            CheckedConnectionHandle.CreateSchema(schema, false);
        }

        /// <summary>
        /// Gets the schema names.
        /// </summary>
        /// <param name="database">If specified, it returns schemas in this database. Otherwise it returns
        /// schemas in the first database in the database search path.</param>
        /// <returns></returns>
        public IEnumerable<SchemaName> GetSchemaNames(DatabaseName database = null)
        {
            return CheckedConnectionHandle.GetSchemaNames(database?.Unescaped).Select(s => (database != null) ? new SchemaName(database, s) : new SchemaName(s));
        }

        /// <summary>
        /// Gets the names of all tables in the specified schema.
        /// </summary>
        /// <param name="schema">Schema name.</param>
        /// <returns></returns>
        public IEnumerable<TableName> GetTableNames(SchemaName schema)
        {
            Util.CheckArgument(schema != null, "schema name must be specified");

            schema.GetComponents(out string databaseName, out string schemaName);
            return CheckedConnectionHandle.GetTableNames(databaseName, schemaName).Select(s => new TableName(schema, s));
        }

        /// <summary>
        /// Creates a database. Throws an exception if the database already exists.
        /// </summary>
        /// <param name="databasePath">Database path.</param>
        public void CreateDatabase(string databasePath)
        {
            Util.CheckArgument(!string.IsNullOrEmpty(databasePath), "database path must be specified");
            CheckedConnectionHandle.CreateDatabase(databasePath, true);
        }

        /// <summary>
        /// Creates a database. Does nothing if the database already exists.
        /// </summary>
        /// <param name="databasePath">Database path.</param>
        public void CreateDatabaseIfNotExists(string databasePath)
        {
            Util.CheckArgument(!string.IsNullOrEmpty(databasePath), "database path must be specified");
            CheckedConnectionHandle.CreateDatabase(databasePath, false);
        }

        /// <summary>
        /// Attaches a database.
        /// </summary>
        /// <param name="databasePath">Database path.</param>
        /// <param name="alias">Database alias. If not specified then the stem of the database path is used.</param>
        public void AttachDatabase(string databasePath, Name alias = null)
        {
            Util.CheckArgument(!string.IsNullOrEmpty(databasePath), "database path must be specified");
            CheckedConnectionHandle.AttachDatabase(databasePath, alias?.Unescaped);
        }

        /// <summary>
        /// Detaches a database.
        /// </summary>
        /// <param name="alias">Alias of the database to detach.</param>
        public void DetachDatabase(Name alias)
        {
            Util.CheckArgument(alias != null, "alias must be specified");
            CheckedConnectionHandle.DetachDatabase(alias.Unescaped);
        }

        /// <summary>
        /// Detaches all databases from this connection.
        /// </summary>
        public void DetachAllDatabases()
        {
            Connection.DetachAllDatabases();
        }

        /// <summary>
        /// Deletes a database. Throws an exception if the database does not exist.
        /// </summary>
        /// <param name="databasePath">Database path.</param>
        public void DropDatabase(string databasePath)
        {
            Util.CheckArgument(!string.IsNullOrEmpty(databasePath), "database path must be specified");
            CheckedConnectionHandle.DropDatabase(databasePath, true);
        }

        /// <summary>
        /// Deletes a database. If the database does not exist, does nothing.
        /// </summary>
        /// <param name="databasePath">Database path.</param>
        public void DropDatabaseIfExists(string databasePath)
        {
            Util.CheckArgument(!string.IsNullOrEmpty(databasePath), "database path must be specified");
            CheckedConnectionHandle.DropDatabase(databasePath, false);
        }
    }
}
