using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Tableau.HyperAPI.Impl
{
    /// <summary>
    /// Exception thrown from <see cref="Util.Verify(bool, string)"/> when that fails.
    /// </summary>
    internal class InternalErrorException : Exception
    {
        internal InternalErrorException(string message)
            : base(message)
        {
        }

        internal InternalErrorException(string message, Exception cause)
            : base(message, cause)
        {
        }
    }

    /// <summary>
    /// Miscellaneous utility functions.
    /// </summary>
    internal static class Util
    {
        /// <summary>
        /// Check that the function argument satisfies the condition, throw if it doesn't.
        /// This is meant to be used to verify user errors - preconditions and such. Use
        /// <see cref="Verify(bool, string)"/> to check things that aren't supposed to ever happen.
        /// </summary>
        internal static void CheckArgument(bool condition)
        {
            CheckArgument(condition, "invalid arguments");
        }

        /// <summary>
        /// Check that the function argument satisfies the condition, throw if it doesn't.
        /// This is meant to be used to verify user errors - preconditions and such. Use
        /// <see cref="Verify(bool, string)"/> to check things that aren't supposed to ever happen.
        /// </summary>
        internal static void CheckArgument(bool condition, string message)
        {
            if (!condition)
                throw new ArgumentException(message);
        }

        /// <summary>
        /// Check that the function argument satisfies the condition, throw if it doesn't.
        /// This is meant to be used to verify user errors - preconditions and such. Use
        /// <see cref="Verify(bool, string)"/> to check things that aren't supposed to ever happen.
        /// </summary>
        internal static void CheckArgument<TException>(bool condition, Func<TException> createException)
            where TException : Exception
        {
            if (!condition)
                throw createException();
        }

        /// <summary>
        /// Verify that the condition is satisfied. If it fails it means our bug, do not use
        /// this to verify user input, use <c>CheckArgument()</c> for that.
        /// </summary>
        internal static void Verify(bool condition, string messageForLogs = null)
        {
            if (!condition)
            {
                Logger.Error(messageForLogs ?? "internal error");
                throw new InternalErrorException("internal error");
            }
        }

        internal static void WarnNotDisposedObject(string message)
        {
            if (Debugger.IsAttached)
                Debugger.Break();

            Logger.Warn(message);
        }

        internal static void WarnNotDisposedUserObject(string message)
        {
            WarnNotDisposedObject(message);
        }

        internal static void WarnNotDisposedInternalObject(string objectName)
        {
            WarnNotDisposedObject($"Object {objectName} was not properly disposed, this means a bug in the library.");
        }
    }
}
