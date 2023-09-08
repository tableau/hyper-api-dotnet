using System;

using Tableau.HyperAPI.Impl;

namespace Tableau.HyperAPI
{
    /// <summary>
    /// Quoted and escaped schema name.
    /// </summary>
    /// <remarks>
    /// <see cref="SchemaName"/> is implicitly convertible from <c>string</c> and <see cref="Name"/>, so one
    /// can pass raw unescaped names as strings or <see cref="Name"/> objects to methods which expect a
    /// <see cref="SchemaName"/>.
    /// </remarks>
    public sealed class SchemaName : IEquatable<SchemaName>, IComparable, IComparable<SchemaName>
    {
        /// <summary>
        /// Gets the escaped database name.
        /// </summary>
        public DatabaseName DatabaseName { get; }

        /// <summary>
        /// Gets the escaped schema name.
        /// </summary>
        public Name Name { get; }

        private readonly string fullEscaped;

        /// <summary>
        /// Constructs a non-qualified Schema Name
        /// </summary>
        /// <param name="name">Schema Name.</param>
        public SchemaName(Name name)
            : this(null, name)
        {
        }

        /// <summary>
        /// Converts an unescaped string to a <see cref="SchemaName"/>.
        /// </summary>
        /// <param name="identifier">Identifier to convert.</param>
        public static implicit operator SchemaName(string identifier)
        {
            return new SchemaName(identifier);
        }

        /// <summary>
        /// Converts a <see cref="Name"/> to a <see cref="SchemaName"/>.
        /// </summary>
        /// <param name="identifier">Identifier to convert.</param>
        public static implicit operator SchemaName(Name identifier)
        {
            return new SchemaName(identifier);
        }

        /// <summary>
        /// Constructs a fully qualified Schema name.
        /// </summary>
        /// <param name="databaseName">Database Name.</param>
        /// <param name="name">Schema name.</param>
        public SchemaName(DatabaseName databaseName, Name name)
        {
            Util.CheckArgument(name != null, "Name must not be null");
            DatabaseName = databaseName;
            Name = name;

            if (databaseName != null)
                fullEscaped = databaseName.ToString() + "." + name.ToString();
            else
                fullEscaped = name.ToString();
        }

        /// <summary>
        /// Returns if the SchemaName is fully qualified. i.e It has a database name
        /// </summary>
        public bool IsFullyQualified()
        {
            return DatabaseName != null;
        }

        /// <summary>
        /// Returns the fully escaped and quoted name, like "database"."schema".
        /// </summary>
        public override string ToString()
        {
            return fullEscaped;
        }

        /// <summary>
        /// Comparison.
        /// </summary>
        /// <param name="obj">Object to compare with.</param>
        /// <returns><c>true</c> if the two objects compare equal.</returns>
        /// <remarks>
        /// Two <see cref="SchemaName"/> objects are equal if and only if they have the same
        /// number of components and the corresponding components are equal
        /// </remarks>
        public override bool Equals(object obj)
        {
            return Equals(obj as SchemaName);
        }

        /// <summary>
        /// Comparison.
        /// </summary>
        /// <param name="other">Name to compare with.</param>
        /// <returns><c>true</c> if the two names are equal.</returns>
        /// <remarks>
        /// Two <see cref="SchemaName"/> objects are equal if and only if they have the same
        /// number of components and the corresponding components are equal
        /// </remarks>
        public bool Equals(SchemaName other)
        {
            if (other == null)
                return false;

            return fullEscaped == other.fullEscaped;
        }

        /// <summary>
        /// Hash code.
        /// </summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode()
        {
            return fullEscaped.GetHashCode();
        }

        internal void GetComponents(out string database, out string schema)
        {
            database = DatabaseName?.Unescaped;
            schema = Name.Unescaped;
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

                case SchemaName schemaName:
                    return CompareTo(schemaName);

                default:
                    throw new ArgumentException("can compare SchemaName only with SchemaName");
            }
        }

        /// <summary>
        /// Lexicographic comparison of <see cref="SchemaName"/> A non-qualified SchemaName is considered less that a fully qualified SchemaName.
        /// </summary>
        public int CompareTo(SchemaName other)
        {
            if (other == null)
                return 1;

            if (DatabaseName == null)
            {
                if (other.DatabaseName != null)
                    return -1;

                return Name.CompareTo(other.Name);
            }

            if (other.DatabaseName == null)
                return 1;

            int cmp = DatabaseName.CompareTo(other.DatabaseName);
            if (cmp != 0)
                return cmp;

            return Name.CompareTo(other.Name);
        }

        /// <summary>
        /// Compares two <see cref="SchemaName"/> values.
        /// </summary>
        /// <param name="schemaName1">Value to compare.</param>
        /// <param name="schemaName2">Value to compare.</param>
        public static bool operator ==(SchemaName schemaName1, SchemaName schemaName2)
        {
            if (schemaName1 is null)
                return schemaName2 is null;
            else
                return schemaName1.Equals(schemaName2);
        }

        /// <summary>
        /// Compares two <see cref="SchemaName"/> values.
        /// </summary>
        /// <param name="schemaName1">Value to compare.</param>
        /// <param name="schemaName2">Value to compare.</param>
        public static bool operator !=(SchemaName schemaName1, SchemaName schemaName2)
        {
            return !(schemaName1 == schemaName2);
        }

        /// <summary>
        /// Compares two <see cref="SchemaName"/> values.
        /// </summary>
        /// <param name="schemaName1">Value to compare.</param>
        /// <param name="schemaName2">Value to compare.</param>
        public static bool operator <(SchemaName schemaName1, SchemaName schemaName2)
        {
            if (schemaName1 is null)
                return !(schemaName2 is null);

            return schemaName1.CompareTo(schemaName2) < 0;
        }

        /// <summary>
        /// Compares two <see cref="SchemaName"/> values.
        /// </summary>
        /// <param name="schemaName1">Value to compare.</param>
        /// <param name="schemaName2">Value to compare.</param>
        public static bool operator <=(SchemaName schemaName1, SchemaName schemaName2)
        {
            if (schemaName1 is null)
                return true;

            return schemaName1.CompareTo(schemaName2) <= 0;
        }

        /// <summary>
        /// Compares two <see cref="SchemaName"/> values.
        /// </summary>
        /// <param name="schemaName1">Value to compare.</param>
        /// <param name="schemaName2">Value to compare.</param>
        public static bool operator >(SchemaName schemaName1, SchemaName schemaName2)
        {
            return schemaName2 < schemaName1;
        }

        /// <summary>
        /// Compares two <see cref="SchemaName"/> values.
        /// </summary>
        /// <param name="schemaName1">Value to compare.</param>
        /// <param name="schemaName2">Value to compare.</param>
        public static bool operator >=(SchemaName schemaName1, SchemaName schemaName2)
        {
            return schemaName2 <= schemaName1;
        }
    }
}
