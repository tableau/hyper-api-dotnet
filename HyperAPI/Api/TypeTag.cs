using Tableau.HyperAPI.Raw;

namespace Tableau.HyperAPI
{
    /// <summary>
    /// Hyper data types.
    /// </summary>
    public enum TypeTag
    {
        /// <summary>
        /// Unsupported type. Queries and tables may have columns of this type if the database was created by
        /// a newer version of the library. Values are represented by byte arrays.
        /// </summary>
        Unsupported = Dll.HYPER_UNSUPPORTED,

        /// <summary>
        /// Boolean values, represented by <c>bool</c>.
        /// </summary>
        Bool = Dll.HYPER_BOOL,

        /// <summary>
        /// Eight-byte signed integer values, represented by <c>long</c>.
        /// </summary>
        BigInt = Dll.HYPER_BIG_INT,

        /// <summary>
        /// Two-byte signed integer values, represented by <c>short</c>.
        /// </summary>
        SmallInt = Dll.HYPER_SMALL_INT,

        /// <summary>
        /// Four-byte signed integer values, represented by <c>int</c>.
        /// </summary>
        Int = Dll.HYPER_INT,

        /// <summary>
        /// Exact decimal numbers with user-specified precision, represented by <c>decimal</c>.
        /// </summary>
        Numeric = Dll.HYPER_NUMERIC,

        /// <summary>
        /// Double precision floating point values, represented by <c>double</c>.
        /// </summary>
        Double = Dll.HYPER_DOUBLE,

        /// <summary>
        /// OID values, represented by <c>uint</c>.
        /// </summary>
        Oid = Dll.HYPER_OID,

        /// <summary>
        /// Byte array.
        /// </summary>
        Bytes = Dll.HYPER_BYTE_A,

        /// <summary>
        /// Unicode text.
        /// </summary>
        Text = Dll.HYPER_TEXT,

        /// <summary>
        /// Unicode text with maximum length.
        /// </summary>
        /// <remarks>
        /// Do not use this type unless you have compatibility requirements. <see cref="Text"/> will work just as well,
        /// there are no performance or space optimizations which would make <see cref="Varchar"/> more efficient.
        /// </remarks>
        Varchar = Dll.HYPER_VARCHAR,

        /// <summary>
        /// Space-padded Unicode text of fixed length.
        /// </summary>
        /// <remarks>
        /// Do not use this type unless you have compatibility requirements. <see cref="Text"/> will work just as well,
        /// there are no performance or space optimizations which would make <see cref="Char"/> more efficient.
        /// </remarks>
        Char = Dll.HYPER_CHAR,

        /// <summary>
        /// Json.
        /// </summary>
        Json = Dll.HYPER_JSON,

        /// <summary>
        /// Date values, represented by <see cref="HyperAPI.Date"/>.
        /// </summary>
        Date = Dll.HYPER_DATE,

        /// <summary>
        /// Time interval, represented by <see cref="HyperAPI.Interval"/>.
        /// </summary>
        Interval = Dll.HYPER_INTERVAL,

        /// <summary>
        /// Time of the day, from 00:00:00 to 23:59:59:999999. Represented by <c>TimeSpan</c>.
        /// </summary>
        Time = Dll.HYPER_TIME,

        /// <summary>
        /// Timestamp - date and time of day. Represented by <see cref="HyperAPI.Timestamp"/>.
        /// </summary>
        Timestamp = Dll.HYPER_TIMESTAMP,

        /// <summary>
        /// UTC Timestamp - date and time of day in UTC time zone. Represented by <see cref="HyperAPI.Timestamp"/>.
        /// </summary>
        TimestampTZ = Dll.HYPER_TIMESTAMP_TZ,

        /// <summary>
        /// Geography, represented by <c>byte[]</c>.
        /// </summary>
        Geography = Dll.HYPER_GEOGRAPHY,
    }
}
