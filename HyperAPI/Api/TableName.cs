using System;

using Tableau.HyperAPI.Impl;

namespace Tableau.HyperAPI
{
    /// <summary>
    /// Quoted and escaped table name.
    /// </summary>
    /// <remarks>
    /// <see cref="TableName"/> is implicitly convertible from <c>string</c> and <see cref="Name"/>, so one
    /// can pass raw unescaped names as strings or <see cref="Name"/> objects to methods which expect a
    /// <see cref="TableName"/>.
    /// </remarks>
    public sealed class TableName : IEquatable<TableName>, IComparable, IComparable<TableName>
    {
        /// <summary>
        /// Gets the escaped Schema name.
        /// </summary>
        public SchemaName SchemaName { get; }

        /// <summary>
        /// Gets the escaped Table name.
        /// </summary>
        public Name Name { get; }

        /// <summary>
        ///  Gets the database name
        /// </summary>
        /// <returns> Database Name or null </returns>
        public DatabaseName DatabaseName
        {
            get
            {
                return SchemaName?.DatabaseName;
            }
        }

        private readonly string fullEscaped;

        /// <summary>
        /// Constructs a non-qualified table name.
        /// </summary>
        /// <param name="name">Table Name</param>
        public TableName(Name name)
            : this(null, name)
        {
        }

        /// <summary>
        /// Converts an unescaped string to a <see cref="TableName"/>.
        /// </summary>
        /// <param name="identifier">Identifier to convert.</param>
        public static implicit operator TableName(string identifier)
        {
            return new TableName(new Name(identifier));
        }

        /// <summary>
        /// Converts a <see cref="Name"/> to a <see cref="TableName"/>.
        /// </summary>
        /// <param name="identifier">Identifier to convert.</param>
        public static implicit operator TableName(Name identifier)
        {
            return new TableName(identifier);
        }

        /// <summary>
        /// Constructs a partly qualified table name.
        /// </summary>
        /// <param name="schemaName">Schema Name.</param>
        /// <param name="name">Table Name.</param>
        public TableName(SchemaName schemaName, Name name)
        {
            Util.CheckArgument(name != null, "name must not be null");
            SchemaName = schemaName;
            Name = name;

            if (schemaName != null)
                fullEscaped = schemaName.ToString() + "." + name.ToString();
            else
                fullEscaped = name.ToString();
        }

        /// <summary>
        /// Constructs a fully qualified table name
        /// </summary>
        /// <param name="databaseName">Database Name.</param>
        /// <param name="schemaName">Schema Name.</param>
        /// <param name="name">Table Name.</param>
        public TableName(DatabaseName databaseName, Name schemaName, Name name)
            : this(CheckDatabaseAndSchemaName(databaseName, schemaName), name)
        {
        }

        private static SchemaName CheckDatabaseAndSchemaName(DatabaseName databaseName, Name schemaName)
        {
            Util.CheckArgument(schemaName != null || databaseName == null, "schemaName must not be null if databaseName is not null");
            return schemaName != null ? new SchemaName(databaseName, schemaName) : null;
        }

        /// <summary>
        /// Returns if the TableName is fully qualified. i.e It has a schema name and a database name
        /// </summary>
        public bool IsFullyQualified()
        {
            return SchemaName != null && SchemaName.IsFullyQualified();
        }

        /// <summary>
        /// Returns the fully escaped and quoted name, like "database"."schema"."table".
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
        /// Two <see cref="TableName"/> objects are equal if and only if they have the same
        /// number of components and the corresponding components are equal
        /// </remarks>
        public override bool Equals(object obj)
        {
            return Equals(obj as TableName);
        }

        /// <summary>
        /// Comparison.
        /// </summary>
        /// <param name="other">TableName to compare with.</param>
        /// <returns><c>true</c> if the two names are equal.</returns>
        /// <remarks>
        /// Two <see cref="TableName"/> objects are equal if and only if they have the same
        /// number of components and the corresponding components are equal
        /// </remarks>
        public bool Equals(TableName other)
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

        internal void GetComponents(out string database, out string schema, out string table)
        {
            if (SchemaName != null)
            {
                SchemaName.GetComponents(out database, out schema);
            }
            else
            {
                database = null;
                schema = null;
            }

            table = Name.Unescaped;
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

                case TableName tableName:
                    return CompareTo(tableName);

                default:
                    throw new ArgumentException("can compare TableName only with TableName");
            }
        }

        /// <summary>
        /// Lexicographic comparison of <see cref="TableName"/> A non-qualified table name is considered less that partly qualified Table Name.
        /// Similarly a partly qualified TableName is considered less than fully qualified Table Name.
        /// </summary>
        public int CompareTo(TableName other)
        {
            if (other == null)
                return 1;

            if (SchemaName == null)
            {
                if (other.SchemaName != null)
                    return -1;

                return Name.CompareTo(other.Name);
            }

            if (other.SchemaName == null)
                return 1;

            int cmp = SchemaName.CompareTo(other.SchemaName);
            if (cmp != 0)
                return cmp;

            return Name.CompareTo(other.Name);
        }

        /// <summary>
        /// Compares two <see cref="TableName"/> values.
        /// </summary>
        /// <param name="tableName1">Value to compare.</param>
        /// <param name="tableName2">Value to compare.</param>
        public static bool operator ==(TableName tableName1, TableName tableName2)
        {
            if (tableName1 is null)
                return tableName2 is null;
            else
                return tableName1.Equals(tableName2);
        }

        /// <summary>
        /// Compares two <see cref="TableName"/> values.
        /// </summary>
        /// <param name="tableName1">Value to compare.</param>
        /// <param name="tableName2">Value to compare.</param>
        public static bool operator !=(TableName tableName1, TableName tableName2)
        {
            return !(tableName1 == tableName2);
        }

        /// <summary>
        /// Compares two <see cref="TableName"/> values.
        /// </summary>
        /// <param name="tableName1">Value to compare.</param>
        /// <param name="tableName2">Value to compare.</param>
        public static bool operator <(TableName tableName1, TableName tableName2)
        {
            if (tableName1 is null)
                return !(tableName2 is null);

            return tableName1.CompareTo(tableName2) < 0;
        }

        /// <summary>
        /// Compares two <see cref="TableName"/> values.
        /// </summary>
        /// <param name="tableName1">Value to compare.</param>
        /// <param name="tableName2">Value to compare.</param>
        public static bool operator <=(TableName tableName1, TableName tableName2)
        {
            if (tableName1 is null)
                return true;

            return tableName1.CompareTo(tableName2) <= 0;
        }

        /// <summary>
        /// Compares two <see cref="TableName"/> values.
        /// </summary>
        /// <param name="tableName1">Value to compare.</param>
        /// <param name="tableName2">Value to compare.</param>
        public static bool operator >(TableName tableName1, TableName tableName2)
        {
            return tableName2 < tableName1;
        }

        /// <summary>
        /// Compares two <see cref="TableName"/> values.
        /// </summary>
        /// <param name="tableName1">Value to compare.</param>
        /// <param name="tableName2">Value to compare.</param>
        public static bool operator >=(TableName tableName1, TableName tableName2)
        {
            return tableName2 <= tableName1;
        }
    }
}
