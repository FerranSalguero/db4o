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
using Db4objects.Db4o.IO;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Config
{
    public class EmbeddedConfigurationItemIntegrationTestCase : ITestCase
    {
        public virtual void Test()
        {
            var config = Db4oEmbedded.NewConfiguration();
            config.File.Storage = new MemoryStorage();
            var item = new DummyConfigurationItem
                (this);
            config.AddConfigurationItem(item);
            var container = Db4oEmbedded.OpenFile(config, string.Empty);
            item.Verify(config, container);
            container.Close();
        }

        private sealed class DummyConfigurationItem : IEmbeddedConfigurationItem
        {
            private readonly EmbeddedConfigurationItemIntegrationTestCase _enclosing;
            private int _applyCount;
            private IEmbeddedConfiguration _config;
            private IEmbeddedObjectContainer _container;
            private int _prepareCount;

            internal DummyConfigurationItem(EmbeddedConfigurationItemIntegrationTestCase _enclosing
                )
            {
                this._enclosing = _enclosing;
            }

            public void Prepare(IEmbeddedConfiguration configuration)
            {
                _config = configuration;
                _prepareCount++;
            }

            public void Apply(IEmbeddedObjectContainer container)
            {
                _container = container;
                _applyCount++;
            }

            internal void Verify(IEmbeddedConfiguration config, IEmbeddedObjectContainer container
                )
            {
                Assert.AreSame(config, _config);
                Assert.AreSame(container, _container);
                Assert.AreEqual(1, _prepareCount);
                Assert.AreEqual(1, _applyCount);
            }
        }
    }
}