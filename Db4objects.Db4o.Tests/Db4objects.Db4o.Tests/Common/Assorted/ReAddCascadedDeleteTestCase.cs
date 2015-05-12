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
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Assorted
{
    public class ReAddCascadedDeleteTestCase : AbstractDb4oTestCase
    {
        public static void Main(string[] args)
        {
            new ReAddCascadedDeleteTestCase().RunAll();
        }

        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(typeof (Item)).CascadeOnDelete(true
                );
            config.ObjectClass(typeof (Item)).ObjectField("_name")
                .Indexed(true);
        }

        protected override void Store()
        {
            Db().Store(new Item("parent", new Item
                ("child")));
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestDeletingAndReaddingMember()
        {
            DeleteParentAndReAddChild();
            Reopen();
            Assert.IsNotNull(Query("child"));
            Assert.IsNull(Query("parent"));
        }

        private void DeleteParentAndReAddChild()
        {
            var i = Query("parent");
            Db().Delete(i);
            Db().Store(i._member);
            Db().Commit();
            var id = Db().GetID(i._member);
            new FieldIndexAssert(typeof (Item), "_name").AssertSingleEntry
                (FileSession(), id);
        }

        private Item Query(string name)
        {
            var objectSet = Db().QueryByExample(new Item(name
                ));
            if (!objectSet.HasNext())
            {
                return null;
            }
            return (Item) objectSet.Next();
        }

        public class Item
        {
            public Item _member;
            public string _name;

            public Item()
            {
            }

            public Item(string name)
            {
                _name = name;
            }

            public Item(string name, Item member)
            {
                _name = name;
                _member = member;
            }
        }
    }
}