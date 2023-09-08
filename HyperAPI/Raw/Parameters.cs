using System;

using Tableau.HyperAPI.Impl;

namespace Tableau.HyperAPI.Raw
{
    /// <summary>
    /// Wrapper around hyper_parameters_t. Owns the C object.
    /// </summary>
    internal abstract class ParametersHandle : IDisposable
    {
        internal IntPtr NativePtr { get; private set; }

        private bool IsNull => NativePtr == IntPtr.Zero;

        public void Set(string key, string value)
        {
            Util.Verify(key != null);
            Util.Verify(value != null);

            if (NativePtr == IntPtr.Zero)
                throw new ObjectDisposedException("parameters");

            Error.Check(Dll.hyper_parameters_set(NativePtr, key, value));
        }

        // hyper_error_t* func(hyper_parameters_t** out_params)
        protected delegate IntPtr FactoryFunction(out IntPtr p);

        protected ParametersHandle(FactoryFunction factoryFunc)
        {
            Error.Check(factoryFunc(out IntPtr p));
            NativePtr = p;
        }

        ~ParametersHandle()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!IsNull)
            {
                if (!disposing)
                    Util.WarnNotDisposedInternalObject("ParametersHandle");


                Dll.hyper_parameters_destroy(NativePtr);
                NativePtr = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    internal class InstanceParametersHandle : ParametersHandle
    {
        public InstanceParametersHandle()
            : base((out IntPtr p) => Dll.hyper_create_instance_parameters(out p, true))
        {
        }
    }

    internal class ConnectionParametersHandle : ParametersHandle
    {
        public ConnectionParametersHandle()
            : base((out IntPtr p) => Dll.hyper_create_connection_parameters(IntPtr.Zero, out p))
        {
        }
    }
}
