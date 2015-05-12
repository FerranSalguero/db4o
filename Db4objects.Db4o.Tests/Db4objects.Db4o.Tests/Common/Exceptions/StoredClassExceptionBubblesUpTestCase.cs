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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Internal;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Exceptions
{
    public class StoredClassExceptionBubblesUpTestCase : AbstractDb4oTestCase, IOptOutMultiSession
        , IOptOutDefragSolo
    {
        public static void Main(string[] args)
        {
            new StoredClassExceptionBubblesUpTestCase().RunSolo();
        }

        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(typeof (Item)).Translate(new ItemTranslator
                ());
        }

        public virtual void Test()
        {
            Assert.Expect(typeof (ReflectException), typeof (ItemException), new _ICodeBlock_43
                (this));
        }

        public sealed class ItemTranslator : IObjectTranslator
        {
            public void OnActivate(IObjectContainer container, object applicationObject, object
                storedObject)
            {
            }

            public object OnStore(IObjectContainer container, object applicationObject)
            {
                return null;
            }

            public Type StoredClass()
            {
                throw new ItemException();
            }
        }

        private sealed class _ICodeBlock_43 : ICodeBlock
        {
            private readonly StoredClassExceptionBubblesUpTestCase _enclosing;

            public _ICodeBlock_43(StoredClassExceptionBubblesUpTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.Store(new Item());
            }
        }
    }
}