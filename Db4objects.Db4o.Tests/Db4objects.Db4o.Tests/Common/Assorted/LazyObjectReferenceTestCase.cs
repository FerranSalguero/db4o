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
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Internal;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Assorted
{
    public class LazyObjectReferenceTestCase : AbstractDb4oTestCase
    {
        public static void Main(string[] arguments)
        {
            new LazyObjectReferenceTestCase().RunSolo();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            base.Configure(config);
            config.ObjectClass(typeof (Item)).GenerateUUIDs(true);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            for (var i = 0; i < 10; i++)
            {
                Store(new Item());
            }
        }

        public virtual void Test()
        {
            var q = Db().Query();
            q.Constrain(typeof (Item));
            var objectSet = q.Execute();
            var ids = objectSet.Ext().GetIDs();
            var infos = new IObjectInfo[ids.Length];
            var items = new Item[ids
                .Length];
            for (var i = 0; i < items.Length; i++)
            {
                items[i] = (Item) Db().GetByID(ids[i]);
                infos[i] = new LazyObjectReference(Trans(), (int) ids[i]);
            }
            AssertInfosAreConsistent(ids, infos);
            for (var i = 0; i < items.Length; i++)
            {
                Db().Purge(items[i]);
            }
            Db().Purge();
            AssertInfosAreConsistent(ids, infos);
        }

        private void AssertInfosAreConsistent(long[] ids, IObjectInfo[] infos)
        {
            for (var i = 0; i < infos.Length; i++)
            {
                var info = Db().GetObjectInfo(Db().GetByID(ids[i]));
                Assert.AreEqual(info.GetInternalID(), infos[i].GetInternalID());
                Assert.AreEqual(info.GetUUID().GetLongPart(), infos[i].GetUUID().GetLongPart());
                Assert.AreSame(info.GetObject(), infos[i].GetObject());
            }
        }

        public class Item
        {
        }
    }
}