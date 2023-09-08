using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using Tableau.HyperAPI;

namespace Benchmark
{
    internal abstract class Column
    {
        public string Name { get; }
        public abstract long RowSize { get; }
        public abstract void GenerateRandomData(int rowCount, bool willRead);
        public abstract void AddColumnDefinitions(TableDefinition schema, string baseName);
        public readonly int TableColumnCount;
        public abstract void InsertValue(Inserter inserter, int row);
        public abstract void ReadRow(Result result, int tableColumn, int row);
        public abstract void ValidateData(int rowCount);

        protected Column(string name, int tableColumnCount = 1)
        {
            Name = name;
            TableColumnCount = tableColumnCount;
        }
    }

    internal class BooleanColumn : Column
    {
        private bool[] data;
        private bool[] readData;

        public BooleanColumn()
            : base("Bool")
        {
        }

        public override long RowSize => 1;

        public override void AddColumnDefinitions(TableDefinition schema, string baseName)
        {
            schema.AddColumn(baseName, SqlType.Bool());
        }

        public override void GenerateRandomData(int rowCount, bool willRead)
        {
            data = new bool[rowCount];
            if (willRead)
                readData = new bool[rowCount];
            Random rnd = new Random();
            for (long i = 0; i < rowCount; ++i)
                data[i] = rnd.Next(2) == 1;
        }

        public override void InsertValue(Inserter inserter, int row)
        {
            inserter.Add(data[row]);
        }

        public override void ReadRow(Result result, int tableColumn, int row)
        {
            readData[row] = result.GetBool(tableColumn);
        }

        public override void ValidateData(int rowCount)
        {
            // Count true values
            int wrote = data.Count(v => v);
            int read = readData.Count(v => v);
            Assert.AreEqual(wrote, read);
        }
    }

    internal class ShortColumn : Column
    {
        private short[] data;
        private short[] readData;

        public ShortColumn()
            : base("SmallInt")
        {
        }

        public override long RowSize => 2;

        public override void AddColumnDefinitions(TableDefinition schema, string baseName)
        {
            schema.AddColumn(baseName, SqlType.SmallInt());
        }

        public override void GenerateRandomData(int rowCount, bool willRead)
        {
            data = new short[rowCount];
            if (willRead)
                readData = new short[rowCount];
            Random rnd = new Random();
            for (long i = 0; i < rowCount; ++i)
                data[i] = (short)rnd.Next(short.MinValue, (int)short.MaxValue + 1);
        }

        public override void InsertValue(Inserter inserter, int row)
        {
            inserter.Add(data[row]);
        }

        public override void ReadRow(Result result, int tableColumn, int row)
        {
            readData[row] = result.GetShort(tableColumn);
        }

        public override void ValidateData(int rowCount)
        {
            int sumWritten = data.Aggregate((sum, next) => (short)(sum + next));
            int sumRead = readData.Aggregate((sum, next) => (short)(sum + next));
            Assert.AreEqual(sumWritten, sumRead);
        }
    }

    internal class IntColumn : Column
    {
        private int[,] data;
        private int[,] readData;

        public IntColumn(int columnCount)
            : base(columnCount == 1 ? "Int" : $"Int({columnCount})", columnCount)
        {
        }

        public override long RowSize => 4 * TableColumnCount;

        public override void AddColumnDefinitions(TableDefinition schema, string baseName)
        {
            for (int i = 0; i < TableColumnCount; ++i)
                schema.AddColumn($"{baseName}_{i}", SqlType.Int());
        }

        public override void GenerateRandomData(int rowCount, bool willRead)
        {
            data = new int[TableColumnCount, rowCount];
            if (willRead)
                readData = new int[TableColumnCount, rowCount];
            Random rnd = new Random();
            for (long i = 0; i < TableColumnCount; ++i)
                for (int j = 0; j < rowCount; ++j)
                    data[i, j] = rnd.Next();
        }

        public override void InsertValue(Inserter inserter, int row)
        {
            for (int i = 0; i < TableColumnCount; ++i)
                inserter.Add(data[i, row]);
        }

        public override void ReadRow(Result result, int tableColumn, int row)
        {
            for (int i = 0; i < TableColumnCount; ++i)
                readData[i, row] = result.GetInt(tableColumn + i);
        }

        public override void ValidateData(int rowCount)
        {
            int wrote = 0;
            int read = 0;
            for (int i = 0; i < TableColumnCount; ++i)
            {
                for (int j = 0; j < rowCount; ++j)
                {
                    wrote = wrote + data[i, j];
                    read = read + readData[i, j];
                }
            }

            Assert.AreEqual(wrote, read);
        }
    }

    internal class LongColumn : Column
    {
        private long[] data;
        private long[] readData;

        public LongColumn()
            : base("BigInt")
        {
        }

        public override long RowSize => 8;

        public override void AddColumnDefinitions(TableDefinition schema, string baseName)
        {
            schema.AddColumn(baseName, SqlType.BigInt());
        }

