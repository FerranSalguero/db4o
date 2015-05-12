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
using Db4objects.Db4o.CS.Internal;
using Db4objects.Db4o.Events;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Internal;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;
using Sharpen;

namespace Db4objects.Db4o.Tests.Common.Acid
{
    public class ReadCommittedIsolationTestCase : AbstractDb4oTestCase, IOptOutSolo
    {
        private static readonly string Original = "original";
        private static readonly string Modified = "modified";
        private readonly object _updatesMonitor = new object();
        private IExtObjectContainer _client2;
        // We introduce this variable to be able to wait for completion.
        // For a real usecase it is not necessary.
        public static void Main(string[] arguments)
        {
            new ReadCommittedIsolationTestCase().RunAll();
        }

        public virtual void TestRefresh()
        {
            var item2 = RetrieveOnlyInstance(Client2());
            Assert.AreEqual(Original, item2.name);
            var item1 = RetrieveOnlyInstance(Client1());
            Assert.AreEqual(Original, item1.name);
            item1.name = Modified;
            Client1().Store(item1);
            Client1().Commit();
            Assert.AreEqual(Original, item2.name);
            Client2().Refresh(item2, 2);
            Assert.AreEqual(Modified, item2.name);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestPushedUpdates()
        {
            RegisterPushedUpdates(Client2());
            var item2 = RetrieveOnlyInstance(Client2());
            Assert.AreEqual(Original, item2.name);
            var item1 = RetrieveOnlyInstance(Client1());
            Assert.AreNotSame(item2, item1);
            Assert.AreEqual(Original, item1.name);
            item1.name = Modified;
            Client1().Store(item1);
            lock (_updatesMonitor)
            {
                Client1().Commit();
                if (IsNetworkingCS())
                {
                    Runtime.Wait(_updatesMonitor, 1000);
                }
            }
            Assert.AreEqual(Modified, item2.name);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Db4oSetupAfterStore()
        {
            _client2 = OpenNewSession();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Db4oTearDownBeforeClean()
        {
            _client2.Close();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(new Item(Original));
        }

        private IExtObjectContainer Client1()
        {
            return Db();
        }

        private IExtObjectContainer Client2()
        {
            return _client2;
        }

        private Item RetrieveOnlyInstance(IExtObjectContainer
            container)
        {
            var q = container.Query();
            q.Constrain(typeof (Item));
            var objectSet = q.Execute();
            Assert.AreEqual(1, objectSet.Count);
            return (Item) objectSet.Next();
        }

        private bool IsNetworkingCS()
        {
            return Client2() is ClientObjectContainer;
        }

        private void RegisterPushedUpdates(IExtObjectContainer client)
        {
            var eventRegistry = EventRegistryFactory.ForObjectContainer(client);
            eventRegistry.Committed += new _IEventListener4_120(this, client).OnEvent;
        }

        public class Item
        {
            public string name;

            public Item(string name_)
            {
                name = name_;
            }

            public override string ToString()
            {
                return "Item: " + name;
            }
        }

        private sealed class _IEventListener4_120
        {
            private readonly ReadCommittedIsolationTestCase _enclosing;
            private readonly IExtObjectContainer client;

            public _IEventListener4_120(ReadCommittedIsolationTestCase _enclosing, IExtObjectContainer
                client)
            {
                this._enclosing = _enclosing;
                this.client = client;
            }

            public void OnEvent(object sender, CommitEventArgs args)
            {
                lock (_enclosing._updatesMonitor)
                {
                    var trans = ((IInternalObjectContainer) client).Transaction;
                    var updated = args.Updated;
                    var infos = updated.GetEnumerator();
                    while (infos.MoveNext())
                    {
                        var info = (IObjectInfo) infos.Current;
                        var obj = trans.ObjectForIdFromCache((int) info.GetInternalID());
                        if (obj == null)
                        {
                            continue;
                        }
                        // DEPTH may need to be 2 for member collections
                        // to be updated also.
                        client.Refresh(obj, 1);
                    }
                    if (_enclosing.IsNetworkingCS())
                    {
                        Runtime.NotifyAll(_enclosing._updatesMonitor);
                    }
                }
            }
        }
    }
}

#endif // !SILVERLIGHT