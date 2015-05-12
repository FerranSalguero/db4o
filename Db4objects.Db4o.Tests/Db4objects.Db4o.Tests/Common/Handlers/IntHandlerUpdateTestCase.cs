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
    public class IntHandlerUpdateTestCase : HandlerUpdateTestCaseBase
    {
        private readonly int[] data;

        public IntHandlerUpdateTestCase()
        {
            data = new[]
            {
                int.MinValue, int.MinValue + 1, -5, -1, 0, 1, 5, int.MaxValue
                                                                 - 1,
                UsesNullMarkerValue() ? 0 : int.MaxValue
            };
        }

        public static void Main(string[] args)
        {
            new ConsoleTestRunner(typeof (IntHandlerUpdateTestCase
                )).Run();
        }

        protected override string TypeName()
        {
            return "int";
        }

        protected override object[] CreateValues()
        {
            var values = new Item[data.Length
                                  + 1];
            for (var i = 0; i < data.Length; i++)
            {
                var item = new Item();
                values[i] = item;
                item._typedPrimitive = data[i];
                item._typedWrapper = data[i];
                item._untyped = data[i];
            }
            values[values.Length - 1] = new Item();
            return values;
        }

        protected override object CreateArrays()
        {
            var item = new ItemArrays
                ();
            CreateTypedPrimitiveArray(item);
            CreateTypedWrapperArray(item);
            // Will be removed for .NET by sharpen.
            CreatePrimitiveArrayInObject(item);
            CreateWrapperArrayInObject(item);
            return item;
        }

        private void CreateTypedPrimitiveArray(ItemArrays item)
        {
            item._typedPrimitiveArray = new int[data.Length];
            Array.Copy(data, 0, item._typedPrimitiveArray, 0, data.Length);
        }

        private void CreateTypedWrapperArray(ItemArrays item)
        {
            item._typedWrapperArray = new int[data.Length + 1];
            for (var i = 0; i < data.Length; i++)
            {
                item._typedWrapperArray[i] = data[i];
            }
        }

        private void CreatePrimitiveArrayInObject(ItemArrays item
            )
        {
            var arr = new int[data.Length];
            Array.Copy(data, 0, arr, 0, data.Length);
            item._primitiveArrayInObject = arr;
        }

        private void CreateWrapperArrayInObject(ItemArrays item)
        {
            var arr = new int[data.Length + 1];
            for (var i = 0; i < data.Length; i++)
            {
                arr[i] = data[i];
            }
            item._wrapperArrayInObject = arr;
        }

        protected override void AssertValues(IExtObjectContainer objectContainer, object[]
            values)
        {
            for (var i = 0; i < data.Length; i++)
            {
                var item = (Item) values[i];
                AssertAreEqual(data[i], item._typedPrimitive);
                AssertAreEqual(data[i], item._typedWrapper);
                AssertAreEqual(data[i], item._untyped);
            }
            var nullItem = (Item) values[values
                .Length - 1];
            Assert.AreEqual(0, nullItem._typedPrimitive);
            Assert.IsNull(nullItem._untyped);
        }

        protected override void AssertArrays(IExtObjectContainer objectContainer, object
            obj)
        {
            var item = (ItemArrays) obj;
            AssertTypedPrimitiveArray(item);
            AssertTypedWrapperArray(item);
            // Will be removed for .NET by sharpen.
            AssertPrimitiveArrayInObject(item);
            AssertWrapperArrayInObject(item);
        }

        private void AssertTypedPrimitiveArray(ItemArrays item)
        {
            AssertData(item._typedPrimitiveArray);
        }

        private void AssertTypedWrapperArray(ItemArrays item)
        {
            AssertWrapperData(item._typedWrapperArray);
        }

        private void AssertPrimitiveArrayInObject(ItemArrays item
            )
        {
            AssertData(CastToIntArray(item._primitiveArrayInObject));
        }

        private void AssertWrapperArrayInObject(ItemArrays item)
        {
            AssertWrapperData((int[]) item._wrapperArrayInObject);
        }

        private void AssertData(int[] values)
        {
            for (var i = 0; i < data.Length; i++)
            {
                AssertAreEqual(data[i], values[i]);
            }
        }

        private void AssertWrapperData(int[] values)
        {
            for (var i = 0; i < data.Length; i++)
            {
                AssertAreEqual(data[i], values[i]);
            }
        }

        // FIXME: The following fails as is because of a deficiency 
        //        in the storage format of arrays.
        //        Arrays should also get a null Bitmap to fix.
        // Assert.isNull(values[data.length]);
        private void AssertAreEqual(int expected, int actual)
        {
            if (expected == int.MaxValue && Db4oHandlerVersion() == 0)
            {
                // Bug in the oldest format: It treats Integer.MAX_VALUE as null. 
                expected = 0;
            }
            Assert.AreEqual(expected, actual);
        }

        private void AssertAreEqual(object expected, object actual)
        {
            if (int.MaxValue.Equals(expected) && Db4oHandlerVersion() == 0)
            {
                // Bug in the oldest format: It treats Integer.MAX_VALUE as null.
                expected = null;
            }
            Assert.AreEqual(expected, actual);
        }

        public class Item
        {
            public int _typedPrimitive;
            public int _typedWrapper;
            public object _untyped;
        }

        public class ItemArrays
        {
            public object _primitiveArrayInObject;
            public int[] _typedPrimitiveArray;
            public int[] _typedWrapperArray;
            public object[] _untypedObjectArray;
            public object _wrapperArrayInObject;
        }
    }
}