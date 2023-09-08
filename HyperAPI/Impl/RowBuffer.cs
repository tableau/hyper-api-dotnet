using System;
using System.Numerics;
using System.Text;
using System.Runtime.InteropServices;

using Tableau.HyperAPI.Raw;

namespace Tableau.HyperAPI.Impl
{
    /// <summary>
    /// Chunk of row data, collected by Inserter.Add* methods and sent to hyper in Inserter.Flush().
    /// It contains the hyper binary header and the row data.
    /// </summary>
    internal class RowBuffer : IDisposable
    {
        private const double GROW_FACTOR = 1.2;

        /// <summary>
        /// Start of the allocated unmanaged block of memory.
        /// </summary>
        public IntPtr Start { get; private set; }

        /// <summary>
        /// Size of the allocated block of memory.
        /// </summary>
        private ulong Allocated;

        /// <summary>
        /// Number of bytes currently written into the buffer.
        /// </summary>
        public ulong Size { get; private set; }

        private byte[] scratchPad;
        private Encoding Utf8Encoding = Encoding.UTF8;

        public RowBuffer(int initialSize)
        {
            Start = Marshal.AllocHGlobal(initialSize);

            try
            {
                Allocated = (ulong)initialSize;
                Size = 0;
                CopyHeader();
            }
            catch
            {
                Marshal.FreeHGlobal(Start);
                throw;
            }
        }

        /// <summary>
        /// Pointer past the written bytes. New data comes in at this address.
        /// </summary>
        private IntPtr Cursor => Start + (int)Size;

        /// <summary>
        /// How many free bytes are there?
        /// </summary>
        private ulong Available => Allocated - Size;

        /// <summary>
        /// Resize the buffer if necessary to make at least spaceNeeded bytes available.
        /// </summary>
        private void Resize(ulong spaceNeeded)
        {
            ulong available = Allocated - Size;
            if (spaceNeeded <= available)
                return;

            ulong delta = spaceNeeded - available;
            ulong newSize = Math.Max(Allocated + delta, (ulong)(Allocated * GROW_FACTOR));
            if (newSize >= int.MaxValue)
            {
                throw new Exception("row is too big");
            }
            Start = Marshal.ReAllocHGlobal(Start, (IntPtr)newSize);
            Allocated = newSize;
        }

        private byte[] GetScratchPad(int sizeNeeded)
        {
            int newSize;

            if (scratchPad == null)
            {
                newSize = Math.Max(sizeNeeded, 1024);
            }
            else
            {
                int oldSize = scratchPad.Length;

                if (sizeNeeded <= oldSize)
                    return scratchPad;

                newSize = Math.Max(sizeNeeded, (int)Math.Min(int.MaxValue, (long)(oldSize * 1.2)));
            }

            scratchPad = new byte[newSize];
            return scratchPad;
        }

        private void CopyHeader()
        {
            Util.Verify(Size == 0);
            Size = Dll.hyper_write_header(Start, Allocated);
            Util.Verify(Size < Allocated);
        }

        /// <summary>
        /// Reset the buffer, so it contains only the header and is ready to receive new chunk of data.
        /// </summary>
        public void Reset()
        {
            Size = 0;
            CopyHeader();
        }

        public void WriteNull()
        {
            if (Available < 1)
                Resize(1);

            unsafe
            {
                *(byte*)Cursor = 1;
                Size += 1;
            }
        }

        private void WriteByteNotNullable(byte v)
        {
            if (Available < 1)
                Resize(1);

            unsafe
            {
                *(byte*)Cursor = v;
                Size += 1;
            }
        }

        private void WriteByteNullable(byte v)
        {
            if (Available < 2)
                Resize(2);

            unsafe
            {
                byte* p = (byte*)Cursor;
                *p = 0;
                *(p + 1) = v;
                Size += 2;
            }
        }

        public void WriteShort(short v, bool isNullable)
        {
            if (isNullable)
                WriteShortNullable(v);
            else
                WriteShortNotNullable(v);
        }

        private void WriteShortNotNullable(short v)
        {
            if (Available < 2)
                Resize(2);

            unsafe
            {
                *(short*)Cursor = v;
                Size += 2;
            }
        }

        private void WriteShortNullable(short v)
        {
            if (Available < 3)
                Resize(3);

            unsafe
            {
                byte* p = (byte*)Cursor;
                *p = 0;
                *(short*)(p + 1) = v;
                Size += 3;
            }
        }

        public void WriteInt(int v, bool isNullable)
        {
            if (isNullable)
                WriteIntNullable(v);
            else
                WriteIntNotNullable(v);
        }

