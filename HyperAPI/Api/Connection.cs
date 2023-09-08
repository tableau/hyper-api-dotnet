using System;
using System.Collections.Generic;

using Tableau.HyperAPI.Raw;
using Tableau.HyperAPI.Impl;

namespace Tableau.HyperAPI
{
    /// <summary>
    /// Constants which specify what should happen when connecting to a database depending on whether it exists.
    /// </summary>
    public enum CreateMode
    {
        /// <summary>
        /// Do not create the database. Connection will fail if database doesn't exist.
        /// </summary>
        None = Dll.HYPER_DO_NOT_CREATE,

        /// <summary>
        /// Create the database. Connection will fail if the database already exists.
        /// </summary>
        Create = Dll.HYPER_CREATE,

        /// <summary>
        /// Create the database if it doesn't exist before connecting.
        /// </summary>
        CreateIfNotExists = Dll.HYPER_CREATE_IF_NOT_EXISTS,

        /// <summary>
        /// Create the database before connecting. If it already exists, drop the old one first.
        /// </summary>
        CreateAndReplace = Dll.HYPER_CREATE_AND_REPLACE,
    }

    /// <summary>
    /// Connection to a Hyper server.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Connection must be closed after you are done with it, either by calling <c>Close()</c>
    /// or implicitly with a <c>using</c> statement.
    /// </para>
    /// <para>
    /// This class is not thread-safe, no two methods may be called simultaneously from different threads.
    /// The only exception is <c>Connection.Cancel()</c>, which may be safely called from a different thread.
    /// </para>
    /// </remarks>
    public sealed class Connection : IDisposable
    {
        // Object used to serialize Cancel() and Close() methods.
        private readonly object cancelLock = new object();

        internal ConnectionHandle NativeHandle { get; private set; }

        internal Endpoint Endpoint { get; }

        static Connection()
        {
            Logger.Init();
        }

        /// <summary>
        /// Connects to a hyper endpoint.
        /// </summary>
        /// <param name="endpoint">Endpoint to connect to.</param>
        /// <param name="parameters">Optional connection parameters to pass to Hyper. The available parameters are documented in the
        /// <see href="https://tableau.github.io/hyper-db/docs/hyper-api/connection#connection-settings">
        /// Tableau Hyper documentation, chapter "Connection Settings"</see>.</param>
        public Connection(Endpoint endpoint, Dictionary<string, string> parameters = null)
            : this(endpoint, null, CreateMode.None, parameters)
        {
        }

        /// <summary>
        /// Connects to a hyper endpoint and attaches to exactly one database.
        /// </summary>
        /// <param name="endpoint"><see cref="Endpoint"/> to connect to.</param>
        /// <param name="databasePath">Path to the database file.</param>
        /// <param name="createMode">Whether the database should be created and what to do in case
        /// of an already existing database. <see cref="CreateMode.None"/> by default, which means that
        /// the database must already exist.</param>
        /// <param name="parameters">Optional connection parameters to pass to Hyper. The available parameters are documented in the
        /// <see href="https://tableau.github.io/hyper-db/docs/hyper-api/connection#connection-settings">
        /// Tableau Hyper documentation, chapter "Connection Settings"</see>.</param>
        public Connection(Endpoint endpoint, string databasePath, CreateMode createMode = CreateMode.None, Dictionary<string, string> parameters = null)
        {
            Util.CheckArgument(endpoint != null, "endpoint must be specified");
            Util.CheckArgument(!string.IsNullOrEmpty(databasePath) || createMode == CreateMode.None, "CreateMode may be specified only with a database to connect to");

            using (ConnectionParametersHandle rawParams = new ConnectionParametersHandle())
            {
                rawParams.Set("endpoint", endpoint.ConnectionDescriptor);

                if (!string.IsNullOrEmpty(databasePath))
                    rawParams.Set("dbname", databasePath);

                if (!string.IsNullOrEmpty(endpoint.UserAgent))
                {
                    rawParams.Set("user_agent", endpoint.UserAgent);
                }
                rawParams.Set("api_language", "C#");

                if (parameters != null)
                {
                    foreach (KeyValuePair<string, string> kv in parameters)
                        rawParams.Set(kv.Key, kv.Value);
                }

                NativeHandle = new ConnectionHandle(rawParams, (int)createMode);
            }

            Endpoint = endpoint;
            Catalog = new Catalog(this);
        }

        /// <summary>
        /// Is the connection still open?
        /// </summary>
        public bool IsOpen => NativeHandle != null;

        /// <summary>
        /// Gets the catalog for this connection.
        /// </summary>
        public Catalog Catalog { get; }

        /// <summary>
        /// Cancels the current SQL command of this connection (if any).
        /// </summary>
        /// <remarks>
        /// This method is thread-safe, it may be safely called from a different thread.
        /// </remarks>
        public void Cancel()
        {
            lock (cancelLock)
            {
                if (IsOpen)
                    NativeHandle.Cancel();
            }
        }

