using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;

using Tableau.HyperAPI.Raw;
using Tableau.HyperAPI.Impl;

namespace Tableau.HyperAPI
{
    /// <summary>
    /// Result of a query.
    /// </summary>
    /// <remarks>
    /// Result object allows reading data row by row. <see cref="NextRow"/> advances to the next row,
    /// <see cref="IsNull(int)"/> checks whether the value at the given index is NULL, and
    /// <c>Get*(int)</c> methods retrieve values.
    /// <para>
    /// Alternatively one can use <see cref="GetValues(object[])"/> and <see cref="GetValues()"/>
    /// to retrieve all row values as an array of objects, or <see cref="GetValue(int)"/> to
    /// retrieve individual values.
    /// </para>
    /// </remarks>
    public sealed class Result : IDisposable
    {
        private ResultHandle nativeHandle;

        /// <summary>
        /// Schema of the result.
        /// </summary>
        public ResultSchema Schema { get; }

        private class ColumnInfo
        {
            public Name Name;
            public SqlType Type;

            public ColumnInfo(ResultSchema.Column column)
            {
                Name = column.Name;
                Type = column.Type;
            }
        }

        private int columnCount;
        private ColumnInfo[] columns;

        private Chunk currentChunk;

        internal static decimal[] PowersOfTen = new decimal[]
        {
            1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000, 1000000000,
            10000000000, 100000000000, 1000000000000, 10000000000000, 100000000000000,
            1000000000000000, 10000000000000000, 100000000000000000, 1000000000000000000,
            10000000000000000000,decimal.Parse("100000000000000000000"),decimal.Parse("1000000000000000000000"),
            decimal.Parse("10000000000000000000000"),decimal.Parse("100000000000000000000000"),decimal.Parse("1000000000000000000000000"),
            decimal.Parse("10000000000000000000000000"),decimal.Parse("100000000000000000000000000"),decimal.Parse("1000000000000000000000000000"),
            decimal.Parse("10000000000000000000000000000")
        };

        private struct Chunk
        {
            private IntPtr pChunk;
            private int currentRow;
            private int currentRowStart;
            private long rowCount;
            private long valueCount;
            private long columnCount;
            private IntPtr values;
            private IntPtr sizes;

            public bool IsNull => pChunk == IntPtr.Zero;
            public bool HasRows => rowCount > 0;

            public void Init(ResultHandle handle)
            {
                Destroy();

                pChunk = handle.NextChunk();
                if (pChunk == IntPtr.Zero)
                    return;

                ulong uRowCount;
                ulong uColumnCount;
                Dll.hyper_rowset_chunk_field_values(pChunk, out uColumnCount, out uRowCount, out values, out sizes);
                rowCount = (long)uRowCount;
                columnCount = (long)uColumnCount;

                if (rowCount == 0)
                {
                    Destroy();
                    return;
                }

                if (rowCount * columnCount > int.MaxValue)
                {
                    Destroy();
                    throw new Exception("too many values");
                }

                valueCount = rowCount * columnCount;
            }

            public void Destroy()
            {
                if (!IsNull)
                {
                    Dll.hyper_destroy_rowset_chunk(pChunk);
                    pChunk = IntPtr.Zero;
                    valueCount = 0;
                    rowCount = 0;
                    columnCount = 0;
                    values = IntPtr.Zero;
                    sizes = IntPtr.Zero;
                    currentRowStart = 0;
                    currentRow = 0;
                }
            }

            public bool NextRow()
            {
                Debug.Assert(rowCount > 0);
                currentRow += 1;
                currentRowStart += (int)columnCount;
                return currentRow < rowCount;
            }

            public bool IsValueNull(int column)
            {
                unsafe
                {
                    int idx = currentRowStart + column;
                    Debug.Assert(idx < valueCount);

                    return ((void**)values)[idx] == null;
                }
            }

            public Dll.hyper_value_t GetValue(int column)
            {
                unsafe
                {
                    int idx = currentRowStart + column;
                    Debug.Assert(idx < valueCount);

                    if (((void**)values)[idx] == null)
                        return new Dll.hyper_value_t();

                    void** v = (void**)values;
                    ulong* s = (ulong*)sizes;
                    return new Dll.hyper_value_t
                    {
                        value = (IntPtr)v[idx],
                        size = s[idx],
                    };
                }
            }
        }

        internal Result(Connection connection, ResultHandle nativeHandle)
        {
            Util.Verify(nativeHandle != null);
            this.nativeHandle = nativeHandle;

            try
            {
                Connection = connection;
                Schema = nativeHandle.Schema;
                columnCount = Schema.ColumnCount;
                columns = Schema.Columns.Select(c => new ColumnInfo(c)).ToArray();
            }
            catch
            {
                nativeHandle.Dispose();
                throw;
            }
        }

