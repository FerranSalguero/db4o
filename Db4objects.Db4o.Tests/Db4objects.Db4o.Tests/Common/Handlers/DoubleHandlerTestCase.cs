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

using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Handlers;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Handlers
{
    /// <exclude></exclude>
    public class DoubleHandlerTestCase : TypeHandlerTestCaseBase
    {
        private IIndexable4 _handler;

        public static void Main(string[] args)
        {
            new DoubleHandlerTestCase().RunSolo();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Db4oSetupBeforeStore()
        {
            _handler = new DoubleHandler();
        }

        public virtual void TestMarshalling()
        {
            var expected = 1.1;
            var buffer = new ByteArrayBuffer(_handler.LinkLength());
            _handler.WriteIndexEntry(Context(), buffer, expected);
            buffer.Seek(0);
            var actual = _handler.ReadIndexEntry(Context(), buffer);
            Assert.AreEqual(expected, actual);
        }

        public virtual void TestComparison()
        {
            AssertComparison(0, 1.1, 1.1);
            AssertComparison(-1, 1.0, 1.1);
            AssertComparison(1, 1.1, 0.5);
        }

        private void AssertComparison(int expected, double prepareWith, double compareTo)
        {
            var preparedComparison = _handler.PrepareComparison(Stream().Transaction
                .Context(), prepareWith);
            var doubleCompareTo = compareTo;
            Assert.AreEqual(expected, preparedComparison.CompareTo(doubleCompareTo));
        }

        public virtual void TestReadWrite()
        {
            var writeContext = new MockWriteContext(Db());
            var doubleHandler = (DoubleHandler) _handler;
            var expected = 1.23456789;
            doubleHandler.Write(writeContext, expected);
            var readContext = new MockReadContext(writeContext);
            var d = (double) doubleHandler.Read(readContext);
            Assert.AreEqual(expected, d);
        }

        public virtual void TestStoreObject()
        {
            var storedItem = new Item(1.023456789
                , 1.023456789);
            DoTestStoreObject(storedItem);
        }

        public class Item
        {
            public double _double;
            public double _doubleWrapper;

            public Item(double d, double wrapper)
            {
                _double = d;
                _doubleWrapper = wrapper;
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
                var other = (Item) obj;
                return (other._double == _double) && _doubleWrapper.Equals(other._doubleWrapper
                    );
            }

            public override string ToString()
            {
                return "[" + _double + "," + _doubleWrapper + "]";
            }
        }
    }
}