        /// <summary>
        /// Checks whether the connection is ready, i.e. if the connection
        /// can be used. A connection is not ready when there is an open <see cref="Result"/>
        /// or <see cref="Inserter"/>. Trying to use a connection that is not ready for further
        /// query methods will throw an exception.
        /// </summary>
        public bool IsReady => IsOpen && NativeHandle.IsReady;

        /// <summary>
        /// Executes a SQL query and returns its result as a <see cref="Result"/> object. Close the result
        /// when you are done with it, or use this method in a <c>using</c> statement.
        /// </summary>
        /// <param name="query">SQL query text.</param>
        /// <returns>Query result.</returns>
        public Result ExecuteQuery(string query)
        {
            CheckIsOpen();
            ResultHandle result = NativeHandle.ExecuteQuery(query);
            return new Result(this, result);
        }

        /// <summary>
        /// Executes a SQL statement.
        /// </summary>
        /// <returns>The number of affected rows, or <c>-1</c> if it's not available.</returns>
        public int ExecuteCommand(string statement)
        {
            CheckIsOpen();
            return NativeHandle.ExecuteCommand(statement);
        }

        /// <summary>
        /// Executes a scalar query, i.e. a query which returns exactly one row with one column.
        /// </summary>
        /// <returns>Value in the row 0 column 0, or <c>null</c> if the value was NULL.</returns>
        /// <remarks>It throws an exception if the query did not return exactly one row with one column.</remarks>
        public T ExecuteScalarQuery<T>(string query)
        {
            using (Result result = ExecuteQuery(query))
            {
                if (result.Schema.ColumnCount != 1)
                    throw new HyperException("query result must have exactly one column", new HyperException.ContextId(0x148d260a));

                if (!result.NextRow())
                    throw new HyperException("query returned zero rows", new HyperException.ContextId(0x1099d816));

                object value = result.GetValue(0);

                if (result.NextRow())
                    throw new HyperException("query returned more than one row", new HyperException.ContextId(0xc5b1ec80));

                if (value != null && !(value is T))
                    throw new ArgumentException(string.Format("Invalid type {0} used with ExecuteScalarQuery, the returned value is of type {1}",
                        typeof(T).Name, value.GetType().Name));

                return (T)value;
            }
        }

        /// <summary>
        /// Returns the Hyper Service version of this connection
        /// </summary>
        /// <returns>The Hyper Service version of this connection</returns>
        public HyperServiceVersion HyperServiceVersion() {
            return NativeHandle.HyperServiceVersion();
        }

        /// <summary>
        /// Returns true if the capability flag is active on this connection.
        /// </summary>
        /// <param name="capabilityFlag">The capability flag to check. It is prefixed with `capability_`.</param>
        /// <returns>true if the capability flag is active on this connection.</returns>
        public bool IsCapabilityActive(String capabilityFlag) {
            return NativeHandle.IsCapabilityActive(capabilityFlag);
        }

        /// <summary>
        /// Connects to the Hyper endpoint and determines which Hyper Service version numbers are common between the
        /// Hyper API and the Hyper server.
        /// </summary>
        /// <param name="endpoint">Endpoint to connect to.</param>
        /// <return> List of Hyper Service versions that are supported by both this HAPI and the endpoint.</return>
        static public List<HyperServiceVersion> QuerySupportedHyperServiceVersionRange(Endpoint endpoint)
        {
            using (ConnectionParametersHandle rawParams = new ConnectionParametersHandle())
            {
                rawParams.Set("endpoint", endpoint.ConnectionDescriptor);

                if (!string.IsNullOrEmpty(endpoint.UserAgent))
                {
                    rawParams.Set("user_agent", endpoint.UserAgent);
                }
                rawParams.Set("api_language", "C#");

                return ConnectionHandle.QuerySupportedHyperServiceVersionRange(rawParams);
            }
        }

        internal void DetachAllDatabases()
        {
            CheckIsOpen();
            NativeHandle.DetachAllDatabases();
        }

        internal void CheckIsOpen()
        {
            if (!IsOpen)
                throw new ObjectDisposedException("connection");
        }

        /// <summary>
        /// Closes the connection. Actual connection to the server is not closed if there
        /// is an active result or inserter, and will be closed with those. This is called
        /// automatically by <c>using</c> statement.
        /// </summary>
        public void Close()
        {
            lock (cancelLock)
            {
                if (IsOpen)
                {
                    NativeHandle.Close();
                    NativeHandle = null;
                }
            }
        }

        /// <summary>
        /// Closes the connection. Actual connection to the server is not closed if there
        /// is an active result or inserter, and will be closed with those. Equivalent
        /// to <see cref="Close"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing && IsOpen)
                Util.WarnNotDisposedUserObject("Connection object has not been closed. Close it with Close() or use it in a 'using' statement.");

            if (disposing)
                Close();
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~Connection()
        {
            Dispose(false);
        }
    }
}
