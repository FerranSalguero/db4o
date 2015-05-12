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
using Db4objects.Db4o.Config;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.CS
{
    public class CsDeleteReaddTestCase : Db4oClientServerTestCase
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.GenerateUUIDs(ConfigScope.Globally);
            config.GenerateCommitTimestamps(true);
            config.ObjectClass(typeof (Item)).ObjectField("name").Indexed
                (true);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(new Item("one"));
        }

        public virtual void TestDeleteReadd()
        {
            var client1 = Db();
            var client2 = OpenNewSession();
            var item1 = (Item
                ) RetrieveOnlyInstance(client1, typeof (Item));
            var item2 = (Item
                ) RetrieveOnlyInstance(client2, typeof (Item));
            var idBeforeDelete = client1.GetID(item1);
            client1.Delete(item1);
            client1.Commit();
            client2.Store(item2);
            client2.Commit();
            client2.Close();
            var item3 = ((Item) RetrieveOnlyInstance
                (client1, typeof (Item)));
            var idAfterUpdate = client1.GetID(item3);
            Assert.AreEqual(idBeforeDelete, idAfterUpdate);
            new FieldIndexAssert(typeof (Item), "name").AssertSingleEntry
                (FileSession(), idAfterUpdate);
        }

        public static void Main(string[] arguments)
        {
            new CsDeleteReaddTestCase().RunAll();
        }

        public class ItemParent
        {
        }

        public class Item : ItemParent
        {
            public string name;

            public Item(string name_)
            {
                name = name_;
            }
        }
    }
}

#endif // !SILVERLIGHT