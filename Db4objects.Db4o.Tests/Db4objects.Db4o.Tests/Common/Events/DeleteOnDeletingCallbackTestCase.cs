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
using Db4objects.Db4o.CS.Internal;
using Db4objects.Db4o.Events;
using Db4objects.Db4o.Foundation;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Events
{
    public class DeleteOnDeletingCallbackTestCase : AbstractDb4oTestCase
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(new RootItem());
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void Test()
        {
            var disconnected = new BooleanByRef();
            var Lock = new Lock4();
            if (IsNetworking())
            {
                var clientServerFixture = (IDb4oClientServerFixture) Fixture(
                    );
                var objectServerEvents = (IObjectServerEvents) clientServerFixture
                    .Server();
                objectServerEvents.ClientDisconnected += new _IEventListener4_46(Lock, disconnected).OnEvent;
            }
            var root = ((RootItem
                ) RetrieveOnlyInstance(typeof (RootItem)));
            root.child = new Item();
            Db().Store(root);
            Db().Delete(root);
            Reopen();
            if (IsNetworking())
            {
                Lock.Run(new _IClosure4_63(disconnected, Lock));
            }
            AssertClassIndexIsEmpty();
        }

        private void AssertClassIndexIsEmpty()
        {
            Iterator4Assert.AreEqual(new object[0], GetAllIds());
        }

        private IIntIterator4 GetAllIds()
        {
            return FileSession().GetAll(FileSession().Transaction, QueryEvaluationMode.Immediate
                ).IterateIDs();
        }

        public class Item
        {
        }

        public class RootItem
        {
            public Item child;

            public virtual void ObjectOnDelete(IObjectContainer container)
            {
                container.Delete(child);
            }
        }

        private sealed class _IEventListener4_46
        {
            private readonly BooleanByRef disconnected;
            private readonly Lock4 Lock;

            public _IEventListener4_46(Lock4 Lock, BooleanByRef disconnected)
            {
                this.Lock = Lock;
                this.disconnected = disconnected;
            }

            public void OnEvent(object sender, StringEventArgs args)
            {
                Lock.Run(new _IClosure4_47(disconnected, Lock));
            }

            private sealed class _IClosure4_47 : IClosure4
            {
                private readonly BooleanByRef disconnected;
                private readonly Lock4 Lock;

                public _IClosure4_47(BooleanByRef disconnected, Lock4 Lock)
                {
                    this.disconnected = disconnected;
                    this.Lock = Lock;
                }

                public object Run()
                {
                    disconnected.value = true;
                    Lock.Awake();
                    return null;
                }
            }
        }

        private sealed class _IClosure4_63 : IClosure4
        {
            private readonly BooleanByRef disconnected;
            private readonly Lock4 Lock;

            public _IClosure4_63(BooleanByRef disconnected, Lock4 Lock)
            {
                this.disconnected = disconnected;
                this.Lock = Lock;
            }

            public object Run()
            {
                if (!disconnected.value)
                {
                    Lock.Snooze(1000000);
                }
                return null;
            }
        }
    }
}

#endif // !SILVERLIGHT