using System;
using System.Collections.Generic;
using System.Linq;

namespace Tableau.HyperAPI
{
    /// <summary>
    /// Schema of a query result.
    /// </summary>
    public class ResultSchema
    {
        /// <summary>
        /// Query result column.
        /// </summary>
        public class Column
        {
            /// <summary>
            /// Column name.
            /// </summary>
            public Name Name { get; }

            /// <summary>
            /// Column type.
            /// </summary>
            public SqlType Type { get; }

            internal Column(string name, SqlType type)
            {
                Name = name;
                Type = type;
            }
        }

        private List<Column> columns;

        /// <summary>
        /// List of the result columns.
        /// </summary>
        public IEnumerable<Column> Columns => columns;

        internal ResultSchema(Raw.TableDefinitionHandle nativeSchema)
        {
            columns = new List<Column>();
            for (int i = 0; i < nativeSchema.ColumnCount; ++i)
            {
                columns.Add(new Column(nativeSchema.ColumnName(i),
                    new SqlType((TypeTag)nativeSchema.ColumnTypeTag(i), nativeSchema.ColumnTypeModifier(i), (uint)nativeSchema.ColumnTypeOid(i))));
            }
        }

        /// <summary>
        /// Column count.
        /// </summary>
        public int ColumnCount => columns.Count;

        /// <summary>
        /// Gets a column by its position.
        /// </summary>
        /// <param name="position">Column position.</param>
        /// <returns>Column at the specified position.</returns>
        public Column GetColumn(int position) => columns[position];

        /// <summary>
        /// Gets a column by its name, returns <c>null</c> if it does not exist.
        /// </summary>
        /// <param name="name">Column name.</param>
        /// <returns>Column object or <c>null</c> if it does not exist.</returns>
        public Column GetColumnByName(Name name) => Columns.FirstOrDefault(c => c.Name.Equals(name));

        /// <summary>
        /// Gets a column position by its name, returns <c>-1</c> if it does not exist.
        /// </summary>
        /// <param name="name">Column name.</param>
        /// <returns>Column position or <c>-1</c> if it does not exist.</returns>
        public int GetColumnPosByName(Name name) => columns.FindIndex(c => c.Name.Equals(name));
    }
}
