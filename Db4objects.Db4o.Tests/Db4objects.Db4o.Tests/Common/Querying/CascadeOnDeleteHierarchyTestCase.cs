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
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Querying
{
    public class CascadeOnDeleteHierarchyTestCase : AbstractDb4oTestCase
    {
        public static void Main(string[] args)
        {
            new CascadeOnDeleteHierarchyTestCase().RunAll();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(typeof (Item)).CascadeOnDelete
                (true);
            config.ObjectClass(typeof (SubItem));
            base.Configure(config);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(new SubItem());
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void Test()
        {
            var item = (SubItem) RetrieveOnlyInstance(typeof (SubItem
                ));
            Db().Delete(item);
            AssertOccurrences(typeof (Data), 0);
            Db().Commit();
            AssertOccurrences(typeof (Data), 0);
        }

        public virtual void TestMultipleStoreCalls()
        {
            var item = ((SubItem
                ) RetrieveOnlyInstance(typeof (SubItem)));
            Store(item);
            AssertOccurrences(typeof (Data), 1);
        }

        public class Item
        {
        }

        public class SubItem : Item
        {
            public Data data;

            public SubItem()
            {
                data = new Data();
            }
        }

        public class Data
        {
        }
    }
}