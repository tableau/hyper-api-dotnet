using System;
using System.Collections.Generic;
using System.Text;

using Tableau.HyperAPI.Raw;
using Tableau.HyperAPI.Impl;

namespace Tableau.HyperAPI
{
    /// <summary>
    /// Logging levels.
    /// </summary>
    internal enum LogLevel
    {
        /// <summary>
        /// Logging level for fine-grained events that are useful for debugging the application.
        /// </summary>
        Trace = Dll.HYPER_LOG_LEVEL_TRACE,

        /// <summary>
        /// Logging level for informational events that indicate progress of the application.
        /// </summary>
        Info = Dll.HYPER_LOG_LEVEL_INFO,

        /// <summary>
        /// Logging level for potentially harmful events that indicate problems affecting progress of the application.
        /// </summary>
        Warning = Dll.HYPER_LOG_LEVEL_WARNING,

        /// <summary>
        /// Logging level for error events that prevents normal application progress.
        /// </summary>
        Error = Dll.HYPER_LOG_LEVEL_ERROR,

        /// <summary>
        /// Logging level for very severe error events that might cause the application to terminate.
        /// </summary>
        Fatal = Dll.HYPER_LOG_LEVEL_FATAL,
    }

    /// <summary>
    /// Interface used to log events from the library.
    /// </summary>
    internal interface ILogger
    {
        /// <summary>
        /// Log an event.
        /// </summary>
        /// <param name="level"><see cref="LogLevel"/>.</param>
        /// <param name="topic">Event topic.</param>
        /// <param name="jsonValue">Event details formatted as json.</param>
        void LogEvent(LogLevel level, string topic, string jsonValue);
    }

    /// <summary>
    /// Logging utilities.
    /// </summary>
    /// <remarks>
    /// By default the library does not write log events anywhere. User of the library
    /// may set up logging by providing an <see cref="ILogger"/> implementation.
    /// </remarks>
    internal static class Logging
    {
        /// <summary>
        /// Set logging level for the library. Events less severe than this level
        /// will be ignored.
        /// </summary>
        /// <param name="level">Minimum logging level.</param>
        /// <returns>Previous minimum log level.</returns>
        public static LogLevel SetLogLevel(LogLevel level)
        {
            return Logger.SetLogLevel(level);
        }

        /// <summary>
        /// Set an instance of <see cref="ILogger"/> used to log events from the library.
        /// Setting it to <c>null</c> disables logging.
        /// </summary>
        /// <param name="logger">An <see cref="ILogger"/> instance.</param>
        /// <returns>Previous logger instance.</returns>
        public static ILogger SetLogger(ILogger logger)
        {
            return Logger.SetLogger(logger);
        }
    }
}