        private void WriteIntNotNullable(int v)
        {
            if (Available < 4)
                Resize(4);

            unsafe
            {
                *(int*)Cursor = v;
                Size += 4;
            }
        }

        private void WriteIntNullable(int v)
        {
            if (Available < 5)
                Resize(5);

            unsafe
            {
                byte* p = (byte*)Cursor;
                *p = 0;
                *(int*)(p + 1) = v;
                Size += 5;
            }
        }

        public void WriteLong(long v, bool isNullable)
        {
            if (isNullable)
                WriteLongNullable(v);
            else
                WriteLongNotNullable(v);
        }

        private void WriteLongNotNullable(long v)
        {
            if (Available < 8)
                Resize(8);

            unsafe
            {
                *(long*)Cursor = v;
                Size += 8;
            }
        }

        private void WriteLongNullable(long v)
        {
            if (Available < 9)
                Resize(9);

            unsafe
            {
                byte* p = (byte*)Cursor;
                *p = 0;
                *(long*)(p + 1) = v;
                Size += 9;
            }
        }

        public void WriteUint(uint v, bool isNullable)
        {
            WriteInt((int)v, isNullable);
        }

        public void WriteUlong(ulong v, bool isNullable)
        {
            WriteLong((long)v, isNullable);
        }

        public void WriteDouble(double v, bool isNullable)
        {
            WriteLong(BitConverter.DoubleToInt64Bits(v), isNullable);
        }

        public void WriteBool(bool v, bool isNullable)
        {
            if (isNullable)
                WriteByteNullable(v ? (byte)1 : (byte)0);
            else
                WriteByteNotNullable(v ? (byte)1 : (byte)0);
        }

        private byte[] GetUtf8Bytes(string v, int start, int count, out int size)
        {
            if (count == 0)
            {
                size = 0;
                return GetScratchPad(0);
            }

            if (int.MaxValue / 4 < count)
                throw new HyperException("string is too big", new HyperException.ContextId(0xd4c13d70));

            byte[] buf = GetScratchPad(count * 4);
            size = Utf8Encoding.GetBytes(v, start, count, buf, 0);
            return buf;
        }

        public void WriteString(string v, int start, int count, bool isNullable)
        {
            Util.CheckArgument(v != null && start >= 0 && count >= 0 && count <= v.Length && start <= v.Length - count);

            if (isNullable)
                WriteStringNullable(v, start, count);
            else
                WriteStringNotNullable(v, start, count);
        }

        private void WriteStringNotNullable(string v, int start, int count)
        {
            int size;
            byte[] bytes = GetUtf8Bytes(v, start, count, out size);
            WriteBytesNotNullable(bytes, 0, size);
        }

        private void WriteStringNullable(string v, int start, int count)
        {
            int size;
            byte[] bytes = GetUtf8Bytes(v, start, count, out size);
            WriteBytesNullable(bytes, 0, size);
        }

        public void WriteBytes(byte[] v, int start, int count, bool isNullable)
        {
            Util.CheckArgument(v != null  && start >= 0 && count >= 0 && count <= v.Length && start <= v.Length - count);

            if (isNullable)
                WriteBytesNullable(v, start, count);
            else
                WriteBytesNotNullable(v, start, count);
        }

        private void WriteBytesNotNullable(byte[] v, int start, int count)
        {
            ulong spaceNeeded = (ulong)count + 4;
            if (Available < spaceNeeded)
                Resize(spaceNeeded);

            unsafe
            {
                byte* p = (byte*)Cursor;
                *(int*)p = count;

                if (count > 0)
                    Marshal.Copy(v, start, (IntPtr)(p + 4), count);

                Size += spaceNeeded;
            }
        }

        private void WriteBytesNullable(byte[] v, int start, int count)
        {
            ulong spaceNeeded = (ulong)count + 5;
            if (Available < spaceNeeded)
                Resize(spaceNeeded);

            unsafe
            {
                byte* p = (byte*)Cursor;
                *p = 0;
                *(int*)(p + 1) = count;

                if (count > 0)
                    Marshal.Copy(v, start, (IntPtr)(p + 5), count);

                Size += spaceNeeded;
            }
        }

        public void WriteDate(Date date, bool isNullable)
        {
            WriteUint(date.Value, isNullable);
        }

        public void WriteTime(TimeSpan time, bool isNullable)
        {
            WriteUlong(Conversions.TimeSpanToTimeValue(time), isNullable);
        }

        public void WriteTimestamp(Timestamp timestamp, bool isNullable)
        {
            WriteUlong(timestamp.Value, isNullable);
        }

        public void WriteInterval(Interval interval, bool isNullable)
        {
            if (isNullable)
                WriteIntervalNullable(interval);
            else
                WriteIntervalNotNullable(interval);
        }

