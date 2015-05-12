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
    public class PreserveJoinsTestCase : AbstractDb4oTestCase
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(new Parent(new Child("bar"), "parent"
                ));
        }

        public virtual void Test()
        {
            var barQuery = Db().Query();
            barQuery.Constrain(typeof (Child));
            barQuery.Descend("name").Constrain("bar");
            var barObj = barQuery.Execute().Next();
            var query = Db().Query();
            query.Constrain(typeof (Parent));
            var c1 = query.Descend("value").Constrain("dontexist");
            var c2 = query.Descend("child").Constrain(barObj);
            var c1_and_c2 = c1.And(c2);
            var cParent = query.Descend("value").Constrain("parent");
            c1_and_c2.Or(cParent);
            Assert.AreEqual(1, query.Execute().Count);
        }

        public class Parent
        {
            public Child child;
            public string value;

            public Parent(Child child, string value)
            {
                this.child = child;
                this.value = value;
            }
        }

        public class Child
        {
            public string name;

            public Child(string name)
            {
                this.name = name;
            }
        }
    }
}