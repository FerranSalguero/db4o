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

namespace Db4objects.Db4o.Tests.Common.Querying
{
    public class CascadedDeleteUpdate : AbstractDb4oTestCase
    {
        public static void Main(string[] arguments)
        {
            //		new CascadedDeleteUpdate().runSolo();
            new CascadedDeleteUpdate().RunNetworking();
        }

        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(typeof (ParentItem)).CascadeOnDelete(true);
        }

        protected override void Store()
        {
            var parentItem1 = new ParentItem
                ();
            var parentItem2 = new ParentItem
                ();
            var child = new ChildItem();
            child.parent1 = parentItem1;
            child.parent2 = parentItem2;
            parentItem1.child = child;
            parentItem2.child = child;
            Db().Store(parentItem1);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestAllObjectStored()
        {
            AssertAllObjectStored();
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestUpdate()
        {
            var q = NewQuery(typeof (ParentItem));
            var objectSet = q.Execute();
            while (objectSet.HasNext())
            {
                Db().Store(objectSet.Next());
            }
            Db().Commit();
            AssertAllObjectStored();
        }

        /// <exception cref="System.Exception"></exception>
        private void AssertAllObjectStored()
        {
            Reopen();
            var q = NewQuery(typeof (ParentItem));
            var objectSet = q.Execute();
            while (objectSet.HasNext())
            {
                var parentItem = (ParentItem) objectSet
                    .Next();
                Db().Refresh(parentItem, 3);
                Assert.IsNotNull(parentItem.child);
            }
        }

        public class ParentItem
        {
            public object child;
        }

        public class ChildItem
        {
            public object parent1;
            public object parent2;
        }
    }
}