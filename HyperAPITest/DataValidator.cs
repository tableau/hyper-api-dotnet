using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace Tableau.HyperAPI.Test
{
    public class DataValidator
    {
        private class RowComparer : IComparer<object[]>
        {
            private Schema schema;

            public RowComparer(Schema schema)
            {
                this.schema = schema;
            }

            private int CompareBytes(byte[] x, byte[] y)
            {
                for (int i = 0; i < Math.Min(x.Length, y.Length); ++i)
                {
                    int cmp = x[i].CompareTo(y[i]);
                    if (cmp != 0)
                        return cmp;
                }

                return x.Length.CompareTo(y.Length);
            }

            private int CompareValues(object x, object y, ISchemaColumn col)
            {
                if (x == null)
                {
                    return y == null ? 0 : -1;
                }
                else if (y == null)
                {
                    return 1;
                }

                Assert.AreEqual(x.GetType(), y.GetType());

                switch (col.Type.Tag)
                {
                    case TypeTag.Bytes:
                    case TypeTag.Geography:
                    case TypeTag.Unsupported:
                        return CompareBytes((byte[])x, (byte[])y);

                    default:
                        return ((IComparable)x).CompareTo(y);
                }
            }

            public int Compare(object[] x, object[] y)
            {
                for (int i = 0; i < x.Length; ++i)
                {
                    int cmp = CompareValues(x[i], y[i], schema.Columns[i]);
                    if (cmp != 0)
                        return cmp;
                }

                return 0;
            }
        }

        private static void AssertEqualValues(object expected, object actual, int rowIdx, int colIdx)
        {
            if (expected == null)
            {
                if (actual == null)
                    return;

                Assert.Fail("row {0}, column {1}: expected null, got {2}", rowIdx, colIdx, actual);
                return;
            }

            if (actual == null)
            {
                Assert.Fail("row {0}, column {1}: expected {2}, got null", rowIdx, colIdx, expected);
                return;
            }

            if (expected.GetType() != actual.GetType())
            {
                Assert.Fail("row {0}, column {1}: expected value {2} of type {3}, got value {4} of type {5}",
                    rowIdx, colIdx, expected, expected.GetType(), actual, actual.GetType());
                return;
            }

            Assert.AreEqual(expected, actual, "row {0}, column {1}: expected {2}, got value {3}",
                    rowIdx, colIdx, expected, actual);
        }

        private static void AssertEqualRowLists(List<object[]> expected, List<object[]> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count, "Lists have different length. Expected {0}, got {1}", expected.Count, actual.Count);
            for (int rowIdx = 0; rowIdx < expected.Count; ++rowIdx)
            {
                object[] expectedRow = expected[rowIdx];
                object[] actualRow = actual[rowIdx];
                Assert.AreEqual(expectedRow.Length, actualRow.Length);
                for (int colIdx = 0; colIdx < expectedRow.Length; ++colIdx)
                    AssertEqualValues(expectedRow[colIdx], actualRow[colIdx], rowIdx, colIdx);
            }
        }

        public static void CheckData(object[][] expected, List<object[]> actual, Schema schema)
        {
            List<object[]> expectedList = new List<object[]>(expected);
            expectedList.Sort(new RowComparer(schema));
            actual.Sort(new RowComparer(schema));
            Assert.AreEqual(expectedList, actual);
        }
    }
}
