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

using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.References;
using Db4oUnit;
using Db4oUnit.Extensions;
using Sharpen;
using Sharpen.Lang;

namespace Db4objects.Db4o.Tests.Common.References
{
    public class WeakReferenceCollectionTestCase : AbstractDb4oTestCase
    {
        //COR-1839
#if !SILVERLIGHT
        /// <exception cref="System.Exception"></exception>
        public virtual void Test()
        {
            if (!Platform4.HasWeakReferences())
            {
                return;
            }
            var item = new Item
                ();
            Store(item);
            Commit();
            var reference = new ByRef();
            ReferenceSystem().TraverseReferences(new _IVisitor4_30(reference));
            Assert.IsNotNull(((ObjectReference) reference.value));
            item = null;
            long timeout = 10000;
            var startTime = Runtime.CurrentTimeMillis();
            while (true)
            {
                var currentTime = Runtime.CurrentTimeMillis();
                if (currentTime - startTime >= timeout)
                {
                    Assert.Fail("Timeout waiting for WeakReference collection.");
                }
                Runtime.Gc();
                Runtime.RunFinalization();
                Thread.Sleep(1);
                if (((ObjectReference) reference.value).GetObject() == null)
                {
                    break;
                }
            }
            startTime = Runtime.CurrentTimeMillis();
            while (true)
            {
                var currentTime = Runtime.CurrentTimeMillis();
                if (currentTime - startTime >= timeout)
                {
                    Assert.Fail("Timeout waiting for removal of ObjectReference from ReferenceSystem."
                        );
                }
                var found = new BooleanByRef();
                ReferenceSystem().TraverseReferences(new _IVisitor4_63(reference, found));
                if (!found.value)
                {
                    return;
                }
                Thread.Sleep(10);
            }
        }
#endif // !SILVERLIGHT

        private IReferenceSystem ReferenceSystem()
        {
            return Trans().ReferenceSystem();
        }

        public class Item
        {
        }

        private sealed class _IVisitor4_30 : IVisitor4
        {
            private readonly ByRef reference;

            public _IVisitor4_30(ByRef reference)
            {
                this.reference = reference;
            }

            public void Visit(object @ref)
            {
                if (((ObjectReference) @ref).GetObject() is Item)
                {
                    reference.value = ((ObjectReference) @ref);
                }
            }
        }

        private sealed class _IVisitor4_63 : IVisitor4
        {
            private readonly BooleanByRef found;
            private readonly ByRef reference;

            public _IVisitor4_63(ByRef reference, BooleanByRef found)
            {
                this.reference = reference;
                this.found = found;
            }

            public void Visit(object @ref)
            {
                if (((ObjectReference) @ref) == ((ObjectReference) reference.value))
                {
                    found.value = true;
                }
            }
        }
    }
}