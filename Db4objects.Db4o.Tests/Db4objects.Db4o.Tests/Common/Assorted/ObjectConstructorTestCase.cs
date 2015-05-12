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

namespace Db4objects.Db4o.Tests.Common.Assorted
{
    public class ObjectConstructorTestCase : AbstractDb4oTestCase
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(typeof (Item)).Translate(new ItemConstructor
                ());
        }

        protected override void Store()
        {
            Store(new Item("one"));
        }

        public virtual void Test()
        {
            var item = (Item
                ) RetrieveOnlyInstance(typeof (Item));
            Assert.AreEqual("one", item._name);
        }

        public class Item
        {
            internal readonly string _name;

            public Item(string name)
            {
                _name = name;
            }
        }

        public class ItemConstructor : IObjectConstructor
        {
            public virtual object OnInstantiate(IObjectContainer container, object storedObject
                )
            {
                return new Item((string) storedObject);
            }

            public virtual void OnActivate(IObjectContainer container, object applicationObject
                , object storedObject)
            {
            }

            public virtual object OnStore(IObjectContainer container, object applicationObject
                )
            {
                return ((Item) applicationObject)._name;
            }

            public virtual Type StoredClass()
            {
                return typeof (string);
            }
        }
    }
}