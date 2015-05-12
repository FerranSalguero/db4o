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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.CS.Internal;
using Db4objects.Db4o.CS.Internal.Messages;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Api
{
    public class StoreAllTestCase : AbstractDb4oTestCase
    {
        internal Item item1 = new Item(1);
        internal Item item2 = new Item(2);

        public static void Main(string[] args)
        {
            new StoreAllTestCase().RunAll();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.ClientServer().BatchMessages(false);
        }

        public virtual void Test()
        {
            StoreAll(Container());
            ObjectSetAssert.SameContent(QueryAllItems(), new object[] {item1, item2});
        }

        private void StoreAll(IInternalObjectContainer internalObjectContainer)
        {
            internalObjectContainer.StoreAll(Trans(), Iterators.Iterate(new[] {item1, item2}));
        }

        public virtual void TestClientSendsSingleMessage()
        {
            if (!(Container() is ClientObjectContainer))
            {
                return;
            }
            var clientObjectContainer = (ClientObjectContainer) Container();
            var messages = new ArrayList();
            ClientObjectContainer.IMessageListener listener = new _IMessageListener_65(messages
                );
            Db().Store(new Item(0));
            // class creation
            clientObjectContainer.MessageListener(listener);
            StoreAll(clientObjectContainer);
            clientObjectContainer.Commit();
            Assert.AreEqual(1, messages.Count);
        }

        private IObjectSet QueryAllItems()
        {
            var q = Db().Query();
            q.Constrain(typeof (Item));
            return q.Execute();
        }

        public class Item
        {
            public int _id;

            public Item(int id)
            {
                _id = id;
            }

            public override bool Equals(object obj)
            {
                var other = (Item) obj;
                return _id != other._id;
            }
        }

        private sealed class _IMessageListener_65 : ClientObjectContainer.IMessageListener
        {
            private readonly ArrayList messages;

            public _IMessageListener_65(ArrayList messages)
            {
                this.messages = messages;
            }

            public void OnMessage(Msg msg)
            {
                messages.Add(msg);
            }
        }
    }
}

#endif // !SILVERLIGHT