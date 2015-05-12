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
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Query;
using Db4oUnit;
using Db4oUnit.Extensions.Fixtures;
using Db4oUnit.Fixtures;

namespace Db4objects.Db4o.Tests.Common.CS
{
    /// <summary>
    ///     Options:
    ///     1) activate the objects on the server up to prefetchDepth and store them into
    ///     the TransportObjectContainer 1.1) connect objects to the local client cache
    ///     2) activate the objects on the server up to prefetchDepth, collect all IDs
    ///     and send the required slots to the client 2.1) connect the objects to the
    ///     local client cache
    ///     2') don't activate the objects but traverse slots collecting IDs instead
    ///     3) Introduce slot cache in the client and prefetch slots every time objects
    ///     are activated and the required slots (prefetchDepth) are not available
    /// </summary>
    public class BatchActivationTestCase : FixtureTestSuiteDescription, IOptOutAllButNetworkingCS
    {
        public BatchActivationTestCase()
        {
            {
                TestUnits(new[] {typeof (BatchActivationTestUnit)});
                FixtureProviders(new IFixtureProvider[]
                {
                    new SubjectFixtureProvider(new[]
                    {
                        Pair.Of(0, 2), Pair.Of(1, 0)
                    })
                });
            }
        }

        public class BatchActivationTestUnit : ClientServerTestCaseBase
        {
            // first - prefetchDepth
            // second - expected number of messages exchanged
            /// <exception cref="System.Exception"></exception>
            protected override void Configure(IConfiguration config)
            {
                config.ClientServer().PrefetchDepth(PrefetchDepth());
            }

            /// <exception cref="System.Exception"></exception>
            protected override void Store()
            {
                Store(new Item("foo"));
            }

            public virtual void TestClassOnlyQuery()
            {
                var query = NewQuery(typeof (Item
                    ));
                AssertBatchBehaviorFor(query);
            }

            public virtual void TestConstrainedQuery()
            {
                var query = NewConstrainedQuery();
                AssertBatchBehaviorFor(query);
            }

            public virtual void TestQueryPrefetchDepth0()
            {
                var query = NewConstrainedQuery();
                Client().Config().ClientServer().PrefetchDepth(0);
                AssertBatchBehaviorFor(query, 2);
            }

            public virtual void TestQueryPrefetchDepth1()
            {
                var query = NewConstrainedQuery();
                Client().Config().ClientServer().PrefetchDepth(1);
                AssertBatchBehaviorFor(query, 0);
            }

            public virtual void TestQueryPrefetchDepth0ForClassOnlyQuery()
            {
                var query = NewQuery(typeof (Item
                    ));
                Client().Config().ClientServer().PrefetchDepth(0);
                AssertBatchBehaviorFor(query, 2);
            }

            public virtual void TestQueryPrefetchDepth1ForClassOnlyQuery()
            {
                var query = NewQuery(typeof (Item
                    ));
                Client().Config().ClientServer().PrefetchDepth(1);
                AssertBatchBehaviorFor(query, 0);
            }

            private IQuery NewConstrainedQuery()
            {
                var query = NewQuery(typeof (Item
                    ));
                query.Descend("name").Constrain("foo");
                return query;
            }

            private void AssertBatchBehaviorFor(IQuery query)
            {
                AssertBatchBehaviorFor(query, ExpectedMessageCount());
            }

            private void AssertBatchBehaviorFor(IQuery query, int expectedMessageCount)
            {
                var result = query.Execute();
                var messages = MessageCollector.ForServerDispatcher(ServerDispatcher());
                Assert.AreEqual("foo", ((Item) result
                    .Next()).name);
                Assert.AreEqual(expectedMessageCount, messages.Count, messages.ToString());
            }

            private int PrefetchDepth()
            {
                return (((int) Subject().first));
            }

            private Pair Subject()
            {
                return ((Pair) SubjectFixtureProvider.Value());
            }

            private int ExpectedMessageCount()
            {
                return (((int) Subject().second));
            }

            public class Item
            {
                public string name;

                public Item(string name)
                {
                    this.name = name;
                }
            }
        }
    }
}

#endif // !SILVERLIGHT