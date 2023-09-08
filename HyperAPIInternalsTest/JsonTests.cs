using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

using Tableau.HyperAPI.Impl;

namespace Tableau.HyperAPI.Test
{
    public class JsonTests
    {
        [Test]
        public void TestFormatting()
        {
            Assert.AreEqual("null", JsonForPoor.Format(null));
            Assert.AreEqual("8", JsonForPoor.Format(8));
            Assert.AreEqual("1.23", JsonForPoor.Format(1.23));
            Assert.AreEqual("\"xyz\"", JsonForPoor.Format("xyz"));

            JsonObject obj = new JsonObject();
            obj.AddProperty("a", null);
            obj.AddProperty("b", "abc");
            obj.AddProperty("c", 18);
            obj.AddProperty("d", 1.234);
            obj.AddProperty("e", "\"\b\r\f\n\t\\xyz");
            string formatted = JsonForPoor.Format(obj);
            Assert.AreEqual("{\"a\":null,\"b\":\"abc\",\"c\":18,\"d\":1.234,\"e\":\"\\\"\\b\\r\\f\\n\\t\\\\xyz\"}", formatted);
        }

        [Test]
        public void TestFormatMessage()
        {
            Assert.AreEqual("{\"msg\":\"blah\"}", Logger.FormatJsonMessage("blah"));
            Exception ex = new IndexOutOfRangeException("error");
            Assert.AreEqual("{\"msg\":\"blah\",\"exc\":{\"message\":\"error\"}}", Logger.FormatJsonMessage("blah", ex));
            HyperException cause = new HyperException("cause", null, "cause hint", new HyperException.ContextId(0xeaaa03fd));
            HyperException hex = new HyperException("oops", cause, "a hint", new HyperException.ContextId(0xd42b5c5d));
            Assert.AreEqual("{" +
                "\"msg\":\"blah\"," +
                "\"exc\":{" +
                  "\"main_message\":\"oops\"," +
                  "\"hint\":\"a hint\"," +
                  "\"context_id\":\"0xd42b5c5d\"," +
                  "\"cause\":{" +
                    "\"main_message\":\"cause\"," +
                    "\"hint\":\"cause hint\"," +
                    "\"context_id\":\"0xeaaa03fd\"" +
                  "}" +
                "}" +
                "}", Logger.FormatJsonMessage("blah", hex));
        }
    }
}
