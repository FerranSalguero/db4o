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

#if !SILVERLIGHT
using System;
using System.Reflection;
using Db4objects.Db4o.Qlin;
using Db4oUnit;
using Sharpen;

namespace Db4objects.Db4o.Tests.Common.Qlin
{
    public class PuzzleTypesafeFieldObject : ITestCase
    {
        private const bool IgnoreTransientFields = true;
        private const int RecursionDepth = 10;
        private static Prototypes _prototypes = new Prototypes();

        public virtual void TestTypeSafeFieldAsObject()
        {
            var cat = ((Cat) Prototype(typeof (
                Cat)));
            var nameField = Field(cat, cat.name);
        }

        private object Prototype(Type clazz)
        {
            return _prototypes.PrototypeForClass(clazz);
        }

        public static FieldInfo Field(object onObject, object expression)
        {
            var clazz = onObject.GetType();
            var path = _prototypes.BackingFieldPath(onObject.GetType(), expression);
            path.MoveNext();
            Runtime.Out.WriteLine(((string) path.Current));
            return null;
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void SetUp()
        {
            _prototypes = new Prototypes(Prototypes.DefaultReflector(), RecursionDepth, IgnoreTransientFields
                );
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TearDown()
        {
        }

        public class Cat
        {
            public string name;

            public Cat(string name)
            {
                this.name = name;
            }
        }
    }
}

#endif // !SILVERLIGHT