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
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.IO;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.CS.Config
{
    public class ClientConfigurationItemIntegrationTestCase : ITestCase
    {
        private static readonly string Password = "db4o";
        private static readonly string User = "db4o";

        public virtual void Test()
        {
            var serverConfig = Db4oClientServer.NewServerConfiguration();
            serverConfig.File.Storage = new MemoryStorage();
            var server = Db4oClientServer.OpenServer(serverConfig, string.Empty, Db4oClientServer
                .ArbitraryPort);
            server.GrantAccess(User, Password);
            var clientConfig = Db4oClientServer.NewClientConfiguration();
            var item = new DummyConfigurationItem
                (this);
            clientConfig.AddConfigurationItem(item);
            var client = (IExtClient) Db4oClientServer.OpenClient(clientConfig, "localhost"
                , server.Ext().Port(), User, Password);
            item.Verify(clientConfig, client);
            client.Close();
            server.Close();
        }

        private sealed class DummyConfigurationItem : IClientConfigurationItem
        {
            private readonly ClientConfigurationItemIntegrationTestCase _enclosing;
            private int _applyCount;
            private IExtClient _client;
            private IClientConfiguration _config;
            private int _prepareCount;

            internal DummyConfigurationItem(ClientConfigurationItemIntegrationTestCase _enclosing
                )
            {
                this._enclosing = _enclosing;
            }

            public void Prepare(IClientConfiguration configuration)
            {
                _config = configuration;
                _prepareCount++;
            }

            public void Apply(IExtClient client)
            {
                _client = client;
                _applyCount++;
            }

            internal void Verify(IClientConfiguration config, IExtClient client)
            {
                Assert.AreSame(config, _config);
                Assert.AreSame(client, _client);
                Assert.AreEqual(1, _prepareCount);
                Assert.AreEqual(1, _applyCount);
            }
        }
    }
}

#endif // !SILVERLIGHT