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
using System.Collections;
using System.Collections.Generic;
using Db4objects.Db4o.Collections;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Btree;
using Db4objects.Db4o.Internal.Collections;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;
using Sharpen.Util;

namespace Db4objects.Db4o.Tests.Jre12.Collections
{
    public class BigSetTestCase : AbstractDb4oTestCase, IOptOutMultiSession
    {
        private static readonly Item ItemOne = new Item("one"
            );

        private static readonly Item[] items =
        {
            new Item("one"), new Item("two"), new Item
                ("three")
        };

        public static void Main(string[] args)
        {
            new BigSetTestCase().RunSolo("testBigSetAfterCommit");
        }

        public virtual void TestRefreshBigSet()
        {
            var holder = NewHolderWithBigSet(new Item
                ("1"), new Item("2"));
            StoreAndCommit(holder);
            Db().Refresh(holder, int.MaxValue);
            Assert.AreEqual(2, holder._set.Count);
        }

        public virtual void TestAddAfterCommit()
        {
            RunTestAfterCommit(new _IProcedure4_70());
        }

        private void RunTestAfterCommit(IProcedure4 setOperations)
        {
            var holder = NewHolderWithBigSet(new Item
                ("1"), new Item("2"));
            StoreAndCommit(holder);
            var set = holder._set;
            Assert.AreEqual(2, set.Count);
            setOperations.Apply(set);
            PurgeAll(holder, holder._set);
            var resurrected = (Holder<Item
                >) RetrieveOnlyInstance(holder.GetType());
            IteratorAssert.SameContent(set.GetEnumerator(), resurrected._set.GetEnumerator());
        }

        public virtual void TestClearAfterCommit()
        {
            RunTestAfterCommit(new _IProcedure4_92());
        }

        public virtual void TestRemoveAfterCommit()
        {
            RunTestAfterCommit(new _IProcedure4_100(this));
        }

        protected virtual Item QueryItem(string name)
        {
            var query = NewQuery(typeof (Item));
            query.Descend("_name").Constrain(name);
            return (Item) query.Execute()[0];
        }

        private void StoreAndCommit(Holder<Item> holder)
        {
            Store(holder);
            Db().Commit();
        }

        public virtual void TestPurgeBeforeCommit()
        {
            var holder = NewHolderWithBigSet(new Item
                ("foo"));
            Store(holder);
            PurgeAll(holder, holder._set);
            holder = (Holder<Item>) RetrieveOnlyInstance(holder.
                GetType());
            Assert.AreEqual(1, holder._set.Count);
        }

        private Holder<Item> NewHolderWithBigSet(params Item
            [] item)
        {
            var holder = new Holder<Item
                >();
            holder._set = NewBigSet(item);
            return holder;
        }

        private void PurgeAll(params object[] objects)
        {
            foreach (var @object in objects)
            {
                Db().Purge(@object);
            }
        }

        public virtual void TestTypeHandlerInstalled()
        {
            var typeHandler = Container().Handlers.ConfiguredTypeHandler(Reflector(
                ).ForClass(NewBigSet().GetType()));
            Assert.IsInstanceOf(typeof (BigSetTypeHandler), typeHandler);
        }

        public virtual void TestEmptySet()
        {
            var set = NewBigSet();
            Assert.AreEqual(0, set.Count);
        }

        public virtual void TestSize()
        {
            var set = NewBigSet();
            set.Add(ItemOne);
            Assert.AreEqual(1, set.Count);
            set.Remove(ItemOne);
            Assert.AreEqual(0, set.Count);
            var itemTwo = new Item("two");
            set.Add(itemTwo);
            set.Add(new Item("three"));
            Assert.AreEqual(2, set.Count);
            set.Remove(itemTwo);
            Assert.AreEqual(1, set.Count);
        }

