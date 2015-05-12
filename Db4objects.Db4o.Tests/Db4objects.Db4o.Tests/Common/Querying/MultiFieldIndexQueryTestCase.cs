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
using Sharpen;

namespace Db4objects.Db4o.Tests.Common.Querying
{
    /// <exclude></exclude>
    public class MultiFieldIndexQueryTestCase : AbstractDb4oTestCase
    {
        public static void Main(string[] args)
        {
            new MultiFieldIndexQueryTestCase().RunSolo();
        }

        protected override void Configure(IConfiguration config)
        {
            IndexAllFields(config, typeof (Book));
            IndexAllFields(config, typeof (Person));
        }

        protected virtual void IndexAllFields(IConfiguration config, Type clazz)
        {
            var fields = Runtime.GetDeclaredFields(clazz);
            for (var i = 0; i < fields.Length; i++)
            {
                IndexField(config, clazz, fields[i].Name);
            }
            var superclass = clazz.BaseType;
            if (superclass != null)
            {
                IndexAllFields(config, superclass);
            }
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            var aaron = new Person
                ("Aaron", "OneOK");
            var bill = new Person
                ("Bill", "TwoOK");
            var chris = new Person
                ("Chris", "ThreeOK");
            var dave = new Person
                ("Dave", "FourOK");
            var neil = new Person
                ("Neil", "Notwanted");
            var nat = new Person
                ("Nat", "Neverwanted");
            Db().Store(new Book("Persistence possibilities", new[] {aaron, bill, chris}));
            Db().Store(new Book("Persistence using S.O.D.A.", new[] {aaron}));
            Db().Store(new Book("Persistence using JDO", new[] {bill, dave}));
            Db().Store(new Book("Don't want to find Phil", new[] {aaron, bill, neil}));
            Db().Store(new Book("Persistence by Jeff", new[] {nat}));
        }

        public virtual void Test()
        {
            var qBooks = NewQuery();
            qBooks.Constrain(typeof (Book));
            qBooks.Descend("title").Constrain("Persistence").Like();
            var qAuthors = qBooks.Descend("authors");
            var qFirstName = qAuthors.Descend("firstName");
            var qLastName = qAuthors.Descend("lastName");
            var cAaron = qFirstName.Constrain("Aaron").And(qLastName.Constrain("OneOK"
                ));
            var cBill = qFirstName.Constrain("Bill").And(qLastName.Constrain("TwoOK")
                );
            cAaron.Or(cBill);
            var results = qAuthors.Execute();
            Assert.AreEqual(4, results.Count);
            while (results.HasNext())
            {
                var person = (Person
                    ) results.Next();
                Assert.IsTrue(person.lastName.EndsWith("OK"));
            }
        }

        public class Book
        {
            public Person[] authors;
            public string title;

            public Book()
            {
            }

            public Book(string title, Person[] authors)
            {
                this.title = title;
                this.authors = authors;
            }

            public override string ToString()
            {
                var ret = title;
                if (authors != null)
                {
                    for (var i = 0; i < authors.Length; i++)
                    {
                        ret += "\n  " + authors[i];
                    }
                }
                return ret;
            }
        }

        public class Person
        {
            public string firstName;
            public string lastName;

            public Person()
            {
            }

            public Person(string firstName, string lastName)
            {
                this.firstName = firstName;
                this.lastName = lastName;
            }

            public override string ToString()
            {
                return "Person " + firstName + " " + lastName;
            }
        }
    }
}