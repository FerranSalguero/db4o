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

using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Staging
{
    /// <summary>#COR-1790</summary>
    public class UntypedFieldSortingTestCase : AbstractDb4oTestCase
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(new Item(2));
            Store(new Item(3));
            Store(new Item(1));
        }

        public virtual void Test()
        {
            var query = Db().Query();
            query.Constrain(typeof (Item));
            query.Descend("_id").OrderAscending();
            var objectSet = query.Execute();
            var lastId = 0;
            while (objectSet.HasNext())
            {
                var item = ((Item) objectSet
                    .Next());
                var currentId = ((int) item._id);
                Assert.IsGreater(lastId, currentId);
                currentId = lastId;
            }
        }

        public class Item
        {
            public object _id;

            public Item(object id)
            {
                _id = id;
            }
        }
    }
}