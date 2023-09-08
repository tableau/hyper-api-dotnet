using System;
using System.Collections.Generic;
using System.Text;

using Tableau.HyperAPI.Impl;

namespace Tableau.HyperAPI.Raw
{
    /// <summary>
    /// Wrapper around hyper_rowset_t.
    /// </summary>
    internal class ResultHandle : IDisposable
    {
        private IntPtr nativePtr;

        public ResultHandle(IntPtr p)
        {
            Util.Verify(p != IntPtr.Zero);
            nativePtr = p;
        }

        public bool IsOpen => nativePtr != IntPtr.Zero;

        public void Close()
        {
            if (IsOpen)
            {
                Dll.hyper_close_rowset(nativePtr);
                nativePtr = IntPtr.Zero;
            }
        }

        public ResultSchema Schema
        {
            get
            {
                Util.Verify(IsOpen);
                IntPtr pdef = Dll.hyper_rowset_get_table_definition(nativePtr);
                return new ResultSchema(new TableDefinitionHandle(pdef));
            }
        }

        public int GetAffectedRowCount()
        {
            Util.Verify(IsOpen);
            return (int)Dll.hyper_rowset_get_affected_row_count(nativePtr);
        }

        public IntPtr NextChunk()
        {
            Util.Verify(IsOpen);
            Error.Check(Dll.hyper_rowset_get_next_chunk(nativePtr, out IntPtr pChunk));
            return pChunk;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing && nativePtr != IntPtr.Zero)
                Util.WarnNotDisposedInternalObject("ResultHandle");

            Close();
        }

        ~ResultHandle()
        {
            Dispose(false);
        }
    }
}
