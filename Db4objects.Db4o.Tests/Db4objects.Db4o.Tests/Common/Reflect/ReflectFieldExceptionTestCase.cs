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
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Reflect;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Reflect
{
    public class ReflectFieldExceptionTestCase : ITestCase
    {
        public virtual void TestExceptionIsPropagated()
        {
            var reflector = Platform4.ReflectorForType(typeof (Item
                ));
            var field = reflector.ForClass(typeof (Item
                )).GetDeclaredField("_name");
            Assert.Expect(typeof (Db4oException), typeof (ArgumentException), new _ICodeBlock_18
                (field));
        }

        public class Item
        {
            public string _name;
        }

        private sealed class _ICodeBlock_18 : ICodeBlock
        {
            private readonly IReflectField field;

            public _ICodeBlock_18(IReflectField field)
            {
                this.field = field;
            }

            public void Run()
            {
                field.Set(new Item(), 42);
            }
        }
    }
}