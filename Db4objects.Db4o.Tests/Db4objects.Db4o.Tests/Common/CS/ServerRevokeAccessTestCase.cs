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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.CS;
using Db4objects.Db4o.CS.Internal.Config;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.CS
{
    public class ServerRevokeAccessTestCase : Db4oClientServerTestCase, IOptOutAllButNetworkingCS
    {
        private static readonly string ServerHostname = "127.0.0.1";

        public static void Main(string[] args)
        {
            new ServerRevokeAccessTestCase().RunAll();
        }

#if !CF
        /// <exception cref="System.IO.IOException"></exception>
        public virtual void Test()
        {
            var user = "hohohi";
            var password = "hohoho";
            var server = ClientServerFixture().Server();
            server.GrantAccess(user, password);
            var con = OpenClient(user, password);
            Assert.IsNotNull(con);
            con.Close();
            server.Ext().RevokeAccess(user);
            Assert.Expect(typeof (Exception), new _ICodeBlock_40(this, user, password));
        }
#endif // !CF

        private IObjectContainer OpenClient(string user, string password)
        {
            return Db4oClientServer.OpenClient(Db4oClientServerLegacyConfigurationBridge.AsClientConfiguration
                (Config()), ServerHostname, ClientServerFixture().ServerPort(), user, password);
        }

        private IConfiguration Config()
        {
            return ClientServerFixture().Config();
        }

        private sealed class _ICodeBlock_40 : ICodeBlock
        {
            private readonly ServerRevokeAccessTestCase _enclosing;
            private readonly string password;
            private readonly string user;

            public _ICodeBlock_40(ServerRevokeAccessTestCase _enclosing, string user, string
                password)
            {
                this._enclosing = _enclosing;
                this.user = user;
                this.password = password;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.OpenClient(user, password);
            }
        }
    }
}

#endif // !SILVERLIGHT