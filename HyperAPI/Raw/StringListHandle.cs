using System;
using System.Collections.Generic;
using System.Text;

using Tableau.HyperAPI.Impl;

namespace Tableau.HyperAPI.Raw
{
    /// <summary>
    /// Wraps hyper_string_list_t, owns and frees the object passed to it.
    /// </summary>
    internal class StringListHandle : IDisposable
    {
        private IntPtr nativePtr;

        public StringListHandle(IntPtr p)
        {
            Util.Verify(p != IntPtr.Zero);
            nativePtr = p;
        }

        public List<string> GetValues()
        {
            ulong itemCount = Dll.hyper_string_list_size(nativePtr);
            Util.Verify(itemCount <= int.MaxValue);
            List<string> list = new List<string>((int)itemCount);
            for (int i = 0; i < (int)itemCount; ++i)
                list.Add(Dll.hyper_string_list_at(nativePtr, i));
            return list;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing && nativePtr != IntPtr.Zero)
                Util.WarnNotDisposedInternalObject("StringListHandle");

            if (nativePtr != IntPtr.Zero)
            {
                Dll.hyper_string_list_destroy(nativePtr);
                nativePtr = IntPtr.Zero;
            }
        }

        ~StringListHandle()
        {
            Dispose(false);
        }
    }
}
