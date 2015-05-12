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
using Db4objects.Db4o.Internal.Handlers.Array;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Handlers
{
    public class MultiDimensionalArrayHandlerTestCase : TypeHandlerTestCaseBase
    {
        internal static readonly int[][] ArrayData =
        {
            new[] {1, 2, 3},
            new[] {6, 5, 4}
        };

        internal static readonly int[] Data = {1, 2, 3, 6, 5, 4};

        public static void Main(string[] args)
        {
            new MultiDimensionalArrayHandlerTestCase().RunSolo();
        }

        private ArrayHandler IntArrayHandler()
        {
            return ArrayHandler(typeof (int), true);
        }

        private ArrayHandler ArrayHandler(Type clazz, bool isPrimitive)
        {
            var typeHandler = Container().TypeHandlerForClass(Reflector
                ().ForClass(clazz));
            return new MultidimensionalArrayHandler(typeHandler, isPrimitive);
        }

        public virtual void TestReadWrite()
        {
            var writeContext = new MockWriteContext(Db());
            var expected = new Item
                (ArrayData);
            IntArrayHandler().Write(writeContext, expected._int);
            var readContext = new MockReadContext(writeContext);
            var arr = (int[][]) IntArrayHandler().Read(readContext);
            var actualValue = new Item
                (arr);
            Assert.AreEqual(expected, actualValue);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestStoreObject()
        {
            var storedItem = new Item
                (new[] {new[] {1, 2, 3}, new[] {6, 5, 4}});
            DoTestStoreObject(storedItem);
        }

        public virtual void TestAllElements()
        {
            var pos = 0;
            var allElements = IntArrayHandler().AllElements(Container(), ArrayData);
            while (allElements.MoveNext())
            {
                Assert.AreEqual(Data[pos++], allElements.Current);
            }
            Assert.AreEqual(pos, Data.Length);
        }

        public class Item
        {
            public int[][] _int;

            public Item(int[][] int_)
            {
                _int = int_;
            }

            public override bool Equals(object obj)
            {
                if (obj == this)
                {
                    return true;
                }
                if (!(obj is Item))
                {
                    return false;
                }
                var other = (Item
                    ) obj;
                if (_int.Length != other._int.Length)
                {
                    return false;
                }
                for (var i = 0; i < _int.Length; i++)
                {
                    if (_int[i].Length != other._int[i].Length)
                    {
                        return false;
                    }
                    for (var j = 0; j < _int[i].Length; j++)
                    {
                        if (_int[i][j] != other._int[i][j])
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }
    }
}