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
using Db4objects.Db4o.CS.Caching;
using Db4objects.Db4o.CS.Internal.Config;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.CS.Caching
{
    /// <summary>
    ///     removed for JDK 1.1 because there is no access to the private field
    ///     _clientSlotCache in ClientObjectContainer
    /// </summary>
    public class ClientSlotCacheTestCase : AbstractDb4oTestCase, IOptOutAllButNetworkingCS
    {
        private const int SlotCacheSize = 5;

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            var clientConfiguration = Db4oClientServerLegacyConfigurationBridge
                .AsClientConfiguration(config);
            clientConfiguration.PrefetchSlotCacheSize = SlotCacheSize;
        }

        public virtual void TestSlotCacheIsTransactionBased()
        {
            WithCache(new _IProcedure4_29(this));
        }

        public virtual void TestCacheIsCleanUponTransactionCommit()
        {
            AssertCacheIsCleanAfterTransactionOperation(new _IProcedure4_48());
        }

        public virtual void TestCacheIsCleanUponTransactionRollback()
        {
            AssertCacheIsCleanAfterTransactionOperation(new _IProcedure4_56());
        }

        private void AssertCacheIsCleanAfterTransactionOperation(IProcedure4 operation)
        {
            WithCache(new _IProcedure4_64(this, operation));
        }

        public virtual void TestSlotCacheEntryIsPurgedUponActivation()
        {
            var item = new Item();
            Db().Store(item);
            var id = (int) Db().GetID(item);
            Db().Purge(item);
            Db().Configure().ClientServer().PrefetchDepth(1);
            WithCache(new _IProcedure4_83(this, id));
        }

        public virtual void TestAddOverridesExistingEntry()
        {
            WithCache(new _IProcedure4_94(this));
        }

        public virtual void TestCacheSizeIsBounded()
        {
            WithCache(new _IProcedure4_104(this));
        }

        private void WithCache(IProcedure4 procedure)
        {
            IClientSlotCache clientSlotCache = null;
            try
            {
                clientSlotCache = (IClientSlotCache) Reflection4.GetFieldValue(Container(), "_clientSlotCache"
                    );
            }
            catch (ReflectException e)
            {
                Assert.Fail("Can't get field _clientSlotCache on  container. " + e);
            }
            procedure.Apply(clientSlotCache);
        }

        private sealed class _IProcedure4_29 : IProcedure4
        {
            private readonly ClientSlotCacheTestCase _enclosing;

            public _IProcedure4_29(ClientSlotCacheTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Apply(object cache)
            {
                var t1 = _enclosing.NewTransaction();
                var t2 = _enclosing.NewTransaction();
                var slot = new ByteArrayBuffer(0);
                ((IClientSlotCache) cache).Add(t1, 42, slot);
                Assert.AreSame(slot, ((IClientSlotCache) cache).Get(t1, 42));
                Assert.IsNull(((IClientSlotCache) cache).Get(t2, 42));
                lock (t1.Container().Lock())
                {
                    t1.Commit();
                }
                Assert.IsNull(((IClientSlotCache) cache).Get(t1, 42));
            }
        }

        private sealed class _IProcedure4_48 : IProcedure4
        {
            public void Apply(object value)
            {
                ((Transaction) value).Commit();
            }
        }

        private sealed class _IProcedure4_56 : IProcedure4
        {
            public void Apply(object value)
            {
                ((Transaction) value).Rollback();
            }
        }

        private sealed class _IProcedure4_64 : IProcedure4
        {
            private readonly ClientSlotCacheTestCase _enclosing;
            private readonly IProcedure4 operation;

            public _IProcedure4_64(ClientSlotCacheTestCase _enclosing, IProcedure4 operation)
            {
                this._enclosing = _enclosing;
                this.operation = operation;
            }

            public void Apply(object cache)
            {
                var slot = new ByteArrayBuffer(0);
                ((IClientSlotCache) cache).Add(_enclosing.Trans(), 42, slot);
                operation.Apply(_enclosing.Trans());
                Assert.IsNull(((IClientSlotCache) cache).Get(_enclosing.Trans(), 42));
            }
        }

        private sealed class _IProcedure4_83 : IProcedure4
        {
            private readonly ClientSlotCacheTestCase _enclosing;
            private readonly int id;

            public _IProcedure4_83(ClientSlotCacheTestCase _enclosing, int id)
            {
                this._enclosing = _enclosing;
                this.id = id;
            }

            public void Apply(object cache)
            {
                var items = _enclosing.NewQuery(typeof (Item))
                    .Execute();
                Assert.IsNotNull(((IClientSlotCache) cache).Get(_enclosing.Trans(), id));
                Assert.IsNotNull(((Item) items.Next()));
                Assert.IsNull(((IClientSlotCache) cache).Get(_enclosing.Trans(), id),
                    "activation should have purged slot from cache"
                    );
            }
        }

        private sealed class _IProcedure4_94 : IProcedure4
        {
            private readonly ClientSlotCacheTestCase _enclosing;

            public _IProcedure4_94(ClientSlotCacheTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Apply(object cache)
            {
                ((IClientSlotCache) cache).Add(_enclosing.Trans(), 42, new ByteArrayBuffer(0)
                    );
                ((IClientSlotCache) cache).Add(_enclosing.Trans(), 42, new ByteArrayBuffer(1)
                    );
                Assert.AreEqual(1, ((IClientSlotCache) cache).Get(_enclosing.Trans(), 42).Length
                    ());
            }
        }

        private sealed class _IProcedure4_104 : IProcedure4
        {
            private readonly ClientSlotCacheTestCase _enclosing;

            public _IProcedure4_104(ClientSlotCacheTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Apply(object cache)
            {
                for (var i = 0; i < SlotCacheSize + 1; i++)
                {
                    ((IClientSlotCache) cache).Add(_enclosing.Trans(), i, new ByteArrayBuffer(i));
                }
                for (var i = 1; i < SlotCacheSize + 1; i++)
                {
                    Assert.AreEqual(i, ((IClientSlotCache) cache).Get(_enclosing.Trans(), i).Length
                        ());
                }
                Assert.IsNull(((IClientSlotCache) cache).Get(_enclosing.Trans(), 0));
            }
        }

        public class Item
        {
        }
    }
}

#endif // !SILVERLIGHT