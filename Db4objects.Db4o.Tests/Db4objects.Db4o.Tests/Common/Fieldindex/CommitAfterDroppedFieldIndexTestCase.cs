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
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Fieldindex
{
    public class CommitAfterDroppedFieldIndexTestCase : Db4oClientServerTestCase
    {
        private const int ObjectCount = 100;

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.ClientServer().PrefetchIDCount(1);
            config.ClientServer().BatchMessages(false);
            config.BTreeNodeSize(4);
        }

        public virtual void Test()
        {
            for (var i = 0; i < ObjectCount; i++)
            {
                Store(new Item(1));
            }
            var storedField = FileSession().StoredClass(typeof (Item
                )).StoredField("_id", null);
            storedField.CreateIndex();
            FileSession().Commit();
            var session = OpenNewSession();
            var allItems = session.Query(typeof (Item
                ));
            for (var itemIter = allItems.GetEnumerator(); itemIter.MoveNext();)
            {
                var item = ((Item
                    ) itemIter.Current);
                item._id++;
                session.Store(item);
            }
            // Making sure all storing has been processed.
            session.SetSemaphore("anySemaphore", 0);
            storedField.DropIndex();
            session.Commit();
            storedField.CreateIndex();
        }

        public class Item
        {
            public int _id;

            public Item(int id)
            {
                _id = id;
            }
        }
    }
}

#endif // !SILVERLIGHT