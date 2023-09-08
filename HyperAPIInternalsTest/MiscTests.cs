using System;

using NUnit.Framework;

using Tableau.HyperAPI.Raw;

namespace Tableau.HyperAPI.Test
{
    public class MiscTests
    {
        [Test]
        public unsafe void Data128Test()
        {
            byte[] buf = new byte[16];
            Dll.hyper_data128_t d = new Dll.hyper_data128_t { data1 = 15, data2 = 42 };
            fixed (byte* p = buf)
            {
                ulong space = Dll.hyper_write_data128_not_null(new IntPtr(p), 16, d);
                Assert.AreEqual(16, space);
                Dll.hyper_data128_t roundtripped = Dll.hyper_read_data128(new IntPtr(p));
                Assert.AreEqual(d.data1, roundtripped.data1);
                Assert.AreEqual(d.data2, roundtripped.data2);
            }
        }

        private void SetErrorString(IntPtr errp, int field, string value)
        {
            Utf8ParamMarshaler utf8ParamMarshaler = new Utf8ParamMarshaler();
            // will leak on failure, not a big deal
            Dll.hyper_error_field_value fieldValue = new Dll.hyper_error_field_value
            {
                discriminator = Dll.ErrorFieldString,
                pointer = utf8ParamMarshaler.MarshalManagedToNative(value)
            };
            Error.Check(Dll.hyper_error_set_field(errp, field, fieldValue));
            utf8ParamMarshaler.CleanUpNativeData(fieldValue.pointer);
        }

        private void SetErrorInt(IntPtr errp, int field, int value)
        {
            Dll.hyper_error_field_value fieldValue = new Dll.hyper_error_field_value { discriminator = Dll.ErrorFieldInteger, pointer = new IntPtr(value) };
            Error.Check(Dll.hyper_error_set_field(errp, field, fieldValue));
        }

        private void SetErrorPointer(IntPtr errp, int field, IntPtr value)
        {
            Dll.hyper_error_field_value fieldValue = new Dll.hyper_error_field_value { discriminator = Dll.ErrorFieldPointer, pointer = value };
            Error.Check(Dll.hyper_error_set_field(errp, field, fieldValue));
            Dll.hyper_error_field_value roundtripped;
            Dll.hyper_error_get_field(errp, field, out roundtripped);
            Assert.AreEqual(fieldValue.discriminator, roundtripped.discriminator);
            Assert.AreEqual(fieldValue.pointer, roundtripped.pointer);
        }

        [Test]
        public void DeprecatedExceptionTest()
        {
            HyperException ex = new HyperException("Message 1", null, "Hint 1",new HyperException.ContextId(0x3452B2FC));
            #pragma warning disable 612,618
            Assert.AreEqual("Message 1", ex.PrimaryMessage);
            #pragma warning restore 612,618
            var deprecatedProperty = ex.GetType().GetProperty("PrimaryMessage");
            var attributes = (ObsoleteAttribute[]) deprecatedProperty.GetCustomAttributes(typeof(ObsoleteAttribute),false);
            Assert.IsNotNull(attributes != null);
            Assert.AreEqual(1,attributes.Length);
            Assert.AreEqual("Use MainMessage instead. This property will be removed in the future.",attributes[0].Message);
        }

        [Test]
        public void ErrorTest()
        {
            // will leak on failure, not a big deal
            IntPtr errp = Dll.hyper_error_create((uint)0xdead);
            Assert.AreNotEqual(IntPtr.Zero, errp);
            SetErrorString(errp, Dll.HYPER_ERROR_FIELD_MESSAGE, "message\nnewline");
            SetErrorString(errp, Dll.HYPER_ERROR_FIELD_HINT_MESSAGE, "hint\nnewline");
            IntPtr pcause = Dll.hyper_error_create((uint)0xbeef);
            SetErrorString(pcause, Dll.HYPER_ERROR_FIELD_MESSAGE, "message2\nnewline");
            SetErrorString(pcause, Dll.HYPER_ERROR_FIELD_HINT_MESSAGE, "hint2");

            SetErrorPointer(errp, Dll.HYPER_ERROR_FIELD_CAUSE, pcause);
            HyperException ex = new HyperException(new ErrorHandle(errp));

            Assert.AreEqual("message\nnewline", ex.MainMessage);
            Assert.AreEqual("hint\nnewline", ex.Hint);
            Assert.AreEqual(0xdead, ex.Context.Value);
            Assert.IsInstanceOf(typeof(HyperException), ex.InnerException);

            HyperException cause = (HyperException)ex.InnerException;
            Assert.AreEqual(0xbeef, cause.Context.Value);
            Assert.AreEqual("message2\nnewline", cause.MainMessage);
            Assert.AreEqual("hint2", cause.Hint);
            Dll.hyper_error_destroy(errp);

            Assert.IsTrue(ex.ToString().StartsWith("Tableau.HyperAPI.HyperException: message\n" +
                            "\tnewline\n" +
                            "Hint: hint\n" +
                            "\tnewline\n" +
                            "Context: 0xdead\n" +
                            " ---> Tableau.HyperAPI.HyperException: message2\n" +
                            "\tnewline\n" +
                            "Hint: hint2\n" +
                            "Context: 0xbeef"));
        }

        internal class ComparerWithOperators : TestUtil.IComparerWithOperators<SqlType>
        {
            public bool Eq(SqlType x, SqlType y)
            {
                return x == y;
            }

            public bool Ge(SqlType x, SqlType y)
            {
                return x >= y;
            }

            public bool Gt(SqlType x, SqlType y)
            {
                return x > y;
            }

            public bool Le(SqlType x, SqlType y)
            {
                return x <= y;
            }

            public bool Lt(SqlType x, SqlType y)
            {
                return x < y;
            }

            public bool Ne(SqlType x, SqlType y)
            {
                return x != y;
            }
        }

        [Test]
        public void UnsupportedTypeComparisonTest()
        {
            TestUtil.CheckComparison(
                new SqlType(TypeTag.Unsupported, SqlType.UnusedModifier, 51),
                new SqlType(TypeTag.Unsupported, SqlType.UnusedModifier, 52),
                new ComparerWithOperators());
        }
    }
}
