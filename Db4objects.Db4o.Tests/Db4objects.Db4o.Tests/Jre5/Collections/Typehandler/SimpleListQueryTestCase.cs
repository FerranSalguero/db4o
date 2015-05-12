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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Query;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Jre5.Collections.Typehandler
{
    /// <exclude></exclude>
    public class SimpleListQueryTestCase : AbstractDb4oTestCase
    {
        internal static readonly object[] Data =
        {
            "one", "two", 1, 2, 42, new
                ReferenceTypeElement("one"),
            new ReferenceTypeElement
                ("fortytwo")
        };

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(typeof (Item)).CascadeOnDelete(true);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            for (var i = 0; i < Data.Length; i++)
            {
                StoreItem(Data[i]);
            }
        }

        private void StoreItem(object listElement)
        {
            var item = new Item();
            item.list = new ArrayList();
            item.list.Add(listElement);
            Store(item);
        }

        public virtual void TestListConstrainQuery()
        {
            for (var i = 0; i < Data.Length; i++)
            {
                AssertSingleElementQuery(Data[i]);
            }
        }

        private void AssertSingleElementQuery(object element)
        {
            var q = Db().Query();
            q.Constrain(typeof (Item));
            q.Descend("list").Constrain(element);
            AssertSingleElementQueryResult(q, element);
        }

        private void AssertSingleElementQueryResult(IQuery query, object element)
        {
            var objectSet = query.Execute();
            Assert.AreEqual(1, objectSet.Count);
            var item = (Item) objectSet.Next(
                );
            Assert.AreEqual(element, item.list[0]);
        }

        public class Item
        {
            public IList list;
        }

        public class ReferenceTypeElement
        {
            public string name;

            public ReferenceTypeElement(string name_)
            {
                name = name_;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is ReferenceTypeElement))
                {
                    return false;
                }
                var other = (ReferenceTypeElement
                    ) obj;
                if (name == null)
                {
                    return other.name == null;
                }
                return name.Equals(other.name);
            }
        }
    }
}