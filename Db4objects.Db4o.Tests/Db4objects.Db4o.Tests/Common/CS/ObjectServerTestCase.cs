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
using Db4objects.Db4o.Tests.Common.Api;
using Db4oUnit;
using Sharpen;

namespace Db4objects.Db4o.Tests.Common.CS
{
    public class ObjectServerTestCase : TestWithTempFile
    {
        private string fileName;
        private IExtObjectServer server;

        public virtual void TestClientCount()
        {
            AssertClientCount(0);
            var client1 = OpenClient();
            try
            {
                AssertClientCount(1);
                var client2 = OpenClient();
                try
                {
                    AssertClientCount(2);
                }
                finally
                {
                    client2.Close();
                }
            }
            finally
            {
                client1.Close();
            }
        }

        // closing is asynchronous, relying on completion is hard
        // That's why there is no test here. 
        // ClientProcessesTestCase tests closing.
        public virtual void TestClientDisconnectedEvent()
        {
            var client = (ClientObjectContainer) OpenClient();
            var clientName = client.UserName;
            var eventRaised = new BooleanByRef();
            var events = (IObjectServerEvents) server;
            var Lock = new Lock4();
            events.ClientDisconnected += new _IEventListener4_51(clientName, eventRaised, Lock).OnEvent;
            Lock.Run(new _IClosure4_58(client, eventRaised, Lock));
        }

        public virtual void TestClientConnectedEvent()
        {
            var connections = new ArrayList();
            var events = (IObjectServerEvents) server;
            events.ClientConnected += new
                _IEventListener4_83(connections).OnEvent;
            var client = OpenClient();
            try
            {
                Assert.AreEqual(1, connections.Count);
                Iterator4Assert.AreEqual(ServerMessageDispatchers(), Iterators.Iterator(connections
                    ));
            }
            finally
            {
                client.Close();
            }
        }

        public virtual void TestServerClosedEvent()
        {
            var receivedEvent = new BooleanByRef(false);
            var events = (IObjectServerEvents) server;
            events.Closed += new _IEventListener4_101
                (receivedEvent).OnEvent;
            server.Close();
            Assert.IsTrue(receivedEvent.value);
        }

        private IEnumerator ServerMessageDispatchers()
        {
            return ((ObjectServerImpl) server).IterateDispatchers();
        }

        /// <exception cref="System.Exception"></exception>
        public override void SetUp()
        {
            fileName = TempFile();
            server = Db4oClientServer.OpenServer(fileName, -1).Ext();
            server.GrantAccess(Credentials(), Credentials());
        }

        /// <exception cref="System.Exception"></exception>
        public override void TearDown()
        {
            server.Close();
            base.TearDown();
        }

        private IObjectContainer OpenClient()
        {
            return Db4oClientServer.OpenClient("localhost", Port(), Credentials(), Credentials
                ());
        }

        private void AssertClientCount(int count)
        {
            Assert.AreEqual(count, server.ClientCount());
        }

        private int Port()
        {
            return server.Port();
        }

        private string Credentials()
        {
            return "DB4O";
        }

        private sealed class _IEventListener4_51
        {
            private readonly string clientName;
            private readonly BooleanByRef eventRaised;
            private readonly Lock4 Lock;

            public _IEventListener4_51(string clientName, BooleanByRef eventRaised, Lock4 Lock
                )
            {
                this.clientName = clientName;
                this.eventRaised = eventRaised;
                this.Lock = Lock;
            }

            public void OnEvent(object sender, StringEventArgs args)
            {
                Assert.AreEqual(clientName, args.Message);
                eventRaised.value = true;
                Lock.Awake();
            }
        }

        private sealed class _IClosure4_58 : IClosure4
        {
            private readonly ClientObjectContainer client;
            private readonly BooleanByRef eventRaised;
            private readonly Lock4 Lock;

            public _IClosure4_58(ClientObjectContainer client, BooleanByRef eventRaised, Lock4
                Lock)
            {
                this.client = client;
                this.eventRaised = eventRaised;
                this.Lock = Lock;
            }

            public object Run()
            {
                client.Close();
                var startTime = Runtime.CurrentTimeMillis();
                var currentTime = startTime;
                var timeOut = 1000;
                var timePassed = currentTime - startTime;
                while (timePassed < timeOut && !eventRaised.value)
                {
                    Lock.Snooze(timeOut - timePassed);
                    currentTime = Runtime.CurrentTimeMillis();
                    timePassed = currentTime - startTime;
                }
                Assert.IsTrue(eventRaised.value);
                return null;
            }
        }

        private sealed class _IEventListener4_83
        {
            private readonly ArrayList connections;

            public _IEventListener4_83(ArrayList connections)
            {
                this.connections = connections;
            }

            public void OnEvent(object sender, ClientConnectionEventArgs args)
            {
                connections.Add(args.Connection);
            }
        }

        private sealed class _IEventListener4_101
        {
            private readonly BooleanByRef receivedEvent;

            public _IEventListener4_101(BooleanByRef receivedEvent)
            {
                this.receivedEvent = receivedEvent;
            }

            public void OnEvent(object sender, ServerClosedEventArgs args)
            {
                receivedEvent.value = true;
            }
        }
    }
}

#endif // !SILVERLIGHT