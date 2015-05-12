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
using Db4objects.Db4o.Ext;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Handlers
{
    public class StringHandlerUpdateTestCase : HandlerUpdateTestCaseBase
    {
        private static readonly string[] Data =
        {
            "one", "aAzZ|!Â§$%&/()=?ÃŸÃ¶Ã¤Ã¼Ã„Ã–ÃœYZ;:-_+*~#^Â°'@"
            , string.Empty, null
        };

        public static void Main(string[] args)
        {
            new ConsoleTestRunner(typeof (StringHandlerUpdateTestCase)).Run();
        }

        protected override string TypeName()
        {
            return "string";
        }

        protected override object[] CreateValues()
        {
            var values = new Item[
                Data.Length + 1];
            for (var i = 0; i < Data.Length; i++)
            {
                var item = new Item();
                values[i] = item;
                item._typed = Data[i];
                item._untyped = Data[i];
            }
            values[values.Length - 1] = new Item();
            return values;
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

        private void CreateUntypedArray(ItemArrays item)
        {
            item._untypedArray = new string[Data.Length + 1];
            for (var i = 0; i < Data.Length; i++)
            {
                item._untypedArray[i] = Data[i];
            }
        }

        private void CreateTypedArray(ItemArrays item)
        {
            item._typedArray = new string[Data.Length];
            Array.Copy(Data, 0, item._typedArray, 0, Data.Length);
        }

        private void CreateArrayInObject(ItemArrays item)
        {
            var arr = new string[Data.Length];
            Array.Copy(Data, 0, arr, 0, Data.Length);
            item._arrayInObject = arr;
        }

        protected override void AssertValues(IExtObjectContainer objectContainer, object[]
            values)
        {
            for (var i = 0; i < Data.Length; i++)
            {
                var item = (Item) values[
                    i];
                AssertAreEqual(Data[i], item._typed);
                AssertAreEqual(Data[i], (string) item._untyped);
            }
            var nullItem = (Item) values
                [values.Length - 1];
            Assert.IsNull(nullItem._typed);
            Assert.IsNull(nullItem._untyped);
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

        private void AssertTypedArray(ItemArrays item)
        {
            AssertData(item._typedArray);
        }

        protected virtual void AssertUntypedArray(ItemArrays
            item)
        {
            for (var i = 0; i < Data.Length; i++)
            {
                AssertAreEqual(Data[i], (string) item._untypedArray[i]);
            }
            Assert.IsNull(item._untypedArray[item._untypedArray.Length - 1]);
        }

        private void AssertArrayInObject(ItemArrays item)
        {
            AssertData((string[]) item._arrayInObject);
        }

        private void AssertData(string[] values)
        {
            for (var i = 0; i < Data.Length; i++)
            {
                AssertAreEqual(Data[i], values[i]);
            }
        }

        private void AssertAreEqual(string expected, string actual)
        {
            Assert.AreEqual(expected, actual);
        }

        public class Item
        {
            public string _typed;
            public object _untyped;
        }

        public class ItemArrays
        {
            public object _arrayInObject;
            public string[] _typedArray;
            public object[] _untypedArray;
        }
    }
}