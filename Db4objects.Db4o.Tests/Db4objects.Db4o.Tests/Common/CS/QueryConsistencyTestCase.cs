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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Query;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.CS
{
    public class QueryConsistencyTestCase : AbstractDb4oTestCase, IOptOutAllButNetworkingCS
    {
        public static void Main(string[] args)
        {
            new _Db4oTestSuite_14().RunAll();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.OptimizeNativeQueries(false);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(new Item(42));
        }

        public virtual void TestDelete()
        {
            var found = SodaQueryForItem(42);
            Assert.AreEqual(42, found._id);
            Db().Delete(found);
            Assert.IsNull(SodaQueryForItem(42));
            Assert.IsNull(NativeQueryForItem(42));
            Db().Commit();
            Assert.IsNull(SodaQueryForItem(42));
            Assert.IsNull(NativeQueryForItem(42));
        }

        public virtual void TestUpdate()
        {
            var found = SodaQueryForItem(42);
            Assert.AreEqual(42, found._id);
            Assert.AreSame(found, NativeQueryForItem(42));
            found._id = 21;
            Assert.IsNull(SodaQueryForItem(21));
            Assert.AreSame(found, SodaQueryForItem(42));
            Assert.AreSame(found, NativeQueryForItem(42));
            Store(found);
            Assert.AreSame(found, SodaQueryForItem(21));
            Assert.AreEqual(21, found._id);
            Assert.AreSame(found, NativeQueryForItem(21));
            Assert.AreEqual(21, found._id);
            Db().Commit();
            Assert.AreSame(found, NativeQueryForItem(21));
        }

        private Item NativeQueryForItem(int id)
        {
            var result = Db().Query(new ItemById(id));
            return ((Item) FirstOrNull(result));
        }

        private Item SodaQueryForItem(int id)
        {
            var q = Db().Query();
            q.Constrain(typeof (Item));
            q.Descend("_id").Constrain(id).Equal();
            return ((Item) FirstOrNull(q.Execute()));
        }

        private object FirstOrNull(IObjectSet result)
        {
            return result.HasNext() ? result.Next() : null;
        }

        private sealed class _Db4oTestSuite_14 : Db4oTestSuite
        {
            protected override Type[] TestCases()
            {
                return new[] {typeof (QueryConsistencyTestCase)};
            }
        }

        public class Item
        {
            public int _id;

            public Item(int id)
            {
                _id = id;
            }
        }

        [Serializable]
        public sealed class ItemById : Predicate
        {
            public int _id;

            public ItemById(int id)
            {
                _id = id;
            }

            public bool Match(Item candidate)
            {
                return candidate._id == _id;
            }
        }
    }
}

#endif // !SILVERLIGHT