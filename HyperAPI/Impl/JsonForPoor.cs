using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Tableau.HyperAPI.Impl
{
    internal class JsonObject
    {
        private List<Tuple<string, object>> properties = new List<Tuple<string, object>>();

        public void AddProperty(string property, object value)
        {
            properties.Add(new Tuple<string, object>(property, value));
        }

        public void Serialize(StringBuilder builder)
        {
            builder.Append('{');

            bool first = true;
            foreach (var prop in properties)
            {
                if (!first)
                    builder.Append(',');
                first = false;

                JsonForPoor.Serialize(builder, prop.Item1);
                builder.Append(':');
                JsonForPoor.Serialize(builder, prop.Item2);
            }

            builder.Append('}');
        }
    }

    internal class JsonForPoor
    {
        public static string Format(object obj)
        {
            StringBuilder builder = new StringBuilder();
            Serialize(builder, obj);
            return builder.ToString();
        }

        public static void Serialize(StringBuilder builder, object obj)
        {
            if (obj == null)
                builder.Append("null");
            else if (obj is int || obj is short || obj is long || obj is uint || obj is ushort || obj is ulong || obj is double)
                builder.Append(obj.ToString());
            else if (obj is string)
                Serialize(builder, (string)obj);
            else if (obj is JsonObject)
                ((JsonObject)obj).Serialize(builder);
            else if (obj is IEnumerable)
                Serialize(builder, (IEnumerable)obj);
            else if (obj is Enum)
                Serialize(builder, obj.ToString());
            else
                throw new NotImplementedException($"can not serialize object of type {obj.GetType()}");
        }

        private static void Serialize(StringBuilder builder, string text)
        {
            builder.Append("\"");

            foreach (char c in text.ToCharArray())
            {
                switch (c)
                {
                    case '\b':
                        builder.Append("\\b");
                        break;
                    case '\f':
                        builder.Append("\\f");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    default:
                        builder.Append(c);
                        break;
                }
            }

            builder.Append("\"");
        }

        private static void Serialize(StringBuilder builder, IEnumerable list)
        {
            builder.Append('[');

            bool first = true;
            foreach (object elm in list)
            {
                if (!first)
                    builder.Append(',');
                first = false;
                Serialize(builder, elm);
            }

            builder.Append(']');
        }
    }
}