        private void CheckIsOpen()
        {
            if (!IsOpen)
                throw new ObjectDisposedException("result", "result is closed");
        }

        /// <summary>
        /// Returns <c>true</c> if the result has not been closed yet.
        /// </summary>
        public bool IsOpen => nativeHandle != null;

        /// <summary>
        /// Closes the result object.
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Gets affected row count if it is available, or -1 otherwise.
        /// </summary>
        public int AffectedRowCount
        {
            get
            {
                CheckIsOpen();
                return nativeHandle.GetAffectedRowCount();
            }
        }

        /// <summary>
        /// Gets the connection of the SQL statement that yielded this result.
        /// </summary>
        public Connection Connection { get; }

        /// <summary>
        /// Advances to the next row.
        /// </summary>
        /// <returns><c>true</c> if there is a row to read, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// After this method returned <c>false</c>, the result object is closed and may not be used anymore.
        /// </remarks>
        public bool NextRow()
        {
            if (!IsOpen)
                return false;

            if (currentChunk.IsNull || !currentChunk.NextRow())
            {
                while (true)
                {
                    currentChunk.Init(nativeHandle);

                    if (currentChunk.IsNull)
                    {
                        Close();
                        return false;
                    }

                    if (currentChunk.HasRows)
                        break;
                }
            }

            return true;
        }

        private void CheckColumnIndex(int columnIndex)
        {
            CheckNotDisposed();
            if (columnIndex < 0 || columnIndex >= columnCount)
                throw new IndexOutOfRangeException($"Invalid column index {columnIndex}, must be in the range from 0 to {columnCount - 1}");
        }

        private void CheckCurrentChunkPresent()
        {
            if (currentChunk.IsNull)
                throw new InvalidOperationException("NextRow() has not been called yet");
        }

        /// <summary>
        /// Is the value at the given column NULL?
        /// </summary>
        public bool IsNull(int columnIndex)
        {
            CheckBeforeGet(columnIndex);
            return currentChunk.IsValueNull(columnIndex);
        }

        private Dll.hyper_value_t GetRawValue(int columnIndex)
        {
            Dll.hyper_value_t value = currentChunk.GetValue(columnIndex);
            if (value.value == IntPtr.Zero)
                throw new InvalidCastException($"attempted to convert a NULL value to type {columns[columnIndex].Type.Tag}");
            return value;
        }

        private void CheckBeforeGet(int columnIndex)
        {
            CheckIsOpen();
            CheckCurrentChunkPresent();
            CheckColumnIndex(columnIndex);
        }

        private InvalidCastException TypeMismatchException(int columnIndex, string type)
        {
            return new InvalidCastException($"attempted to get value of type {type} from the column '{columns[columnIndex].Name}' of type {columns[columnIndex].Type.Tag}");
        }

        /// <summary>
        /// Gets a bool value from the specified column.
        /// </summary>
        /// <param name="columnIndex">Column index.</param>
        /// <returns>Value in the column.</returns>
        /// <remarks>
        /// This method throws an <c>InvalidCastException</c> if the column is not of type <see cref="TypeTag.Bool"/>.
        /// It also throws an <c>InvalidCastException</c> if the value is NULL. Use <see cref="IsNull(int)"/>
        /// to check whether the value is NULL if the column may contain NULL values.
        /// </remarks>
        public bool GetBool(int columnIndex)
        {
            CheckBeforeGet(columnIndex);

            switch (columns[columnIndex].Type.Tag)
            {
                case TypeTag.Bool:
                    return GetBoolUnchecked(columnIndex);
            }

            throw TypeMismatchException(columnIndex, "bool");
        }

        /// <summary>
        /// Gets a short value from the specified column.
        /// </summary>
        /// <param name="columnIndex">Column index.</param>
        /// <returns>Value in the column.</returns>
        /// <remarks>
        /// This method throws an <c>InvalidCastException</c> if the column is not of type <see cref="TypeTag.SmallInt"/>.
        /// It also throws an <c>InvalidCastException</c> if the value is NULL. Use <see cref="IsNull(int)"/>
        /// to check whether the value is NULL if the column may contain NULL values.
        /// </remarks>
        public short GetShort(int columnIndex)
        {
            CheckBeforeGet(columnIndex);

            switch (columns[columnIndex].Type.Tag)
            {
                case TypeTag.SmallInt:
                    return GetShortUnchecked(columnIndex);
            }

            throw TypeMismatchException(columnIndex, "short");
        }

