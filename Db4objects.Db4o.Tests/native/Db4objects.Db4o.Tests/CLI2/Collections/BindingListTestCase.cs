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

#if !SILVERLIGHT
using System.ComponentModel;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.CLI2.Collections
{
    internal class BindingListTestCase : AbstractDb4oTestCase
    {
        protected override void Store()
        {
            var item = new Item();
            item._bindingList = new BindingList<Element>();
            item._bindingList.Add(new Element("one"));
            Store(item);
        }

        public void TestRetrieve()
        {
            AssertSingleItem();
        }

        private void AssertSingleItem()
        {
            var item = (Item) RetrieveOnlyInstance(typeof (Item));
            var element = item._bindingList[0];
            Assert.AreEqual("one", element._name);
        }

        public void TestDefragment()
        {
            Defragment();
            AssertSingleItem();
        }

        public class Item
        {
            public BindingList<Element> _bindingList;
        }

        public class Element
        {
            public string _name;

            public Element(string name)
            {
                _name = name;
            }
        }
    }
}

#endif