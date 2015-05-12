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
using System;
using System.Collections;
using Db4objects.Db4o.CS.Internal.Messages;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Query;
using Db4oUnit;
using Db4oUnit.Extensions.Fixtures;
using Sharpen.Util;

namespace Db4objects.Db4o.Tests.Common.CS
{
    public class PrefetchConfigurationTestCase : ClientServerTestCaseBase, IOptOutAllButNetworkingCS
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Db4oSetupBeforeStore()
        {
            EnsureQueryGraphClassMetadataHasBeenExchanged();
        }

        public virtual void TestDefaultPrefetchDepth()
        {
            Assert.AreEqual(0, Client().Config().PrefetchDepth());
        }

        public virtual void TestPrefetchingBehaviorForClassOnlyQuery()
        {
            var query = Client().Query();
            query.Constrain(typeof (Item));
            AssertPrefetchingBehaviorFor(query, Msg.GetInternalIds);
        }

        public virtual void TestPrefetchingBehaviorForConstrainedQuery()
        {
            var query = Client().Query();
            query.Constrain(typeof (Item));
            query.Descend("child").Constrain(null);
            AssertPrefetchingBehaviorFor(query, Msg.QueryExecute);
        }

        public virtual void TestRefreshIsUnaffectedByPrefetchingBehavior()
        {
            var oc1 = Db();
            var oc2 = OpenNewSession();
            oc1.Configure().ClientServer().PrefetchDepth(1);
            oc2.Configure().ClientServer().PrefetchDepth(1);
            try
            {
                Item itemFromClient1 = new RootItem
                    (new Item());
                oc1.Store(itemFromClient1);
                oc1.Commit();
                itemFromClient1.child = null;
                oc1.Store(itemFromClient1);
                Item itemFromClient2 = ((RootItem
                    ) RetrieveOnlyInstance(oc2, typeof (RootItem)));
                Assert.IsNotNull(itemFromClient2.child);
                oc1.Rollback();
                itemFromClient2 = ((RootItem) RetrieveOnlyInstance(oc2
                    , typeof (RootItem)));
                oc2.Refresh(itemFromClient2, int.MaxValue);
                Assert.IsNotNull(itemFromClient2.child);
                oc1.Commit();
                itemFromClient2 = ((RootItem) RetrieveOnlyInstance(oc2
                    , typeof (RootItem)));
                Assert.IsNotNull(itemFromClient2.child);
                oc1.Store(itemFromClient1);
                oc1.Commit();
                oc2.Refresh(itemFromClient2, int.MaxValue);
                itemFromClient2 = ((RootItem) RetrieveOnlyInstance(oc2
                    , typeof (RootItem)));
                Assert.IsNull(itemFromClient2.child);
            }
            finally
            {
                oc2.Close();
            }
        }

        public virtual void TestMaxPrefetchingDepthBehavior()
        {
            StoreAllAndPurge(new[]
            {
                new Item
                    (new Item(new Item()
                        )),
                new Item(new Item
                    (new Item())),
                new Item
                    (new Item(new Item()
                        ))
            });
            Client().Config().PrefetchObjectCount(2);
            Client().Config().PrefetchDepth(int.MaxValue);
            var query = Client().Query();
            query.Constrain(typeof (Item));
            query.Descend("child").Descend("child").Constrain(null).Not();
            AssertQueryIterationProtocol(query, Msg.QueryExecute, new Stimulus
                []
            {
                new Depth2Stimulus(this, new MsgD[] {}), new
                    Depth2Stimulus(this, new MsgD[] {}),
                new Depth2Stimulus
                    (this, new MsgD[] {Msg.ReadMultipleObjects})
            });
        }

        public virtual void TestPrefetchingWithCyclesAscending()
        {
            var a = new Item(1);
            var b = new Item(2);
            var c = new Item(3);
            a.child = b;
            b.child = a;
            c.child = b;
            StoreAllAndPurge(new[] {a, b, c});
            Client().Config().PrefetchObjectCount(2);
            Client().Config().PrefetchDepth(2);
            var query = QueryForItemsWithChild();
            query.Descend("order").OrderAscending();
            AssertQueryIterationProtocol(query, Msg.QueryExecute, new Stimulus
                []
            {
                new Depth2Stimulus(this, new MsgD[] {}), new
                    Depth2Stimulus(this, new MsgD[] {}),
                new Depth2Stimulus
                    (this, new MsgD[] {Msg.ReadMultipleObjects})
            });
        }

