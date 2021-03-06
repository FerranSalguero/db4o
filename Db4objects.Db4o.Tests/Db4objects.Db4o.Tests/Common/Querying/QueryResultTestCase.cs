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

using Db4objects.Db4o.Config;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Query.Processor;
using Db4objects.Db4o.Internal.Query.Result;
using Db4objects.Db4o.Query;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Querying
{
    public abstract class QueryResultTestCase : AbstractDb4oTestCase, IOptOutMultiSession
        , IOptOutDefragSolo
    {
        private static readonly int[] Values = {1, 5, 6, 7, 9};
        private readonly int[] itemIds = new int[Values.Length];
        private int idForGetAll;

        protected override void Configure(IConfiguration config)
        {
            IndexField(config, typeof (Item), "foo");
        }

        public virtual void TestClassQuery()
        {
            AssertIDs(ClassOnlyQuery(), itemIds);
        }

        public virtual void TestGetAll()
        {
            var queryResult = NewQueryResult();
            queryResult.LoadFromClassIndexes(Container().ClassCollection().Iterator());
            var ids = IntArrays4.Concat(itemIds, new[] {idForGetAll});
            AssertIDs(queryResult, ids, true);
        }

        public virtual void TestIndexedFieldQuery()
        {
            var query = NewItemQuery();
            query.Descend("foo").Constrain(6).Smaller();
            var queryResult = ExecuteQuery(query);
            AssertIDs(queryResult, new[] {itemIds[0], itemIds[1]});
        }

        public virtual void TestNonIndexedFieldQuery()
        {
            var query = NewItemQuery();
            query.Descend("bar").Constrain(6).Smaller();
            var queryResult = ExecuteQuery(query);
            AssertIDs(queryResult, new[] {itemIds[0], itemIds[1]});
        }

        private IQueryResult ClassOnlyQuery()
        {
            var queryResult = NewQueryResult();
            queryResult.LoadFromClassIndex(ClassMetadata());
            return queryResult;
        }

        private ClassMetadata ClassMetadata()
        {
            return ClassMetadataFor(typeof (Item));
        }

        private IQueryResult ExecuteQuery(IQuery query)
        {
            var queryResult = NewQueryResult();
            queryResult.LoadFromQuery((QQuery) query);
            return queryResult;
        }

        private void AssertIDs(IQueryResult queryResult, int[] expectedIDs)
        {
            AssertIDs(queryResult, expectedIDs, false);
        }

        private void AssertIDs(IQueryResult queryResult, int[] expectedIDs, bool ignoreUnexpected
            )
        {
            var expectingVisitor = new ExpectingVisitor(IntArrays4.ToObjectArray
                (expectedIDs), false, ignoreUnexpected);
            var i = queryResult.IterateIDs();
            while (i.MoveNext())
            {
                expectingVisitor.Visit(i.CurrentInt());
            }
            expectingVisitor.AssertExpectations();
        }

        protected virtual IQuery NewItemQuery()
        {
            return NewQuery(typeof (Item));
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            StoreItems(Values);
            var ifga = new ItemForGetAll();
            Store(ifga);
            idForGetAll = (int) Db().GetID(ifga);
        }

        protected virtual void StoreItems(int[] foos)
        {
            for (var i = 0; i < foos.Length; i++)
            {
                var item = new Item(foos[i]);
                Store(item);
                itemIds[i] = (int) Db().GetID(item);
            }
        }

        protected abstract AbstractQueryResult NewQueryResult();

        public class Item
        {
            public int bar;
            public int foo;

            public Item()
            {
            }

            public Item(int foo_)
            {
                foo = foo_;
                bar = foo;
            }
        }

        public class ItemForGetAll
        {
        }
    }
}