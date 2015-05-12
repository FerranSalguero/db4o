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

using System.Linq;
using Db4oUnit;

#if !CF // csc fails to find S.R.FieldInfo.GetFieldFromHandle

namespace Db4objects.Db4o.Linq.Tests
{
    public class GenericQueryTestCase : AbstractDb4oLinqTestCase
    {
        protected override void Store()
        {
            Store(new Person {Name = "jb", Age = 24});
            Store(new Person {Name = "ana", Age = 20});
            Store(new Person {Name = "reg", Age = 25});
            Store(new Person {Name = "ro", Age = 32});
        }

        public void TestGenericQuery()
        {
            AssertQuery("(Person(Name == 'ana'))",
                delegate
                {
                    var finder = new PersonFinder<Person>(Db());
                    var ana = finder.GetPerson("ana");

                    Assert.IsNotNull(ana);
                    Assert.AreEqual("ana", ana.Name);
                });
        }

        public class Person
        {
            public int Age;
            public string Name;

            public override bool Equals(object obj)
            {
                var p = obj as Person;
                if (p == null) return false;

                return p.Name == Name && p.Age == Age;
            }

            public override int GetHashCode()
            {
                return Age ^ Name.GetHashCode();
            }
        }

        private class PersonFinder<T> where T : Person
        {
            private readonly IObjectContainer _database;

            public PersonFinder(IObjectContainer database)
            {
                _database = database;
            }

            public T GetPerson(string name)
            {
                return (from T person in _database
                    where person.Name == name
                    select person).FirstOrDefault();
            }
        }
    }
}

#endif