        /// <summary>
        /// Gets an int value from the specified column.
        /// </summary>
        /// <param name="columnIndex">Column index.</param>
        /// <returns>Value in the column.</returns>
        /// <remarks>
        /// This method throws an <c>InvalidCastException</c> if the column is not of type <see cref="TypeTag.Int"/> or <see cref="TypeTag.SmallInt"/>.
        /// It also throws an <c>InvalidCastException</c> if the value is NULL. Use <see cref="IsNull(int)"/>
        /// to check whether the value is NULL if the column may contain NULL values.
        /// </remarks>
        public int GetInt(int columnIndex)
        {
            CheckBeforeGet(columnIndex);

            switch (columns[columnIndex].Type.Tag)
            {
                case TypeTag.SmallInt:
                    return GetShortUnchecked(columnIndex);

                case TypeTag.Int:
                    return GetIntUnchecked(columnIndex);
            }

            throw TypeMismatchException(columnIndex, "int");
        }

        /// <summary>
        /// Gets an int value from the specified column.
        /// </summary>
        /// <param name="columnIndex">Column index.</param>
        /// <returns>Value in the column.</returns>
        /// <remarks>
        /// This method throws an <c>InvalidCastException</c> if the column is not of type <see cref="TypeTag.Oid"/>.
        /// It also throws an <c>InvalidCastException</c> if the value is NULL. Use <see cref="IsNull(int)"/>
        /// to check whether the value is NULL if the column may contain NULL values.
        /// </remarks>
        public uint GetUint(int columnIndex)
        {
            CheckBeforeGet(columnIndex);

            switch (columns[columnIndex].Type.Tag)
            {
                case TypeTag.Oid:
                    return GetUintUnchecked(columnIndex);
            }

            throw TypeMismatchException(columnIndex, "uint");
        }

        /// <summary>
        /// Gets a long value from the specified column.
        /// </summary>
        /// <param name="columnIndex">Column index.</param>
        /// <returns>Value in the column.</returns>
        /// <remarks>
        /// This method throws an <c>InvalidCastException</c> if the column is not of type <see cref="TypeTag.SmallInt"/>, <see cref="TypeTag.Int"/> or <see cref="TypeTag.BigInt"/>.
        /// It also throws an <c>InvalidCastException</c> if the value is NULL. Use <see cref="IsNull(int)"/>
        /// to check whether the value is NULL if the column may contain NULL values.
        /// </remarks>
        public long GetLong(int columnIndex)
        {
            CheckBeforeGet(columnIndex);

            switch (columns[columnIndex].Type.Tag)
            {
                case TypeTag.SmallInt:
                    return GetShortUnchecked(columnIndex);

                case TypeTag.Int:
                    return GetIntUnchecked(columnIndex);

                case TypeTag.Oid:
                    return GetUintUnchecked(columnIndex);

                case TypeTag.BigInt:
                    return GetLongUnchecked(columnIndex);

                case TypeTag.Numeric:
                    return (long)GetNumericUnchecked(columnIndex);
            }

            throw TypeMismatchException(columnIndex, "long");
        }

        /// <summary>
        /// Gets a double value from the specified column.
        /// </summary>
        /// <param name="columnIndex">Column index.</param>
        /// <returns>Value in the column.</returns>
        /// <remarks>
        /// This method throws an <c>InvalidCastException</c> if the column is not of type <see cref="TypeTag.SmallInt" />, <see cref="TypeTag.Int"/>, <see cref="TypeTag.BigInt"/>, <see cref="TypeTag.Numeric"/> or <see cref="TypeTag.Double"/>.
        /// It also throws an <c>InvalidCastException</c> if the value is NULL. Use <see cref="IsNull(int)"/>
        /// to check whether the value is NULL if the column may contain NULL values.
        /// </remarks>
        public double GetDouble(int columnIndex)
        {
            CheckBeforeGet(columnIndex);

            switch (columns[columnIndex].Type.Tag)
            {
                case TypeTag.SmallInt:
                    return GetShortUnchecked(columnIndex);

                case TypeTag.Int:
                    return GetIntUnchecked(columnIndex);

                case TypeTag.Oid:
                    return GetUintUnchecked(columnIndex);

                case TypeTag.BigInt:
                    return GetLongUnchecked(columnIndex);

                case TypeTag.Numeric:
                    return (double)GetNumericUnchecked(columnIndex);

                case TypeTag.Double:
                    return GetDoubleUnchecked(columnIndex);
            }

            throw TypeMismatchException(columnIndex, "long");
        }

