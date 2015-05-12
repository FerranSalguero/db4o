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
using Db4objects.Db4o.Ext;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Exceptions
{
    public class InvalidPasswordTestCase : Db4oClientServerTestCase, IOptOutAllButNetworkingCS
    {
        public virtual void TestInvalidPassword()
        {
            var port = ClientServerFixture().ServerPort();
            Assert.Expect(typeof (InvalidPasswordException), new _ICodeBlock_20(this, port));
        }

        protected virtual IObjectContainer OpenClient(string host, int port, string user,
            string password)
        {
            return Db4oClientServer.OpenClient(host, port, user, password);
        }

        public virtual void TestEmptyUserPassword()
        {
            var port = ClientServerFixture().ServerPort();
            Assert.Expect(typeof (InvalidPasswordException), new _ICodeBlock_35(this, port));
        }

        public virtual void TestEmptyUserNullPassword()
        {
            var port = ClientServerFixture().ServerPort();
            Assert.Expect(typeof (InvalidPasswordException), new _ICodeBlock_44(this, port));
        }

        public virtual void TestNullPassword()
        {
            var port = ClientServerFixture().ServerPort();
            Assert.Expect(typeof (InvalidPasswordException), new _ICodeBlock_53(this, port));
        }

        private sealed class _ICodeBlock_20 : ICodeBlock
        {
            private readonly InvalidPasswordTestCase _enclosing;
            private readonly int port;

            public _ICodeBlock_20(InvalidPasswordTestCase _enclosing, int port)
            {
                this._enclosing = _enclosing;
                this.port = port;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.OpenClient("127.0.0.1", port, "strangeusername", "invalidPassword"
                    );
            }
        }

        private sealed class _ICodeBlock_35 : ICodeBlock
        {
            private readonly InvalidPasswordTestCase _enclosing;
            private readonly int port;

            public _ICodeBlock_35(InvalidPasswordTestCase _enclosing, int port)
            {
                this._enclosing = _enclosing;
                this.port = port;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.OpenClient("127.0.0.1", port, string.Empty, string.Empty);
            }
        }

        private sealed class _ICodeBlock_44 : ICodeBlock
        {
            private readonly InvalidPasswordTestCase _enclosing;
            private readonly int port;

            public _ICodeBlock_44(InvalidPasswordTestCase _enclosing, int port)
            {
                this._enclosing = _enclosing;
                this.port = port;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.OpenClient("127.0.0.1", port, string.Empty, null);
            }
        }

        private sealed class _ICodeBlock_53 : ICodeBlock
        {
            private readonly InvalidPasswordTestCase _enclosing;
            private readonly int port;

            public _ICodeBlock_53(InvalidPasswordTestCase _enclosing, int port)
            {
                this._enclosing = _enclosing;
                this.port = port;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.OpenClient("127.0.0.1", port, null, null);
            }
        }
    }
}

#endif // !SILVERLIGHT