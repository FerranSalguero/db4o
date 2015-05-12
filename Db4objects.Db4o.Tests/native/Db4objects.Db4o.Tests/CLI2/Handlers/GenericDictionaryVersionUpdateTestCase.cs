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
using System.Collections.Generic;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Tests.Common.Handlers;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.CLI2.Handlers
{
    internal class GenericDictionaryVersionUpdateTestCase : HandlerUpdateTestCaseBase
    {
        private static IDictionary<int, int> intDictionary1()
        {
            return NewDictionary<int>(new[] {int.MinValue, 0, int.MaxValue});
        }

        private static IDictionary<int, int> intDictionary2()
        {
            return NewDictionary<int>(new[] {1, 2, 3});
        }

        private static IDictionary<K, K> NewDictionary<K>(Array arr)
        {
            IDictionary<K, K> dict = new Dictionary<K, K>();
            foreach (K obj in arr)
            {
                dict[obj] = obj;
            }
            return dict;
        }

        private static IDictionary<int?, int?> nullableIntDictionary1()
        {
            return NewDictionary<int?>(new int?[] {1, 2, 3});
        }

        private static IDictionary<SimpleItem, SimpleItem> simpleItemDictionary1()
        {
            return NewDictionary<SimpleItem>(new[] {new SimpleItem(100), new SimpleItem(200)});
        }

        private static IDictionary<SimpleItem, SimpleItem> simpleItemDictionary2()
        {
            return NewDictionary<SimpleItem>(new[] {new SimpleItem(-1), new SimpleItem(42)});
        }

        private static IDictionary<SimpleItem, SimpleItem> simpleItemEmptyDictionary()
        {
            return new Dictionary<SimpleItem, SimpleItem>();
        }

        private static IDictionary<string, string> stringDictionary1()
        {
            return NewDictionary<string>(new[] {"Adriano", "Norberto", string.Empty});
        }

        private static IDictionary<string, string> stringDictionary2()
        {
            return NewDictionary<string>(new[] {"Foo", "Bar", string.Empty});
        }

        protected override string TypeName()
        {
            return "Generic List Version Update";
        }

        protected override object[] CreateValues()
        {
            return new object[]
            {
                new Item<int, int>(intDictionary1(), intDictionary2(), null),
                new Item<string, string>(stringDictionary1(), stringDictionary2(), simpleItemDictionary1())

                // TODO: Dictionaries of nullable types are broken after retrieval
                //       The issue becomes apparent when the new Typehandler kicks in
                //       and tries to store them.
                // new Item<int?, int?>( nullableIntDictionary1(), stringDictionary1(), simpleItemDictionary1()),
            };
        }

        protected override object CreateArrays()
        {
            IDictionary<int, int>[] intDictionary = {intDictionary1(), intDictionary2()};

            IDictionary<SimpleItem, SimpleItem>[] simpleItemDictionary =
            {
                simpleItemDictionary1(),
                simpleItemDictionary2(),
                simpleItemEmptyDictionary()
            };

            return new ItemArray(intDictionary, simpleItemDictionary, simpleItemDictionary, intDictionary);
        }

        protected override void AssertValues(IExtObjectContainer objectContainer, object[] values)
        {
            AssertItem(objectContainer, (Item<int, int>) values[0], intDictionary1(), intDictionary2(), null);
            AssertItem(objectContainer, (Item<string, string>) values[1], stringDictionary1(), stringDictionary2(),
                simpleItemDictionary1());

            //TODO: Enable after fixing nullable array handling.
            // AssertItem(objectContainer, (Item<int?, int?>)values[2], nullableIntDictionary1(), stringDictionary1(), simpleItemDictionary1());
        }

        private void AssertItem<T, R>(IExtObjectContainer objectContainer, Item<T, T> tba, IDictionary<T, T> dictionary,
            IDictionary<R, R> untypedGenericList, IDictionary<SimpleItem, SimpleItem> simpleItemDictionary)
        {
            Assert.IsNotNull(tba);
            AssertDictionary(dictionary, tba.dictionary);
            AssertQuery(objectContainer, tba, tba.dictionary, "dictionary");
            AssertDictionary(untypedGenericList, tba.untypedGenericDictionary as IDictionary<R, R>);
            AssertDictionary(simpleItemDictionary, tba.simpleItemDictionary);
        }

        private void AssertQuery<T>(IExtObjectContainer objectContainer, Item<T, T> item, IDictionary<T, T> dictionary,
            string fieldName)
        {
            if (Db4oHandlerVersion() < 4)
            {
                return;
            }
            var keys = dictionary.Keys;
            if (keys.Count < 1)
            {
                return;
            }
            var query = objectContainer.Query();
            query.Constrain(typeof (Item<T, T>));
            var enumerator = keys.GetEnumerator();
            enumerator.MoveNext();
            object constraint = enumerator.Current;
            query.Descend(fieldName).Constrain(constraint);
            var objectSet = query.Execute();
            Assert.AreEqual(1, objectSet.Count);
            var queriedItem = (Item<T, T>) objectSet.Next();
            Assert.AreSame(item, queriedItem);
        }

        private void AssertDictionary<T, S>(IDictionary<T, T> expected, IDictionary<S, S> actual)
        {
            if (expected != null)
            {
                Assert.IsNotNull(actual);
                Iterator4Assert.AreEqual(expected.GetEnumerator(), actual.GetEnumerator());
                foreach (var key in expected.Keys)
                {
                    Assert.AreEqual(key, expected[key]);
                }
            }
            else
            {
                Assert.IsNull(actual);
            }
        }

        protected override void AssertArrays(IExtObjectContainer objectContainer, object obj)
        {
            var itemArray = obj as ItemArray;
            Assert.IsNotNull(itemArray);

            AssertDictionary(
                new[] {intDictionary1(), intDictionary2()},
                itemArray.arrayOfIntDictionary);

            AssertDictionary(
                new[] {simpleItemDictionary1(), simpleItemDictionary2(), simpleItemEmptyDictionary()},
                itemArray.arrayOfSimpleItemDictionary);

            AssertDictionary(
                new[] {simpleItemDictionary1(), simpleItemDictionary2(), simpleItemEmptyDictionary()},
                (IDictionary<SimpleItem, SimpleItem>[]) itemArray.genericDictionaryArrayInObject);

            AssertDictionary(
                new[] {intDictionary1(), intDictionary2()},
                (IDictionary<int, int>[]) itemArray.genericDictionaryArrayInObjectArray);
        }

        private void AssertDictionary<T>(IDictionary<T, T>[] expected, IDictionary<T, T>[] actual)
        {
            Assert.AreEqual(expected.Length, actual.Length);
            for (var i = 0; i < expected.Length; i++)
            {
                AssertDictionary(expected[i], actual[i]);
            }
        }

        protected override void ConfigureForTest(IConfiguration config)
        {
            if (Db4oMajorVersion() > 7)
            {
                return;
            }
            if (Db4oMajorVersion() == 7)
            {
                if (Db4oMinorVersion() > 5)
                {
                    return;
                }
            }
            config.ExceptionsOnNotStorable(false);
        }

        private class Item<K, V>
        {
            public readonly IDictionary<K, V> dictionary;
            public readonly IDictionary<SimpleItem, SimpleItem> simpleItemDictionary;
            public readonly object untypedGenericDictionary;

            public Item(IDictionary<K, V> list_, object untypedGenericDictionary_,
                IDictionary<SimpleItem, SimpleItem> simpleItemDictionary_)
            {
                dictionary = list_;
                untypedGenericDictionary = untypedGenericDictionary_;
                simpleItemDictionary = simpleItemDictionary_;
            }
        }

        private class ItemArray
        {
            public readonly IDictionary<int, int>[] arrayOfIntDictionary;
            public readonly IDictionary<SimpleItem, SimpleItem>[] arrayOfSimpleItemDictionary;
            public readonly object genericDictionaryArrayInObject;
            public readonly object[] genericDictionaryArrayInObjectArray;

            public ItemArray(
                IDictionary<int, int>[] arrayOfIntDictionary_,
                IDictionary<SimpleItem, SimpleItem>[] arrayOfSimpleItemDictionary_,
                object genericDictionaryArrayInObject_,
                object[] genericDictionaryArrayInObjectArray_)
            {
                arrayOfIntDictionary = arrayOfIntDictionary_;
                arrayOfSimpleItemDictionary = arrayOfSimpleItemDictionary_;
                genericDictionaryArrayInObject = genericDictionaryArrayInObject_;
                genericDictionaryArrayInObjectArray = genericDictionaryArrayInObjectArray_;
            }
        }

        private sealed class SimpleItem
        {
            public readonly int foo;

            public SimpleItem(int foo_)
            {
                foo = foo_;
            }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;

                if (obj.GetType() != GetType()) return false;

                var item = (SimpleItem) obj;
                return item.foo == foo;
            }
        }
    }
}