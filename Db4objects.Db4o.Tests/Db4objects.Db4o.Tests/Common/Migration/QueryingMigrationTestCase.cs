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

using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Tests.Common.Handlers;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Migration
{
    public class QueryingMigrationTestCase : HandlerUpdateTestCaseBase
    {
        private const int ObjectCount = 5;

        protected override object[] CreateValues()
        {
            var cars = new object[ObjectCount];
            for (var i = 0; i < cars.Length; i++)
            {
                var car = new Car();
                car._name = "Car " + i;
                var pilot = new Pilot();
                car._pilot = pilot;
                pilot._name = "Pilot " + i;
                cars[i] = car;
            }
            return cars;
        }

        protected override void AssertValues(IExtObjectContainer objectContainer, object[]
            values)
        {
        }

        protected override object CreateArrays()
        {
            return null;
        }

        protected override void AssertArrays(IExtObjectContainer objectContainer, object
            obj)
        {
        }

        // do nothing
        protected override void AssertQueries(IExtObjectContainer objectContainer)
        {
            for (var i = 0; i < ObjectCount; i++)
            {
                var query = objectContainer.Query();
                query.Constrain(typeof (Car));
                query.Descend("_pilot").Descend("_name").Constrain("Pilot " + i);
                var objectSet = query.Execute();
                Assert.AreEqual(1, objectSet.Count);
                var car = (Car) objectSet.Next
                    ();
                Assert.AreEqual("Car " + i, car._name);
                Assert.AreEqual("Pilot " + i, car._pilot._name);
            }
        }

        protected override string TypeName()
        {
            return "querying";
        }

        public class Car
        {
            public string _name;
            public Pilot _pilot;
        }

        public class Pilot
        {
            public string _name;
        }
    }
}