        public override void GenerateRandomData(int rowCount, bool willRead)
        {
            data = new long[rowCount];
            if (willRead)
                readData = new long[rowCount];
            Random rnd = new Random();
            for (long i = 0; i < rowCount; ++i)
            {
                byte[] buf = new byte[8];
                rnd.NextBytes(buf);
                data[i] = BitConverter.ToInt64(buf, 0);
            }
        }

        public override void InsertValue(Inserter inserter, int row)
        {
            inserter.Add(data[row]);
        }

        public override void ReadRow(Result result, int tableColumn, int row)
        {
            readData[row] = result.GetLong(tableColumn);
        }

        public override void ValidateData(int rowCount)
        {
            long sumWritten = data.Aggregate((sum, next) => sum + next);
            long sumRead = readData.Aggregate((sum, next) => sum + next);
            Assert.AreEqual(sumWritten, sumRead);
        }
    }

    internal class DoubleColumn : Column
    {
        private double[] data;
        private double[] readData;

        public DoubleColumn()
            : base("Double")
        {
        }

        public override long RowSize => 8;

        public override void AddColumnDefinitions(TableDefinition schema, string baseName)
        {
            schema.AddColumn(baseName, SqlType.Double());
        }

        public override void GenerateRandomData(int rowCount, bool willRead)
        {
            data = new double[rowCount];
            if (willRead)
                readData = new double[rowCount];
            Random rnd = new Random();
            for (long i = 0; i < rowCount; ++i)
            {
                data[i] = rnd.NextDouble();
            }
        }

        public override void InsertValue(Inserter inserter, int row)
        {
            inserter.Add(data[row]);
        }

        public override void ReadRow(Result result, int tableColumn, int row)
        {
            readData[row] = result.GetDouble(tableColumn);
        }

        public override void ValidateData(int rowCount)
        {
            double sumWritten = data.Aggregate((sum, next) => sum + next);
            double sumRead = readData.Aggregate((sum, next) => sum + next);
            Assert.AreEqual(sumWritten, sumRead, rowCount * 0.01);
        }
    }

    internal class StringColumn : Column
    {
        private string[] data;
        private string[] readData;
        private int minLen;
        private int maxLen;

        public StringColumn(int minLen, int maxLen)
            : base($"Text({minLen}:{maxLen})")
        {
            this.minLen = minLen;
            this.maxLen = maxLen;
        }

        public override long RowSize => (minLen + maxLen) / 2;

        public override void AddColumnDefinitions(TableDefinition schema, string baseName)
        {
            schema.AddColumn(baseName, SqlType.Text());
        }

        public override void GenerateRandomData(int rowCount, bool willRead)
        {
            data = new string[rowCount];
            if (willRead)
                readData = new string[rowCount];
            Random rnd = new Random();
            string alphanum = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            for (int i = 0; i < rowCount; ++i)
            {
                int len = minLen + rnd.Next(maxLen - minLen + 1);
                char[] value = new char[len];
                for (int j = 0; j < len; ++j)
                    value[j] = alphanum[rnd.Next(alphanum.Length)];
                data[i] = new string(value);
            }
        }

        public override void InsertValue(Inserter inserter, int row)
        {
            inserter.Add(data[row]);
        }

        public override void ReadRow(Result result, int tableColumn, int row)
        {
            readData[row] = result.GetString(tableColumn);
        }

        public override void ValidateData(int rowCount)
        {
            int lenSumRead = 0;
            int hashSumRead = 0;
            int lenSumWritten = 0;
            int hashSumWritten = 0;
            for (int i = 0; i < rowCount; ++i)
            {
                lenSumRead += readData[i].Length;
                hashSumRead += readData[i].GetHashCode();
                lenSumWritten += data[i].Length;
                hashSumWritten += data[i].GetHashCode();
            }
            Assert.AreEqual(lenSumWritten, lenSumRead);
            Assert.AreEqual(hashSumWritten, hashSumRead);
        }
    }

    internal class DateColumn : Column
    {
        private Date[] data;
        private Date[] readData;

        public DateColumn()
            : base("Date")
        {
        }

        public override long RowSize => 4;

        public override void AddColumnDefinitions(TableDefinition schema, string baseName)
        {
            schema.AddColumn(baseName, SqlType.Date());
        }

        public static Date RandomDate(Random rnd)
        {
            int year = rnd.Next(-4712, 100000);
            int month = rnd.Next(1, 13);
            int day = rnd.Next(1, 29);
            return new Date(year, month, day);
        }

        public override void GenerateRandomData(int rowCount, bool willRead)
        {
            data = new Date[rowCount];
            if (willRead)
                readData = new Date[rowCount];
            Random rnd = new Random();
            for (int i = 0; i < rowCount; ++i)
                data[i] = RandomDate(rnd);
        }

        public override void InsertValue(Inserter inserter, int row)
        {
            inserter.Add(data[row]);
        }

        public override void ReadRow(Result result, int tableColumn, int row)
        {
            readData[row] = result.GetDate(tableColumn);
        }

        public override void ValidateData(int rowCount)
        {
            long sumW = 0;
            long sumR = 0;
            for (int i = 0; i < rowCount; ++i)
            {
                sumW = sumW + data[i].Year + data[i].Month + data[i].Day;
                sumR = sumR + readData[i].Year + readData[i].Month + readData[i].Day;
            }
            Assert.AreEqual(sumW, sumR);
        }
    }

