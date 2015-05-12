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
using Db4objects.Db4o.CS;
using Db4objects.Db4o.CS.Config;
using Db4objects.Db4o.IO;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.CS.Config
{
    public class ServerConfigurationItemIntegrationTestCase : ITestCase
    {
        public virtual void Test()
        {
            var config = Db4oClientServer.NewServerConfiguration();
            config.File.Storage = new MemoryStorage();
            var item = new DummyConfigurationItem
                (this);
            config.AddConfigurationItem(item);
            var server = Db4oClientServer.OpenServer(config, string.Empty, Db4oClientServer
                .ArbitraryPort);
            item.Verify(config, server);
            server.Close();
        }

        private sealed class DummyConfigurationItem : IServerConfigurationItem
        {
            private readonly ServerConfigurationItemIntegrationTestCase _enclosing;
            private int _applyCount;
            private IServerConfiguration _config;
            private int _prepareCount;
            private IObjectServer _server;

            internal DummyConfigurationItem(ServerConfigurationItemIntegrationTestCase _enclosing
                )
            {
                this._enclosing = _enclosing;
            }

            public void Prepare(IServerConfiguration configuration)
            {
                _config = configuration;
                _prepareCount++;
            }

            public void Apply(IObjectServer server)
            {
                _server = server;
                _applyCount++;
            }

            internal void Verify(IServerConfiguration config, IObjectServer server)
            {
                Assert.AreSame(config, _config);
                Assert.AreSame(server, _server);
                Assert.AreEqual(1, _prepareCount);
                Assert.AreEqual(1, _applyCount);
            }
        }
    }
}

#endif // !SILVERLIGHT