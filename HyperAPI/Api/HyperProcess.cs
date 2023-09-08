using System;
using System.Collections.Generic;
using System.IO;

using Tableau.HyperAPI.Raw;
using Tableau.HyperAPI.Impl;

namespace Tableau.HyperAPI
{
    /// <summary>
    /// Constants which define whether Hyper server should send telemetry data to Tableau.
    /// </summary>
    public enum Telemetry
    {
        /// <summary>
        /// Enable collection of telemetry data.
        /// </summary>
        SendUsageDataToTableau = Dll.HYPER_ENABLE_TELEMETRY,

        /// <summary>
        /// Disable collection of telemetry data.
        /// </summary>
        DoNotSendUsageDataToTableau = Dll.HYPER_DISABLE_TELEMETRY,
    }

    /// <summary>
    /// Local Hyper server instance managed by the library.
    /// </summary>
    public class HyperProcess : IDisposable
    {
        internal IntPtr NativePtr { get; private set; }
        private string userAgent;

        static HyperProcess()
        {
            Logger.Init();
        }

        /// <summary>
        /// Starts a local Hyper server instance. Call <see cref="Shutdown"/> or <see cref="Close"/> when
        /// you are done with it, or use it in a <c>using</c> statement.
        /// </summary>
        /// <param name="hyperPath">Path to the directory which contains hyperd executable.</param>
        /// <param name="telemetry">Whether telemetry should be enabled.</param>
        /// <param name="userAgent">Optional user agent string identifying the application, which will be
        /// used in logging.</param>
        /// <param name="parameters">Optional parameters for starting the Hyper process.
        /// The available parameters are documented
        /// <see href="https://tableau.github.io/hyper-db/docs/hyper-api/hyper_process#process-settings">
        /// in the Tableau Hyper documentation, chapter "Process Settings"</see>.</param>
        public HyperProcess(string hyperPath, Telemetry telemetry, string userAgent = null, Dictionary<string, string> parameters = null)
        {
            this.userAgent = userAgent;

            using (InstanceParametersHandle rawParams = new InstanceParametersHandle())
            {
                if (parameters != null)
                {
                    foreach (KeyValuePair<string, string> kv in parameters)
                        rawParams.Set(kv.Key, kv.Value);
                }

                Error.Check(Dll.hyper_instance_create(hyperPath,
                    telemetry == Telemetry.SendUsageDataToTableau ? 1 : 0, rawParams.NativePtr, out IntPtr p));
                NativePtr = p;
            }
        }

        /// <summary>
        /// Starts a local Hyper server instance, trying to locate it in the "hyper" directory
        /// next to the dll. Call <see cref="Shutdown"/> or <see cref="Close"/> when
        /// you are done with it, or use it in a <c>using</c> statement.
        /// </summary>
        /// <param name="telemetry">Whether telemetry should be enabled.</param>
        /// <param name="userAgent">Optional user agent string identifying the application, which will be
        /// used in logging.</param>
        /// <param name="parameters">Optional parameters for starting the Hyper process.
        /// The available parameters are documented
        /// <see href="https://tableau.github.io/hyper-db/docs/hyper-api/hyper_process#process-settings">
        /// in the Tableau Hyper documentation, chapter "Process Settings"</see>.</param>
        public HyperProcess(Telemetry telemetry, string userAgent = null, Dictionary<string, string> parameters = null)
            : this(null, telemetry, userAgent, parameters)
        {
        }

        /// <summary>
        /// The network endpoint to connect to this hyper server instance.
        /// </summary>
        public Endpoint Endpoint
        {
            get
            {
                CheckIsOpen();
                return new Endpoint(Dll.hyper_instance_get_endpoint_descriptor(NativePtr), userAgent);
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the server has not been stopped yet.
        /// </summary>
        public bool IsOpen => NativePtr != IntPtr.Zero;

        /// <summary>
        /// Stops the server.
        /// </summary>
        /// <remarks>
        /// This is called automatically by <c>using</c> statement, so normally you don't need
        /// to call it explicitly. Even if it's not called (implicitly or explicitly), the server will
        /// be stopped when the application exits, this library may not be used to simply start the
        /// Hyper server and leave it running.
        /// </remarks>
        public void Close()
        {
            if (IsOpen)
            {
                Dll.hyper_instance_close(NativePtr);
                NativePtr = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Shuts down the Hyper process, throws a <see cref="HyperException"/> if there was an error stopping the process,
        /// if the process was forcefully killed after the timeout, or if the process already exited with a non-zero exit code.
        /// </summary>
        /// <remarks>
        /// If `timeoutMs` &gt; 0ms, wait for Hyper to shut down gracefully. If the process is still running after a timeout
        /// of `timeoutMs` milliseconds timeout, forcefully terminate the process and throw an exception.
        ///
        /// If `timeoutMs` &lt; 0ms, wait indefinitely for Hyper to shut down.
        ///
        /// If `timeoutMs` == 0ms, immediately terminate Hyper forcefully. Does not throw if the process already exited with a non-zero exit code.
        /// </remarks>
        /// <param name="timeoutMs">The timeout in milliseconds.</param>
        public void Shutdown(int timeoutMs = -1)
        {
            if (IsOpen)
            {
                try
                {
                    Error.Check(Dll.hyper_instance_shutdown(NativePtr, timeoutMs));
                }
                finally
                {
                    NativePtr = IntPtr.Zero;
                }
            }
        }

        private void CheckIsOpen()
        {
            if (!IsOpen)
                throw new ObjectDisposedException("HyperProcess");
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~HyperProcess()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing && IsOpen)
                Util.WarnNotDisposedUserObject("Hyper process has not been shut down. Close it with Shutdown() or Close() or use it in a 'using' statement.");

            Close();
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        /// <remarks>
        /// This is called automatically by <c>using</c> statement, so normally you don't need
        /// to call it explicitly. Even if it's not called (implicitly or explicitly), the server will
        /// be stopped when the application exits, this library may not be used to simply start the
        /// Hyper server and leave it running.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
