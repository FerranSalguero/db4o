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
using Db4objects.Db4o.Activation;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Delete;
using Db4objects.Db4o.Marshall;
using Db4objects.Db4o.TA;
using Db4objects.Db4o.Typehandlers;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.TA
{
    public class ReentrantActivationTestCase : AbstractDb4oTestCase
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.Add(new TransparentPersistenceSupport());
            config.RegisterTypeHandler(new SingleClassTypeHandlerPredicate(typeof (ReentratActivatableItem
                )), new ReentrantActivationTypeHandler());
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(new ReentratActivatableItem());
        }

        public virtual void Test()
        {
            var item = ((ReentratActivatableItem
                ) RetrieveOnlyInstance(typeof (ReentratActivatableItem
                    )));
            Assert.IsFalse(item.Activated());
            item.ActivateForRead();
            Assert.IsTrue(item.Activated());
            AssertNotActivatedForWrite(item);
        }

        private void AssertNotActivatedForWrite(ReentratActivatableItem
            item)
        {
            Commit();
            Assert.IsFalse(item.Written());
        }

        public class ReentratActivatableItem : IActivatable
        {
            [NonSerialized] private bool _activated;

            private IActivator _activator;

            [NonSerialized] private bool _written;

            public virtual void Activate(ActivationPurpose purpose)
            {
                _activator.Activate(purpose);
                _activated = true;
            }

            public virtual void Bind(IActivator activator)
            {
                _activator = activator;
            }

            public virtual void ObjectOnUpdate(IObjectContainer container)
            {
                _written = true;
            }

            public virtual void ActivateForRead()
            {
                Activate(ActivationPurpose.Read);
            }

            public virtual void ActivateForWrite()
            {
                Activate(ActivationPurpose.Write);
            }

            public virtual bool Activated()
            {
                return _activated;
            }

            public virtual bool Written()
            {
                return _written;
            }
        }

        public class ReentrantActivationTypeHandler : IReferenceTypeHandler
        {
            public virtual void Activate(IReferenceActivationContext context)
            {
                var item = (ReentratActivatableItem
                    ) context.PersistentObject();
                item.ActivateForWrite();
            }

            public virtual void Defragment(IDefragmentContext context)
            {
            }

            /// <exception cref="Db4objects.Db4o.Ext.Db4oIOException"></exception>
            public virtual void Delete(IDeleteContext context)
            {
            }

            public virtual void Write(IWriteContext context, object obj)
            {
            }
        }
    }
}