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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Btree;
using Db4objects.Db4o.Internal.Fieldindex;
using Db4objects.Db4o.Query;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Fieldindex
{
    public class FieldIndexProcessorTestCase : FieldIndexProcessorTestCaseBase
    {
        public static void Main(string[] args)
        {
            new FieldIndexProcessorTestCase().RunAll();
        }

        protected override void Configure(IConfiguration config)
        {
            base.Configure(config);
            IndexField(config, typeof (NonIndexedFieldIndexItem), "indexed");
        }

        protected override void Store()
        {
            Container().ProduceClassMetadata(ReflectClass(typeof (NonIndexedFieldIndexItem)));
            StoreItems(new[] {3, 4, 7, 9});
            StoreComplexItems(new[] {3, 4, 7, 9}, new[] {2, 2, 8, 8});
        }

        public virtual void TestIdentity()
        {
            var query = CreateComplexItemQuery();
            query.Descend("foo").Constrain(3);
            var item = (ComplexFieldIndexItem) query.Execute().Next();
            query = CreateComplexItemQuery();
            query.Descend("child").Constrain(item).Identity();
            AssertExpectedFoos(typeof (ComplexFieldIndexItem), new[] {4}, query);
        }

        public virtual void TestSingleIndexNotSmaller()
        {
            var query = CreateItemQuery();
            query.Descend("foo").Constrain(5).Smaller().Not();
            AssertExpectedFoos(typeof (FieldIndexItem), new[] {7, 9}, query);
        }

        public virtual void TestSingleIndexNotGreater()
        {
            var query = CreateItemQuery();
            query.Descend("foo").Constrain(4).Greater().Not();
            AssertExpectedFoos(typeof (FieldIndexItem), new[] {3, 4}, query);
        }

        public virtual void TestSingleIndexSmallerOrEqual()
        {
            var query = CreateItemQuery();
            query.Descend("foo").Constrain(7).Smaller().Equal();
            AssertExpectedFoos(typeof (FieldIndexItem), new[] {3, 4, 7}, query);
        }

        public virtual void TestSingleIndexGreaterOrEqual()
        {
            var query = CreateItemQuery();
            query.Descend("foo").Constrain(7).Greater().Equal();
            AssertExpectedFoos(typeof (FieldIndexItem), new[] {7, 9}, query);
        }

        public virtual void TestSingleIndexRange()
        {
            var query = CreateItemQuery();
            query.Descend("foo").Constrain(3).Greater();
            query.Descend("foo").Constrain(9).Smaller();
            AssertExpectedFoos(typeof (FieldIndexItem), new[] {4, 7}, query);
        }

        public virtual void TestSingleIndexAndRange()
        {
            var query = CreateItemQuery();
            var c1 = query.Descend("foo").Constrain(3).Greater();
            var c2 = query.Descend("foo").Constrain(9).Smaller();
            c1.And(c2);
            AssertExpectedFoos(typeof (FieldIndexItem), new[] {4, 7}, query);
        }

        public virtual void TestSingleIndexOr()
        {
            var query = CreateItemQuery();
            var c1 = query.Descend("foo").Constrain(4).Smaller();
            var c2 = query.Descend("foo").Constrain(7).Greater();
            c1.Or(c2);
            AssertExpectedFoos(typeof (FieldIndexItem), new[] {3, 9}, query);
        }

        public virtual void TestExplicitAndOverOr()
        {
            AssertAndOverOrQuery(true);
        }

        public virtual void TestImplicitAndOverOr()
        {
            AssertAndOverOrQuery(false);
        }

        public virtual void TestSingleIndexOrRange()
        {
            var query = CreateItemQuery();
            var c1 = query.Descend("foo").Constrain(1).Greater();
            var c2 = query.Descend("foo").Constrain(4).Smaller();
            var c3 = query.Descend("foo").Constrain(4).Greater();
            var c4 = query.Descend("foo").Constrain(10).Smaller();
            var cc1 = c1.And(c2);
            var cc2 = c3.And(c4);
            cc1.Or(cc2);
            AssertExpectedFoos(typeof (FieldIndexItem), new[] {3, 7, 9}, query);
        }

        public virtual void TestImplicitAndOnOrs()
        {
            var query = CreateItemQuery();
            var c1 = query.Descend("foo").Constrain(4).Smaller();
            var c2 = query.Descend("foo").Constrain(3).Greater();
            var c3 = query.Descend("foo").Constrain(4).Greater();
            c1.Or(c2);
            c1.Or(c3);
            AssertExpectedFoos(typeof (FieldIndexItem), new[] {3, 4, 7, 9}, query);
        }

        public virtual void TestTwoLevelDescendOr()
        {
            var query = CreateComplexItemQuery();
            var c1 = query.Descend("child").Descend("foo").Constrain(4).Smaller();
            var c2 = query.Descend("child").Descend("foo").Constrain(4).Greater();
            c1.Or(c2);
            AssertExpectedFoos(typeof (ComplexFieldIndexItem), new[] {4, 9}, query);
        }

        public virtual void TestThreeOrs()
        {
            var query = CreateItemQuery();
            var c1 = query.Descend("foo").Constrain(3);
            var c2 = query.Descend("foo").Constrain(4);
            var c3 = query.Descend("foo").Constrain(7);
            c1.Or(c2).Or(c3);
            AssertExpectedFoos(typeof (FieldIndexItem), new[] {3, 4, 7}, query);
        }

        public virtual void _testOrOnDifferentFields()
        {
            var query = CreateComplexItemQuery();
            var c1 = query.Descend("foo").Constrain(3);
            var c2 = query.Descend("bar").Constrain(8);
            c1.Or(c2);
            AssertExpectedFoos(typeof (ComplexFieldIndexItem), new[] {3, 7, 9}, query);
        }

        public virtual void TestCantOptimizeOrInvolvingNonIndexedField()
        {
            var query = CreateQuery(typeof (NonIndexedFieldIndexItem));
            var c1 = query.Descend("indexed").Constrain(1);
            var c2 = query.Descend("foo").Constrain(2);
            c1.Or(c2);
            AssertCantOptimize(query);
        }

        public virtual void TestCantOptimizeDifferentLevels()
        {
            var query = CreateComplexItemQuery();
            var c1 = query.Descend("child").Descend("foo").Constrain(4).Smaller();
            var c2 = query.Descend("foo").Constrain(7).Greater();
            c1.Or(c2);
            AssertCantOptimize(query);
        }

        public virtual void TestCantOptimizeJoinOnNonIndexedFields()
        {
            var query = CreateQuery(typeof (NonIndexedFieldIndexItem));
            var c1 = query.Descend("foo").Constrain(1);
            var c2 = query.Descend("foo").Constrain(2);
            c1.Or(c2);
            AssertCantOptimize(query);
        }

        public virtual void TestIndexSelection()
        {
            var query = CreateComplexItemQuery();
            query.Descend("bar").Constrain(2);
            query.Descend("foo").Constrain(3);
            AssertBestIndex("foo", query);
            query = CreateComplexItemQuery();
            query.Descend("foo").Constrain(3);
            query.Descend("bar").Constrain(2);
            AssertBestIndex("foo", query);
        }

        public virtual void TestDoubleDescendingOnQuery()
        {
            var query = CreateComplexItemQuery();
            query.Descend("child").Descend("foo").Constrain(3);
            AssertExpectedFoos(typeof (ComplexFieldIndexItem), new[] {4}, query);
        }

        public virtual void TestTripleDescendingOnQuery()
        {
            var query = CreateComplexItemQuery();
            query.Descend("child").Descend("child").Descend("foo").Constrain(3);
            AssertExpectedFoos(typeof (ComplexFieldIndexItem), new[] {7}, query);
        }

        public virtual void TestMultiTransactionSmallerWithCommit()
        {
            var transaction = NewTransaction();
            FillTransactionWith(transaction, 0);
            var expectedZeros = NewBTreeNodeSizedArray(0);
            AssertSmaller(transaction, expectedZeros, 3);
            transaction.Commit();
            FillTransactionWith(transaction, 5);
            AssertSmaller(IntArrays4.Concat(expectedZeros, new[] {3, 4}), 7);
        }

        public virtual void TestMultiTransactionWithRollback()
        {
            var transaction = NewTransaction();
            FillTransactionWith(transaction, 0);
            var expectedZeros = NewBTreeNodeSizedArray(0);
            AssertSmaller(transaction, expectedZeros, 3);
            transaction.Rollback();
            AssertSmaller(transaction, new int[0], 3);
            FillTransactionWith(transaction, 5);
            AssertSmaller(new[] {3, 4}, 7);
        }

        public virtual void TestMultiTransactionSmaller()
        {
            var transaction = NewTransaction();
            FillTransactionWith(transaction, 0);
            var expected = NewBTreeNodeSizedArray(0);
            AssertSmaller(transaction, expected, 3);
            FillTransactionWith(transaction, 5);
            AssertSmaller(new[] {3, 4}, 7);
        }

        public virtual void TestMultiTransactionGreater()
        {
            FillTransactionWith(SystemTrans(), 10);
            FillTransactionWith(SystemTrans(), 5);
            AssertGreater(new[] {4, 7, 9}, 3);
            RemoveFromTransaction(SystemTrans(), 5);
            AssertGreater(new[] {4, 7, 9}, 3);
            RemoveFromTransaction(SystemTrans(), 10);
            AssertGreater(new[] {4, 7, 9}, 3);
        }

        public virtual void TestSingleIndexEquals()
        {
            var expectedBar = 3;
            AssertExpectedFoos(typeof (FieldIndexItem), new[] {expectedBar}, CreateQuery
                (expectedBar));
        }

        public virtual void TestSingleIndexSmaller()
        {
            AssertSmaller(new[] {3, 4}, 7);
        }

        public virtual void TestSingleIndexGreater()
        {
            AssertGreater(new[] {4, 7, 9}, 3);
        }

        private void AssertCantOptimize(IQuery query)
        {
            var result = ExecuteProcessor(query);
            Assert.AreSame(FieldIndexProcessorResult.NoIndexFound, result);
        }

        private void AssertBestIndex(string expectedFieldIndex, IQuery query)
        {
            var node = SelectBestIndex(query);
            AssertComplexItemIndex(expectedFieldIndex, node);
        }

        private void AssertAndOverOrQuery(bool explicitAnd)
        {
            var query = CreateItemQuery();
            var c1 = query.Descend("foo").Constrain(3);
            var c2 = query.Descend("foo").Constrain(9);
            var c3 = query.Descend("foo").Constrain(3);
            var c4 = query.Descend("foo").Constrain(7);
            var cc1 = c1.Or(c2);
            var cc2 = c3.Or(c4);
            if (explicitAnd)
            {
                cc1.And(cc2);
            }
            AssertExpectedFoos(typeof (FieldIndexItem), new[] {3}, query);
        }

        private void AssertGreater(int[] expectedFoos, int greaterThan)
        {
            var query = CreateItemQuery();
            query.Descend("foo").Constrain(greaterThan).Greater();
            AssertExpectedFoos(typeof (FieldIndexItem), expectedFoos, query);
        }

        private void AssertExpectedFoos(Type itemClass, int[] expectedFoos, IQuery query)
        {
            var trans = TransactionFromQuery(query);
            var expectedIds = MapToObjectIds(CreateQuery(trans, itemClass), expectedFoos);
            AssertExpectedIDs(expectedIds, query);
        }

        private void AssertExpectedIDs(int[] expectedIds, IQuery query)
        {
            var result = ExecuteProcessor(query);
            if (expectedIds.Length == 0)
            {
                Assert.AreSame(FieldIndexProcessorResult.FoundIndexButNoMatch, result);
                return;
            }
            AssertTreeInt(expectedIds, result.ToTreeInt());
        }

        private FieldIndexProcessorResult ExecuteProcessor(IQuery query)
        {
            return CreateProcessor(query).Run();
        }

        private BTree Btree()
        {
            return FieldIndexBTree(typeof (FieldIndexItem), "foo");
        }

        private void Store(Transaction trans, FieldIndexItem item)
        {
            Container().Store(trans, item);
        }

        private void FillTransactionWith(Transaction trans, int bar)
        {
            for (var i = 0; i < BTreeAssert.FillSize(Btree()); ++i)
            {
                Store(trans, new FieldIndexItem(bar));
            }
        }

        private int[] NewBTreeNodeSizedArray(int value)
        {
            var btree = Btree();
            return BTreeAssert.NewBTreeNodeSizedArray(btree, value);
        }

        private void RemoveFromTransaction(Transaction trans, int foo)
        {
            var found = CreateItemQuery(trans).Execute();
            while (found.HasNext())
            {
                var item = (FieldIndexItem) found.Next();
                if (item.foo == foo)
                {
                    Container().Delete(trans, item);
                }
            }
        }

        private void AssertSmaller(int[] expectedFoos, int smallerThan)
        {
            AssertSmaller(Trans(), expectedFoos, smallerThan);
        }

        private void AssertSmaller(Transaction transaction, int[] expectedFoos, int smallerThan
            )
        {
            var query = CreateItemQuery(transaction);
            query.Descend("foo").Constrain(smallerThan).Smaller();
            AssertExpectedFoos(typeof (FieldIndexItem), expectedFoos, query);
        }
    }
}