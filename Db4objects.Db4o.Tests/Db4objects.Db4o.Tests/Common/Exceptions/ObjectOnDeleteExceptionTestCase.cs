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
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Exceptions
{
    public class ObjectOnDeleteExceptionTestCase : AbstractDb4oTestCase, IOptOutMultiSession
    {
        public static void Main(string[] args)
        {
            new ObjectOnDeleteExceptionTestCase().RunSolo();
        }

        public virtual void Test()
        {
            var item = new Item
                ();
            Store(item);
            Assert.Expect(typeof (ReflectException), typeof (ItemException), new _ICodeBlock_27
                (this, item));
        }

        public class Item
        {
            public virtual bool ObjectOnDelete(IObjectContainer container)
            {
                throw new ItemException();
            }
        }

        private sealed class _ICodeBlock_27 : ICodeBlock
        {
            private readonly ObjectOnDeleteExceptionTestCase _enclosing;
            private readonly Item item;

            public _ICodeBlock_27(ObjectOnDeleteExceptionTestCase _enclosing, Item
                item)
            {
                this._enclosing = _enclosing;
                this.item = item;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.Db().Delete(item);
                _enclosing.Db().Commit();
            }
        }
    }
}