        /// <summary>
        /// Gets a decimal value from the specified column.
        /// </summary>
        /// <param name="columnIndex">Column index.</param>
        /// <returns>Value in the column.</returns>
        /// <remarks>
        /// This method throws an <c>InvalidCastException</c> if the column is not of type <see cref="TypeTag.SmallInt"/>, <see cref="TypeTag.Int"/>, <see cref="TypeTag.BigInt"/> or <see cref="TypeTag.Numeric"/>.
        /// It also throws an <c>InvalidCastException</c> if the value is NULL. Use <see cref="IsNull(int)"/>
        /// to check whether the value is NULL if the column may contain NULL values.
        /// </remarks>
        public decimal GetDecimal(int columnIndex)
        {
            CheckBeforeGet(columnIndex);

            switch (columns[columnIndex].Type.Tag)
            {
                case TypeTag.SmallInt:
                    return GetShortUnchecked(columnIndex);

                case TypeTag.Int:
                    return GetIntUnchecked(columnIndex);

                case TypeTag.Oid:
                    return GetUintUnchecked(columnIndex);

                case TypeTag.BigInt:
                    return GetLongUnchecked(columnIndex);

                case TypeTag.Numeric:
                    return GetNumericUnchecked(columnIndex);
            }

            throw TypeMismatchException(columnIndex, "decimal");
        }

        /// <summary>
        /// Gets a <see cref="Date"/> value from the specified column.
        /// </summary>
        /// <param name="columnIndex">Column index.</param>
        /// <returns>Value in the column.</returns>
        /// <remarks>
        /// This method throws an <c>InvalidCastException</c> if the column is not of type <see cref="TypeTag.Date"/>.
        /// It also throws an <c>InvalidCastException</c> if the value is NULL. Use <see cref="IsNull(int)"/>
        /// to check whether the value is NULL if the column may contain NULL values.
        /// </remarks>
        public Date GetDate(int columnIndex)
        {
            CheckBeforeGet(columnIndex);

            switch (columns[columnIndex].Type.Tag)
            {
                case TypeTag.Date:
                    return GetDateUnchecked(columnIndex);
            }

            throw TypeMismatchException(columnIndex, "Date");
        }

        /// <summary>
        /// Gets a TimeSpan value from the specified column.
        /// </summary>
        /// <param name="columnIndex">Column index.</param>
        /// <returns>Value in the column.</returns>
        /// <remarks>
        /// This method throws an <c>InvalidCastException</c> if the column is not of type <see cref="TypeTag.Time"/>.
        /// It also throws an <c>InvalidCastException</c> if the value is NULL. Use <see cref="IsNull(int)"/>
        /// to check whether the value is NULL if the column may contain NULL values.
        /// </remarks>
        public TimeSpan GetTime(int columnIndex)
        {
            CheckBeforeGet(columnIndex);

            switch (columns[columnIndex].Type.Tag)
            {
                case TypeTag.Time:
                    return GetTimeUnchecked(columnIndex);
            }

            throw TypeMismatchException(columnIndex, "TimeSpan");
        }

        /// <summary>
        /// Gets a <see cref="Timestamp"/> value from the specified column.
        /// </summary>
        /// <param name="columnIndex">Column index.</param>
        /// <returns>Value in the column.</returns>
        /// <remarks>
        /// <para>
        /// <c>Timestamp</c> does not contain time zone information. However, the <c>Kind</c> property
        /// indicates whether the value represents local time, Coordinated Universal Time (UTC), or neither.
        /// For a <c>TIMESTAMP_TZ</c> column, the value is returned in UTC with <c>Kind</c> set to <c>Utc</c>.
        /// For a <c>TIMESTAMP</c> column, the value is returned unchanged with <c>Kind</c> set to <c>Unspecified</c>.
        /// </para>
        /// <para>
        /// This method throws an <c>InvalidCastException</c> if the column is not of type <see cref="TypeTag.Timestamp"/> or <see cref="TypeTag.TimestampTZ"/>.
        /// It also throws an <c>InvalidCastException</c> if the value is NULL. Use <see cref="IsNull(int)"/>
        /// to check whether the value is NULL if the column may contain NULL values.
        /// </para>
        /// </remarks>
        public Timestamp GetTimestamp(int columnIndex)
        {
            CheckBeforeGet(columnIndex);

            switch (columns[columnIndex].Type.Tag)
            {
                case TypeTag.Timestamp:
                    return GetTimestampUnchecked(columnIndex);
                case TypeTag.TimestampTZ:
                    return GetTimestampTzUnchecked(columnIndex);
            }

            throw TypeMismatchException(columnIndex, "Timestamp");
        }

