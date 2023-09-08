using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using NUnit.Framework;

namespace Tableau.HyperAPI.Test
{
    internal static class TestUtil
    {
        public static void Verify(bool condition)
        {
            if (!condition)
                throw new Exception("condition failed");
        }

        private static void CheckComparisonInterfaces<T>(T lhs, T rhs, int expected, bool checkReverse)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            Assert.IsTrue(lhs.Equals(lhs));
            Assert.IsTrue(((object)lhs).Equals(lhs));
            Assert.IsTrue(rhs.Equals(rhs));
            Assert.IsTrue(((object)rhs).Equals(rhs));
            Assert.AreEqual(0, lhs.CompareTo(lhs));
            Assert.AreEqual(0, ((IComparable)lhs).CompareTo(lhs));
            Assert.AreEqual(0, rhs.CompareTo(rhs));
            Assert.AreEqual(0, ((IComparable)rhs).CompareTo(rhs));

            bool expectedEqual = expected == 0;
            Assert.AreEqual(expectedEqual, lhs.Equals(rhs));
            Assert.AreEqual(expectedEqual, ((object)lhs).Equals(rhs));
            Assert.AreEqual(expectedEqual, rhs.Equals(lhs));
            Assert.AreEqual(expectedEqual, ((object)rhs).Equals(lhs));
            Assert.AreEqual(Math.Sign(expected), Math.Sign(lhs.CompareTo(rhs)));
            Assert.AreEqual(Math.Sign(expected), Math.Sign(((IComparable)lhs).CompareTo(rhs)));
            Assert.AreEqual(Math.Sign(-expected), Math.Sign(rhs.CompareTo(lhs)));
            Assert.AreEqual(Math.Sign(-expected), Math.Sign(((IComparable)rhs).CompareTo(lhs)));

            if (checkReverse)
                CheckComparisonInterfaces(rhs, lhs, -expected, false);
        }

        public interface IComparerWithOperators<T>
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            bool Eq(T x, T y);
            bool Ne(T x, T y);
            bool Lt(T x, T y);
            bool Le(T x, T y);
            bool Gt(T x, T y);
            bool Ge(T x, T y);
        }

        private static void CheckComparisonOperators<T>(T lhs, T rhs, bool checkReverse, IComparerWithOperators<T> comparer)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            int comp = lhs.CompareTo(rhs);
            Assert.AreEqual(comp == 0, comparer.Eq(lhs, rhs));
            Assert.AreEqual(comp != 0, comparer.Ne(lhs, rhs));
            Assert.AreEqual(comp < 0, comparer.Lt(lhs, rhs));
            Assert.AreEqual(comp <= 0, comparer.Le(lhs, rhs));
            Assert.AreEqual(comp > 0, comparer.Gt(lhs, rhs));
            Assert.AreEqual(comp >= 0, comparer.Ge(lhs, rhs));

            if (checkReverse)
                CheckComparisonOperators(rhs, lhs, false, comparer);
        }

        private static void CheckComparisonWithNull<T>(T obj, IComparerWithOperators<T> comparer)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            T defaultValue = default(T);
            Assert.AreEqual(false, obj.Equals(null));
            Assert.AreEqual(false, obj.Equals(comparer));

            // if it's a nullable type, check comparison with null
            if (ReferenceEquals(defaultValue, null))
            {
                Assert.AreEqual(false, obj.Equals(defaultValue));
                Assert.AreEqual(1, obj.CompareTo(defaultValue));
                Assert.AreEqual(false, comparer.Eq(obj, defaultValue));
                Assert.AreEqual(false, comparer.Eq(defaultValue, obj));
                Assert.AreEqual(true, comparer.Ne(defaultValue, obj));
                Assert.AreEqual(true, comparer.Ne(obj, defaultValue));
                Assert.AreEqual(true, comparer.Le(defaultValue, obj));
                Assert.AreEqual(false, comparer.Le(obj, defaultValue));
                Assert.AreEqual(true, comparer.Lt(defaultValue, obj));
                Assert.AreEqual(false, comparer.Lt(obj, defaultValue));
                Assert.AreEqual(false, comparer.Ge(defaultValue, obj));
                Assert.AreEqual(true, comparer.Ge(obj, defaultValue));
                Assert.AreEqual(false, comparer.Gt(defaultValue, obj));
                Assert.AreEqual(true, comparer.Gt(obj, defaultValue));
            }
        }

        public static void CheckComparison<T>(T lhs, T rhs, IComparerWithOperators<T> comparer)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            CheckComparisonInterfaces(lhs, rhs, -1, true);
            CheckComparisonOperators(lhs, rhs, true, comparer);
            CheckComparisonOperators(lhs, lhs, false, comparer);
            CheckComparisonOperators(rhs, rhs, false, comparer);
            CheckComparisonWithNull(lhs, comparer);
            CheckComparisonWithNull(rhs, comparer);
        }

        public static void AssertEqualSets<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            List<T> expectedList = new List<T>(expected);
            List<T> actualList = new List<T>(actual);
            expectedList.Sort();
            actualList.Sort();
            Assert.AreEqual(expectedList, actualList);
        }

        public static void AssertThrowsWithContextId(Action func, uint id)
        {
            HyperException ex = Assert.Throws<HyperException>(() => func());
            Assert.AreEqual(id, ex.Context.Value);
        }
    }
}
