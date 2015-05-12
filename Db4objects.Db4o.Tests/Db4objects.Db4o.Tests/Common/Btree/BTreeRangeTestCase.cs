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

using Db4objects.Db4o.Internal.Btree;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Btree
{
    public class BTreeRangeTestCase : BTreeTestCaseBase
    {
        public static void Main(string[] args)
        {
            new BTreeRangeTestCase().RunSolo();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Db4oSetupAfterStore()
        {
            base.Db4oSetupAfterStore();
            Add(new[] {3, 7, 4, 9});
        }

        public virtual void TestLastPointer()
        {
            AssertLastPointer(8, 7);
            AssertLastPointer(11, 9);
            AssertLastPointer(4, 3);
        }

        private void AssertLastPointer(int searchValue, int expectedValue)
        {
            var single = Search(searchValue);
            var smallerRange = single.Smaller();
            var lastPointer = smallerRange.LastPointer();
            Assert.AreEqual(expectedValue, lastPointer.Key());
        }

        public virtual void TestSize()
        {
            AssertSize(4, Range(3, 9));
            AssertSize(3, Range(4, 9));
            AssertSize(3, Range(3, 7));
            AssertSize(4, Range(2, 9));
            AssertSize(4, Range(3, 10));
            Add(new[] {5, 6, 8, 10, 2, 1});
            AssertSize(10, Range(1, 10));
            AssertSize(9, Range(1, 9));
            AssertSize(9, Range(2, 10));
            AssertSize(9, Range(2, 11));
            AssertSize(10, Range(0, 10));
        }

        private void AssertSize(int size, IBTreeRange range)
        {
            Assert.AreEqual(size, range.Size());
        }

        public virtual void TestIntersectSingleSingle()
        {
            AssertIntersection(new[] {4, 7}, Range(3, 7), Range(4, 9));
            AssertIntersection(new int[] {}, Range(3, 4), Range(7, 9));
            AssertIntersection(new[] {3, 4, 7, 9}, Range(3, 9), Range(3, 9));
            AssertIntersection(new[] {3, 4, 7, 9}, Range(3, 10), Range(3, 9));
            AssertIntersection(new int[] {}, Range(1, 2), Range(3, 9));
        }

        public virtual void TestIntersectSingleUnion()
        {
            var union = Range(3, 3).Union(Range(7, 9));
            var single = Range(4, 7);
            AssertIntersection(new[] {7}, union, single);
            AssertIntersection(new[] {3, 7}, union, Range(3, 7));
        }

        public virtual void TestIntersectUnionUnion()
        {
            var union1 = Range(3, 3).Union(Range(7, 9));
            var union2 = Range(3, 3).Union(Range(9, 9));
            AssertIntersection(new[] {3, 9}, union1, union2);
        }

        public virtual void TestUnion()
        {
            AssertUnion(new[] {3, 4, 7, 9}, Range(3, 4), Range(7, 9));
            AssertUnion(new[] {3, 4, 7, 9}, Range(3, 7), Range(4, 9));
            AssertUnion(new[] {3, 7, 9}, Range(3, 3), Range(7, 9));
            AssertUnion(new[] {3, 9}, Range(3, 3), Range(9, 9));
        }

        public virtual void TestIsEmpty()
        {
            Assert.IsTrue(Range(0, 0).IsEmpty());
            Assert.IsFalse(Range(3, 3).IsEmpty());
            Assert.IsFalse(Range(9, 9).IsEmpty());
            Assert.IsTrue(Range(10, 10).IsEmpty());
        }

        public virtual void TestUnionWithEmptyDoesNotCreateNewRange()
        {
            var range = Range(3, 4);
            var empty = Range(0, 0);
            Assert.AreSame(range, range.Union(empty));
            Assert.AreSame(range, empty.Union(range));
            var union = range.Union(Range(8, 9));
            Assert.AreSame(union, union.Union(empty));
            Assert.AreSame(union, empty.Union(union));
        }

        public virtual void TestUnionsMerge()
        {
            var range = Range(3, 3).Union(Range(7, 7)).Union(Range(4, 4));
            AssertIsRangeSingle(range);
            BTreeAssert.AssertRange(new[] {3, 4, 7}, range);
        }

        private void AssertIsRangeSingle(IBTreeRange range)
        {
            Assert.IsInstanceOf(typeof (BTreeRangeSingle), range);
        }

        public virtual void TestUnionsOfUnions()
        {
            var union1 = Range(3, 4).Union(Range(8, 9));
            BTreeAssert.AssertRange(new[] {3, 4, 9}, union1);
            BTreeAssert.AssertRange(new[] {3, 4, 7, 9}, union1.Union(Range(7, 7)));
            var union2 = Range(3, 3).Union(Range(7, 7));
            AssertUnion(new[] {3, 4, 7, 9}, union1, union2);
            AssertIsRangeSingle(union1.Union(union2));
            AssertIsRangeSingle(union2.Union(union1));
            var union3 = Range(3, 3).Union(Range(9, 9));
            AssertUnion(new[] {3, 7, 9}, union2, union3);
        }

        public virtual void TestExtendToLastOf()
        {
            BTreeAssert.AssertRange(new[] {3, 4, 7}, Range(3, 7));
            BTreeAssert.AssertRange(new[] {4, 7, 9}, Range(4, 9));
        }

        public virtual void TestUnionOfOverlappingSingleRangesYieldSingleRange()
        {
            Assert.IsInstanceOf(typeof (BTreeRangeSingle), Range(3, 4).Union(Range(4, 9)));
        }

        private void AssertUnion(int[] expectedKeys, IBTreeRange range1, IBTreeRange range2
            )
        {
            BTreeAssert.AssertRange(expectedKeys, range1.Union(range2));
            BTreeAssert.AssertRange(expectedKeys, range2.Union(range1));
        }

        private void AssertIntersection(int[] expectedKeys, IBTreeRange range1, IBTreeRange
            range2)
        {
            BTreeAssert.AssertRange(expectedKeys, range1.Intersect(range2));
            BTreeAssert.AssertRange(expectedKeys, range2.Intersect(range1));
        }
    }
}