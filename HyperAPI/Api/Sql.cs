using System;
using System.Text;
using System.Runtime.InteropServices;

namespace Tableau.HyperAPI
{
    /// <summary>
    /// Utility functions.
    /// </summary>
    public static class Sql
    {
        private delegate ulong QuoteFunction(IntPtr target, ulong space, IntPtr value, ulong length);

        private static string InvokeQuoteFunction(string input, QuoteFunction function)
        {
            IntPtr inputBuffer = IntPtr.Zero;
            IntPtr outputBuffer = IntPtr.Zero;
            try
            {
                inputBuffer = Raw.NativeStringConverter.StringToNativeUtf8(input, out int inputSize);
                ulong outputSize = function(IntPtr.Zero, 0, inputBuffer, (ulong)inputSize);
                outputBuffer = Marshal.AllocHGlobal((int)outputSize);
                function(outputBuffer, outputSize, inputBuffer, (ulong)inputSize);
                unsafe
                {
                    return Encoding.UTF8.GetString((byte*)outputBuffer, (int)outputSize);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(outputBuffer);
                Marshal.FreeHGlobal(inputBuffer);
            }
        }

        internal static string EscapeNameString(string identifier)
        {
            return InvokeQuoteFunction(identifier, Raw.Dll.hyper_quote_sql_identifier);
        }

        /// <summary>
        /// Quote and escape an identifier for use in SQL queries. Equivalent to <c>new Name(identifier)</c>.
        /// </summary>
        /// <param name="identifier">Identifier.</param>
        /// <returns>Quoted identifier as a <see cref="Name"/> object.</returns>
        public static Name EscapeName(string identifier)
        {
            return new Name(identifier);
        }

        /// <summary>
        /// Quote and escape a string literal for use in SQL queries.
        /// </summary>
        /// <param name="text">Input text.</param>
        /// <returns>Quoted and escaped text, including the quotes.</returns>
        public static string EscapeStringLiteral(string text)
        {
            return InvokeQuoteFunction(text, Raw.Dll.hyper_quote_sql_literal);
        }
    }
}