        /// <summary>
        /// Gets a <see cref="DateTime"/> value from the specified column.
        /// </summary>
        /// <param name="columnIndex">Column index.</param>
        /// <returns>Value in the column.</returns>
        /// <remarks>
        /// <para>
        /// <c>DateTime</c> does not contain time zone information. However, the <c>Kind</c> property
        /// indicates whether the value represents local time, Coordinated Universal Time (UTC), or neither.
        /// For a <c>TIMESTAMP_TZ</c> column, the value is returned in UTC with <c>Kind</c> set to <c>Utc</c>.
        /// For a <c>TIMESTAMP</c> column, the value is returned unchanged with <c>Kind</c> set to <c>Unspecified</c>.
        /// </para>
        /// <para>
        /// This method throws an <c>InvalidCastException</c> if the column is not of type <see cref="TypeTag.Timestamp"/> or <see cref="TypeTag.TimestampTZ"/>.
        /// It also throws an <c>InvalidCastException</c> if the value is NULL. Use <see cref="IsNull(int)"/>
        /// to check whether the value is NULL if the column may contain NULL values.
        /// </para>
        /// </remarks>
        public DateTime GetDateTime(int columnIndex)
        {
            CheckBeforeGet(columnIndex);

            switch (columns[columnIndex].Type.Tag)
            {
                case TypeTag.Timestamp:
                    return (DateTime)GetTimestampUnchecked(columnIndex);
                case TypeTag.TimestampTZ:
                    return (DateTime)GetTimestampTzUnchecked(columnIndex);
            }

            throw TypeMismatchException(columnIndex, "DateTime");
        }

        /// <summary>
        /// Gets a <see cref="DateTimeOffset"/> value from the specified column.
        /// </summary>
        /// <param name="columnIndex">Column index.</param>
        /// <returns>Value in the column.</returns>
        /// <remarks>
        /// <para>
        /// <c>DateTimeOffset</c> is time-zone-aware. It represents a date and time value together with a time zone offset.
        /// Thus, the value always unambiguously identifies a single point in time.
        /// For a <c>TIMESTAMP_TZ</c> column, the value is returned in UTC with the offset set to 0.
        /// </para>
        /// <para>
        /// This method throws an <c>InvalidCastException</c> if the column is not of type <see cref="TypeTag.TimestampTZ"/>.
        /// It also throws an <c>InvalidCastException</c> if the value is NULL. Use <see cref="IsNull(int)"/>
        /// to check whether the value is NULL if the column may contain NULL values.
        /// </para>
        /// </remarks>
        public DateTimeOffset GetDateTimeOffset(int columnIndex)
        {
            CheckBeforeGet(columnIndex);

            switch (columns[columnIndex].Type.Tag)
            {
                case TypeTag.TimestampTZ:
                    return (DateTimeOffset)GetTimestampTzUnchecked(columnIndex);
            }

            throw TypeMismatchException(columnIndex, "DateTimeOffset");
        }

        /// <summary>
        /// Gets an <see cref="Interval"/> value from the specified column.
        /// </summary>
        /// <param name="columnIndex">Column index.</param>
        /// <returns>Value in the column.</returns>
        /// <remarks>
        /// This method throws an <c>InvalidCastException</c> if the column is not of type <see cref="TypeTag.Interval"/>.
        /// It also throws an <c>InvalidCastException</c> if the value is NULL. Use <see cref="IsNull(int)"/>
        /// to check whether the value is NULL if the column may contain NULL values.
        /// </remarks>
        public Interval GetInterval(int columnIndex)
        {
            CheckBeforeGet(columnIndex);

            switch (columns[columnIndex].Type.Tag)
            {
                case TypeTag.Interval:
                    return GetIntervalUnchecked(columnIndex);
            }

            throw TypeMismatchException(columnIndex, "Interval");
        }

        /// <summary>
        /// Gets a String value from the specified column.
        /// </summary>
        /// <param name="columnIndex">Column index.</param>
        /// <returns>Value in the column.</returns>
        /// <remarks>
        /// This method throws an <c>InvalidCastException</c> if the column is not of type <see cref="TypeTag.Text"/>, <see cref="TypeTag.Varchar"/>, <see cref="TypeTag.Json"/> or <see cref="TypeTag.Text"/>.
        /// It also throws an <c>InvalidCastException</c> if the value is NULL. Use <see cref="IsNull(int)"/>
        /// to check whether the value is NULL if the column may contain NULL values.
        /// </remarks>
        public string GetString(int columnIndex)
        {
            CheckBeforeGet(columnIndex);

            switch (columns[columnIndex].Type.Tag)
            {
                case TypeTag.Text:
                case TypeTag.Varchar:
                case TypeTag.Json:
                case TypeTag.Char:
                    return GetStringUnchecked(columnIndex);

            }

            throw TypeMismatchException(columnIndex, "string");
        }