        public virtual void TestContains()
        {
            var set = NewBigSet();
            set.Add(ItemOne);
            Assert.IsTrue(set.Contains(ItemOne));
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestPersistence()
        {
            var holder = new Holder<Item
                >();
            holder._set = NewBigSet();
            var set = holder._set;
            set.Add(ItemOne);
            Store(holder);
            Reopen();
            holder = (Holder<Item>) RetrieveOnlyInstance(holder.
                GetType());
            set = holder._set;
            AssertSinglePersistentItem(set);
        }

        private void AssertSinglePersistentItem(Db4o.Collections.ISet<Item
            > set)
        {
            var expectedItem = (Item) RetrieveOnlyInstance(typeof (
                Item));
            Assert.IsNotNull(set);
            Assert.AreEqual(1, set.Count);
            IEnumerator setIterator = set.GetEnumerator();
            Assert.IsNotNull(setIterator);
            Assert.IsTrue(setIterator.MoveNext());
            var actualItem = (Item) setIterator.Current;
            Assert.AreSame(expectedItem, actualItem);
        }

        public virtual void TestAddAllContainsAll()
        {
            var set = NewBigSet();
            var collection = ItemList();
            Assert.IsTrue(Sharpen.Collections.AddAll(set, collection));
            Assert.IsTrue(set.ContainsAll(collection));
            Assert.IsFalse(Sharpen.Collections.AddAll(set, collection));
            Assert.AreEqual(collection.Count, set.Count);
        }

        public virtual void TestRemove()
        {
            var set = NewBigSet();
            var collection = ItemList();
            Sharpen.Collections.AddAll(set, collection);
            var first = collection[0];
            set.Remove(first);
            Assert.IsTrue(collection.Remove(first));
            Assert.IsFalse(collection.Remove(first));
            Assert.IsTrue(set.ContainsAll(collection));
            Assert.IsFalse(set.Contains(first));
        }

        public virtual void TestRemoveAll()
        {
            var set = NewBigSet();
            var collection = ItemList();
            Sharpen.Collections.AddAll(set, collection);
            Assert.IsTrue(set.RemoveAll(collection));
            Assert.AreEqual(0, set.Count);
            Assert.IsFalse(set.RemoveAll(collection));
        }

        public virtual void TestIsEmpty()
        {
            var set = NewBigSet();
            Assert.IsTrue(set.IsEmpty);
            set.Add(ItemOne);
            Assert.IsFalse(set.IsEmpty);
            set.Remove(ItemOne);
            Assert.IsTrue(set.IsEmpty);
        }

        public virtual void TestIterator()
        {
            var set = NewBigSet();
            ICollection<Item> collection = ItemList();
            Sharpen.Collections.AddAll(set, collection);
            IEnumerator i = set.GetEnumerator();
            Assert.IsNotNull(i);
            IteratorAssert.SameContent(collection.GetEnumerator(), i);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestDelete()
        {
            var set = NewBigSet();
            set.Add(ItemOne);
            Db().Store(set);
            Db().Commit();
            var bTree = BTree(set);
            BTreeAssert.AssertAllSlotsFreed(FileTransaction(), bTree, new _ICodeBlock_259(this
                , set));
            Assert.Expect(typeof (InvalidOperationException), new _ICodeBlock_265(set));
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestDefragment()
        {
            var set = NewBigSet();
            set.Add(ItemOne);
            Db().Store(set);
            Db().Commit();
            Defragment();
            set = (Db4o.Collections.ISet<Item>) RetrieveOnlyInstance
                (set.GetType());
            AssertSinglePersistentItem(set);
        }

        public virtual void TestClear()
        {
            var set = NewBigSet();
            set.Add(ItemOne);
            set.Clear();
            Assert.AreEqual(0, set.Count);
        }

        private IList<Item> ItemList()
        {
            IList<Item> c = new List<Item>();
            for (var i = 0; i < items.Length; i++)
            {
                c.Add(items[i]);
            }
            return c;
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestGetInternalImplementation()
        {
            var set = NewBigSet();
            var bTree = BTree(set);
            Assert.IsNotNull(bTree);
        }

        private Db4o.Collections.ISet<Item> NewBigSet(params Item
            [] initialSet)
        {
            var set = CollectionFactory.ForObjectContainer
                (Db()).NewBigSet<Item>();
            Sharpen.Collections.AddAll(set, Arrays.AsList(initialSet));
            return set;
        }

        /// <exception cref="System.MemberAccessException"></exception>
        public static BTree BTree(Db4o.Collections.ISet<Item> set
            )
        {
            return (BTree) Reflection4.GetFieldValue(set, "_bTree");
        }

        private LocalTransaction FileTransaction()
        {
            return ((LocalTransaction) Trans());
        }

        public class Holder<E>
        {
            public Db4o.Collections.ISet<E> _set;
        }

        public class Item
        {
            public string _name;

            public Item(string name)
            {
                _name = name;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Item))
                {
                    return false;
                }
                var other = (Item) obj;
                if (_name == null)
                {
                    return other._name == null;
                }
                return _name.Equals(other._name);
            }

            public override string ToString()
            {
                return "Item(" + _name + ")";
            }
        }

        private sealed class _IProcedure4_70 : IProcedure4
        {
            public void Apply(object set)
            {
                ((Db4o.Collections.ISet<Item>) set).Add(new Item
                    ("3"));
            }
        }

        private sealed class _IProcedure4_92 : IProcedure4
        {
            public void Apply(object set)
            {
                ((Db4o.Collections.ISet<Item>) set).Clear();
            }
        }

        private sealed class _IProcedure4_100 : IProcedure4
        {
            private readonly BigSetTestCase _enclosing;

            public _IProcedure4_100(BigSetTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Apply(object set)
            {
                ((Db4o.Collections.ISet<Item>) set).Remove(_enclosing
                    .QueryItem("1"));
            }
        }

        private sealed class _ICodeBlock_259 : ICodeBlock
        {
            private readonly BigSetTestCase _enclosing;
            private readonly Db4o.Collections.ISet<Item> set;

            public _ICodeBlock_259(BigSetTestCase _enclosing, Db4o.Collections.ISet
                <Item> set)
            {
                this._enclosing = _enclosing;
                this.set = set;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.Db().Delete(set);
                _enclosing.Db().Commit();
            }
        }

        private sealed class _ICodeBlock_265 : ICodeBlock
        {
            private readonly Db4o.Collections.ISet<Item> set;

            public _ICodeBlock_265(Db4o.Collections.ISet<Item> set)
            {
                this.set = set;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                set.Add(ItemOne);
            }
        }
    }
}