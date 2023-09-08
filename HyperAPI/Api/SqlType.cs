using System;
using System.Collections.Generic;

using Tableau.HyperAPI.Raw;
using Tableau.HyperAPI.Impl;

namespace Tableau.HyperAPI
{
    /// <summary>
    /// Data type of a column in a table or a query result.
    /// </summary>
    public sealed class SqlType : IEquatable<SqlType>, IComparable, IComparable<SqlType>
    {
        /// <summary>
        /// Base data type.
        /// </summary>
        public TypeTag Tag { get; }

        /// <summary>
        /// OID of this type. This is an internal method which may change or disappear in future versions of the library.
        /// </summary>
        public uint InternalOid { get; }

        /// <summary>
        /// Type modifier. This is an internal method which may change or disappear in the future versions of the library.
        /// </summary>
        public uint InternalTypeModifier { get; }

        internal const uint UnusedModifier = 0xFFFFFFFF;

        private static Dictionary<TypeTag, uint> OidMap = new Dictionary<TypeTag, uint>
        {
            { TypeTag.Bool, Dll.HYPER_OID_BOOL },
            { TypeTag.BigInt, Dll.HYPER_OID_BIG_INT },
            { TypeTag.SmallInt, Dll.HYPER_OID_SMALL_INT },
            { TypeTag.Int, Dll.HYPER_OID_INT },
            { TypeTag.Numeric, Dll.HYPER_OID_NUMERIC },
            { TypeTag.Double, Dll.HYPER_OID_DOUBLE },
            { TypeTag.Oid, Dll.HYPER_OID_OID },
            { TypeTag.Bytes, Dll.HYPER_OID_BYTE_A },
            { TypeTag.Text, Dll.HYPER_OID_TEXT },
            { TypeTag.Varchar, Dll.HYPER_OID_VARCHAR },
            { TypeTag.Char, Dll.HYPER_OID_CHAR },
            { TypeTag.Json, Dll.HYPER_OID_JSON },
            { TypeTag.Date, Dll.HYPER_OID_DATE },
            { TypeTag.Interval, Dll.HYPER_OID_INTERVAL },
            { TypeTag.Time, Dll.HYPER_OID_TIME },
            { TypeTag.Timestamp, Dll.HYPER_OID_TIMESTAMP },
            { TypeTag.TimestampTZ, Dll.HYPER_OID_TIMESTAMP_TZ },
            { TypeTag.Geography, Dll.HYPER_OID_GEOGRAPHY },
        };

        private const uint OidChar1 = Dll.HYPER_OID_CHAR1;
        private static readonly uint Length1Modifier = Dll.hyper_encode_string_modifier(1);

        internal SqlType(TypeTag tag, uint modifier = UnusedModifier, uint oid = 0)
        {
            Tag = tag;
            InternalTypeModifier = modifier;

            if (oid == 0)
            {
                if (tag == TypeTag.Char && modifier == Length1Modifier)
                    oid = OidChar1;
                else
                    oid = OidMap[tag];
            }

            InternalOid = oid;

            Precision = Tag == TypeTag.Numeric ? (int)Dll.hyper_get_precision_from_modifier(InternalTypeModifier) : -1;
            Scale = Tag == TypeTag.Numeric ? (int)Dll.hyper_get_scale_from_modifier(InternalTypeModifier) : -1;
            MaxLength = Tag == TypeTag.Char || Tag == TypeTag.Varchar ? (int)Dll.hyper_get_max_length_from_modifier(InternalTypeModifier) : -1;
        }

        /// <summary>
        /// Precision, i.e. maximum number of digits, of a <see cref="TypeTag.Numeric"/> value.
        /// Returns <c>-1</c> for other types.
        /// </summary>
        public int Precision { get; }

        /// <summary>
        /// Scale, i.e. number of fractional digits, of a <see cref="TypeTag.Numeric"/> value.
        /// Returns <c>-1</c> for other types.
        /// </summary>
        public int Scale { get; }

        /// <summary>
        /// Max length of this type if it is <see cref="TypeTag.Char"/> or <see cref="TypeTag.Varchar"/>.
        /// Returns <c>-1</c> for other types.
        /// </summary>
        public int MaxLength { get; }

        /// <summary>
        /// Gets an instance of <see cref="TypeTag.Bool"/> type.
        /// </summary>
        /// <returns>The <see cref="SqlType"/> instance.</returns>
        public static SqlType Bool() => new SqlType(TypeTag.Bool);

        /// <summary>
        /// Gets an instance of <see cref="TypeTag.SmallInt"/> type.
        /// </summary>
        /// <returns>The <see cref="SqlType"/> instance.</returns>
        public static SqlType SmallInt() => new SqlType(TypeTag.SmallInt);

        /// <summary>
        /// Gets an instance of <see cref="TypeTag.Int"/> type.
        /// </summary>
        /// <returns>The <see cref="SqlType"/> instance.</returns>
        public static SqlType Int() => new SqlType(TypeTag.Int);

