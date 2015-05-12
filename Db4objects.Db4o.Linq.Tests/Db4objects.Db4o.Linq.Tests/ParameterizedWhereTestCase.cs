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

namespace Db4objects.Db4o.Linq.Tests
{
    public class ParameterizedWhereTestCase : AbstractDb4oLinqTestCase
    {
        private static readonly string _static_name = "ro";
        private readonly string _name = "reg";

        protected override void Store()
        {
            Store(new Person {Name = "jb", Age = 24});
            Store(new Person {Name = "ana", Age = 20});
            Store(new Person {Name = "reg", Age = 25});
            Store(new Person {Name = "ro", Age = 32});
            Store(new Person {Name = "jb", Age = 7});
            Store(new Person {Name = "jb", Age = 28});
            Store(new Person {Name = "jb", Age = 34});
        }

        public void TestEqualsLocalVariable()
        {
            var name = "jb";

            AssertQuery("(Person(Name == 'jb'))",
                delegate
                {
                    var jbs = from Person p in Db()
                        where p.Name == name
                        select p;

                    AssertSet(new[]
                    {
                        new Person {Name = "jb", Age = 24},
                        new Person {Name = "jb", Age = 7},
                        new Person {Name = "jb", Age = 28},
                        new Person {Name = "jb", Age = 34}
                    }, jbs);
                });
        }

        public void TestEqualsInstanceField()
        {
            AssertQuery("(Person(Name == 'reg'))",
                delegate
                {
                    var jbs = from Person p in Db()
                        where p.Name == _name
                        select p;

                    AssertSet(new[]
                    {
                        new Person {Name = "reg", Age = 25}
                    }, jbs);
                });
        }

        public void TestEqualsStaticField()
        {
            AssertQuery("(Person(Name == 'ro'))",
                delegate
                {
                    var jbs = from Person p in Db()
                        where p.Name == _static_name
                        select p;

                    AssertSet(new[]
                    {
                        new Person {Name = "ro", Age = 32}
                    }, jbs);
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
    }
}