        /// <summary>
        /// Gets a byte[] value from the specified column.
        /// </summary>
        /// <param name="columnIndex">Column index.</param>
        /// <returns>Value in the column.</returns>
        /// <remarks>
        /// This method throws an <c>InvalidCastException</c> if the column is not of type <see cref="TypeTag.Bytes"/> or <see cref="TypeTag.Geography"/>.
        /// It also throws an <c>InvalidCastException</c> if the value is NULL. Use <see cref="IsNull(int)"/>
        /// to check whether the value is NULL if the column may contain NULL values.
        /// </remarks>
        public byte[] GetBytes(int columnIndex)
        {
            CheckBeforeGet(columnIndex);

            switch (columns[columnIndex].Type.Tag)
            {
                case TypeTag.Varchar:
                case TypeTag.Text:
                case TypeTag.Json:
                case TypeTag.Bytes:
                case TypeTag.Geography:
                case TypeTag.Unsupported:
                    return GetBytesUnchecked(columnIndex);

                case TypeTag.Char:
                    return GetBytesUnchecked(columnIndex);
            }

            throw TypeMismatchException(columnIndex, "Bytes");
        }

        /// <summary>
        /// Copies array of bytes from a byte[] value from the specified column.
        /// </summary>
        /// <param name="columnIndex">Column index.</param>
        /// <param name="dataOffset">Copy starting from this offset.</param>
        /// <param name="buffer">Copy into this buffer.</param>
        /// <param name="bufferOffset">Copy into the output buffer starting at this offset.</param>
        /// <param name="length">Copy no more than this many bytes.</param>
        /// <returns>Number of copied bytes.</returns>
        /// <remarks>
        /// This method throws an <c>InvalidCastException</c> if the column is not of type <see cref="TypeTag.Bytes"/> or <see cref="TypeTag.Geography"/>.
        /// It also throws an <c>InvalidCastException</c> if the value is NULL. Use <see cref="IsNull(int)"/>
        /// to check whether the value is NULL if the column may contain NULL values.
        /// </remarks>
        public long GetBytes(int columnIndex, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            CheckBeforeGet(columnIndex);

            switch (columns[columnIndex].Type.Tag)
            {
                case TypeTag.Varchar:
                case TypeTag.Text:
                case TypeTag.Json:
                case TypeTag.Bytes:
                case TypeTag.Geography:
                case TypeTag.Unsupported:
                    return GetBytesUnchecked(columnIndex, dataOffset, buffer, bufferOffset, length);

                case TypeTag.Char:
                    return GetBytesUnchecked(columnIndex, dataOffset, buffer, bufferOffset, length);
            }

            throw TypeMismatchException(columnIndex, "Bytes");
        }

        /// <summary>
        /// Gets a value from the specified column as an array of bytes.
        /// </summary>
        /// <param name="columnIndex">Column index.</param>
        /// <returns>Value in the column.</returns>
        /// <remarks>
        /// This method throws an <c>InvalidCastException</c> if the value is NULL. Use <see cref="IsNull(int)"/>
        /// to check whether the value is NULL if the column may contain NULL values.
        /// </remarks>
        public byte[] GetRaw(int columnIndex)
        {
            CheckBeforeGet(columnIndex);
            return GetBytesUnchecked(columnIndex);
        }

        /// <summary>
        /// Copies array of bytes from the value from the specified column.
        /// </summary>
        /// <param name="columnIndex">Column index.</param>
        /// <param name="dataOffset">Copy starting from this offset.</param>
        /// <param name="buffer">Copy into this buffer.</param>
        /// <param name="bufferOffset">Copy into the output buffer starting at this offset.</param>
        /// <param name="length">Copy no more than this many bytes.</param>
        /// <returns>Number of copied bytes.</returns>
        /// <remarks>
        /// This method throws an <c>InvalidCastException</c> if the value is NULL. Use <see cref="IsNull(int)"/>
        /// to check whether the value is NULL if the column may contain NULL values.
        /// </remarks>
        public long GetRaw(int columnIndex, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            CheckBeforeGet(columnIndex);
            return GetBytesUnchecked(columnIndex, dataOffset, buffer, bufferOffset, length);
        }

        /// <summary>
        /// Gets the pointer to the internal value data as a byte pointer.
        /// </summary>
        /// <param name="columnIndex">Column index.</param>
        /// <param name="size">Set to the value size.</param>
        /// <returns>Pointer to the value.</returns>
        /// <remarks>
        /// This method throws an <c>InvalidCastException</c> if the value is NULL. Use <see cref="IsNull(int)"/>
        /// to check whether the value is NULL if the column may contain NULL values.
        /// </remarks>
        public unsafe byte* GetRawUnsafe(int columnIndex, out ulong size)
        {
            Dll.hyper_value_t v = GetRawValue(columnIndex);
            if (v.size > int.MaxValue)
                throw new HyperException("field is too big", new HyperException.ContextId(0xdd826ab8));
            size = v.size;
            return (byte*)v.value;
        }

        private bool GetBoolUnchecked(int columnIndex)
        {
            Dll.hyper_value_t v = GetRawValue(columnIndex);
            Debug.Assert(v.size == 1);
            unsafe
            {
                return *(byte*)v.value != 0;
            }
        }

