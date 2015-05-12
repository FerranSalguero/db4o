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

using System.Collections;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Soda
{
    /// <exclude></exclude>
    public class CollectIdTestCase : AbstractDb4oTestCase
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            var holder = new ListHolder();
            holder._list = new ArrayList();
            var parent = new Parent();
            holder._list.Add(parent);
            parent._child = new Child();
            parent._child._name = "child";
            Store(holder);
        }

        public virtual void Test()
        {
            var query = NewQuery(typeof (ListHolder));
            var qList = query.Descend("_list");
            // qList.execute();
            var qChild = qList.Descend("_child");
            qChild.Execute();
        }

        public class ListHolder
        {
            public IList _list;
        }

        public class Parent
        {
            public Child _child;
        }

        public class Child
        {
            public string _name;
        }
    }
}