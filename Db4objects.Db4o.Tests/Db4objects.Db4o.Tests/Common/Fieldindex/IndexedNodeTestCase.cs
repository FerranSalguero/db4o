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

using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Fieldindex;
using Db4objects.Db4o.Query;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Fieldindex
{
    public class IndexedNodeTestCase : FieldIndexProcessorTestCaseBase
    {
        public static void Main(string[] args)
        {
            new IndexedNodeTestCase().RunSolo();
        }

        protected override void Store()
        {
            StoreItems(new[] {3, 4, 7, 9});
            StoreComplexItems(new[] {3, 4, 7, 9}, new[] {2, 2, 8, 8});
        }

        public virtual void TestTwoLevelDescendOr()
        {
            var query = CreateComplexItemQuery();
            var c1 = query.Descend("child").Descend("foo").Constrain(4).Smaller();
            var c2 = query.Descend("child").Descend("foo").Constrain(4).Greater();
            c1.Or(c2);
            AssertSingleOrNode(query);
        }

        public virtual void TestMultipleOrs()
        {
            var query = CreateComplexItemQuery();
            var c1 = query.Descend("foo").Constrain(4).Smaller();
            for (var i = 0; i < 5; i++)
            {
                var c2 = query.Descend("foo").Constrain(4).Greater();
                c1 = c1.Or(c2);
            }
            AssertSingleOrNode(query);
        }

        public virtual void TestDoubleDescendingOnIndexedNodes()
        {
            var query = CreateComplexItemQuery();
            query.Descend("child").Descend("foo").Constrain(3);
            query.Descend("bar").Constrain(2);
            var index = SelectBestIndex(query);
            AssertComplexItemIndex("foo", index);
            Assert.IsFalse(index.IsResolved());
            var result = index.Resolve();
            Assert.IsNotNull(result);
            AssertComplexItemIndex("child", result);
            Assert.IsTrue(result.IsResolved());
            Assert.IsNull(result.Resolve());
            AssertComplexItems(new[] {4}, result.ToTreeInt());
        }

        public virtual void TestTripleDescendingOnQuery()
        {
            var query = CreateComplexItemQuery();
            query.Descend("child").Descend("child").Descend("foo").Constrain(3);
            var index = SelectBestIndex(query);
            AssertComplexItemIndex("foo", index);
            Assert.IsFalse(index.IsResolved());
            var result = index.Resolve();
            Assert.IsNotNull(result);
            AssertComplexItemIndex("child", result);
            Assert.IsFalse(result.IsResolved());
            result = result.Resolve();
            Assert.IsNotNull(result);
            AssertComplexItemIndex("child", result);
            AssertComplexItems(new[] {7}, result.ToTreeInt());
        }

        private void AssertComplexItems(int[] expectedFoos, TreeInt found)
        {
            Assert.IsNotNull(found);
            AssertTreeInt(MapToObjectIds(CreateComplexItemQuery(), expectedFoos), found);
        }

        private void AssertSingleOrNode(IQuery query)
        {
            var nodes = CreateProcessor(query).CollectIndexedNodes();
            Assert.IsTrue(nodes.MoveNext());
            var node = (OrIndexedLeaf) nodes.Current;
            Assert.IsNotNull(node);
            Assert.IsFalse(nodes.MoveNext());
        }
    }
}