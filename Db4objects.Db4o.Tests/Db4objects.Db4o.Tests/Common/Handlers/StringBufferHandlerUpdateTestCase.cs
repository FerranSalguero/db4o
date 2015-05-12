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

using System.Text;
using Db4objects.Db4o.Ext;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Handlers
{
    public class StringBufferHandlerUpdateTestCase : HandlerUpdateTestCaseBase
    {
        private static readonly StringBuilder[] data =
        {
            new StringBuilder
                ("one"),
            new StringBuilder("aAzZ\u05d0\u05d1\u4e2d"), new StringBuilder(string.Empty
                ),
            null
        };

        public static void Main(string[] args)
        {
            new ConsoleTestRunner(typeof (StringBufferHandlerUpdateTestCase)).Run();
        }

        protected override void AssertArrays(IExtObjectContainer objectContainer, object
            obj)
        {
            var item = (ItemArrays
                ) obj;
            AssertTypedArray(item);
            AssertUntypedArray(item);
            AssertArrayInObject(item);
        }

        private void AssertArrayInObject(ItemArrays item
            )
        {
            AssertData((StringBuilder[]) item._arrayInObject);
        }

        private void AssertUntypedArray(ItemArrays item
            )
        {
            for (var i = 0; i < data.Length; i++)
            {
                AssertAreEqual(data[i], (StringBuilder) item._untypedArray[i]);
            }
            Assert.IsNull(item._untypedArray[item._untypedArray.Length - 1]);
        }

        private void AssertTypedArray(ItemArrays item)
        {
            AssertData(item._typedArray);
        }

        private void AssertData(StringBuilder[] values)
        {
            for (var i = 0; i < data.Length; i++)
            {
                AssertAreEqual(data[i], values[i]);
            }
        }

        protected override void AssertValues(IExtObjectContainer objectContainer, object[]
            values)
        {
            for (var i = 0; i < data.Length; i++)
            {
                var item = (Item
                    ) values[i];
                AssertAreEqual(data[i], item._typed);
                AssertAreEqual(data[i], (StringBuilder) item._untyped);
            }
            var nullItem = (Item
                ) values[values.Length - 1];
            Assert.IsNull(nullItem._typed);
            Assert.IsNull(nullItem._untyped);
        }

        private void AssertAreEqual(StringBuilder expected, StringBuilder actual)
        {
            var expectedString = (expected == null) ? null : expected.ToString();
            var actualString = (actual == null) ? null : actual.ToString();
            Assert.AreEqual(expectedString, actualString);
        }

        protected override object CreateArrays()
        {
            var item = new ItemArrays
                ();
            CreateTypedArray(item);
            CreateUntypedArray(item);
            CreateArrayInObject(item);
            return item;
        }

        private void CreateArrayInObject(ItemArrays item
            )
        {
            var stringBufferArray = new StringBuilder[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                stringBufferArray[i] = data[i];
            }
            item._arrayInObject = stringBufferArray;
        }

        private void CreateUntypedArray(ItemArrays item
            )
        {
            item._untypedArray = new StringBuilder[data.Length + 1];
            for (var i = 0; i < data.Length; i++)
            {
                item._untypedArray[i] = data[i];
            }
        }

        private void CreateTypedArray(ItemArrays item)
        {
            item._typedArray = new StringBuilder[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                item._typedArray[i] = data[i];
            }
        }

        protected override object[] CreateValues()
        {
            var items = new Item
                [data.Length + 1];
            for (var i = 0; i < data.Length; i++)
            {
                var item = new Item
                    ();
                item._typed = data[i];
                item._untyped = data[i];
                items[i] = item;
            }
            items[items.Length - 1] = new Item();
            return items;
        }

        protected override string TypeName()
        {
            return "StringBuffer";
        }

        public class Item
        {
            public StringBuilder _typed;
            public object _untyped;
        }

        public class ItemArrays
        {
            public object _arrayInObject;
            public StringBuilder[] _typedArray;
            public object[] _untypedArray;
        }
    }
}