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
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Tests.Util;
using Db4oUnit;
using Sharpen;

namespace Db4objects.Db4o.Tests.Common.Handlers
{
    public class ArrayListUpdateTestCase : HandlerUpdateTestCaseBase
    {
        private static readonly object[] Data =
        {
            "one", "aAzZ|!Â§$%&/()=?ÃŸÃ¶Ã¤Ã¼Ã„Ã–ÃœYZ;:-_+*~#^Â°'@"
            , string.Empty, CreateNestedList(10), null
        };

        private static IList CreateNestedList(int depth)
        {
            IList list = new ArrayList();
            list.Add("nested1");
            list.Add("nested2");
            if (depth > 0)
            {
                list.Add(CreateNestedList(depth - 1));
            }
            return list;
        }

        protected override string TypeName()
        {
            return "ArrayList";
        }

        protected override object[] CreateValues()
        {
            if (TestNotCompatibleToOldVersion())
            {
                return new Item[0];
            }
            var values = new Item[3];
            values[0] = CreateItem(typeof (ArrayList));
            values[1] = CreateItem(typeof (ArrayListExtensionWithField
                ));
            values[2] = CreateItem(typeof (ArrayListExtensionWithoutField
                ));
            return values;
        }

        private Item CreateItem(Type clazz)
        {
            var item = new Item();
            item._listClassName = clazz.FullName;
            CreateLists(item, clazz);
            return item;
        }

        private void CreateLists(Item item, Type clazz)
        {
            item._typed = (ArrayList) CreateFilledList(clazz);
            item._untyped = CreateFilledList(clazz);
            item._interface = CreateFilledList(clazz);
            item._emptyTyped = (ArrayList) CreateList(clazz);
            item._emptyUntyped = CreateList(clazz);
            item._emptyInterface = CreateList(clazz);
        }

        private IList CreateFilledList(Type clazz)
        {
            var list = CreateList(clazz);
            FillList(list);
            if (list is ArrayListExtensionWithField)
            {
                var typedList = (ArrayListExtensionWithField
                    ) list;
                typedList.name = ArrayListExtensionWithField.StoredName;
            }
            return list;
        }

        private IList CreateList(Type clazz)
        {
            IList list = null;
            try
            {
                list = (IList) Activator.CreateInstance(clazz);
            }
            catch (Exception e)
            {
                Runtime.PrintStackTrace(e);
            }
            return list;
        }

        private void FillList(object list)
        {
            for (var i = 0; i < Data.Length; i++)
            {
                ((IList) list).Add(Data[i]);
            }
        }

        protected override object CreateArrays()
        {
            return null;
        }

        protected override void AssertValues(IExtObjectContainer objectContainer, object[]
            values)
        {
            if (TestNotCompatibleToOldVersion())
            {
                return;
            }
            AssertItem(values[0], typeof (ArrayList));
            AssertItem(values[1], typeof (ArrayListExtensionWithField)
                );
            AssertItem(values[2], typeof (ArrayListExtensionWithoutField
                ));
        }

        protected override void AssertQueries(IExtObjectContainer objectContainer)
        {
            if (TestNotCompatibleToOldVersion())
            {
                return;
            }
            AssertQueries(objectContainer, typeof (ArrayList));
            AssertQueries(objectContainer, typeof (ArrayListExtensionWithField
                ));
            AssertQueries(objectContainer, typeof (ArrayListExtensionWithoutField
                ));
        }

        private void AssertQueries(IExtObjectContainer objectContainer, Type clazz)
        {
            AssertQuery(objectContainer, clazz, "_typed");
        }

        //        assertQuery(objectContainer, clazz, "_untyped");
        //        assertQuery(objectContainer, clazz, "_interface");
        private void AssertQuery(IExtObjectContainer objectContainer, Type clazz, string
            fieldName)
        {
            var q = objectContainer.Query();
            q.Constrain(typeof (Item));
            q.Descend("_listClassName").Constrain(clazz.FullName);
            q.Descend(fieldName).Constrain("one");
            var objectSet = q.Execute();
            Assert.AreEqual(1, objectSet.Count);
            var item = (Item) objectSet.Next(
                );
            AssertItem(item, clazz);
        }

        private void AssertItem(object obj, Type clazz)
        {
            var item = (Item) obj;
            AssertList(item._typed, clazz);
            AssertList(item._untyped, clazz);
            AssertList(item._interface, clazz);
            AssertEmptyList(item._emptyTyped);
            AssertEmptyList(item._emptyUntyped);
            AssertEmptyList(item._emptyInterface);
        }

        private void AssertEmptyList(object obj)
        {
            var list = (IList) obj;
            Assert.AreEqual(0, list.Count);
        }

        private void AssertList(object obj, Type clazz)
        {
            var list = (IList) obj;
            var array = new object[list.Count];
            var idx = 0;
            var i = list.GetEnumerator();
            while (i.MoveNext())
            {
                array[idx++] = i.Current;
            }
            ArrayAssert.AreEqual(Data, array);
            Assert.IsInstanceOf(clazz, list);
            if (list is ArrayListExtensionWithField)
            {
                var typedList = (ArrayListExtensionWithField
                    ) list;
                Assert.AreEqual(ArrayListExtensionWithField.StoredName, typedList
                    .name);
            }
        }

        protected override void AssertArrays(IExtObjectContainer objectContainer, object
            obj)
        {
        }

        // do nothing
        private bool TestNotCompatibleToOldVersion()
        {
            // This test fails for 3.0 and 4.0 versions, probably
            // because translators are incompatible.
            if (Db4oMajorVersion() < 5)
            {
                return true;
            }
            return Db4oHeaderVersion() == VersionServices.Header3040;
        }

        public class Item
        {
            public IList _emptyInterface;
            public ArrayList _emptyTyped;
            public object _emptyUntyped;
            public IList _interface;
            public string _listClassName;
            public ArrayList _typed;
            public object _untyped;
        }

        /// <summary>Todo: add as type to Item</summary>
        [Serializable]
        public class ArrayListExtensionWithField : ArrayList
        {
            public static readonly string StoredName = "outListsName";
            public string name;

            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                {
                    return false;
                }
                var other = (ArrayListExtensionWithField
                    ) obj;
                if (name == null)
                {
                    return other.name == null;
                }
                return name.Equals(other.name);
            }
        }

        /// <summary>Todo: add as type to Item</summary>
        [Serializable]
        public class ArrayListExtensionWithoutField : ArrayList
        {
        }
    }
}