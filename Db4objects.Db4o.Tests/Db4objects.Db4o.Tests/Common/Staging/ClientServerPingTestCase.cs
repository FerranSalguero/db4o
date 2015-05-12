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
using Db4objects.Db4o.CS.Internal.Messages;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Tests.Common.CS;
using Db4oUnit;
using Sharpen.Lang;

namespace Db4objects.Db4o.Tests.Common.Staging
{
    /// <exclude></exclude>
    public class ClientServerPingTestCase : ClientServerTestCaseBase
    {
        private const int ItemCount = 100;

        public static void Main(string[] arguments)
        {
            new ClientServerPingTestCase().RunNetworking();
        }

        protected override void Configure(IConfiguration config)
        {
            config.ClientServer().BatchMessages(false);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void Test()
        {
            if (IsEmbedded())
            {
                // This test really doesn't make sense for MTOC, there
                // is no client to ping.
                return;
            }
            var dispatcher = ServerDispatcher();
            var pingThread = new PingThread
                (dispatcher);
            pingThread.Start();
            for (var i = 0; i < ItemCount; i++)
            {
                var item = new Item(i);
                Store(item);
            }
            Assert.AreEqual(ItemCount, Db().QueryByExample(typeof (Item
                )).Count);
            pingThread.Close();
        }

        public class Item
        {
            public int data;

            public Item(int i)
            {
                data = i;
            }
        }

        internal class PingThread : Thread
        {
            private readonly object Lock = new object();
            internal IServerMessageDispatcher _dispatcher;
            internal bool _stop;

            public PingThread(IServerMessageDispatcher dispatcher)
            {
                _dispatcher = dispatcher;
            }

            public virtual void Close()
            {
                lock (Lock)
                {
                    _stop = true;
                }
            }

            private bool NotStopped()
            {
                lock (Lock)
                {
                    return !_stop;
                }
            }

            public override void Run()
            {
                while (NotStopped())
                {
                    _dispatcher.Write(Msg.Ping);
                    Runtime4.Sleep(1);
                }
            }
        }
    }
}

#endif // !SILVERLIGHT