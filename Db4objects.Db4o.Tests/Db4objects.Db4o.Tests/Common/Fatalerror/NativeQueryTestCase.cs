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

using System;
using Db4objects.Db4o.Query;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Fatalerror
{
    public class NativeQueryTestCase : AbstractDb4oTestCase
    {
        public static void Main(string[] args)
        {
            new NativeQueryTestCase().RunSoloAndClientServer();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(new Item("hello"));
        }

        public virtual void _test()
        {
            Assert.Expect(typeof (NQError), new _ICodeBlock_29(this));
            Assert.IsTrue(Db().IsClosed());
        }

        public class Item
        {
            public string str;

            public Item(string s)
            {
                str = s;
            }
        }

        private sealed class _ICodeBlock_29 : ICodeBlock
        {
            private readonly NativeQueryTestCase _enclosing;

            public _ICodeBlock_29(NativeQueryTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                Predicate fatalErrorPredicate = new FatalErrorPredicate();
                _enclosing.Db().Query(fatalErrorPredicate);
            }
        }

        [Serializable]
        public class FatalErrorPredicate : Predicate
        {
            public virtual bool Match(object item)
            {
                throw new NQError("nq error!");
            }
        }

        [Serializable]
        public class NQError : Exception
        {
            public NQError(string msg) : base(msg)
            {
            }
        }
    }
}