        /// <summary>
        /// Gets an instance of <see cref="TypeTag.BigInt"/> type.
        /// </summary>
        /// <returns>The <see cref="SqlType"/> instance.</returns>
        public static SqlType BigInt() => new SqlType(TypeTag.BigInt);

        /// <summary>
        /// Gets an instance of <see cref="TypeTag.Double"/> type.
        /// </summary>
        /// <returns>The <see cref="SqlType"/> instance.</returns>
        public static SqlType Double() => new SqlType(TypeTag.Double);

        /// <summary>
        /// Gets an instance of <see cref="TypeTag.Oid"/> type.
        /// </summary>
        /// <returns>The <see cref="SqlType"/> instance.</returns>
        public static SqlType Oid() => new SqlType(TypeTag.Oid);

        /// <summary>
        /// Gets an instance of <see cref="TypeTag.Numeric"/> type.
        /// </summary>
        /// <param name="precision">Precision, i.e. the number of decimal digits. Must be between 1 and 18.</param>
        /// <param name="scale">Scale, i.e. the number of fractional decimal digits. Must between 0 and <c>precision</c>.</param>
        /// <returns>The <see cref="SqlType"/> instance.</returns>
        public static SqlType Numeric(uint precision, uint scale)
        {
            Util.CheckArgument(1 <= precision && precision <= 38, "precision must be between 1 and 38");
            Util.CheckArgument(scale <= precision, "scale must be less or equal to precision");
            return new SqlType(TypeTag.Numeric, Dll.hyper_encode_numeric_modifier(precision, scale));
        }

        /// <summary>
        /// Gets an instance of <see cref="TypeTag.Date"/> type.
        /// </summary>
        /// <returns>The <see cref="SqlType"/> instance.</returns>
        public static SqlType Date() => new SqlType(TypeTag.Date);

        /// <summary>
        /// Gets an instance of <see cref="TypeTag.Time"/> type.
        /// </summary>
        /// <returns>The <see cref="SqlType"/> instance.</returns>
        public static SqlType Time() => new SqlType(TypeTag.Time);

        /// <summary>
        /// Gets an instance of <see cref="TypeTag.Timestamp"/> type.
        /// </summary>
        /// <returns>The <see cref="SqlType"/> instance.</returns>
        public static SqlType Timestamp() => new SqlType(TypeTag.Timestamp);

        /// <summary>
        /// Gets an instance of <see cref="TypeTag.TimestampTZ"/> type.
        /// </summary>
        /// <returns>The <see cref="SqlType"/> instance.</returns>
        public static SqlType TimestampTZ() => new SqlType(TypeTag.TimestampTZ);

        /// <summary>
        /// Gets an instance of <see cref="TypeTag.Interval"/> type.
        /// </summary>
        /// <returns>The <see cref="SqlType"/> instance.</returns>
        public static SqlType Interval() => new SqlType(TypeTag.Interval);

        /// <summary>
        /// Gets an instance of <see cref="TypeTag.Bytes"/> type.
        /// </summary>
        /// <returns>The <see cref="SqlType"/> instance.</returns>
        public static SqlType Bytes() => new SqlType(TypeTag.Bytes);

        /// <summary>
        /// Gets an instance of <see cref="TypeTag.Text"/> type.
        /// </summary>
        /// <returns>The <see cref="SqlType"/> instance.</returns>
        public static SqlType Text() => new SqlType(TypeTag.Text);

        /// <summary>
        /// Gets an instance of <see cref="TypeTag.Json"/> type.
        /// </summary>
        /// <returns>The <see cref="SqlType"/> instance.</returns>
        public static SqlType Json() => new SqlType(TypeTag.Json);

        /// <summary>
        /// Gets an instance of <see cref="TypeTag.Geography"/> type.
        /// </summary>
        /// <returns>The <see cref="SqlType"/> instance.</returns>
        public static SqlType Geography() => new SqlType(TypeTag.Geography);

        /// <summary>
        /// Gets an instance of <see cref="TypeTag.Char"/> type.
        /// </summary>
        /// <param name="length">
        /// String length, in unicode code points. Strings shorter
        /// than this will be space-padded to this length. Must be greater than zero.
        /// </param>
        /// <returns>The <see cref="SqlType"/> instance.</returns>
        /// <remarks>
        /// Do not use this type unless you have compatibility requirements, e.g. if the
        /// strings must be space-padded. If you want to store fixed-length strings, then
        /// <see cref="Text"/> will work just as well, there are no performance or space
        /// optimizations which make <c>Char</c> more efficient.
        /// </remarks>
        public static SqlType Char(uint length)
        {
            Util.CheckArgument(length >= 1, "length must be greater than zero");
            return new SqlType(TypeTag.Char, Dll.hyper_encode_string_modifier(length));
        }