        private short GetShortUnchecked(int columnIndex)
        {
            Dll.hyper_value_t v = GetRawValue(columnIndex);
            Debug.Assert(v.size == 2);
            unsafe
            {
                return *(short*)v.value;
            }
        }

        private int GetIntUnchecked(int columnIndex)
        {
            Dll.hyper_value_t v = GetRawValue(columnIndex);
            Debug.Assert(v.size == 4);
            unsafe
            {
                return *(int*)v.value;
            }
        }

        private uint GetUintUnchecked(int columnIndex)
        {
            return (uint)GetIntUnchecked(columnIndex);
        }

        private long GetLongUnchecked(int columnIndex)
        {
            Dll.hyper_value_t v = GetRawValue(columnIndex);
            Debug.Assert(v.size == 8);
            unsafe
            {
                return *(long*)v.value;
            }
        }

        private ulong GetUlongUnchecked(int columnIndex)
        {
            return (ulong)GetLongUnchecked(columnIndex);
        }

        private double GetDoubleUnchecked(int columnIndex)
        {
            Dll.hyper_value_t v = GetRawValue(columnIndex);
            Debug.Assert(v.size == 8);
            unsafe
            {
                return *(double*)v.value;
            }
        }

        private Interval GetIntervalUnchecked(int columnIndex)
        {
            Dll.hyper_value_t v = GetRawValue(columnIndex);
            Debug.Assert(v.size == 16);
            unsafe
            {
                return *(Interval*)v.value;
            }
        }

        private UInt32 addOne(UInt32 val, out bool overflow)
        {
            overflow = val == UInt32.MaxValue;
            return val + 1;
        }

        private decimal GetNumericUnchecked(int columnIndex)
        {
            Dll.hyper_value_t v = GetRawValue(columnIndex);
            int scale = columns[columnIndex].Type.Scale;
            if (scale > 28)
            {
                // C# numerics cannot represent this value
                throw new OverflowException("Numeric to decimal conversion error: C# decimals can at most have a scale of 28. The requested numeric column has scale: " + scale);
            }

            if (columns[columnIndex].Type.Precision <= 18)
            {
                // 64-bit numeric
                Debug.Assert(v.size == 8);
                long value;
                unsafe
                {
                    value = *(long*)v.value;
                }
                return value /  PowersOfTen[scale];
            }
            else
            {
                // 128-bit numeric
                Debug.Assert(v.size == 16);
                // Transform the 128-bit Hyper numeric into a C# decimal:
                // The first 96-bit are the absolute value of the decimal
                // The last 32-bit contain the sign flag, some padding zeroes and the scale:
                // SIGN-BIT | 0...0 (7 bit) | Scale (8 bit) | 0...0 (16 bit)
                int[] decimalRaw = new int[4];
                unsafe
                {
                    int highInt = *((int*)v.value+3);
                    if (highInt == 0)
                    {
                        // Positive number that fits 96 bits
                        decimalRaw[0] = *((int*)v.value);
                        decimalRaw[1] = *((int*)v.value+1);
                        decimalRaw[2] = *((int*)v.value+2);
                        // Set Scale
                        decimalRaw[3] = columns[columnIndex].Type.Scale << 16;
                    } else if (highInt == -1)
                    {
                        // Negative number that fits 96 bits
                        // Take the two-complement since C# decimals store the absolute value + a sign flag
                        decimalRaw[0] = ~*((int*)v.value);
                        decimalRaw[1] = ~*((int*)v.value+1);
                        decimalRaw[2] = ~*((int*)v.value+2);
                        bool overflow;
                        decimalRaw[0] = (int)addOne((UInt32)decimalRaw[0], out overflow);
                        if (overflow) decimalRaw[1] = (int)addOne((UInt32)decimalRaw[1], out overflow);
                        if (overflow) decimalRaw[2] = (int)addOne((UInt32)decimalRaw[2], out overflow);
                        if (overflow) throw new OverflowException("Numeric to decimal conversion overflow");
                        // Set Scale
                        decimalRaw[3] = columns[columnIndex].Type.Scale << 16;
                        // Set sign bit
                        decimalRaw[3] |= (1 << 31);
                    }
                    else
                    {
                        // Absolute value of the numeric exceeds 96-bits, C# numerics cannot represent this value
                        throw new OverflowException("Numeric to decimal conversion overflow");
                    }

                }
                return new Decimal(decimalRaw);
            }
        }

        private Date GetDateUnchecked(int columnIndex)
        {
            return new Date(GetUintUnchecked(columnIndex));
        }

        private TimeSpan GetTimeUnchecked(int columnIndex)
        {
            return Conversions.TimeValueToTimeSpan(GetUlongUnchecked(columnIndex));
        }

        private Timestamp GetTimestampUnchecked(int columnIndex)
        {
            return new Timestamp(GetUlongUnchecked(columnIndex), DateTimeKind.Unspecified);
        }

