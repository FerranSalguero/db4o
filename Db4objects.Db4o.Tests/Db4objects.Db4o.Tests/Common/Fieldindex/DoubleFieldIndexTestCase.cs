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
using Db4objects.Db4o.Config;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Fieldindex
{
    /// <exclude></exclude>
    public class DoubleFieldIndexTestCase : AbstractDb4oTestCase
    {
        public static void Main(string[] args)
        {
            new DoubleFieldIndexTestCase().RunSolo();
        }

        protected override void Configure(IConfiguration config)
        {
            IndexField(config, typeof (Item), "value");
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Db().Store(new Item(0.5));
            Db().Store(new Item(1.1));
            Db().Store(new Item(2));
        }

        public virtual void TestEqual()
        {
            var query = NewQuery(typeof (Item));
            query.Descend("value").Constrain(1.1);
            AssertItems(new[] {1.1}, query.Execute());
        }

        public virtual void TestGreater()
        {
            var query = NewQuery(typeof (Item));
            var descend = query.Descend("value");
            descend.Constrain(Convert.ToDouble(1)).Greater();
            descend.OrderAscending();
            AssertItems(new[] {1.1, 2}, query.Execute());
        }

        private void AssertItems(double[] expected, IObjectSet set)
        {
            ArrayAssert.AreEqual(expected, ToDoubleArray(set));
        }

        private double[] ToDoubleArray(IObjectSet set)
        {
            var array = new double[set.Count];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = ((Item) set.Next()).value;
            }
            return array;
        }

        public class Item
        {
            public double value;

            public Item()
            {
            }

            public Item(double value_)
            {
                value = value_;
            }
        }
    }
}