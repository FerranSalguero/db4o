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

namespace Db4objects.Db4o.Tests.Common.Soda.Ordered
{
    public class TopLevelOrderExceptionTestCase : AbstractDb4oTestCase
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(new Item());
            Store(new Item());
        }

        public virtual void TestDescending()
        {
            var query = NewQuery(typeof (Item));
            Assert.Expect(typeof (InvalidOperationException), new _ICodeBlock_20(query));
        }

        public virtual void TestAscending()
        {
            var query = NewQuery(typeof (Item));
            Assert.Expect(typeof (InvalidOperationException), new _ICodeBlock_29(query));
        }

        public class Item
        {
        }

        private sealed class _ICodeBlock_20 : ICodeBlock
        {
            private readonly IQuery query;

            public _ICodeBlock_20(IQuery query)
            {
                this.query = query;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                query.OrderDescending();
            }
        }

        private sealed class _ICodeBlock_29 : ICodeBlock
        {
            private readonly IQuery query;

            public _ICodeBlock_29(IQuery query)
            {
                this.query = query;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                query.OrderAscending();
            }
        }
    }
}