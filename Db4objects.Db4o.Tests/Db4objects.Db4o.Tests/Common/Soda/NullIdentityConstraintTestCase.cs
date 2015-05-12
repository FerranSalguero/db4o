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

using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Soda
{
    public class NullIdentityConstraintTestCase : AbstractDb4oTestCase
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            var a = new Data(null
                );
            var b = new Data(a
                );
            Store(b);
        }

        public virtual void TestNullIdentity()
        {
            var query = NewQuery(typeof (Data));
            query.Descend("_prev").Constrain(null).Identity();
            Assert.AreEqual(1, query.Execute().Count);
        }

        public class Data
        {
            public Data _prev;

            public Data(Data prev)
            {
                _prev = prev;
            }
        }
    }
}