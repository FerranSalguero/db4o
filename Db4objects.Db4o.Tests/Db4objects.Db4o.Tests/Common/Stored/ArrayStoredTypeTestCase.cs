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
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Util;

namespace Db4objects.Db4o.Tests.Common.Stored
{
    public class ArrayStoredTypeTestCase : AbstractDb4oTestCase
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            var data = new Data(new[]
            {
                true, false
            }, new[] {true, false}, new[] {0, 1, 2}, new[]
            {
                4,
                5, 6
            });
            Store(data);
        }

        public virtual void TestArrayStoredTypes()
        {
            var clazz = Db().StoredClass(typeof (Data));
            AssertStoredType(clazz, "_primitiveBoolean", typeof (bool));
            AssertStoredType(clazz, "_wrapperBoolean", typeof (bool));
            AssertStoredType(clazz, "_primitiveInt", typeof (int));
            AssertStoredType(clazz, "_wrapperInteger", typeof (int));
        }

        private void AssertStoredType(IStoredClass clazz, string fieldName, Type type)
        {
            var field = clazz.StoredField(fieldName, null);
            Assert.AreEqual(type.FullName, CrossPlatformServices.SimpleName(field.GetStoredType
                ().GetName()));
        }

        public class Data
        {
            public bool[] _primitiveBoolean;
            public int[] _primitiveInt;
            public bool[] _wrapperBoolean;
            public int[] _wrapperInteger;

            public Data(bool[] primitiveBoolean, bool[] wrapperBoolean, int[] primitiveInteger
                , int[] wrapperInteger)
            {
                _primitiveBoolean = primitiveBoolean;
                _wrapperBoolean = wrapperBoolean;
                _primitiveInt = primitiveInteger;
                _wrapperInteger = wrapperInteger;
            }
        }
    }
}