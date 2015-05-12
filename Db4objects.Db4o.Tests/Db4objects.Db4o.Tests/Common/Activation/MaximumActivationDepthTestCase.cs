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
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Activation
{
    public class MaximumActivationDepthTestCase : AbstractDb4oTestCase, IOptOutTA
    {
        protected override void Configure(IConfiguration config)
        {
            config.ActivationDepth(int.MaxValue);
            config.ObjectClass(typeof (Data)).MaximumActivationDepth
                (1);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            var data = new Data
                (2, null);
            data = new Data(1, data);
            data = new Data(0, data);
            Store(data);
        }

        public virtual void TestActivationRestricted()
        {
            var query = NewQuery(typeof (Data));
            query.Descend("_id").Constrain(0);
            var result = query.Execute();
            Assert.AreEqual(1, result.Count);
            var data = (Data) result
                .Next();
            Assert.IsNotNull(data._prev);
            Assert.IsNull(data._prev._prev);
        }

        public class Data
        {
            public int _id;
            public Data _prev;

            public Data(int id, Data prev)
            {
                _id = id;
                _prev = prev;
            }
        }
    }
}