        public virtual void TestPrefetchingWithCyclesDescending()
        {
            var a = new Item(1);
            var b = new Item(2);
            var c = new Item(3);
            a.child = b;
            b.child = a;
            c.child = b;
            StoreAllAndPurge(new[] {a, b, c});
            Client().Config().PrefetchObjectCount(2);
            Client().Config().PrefetchDepth(2);
            var query = QueryForItemsWithChild();
            query.Descend("order").OrderDescending();
            AssertQueryIterationProtocol(query, Msg.QueryExecute, new Stimulus
                []
            {
                new Depth2Stimulus(this, new MsgD[] {}), new
                    Depth2Stimulus(this, new MsgD[] {}),
                new Depth2Stimulus
                    (this, new MsgD[] {})
            });
        }

        public virtual void TestPrefetchingDepth2Behavior()
        {
            StoreDepth2Graph();
            Client().Config().PrefetchObjectCount(2);
            Client().Config().PrefetchDepth(2);
            var query = QueryForItemsWithChild();
            AssertQueryIterationProtocol(query, Msg.QueryExecute, new Stimulus
                []
            {
                new Depth2Stimulus(this, new MsgD[] {}), new
                    Depth2Stimulus(this, new MsgD[] {}),
                new Depth2Stimulus
                    (this, new MsgD[] {Msg.ReadMultipleObjects})
            });
        }

        public virtual void TestGraphOfDepth2WithPrefetchDepth1()
        {
            StoreDepth2Graph();
            Client().Config().PrefetchObjectCount(2);
            Client().Config().PrefetchDepth(1);
            var query = QueryForItemsWithChild();
            AssertQueryIterationProtocol(query, Msg.QueryExecute, new Stimulus
                []
            {
                new Depth2Stimulus(this, new MsgD[]
                {
                    Msg.ReadReaderById
                }),
                new Depth2Stimulus(this, new MsgD[]
                {
                    Msg.ReadReaderById
                }),
                new Depth2Stimulus(this, new MsgD[]
                {
                    Msg.ReadMultipleObjects
                    , Msg.ReadReaderById
                })
            });
        }

        public virtual void TestPrefetchCount1()
        {
            StoreAllAndPurge(new[]
            {
                new Item
                    (),
                new Item(), new Item
                    ()
            });
            Client().Config().PrefetchObjectCount(1);
            Client().Config().PrefetchDepth(1);
            var query = QueryForItemsWithoutChildren();
            AssertQueryIterationProtocol(query, Msg.QueryExecute, new[]
            {
                new Stimulus(new MsgD[] {}), new Stimulus
                    (new MsgD[] {Msg.ReadMultipleObjects}),
                new Stimulus
                    (new MsgD[] {Msg.ReadMultipleObjects})
            });
        }

        public virtual void TestPrefetchingAfterDeleteFromOtherClient()
        {
            StoreAllAndPurge(new[]
            {
                new Item
                    (),
                new Item(), new Item
                    ()
            });
            Client().Config().PrefetchObjectCount(1);
            Client().Config().PrefetchDepth(1);
            var query = QueryForItemsWithoutChildren();
            var result = query.Execute();
            DeleteAllItemsFromSecondClient();
            Assert.IsNotNull(((Item) result.Next()));
            Assert.Expect(typeof (InvalidOperationException), new _ICodeBlock_211(result));
        }

        private IQuery QueryForItemsWithoutChildren()
        {
            var query = NewQuery(typeof (Item));
            query.Descend("child").Constrain(null);
            return query;
        }

        private void DeleteAllItemsFromSecondClient()
        {
            var client = OpenNewSession();
            try
            {
                DeleteAll(client, typeof (Item));
                client.Commit();
            }
            finally
            {
                client.Close();
            }
        }

        private IQuery QueryForItemsWithChild()
        {
            var query = Client().Query();
            query.Constrain(typeof (Item));
            query.Descend("child").Constrain(null).Not();
            return query;
        }

        private void StoreDepth2Graph()
        {
            StoreAllAndPurge(new[]
            {
                new Item
                    (new Item()),
                new Item
                    (new Item()),
                new Item
                    (new Item())
            });
        }

        private void AssertPrefetchingBehaviorFor(IQuery query, MsgD expectedFirstMessage
            )
        {
            StoreFlatItemGraph();
            Client().Config().PrefetchObjectCount(2);
            Client().Config().PrefetchDepth(1);
            AssertQueryIterationProtocol(query, expectedFirstMessage, new[]
            {
                new Stimulus(new MsgD[] {}), new Stimulus
                    (new MsgD[] {}),
                new Stimulus(new MsgD[]
                {
                    Msg.ReadMultipleObjects
                }),
                new Stimulus(new MsgD[] {}), new Stimulus
                    (new MsgD[] {Msg.ReadMultipleObjects})
            });
        }

