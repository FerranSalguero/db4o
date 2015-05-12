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
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Api
{
    public class Db4oClientServerTestCase : TestWithTempFile
    {
        public virtual void TestClientServerApi()
        {
            var config = Db4oClientServer.NewServerConfiguration();
            var server = Db4oClientServer.OpenServer(config, TempFile(), unchecked(
                0xdb40));
            try
            {
                server.GrantAccess("user", "password");
                var clientConfig = Db4oClientServer.NewClientConfiguration();
                var client1 = Db4oClientServer.OpenClient(clientConfig, "localhost",
                    unchecked(0xdb40), "user", "password");
                try
                {
                }
                finally
                {
                    Assert.IsTrue(client1.Close());
                }
            }
            finally
            {
                Assert.IsTrue(server.Close());
            }
        }

        public virtual void TestConfigurationHierarchy()
        {
            Assert.IsInstanceOf(typeof (INetworkingConfigurationProvider), Db4oClientServer.NewClientConfiguration
                ());
            Assert.IsInstanceOf(typeof (INetworkingConfigurationProvider), Db4oClientServer.NewServerConfiguration
                ());
        }
    }
}

#endif // !SILVERLIGHT