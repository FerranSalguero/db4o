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
using System.Collections;
using Db4objects.Db4o.CS;
using Db4objects.Db4o.CS.Internal;
using Db4objects.Db4o.Events;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Freespace;
using Db4objects.Db4o.Internal.Ids;
using Db4objects.Db4o.Internal.Slots;
using Db4objects.Db4o.Tests.Common.Api;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.CS
{
    public class PrefetchIDCountTestCase : TestWithTempFile
    {
        private const int PrefetchIdCount = 100;
        private static readonly string User = "db4o";
        private static readonly string Password = "db4o";

        public static void Main(string[] args)
        {
            new ConsoleTestRunner(typeof (PrefetchIDCountTestCase)).Run();
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void Test()
        {
            var server = (ObjectServerImpl) Db4oClientServer.OpenServer(TempFile(
                ), Db4oClientServer.ArbitraryPort);
            var Lock = new Lock4();
            server.ClientDisconnected += new _IEventListener4_39(Lock).OnEvent;
            server.GrantAccess(User, Password);
            var client = OpenClient(server.Port());
            var msgDispatcher = FirstMessageDispatcherFor(server);
            var transaction = msgDispatcher.Transaction();
            var idSystem = transaction.IdSystem();
            var prefetchedID = idSystem.PrefetchID();
            Assert.IsGreater(0, prefetchedID);
            Lock.Run(new _IClosure4_58(client, Lock, idSystem, prefetchedID));
            // This wont work with the PointerBasedIdSystem
            server.Close();
        }

        private ServerMessageDispatcherImpl FirstMessageDispatcherFor(ObjectServerImpl server
            )
        {
            var dispatchers = server.IterateDispatchers();
            Assert.IsTrue(dispatchers.MoveNext());
            var msgDispatcher = (ServerMessageDispatcherImpl) dispatchers
                .Current;
            return msgDispatcher;
        }

        private IObjectContainer OpenClient(int port)
        {
            var config = Db4oClientServer.NewClientConfiguration();
            config.PrefetchIDCount = PrefetchIdCount;
            return Db4oClientServer.OpenClient(config, "localhost", port, User, Password);
        }

        public class Item
        {
        }

        private sealed class _IEventListener4_39
        {
            private readonly Lock4 Lock;

            public _IEventListener4_39(Lock4 Lock)
            {
                this.Lock = Lock;
            }

            public void OnEvent(object sender, StringEventArgs args)
            {
                Lock.Run(new _IClosure4_40(Lock));
            }

            private sealed class _IClosure4_40 : IClosure4
            {
                private readonly Lock4 Lock;

                public _IClosure4_40(Lock4 Lock)
                {
                    this.Lock = Lock;
                }

                public object Run()
                {
                    Lock.Awake();
                    return null;
                }
            }
        }

        private sealed class _IClosure4_58 : IClosure4
        {
            private readonly IObjectContainer client;
            private readonly ITransactionalIdSystem idSystem;
            private readonly Lock4 Lock;
            private readonly int prefetchedID;

            public _IClosure4_58(IObjectContainer client, Lock4 Lock, ITransactionalIdSystem
                idSystem, int prefetchedID)
            {
                this.client = client;
                this.Lock = Lock;
                this.idSystem = idSystem;
                this.prefetchedID = prefetchedID;
            }

            public object Run()
            {
                client.Close();
                Lock.Snooze(100000);
                Assert.Expect(typeof (InvalidIDException), new _ICodeBlock_63(idSystem, prefetchedID
                    ));
                return null;
            }

            private sealed class _ICodeBlock_63 : ICodeBlock
            {
                private readonly ITransactionalIdSystem idSystem;
                private readonly int prefetchedID;

                public _ICodeBlock_63(ITransactionalIdSystem idSystem, int prefetchedID)
                {
                    this.idSystem = idSystem;
                    this.prefetchedID = prefetchedID;
                }

                /// <exception cref="System.Exception"></exception>
                public void Run()
                {
                    idSystem.CommittedSlot(prefetchedID);
                }
            }
        }

        public class DebugFreespaceManager : AbstractFreespaceManager
        {
            private readonly IList _freedSlots = new ArrayList();

            public DebugFreespaceManager(LocalObjectContainer file) : base(null, 0, 0)
            {
            }

            public virtual bool WasFreed(int id)
            {
                return _freedSlots.Contains(id);
            }

            public override Slot AllocateSlot(int length)
            {
                return null;
            }

            public override Slot AllocateSafeSlot(int length)
            {
                return null;
            }

            public override void BeginCommit()
            {
            }

            // TODO Auto-generated method stub
            public override void Commit()
            {
            }

            // TODO Auto-generated method stub
            public override void EndCommit()
            {
            }

            // TODO Auto-generated method stub
            public override void Free(Slot slot)
            {
                _freedSlots.Add(slot.Address());
            }

            public override void FreeSelf()
            {
            }

            // TODO Auto-generated method stub
            public override void FreeSafeSlot(Slot slot)
            {
            }

            // TODO Auto-generated method stub
            public override void Listener(IFreespaceListener listener)
            {
            }

            // TODO Auto-generated method stub
            public override void MigrateTo(IFreespaceManager fm)
            {
            }

            // TODO Auto-generated method stub
            public override int SlotCount()
            {
                // TODO Auto-generated method stub
                return 0;
            }

            public override void Start(int id)
            {
            }

            // TODO Auto-generated method stub
            public override byte SystemType()
            {
                // TODO Auto-generated method stub
                return 0;
            }

            public override int TotalFreespace()
            {
                // TODO Auto-generated method stub
                return 0;
            }

            public override void Traverse(IVisitor4 visitor)
            {
            }

            // TODO Auto-generated method stub
            public override void Write(LocalObjectContainer container)
            {
            }

            public override bool IsStarted()
            {
                return false;
            }

            public override Slot AllocateTransactionLogSlot(int length)
            {
                // TODO Auto-generated method stub
                return null;
            }

            public override void Read(LocalObjectContainer container, Slot slot)
            {
            }
        }
    }
}

#endif // !SILVERLIGHT