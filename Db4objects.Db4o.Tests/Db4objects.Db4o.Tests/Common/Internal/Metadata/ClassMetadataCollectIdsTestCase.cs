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

using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal.Marshall;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Internal.Metadata
{
    public class ClassMetadataCollectIdsTestCase : AbstractDb4oTestCase, IOptOutMultiSession
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(new Item("root", new Item
                ("typed", null, null, new object[] {}), new Item
                    ("untyped", null, null, new object[] {}), new object[]
                    {
                        new Item
                            ("array", null, null, new object[] {})
                    }));
        }

        public virtual void TestCollectIdsByFieldName()
        {
            var root = QueryRootItem();
            var context = CollectIdContext.ForID(Trans(), (int) Db().GetID(root));
            context.ClassMetadata().CollectIDs(context, "typedRef");
            AssertCollectedIds(context, new object[] {root.typedRef});
        }

        public virtual void TestCollectIds()
        {
            var root = QueryRootItem();
            var context = CollectIdContext.ForID(Trans(), (int) Db().GetID(root));
            context.ClassMetadata().CollectIDs(context);
            AssertCollectedIds(context, new[]
            {
                root.typedRef, root.untypedRef, root.UntypedElementAt
                    (0)
            });
        }

        private void AssertCollectedIds(CollectIdContext context, object[] expectedReferences
            )
        {
            Iterator4Assert.SameContent(Iterators.Map(expectedReferences, new _IFunction4_66(
                this)), new TreeKeyIterator(context.Ids()));
        }

        private Item QueryRootItem()
        {
            return ((Item) QueryItemByName("root").Next());
        }

        private IObjectSet QueryItemByName(string itemName)
        {
            var query = NewQuery(typeof (Item));
            query.Descend("name").Constrain(itemName);
            return query.Execute();
        }

        public class Item
        {
            public string name;
            public Item typedRef;
            public object untypedArray;
            public object untypedRef;

            public Item(string name, Item ref1, Item
                ref2, object[] untypedArray)
            {
                this.name = name;
                typedRef = ref1;
                untypedRef = ref2;
                this.untypedArray = untypedArray;
            }

            public virtual object UntypedElementAt(int index)
            {
                return ((object[]) untypedArray)[index];
            }
        }

        private sealed class _IFunction4_66 : IFunction4
        {
            private readonly ClassMetadataCollectIdsTestCase _enclosing;

            public _IFunction4_66(ClassMetadataCollectIdsTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public object Apply(object reference)
            {
                return (int) _enclosing.Db().GetID(reference);
            }
        }
    }
}