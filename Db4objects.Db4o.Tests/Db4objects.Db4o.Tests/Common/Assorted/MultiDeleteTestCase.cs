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
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Assorted
{
    public class MultiDeleteTestCase : AbstractDb4oTestCase, IOptOutDefragSolo
    {
        public static void Main(string[] args)
        {
            new MultiDeleteTestCase().RunSoloAndClientServer();
        }

        protected override void Configure(IConfiguration config)
        {
            var itemClass = config.ObjectClass(typeof (Item));
            itemClass.CascadeOnDelete(true);
            itemClass.CascadeOnUpdate(true);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            var md = new Item();
            md.name = "killmefirst";
            md.SetMembers();
            md.child = new Item();
            md.child.SetMembers();
            Db().Store(md);
        }

        public virtual void TestDeleteCanBeCalledTwice()
        {
            var item = ItemByName("killmefirst");
            Assert.IsNotNull(item);
            var id = Db().GetID(item);
            Db().Delete(item);
            Assert.AreSame(item, ItemById(id));
            Db().Delete(item);
            Assert.AreSame(item, ItemById(id));
        }

        private Item ItemByName(string name)
        {
            var q = NewQuery(typeof (Item));
            q.Descend("name").Constrain(name);
            return (Item) q.Execute().Next();
        }

        private Item ItemById(long id)
        {
            return (Item) Db().GetByID(id);
        }

        public class Item
        {
            public Item child;
            public object forLong;
            public long myLong;
            public string name;
            public long[] typedArr;
            public object[] untypedArr;

            public virtual void SetMembers()
            {
                forLong = Convert.ToInt64(100);
                myLong = Convert.ToInt64(100);
                untypedArr = new object[]
                {
                    Convert.ToInt64(10), "hi", new Item
                        ()
                };
                typedArr = new[]
                {
                    Convert.ToInt64(3), Convert.ToInt64(7), Convert.ToInt64
                        (9)
                };
            }
        }
    }
}