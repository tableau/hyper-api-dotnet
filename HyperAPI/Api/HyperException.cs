using System;

using Tableau.HyperAPI.Raw;

namespace Tableau.HyperAPI
{
    /// <summary>
    /// Exception thrown by Hyper API methods.
    /// Note that error messages may change in the future versions of the library.
    /// </summary>
    public sealed class HyperException : Exception
    {
        /// <summary>
        /// A context id.
        ///
        /// Used to identify the source of an exception. Each throw statement has a unique context id that is stored in the thrown error.
        /// </summary>
        public struct ContextId
        {
            /// <summary>
            /// The value.
            /// </summary>
            public uint Value { get; }

            internal ContextId(uint value) { Value = value; }
        }

        /// <summary>
        /// The main error message.
        /// </summary>
        public string MainMessage { get; }

        /// <summary>
        /// The primary error message.
        ///
        /// <para>
        /// This property is obsolete, use MainMessage instead. This property will be removed in the future.
        /// </para>
        /// </summary>
        [ObsoleteAttribute("Use MainMessage instead. This property will be removed in the future.")]
        public string PrimaryMessage { get { return MainMessage; } }

        /// <summary>
        /// Suggestion on how to avoid the error.
        /// </summary>
        public string Hint { get; }

        /// <summary>
        /// Context id.
        /// </summary>
        public ContextId Context { get; }

        internal HyperException(ErrorHandle errp)
            : this(errp.Message, GetInnerException(errp), errp.Hint, new ContextId(errp.ContextId))
        {
        }

        internal HyperException(string message, HyperException cause, string hint, ContextId id)
            : base(FormatExceptionMessage(message, cause, hint, id), cause)
        {
            MainMessage = message;
            Hint = hint;
            Context = id;
        }

        internal HyperException(string message, ContextId id)
            : this(message, null, null, id)
        {
        }

        internal static string FormatExceptionMessage(string message, HyperException cause, string hint, ContextId id)
        {
            string str = "";

            if (!string.IsNullOrEmpty(message))
                str = message.Replace("\n", "\n\t");

            if (!string.IsNullOrEmpty(hint))
                str += "\nHint: " + hint.Replace("\n", "\n\t");

            str += "\nContext: 0x" + id.Value.ToString("x2");

            return str;
        }


        /// <summary>
        /// Builds the string representation of the exception
        /// </summary>
        public override string ToString()
        {
            // We consistently use '\n' as line breaks. However, .NET introduces some system specific line breaks.
            return base.ToString().Replace(Environment.NewLine, "\n");
        }

        private static HyperException GetInnerException(ErrorHandle errp)
        {
            ErrorHandle cause = errp.Cause;
            if (cause.IsSet)
                return new HyperException(cause);
            return null;
        }
    }
}
