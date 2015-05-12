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
using Db4objects.Db4o.CS;
using Db4objects.Db4o.Events;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.IO;
using Db4oUnit;
using Db4oUnit.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Events
{
    public class OwnCommittedCallbacksFixture
    {
        public static readonly FixtureVariable Factory = FixtureVariable.NewInstance("mode"
            );

        public static readonly FixtureVariable Action = FixtureVariable.NewInstance("client"
            );

        public interface IContainerFactory : ILabeled
        {
            IObjectContainer OpenClient();
            void Open();
            void Close();
        }

        public abstract class CommitAction : ILabeled
        {
            public abstract string Label();

            public virtual void CommitItem(object item, IObjectContainer clientA, IObjectContainer
                clientB)
            {
                var client = SelectClient(clientA, clientB);
                client.Store(item);
                client.Commit();
            }

            public abstract bool SelectsFirstClient();

            protected abstract IObjectContainer SelectClient(IObjectContainer clientA, IObjectContainer
                clientB);
        }

        public class NetworkingCSContainerFactory : IContainerFactory
        {
            private static readonly string Host = "localhost";
            private static readonly string User = "db4o";
            private static readonly string Pass = "db4o";
            private IObjectServer _server;

            public virtual void Open()
            {
                var config = Db4oClientServer.NewServerConfiguration();
                config.File.Storage = new MemoryStorage();
                _server = Db4oClientServer.OpenServer(config, string.Empty, Db4oClientServer.ArbitraryPort
                    );
                _server.GrantAccess(User, Pass);
            }

            public virtual IObjectContainer OpenClient()
            {
                return Db4oClientServer.OpenClient(Host, _server.Ext().Port(), User, Pass);
            }

            public virtual void Close()
            {
                _server.Close();
            }

            public virtual string Label()
            {
                return "Networking C/S";
            }
        }

        public class EmbeddedCSContainerFactory : IContainerFactory
        {
            private IObjectServer _server;

            public virtual void Open()
            {
                var config = Db4oClientServer.NewServerConfiguration();
                config.File.Storage = new MemoryStorage();
                _server = Db4oClientServer.OpenServer(config, string.Empty, 0);
            }

            public virtual IObjectContainer OpenClient()
            {
                return _server.OpenClient();
            }

            public virtual void Close()
            {
                _server.Close();
            }

            public virtual string Label()
            {
                return "Embedded C/S";
            }
        }

        public class EmbeddedSessionContainerFactory : IContainerFactory
        {
            private IEmbeddedObjectContainer _server;

            public virtual void Open()
            {
                var config = Db4oEmbedded.NewConfiguration();
                config.File.Storage = new MemoryStorage();
                _server = Db4oEmbedded.OpenFile(config, string.Empty);
            }

            public virtual IObjectContainer OpenClient()
            {
                return _server.Ext().OpenSession();
            }

            public virtual void Close()
            {
                _server.Close();
            }

            public virtual string Label()
            {
                return "Embedded Session";
            }
        }

        public class ClientACommitAction : CommitAction
        {
            protected override IObjectContainer SelectClient(IObjectContainer clientA, IObjectContainer
                clientB)
            {
                return clientA;
            }

            public override bool SelectsFirstClient()
            {
                return true;
            }

            public override string Label()
            {
                return "Client A";
            }
        }

        public class ClientBCommitAction : CommitAction
        {
            protected override IObjectContainer SelectClient(IObjectContainer clientA, IObjectContainer
                clientB)
            {
                return clientB;
            }

            public override bool SelectsFirstClient()
            {
                return false;
            }

            public override string Label()
            {
                return "Client B";
            }
        }

        public class OwnCommitCallbackFlaggedTestUnit : ITestCase
        {
            private const long Timeout = 1000;
#if !CF
            /// <exception cref="System.Exception"></exception>
            public virtual void TestCommittedCallbacks()
            {
                var lockObject = new Lock4();
                var ownEvent = new BooleanByRef(false);
                var gotEvent = new BooleanByRef(false);
                var shallListen = new BooleanByRef(false);
                var factory = ((IContainerFactory
                    ) Factory.Value);
                var action = ((CommitAction
                    ) Action.Value);
                factory.Open();
                var clientA = factory.OpenClient();
                var clientB = factory.OpenClient();
                var registry = EventRegistryFactory.ForObjectContainer(clientA);
                registry.Committed += new _IEventListener4_153(shallListen, gotEvent, ownEvent, lockObject).OnEvent;
                lockObject.Run(new _IClosure4_170(shallListen, action, clientA, clientB, lockObject
                    ));
                shallListen.value = false;
                clientB.Close();
                clientA.Close();
                factory.Close();
                Assert.IsTrue(gotEvent.value);
                Assert.AreEqual(action.SelectsFirstClient(), ownEvent.value);
            }
#endif // !CF

            private sealed class _IEventListener4_153
            {
                private readonly BooleanByRef gotEvent;
                private readonly Lock4 lockObject;
                private readonly BooleanByRef ownEvent;
                private readonly BooleanByRef shallListen;

                public _IEventListener4_153(BooleanByRef shallListen, BooleanByRef gotEvent, BooleanByRef
                    ownEvent, Lock4 lockObject)
                {
                    this.shallListen = shallListen;
                    this.gotEvent = gotEvent;
                    this.ownEvent = ownEvent;
                    this.lockObject = lockObject;
                }

                public void OnEvent(object sender, CommitEventArgs args)
                {
                    if (!shallListen.value)
                    {
                        return;
                    }
                    Assert.IsFalse(gotEvent.value);
                    gotEvent.value = true;
                    ownEvent.value = args.IsOwnCommit();
                    lockObject.Run(new _IClosure4_161(lockObject));
                }

                private sealed class _IClosure4_161 : IClosure4
                {
                    private readonly Lock4 lockObject;

                    public _IClosure4_161(Lock4 lockObject)
                    {
                        this.lockObject = lockObject;
                    }

                    public object Run()
                    {
                        lockObject.Awake();
                        return null;
                    }
                }
            }

            private sealed class _IClosure4_170 : IClosure4
            {
                private readonly CommitAction action;
                private readonly IObjectContainer clientA;
                private readonly IObjectContainer clientB;
                private readonly Lock4 lockObject;
                private readonly BooleanByRef shallListen;

                public _IClosure4_170(BooleanByRef shallListen, CommitAction
                    action, IObjectContainer clientA, IObjectContainer clientB, Lock4 lockObject)
                {
                    this.shallListen = shallListen;
                    this.action = action;
                    this.clientA = clientA;
                    this.clientB = clientB;
                    this.lockObject = lockObject;
                }

                public object Run()
                {
                    shallListen.value = true;
                    action.CommitItem(new OwnCommitCallbackFlaggedNetworkingTestSuite.Item(42), clientA
                        , clientB);
                    lockObject.Snooze(Timeout
                        );
                    return null;
                }
            }
        }
    }
}

#endif // !SILVERLIGHT