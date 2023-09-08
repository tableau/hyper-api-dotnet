using System;
using System.Collections.Generic;
using System.Text;

using Tableau.HyperAPI.Impl;

namespace Tableau.HyperAPI.Raw
{
    /// <summary>
    /// Wrapper around hyper_inserter_t.
    /// </summary>
    internal class InserterHandle : IDisposable
    {
        private IntPtr nativePtr;
        private MutableTableDefinitionHandle tableDefinition;

        internal InserterHandle(IntPtr p, MutableTableDefinitionHandle tableDefinition)
        {
            Util.Verify(p != IntPtr.Zero && tableDefinition != null);
            this.nativePtr = p;
            this.tableDefinition = tableDefinition;
        }

        public bool IsOpen => nativePtr != IntPtr.Zero;

        public void InsertChunk(IntPtr data, ulong length)
        {
            CheckIsOpen();
            Error.Check(Dll.hyper_inserter_insert_chunk(nativePtr, data, length));
        }

        public void InsertComputedExpressions(string selectList)
        {
            CheckIsOpen();
            Error.Check(Dll.hyper_insert_computed_expressions(nativePtr, selectList));
        }

        public void InitBulkInsert(MutableTableDefinitionHandle streamDefinition, string selectList)
        {
            CheckIsOpen();
            Error.Check(Dll.hyper_init_bulk_insert(nativePtr, streamDefinition.NativePtr, selectList));
        }

        private void CheckIsOpen()
        {
            if (!IsOpen)
                throw new ObjectDisposedException("inserter", "Inserter object has been closed");
        }

        public void Close(bool insertData, bool inFinalizer)
        {
            if (IsOpen)
            {
                Exception closeException = null;

                try
                {
                    Error.Check(Dll.hyper_close_inserter(nativePtr, insertData));
                }
                catch (Exception ex)
                {
                    closeException = ex;
                }

                nativePtr = IntPtr.Zero;

                if (tableDefinition != null && !inFinalizer)
                {
                    tableDefinition.Dispose();
                    tableDefinition = null;
                }

                if (closeException != null)
                    throw closeException;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing && IsOpen)
                Util.WarnNotDisposedInternalObject("InserterHandle");

            Close(insertData: false, inFinalizer: !disposing);
        }

        ~InserterHandle()
        {
            Dispose(false);
        }
    }
}
