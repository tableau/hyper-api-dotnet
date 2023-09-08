using System;

using Tableau.HyperAPI.Impl;

namespace Tableau.HyperAPI
{
    /// <summary>
    /// Quoted and escaped database name
    /// </summary>
    /// <remarks>
    /// <see cref="DatabaseName"/> is implicitly convertible from <c>string</c> or <see cref="Name"/>, so one can pass raw
    /// unescaped names as strings or <see cref="Name"/> object to methods which expect a <see cref="DatabaseName"/>.
    /// </remarks>
    public sealed class DatabaseName : IEquatable<DatabaseName>, IComparable, IComparable<DatabaseName>
    {
        /// <summary>
        /// Gets the escaped database name.
        /// </summary>
        public Name Name { get; }

        /// <summary>
        /// Constructs a <see cref="DatabaseName"/> from an escaped identifier.
        /// </summary>
        /// <param name="name">database name</param>
        public DatabaseName(Name name)
        {
            Util.CheckArgument(name != null, "Name may not be null");
            this.Name = name;
        }

        /// <summary>
        /// Converts an unescaped string to a <see cref="DatabaseName"/>.
        /// </summary>
        /// <param name="identifier">Identifier to convert.</param>
        public static implicit operator DatabaseName(string identifier)
        {
            return new DatabaseName(identifier);
        }

        /// <summary>
        /// Converts a <see cref="Name"/> to a <see cref="DatabaseName"/>.
        /// </summary>
        /// <param name="identifier">Identifier to convert.</param>
        public static implicit operator DatabaseName(Name identifier)
        {
            return new DatabaseName(identifier);
        }

        /// <summary>
        /// Gets the original unescaped unquoted identifier.
        /// </summary>
        public string Unescaped { get { return Name.Unescaped; } }

        /// <summary>
        /// Comparison.
        /// </summary>
        /// <param name="obj">Object to compare with.</param>
        /// <returns><c>true</c> if the two objects compare equal.</returns>
        /// <remarks>
        /// Two <see cref="DatabaseName"/> objects are equal if and only if the corresponding
        /// names are equal
        /// </remarks>
        public override bool Equals(object obj)
        {
            return Equals(obj as DatabaseName);
        }

        /// <summary>
        /// Comparison.
        /// </summary>
        /// <param name="other">DatabaseName to compare with.</param>
        /// <returns><c>true</c> if the two names are equal.</returns>
        /// <remarks>
        /// Two <see cref="DatabaseName"/> objects are equal if and only if the corresponding
        /// unescaped names are equal
        /// </remarks>
        public bool Equals(DatabaseName other)
        {
            if (other == null)
                return false;

            return Name.Equals(other.Name);
        }

        /// <summary>
        /// Hash.
        /// </summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        /// <summary>
        /// String representation, quoted and escaped. This is suitable for passing to string.Format() to compose SQL queries.
        /// </summary>
        public override string ToString()
        {
            return Name.ToString();
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

                case DatabaseName databaseName:
                    return CompareTo(databaseName);

                default:
                    throw new ArgumentException("can compare DatabaseName only with DatabaseName");
            }
        }

        /// <summary>
        /// Lexicographic comparison.
        /// </summary>
        /// <param name="other"><see cref="DatabaseName"/> instance to compare with.</param>
        /// <returns>How two objects compare</returns>
        public int CompareTo(DatabaseName other)
        {
            if (other == null)
                return 1;

            return Name.CompareTo(other.Name);
        }

        /// <summary>
        /// Compares two <see cref="DatabaseName"/> values.
        /// </summary>
        /// <param name="databaseName1">Value to compare.</param>
        /// <param name="databaseName2">Value to compare.</param>
        public static bool operator ==(DatabaseName databaseName1, DatabaseName databaseName2)
        {
            if (databaseName1 is null)
                return databaseName2 is null;
            else
                return databaseName1.Equals(databaseName2);
        }

        /// <summary>
        /// Compares two <see cref="DatabaseName"/> values.
        /// </summary>
        /// <param name="databaseName1">Value to compare.</param>
        /// <param name="databaseName2">Value to compare.</param>
        public static bool operator !=(DatabaseName databaseName1, DatabaseName databaseName2)
        {
            return !(databaseName1 == databaseName2);
        }

        /// <summary>
        /// Compares two <see cref="DatabaseName"/> values.
        /// </summary>
        /// <param name="databaseName1">Value to compare.</param>
        /// <param name="databaseName2">Value to compare.</param>
        public static bool operator <(DatabaseName databaseName1, DatabaseName databaseName2)
        {
            if (databaseName1 is null)
                return !(databaseName2 is null);

            return databaseName1.CompareTo(databaseName2) < 0;
        }

        /// <summary>
        /// Compares two <see cref="DatabaseName"/> values.
        /// </summary>
        /// <param name="databaseName1">Value to compare.</param>
        /// <param name="databaseName2">Value to compare.</param>
        public static bool operator <=(DatabaseName databaseName1, DatabaseName databaseName2)
        {
            if (databaseName1 is null)
                return true;

            return databaseName1.CompareTo(databaseName2) <= 0;
        }

        /// <summary>
        /// Compares two <see cref="DatabaseName"/> values.
        /// </summary>
        /// <param name="databaseName1">Value to compare.</param>
        /// <param name="databaseName2">Value to compare.</param>
        public static bool operator >(DatabaseName databaseName1, DatabaseName databaseName2)
        {
            return databaseName2 < databaseName1;
        }

        /// <summary>
        /// Compares two <see cref="DatabaseName"/> values.
        /// </summary>
        /// <param name="databaseName1">Value to compare.</param>
        /// <param name="databaseName2">Value to compare.</param>
        public static bool operator >=(DatabaseName databaseName1, DatabaseName databaseName2)
        {
            return databaseName2 <= databaseName1;
        }
    }
}
