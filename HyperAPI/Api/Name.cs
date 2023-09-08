using System;

using Tableau.HyperAPI.Impl;

namespace Tableau.HyperAPI
{
    /// <summary>
    /// Quoted and escaped identifier, to be used in SQL queries.
    /// </summary>
    /// <remarks>
    /// <see cref="Name"/> is implicitly convertible from <c>string</c>, so one can pass raw
    /// unescaped names as strings to methods which expect a <see cref="Name"/>.
    /// </remarks>
    public sealed class Name : IEquatable<Name>, IComparable, IComparable<Name>
    {
        private string Escaped { get; }

        /// <summary>
        /// Constructs a <see cref="Name"/> from an unescaped identifier.
        /// </summary>
        /// <param name="identifier">Identifier.</param>
        public Name(string identifier)
        {
            Util.CheckArgument(!string.IsNullOrEmpty(identifier), "identifier may not be null or empty");
            Unescaped = identifier;
            Escaped = Sql.EscapeNameString(identifier);
        }

        /// <summary>
        /// Gets the original unescaped unquoted identifier.
        /// </summary>
        public string Unescaped { get; }

        /// <summary>
        /// Converts a string to a <see cref="Name"/>.
        /// </summary>
        /// <param name="identifier">Identifier to convert.</param>
        public static implicit operator Name(string identifier)
        {
            return new Name(identifier);
        }

        /// <summary>
        /// Comparison.
        /// </summary>
        /// <param name="obj">Object to compare with.</param>
        /// <returns><c>true</c> if the two objects compare equal.</returns>
        /// <remarks>
        /// Two <see cref="Name"/> objects are equal if and only if the corresponding
        /// names are equal, i.e if <c>this.Unescaped == obj.Unescaped</c>.
        /// </remarks>
        public override bool Equals(object obj)
        {
            return Equals(obj as Name);
        }

        /// <summary>
        /// Comparison.
        /// </summary>
        /// <param name="name">Name to compare with.</param>
        /// <returns><c>true</c> if the two names are equal.</returns>
        /// <remarks>
        /// Two <see cref="Name"/> objects are equal if and only if the corresponding
        /// escaped names are equal, i.e. if <c>this.Unescaped() == obj.Unescaped</c>.
        /// </remarks>
        public bool Equals(Name name)
        {
            if (name == null)
                return false;

            return Unescaped == name.Unescaped;
        }

        /// <summary>
        /// Hash.
        /// </summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode()
        {
            return Unescaped.GetHashCode();
        }

        /// <summary>
        /// String representation, quoted and escaped. This is suitable for passing to string.Format() to compose SQL queries.
        /// </summary>
        public override string ToString()
        {
            return Escaped;
        }

        /// <summary>
        /// Comparison.
        /// </summary>
        int IComparable.CompareTo(object obj)
        {
            switch (obj)
            {
                case null:
                    return 1;

                case Name name:
                    return CompareTo(name);

                default:
                    throw new ArgumentException("can compare Name only with Name");
            }
        }

        /// <summary>
        /// Lexicographic comparison.
        /// </summary>
        /// <param name="other"><see cref="Name"/> instance to compare with.</param>
        /// <returns>How two objects compare, see <c>string.CompareTo()</c>.</returns>
        public int CompareTo(Name other)
        {
            if (other == null)
                return 1;

            return Unescaped.CompareTo(other.Unescaped);
        }

        /// <summary>
        /// Compares two <see cref="Name"/> values.
        /// </summary>
        /// <param name="name1">Value to compare.</param>
        /// <param name="name2">Value to compare.</param>
        public static bool operator ==(Name name1, Name name2)
        {
            if (name1 is null)
                return name2 is null;
            else
                return name1.Equals(name2);
        }

        /// <summary>
        /// Compares two <see cref="Name"/> values.
        /// </summary>
        /// <param name="name1">Value to compare.</param>
        /// <param name="name2">Value to compare.</param>
        public static bool operator !=(Name name1, Name name2)
        {
            return !(name1 == name2);
        }

        /// <summary>
        /// Compares two <see cref="Name"/> values.
        /// </summary>
        /// <param name="name1">Value to compare.</param>
        /// <param name="name2">Value to compare.</param>
        public static bool operator <(Name name1, Name name2)
        {
            if (name1 is null)
                return !(name2 is null);

            return name1.CompareTo(name2) < 0;
        }

        /// <summary>
        /// Compares two <see cref="Name"/> values.
        /// </summary>
        /// <param name="name1">Value to compare.</param>
        /// <param name="name2">Value to compare.</param>
        public static bool operator <=(Name name1, Name name2)
        {
            if (name1 is null)
                return true;

            return name1.CompareTo(name2) <= 0;
        }

        /// <summary>
        /// Compares two <see cref="Name"/> values.
        /// </summary>
        /// <param name="name1">Value to compare.</param>
        /// <param name="name2">Value to compare.</param>
        public static bool operator >(Name name1, Name name2)
        {
            return name2 < name1;
        }

        /// <summary>
        /// Compares two <see cref="Name"/> values.
        /// </summary>
        /// <param name="name1">Value to compare.</param>
        /// <param name="name2">Value to compare.</param>
        public static bool operator >=(Name name1, Name name2)
        {
            return name2 <= name1;
        }
    }
}
