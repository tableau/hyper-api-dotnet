using System;
using System.Text;
using System.Runtime.InteropServices;

namespace Tableau.HyperAPI.Raw
{
    internal static class NativeStringConverter
    {
        // store it in the class because Encoding.UTF8 is in fact a quite expensive function
        private static Encoding UTF8 { get; }

        static NativeStringConverter()
        {
            UTF8 = Encoding.UTF8;
        }

        /// <summary>
        /// Convert an UTF8 string in unmanaged memory to a C# string.
        /// </summary>
        /// <remarks>
        /// .NET Core has this, but .NET Framework doesn't.
        /// </remarks>
        public static string PtrToStringUtf8(IntPtr s)
        {
            if (s == IntPtr.Zero)
                return null;

            unsafe
            {
                // This isn't the most efficient implementation, but it is meant to
                // be used only for function parameters.
                byte* p = (byte*)s.ToPointer();
                int len = 0;
                while (p[len] != 0)
                    ++len;
                return UTF8.GetString(p, len);
            }
        }

        /// <summary>
        /// Convert an UTF8 string in unmanaged memory to a C# string.
        /// </summary>
        /// <remarks>
        /// .NET Core has this, but .NET Framework doesn't.
        /// </remarks>
        public static string PtrToStringUtf8(IntPtr s, ulong size)
        {
            unsafe
            {
                return UTF8.GetString((byte*)s.ToPointer(), (int)size);
            }
        }

        /// <summary>
        /// Convert a C# string to a zero-terminated UTF-8 string in unmanaged memory.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <param name="len">Size of the returned buffer NOT including terminating zero.</param>
        /// <returns>Allocated buffer. It must be freed with <c>Marshal.FreeHGlobal()</c>.</returns>
        public static IntPtr StringToNativeUtf8(string s, out int len)
        {
            byte[] bytes = UTF8.GetBytes(s);
            IntPtr p = Marshal.AllocHGlobal(bytes.Length + 1);
            Marshal.Copy(bytes, 0, p, bytes.Length);
            unsafe
            {
                *((byte*)p.ToPointer() + bytes.Length) = 0;
            }
            len = bytes.Length;
            return p;
        }
    }

    /// <summary>
    /// Marshaler which converts a managed string to a native UTF8 string, to be used for function
    /// parameters.
    /// </summary>
    /// <remarks>
    /// This is the same as UnmanagedType.LPUTF8Str, which is not available in .NET Standard 2.0.
    /// </remarks>
    internal class Utf8ParamMarshaler : ICustomMarshaler
    {
        static ICustomMarshaler GetInstance(string cookie)
        {
            return new Utf8ParamMarshaler();
        }

        public void CleanUpManagedData(object o)
        {
        }

        public void CleanUpNativeData(IntPtr p)
        {
            if (p != IntPtr.Zero)
                Marshal.FreeHGlobal(p);
        }

        public int GetNativeDataSize()
        {
            return IntPtr.Size;
        }

        public IntPtr MarshalManagedToNative(object o)
        {
            if (o == null)
                return IntPtr.Zero;

            return NativeStringConverter.StringToNativeUtf8((string)o, out int len);
        }

        public object MarshalNativeToManaged(IntPtr s)
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Marshaler which converts constant native UTF8 strings to managed strings, to be used
    /// for return values of functions which return UTF8 strings which are not owned by the caller.
    /// </summary>
    /// <remarks>
    /// This is similar to UnmanagedType.LPUTF8Str, except it assumes that return values are owned
    /// by the caller, which is the case in hyper API.
    /// </remarks>
    internal class ConstUtf8RetValueMarshaler : ICustomMarshaler
    {
        static ICustomMarshaler GetInstance(string cookie)
        {
            return new ConstUtf8RetValueMarshaler();
        }

        public void CleanUpManagedData(object o)
        {
        }

        public void CleanUpNativeData(IntPtr p)
        {
        }

        public int GetNativeDataSize()
        {
            return IntPtr.Size;
        }

        public IntPtr MarshalManagedToNative(object o)
        {
            throw new InvalidOperationException();
        }

        public object MarshalNativeToManaged(IntPtr s)
        {
            return NativeStringConverter.PtrToStringUtf8(s);
        }
    }
}