        private void WriteIntervalNotNullable(Interval interval)
        {
            if (Available < 16)
                Resize(16);

            unsafe
            {
                byte* p = (byte*)Cursor;
                *(Interval*)p = interval;
                Size += 16;
            }
        }

        private void WriteIntervalNullable(Interval interval)
        {
            if (Available < 17)
                Resize(17);

            unsafe
            {
                byte* p = (byte*)Cursor;
                *p = 0;
                *(Interval*)(p + 1) = interval;
                Size += 17;
            }
        }

        internal static BigInteger[] PowersOfTen = new BigInteger[]
        {
            1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000, 1000000000,
            10000000000, 100000000000, 1000000000000, 10000000000000, 100000000000000,
            1000000000000000, 10000000000000000, 100000000000000000, 1000000000000000000,
            10000000000000000000,BigInteger.Parse("100000000000000000000"),BigInteger.Parse("1000000000000000000000"),
            BigInteger.Parse("10000000000000000000000"),BigInteger.Parse("100000000000000000000000"),BigInteger.Parse("1000000000000000000000000"),
            BigInteger.Parse("10000000000000000000000000"),BigInteger.Parse("100000000000000000000000000"),BigInteger.Parse("1000000000000000000000000000"),
            BigInteger.Parse("10000000000000000000000000000"), BigInteger.Parse("100000000000000000000000000000"), BigInteger.Parse("1000000000000000000000000000000"),
            BigInteger.Parse("10000000000000000000000000000000"), BigInteger.Parse("100000000000000000000000000000000"), BigInteger.Parse("1000000000000000000000000000000000"),
            BigInteger.Parse("10000000000000000000000000000000000"), BigInteger.Parse("100000000000000000000000000000000000"), BigInteger.Parse("1000000000000000000000000000000000000"),
            BigInteger.Parse("10000000000000000000000000000000000000"), BigInteger.Parse("100000000000000000000000000000000000000")
        };

        public void WriteNumeric(decimal value, SqlType type, bool isNullable)
        {
            if (type.Precision <= 18)
            {
                // 64-bit numeric
                WriteLong((long)(value * Result.PowersOfTen[type.Scale]), isNullable);
            }
            else
            {
                // 128-bit numeric
                ulong sizeReq = 16u + (isNullable ? 1u : 0u);
                if (Available < sizeReq)
                    Resize(sizeReq);

                unsafe
                {
                    byte* p = (byte*)Cursor;
                    if (isNullable)
                    {
                        *p = 0;
                        p += 1;
                    }

                    // Convert the C# decimal to a Hyper numeric
                    int[] rawDecimal = decimal.GetBits(value);
                    int decimalInternalScale = (rawDecimal[3] & (byte.MaxValue << 16)) >> 16;
                    BigInteger bigInt;
                    // The decimal point of the given decimal value has to be shifted right by the Scale of the Numeric type in Hyper.
                    // Special care is required to avoid overflows Since C# decimals only have 96-bit.
                    if (decimalInternalScale >= type.Scale)
                    {
                        // If the internal scale of the C# decimal is large enough, it cannot overflow while shifting the decimal point.
                        // Converting to BigInteger may truncate.
                        bigInt = new BigInteger(decimal.Multiply(value, Result.PowersOfTen[type.Scale]));
                    }
                    else
                    {
                        // Special care is required to guarantee that no overflows occur. Shifting the decimal point of the decimal by it's
                        // internal scale is always safe. The remaining shift is then done using BigInteger, which has sufficient precision.
                        bigInt = new BigInteger(decimal.Multiply(value, Result.PowersOfTen[decimalInternalScale]));
                        bigInt = BigInteger.Multiply(bigInt,PowersOfTen[type.Scale - decimalInternalScale]);
                    }
                    // Hyper expects little endian:
                    // First write the raw BigInt (which already is little-endian)
                    byte[] rawBigInt = bigInt.ToByteArray();
                    foreach (byte b in rawBigInt)
                    {
                        *p = b;
                        p += 1;
                    }
                    // Fill up to 16 byte
                    byte fillerByte = (bigInt.Sign == -1) ? (byte)0xFF : (byte)0x00;
                    for (int i = rawBigInt.Length; i < 16; ++i)
                    {
                        *p = fillerByte;
                        p += 1;
                    }
                    Size += sizeReq;
                }

            }
        }

        ~RowBuffer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing && Start != IntPtr.Zero)
                Util.WarnNotDisposedInternalObject("RowBuffer");

            if (Start != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(Start);
                Start = IntPtr.Zero;
            }
        }
    }
}
