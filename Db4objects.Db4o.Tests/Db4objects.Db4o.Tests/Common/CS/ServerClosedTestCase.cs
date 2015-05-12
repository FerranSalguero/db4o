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
using Db4objects.Db4o.CS.Internal;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.CS
{
    public class ServerClosedTestCase : Db4oClientServerTestCase, IOptOutAllButNetworkingCS
    {
        public static void Main(string[] args)
        {
            new ServerClosedTestCase().RunAll();
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void Test()
        {
            var db = Fixture().Db();
            var serverImpl = (ObjectServerImpl) ClientServerFixture().Server();
            try
            {
                var iter = serverImpl.IterateDispatchers();
                iter.MoveNext();
                var serverDispatcher = (ServerMessageDispatcherImpl) iter.
                    Current;
                serverDispatcher.Socket().Close();
                Runtime4.Sleep(1000);
                Assert.Expect(typeof (DatabaseClosedException), new _ICodeBlock_30(db));
                Assert.IsTrue(db.IsClosed());
            }
            finally
            {
                serverImpl.Close();
            }
        }

        private sealed class _ICodeBlock_30 : ICodeBlock
        {
            private readonly IExtObjectContainer db;

            public _ICodeBlock_30(IExtObjectContainer db)
            {
                this.db = db;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                db.QueryByExample(null);
            }
        }
    }
}

#endif // !SILVERLIGHT