    internal class TimeColumn : Column
    {
        private TimeSpan[] data;
        private TimeSpan[] readData;

        public TimeColumn()
            : base("Time")
        {
        }

        public override long RowSize => 8;

        public override void AddColumnDefinitions(TableDefinition schema, string baseName)
        {
            schema.AddColumn(baseName, SqlType.Time());
        }

        public static TimeSpan RandomTime(Random rnd)
        {
            int hour = rnd.Next(24);
            int minute = rnd.Next(60);
            int second = rnd.Next(60);
            int microsecond = rnd.Next(1_000_000);
            return new TimeSpan(hour * TimeSpan.TicksPerHour + minute * TimeSpan.TicksPerMinute + second * TimeSpan.TicksPerSecond +
                microsecond * 10);
        }

        public override void GenerateRandomData(int rowCount, bool willRead)
        {
            data = new TimeSpan[rowCount];
            if (willRead)
                readData = new TimeSpan[rowCount];
            Random rnd = new Random();
            for (int i = 0; i < rowCount; ++i)
                data[i] = RandomTime(rnd);
        }

        public override void InsertValue(Inserter inserter, int row)
        {
            inserter.Add(data[row]);
        }

        public override void ReadRow(Result result, int tableColumn, int row)
        {
            readData[row] = result.GetTime(tableColumn);
        }

        public override void ValidateData(int rowCount)
        {
            long sumW = 0;
            long sumR = 0;
            for (int i = 0; i < rowCount; ++i)
            {
                sumW = sumW + data[i].Ticks;
                sumR = sumR + readData[i].Ticks;
            }
            Assert.AreEqual(sumW, sumR);
        }
    }

    internal class TimestampColumn : Column
    {
        private Timestamp[] data;
        private Timestamp[] readData;

        public TimestampColumn()
            : base("Timestamp")
        {
        }

        public override long RowSize => 8;

        public override void AddColumnDefinitions(TableDefinition schema, string baseName)
        {
            schema.AddColumn(baseName, SqlType.Timestamp());
        }

        public override void GenerateRandomData(int rowCount, bool willRead)
        {
            data = new Timestamp[rowCount];
            if (willRead)
                readData = new Timestamp[rowCount];
            Random rnd = new Random();
            for (int i = 0; i < rowCount; ++i)
            {
                Timestamp timestamp = new Timestamp(
                    rnd.Next(-4712, 100000),
                    rnd.Next(1, 13), rnd.Next(1, 29), rnd.Next(0, 24),
                    rnd.Next(0, 60), rnd.Next(0, 60), rnd.Next(0, 1_000_000));
                data[i] = timestamp;
            }
        }

        public override void InsertValue(Inserter inserter, int row)
        {
            inserter.Add(data[row]);
        }

        public override void ReadRow(Result result, int tableColumn, int row)
        {
            readData[row] = result.GetTimestamp(tableColumn);
        }

        public override void ValidateData(int rowCount)
        {
            long sumW = 0;
            long sumR = 0;
            for (int i = 0; i < rowCount; ++i)
            {
                sumW = sumW + data[i].Year + data[i].Month + data[i].Day + data[i].Hour + data[i].Minute + data[i].Second + data[i].Microsecond;
                sumR = sumR + readData[i].Year + readData[i].Month + readData[i].Day + readData[i].Hour + readData[i].Minute + readData[i].Second + readData[i].Microsecond;
            }
            Assert.AreEqual(sumW, sumR);
        }
    }

    internal class IntervalColumn : Column
    {
        private Interval[] data;
        private Interval[] readData;

        public IntervalColumn()
            : base("Interval")
        {
        }

        public override long RowSize => 16;

        public override void AddColumnDefinitions(TableDefinition schema, string baseName)
        {
            schema.AddColumn(baseName, SqlType.Interval());
        }

        public override void GenerateRandomData(int rowCount, bool willRead)
        {
            data = new Interval[rowCount];
            if (willRead)
                readData = new Interval[rowCount];
            Random rnd = new Random();
            for (int i = 0; i < rowCount; ++i)
                data[i] = new Interval(rnd.Next(0, 200) * 12 + rnd.Next(0, 200), rnd.Next(0, 30), rnd.Next(0, 1_000_000));
        }

        public override void InsertValue(Inserter inserter, int row)
        {
            inserter.Add(data[row]);
        }

        public override void ReadRow(Result result, int tableColumn, int row)
        {
            readData[row] = result.GetInterval(tableColumn);
        }

        public override void ValidateData(int rowCount)
        {
            long sumW = 0;
            long sumR = 0;
            for (int i = 0; i < rowCount; ++i)
            {
                sumW = sumW + data[i].Months;
                sumW = sumW + data[i].Days;
                sumW = sumW + data[i].Microseconds;
                sumR = sumR + readData[i].Months;
                sumR = sumR + readData[i].Days;
                sumR = sumR + readData[i].Microseconds;
            }
            Assert.AreEqual(sumW, sumR);
        }
    }
}
