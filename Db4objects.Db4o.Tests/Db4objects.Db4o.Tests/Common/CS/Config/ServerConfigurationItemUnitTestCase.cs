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
using Db4objects.Db4o.CS.Config;
using Db4objects.Db4o.CS.Internal.Config;
using Db4oUnit;
using Db4oUnit.Extensions.Dbmock;
using Sharpen.Util;

namespace Db4objects.Db4o.Tests.Common.CS.Config
{
    public class ServerConfigurationItemUnitTestCase : ITestLifeCycle
    {
        private IList _applied;
        private ServerConfigurationImpl _config;

        /// <exception cref="System.Exception"></exception>
        public virtual void SetUp()
        {
            _applied = new ArrayList();
            _config = (ServerConfigurationImpl) Db4oClientServer.NewServerConfiguration();
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TearDown()
        {
        }

        public virtual void TestPrepareApply()
        {
            IList items = Arrays.AsList(new[]
            {
                new DummyConfigurationItem(_applied), new
                    DummyConfigurationItem(_applied)
            });
            for (var itemIter = items.GetEnumerator(); itemIter.MoveNext();)
            {
                var item = ((DummyConfigurationItem
                    ) itemIter.Current);
                _config.AddConfigurationItem(item);
                Assert.AreEqual(1, item.PrepareCount());
            }
            Assert.AreEqual(0, _applied.Count);
            _config.ApplyConfigurationItems(new MockServer());
            AssertListsAreEqual(items, _applied);
            for (var itemIter = items.GetEnumerator(); itemIter.MoveNext();)
            {
                var item = ((DummyConfigurationItem
                    ) itemIter.Current);
                Assert.AreEqual(1, item.PrepareCount());
            }
        }

        public virtual void TestAddTwice()
        {
            var item = new DummyConfigurationItem
                (_applied);
            _config.AddConfigurationItem(item);
            _config.AddConfigurationItem(item);
            _config.ApplyConfigurationItems(new MockServer());
            Assert.AreEqual(1, item.PrepareCount());
            AssertListsAreEqual(Arrays.AsList(new[] {item}), _applied);
        }

        private void AssertListsAreEqual(IList a, IList b)
        {
            Assert.AreEqual(a.Count, b.Count);
            for (var i = 0; i < a.Count; i++)
            {
                Assert.AreEqual(a[i], b[i]);
            }
        }

        private class DummyConfigurationItem : IServerConfigurationItem
        {
            private readonly IList _applied;
            private int _prepareCount;

            public DummyConfigurationItem(IList applied)
            {
                _applied = applied;
            }

            public virtual void Apply(IObjectServer server)
            {
                _applied.Add(this);
            }

            public virtual void Prepare(IServerConfiguration configuration)
            {
                _prepareCount++;
            }

            public virtual int PrepareCount()
            {
                return _prepareCount;
            }
        }
    }
}

#endif // !SILVERLIGHT