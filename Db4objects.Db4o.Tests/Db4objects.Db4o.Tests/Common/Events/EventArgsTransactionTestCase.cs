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
using Db4objects.Db4o.Foundation;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Events
{
    public class EventArgsTransactionTestCase : AbstractDb4oTestCase
    {
        public virtual void TestTransactionInEventArgs()
        {
            var factory = EventRegistryFactory.ForObjectContainer(Db());
            var called = new BooleanByRef();
            var foundTrans = new ObjectByRef();
            factory.Creating += new _IEventListener4_20(called, foundTrans).OnEvent;
            Db().Store(new Item());
            Db().Commit();
            Assert.IsTrue(called.value);
            Assert.AreSame(Trans(), foundTrans.value);
        }

        public static void Main(string[] args)
        {
            new EventArgsTransactionTestCase().RunAll();
        }

        public class Item
        {
        }

        private sealed class _IEventListener4_20
        {
            private readonly BooleanByRef called;
            private readonly ObjectByRef foundTrans;

            public _IEventListener4_20(BooleanByRef called, ObjectByRef foundTrans)
            {
                this.called = called;
                this.foundTrans = foundTrans;
            }

            public void OnEvent(object sender, CancellableObjectEventArgs
                args)
            {
                called.value = true;
                foundTrans.value = args.Transaction();
            }
        }
    }
}