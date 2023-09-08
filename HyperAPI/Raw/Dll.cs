using System;
using System.Collections.Generic;
using System.Text;

namespace Tableau.HyperAPI.Raw
{
    internal static partial class Dll
    {
        public const int ErrorFieldInteger = 0;
        public const int ErrorFieldString = 1;
        public const int ErrorFieldPointer = 2;

#pragma warning disable 649

        public struct hyper_error_field_value
        {
            public int discriminator;
            public IntPtr pointer;
        }

        public struct hyper_value_t
        {
            public IntPtr value;
            public ulong size;
        };

        public struct hyper_service_version_t
        {
           public uint major;
           public uint minor;
        };

        public struct hyper_data128_t
        {
            public ulong data1;
            public ulong data2;
        }

        public struct hyper_date_components_t
        {
            public int year;
            public short month;
            public short day;
        }

        public struct hyper_time_components_t
        {
            public byte hour;
            public byte minute;
            public byte second;
            public int microsecond;
        }

        public struct hyper_interval_components_t
        {
            public int years;
            public int months;
            public int days;
            public int hours;
            public int minutes;
            public int seconds;
            public int microseconds;
        }

#pragma warning restore 649
    }
}
