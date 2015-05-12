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
using Db4objects.Db4o.Internal.Handlers.Array;
using Db4objects.Db4o.Typehandlers;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Handlers
{
    public class ArrayHandlerTestCase : AbstractDb4oTestCase
    {
        public static void Main(string[] args)
        {
            new ArrayHandlerTestCase().RunSolo();
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestFloatArrayRoundtrip()
        {
            float[] expected =
            {
                float.MinValue, float.MinValue + 1, 0.0f, float.MaxValue
                                                          - 1,
                float.MaxValue
            };
            Store(new FloatArrayHolder(expected));
            Reopen();
            var stored = ((FloatArrayHolder
                ) RetrieveOnlyInstance(typeof (FloatArrayHolder)));
            ArrayAssert.AreEqual(expected, stored.JaggedFloats());
            ArrayAssert.AreEqual(expected, stored.Floats());
            ArrayAssert.AreEqual(FloatArrayHolder.Lift(expected), stored
                .JaggedWrappers());
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestArraysHaveNoIdentity()
        {
            float[] expected =
            {
                float.MinValue, float.MinValue + 1, 0.0f, float.MaxValue
                                                          - 1,
                float.MaxValue
            };
            Store(new FloatArrayHolder(expected));
            Store(new FloatArrayHolder(expected));
            Reopen();
            var stored = Db().Query(typeof (FloatArrayHolder));
            var first = ((FloatArrayHolder
                ) stored.Next());
            var second = ((FloatArrayHolder
                ) stored.Next());
            Assert.AreNotSame(first._floats, second._floats);
        }

        public virtual void TestHandlerVersion()
        {
            var intArrayHolder = new IntArrayHolder
                (new int[0]);
            Store(intArrayHolder);
            var claxx = Reflector().ForObject(intArrayHolder);
            var classMetadata = Container().ProduceClassMetadata(claxx
                );
            var fieldMetadata = classMetadata.FieldMetadataForName("_ints");
            var arrayHandler = fieldMetadata.GetHandler();
            Assert.IsInstanceOf(typeof (ArrayHandler), arrayHandler);
            AssertCorrectedHandlerVersion(arrayHandler, 0, typeof (ArrayHandler0));
            AssertCorrectedHandlerVersion(arrayHandler, 1, typeof (ArrayHandler1));
            AssertCorrectedHandlerVersion(arrayHandler, 2, typeof (ArrayHandler3));
            AssertCorrectedHandlerVersion(arrayHandler, 3, typeof (ArrayHandler3));
            AssertCorrectedHandlerVersion(arrayHandler, HandlerRegistry.HandlerVersion, typeof (
                ArrayHandler));
        }

        public virtual void TestIntArrayReadWrite()
        {
            var writeContext = new MockWriteContext(Db());
            int[] expected = {7, 8, 9};
            IntArrayHandler().Write(writeContext, expected);
            var readContext = new MockReadContext(writeContext);
            var actual = (int[]) IntArrayHandler().Read(readContext);
            ArrayAssert.AreEqual(expected, actual);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestIntArrayStoreObject()
        {
            var expectedItem = new IntArrayHolder
                (new[] {1, 2, 3});
            Db().Store(expectedItem);
            Db().Purge(expectedItem);
            var readItem = (IntArrayHolder) RetrieveOnlyInstance(typeof (IntArrayHolder
                ));
            Assert.AreNotSame(expectedItem, readItem);
            ArrayAssert.AreEqual(expectedItem._ints, readItem._ints);
            ArrayAssert.AreEqual(expectedItem._ints, readItem.JaggedInts());
        }

        public virtual void TestStringArrayReadWrite()
        {
            var writeContext = new MockWriteContext(Db());
            string[] expected = {"one", "two", "three"};
            StringArrayHandler().Write(writeContext, expected);
            var readContext = new MockReadContext(writeContext);
            var actual = (string[]) StringArrayHandler().Read(readContext);
            ArrayAssert.AreEqual(expected, actual);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestStringArrayStoreObject()
        {
            var expectedItem = new StringArrayHolder
                (new[] {"one", "two", "three"});
            Db().Store(expectedItem);
            Db().Purge(expectedItem);
            var readItem = (StringArrayHolder) RetrieveOnlyInstance(typeof (StringArrayHolder
                ));
            Assert.AreNotSame(expectedItem, readItem);
            ArrayAssert.AreEqual(expectedItem._strings, readItem._strings);
        }

        private ArrayHandler ArrayHandler(Type clazz, bool isPrimitive)
        {
            var classMetadata = Container().ProduceClassMetadata(Reflector().ForClass
                (clazz));
            return new ArrayHandler(classMetadata.TypeHandler(), isPrimitive);
        }

        private void AssertCorrectedHandlerVersion(ITypeHandler4 arrayHandler, int version
            , Type handlerClass)
        {
            var correctedHandlerVersion = Container().Handlers.CorrectHandlerVersion
                (arrayHandler, version);
            Assert.IsInstanceOf(handlerClass, correctedHandlerVersion);
        }

        private ArrayHandler IntArrayHandler()
        {
            return ArrayHandler(typeof (int), true);
        }

        private ArrayHandler StringArrayHandler()
        {
            return ArrayHandler(typeof (string), false);
        }

        public class FloatArrayHolder
        {
            public float[] _floats;
            public float[][] _jaggedFloats;
            public float[][] _jaggedFloatWrappers;

            public FloatArrayHolder()
            {
            }

            public FloatArrayHolder(float[] floats)
            {
                // for jres that require instantiation through the constructor
                _floats = floats;
                _jaggedFloats = new[] {floats};
                _jaggedFloatWrappers = new[] {Lift(floats)};
            }

            public static float[] Lift(float[] floats)
            {
                var wrappers = new float[floats.Length];
                for (var i = 0; i < floats.Length; ++i)
                {
                    wrappers[i] = floats[i];
                }
                return wrappers;
            }

            public virtual float[] Floats()
            {
                return _floats;
            }

            public virtual float[] JaggedFloats()
            {
                return _jaggedFloats[0];
            }

            public virtual float[] JaggedWrappers()
            {
                return _jaggedFloatWrappers[0];
            }
        }

        public class IntArrayHolder
        {
            public int[] _ints;
            public int[][] _jaggedInts;

            public IntArrayHolder(int[] ints)
            {
                _ints = ints;
                _jaggedInts = new[] {_ints};
            }

            public virtual int[] JaggedInts()
            {
                return _jaggedInts[0];
            }
        }

        public class StringArrayHolder
        {
            public string[] _strings;

            public StringArrayHolder(string[] strings)
            {
                _strings = strings;
            }
        }
    }
}