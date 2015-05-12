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

using Db4objects.Db4o.Events;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Assorted
{
    public class ObjectContainerMemberTestCase : AbstractDb4oTestCase
    {
        /// <exception cref="System.Exception"></exception>
        public virtual void Test()
        {
            var eventRegistryFactory = EventRegistryFactory.ForObjectContainer(Db(
                ));
            eventRegistryFactory.Creating += new _IEventListener4_23().OnEvent;
            var item = new Item(
                );
            item._typedObjectContainer = Db();
            item._untypedObjectContainer = Db();
            Store(item);
            // Special case: Cascades activation to existing ObjectContainer member
            Db().QueryByExample(typeof (Item)).Next();
        }

        public class Item
        {
            public IObjectContainer _typedObjectContainer;
            public object _untypedObjectContainer;
        }

        private sealed class _IEventListener4_23
        {
            public void OnEvent(object sender, CancellableObjectEventArgs
                args)
            {
                var obj = args.Object;
                Assert.IsFalse(obj is IObjectContainer);
            }
        }
    }
}