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

using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Classindex
{
    public class ClassIndexTestCase : AbstractDb4oTestCase, IOptOutMultiSession
    {
        public static void Main(string[] args)
        {
            new ClassIndexTestCase().RunSolo();
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestDelete()
        {
            var item = new Item("test");
            Store(item);
            var id = (int) Db().GetID(item);
            AssertID(id);
            Reopen();
            item = (Item) Db().QueryByExample(item).Next();
            id = (int) Db().GetID(item);
            AssertID(id);
            Db().Delete(item);
            Db().Commit();
            AssertEmpty();
            Reopen();
            AssertEmpty();
        }

        private void AssertID(int id)
        {
            AssertIndex(new object[] {id});
        }

        private void AssertEmpty()
        {
            AssertIndex(new object[] {});
        }

        private void AssertIndex(object[] expected)
        {
            var visitor = new ExpectingVisitor(expected);
            var index = ClassMetadataFor(typeof (Item)).Index
                ();
            index.TraverseAll(Trans(), visitor);
            visitor.AssertExpectations();
        }

        public class Item
        {
            public string name;

            public Item(string _name)
            {
                name = _name;
            }
        }
    }
}