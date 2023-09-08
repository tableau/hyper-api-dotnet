using System;

namespace Tableau.HyperAPI.Raw
{
    internal static class Error
    {
        /// <summary>
        /// Check if a hyper_error_t* is set, free it and throw a HyperException if it is.
        /// </summary>
        public static void Check(IntPtr errp)
        {
            if (errp != IntPtr.Zero)
            {
                HyperException ex = new HyperException(new ErrorHandle(errp));
                Dll.hyper_error_destroy(errp);
                throw ex;
            }
        }
    }

    /// <summary>
    /// Wrapper around hyper_error_t*. Does not manage its lifetime, it only provides
    /// wrappers for hyper_error_* methods.
    /// </summary>
    internal struct ErrorHandle
    {
        private readonly IntPtr nativePtr;

        public ErrorHandle(IntPtr p)
        {
            nativePtr = p;
        }

        public bool IsSet
        {
            get
            {
                return nativePtr != IntPtr.Zero;
            }
        }

        public string Message
        {
            get
            {
                return GetStringField(Dll.HYPER_ERROR_FIELD_MESSAGE);
            }
        }

        public string Hint
        {
            get
            {
                return GetStringField(Dll.HYPER_ERROR_FIELD_HINT_MESSAGE);
            }
        }

        public uint ContextId
        {
            get
            {
                return GetUIntField(Dll.HYPER_ERROR_FIELD_CONTEXT_ID);
            }
        }

        public ErrorHandle Cause
        {
            get
            {
                return new ErrorHandle(GetPointerField(Dll.HYPER_ERROR_FIELD_CAUSE));
            }
        }

        private Dll.hyper_error_field_value GetFieldValue(int field)
        {
            if (!IsSet)
                return new Dll.hyper_error_field_value();

            IntPtr errp = Dll.hyper_error_get_field(nativePtr, field, out Dll.hyper_error_field_value value);
            if (errp != IntPtr.Zero)
            {
                Dll.hyper_error_destroy(errp);
                throw new Exception("exception while getting error information");
            }

            return value;
        }

        private string GetStringField(int field)
        {
            if (!IsSet)
                return null;

            Dll.hyper_error_field_value value = GetFieldValue(field);
            return NativeStringConverter.PtrToStringUtf8(value.pointer);
        }

        private int GetIntField(int field)
        {
            if (!IsSet)
                return 0;

            Dll.hyper_error_field_value value = GetFieldValue(field);
            // strip upper 4 bytes of garbage
            return (int)value.pointer.ToInt64();
        }

        private uint GetUIntField(int field)
        {
            if (!IsSet)
                return 0;

            Dll.hyper_error_field_value value = GetFieldValue(field);
            // strip upper 4 bytes of garbage
            return (uint)value.pointer.ToInt64();
        }

        private IntPtr GetPointerField(int field)
        {
            if (!IsSet)
                return IntPtr.Zero;

            Dll.hyper_error_field_value value = GetFieldValue(field);
            return value.pointer;
        }
    }
}
