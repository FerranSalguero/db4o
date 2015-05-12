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

namespace Db4objects.Db4o.Tests.Common.Soda.Arrays
{
    public class ArrayDescendSubQueryTestCase : AbstractDb4oTestCase
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            var erich = new Person
                ("Erich");
            var kent = new Person
                ("Kent");
            var bill = new Person
                ("Bill");
            var gof = new Book("gof"
                , erich, new Book[0]);
            var xp = new Book("xp"
                , kent, new[] {gof});
            var ddd = new Book("ddd"
                , bill, new[] {gof, xp});
            Store(ddd);
        }

        // all books cited in ddd - works
        public virtual void TestSimpleDescend()
        {
            var topQuery = NewQuery(typeof (Book));
            topQuery.Descend("_title").Constrain("ddd");
            var subQuery = topQuery.Descend("_cites");
            Assert.AreEqual(2, subQuery.Execute().Count);
        }

        // all authors of books cited in ddd - only selects one array element as root for the second descend
        public virtual void TestDoubleDescend()
        {
            var topQuery = NewQuery(typeof (Book));
            topQuery.Descend("_title").Constrain("ddd");
            var subQuery = topQuery.Descend("_cites").Descend("_author");
            var result = subQuery.Execute();
            //		while(result.hasNext()) {
            //			System.out.println(result.next());
            //		}
            Assert.AreEqual(2, result.Count);
        }

        public class Person
        {
            public string _name;

            public Person(string name)
            {
                //COR-1977
                _name = name;
            }

            public override string ToString()
            {
                return _name;
            }
        }

        public class Book
        {
            public Person _author;
            public Book[] _cites;
            public string _title;

            public Book(string title, Person author, Book
                [] cites)
            {
                _title = title;
                _author = author;
                _cites = cites;
            }

            public override string ToString()
            {
                return _title;
            }
        }
    }
}