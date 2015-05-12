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

namespace Db4objects.Db4o.Tests.Common.Querying
{
    public class SameChildOnDifferentParentQueryTestCase : AbstractDb4oTestCase
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            var unique = new Item
                ("unique");
            var shared = new Item
                ("shared");
            Store(new Holder(shared));
            Store(new Holder(unique));
            Store(new Holder(shared));
        }

        public virtual void TestUniqueResult()
        {
            var query = Db().Query();
            query.Constrain(typeof (Holder));
            query.Descend("_child").Descend("_name").Constrain("unique");
            var result = query.Execute();
            Assert.AreEqual(1, result.Count);
            var holder = ((Holder
                ) result.Next());
            Assert.AreEqual("unique", holder._child._name);
        }

        public class Holder
        {
            public Item _child;

            public Holder(Item belongs)
            {
                _child = belongs;
            }
        }

        public class Item
        {
            public string _name;

            public Item(string name)
            {
                _name = name;
            }
        }
    }
}