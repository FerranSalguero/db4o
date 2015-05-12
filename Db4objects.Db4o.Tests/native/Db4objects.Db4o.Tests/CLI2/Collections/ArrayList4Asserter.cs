/* This file is part of the db4o object database http://www.db4o.com

Copyright (C) 2004 - 2011  Versant Corporation http://www.versant.com

db4o is free software; you can redistribute it and/or modify it under
the terms of version 3 of the GNU General Public License as published
by the Free Software Foundation.

db4o is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or
FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
for more details.

You should have received a copy of the GNU General Public License along
with this program.  If not, see http://www.gnu.org/licenses/. */

#if !SILVERLIGHT
using System;
using System.Collections;
using System.Collections.Generic;
using Db4objects.Db4o.Collections;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.CLI2.Collections
{
    public class ArrayList4Asserter
    {
        private const int MULTIPLIER = 3;
        private const int OFFSET = 10000;

        public static void AssertLowerBound<T>(IList<T> list)
        {
            Assert.Expect(
                typeof (ArgumentOutOfRangeException),
                new CodeBlockRunner<int>(
                    delegate { var item = list[-1]; },
                    10));
        }

        public static void AssertUpperBound()
        {
            Assert.Expect(
                typeof (ArgumentOutOfRangeException),
                new CodeBlockRunner<int>(
                    delegate(int len)
                    {
                        var list = CreateArrayList(len);
                        var i = list[list.Count + 1];
                    },
                    10));
        }

        public static void AssertItems()
        {
            var list = CreateArrayListAndAssertValues(10);
            for (var i = 0; i < list.Count; i++)
            {
                Assert.AreEqual(i*3, list[i]);
            }
        }

        public static void AssertAddItem()
        {
            var list = CreateArrayListAndAssertValues(10);
            var index = list.Count;
            for (var i = 0; i < 3; i++)
            {
                list.Add(ValueForIndex(index++));
            }

            AssertArrayListValues(list);
            Assert.AreEqual(index, list.Count);
        }

        public static void AssertIsReadOnly()
        {
            var list = CreateArrayList(10);
            Assert.IsFalse(list.IsReadOnly);
        }

        public static void AssertClear()
        {
            var list = CreateArrayListAndAssertValues(10);
            list.Clear();
            Assert.AreEqual(0, list.Count);
        }

        public static void AssertContains<T>(IList<T> list, params T[] nonExistingItems)
        {
            var nonGenericList = (IList) list;
            ForEach(
                list,
                delegate(int index, T value)
                {
                    Assert.IsTrue(list.Contains(value));
                    Assert.IsTrue(nonGenericList.Contains(value));
                });

            foreach (var item in nonExistingItems)
            {
                Assert.IsFalse(list.Contains(item));
            }
        }

        public static void AssertCopyTo(IList<int> list)
        {
            AssertArrayListValues(list);

            var backup = new int[list.Count];
            list.CopyTo(backup, 0);

            AssertAreEqual(backup, list, 0, 0, backup.Length);

            backup = new int[list.Count + 1];
            backup[0] = 0xCC;
            list.CopyTo(backup, 1);
            Assert.AreEqual(0xCC, backup[0]);
            AssertAreEqual(backup, list, 1, 0, backup.Length - 1);

            backup = new int[list.Count + 2];
            backup[0] = 0xDE;
            backup[1] = 0xAD;
            list.CopyTo(backup, 2);
            Assert.AreEqual(0xDE, backup[0]);
            Assert.AreEqual(0xAD, backup[1]);
            AssertAreEqual(backup, list, 2, 0, list.Count);
        }

        private static void AssertAreEqual<T>(T[] array, IList<T> list, int arrayStartIndex, int listIndex, int count)
        {
            Assert.AreEqual(array.Length - arrayStartIndex, list.Count - listIndex);
            for (var i = arrayStartIndex; i < array.Length; i++)
            {
                Assert.AreEqual(array[i], list[listIndex++]);
            }
        }

        public static void AssertCopyToWithInvalidSize(IList<int> list)
        {
            Assert.Expect(
                typeof (ArgumentException),
                new CodeBlockRunner<int>(
                    delegate
                    {
                        var backup = new int[list.Count - 1];
                        list.CopyTo(backup, 0);
                    },
                    10));
        }

        public static void AssertCopyToWithNullTarget<T>(IList<T> list)
        {
            Assert.Expect(
                typeof (ArgumentNullException),
                new CodeBlockRunner<int>(
                    delegate { list.CopyTo(null, 0); },
                    10)
                );
        }

        public static void AssertCopyToWithMultiDimensionalArray(IList<int> values)
        {
            Assert.Expect(
                typeof (ArgumentException),
                new CodeBlockRunner<int>(
                    delegate(int len)
                    {
                        var list = values as ICollection;

                        var backup = new int[len, len];
                        list.CopyTo(backup, backup.Length + 1);
                    },
                    10
                    ));
        }

        public static void AssertCopyToInvalidIndex<T>(IList<T> list)
        {
            Assert.Expect(
                typeof (ArgumentException),
                new CodeBlockRunner<int>(
                    delegate
                    {
                        var backup = new T[list.Count];
                        list.CopyTo(backup, backup.Length + 1);
                    },
                    10
                    ));
        }

        public static void AssertRemove<T>(IList<T> list, params IndexOfItems<T, bool>[] toBeRemoved)
        {
            var size = list.Count;
            foreach (var tbr in toBeRemoved)
            {
                var ret = list.Remove(tbr.Value);
                Assert.AreEqual(tbr.Expected, ret);

                if (ret)
                {
                    size--;
                }
            }

            Assert.AreEqual(size, list.Count);
        }

        public static void AssertRemoveAt<T>(IList<T> list, params IndexOfItems<int, Type>[] expected)
        {
            var size = list.Count;

            var removeAtCodeBlock = new CodeBlockRunner<int>(
                delegate(int index) { list.RemoveAt(index); });

            foreach (var item in expected)
            {
                if (typeof (Exception).IsAssignableFrom(item.Expected))
                {
                    Assert.Expect(
                        item.Expected,
                        removeAtCodeBlock.WithValue(item.Value));
                }
                else
                {
                    list.RemoveAt(item.Value);
                    size--;
                }
            }

            list.RemoveAt(--size);
            Assert.AreEqual(size, list.Count);
        }

        public static void AssertIndexOf<T, E>(IList<T> list, params IndexOfItems<T, E>[] itemsToFind)
        {
            var size = list.Count;

            foreach (var item in itemsToFind)
            {
                Assert.AreEqual(item.Expected, list.IndexOf(item.Value));
            }

            for (var i = 0; i < list.Count; i++)
            {
                Assert.AreEqual(i, list.IndexOf(list[i]));
            }

            var nonGenericList = (IList) list;
            for (var i = 0; i < list.Count; i++)
            {
                Assert.AreEqual(i, nonGenericList.IndexOf(list[i]));
            }
        }

        public static void AssertInsert<T>(IList<T> list, T[] expected, params IndexOfItems<T, int>[] toBeInserted)
        {
            var size = list.Count;
            foreach (var item in toBeInserted)
            {
                list.Insert(item.Expected, item.Value);
            }

            AssertAreEqual(expected, list, 0, 0, size);
        }

        public static void AssertEnumerable<T>(IList<T> list, IEnumerable<T> expected)
        {
            var count = 0;
            var expectedEnum = expected.GetEnumerator();
            foreach (var item in list)
            {
                Assert.IsTrue(expectedEnum.MoveNext());
                Assert.AreEqual(item, expectedEnum.Current);
                ++count;
            }

            Assert.AreEqual(count, list.Count);
            Assert.IsFalse(expectedEnum.MoveNext());
        }

        private static void InsertInList(IList<int> list, int index)
        {
            list.Insert(index, ValueWithOffsetForIndex(index));
        }

        private static int ValueWithOffsetForIndex(int index)
        {
            return ValueForIndex(index) + OFFSET;
        }

        private static void AssertArrayListValues(IList<int> list)
        {
            AssertArrayListValuesOffset(list, 0);
        }

        private static void AssertArrayListValuesOffset(IList<int> list, int offset)
        {
            for (var i = 0; i < list.Count; i++)
            {
                Assert.AreEqual(ValueForIndex(i + offset), list[i]);
            }
        }

        internal static int ValueForIndex(int index)
        {
            return index*MULTIPLIER;
        }

        internal static IList<int> CreateArrayListAndAssertValues(int size)
        {
            var list = CreateArrayList(size);
            Assert.AreEqual(size, list.Count);
            return list;
        }

        public static IList<int> CreateArrayList(int count)
        {
            IList<int> list = new ArrayList4<int>();
            for (var i = 0; i < count; i++)
            {
                list.Add(ValueForIndex(i));
            }
            return list;
        }

        private static void ForEach<T>(IList<T> list, AssertDelegate<T> method)
        {
            for (var i = 0; i < list.Count; i++)
            {
                method(i, list[i]);
            }
        }

        public static void TestAddDifferentTypes<T>(IList<T> coll, object value)
        {
            var list = coll as IList;
            Assert.Expect(
                typeof (ArgumentException),
                new CodeBlockRunner<int>(delegate { list.Add(value); }));
        }

        public static void AssertToString<T>(IList<T> list, string expected)
        {
            Assert.AreEqual(expected, list.ToString());
        }

        public static void AssertFailEnumerator<T>(IList<T> list, IEnumerable<T> enumerable, int index, T valueToBeAdded)
        {
            var i = 0;
            Assert.Expect(
                typeof (InvalidOperationException),
                new CodeBlockRunner<int>(
                    delegate
                    {
                        foreach (var item in list)
                        {
                            if (i == index)
                            {
                                list.Add(valueToBeAdded);
                            }
                            else
                            {
                                i++;
                            }
                        }
                    }));

            Assert.AreEqual(index, i);
        }

        public static void AssertBinarySearch<T>(ArrayList4<T> list, params IndexOfItems<T, int>[] expectedResults)
        {
            foreach (var result in expectedResults)
            {
                if (result.Expected >= 0)
                {
                    Assert.AreEqual(result.Expected, list.BinarySearch(result.Value));
                }
                else
                {
                    Assert.IsGreater(list.BinarySearch(result.Value), 0);
                }
            }
        }

        private delegate void AssertDelegate<T>(int index, T value);
    }

    public class IndexOfItems<T, E>
    {
        internal E Expected;
        internal T Value;

        public IndexOfItems(T value, E expected)
        {
            Value = value;
            Expected = expected;
        }
    }
}

#endif