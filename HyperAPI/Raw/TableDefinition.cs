using System;

using Tableau.HyperAPI.Impl;

namespace Tableau.HyperAPI.Raw
{
    /// <summary>
    /// Wrapper around hyper_table_definition_t. It does not manage its lifetime,
    /// <see cref="MutableTableDefinitionHandle"/> is the one that owns the C object.
    /// </summary>
    internal class TableDefinitionHandle
    {
        internal IntPtr NativePtr { get; set; }
        public int ColumnCount { get; private set; }

        internal TableDefinitionHandle(IntPtr p)
        {
            NativePtr = p;
            ColumnCount = (int)Dll.hyper_table_definition_column_count(p);
        }

        internal bool IsNull => NativePtr == IntPtr.Zero;

        protected void CheckNotDisposed()
        {
            if (IsNull)
                throw new ObjectDisposedException("tableDefinition");
        }

        protected void CheckColumnIndex(int columnIndex)
        {
            CheckNotDisposed();
            if (columnIndex < 0 || columnIndex >= ColumnCount)
                throw new IndexOutOfRangeException();
        }

        public string DatabaseName
        {
            get
            {
                CheckNotDisposed();
                return Dll.hyper_table_definition_database_name(NativePtr);
            }
        }

        public string SchemaName
        {
            get
            {
                CheckNotDisposed();
                return Dll.hyper_table_definition_schema_name(NativePtr);
            }
        }

        public string TableName
        {
            get
            {
                CheckNotDisposed();
                return Dll.hyper_table_definition_table_name(NativePtr);
            }
        }

        public int ColumnTypeTag(int columnIndex)
        {
            CheckColumnIndex(columnIndex);
            return (int)Dll.hyper_table_definition_column_type_tag(NativePtr, (uint)columnIndex);
        }

        public uint ColumnTypeOid(int columnIndex)
        {
            CheckColumnIndex(columnIndex);
            return Dll.hyper_table_definition_column_type_oid(NativePtr, (uint)columnIndex);
        }

        public uint ColumnTypeModifier(int columnIndex)
        {
            CheckColumnIndex(columnIndex);
            return Dll.hyper_table_definition_column_type_modifier(NativePtr, (uint)columnIndex);
        }

        public int ColumnIndex(string columnName)
        {
            CheckNotDisposed();
            int idx = (int)Dll.hyper_table_definition_column_index(NativePtr, columnName);
            if (idx < 0 || idx >= ColumnCount)
                throw new IndexOutOfRangeException($"Invalid column name {columnName}");
            return idx;
        }

        public string ColumnName(int columnIndex)
        {
            CheckColumnIndex(columnIndex);
            return Dll.hyper_table_definition_column_name(NativePtr, (uint)columnIndex);
        }

        public string ColumnCollation(int columnIndex)
        {
            CheckColumnIndex(columnIndex);
            return Dll.hyper_table_definition_column_collation(NativePtr, (uint)columnIndex);
        }

        public bool ColumnIsNullable(int columnIndex)
        {
            CheckColumnIndex(columnIndex);
            return Dll.hyper_table_definition_column_is_nullable(NativePtr, (uint)columnIndex);
        }
    }

    /// <summary>
    /// Wrapper around hyper_table_definition_t which owns the C object.
    /// </summary>
    internal class MutableTableDefinitionHandle : TableDefinitionHandle, IDisposable
    {
        public MutableTableDefinitionHandle(IntPtr p)
            : base(p)
        {
        }

        public MutableTableDefinitionHandle(string databaseName, string schemaName, string tableName, Persistence persistence)
            : base(Create(databaseName, schemaName, tableName, persistence))
        {
        }

        private static IntPtr Create(string databaseName, string schemaName, string tableName, Persistence persistence)
        {
            IntPtr p = Dll.hyper_create_table_definition(databaseName, schemaName, tableName, (int)persistence, false);
            if (p == IntPtr.Zero)
                throw new OutOfMemoryException();
            return p;
        }

        public void AddColumn(string columnName, int typeTag, uint typeModifier, string collation, bool isNullable)
        {
            CheckNotDisposed();
            Error.Check(Dll.hyper_table_definition_add_column(
                NativePtr, columnName, typeTag, typeModifier, collation, isNullable));
        }

        ~MutableTableDefinitionHandle()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing && !IsNull)
                Util.WarnNotDisposedInternalObject("MutableTableDefinitionHandle");

            if (!IsNull)
            {
                Dll.hyper_destroy_table_definition(NativePtr);
                NativePtr = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
