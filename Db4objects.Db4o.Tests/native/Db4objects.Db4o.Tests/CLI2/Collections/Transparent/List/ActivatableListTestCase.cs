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

using System;
using System.Collections.Generic;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.CLI2.Collections.Transparent.List
{
    public partial class ActivatableListTestCase :
        AbstractActivatableCollectionApiTestCase<IList<ICollectionElement>, ICollectionElement>
    {
        #region IList<T> members

        public void TestCorrectContent()
        {
            IteratorAssert.AreEqual(NewPopulatedPlainCollection().GetEnumerator(), SingleCollection().GetEnumerator());
        }

        public void TestCollectionIsNotActivated()
        {
            Assert.IsFalse(Db().IsActive(SingleCollection()));
        }

        public void TestIndexOf()
        {
            const int itemIndex = 2;
            var collection = SingleCollection();
            var i = collection.Count;
            Assert.AreEqual(itemIndex, collection.IndexOf(NewElement(itemIndex)));
        }

        public void TestIndexerGetter()
        {
            const int indexToTest = 1;
            Assert.AreEqual(NewElement(indexToTest), SingleCollection()[indexToTest]);
        }

        public void TestCopyTo()
        {
            AssertCopy(delegate(ICollectionElement[] elements) { SingleCollection().CopyTo(elements, 0); });
        }

        public void TestIndexerSetter()
        {
            AssertCollectionChange(delegate(IList<ICollectionElement> list)
            {
                const int indexToTest = 1;
                list[indexToTest] = new Element("one-and-half");
            });
        }

        public void TestInsert()
        {
            AssertCollectionChange(delegate(IList<ICollectionElement> list)
            {
                const int insertionIndex = 2;
                const string newItemName = "two-and-half";

                list.Insert(insertionIndex, new Element(newItemName));
            });
        }

        public void TestRemoveAt()
        {
            AssertCollectionChange(delegate(IList<ICollectionElement> list) { list.RemoveAt(0); });
        }

        public void TestRepeatedAdd()
        {
            ICollectionElement four = new Element("four");
            SingleCollection().Add(four);
            Db().Purge();

            ICollectionElement five = new Element("five");
            SingleCollection().Add(five);
            Reopen();

            var retrieved = SingleCollection();
            Assert.IsTrue(retrieved.Contains(four));
            Assert.IsTrue(retrieved.Contains(five));
        }

        #endregion

        #region List<T> members

        public void TestReadOnly()
        {
            var source = SingleActivatableCollection();
            var readOnly = source.AsReadOnly();

            IteratorAssert.AreEqual(NewPopulatedPlainList().GetEnumerator(), readOnly.GetEnumerator());

            source.Add(new Element("n"));
            Assert.IsGreaterOrEqual(0, readOnly.IndexOf(new Element("n")));
        }

        public void TestAddRange()
        {
            SingleActivatableCollection().AddRange(ToBeAdded());
            Reopen();

            var expected = NewPopulatedPlainList();
            expected.AddRange(ToBeAdded());

            IteratorAssert.AreEqual(expected.GetEnumerator(), SingleCollection().GetEnumerator());
        }

        public void TestBinarySearch()
        {
            SingleActivatableCollection().Sort();
            Reopen();

            foreach (var name in Names)
            {
                Assert.IsGreaterOrEqual(0, SingleActivatableCollection().BinarySearch(new ActivatableElement(name)));
            }
        }

        public void TestBinarySearch1()
        {
            var collection = SingleActivatableCollection();
            collection.Sort();
            var count = collection.Count;
            Reopen();

            foreach (var name in Names)
            {
                Assert.IsGreaterOrEqual(0,
                    SingleActivatableCollection()
                        .BinarySearch(0, count, new ActivatableElement(name), SimpleComparer.Instance));
            }
        }

        public void TestBinarySearch2()
        {
            SingleActivatableCollection().Sort();
            Reopen();

            foreach (var name in Names)
            {
                Assert.IsGreaterOrEqual(0,
                    SingleActivatableCollection().BinarySearch(new ActivatableElement(name), SimpleComparer.Instance));
            }
        }

        public void TestCapacity()
        {
            var list = SingleActivatableCollection();
            Assert.IsGreater(0, list.Capacity);
            Assert.IsTrue(Db().IsActive(list));

            Reopen();
            list = SingleActivatableCollection();
            list.Capacity = 10;
            Assert.IsTrue(Db().IsActive(list));
        }

        public void TestCopyTo2()
        {
            AssertCopy(delegate(ICollectionElement[] elements) { SingleActivatableCollection().CopyTo(elements); });
        }

        public void TestCopyTo3()
        {
            AssertCopy(
                delegate(ICollectionElement[] elements)
                {
                    SingleActivatableCollection().CopyTo(0, elements, 0, elements.Length);
                });
        }

#if !SILVERLIGHT
        public void TestExists()
        {
            Assert.IsTrue(
                SingleActivatableCollection()
                    .Exists(delegate(ICollectionElement candidate) { return candidate.Name == Names[1]; }));
        }

        public void TestFind()
        {
            var found =
                SingleActivatableCollection()
                    .Find(delegate(ICollectionElement candidate) { return candidate.Name == Names[0]; });
            Assert.IsNotNull(found);
        }

        public void TestFindAll()
        {
            Predicate<ICollectionElement> predicate =
                delegate(ICollectionElement candidate) { return candidate.Name == Names[0]; };

            var expected = NewPopulatedPlainList().FindAll(predicate);
            var actual = SingleActivatableCollection().FindAll(predicate);

            IteratorAssert.SameContent(expected, actual);
        }

        public void TestFindIndexSimplePredicate()
        {
            Assert.IsGreaterOrEqual(0,
                SingleActivatableCollection()
                    .FindIndex(delegate(ICollectionElement candidate) { return candidate.Name == Names[1]; }));
        }

        public void TestFindIndexWithStartIndex()
        {
            Assert.IsGreaterOrEqual(0,
                SingleActivatableCollection()
                    .FindIndex(0, delegate(ICollectionElement candidate) { return candidate.Name == Names[1]; }));
        }

        public void FindIndexWithCount()
        {
            Assert.IsGreaterOrEqual(0,
                SingleActivatableCollection()
                    .FindIndex(0, NewPopulatedPlainList().Count,
                        delegate(ICollectionElement candidate) { return candidate.Name == Names[1]; }));
        }

        public void TestFindLast()
        {
            Predicate<ICollectionElement> match =
                delegate(ICollectionElement candidate) { return candidate.Name == Names[2]; };

            var expected = NewPopulatedPlainList().FindLast(match);
            var actual = SingleActivatableCollection().FindLast(match);

            Assert.AreEqual(expected, actual);
        }

        public void TestFindLastIndexSimplePredicate()
        {
            Assert.IsGreaterOrEqual(0,
                SingleActivatableCollection()
                    .FindLastIndex(delegate(ICollectionElement candidate) { return candidate.Name == Names[1]; }));
        }

        public void TestFindLastIndexWithStartIndex()
        {
            Assert.IsGreaterOrEqual(0,
                SingleActivatableCollection()
                    .FindLastIndex(NewPopulatedPlainList().Count - 1,
                        delegate(ICollectionElement candidate) { return candidate.Name == Names[1]; }));
        }

        public void TestFindLastIndexWithCount()
        {
            Assert.IsGreaterOrEqual(0,
                SingleActivatableCollection()
                    .FindLastIndex(NewPopulatedPlainList().Count - 1, NewPopulatedPlainList().Count,
                        delegate(ICollectionElement candidate) { return candidate.Name == Names[1]; }));
        }
#endif

        public void TestForEach()
        {
            var count = 0;
            SingleActivatableCollection().ForEach(delegate(ICollectionElement candidate)
            {
                Assert.IsTrue(candidate.Name.Length > 2);
                count++;
            });

            Assert.IsGreater(0, count);
        }


        public void TestGetRange()
        {
            var startIndex = 1;
            var count = 3;

            var expected = NewPopulatedPlainList().GetRange(startIndex, count);
            var actual = SingleActivatableCollection().GetRange(startIndex, count);

            IteratorAssert.SameContent(expected, actual);
        }

        public void TestIndexOfWithStartIndex()
        {
            var tbf = NewElement(Names.Count - 2);
            const int startIndex = 1;

            var expectedIndex = NewPopulatedPlainList().IndexOf(tbf, startIndex);
            var actualIndex = SingleActivatableCollection().IndexOf(tbf, startIndex);

            Assert.AreEqual(expectedIndex, actualIndex);
        }

        public void TestIndexOfWithStartIndexAndCount()
        {
            var tbf = NewElement(Names.Count - 2);
            const int startIndex = 1;
            const int count = 3;

            var expectedIndex = NewPopulatedPlainList().IndexOf(tbf, startIndex, count);
            var actualIndex = SingleActivatableCollection().IndexOf(tbf, startIndex, count);

            Assert.AreEqual(expectedIndex, actualIndex);
        }

        public void TestInsertRange()
        {
            const int index = 2;

            SingleActivatableCollection().InsertRange(index, ToBeAdded());
            Reopen();

            var expected = NewPopulatedPlainList();
            expected.InsertRange(index, ToBeAdded());

            IteratorAssert.SameContent(expected, SingleActivatableCollection());
        }

        public void TestLastIndexOf()
        {
            var tbf = NewElement(1);

            var collection = NewPopulatedPlainList();
            var expected = collection.LastIndexOf(tbf);
            var actual = SingleActivatableCollection().LastIndexOf(tbf);

            Assert.AreEqual(expected, actual);
        }

        public void TestLastIndexOfWithStartIndex()
        {
            var tbf = NewElement(1);

            var startIndex = LastIndex();
            var expected = NewPopulatedPlainList().LastIndexOf(tbf, startIndex);
            var actual = SingleActivatableCollection().LastIndexOf(tbf, startIndex);

            Assert.AreEqual(expected, actual);
        }

        public void TestLastIndexOfWithStartIndexAndCount()
        {
            var tbf = NewElement(1);

            var startIndex = Names.Count;
            const int count = 5;

            var expected = NewPopulatedPlainList().LastIndexOf(tbf, startIndex, count);
            var actual = SingleActivatableCollection().LastIndexOf(tbf, startIndex, count);

            Assert.AreEqual(expected, actual);
        }

#if !SILVERLIGHT
        public void TestRemoveAll()
        {
            Predicate<ICollectionElement> predicate =
                delegate(ICollectionElement candidate) { return candidate.Name.Length > 3; };

            var actualCount = SingleActivatableCollection().RemoveAll(predicate);
            Reopen();

            var expectedCollection = NewPopulatedPlainList();
            var expectedCount = expectedCollection.RemoveAll(predicate);

            Assert.AreEqual(expectedCount, actualCount);
            IteratorAssert.SameContent(expectedCollection, SingleActivatableCollection());
        }
#endif

        public void TestRemoveRange()
        {
            const int startIndex = 1;
            const int count = 2;

            SingleActivatableCollection().RemoveRange(startIndex, count);
            Reopen();

            var expected = NewPopulatedPlainList();
            expected.RemoveRange(startIndex, count);

            IteratorAssert.SameContent(expected, SingleActivatableCollection());
        }

        public void TestReverse()
        {
            SingleActivatableCollection().Reverse();
            Reopen();

            var expected = NewPopulatedPlainList();
            expected.Reverse();

            IteratorAssert.AreEqual(expected.GetEnumerator(), SingleActivatableCollection().GetEnumerator());
        }

        public void TestReverseWithIndexAndCount()
        {
            const int index = 1;
            const int count = 2;

            SingleActivatableCollection().Reverse(index, count);
            Reopen();

            var expected = NewPopulatedPlainList();
            expected.Reverse(index, count);

            IteratorAssert.AreEqual(expected.GetEnumerator(), SingleActivatableCollection().GetEnumerator());
        }

        public void TestSortDefaultComparer()
        {
            var actual = SingleActivatableCollection();
            actual.Sort();

            var expected = NewPopulatedPlainList();
            expected.Sort();

            IteratorAssert.AreEqual(expected.GetEnumerator(), actual.GetEnumerator());
        }

        public void TestSortWithIndexAndComparer()
        {
            const int index = 1;
            const int count = 3;
            var comparer = new SimpleComparer();

            SingleActivatableCollection().Sort(index, count, comparer);
            Reopen();

            var expected = NewPopulatedPlainList();
            expected.Sort(index, count, comparer);

            IteratorAssert.AreEqual(expected.GetEnumerator(), SingleActivatableCollection().GetEnumerator());
        }

        public void TestSortWithComparer()
        {
            var comparer = new SimpleComparer();

            SingleActivatableCollection().Sort(comparer);
            Reopen();

            var expected = NewPopulatedPlainList();
            expected.Sort(comparer);

            IteratorAssert.AreEqual(expected.GetEnumerator(), SingleActivatableCollection().GetEnumerator());
        }

        public void TestSortComparison()
        {
            Comparison<ICollectionElement> comparison =
                delegate(ICollectionElement lhs, ICollectionElement rhs) { return lhs.CompareTo(rhs); };

            SingleActivatableCollection().Sort(comparison);
            Reopen();

            var expected = NewPopulatedPlainList();
            expected.Sort(comparison);

            IteratorAssert.AreEqual(expected.GetEnumerator(), SingleActivatableCollection().GetEnumerator());
        }

        public void TestToArray()
        {
            var expected = NewPopulatedPlainList().ToArray();
            var actual = SingleActivatableCollection().ToArray();

            IteratorAssert.AreEqual(expected, actual.GetEnumerator());
        }

        public void TestTrimExcess()
        {
            var collection = SingleActivatableCollection();
            collection.Clear();
            collection.TrimExcess();

            Assert.AreEqual(collection.Count, collection.Capacity);
        }

#if !SILVERLIGHT
        public void TestTrueForAll()
        {
            Assert.IsGreater(0, SingleActivatableCollection().Count);
            Db().Purge();

            var count = 0;
            Assert.IsTrue(SingleActivatableCollection().TrueForAll(delegate(ICollectionElement candidate)
            {
                count++;
                return candidate.Name.Length > 1;
            }));

            Assert.IsGreater(0, count);
        }

        public void TestConvertAll()
        {
            Converter<ICollectionElement, string> toString = delegate(ICollectionElement source) { return source.Name; };
            var expectedlNames = NewPopulatedPlainList().ConvertAll(toString);
            var actualNames = SingleActivatableCollection().ConvertAll(toString);

            IteratorAssert.AreEqual(expectedlNames.GetEnumerator(), actualNames.GetEnumerator());
        }
#endif

        #endregion
    }
}