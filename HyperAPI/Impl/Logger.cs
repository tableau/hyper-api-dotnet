using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Specialized;

using Tableau.HyperAPI.Raw;

namespace Tableau.HyperAPI.Impl
{
    internal class Logger
    {
        private static object loggerLock = new object();
        private static ILogger currentLogger = null;
        private static LogLevel currentLogLevel = LogLevel.Info;
        // prevent GC from collecting the delegate object we pass to hyper_log_set_log_function
        private static GCHandle logFunctionHandle;

        static Logger()
        {
            Dll.hyper_log_set_log_level((int)LogLevel.Info);
            Dll.hyper_log_function_t func = HyperLogFunction;
            logFunctionHandle = GCHandle.Alloc(func);
            Dll.hyper_log_set_log_function(func, IntPtr.Zero);
        }

        internal static void Init()
        {
            // Do nothing, static constructor does the initialization.
        }

        internal static ILogger SetLogger(ILogger logger)
        {
            ILogger prevLogger;

            lock (loggerLock)
            {
                prevLogger = currentLogger;
                currentLogger = logger;
            }

            return prevLogger;
        }

        internal static LogLevel SetLogLevel(LogLevel level)
        {
            LogLevel prevLevel;

            lock (loggerLock)
            {
                prevLevel = currentLogLevel;
                currentLogLevel = level;
            }

            Dll.hyper_log_set_log_level((int)level);

            return prevLevel;
        }

        private static bool GetLogger(LogLevel level, out ILogger logger)
        {
            LogLevel currentLevel;

            lock (loggerLock)
            {
                logger = currentLogger;
                currentLevel = currentLogLevel;
            }

            return logger != null && (int)level >= (int)currentLevel;
        }

        private static void HyperLogFunction(int logLevel, IntPtr topic, IntPtr jsonValue, IntPtr context)
        {
            if (!GetLogger((LogLevel)logLevel, out ILogger logger))
                return;

            logger.LogEvent((LogLevel)logLevel, NativeStringConverter.PtrToStringUtf8(topic), NativeStringConverter.PtrToStringUtf8(jsonValue));
        }

        private static void DoLog(LogLevel level, string message, Exception ex = null)
        {
            if (!GetLogger(level, out ILogger logger))
                return;

            logger.LogEvent(level, "hyper-api-dotnet", FormatJsonMessage(message, ex));
        }

        public static void Info(string message)
        {
            DoLog(LogLevel.Info, message);
        }

        public static void Warn(string message)
        {
            DoLog(LogLevel.Warning, message);
        }

        public static void Error(string message)
        {
            DoLog(LogLevel.Error, message);
        }

        public static void Error(string message, Exception ex)
        {
            DoLog(LogLevel.Error, message, ex);
        }

        private static void AppendStringLiteral(StringBuilder builder, string text)
        {
        }

        private static void AppendException(StringBuilder builder, Exception ex)
        {
            builder.Append("{\"message\":");
            AppendStringLiteral(builder, ex.Message);
        }

        private static void AddHyperExceptionProperties(JsonObject obj, HyperException ex)
        {
            obj.AddProperty("hint", ex.Hint);
            obj.AddProperty("context_id", "0x" + ex.Context.Value.ToString("x2"));
        }

        private static JsonObject SerializeException(Exception ex)
        {
            JsonObject obj = new JsonObject();
            if (ex is HyperException)
            {
                obj.AddProperty("main_message", ((HyperException)ex).MainMessage);
                AddHyperExceptionProperties(obj, (HyperException)ex);
            }
            else
            {
                obj.AddProperty("message", ex.Message);
            }
            if (ex.InnerException != null)
                obj.AddProperty("cause", SerializeException(ex.InnerException));
            return obj;
        }

        internal static string FormatJsonMessage(string message, Exception ex = null)
        {
            JsonObject obj = new JsonObject();
            obj.AddProperty("msg", message);

            if (ex != null)
                obj.AddProperty("exc", SerializeException(ex));

            return JsonForPoor.Format(obj);
        }
    }
}
