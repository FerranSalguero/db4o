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
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Handlers;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Internal
{
    public partial class Comparable4TestCase : AbstractDb4oTestCase, IOptOutMultiSession
    {
        public static void Main(string[] args)
        {
            new Comparable4TestCase().RunAll();
        }

        public virtual void TestHandlers()
        {
            AssertHandlerComparison(typeof (BooleanHandler), false, true);
            AssertHandlerComparison(typeof (ByteHandler), (byte) 1, (byte) 2);
            AssertHandlerComparison(typeof (ByteHandler), byte.MinValue, byte.MaxValue);
            AssertHandlerComparison(typeof (CharHandler), (char) 1, (char) 2);
            AssertHandlerComparison(typeof (CharHandler), char.MinValue, char.MaxValue);
            AssertHandlerComparison(typeof (DoubleHandler), System.Convert.ToDouble(1), System.Convert.ToDouble
                (2));
            AssertHandlerComparison(typeof (DoubleHandler), 0.1, 0.2);
            AssertHandlerComparison(typeof (DoubleHandler), double.MinValue, double.MaxValue);
            AssertHandlerComparison(typeof (FloatHandler), System.Convert.ToSingle(1), System.Convert.ToSingle
                (2));
            AssertHandlerComparison(typeof (FloatHandler), System.Convert.ToSingle(0.1), System.Convert.ToSingle
                (0.2));
            AssertHandlerComparison(typeof (FloatHandler), float.MinValue, float.MaxValue);
            AssertHandlerComparison(typeof (IntHandler), 2, 4);
            AssertHandlerComparison(typeof (IntHandler), int.MinValue, int.MaxValue);
            AssertHandlerComparison(typeof (LongHandler), System.Convert.ToInt64(2), System.Convert.ToInt64
                (4));
            AssertHandlerComparison(typeof (LongHandler), long.MinValue, long.MaxValue);
            AssertHandlerComparison(typeof (ShortHandler), (short) 2, (short) 4);
            AssertHandlerComparison(typeof (ShortHandler), short.MinValue, short.MaxValue);
            AssertHandlerComparison(typeof (StringHandler), "a", "b");
            AssertHandlerComparison(typeof (StringHandler), "Hello", "Hello_");
            AssertClassHandler();
        }

        private void AssertClassHandler()
        {
            var id1 = StoreItem();
            var id2 = StoreItem();
            var smallerID = Math.Min(id1, id2);
            var biggerID = Math.Max(id1, id2);
            var classMetadata = new ClassMetadata(Container(), Reflector().ForClass
                (typeof (Item)));
            AssertHandlerComparison((IComparable4) classMetadata.TypeHandler(), smallerID, biggerID
                );
        }

        private int StoreItem()
        {
            var item = new Item();
            Db().Store(item);
            return (int) Db().GetID(item);
        }

        private void AssertHandlerComparison(Type handlerClass, object smaller, object greater
            )
        {
            var handler = (IComparable4) NewInstance(handlerClass);
            AssertHandlerComparison(handler, smaller, greater);
        }

        private void AssertHandlerComparison(IComparable4 handler, object smaller, object
            greater)
        {
            var comparable = handler.PrepareComparison(Context(), smaller);
            Assert.IsNotNull(comparable);
            Assert.AreEqual(0, comparable.CompareTo(smaller));
            Assert.IsSmaller(0, comparable.CompareTo(greater));
            Assert.IsGreater(0, comparable.CompareTo(null));
            comparable = handler.PrepareComparison(Context(), greater);
            Assert.IsNotNull(comparable);
            Assert.AreEqual(0, comparable.CompareTo(greater));
            Assert.IsGreater(0, comparable.CompareTo(smaller));
            Assert.IsGreater(0, comparable.CompareTo(null));
            comparable = handler.PrepareComparison(Context(), null);
            Assert.IsNotNull(comparable);
            Assert.AreEqual(0, comparable.CompareTo(null));
            Assert.IsSmaller(0, comparable.CompareTo(smaller));
        }

        private object NewInstance(Type clazz)
        {
            var classReflector = Reflector().ForClass(clazz);
            var obj = classReflector.NewInstance();
            if (obj == null)
            {
                throw new ArgumentException("No usable constructor for Class " + clazz);
            }
            return obj;
        }

        public class Item
        {
        }
    }
}