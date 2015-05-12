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

using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4oUnit;
using Db4oUnit.Util;

namespace Db4objects.Db4o.Tests.Common.Handlers
{
    /// <exclude></exclude>
    public class MultiDimensionalArrayHandlerUpdateTestCase : HandlerUpdateTestCaseBase
    {
        public static readonly int[][] intData2D = {new[] {1, 2, 3}, new[] {4, 5, 6}};

        public static readonly string[][] stringData2D =
        {
            new[]
            {
                "one", "two"
            },
            new[] {"three", "four"}
        };

        public static readonly object[][] objectData2D =
        {
            new object[]
            {
                new Item("one"), null, new Item
                    ("two")
            },
            new object[]
            {
                new Item("three"
                    ),
                new Item("four"), null
            }
        };

        public static readonly object[][] stringObjectData2D =
        {
            new object
                [] {"one", "two"},
            new object[] {"three", "four"}
        };

        public static readonly byte[][] byteData2D =
        {
            ByteHandlerUpdateTestCase
                .data,
            ByteHandlerUpdateTestCase.data
        };

        protected override object CreateArrays()
        {
            var item = new ItemArrays
                ();
            if (MultiDimensionalArraysCantBeStored())
            {
                return item;
            }
            item._typedIntArray = intData2D;
            item._untypedIntArray = intData2D;
            item._typedStringArray = stringData2D;
            item._untypedStringArray = stringData2D;
            item._objectArray = objectData2D;
            item._stringObjectArray = stringObjectData2D;
            item._typedByteArray = byteData2D;
            return item;
        }

        protected override void AssertArrays(IExtObjectContainer objectContainer, object
            obj)
        {
            if (MultiDimensionalArraysCantBeStored())
            {
                return;
            }
            var item = (ItemArrays
                ) obj;
            AssertAreEqual(intData2D, item._typedIntArray);
            AssertAreEqual(intData2D, CastToIntArray2D(item._untypedIntArray));
            AssertAreEqual(stringData2D, item._typedStringArray);
            AssertAreEqual(stringData2D, (string[][]) item._untypedStringArray);
            AssertAreEqual(objectData2D, item._objectArray);
            AssertAreEqual(objectData2D, item._objectArray);
            AssertAreEqual(byteData2D, item._typedByteArray);
        }

        private bool MultiDimensionalArraysCantBeStored()
        {
            return PlatformInformation.IsDotNet() && (Db4oMajorVersion() < 6);
        }

        public static void AssertAreEqual(int[][] expected, int[][] actual)
        {
            Assert.AreEqual(expected.Length, actual.Length);
            for (var i = 0; i < expected.Length; i++)
            {
                ArrayAssert.AreEqual(expected[i], actual[i]);
            }
        }

        public static void AssertAreEqual(string[][] expected, string[][] actual)
        {
            Assert.AreEqual(expected.Length, actual.Length);
            for (var i = 0; i < expected.Length; i++)
            {
                ArrayAssert.AreEqual(expected[i], actual[i]);
            }
        }

        public static void AssertAreEqual(object[][] expected, object[][] actual)
        {
            Assert.AreEqual(expected.Length, actual.Length);
            for (var i = 0; i < expected.Length; i++)
            {
                ArrayAssert.AreEqual(expected[i], actual[i]);
            }
        }

        protected virtual int[][] CastToIntArray2D(object obj)
        {
            var byRef = new ObjectByRef(obj);
            return (int[][]) byRef.value;
        }

        public static void AssertAreEqual(byte[][] expected, byte[][] actual)
        {
            Assert.AreEqual(expected.Length, actual.Length);
            for (var i = 0; i < expected.Length; i++)
            {
                ArrayAssert.AreEqual(expected[i], actual[i]);
            }
        }

        // Bug in the oldest format: 
        // It accidentally converted int[][] arrays to Integer[][] arrays.
        protected override object[] CreateValues()
        {
            // not used
            return null;
        }

        protected override void AssertValues(IExtObjectContainer objectContainer, object[]
            values)
        {
        }

        // not used
        protected override string TypeName()
        {
            return "multidimensional_array";
        }

        public class ItemArrays
        {
            public object[][] _objectArray;
            public object[][] _stringObjectArray;
            public byte[][] _typedByteArray;
            public int[][] _typedIntArray;
            public string[][] _typedStringArray;
            public object _untypedIntArray;
            public object _untypedStringArray;
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
                var other = (Item
                    ) obj;
                if (_name == null)
                {
                    return other._name == null;
                }
                return _name.Equals(other._name);
            }
        }
    }
}