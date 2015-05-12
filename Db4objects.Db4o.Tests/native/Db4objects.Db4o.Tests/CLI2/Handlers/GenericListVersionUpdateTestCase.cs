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

using System.Collections.Generic;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Tests.Common.Handlers;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.CLI2.Handlers
{
    internal class GenericListVersionUpdateTestCase : HandlerUpdateTestCaseBase
    {
        private static IList<int> intList1()
        {
            return new List<int>(new[] {int.MinValue, 0, int.MaxValue});
        }

        private static IList<int> intList2()
        {
            return new List<int>(new[] {1, 2, 3});
        }

        private static IList<int?> nullableIntList1()
        {
            return new List<int?>(new int?[] {1, 2, 3});
        }

        private static IList<SimpleItem> simpleItemList1()
        {
            return new List<SimpleItem>(new[] {new SimpleItem(100), new SimpleItem(200)});
        }

        private static IList<SimpleItem> simpleItemList2()
        {
            return new List<SimpleItem>(new[] {new SimpleItem(-1), new SimpleItem(42)});
        }

        private static IList<SimpleItem> simpleItemEmptyList()
        {
            return new List<SimpleItem>();
        }

        private static IList<string> stringList1()
        {
            return new List<string>(new[] {"Adriano", null, "Norberto", string.Empty});
        }

        private static IList<string> stringList2()
        {
            return new List<string>(new[] {"Foo", "Bar", string.Empty});
        }

        protected override string TypeName()
        {
            return "Generic List Version Update";
        }

        protected override object[] CreateValues()
        {
            return new object[]
            {
                new Item<int>(intList1(), intList2(), null),
                new Item<string>(stringList1(), stringList2(), simpleItemList1())

                // TODO: Lists of nullable types are broken after retrieval
                //       The issue becomes apparent when the new Typehandler kicks in
                //       and tries to store them.
                // new Item<int?>( nullableIntList1(), stringList1(), simpleItemList1()),
            };
        }

        protected override object CreateArrays()
        {
            IList<int>[] intList = {intList1(), intList2()};

            IList<SimpleItem>[] simpleItemList =
            {
                simpleItemList1(),
                null,
                simpleItemList2(),
                simpleItemEmptyList()
            };

            return new ItemArray(intList, simpleItemList, simpleItemList, intList);
        }

        protected override void AssertValues(IExtObjectContainer objectContainer, object[] values)
        {
            AssertItem(objectContainer, (Item<int>) values[0], intList1(), intList2(), null);
            AssertItem(objectContainer, (Item<string>) values[1], stringList1(), stringList2(), simpleItemList1());

            //TODO: Enable after fixing nullable array handling.
            //AssertItem((Item<int?>)values[2], nullableIntList1(), stringList1(), simpleItemList1());
        }

        private void AssertItem<T, R>(IExtObjectContainer objectContainer, Item<T> tba, IList<T> list,
            IList<R> untypedGenericList, IList<SimpleItem> simpleItemList)
        {
            Assert.IsNotNull(tba);
            AssertList(list, tba.list);
            AssertQuery(objectContainer, tba, tba.list, "list");
            AssertList(untypedGenericList, tba.untypedGenericList as IList<R>);
            AssertList(simpleItemList, tba.simpleItemList);
        }

        private void AssertQuery<T>(IExtObjectContainer objectContainer, Item<T> item, IList<T> list, string fieldName)
        {
            if (Db4oHandlerVersion() < 4)
            {
                return;
            }
            if (list.Count < 1)
            {
                return;
            }
            var query = objectContainer.Query();
            query.Constrain(typeof (Item<T>));
            object constraint = list[0];
            query.Descend(fieldName).Constrain(constraint);
            var objectSet = query.Execute();
            Assert.AreEqual(1, objectSet.Count);
            var queriedItem = (Item<T>) objectSet.Next();
            Assert.AreSame(item, queriedItem);
        }

        private void AssertList<T, S>(IList<T> expected, IList<S> actual)
        {
            if (expected != null)
            {
                Assert.IsNotNull(actual);
                Iterator4Assert.AreEqual(expected.GetEnumerator(), actual.GetEnumerator());
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

            AssertArrayList(
                new[] {intList1(), intList2()},
                itemArray.arrayOfIntList);

            AssertArrayList(
                new[] {simpleItemList1(), null, simpleItemList2(), simpleItemEmptyList()},
                itemArray.arrayOfSimpleItemList);

            AssertArrayList(
                new[] {simpleItemList1(), null, simpleItemList2(), simpleItemEmptyList()},
                (IList<SimpleItem>[]) itemArray.genericListArrayInObject);

            AssertArrayList(
                new[] {intList1(), intList2()},
                (IList<int>[]) itemArray.genericListArrayInObjectArray);
        }

        private void AssertArrayList<T>(IList<T>[] expected, IList<T>[] actual)
        {
            Assert.AreEqual(expected.Length, actual.Length);
            for (var i = 0; i < expected.Length; i++)
            {
                AssertList(expected[i], actual[i]);
            }
        }

        private class Item<T>
        {
            public readonly IList<T> list;
            public readonly IList<SimpleItem> simpleItemList;
            public readonly object untypedGenericList;

            public Item(IList<T> list_, object untypedGenericList_, IList<SimpleItem> simpleItemList_)
            {
                list = list_;
                untypedGenericList = untypedGenericList_;
                simpleItemList = simpleItemList_;
            }
        }

        private class ItemArray
        {
            public readonly IList<int>[] arrayOfIntList;
            public readonly IList<SimpleItem>[] arrayOfSimpleItemList;
            public readonly object genericListArrayInObject;
            public readonly object[] genericListArrayInObjectArray;

            public ItemArray(
                IList<int>[] arrayOfIntList_,
                IList<SimpleItem>[] arrayOfSimpleItemList_,
                object genericListArrayInObject_,
                object[] genericListArrayInObjectArray_)
            {
                arrayOfIntList = arrayOfIntList_;
                arrayOfSimpleItemList = arrayOfSimpleItemList_;
                genericListArrayInObject = genericListArrayInObject_;
                genericListArrayInObjectArray = genericListArrayInObjectArray_;
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