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
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Slots;
using Db4objects.Db4o.Query;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Fieldindex
{
    /// <exclude></exclude>
    public abstract class StringIndexTestCaseBase : AbstractDb4oTestCase
    {
        protected override void Configure(IConfiguration config)
        {
            IndexField(config, typeof (Item), "name");
        }

        protected virtual void AssertItems(string[] expected, IObjectSet result)
        {
            var expectingVisitor = new ExpectingVisitor(ToObjectArray(expected));
            while (result.HasNext())
            {
                expectingVisitor.Visit(((Item) result.Next()).name);
            }
            expectingVisitor.AssertExpectations();
        }

        protected virtual object[] ToObjectArray(string[] source)
        {
            var array = new object[source.Length];
            Array.Copy(source, 0, array, 0, source.Length);
            return array;
        }

        protected virtual void GrafittiFreeSpace()
        {
            if (!(Db() is IoAdaptedObjectContainer))
            {
                return;
            }
            var file = ((IoAdaptedObjectContainer) Db());
            var fm = file.FreespaceManager();
            fm.Traverse(new _IVisitor4_60(file));
        }

        protected virtual void AssertExists(string itemName)
        {
            AssertExists(Trans(), itemName);
        }

        protected virtual void Add(string itemName)
        {
            Add(Trans(), itemName);
        }

        protected virtual void Add(Transaction transaction, string itemName)
        {
            Container().Store(transaction, new Item(itemName));
        }

        protected virtual void AssertExists(Transaction transaction, string itemName)
        {
            Assert.IsNotNull(Query(transaction, itemName));
        }

        protected virtual void Rename(Transaction transaction, string from, string to)
        {
            var item = Query(transaction, from);
            Assert.IsNotNull(item);
            item.name = to;
            Container().Store(transaction, item);
        }

        protected virtual void Rename(string from, string to)
        {
            Rename(Trans(), from, to);
        }

        protected virtual Item Query(string name)
        {
            return Query(Trans(), name);
        }

        protected virtual Item Query(Transaction transaction, string
            name)
        {
            var objectSet = NewQuery(transaction, name).Execute();
            if (!objectSet.HasNext())
            {
                return null;
            }
            return (Item) objectSet.Next();
        }

        protected virtual IQuery NewQuery(Transaction transaction, string itemName)
        {
            var query = Container().Query(transaction);
            query.Constrain(typeof (Item));
            query.Descend("name").Constrain(itemName);
            return query;
        }

        public class Item
        {
            public string name;

            public Item()
            {
            }

            public Item(string name_)
            {
                name = name_;
            }
        }

        private sealed class _IVisitor4_60 : IVisitor4
        {
            private readonly IoAdaptedObjectContainer file;

            public _IVisitor4_60(IoAdaptedObjectContainer file)
            {
                this.file = file;
            }

            public void Visit(object obj)
            {
                var slot = (Slot) obj;
                file.OverwriteDeletedBlockedSlot(slot);
            }
        }
    }
}