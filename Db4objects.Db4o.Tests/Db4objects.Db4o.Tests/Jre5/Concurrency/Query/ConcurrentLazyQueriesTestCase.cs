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
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Query;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Jre5.Concurrency.Query
{
    public class ConcurrentLazyQueriesTestCase : Db4oClientServerTestCase
    {
        private const int ItemCount = 100;

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            ConfigLazyQueries(config);
        }

        private void ConfigLazyQueries(IConfiguration config)
        {
            config.Queries().EvaluationMode(QueryEvaluationMode.Lazy);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            var root = new Item(
                null, -1);
            for (var i = 0; i < ItemCount; ++i)
            {
                Store(new Item(root, i));
            }
        }

        public virtual void Conc(IExtObjectContainer client)
        {
            IExtObjectContainer container = FileSession();
            var root = QueryRoot(container);
            for (var i = 0; i < 100; ++i)
            {
                AssertAllItems(QueryItems(root, container));
            }
        }

        private Item QueryRoot(IExtObjectContainer container
            )
        {
            var q = ItemQuery(container);
            q.Descend("id").Constrain(-1);
            return (Item) q.Execute().Next();
        }

        private void AssertAllItems(IEnumerator result)
        {
            var expected = Range(ItemCount);
            for (var i = 0; i < ItemCount; ++i)
            {
                var nextItem = (Item
                    ) IteratorPlatform.Next(result);
                expected.Remove(nextItem.id);
            }
            Assert.AreEqual("[]", expected.ToString());
        }

        private Collection4 Range(int end)
        {
            var range = new Collection4();
            for (var i = 0; i < end; ++i)
            {
                range.Add(i);
            }
            return range;
        }

        private IEnumerator QueryItems(Item parent, IExtObjectContainer
            container)
        {
            var q = ItemQuery(container);
            q.Descend("parent").Constrain(parent).Identity();
            // the cast is necessary for sharpen
            return q.Execute().GetEnumerator();
        }

        private IQuery ItemQuery(IExtObjectContainer container)
        {
            return NewQuery(container, typeof (Item));
        }

        public sealed class Item
        {
            public int id;
            public Item parent;

            public Item()
            {
            }

            public Item(Item parent_, int id_)
            {
                id = id_;
                parent = parent_;
            }
        }
    }
}