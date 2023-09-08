using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Tableau.HyperAPI.Impl;

namespace Tableau.HyperAPI.Raw
{
    /// <summary>
    /// Wrapper around native connection object.
    /// </summary>
    internal class ConnectionHandle : IDisposable
    {
        private IntPtr nativePtr;

        public ConnectionHandle(ConnectionParametersHandle parameters, int createMode)
        {
            Error.Check(Dll.hyper_connect(parameters.NativePtr, out nativePtr, createMode));
        }

        private bool IsOpen => nativePtr != IntPtr.Zero;

        private void CheckIsOpen()
        {
            if (!IsOpen)
                throw new ObjectDisposedException("connection");
        }

        public bool IsReady => IsOpen && Dll.hyper_connection_is_ready(nativePtr);

        public void Close()
        {
            if (IsOpen)
            {
                Dll.hyper_disconnect(nativePtr);
                nativePtr = IntPtr.Zero;
            }
        }

        public void Cancel()
        {
            CheckIsOpen();
            Error.Check(Dll.hyper_cancel(nativePtr));
        }

        public MutableTableDefinitionHandle GetTableDefinition(string databaseName, string schemaName, string tableName)
        {
            IntPtr pTableDef;
            Error.Check(Dll.hyper_get_table_definition(nativePtr, databaseName, schemaName, tableName, out pTableDef));
            return new MutableTableDefinitionHandle(pTableDef);
        }

        public int ExecuteCommand(string statement)
        {
            CheckIsOpen();
            Error.Check(Dll.hyper_execute_command(nativePtr, statement, out int rowCount));
            return rowCount;
        }

        public static List<HyperServiceVersion> QuerySupportedHyperServiceVersionRange(ConnectionParametersHandle parameters) {
            IntPtr versionPtr = new IntPtr(0);
            Error.Check(Dll.hyper_query_supported_hyper_service_version_range(parameters.NativePtr, out versionPtr, out ulong elements));
            List<HyperServiceVersion> versionList = new List<HyperServiceVersion>();
            if (versionPtr == IntPtr.Zero) {
               return versionList;
            } else {
               for (int i = 0; i < (int)elements; i++)
               {
                 var elementPtr = IntPtr.Add(versionPtr,(int)i*Marshal.SizeOf(typeof(Dll.hyper_service_version_t)));
                 Dll.hyper_service_version_t minVersion = Marshal.PtrToStructure<Dll.hyper_service_version_t>(elementPtr);
                 versionList.Add(new HyperServiceVersion(minVersion.major, minVersion.minor));
               }
               return versionList;
            }
        }

        public ResultHandle ExecuteQuery(string query)
        {
            CheckIsOpen();
            Error.Check(Dll.hyper_execute_query(nativePtr, query, out IntPtr pResult));
            return new ResultHandle(pResult);
        }

        public InserterHandle CreateInserter(MutableTableDefinitionHandle tableDefinition)
        {
            CheckIsOpen();
            Error.Check(Dll.hyper_create_inserter(nativePtr, tableDefinition.NativePtr, out IntPtr pInserter));
            return new InserterHandle(pInserter, tableDefinition);
        }

        public void CreateDatabase(string path, bool failIfExists)
        {
            CheckIsOpen();
            Error.Check(Dll.hyper_create_database(nativePtr, path, failIfExists));
        }

        public void DropDatabase(string path, bool failIfNotExists)
        {
            CheckIsOpen();
            Error.Check(Dll.hyper_drop_database(nativePtr, path, failIfNotExists));
        }

        public void CreateTable(MutableTableDefinitionHandle tableDefinition, bool failIfExists)
        {
            CheckIsOpen();
            Error.Check(Dll.hyper_create_table(nativePtr, tableDefinition.NativePtr, failIfExists));
        }

        public void CreateSchema(SchemaName schema, bool failIfExists)
        {
            CheckIsOpen();
            string dbPart, schemaPart;
            schema.GetComponents(out dbPart, out schemaPart);
            Error.Check(Dll.hyper_create_schema(nativePtr, dbPart, schemaPart, failIfExists));
        }

        public bool HasTable(string databaseName, string schemaName, string tableName)
        {
            CheckIsOpen();
            Error.Check(Dll.hyper_has_table(nativePtr, databaseName, schemaName, tableName, out bool exists));
            return exists;
        }

        public List<string> GetSchemaNames(string database)
        {
            CheckIsOpen();
            Error.Check(Dll.hyper_get_schema_names(nativePtr, database, out IntPtr schemaNames));
            using (StringListHandle list = new StringListHandle(schemaNames))
                return list.GetValues();
        }

        public List<string> GetTableNames(string databaseName, string schemaName)
        {
            CheckIsOpen();
            Error.Check(Dll.hyper_get_table_names(nativePtr, databaseName, schemaName, out IntPtr tableNames));
            using (StringListHandle list = new StringListHandle(tableNames))
                return list.GetValues();
        }

        public HyperServiceVersion HyperServiceVersion()
        {
            CheckIsOpen();
            IntPtr versionPtr = Marshal.AllocHGlobal(Marshal.SizeOf<Dll.hyper_service_version_t>());
            try {
               Error.Check(Dll.hyper_connection_get_hyper_service_version(nativePtr, versionPtr));

               Dll.hyper_service_version_t version = Marshal.PtrToStructure<Dll.hyper_service_version_t>(versionPtr);
               return new HyperServiceVersion(version.major, version.minor);
            }
            finally
            {
               Marshal.FreeHGlobal(versionPtr);
            }
        }

        public bool IsCapabilityActive(String capabilityFlag)
        {
            CheckIsOpen();
            return Dll.hyper_connection_is_capability_active(nativePtr, capabilityFlag);
        }

        public void AttachDatabase(string databasePath, string alias)
        {
            CheckIsOpen();
            Error.Check(Dll.hyper_attach_database(nativePtr, databasePath, alias));
        }

        public void DetachDatabase(string alias)
        {
            CheckIsOpen();
            Error.Check(Dll.hyper_detach_database(nativePtr, alias));
        }

        public void DetachAllDatabases()
        {
            CheckIsOpen();
            Error.Check(Dll.hyper_detach_all_databases(nativePtr));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing && IsOpen)
                Util.WarnNotDisposedInternalObject("ConnectionHandle");

            Close();
        }

        ~ConnectionHandle()
        {
            Dispose(false);
        }
    }
}
