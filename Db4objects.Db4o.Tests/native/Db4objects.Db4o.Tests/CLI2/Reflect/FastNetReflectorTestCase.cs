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

#if !CF
using Db4oUnit;
using Db4objects.Db4o.Internal.Reflect;
#endif
using Db4oUnit;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.CLI2.Reflector
{
    public class FastNetReflectorTestCase : ITestCase, IOptOutSilverlight
    {
#if !CF
        public void TestNullAssignmentToValueTypeField()
        {
            var reflector = new FastNetReflector();
            var field = reflector.ForClass(typeof (ValueTypeContainer)).GetDeclaredField("_value");
            var subject = new ValueTypeContainer(0xDb40);

            field.Set(subject, null);
            Assert.AreEqual(0, subject.Value);

            field.Set(subject, 42);
            Assert.AreEqual(42, subject.Value);
        }

        public void TestNonAccessibleGenericTypeParamenterBugInReflectionEmit()
        {
            var reflector = new FastNetReflector();
            var sizeField = reflector.ForClass(typeof (GenericClass<NotAccessible>)).GetDeclaredField("_size");

            var obj = new GenericClass<NotAccessible>();
            sizeField.Set(obj, 42);
            Assert.AreEqual(42, sizeField.Get(obj));
        }

#if !NET_4_0 //TODO: Investigate why this is failing on .Net 4.0
		public void TestDynamicMethodsOnSecurityCriticalTypes()
		{
			FastNetReflector reflector = new FastNetReflector();
			IReflectField sizeField = reflector.ForClass(typeof(List<NotAccessible>)).GetDeclaredField("_size");

			List<NotAccessible> obj = new List<NotAccessible>();
			sizeField.Set(obj, 42);
			Assert.AreEqual(42, sizeField.Get(obj));
		}
#endif

        internal class ValueTypeContainer
        {
            public ValueTypeContainer(int initialValue)
            {
                Value = initialValue;
            }

            public int Value { get; private set; }
        }

        private class NotAccessible
        {
        }

        private class GenericClass<T>
        {
            private int _size;
        }
#endif
    }
}