        private void AssertQueryIterationProtocol(IQuery query, MsgD expectedResultMessage
            , Stimulus[] stimuli)
        {
            var messages = MessageCollector.ForServerDispatcher(ServerDispatcher());
            var result = query.Execute();
            AssertMessages(messages, new IMessage[] {expectedResultMessage});
            messages.Clear();
            for (var stimulusIndex = 0; stimulusIndex < stimuli.Length; ++stimulusIndex)
            {
                var stimulus = stimuli[stimulusIndex];
                stimulus.ActUpon(result);
                AssertMessages(messages, stimulus.expectedMessagesAfter);
                messages.Clear();
            }
            if (result.HasNext())
            {
                Assert.Fail("Unexpected item: " + ((Item) result.Next
                    ()));
            }
            AssertMessages(messages, new IMessage[] {});
        }

        private void AssertMessages(IList actualMessages, IMessage[] expectedMessages)
        {
            Iterator4Assert.AreEqual(expectedMessages, Iterators.Iterator(actualMessages));
        }

        private void EnsureQueryGraphClassMetadataHasBeenExchanged()
        {
            Container().ProduceClassMetadata(ReflectClass(typeof (Item
                )));
            // ensures classmetadata exists for query objects
            var query = Client().Query();
            query.Constrain(typeof (Item));
            query.Descend("child").Descend("child").Constrain(null).Not();
            query.Descend("order").OrderAscending();
            Assert.AreEqual(0, query.Execute().Count);
        }

        private void StoreFlatItemGraph()
        {
            StoreAllAndPurge(new[]
            {
                new Item
                    (),
                new Item(), new Item
                    (),
                new Item(), new Item
                    ()
            });
        }

        private void StoreAllAndPurge(Item[] items)
        {
            StoreAll(items);
            PurgeAll(items);
            Client().Commit();
        }

        private void StoreAll(Item[] items)
        {
            for (var itemIndex = 0; itemIndex < items.Length; ++itemIndex)
            {
                var item = items[itemIndex];
                Client().Store(item);
            }
        }

        private void PurgeAll(Item[] items)
        {
            var purged = new HashSet();
            for (var itemIndex = 0; itemIndex < items.Length; ++itemIndex)
            {
                var item = items[itemIndex];
                Purge(purged, item);
            }
        }

        private void Purge(ISet purged, Item item
            )
        {
            if (purged.Contains(item))
            {
                return;
            }
            purged.Add(item);
            Client().Purge(item);
            var child = item.child;
            if (null != child)
            {
                Purge(purged, child);
            }
        }

        private sealed class _ICodeBlock_211 : ICodeBlock
        {
            private readonly IObjectSet result;

            public _ICodeBlock_211(IObjectSet result)
            {
                this.result = result;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                result.Next();
            }
        }

        private class Depth2Stimulus : Stimulus
        {
            private readonly PrefetchConfigurationTestCase _enclosing;

            public Depth2Stimulus(PrefetchConfigurationTestCase _enclosing, MsgD[] expectedMessagesAfter
                ) : base(expectedMessagesAfter)
            {
                this._enclosing = _enclosing;
            }

            public override void ActUpon(IObjectSet result)
            {
                ActUpon(((Item) result.Next()));
            }

            protected virtual void ActUpon(Item item)
            {
                Assert.IsNotNull(item.child);
                _enclosing.Db().Activate(item.child, 1);
            }
        }

        public class Stimulus
        {
            public readonly MsgD[] expectedMessagesAfter;

            public Stimulus(MsgD[] expectedMessagesAfter)
            {
                this.expectedMessagesAfter = expectedMessagesAfter;
            }

            public virtual void ActUpon(IObjectSet result)
            {
                Assert.IsNotNull(((Item) result.Next()));
            }
        }

        public class Item
        {
            public Item child;
            public int order;

            public Item(Item child)
            {
                this.child = child;
            }

            public Item()
            {
            }

            public Item(int order)
            {
                this.order = order;
            }
        }

        public class RootItem : Item
        {
            public RootItem()
            {
            }

            public RootItem(Item child) : base(child)
            {
            }
        }
    }
}

#endif // !SILVERLIGHT