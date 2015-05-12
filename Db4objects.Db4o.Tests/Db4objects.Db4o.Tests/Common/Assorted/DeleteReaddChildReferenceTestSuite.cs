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

using Db4objects.Db4o.Config;
using Db4objects.Db4o.Ext;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;
using Db4oUnit.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Assorted
{
    /// <summary>
    ///     COR-1539  Readding a deleted object from a different client changes database ID in embedded mode
    /// </summary>
    public class DeleteReaddChildReferenceTestSuite : FixtureTestSuiteDescription, IDb4oTestCase
    {
        public DeleteReaddChildReferenceTestSuite()
        {
            {
                FixtureProviders(new IFixtureProvider[]
                {
                    new SubjectFixtureProvider(new[]
                    {
                        true, false
                    }),
                    new Db4oFixtureProvider()
                });
                TestUnits(new[]
                {
                    typeof (DeleteReaddChildReferenceTestUnit
                        )
                });
            }
        }

        public class DeleteReaddChildReferenceTestUnit : Db4oClientServerTestCase
        {
            private static readonly string ItemName = "child";
            private IExtObjectContainer client1;
            private IExtObjectContainer client2;

            /// <exception cref="System.Exception"></exception>
            protected override void Configure(IConfiguration config)
            {
                if (!(UseIndices()))
                {
                    return;
                }
                IndexField(config, typeof (ItemParent
                    ), ItemName);
                IndexField(config, typeof (Item
                    ), "name");
            }

            private bool UseIndices()
            {
                return ((bool) SubjectFixtureProvider.Value());
            }

            /// <exception cref="System.Exception"></exception>
            protected override void Store()
            {
                var child =
                    new Item(ItemName
                        );
                var parent
                    = new ItemParent
                        ();
                parent.child = child;
                Store(parent);
            }

            public virtual void TestDeleteReaddFromOtherClient()
            {
                if (!PrepareTest())
                {
                    return;
                }
                var parent1
                    = ((ItemParent
                        ) RetrieveOnlyInstance(client1, typeof (ItemParent
                            )));
                var parent2
                    = ((ItemParent
                        ) RetrieveOnlyInstance(client2, typeof (ItemParent
                            )));
                client1.Delete(parent1.child);
                AssertQueries(0, 1);
                client1.Commit();
                AssertQueries(0, 0);
                client2.Store(parent2.child);
                AssertQueries(0, 1);
                client2.Commit();
                AssertQueries(1, 1);
                client2.Close();
                AssertRestoredState();
            }

            public virtual void TestDeleteReaddTwiceFromOtherClient()
            {
                if (!PrepareTest())
                {
                    return;
                }
                var parent1
                    = ((ItemParent
                        ) RetrieveOnlyInstance(client1, typeof (ItemParent
                            )));
                var parent2
                    = ((ItemParent
                        ) RetrieveOnlyInstance(client2, typeof (ItemParent
                            )));
                client1.Delete(parent1.child);
                AssertQueries(0, 1);
                client1.Commit();
                AssertQueries(0, 0);
                client2.Store(parent2.child);
                AssertQueries(0, 1);
                client2.Store(parent2.child);
                AssertQueries(0, 1);
                client2.Commit();
                AssertQueries(1, 1);
                client2.Close();
                AssertRestoredState();
            }

            public virtual void TestDeleteReaddFromBoth()
            {
                if (!PrepareTest())
                {
                    return;
                }
                var parent1
                    = ((ItemParent
                        ) RetrieveOnlyInstance(client1, typeof (ItemParent
                            )));
                var parent2
                    = ((ItemParent
                        ) RetrieveOnlyInstance(client2, typeof (ItemParent
                            )));
                client1.Delete(parent1.child);
                AssertQueries(0, 1);
                client2.Delete(parent2.child);
                AssertQueries(0, 0);
                client1.Store(parent1.child);
                AssertQueries(1, 0);
                client2.Store(parent2.child);
                AssertQueries(1, 1);
                client1.Commit();
                AssertQueries(1, 1);
                client2.Commit();
                AssertQueries(1, 1);
                client2.Close();
                AssertRestoredState();
            }

            private void AssertRestoredState()
            {
                var parent3
                    = ((ItemParent
                        ) RetrieveOnlyInstance(client1, typeof (ItemParent
                            )));
                Db().Refresh(parent3, int.MaxValue);
                Assert.IsNotNull(parent3);
                Assert.IsNotNull(parent3.child);
            }

            private void AssertQueries(int exp1, int exp2)
            {
                AssertQuery(exp1, client1);
                AssertQuery(exp2, client2);
            }

            private bool PrepareTest()
            {
                if (!IsMultiSession())
                {
                    return false;
                }
                client1 = Db();
                client2 = OpenNewSession();
                return true;
            }

            private void AssertQuery(int expectedCount, IExtObjectContainer queryClient)
            {
                AssertChildClassOnlyQuery(expectedCount, queryClient);
                AssertParentChildQuery(expectedCount, queryClient);
                AssertChildQuery(expectedCount, queryClient);
            }

            private void AssertParentChildQuery(int expectedCount, IExtObjectContainer queryClient
                )
            {
                var query = queryClient.Query();
                query.Constrain(typeof (ItemParent
                    ));
                query.Descend("child").Descend("name").Constrain(ItemName);
                Assert.AreEqual(expectedCount, query.Execute().Count);
            }

            private void AssertChildQuery(int expectedCount, IExtObjectContainer queryClient)
            {
                var query = queryClient.Query();
                query.Constrain(typeof (Item
                    ));
                query.Descend("name").Constrain(ItemName);
                Assert.AreEqual(expectedCount, query.Execute().Count);
            }

            private void AssertChildClassOnlyQuery(int expectedCount, IExtObjectContainer queryClient
                )
            {
                var result = queryClient.Query(typeof (Item
                    ));
                Assert.AreEqual(expectedCount, result.Count);
            }

            public static void Main(string[] arguments)
            {
                new DeleteReaddChildReferenceTestUnit().RunAll
                    ();
            }

            public class ItemParent
            {
                public Item
                    child;
            }

            public class Item
            {
                public string name;

                public Item(string name_)
                {
                    name = name_;
                }
            }
        }
    }
}