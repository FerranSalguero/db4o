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
using Db4objects.Db4o.CS.Internal.Config;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Staging
{
    /// <summary>COR-1762</summary>
    public class DeepPrefetchingCacheConcurrencyTestCase : AbstractDb4oTestCase, IOptOutAllButNetworkingCS
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            var clientConfiguration = Db4oClientServerLegacyConfigurationBridge
                .AsClientConfiguration(config);
            clientConfiguration.PrefetchDepth = 3;
            clientConfiguration.PrefetchObjectCount = 3;
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            for (var i = 0; i < 2; i++)
            {
                var item = new Item
                    ("original");
                Store(item);
            }
        }

        public virtual void Test()
        {
            var ids = new int[2];
            var originalResult = NewQuery(typeof (Item
                )).Execute();
            var firstOriginalItem = ((Item
                ) originalResult.Next());
            Db().Purge(firstOriginalItem);
            var otherClient = OpenNewSession();
            var updateResult = otherClient.Query(typeof (Item
                ));
            var idx = 0;
            for (var updateItemIter = updateResult.GetEnumerator();
                updateItemIter.MoveNext
                    ();)
            {
                var updateItem = ((Item
                    ) updateItemIter.Current);
                ids[idx] = (int) otherClient.GetID(updateItem);
                updateItem._name = "updated";
                otherClient.Store(updateItem);
                idx++;
            }
            otherClient.Commit();
            otherClient.Close();
            for (var i = 0; i < ids.Length; i++)
            {
                var checkItem = ((Item
                    ) Db().GetByID(ids[i]));
                Db().Activate(checkItem);
                Assert.AreEqual("updated", checkItem._name);
            }
        }

        public class Item
        {
            public string _name;

            public Item(string name)
            {
                _name = name;
            }
        }
    }
}

#endif // !SILVERLIGHT