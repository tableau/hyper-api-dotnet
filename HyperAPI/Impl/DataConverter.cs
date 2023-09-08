using System;

namespace Tableau.HyperAPI.Impl
{
    internal static class DataConverter
    {
        public static bool ToBool(object value)
        {
            return (bool)value;
        }

        public static short ToShort(object value)
        {
            try
            {
                return (short)value;
            }
            catch
            {
            }

            switch (value)
            {
                case int i:
                    return (short)i;
                case long l:
                    return (short)l;
            }

            throw new InvalidCastException($"cannot convert value of type {value.GetType()} to short");
        }

        public static int ToInt(object value)
        {
            try
            {
                return (int)value;
            }
            catch
            {
            }

            switch (value)
            {
                case short s:
                    return s;
                case long l:
                    return (int)l;
            }

            throw new InvalidCastException($"cannot convert value of type {value.GetType()} to int");
        }

        public static uint ToUint(object value)
        {
            try
            {
                return (uint)value;
            }
            catch
            {
                throw new InvalidCastException($"cannot convert value of type {value.GetType()} to uint");
            }
        }

        public static long ToLong(object value)
        {
            try
            {
                return (long)value;
            }
            catch
            {
            }

            switch (value)
            {
                case short s:
                    return s;
                case int i:
                    return i;
            }

            throw new InvalidCastException($"cannot convert value of type {value.GetType()} to long");
        }

        public static double ToDouble(object value)
        {
            try
            {
                return (double)value;
            }
            catch
            {
            }

            switch (value)
            {
                case float f:
                    return f;
                case short s:
                    return s;
                case int i:
                    return i;
                case long l:
                    return l;
                case decimal d:
                    return (double)d;
            }

            throw new InvalidCastException($"cannot convert value of type {value.GetType()} to double");
        }

        public static decimal ToNumeric(object value)
        {
            try
            {
                return (decimal)value;
            }
            catch
            {
            }

            switch (value)
            {
                case double d:
                    return (decimal)d;
                case float f:
                    return (decimal)f;
                case short s:
                    return s;
                case int i:
                    return i;
                case long l:
                    return l;
            }

            throw new InvalidCastException($"cannot convert value of type {value.GetType()} to decimal");
        }

        public static byte[] ToBytes(object value)
        {
            return (byte[])value;
        }

        public static string ToString(object value)
        {
            return (string)value;
        }

        public static Date ToDate(object value)
        {
            return (Date)value;
        }

        public static TimeSpan ToTime(object value)
        {
            return (TimeSpan)value;
        }

        public static Timestamp ToTimestamp(object value)
        {
            return (Timestamp)value;
        }

        public static Timestamp ToTimestampTz(object value)
        {
            try
            {
                return (Timestamp)value;
            }
            catch
            {
            }

            switch (value)
            {
                case DateTimeOffset dto:
                    return (Timestamp)dto.UtcDateTime;
            }

            throw new InvalidCastException($"cannot convert value of type {value.GetType()} to timestamp");
        }

        public static Interval ToInterval(object value)
        {
            return (Interval)value;
        }
    }
}
