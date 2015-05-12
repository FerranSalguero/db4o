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
using Db4objects.Db4o.Messaging;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Staging
{
    public class PingTestCase : Db4oClientServerTestCase, IOptOutAllButNetworkingCS
    {
        internal TestMessageRecipient recipient = new TestMessageRecipient
            ();

        public static void Main(string[] args)
        {
            new PingTestCase().RunAll();
        }

        protected override void Configure(IConfiguration config)
        {
            config.ClientServer().TimeoutClientSocket(1000);
        }

        public virtual void Test()
        {
            ClientServerFixture().Server().Ext().Configure().ClientServer().SetMessageRecipient
                (recipient);
            var client = ClientServerFixture().Db();
            var sender = client.Configure().ClientServer().GetMessageSender();
            if (IsEmbedded())
            {
                Assert.Expect(typeof (NotSupportedException), new _ICodeBlock_36(sender));
                return;
            }
            sender.Send(new Data());
            // The following query will be block by the sender
            var os = client.QueryByExample(null);
            while (os.HasNext())
            {
                os.Next();
            }
            Assert.IsFalse(client.IsClosed());
        }

        private sealed class _ICodeBlock_36 : ICodeBlock
        {
            private readonly IMessageSender sender;

            public _ICodeBlock_36(IMessageSender sender)
            {
                this.sender = sender;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                sender.Send(new Data());
            }
        }

        public class TestMessageRecipient : IMessageRecipient
        {
            public virtual void ProcessMessage(IMessageContext con, object message)
            {
                Runtime4.Sleep(3000);
            }
        }

        public class Data
        {
        }
    }
}