        private Timestamp GetTimestampTzUnchecked(int columnIndex)
        {
            return new Timestamp(GetUlongUnchecked(columnIndex), DateTimeKind.Utc);
        }

        private string GetStringUnchecked(int columnIndex)
        {
            Dll.hyper_value_t v = GetRawValue(columnIndex);
            return NativeStringConverter.PtrToStringUtf8(v.value, v.size);
        }

        private byte[] GetBytesUnchecked(int columnIndex)
        {
            Dll.hyper_value_t v = GetRawValue(columnIndex);
            if (v.size > int.MaxValue)
                throw new HyperException("field is too big", new HyperException.ContextId(0x5bcd4937));
            byte[] b = new byte[v.size];
            Marshal.Copy(v.value, b, 0, (int)v.size);
            return b;
        }

        private long GetBytesUnchecked(int columnIndex, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            CheckBeforeGet(columnIndex);

            Util.CheckArgument(dataOffset >= 0);
            Util.CheckArgument(bufferOffset >= 0);
            Util.CheckArgument(length >= 0);

            Dll.hyper_value_t v = GetRawValue(columnIndex);
            if ((ulong)dataOffset >= v.size)
                return 0;
            if (v.size - (ulong)dataOffset < (ulong)length)
                length = (int)(v.size - (ulong)dataOffset);
            Marshal.Copy(v.value + (int)dataOffset, buffer, (int)dataOffset, length);
            return length;
        }

        /// <summary>
        /// Gets the value from the specified column.
        /// </summary>
        /// <param name="columnIndex">column index.</param>
        /// <returns>The value. NULL database values are returned as <c>null</c>.
        /// See <see cref="TypeTag"/> documentation for how Hyper data types map to C# types.
        /// </returns>
        public object GetValue(int columnIndex)
        {
            if (IsNull(columnIndex))
                return null;

            // IsNull invokes CheckBeforeGet()

            switch (columns[columnIndex].Type.Tag)
            {
                case TypeTag.BigInt:
                    return GetLongUnchecked(columnIndex);

                case TypeTag.Bool:
                    return GetBoolUnchecked(columnIndex);

                case TypeTag.Bytes:
                case TypeTag.Geography:
                case TypeTag.Unsupported:
                    return GetBytesUnchecked(columnIndex);

                case TypeTag.Date:
                    return GetDateUnchecked(columnIndex);

                case TypeTag.Double:
                    return GetDoubleUnchecked(columnIndex);

                case TypeTag.Int:
                    return GetIntUnchecked(columnIndex);

                case TypeTag.Oid:
                    return GetUintUnchecked(columnIndex);

                case TypeTag.Interval:
                    return GetIntervalUnchecked(columnIndex);

                case TypeTag.SmallInt:
                    return GetShortUnchecked(columnIndex);

                case TypeTag.Char:
                case TypeTag.Text:
                case TypeTag.Varchar:
                case TypeTag.Json:
                    return GetStringUnchecked(columnIndex);

                case TypeTag.Time:
                    return GetTimeUnchecked(columnIndex);

                case TypeTag.Timestamp:
                    return GetTimestampUnchecked(columnIndex);

                case TypeTag.TimestampTZ:
                    return GetTimestampTzUnchecked(columnIndex);

                case TypeTag.Numeric:
                    return GetNumericUnchecked(columnIndex);

                default:
                    throw new HyperException($"unsupported column type {columns[columnIndex].Type.Tag}", new HyperException.ContextId(0xf39fe2ae));
            }
        }

        /// <summary>
        /// Gets current row values as an array of objects. NULL database values are returned as null.
        /// See <see cref="TypeTag"/> documentation for how Hyper data types map to C# types.
        /// </summary>
        /// <param name="values">Array to write values to.</param>
        /// <returns>Number of values written.</returns>
        public int GetValues(object[] values)
        {
            for (int i = 0; i < columnCount; ++i)
                values[i] = GetValue(i);
            return columnCount;
        }

        /// <summary>
        /// Gets current row values as an array of objects. NULL database values are returned as null.
        /// See <see cref="TypeTag"/> documentation for how Hyper data types map to C# types.
        /// </summary>
        public object[] GetValues()
        {
            object[] values = new object[columnCount];
            GetValues(values);
            return values;
        }

        private void CheckNotDisposed()
        {
            if (nativeHandle == null)
                throw new ObjectDisposedException("result");
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~Result()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing && nativeHandle != null)
                Util.WarnNotDisposedUserObject("Result object has not been closed. Close it with Close() or use it in a 'using' statement.");

            if (disposing)
            {
                if (nativeHandle != null)
                {
                    nativeHandle.Close();
                    nativeHandle = null;
                }
            }

            currentChunk.Destroy();
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