        /// <summary>
        /// Gets an instance of <see cref="TypeTag.Varchar"/> type.
        /// </summary>
        /// <param name="maxLength">Maximum string length, in Unicode code points. Must be greater than zero.</param>
        /// <returns>The <see cref="SqlType"/> instance.</returns>
        /// <remarks>
        /// Do not use this type unless you have compatibility requirements. <see cref="Text"/>
        /// will work just as well, there are no performance or space optimizations which make
        /// <c>Varchar</c> more efficient.
        /// </remarks>
        public static SqlType Varchar(uint maxLength)
        {
            Util.CheckArgument(maxLength >= 1, "maxLength must be greater than zero");
            return new SqlType(TypeTag.Varchar, Dll.hyper_encode_string_modifier(maxLength));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        public override string ToString()
        {
            switch (Tag)
            {
                case TypeTag.Numeric:
                    return $"Numeric({Precision}, {Scale})";
                case TypeTag.Varchar:
                case TypeTag.Char:
                    return $"{Tag}({MaxLength})";
                default:
                    return Tag.ToString();
            }
        }

        /// <summary>
        /// Hash code.
        /// </summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode()
        {
            return Impl.HashCode.Combine(InternalTypeModifier, Tag, InternalOid);
        }

        /// <summary>
        /// Comparison.
        /// </summary>
        /// <param name="obj">Object to compare with.</param>
        /// <returns>Whether this object compares equal to <c>obj</c>.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as SqlType);
        }

        /// <summary>
        /// Comparison.
        /// </summary>
        /// <param name="other">Object to compare with.</param>
        /// <returns>Whether this object compares equal to <c>other</c>.</returns>
        public bool Equals(SqlType other)
        {
            if (other is null)
                return false;

            return Tag == other.Tag && InternalTypeModifier == other.InternalTypeModifier && InternalOid == other.InternalOid;
        }

        int IComparable.CompareTo(object obj)
        {
            switch (obj)
            {
                case null:
                    return 1;

                case SqlType type:
                    return CompareTo(type);

                default:
                    throw new ArgumentException("can compare SqlType only with SqlType");
            }
        }

        /// <summary>
        /// Comparison.
        /// </summary>
        /// <param name="other">Type to compare with.</param>
        /// <returns>
        /// How the two values compare. The order of <see cref="SqlType"/> instances
        /// is unspecified and may change across versions of the library. It is a total ordering
        /// on the set of <see cref="SqlType"/> instances, but there are no other guarantees.
        /// </returns>
        public int CompareTo(SqlType other)
        {
            if (other is null)
                return 1;

            int cmp = Tag.CompareTo(other.Tag);
            if (cmp != 0)
                return cmp;

            cmp = InternalTypeModifier.CompareTo(other.InternalTypeModifier);
            if (cmp != 0)
                return cmp;

            return InternalOid.CompareTo(other.InternalOid);
        }

        /// <summary>
        /// Compares two <see cref="SqlType"/> values.
        /// </summary>
        /// <param name="type1">Value to compare.</param>
        /// <param name="type2">Value to compare.</param>
        public static bool operator ==(SqlType type1, SqlType type2)
        {
            if (type1 is null)
                return type2 is null;
            else
                return type1.Equals(type2);
        }

        /// <summary>
        /// Compares two <see cref="SqlType"/> values.
        /// </summary>
        /// <param name="type1">Value to compare.</param>
        /// <param name="type2">Value to compare.</param>
        public static bool operator !=(SqlType type1, SqlType type2)
        {
            return !(type1 == type2);
        }

        /// <summary>
        /// Compares two <see cref="SqlType"/> values.
        /// </summary>
        /// <param name="type1">Value to compare.</param>
        /// <param name="type2">Value to compare.</param>
        public static bool operator <(SqlType type1, SqlType type2)
        {
            if (type1 is null)
                return !(type2 is null);

            return type1.CompareTo(type2) < 0;
        }

        /// <summary>
        /// Compares two <see cref="SqlType"/> values.
        /// </summary>
        /// <param name="type1">Value to compare.</param>
        /// <param name="type2">Value to compare.</param>
        public static bool operator <=(SqlType type1, SqlType type2)
        {
            if (type1 is null)
                return true;

            return type1.CompareTo(type2) <= 0;
        }

        /// <summary>
        /// Compares two <see cref="SqlType"/> values.
        /// </summary>
        /// <param name="type1">Value to compare.</param>
        /// <param name="type2">Value to compare.</param>
        public static bool operator >(SqlType type1, SqlType type2)
        {
            return type2 < type1;
        }

        /// <summary>
        /// Compares two <see cref="SqlType"/> values.
        /// </summary>
        /// <param name="type1">Value to compare.</param>
        /// <param name="type2">Value to compare.</param>
        public static bool operator >=(SqlType type1, SqlType type2)
        {
            return type2 <= type1;
        }
    }
}
