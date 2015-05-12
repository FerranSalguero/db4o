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
using Db4objects.Db4o.Events;
using Db4objects.Db4o.TA;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.TA.Events
{
    public class ActivationEventsTestCase : TransparentActivationTestCaseBase
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            var nonActivatable = new NonActivatableItem
                ("Eric Idle");
            Store(nonActivatable);
            Store(new ActivatableItem("John Cleese", nonActivatable)
                );
        }

        public virtual void TestActivatingCancelNonActivatableDepth0()
        {
            AddCancelAnyListener();
            var item = QueryNonActivatableItem();
            Assert.IsNull(item.name);
        }

        public virtual void TestActivatingCancelActivatableDepth0()
        {
            AddCancelAnyListener();
            var item = QueryActivatableItem();
            item.Activate(ActivationPurpose.Read);
            Assert.IsNull(item.name);
        }

        public virtual void TestActivatingCancelDepth1()
        {
            AddCancelNonActivatableListener();
            var item = QueryActivatableItem();
            item.Activate(ActivationPurpose.Read);
            Assert.IsNotNull(item.name);
            Assert.IsNotNull(item.child);
            Assert.IsNull(item.child.name);
        }

        private void AddCancelNonActivatableListener()
        {
            EventRegistry().Activating += new _IEventListener4_79().OnEvent;
        }

        private void AddCancelAnyListener()
        {
            EventRegistry().Activating += new _IEventListener4_90().OnEvent;
        }

        private NonActivatableItem QueryNonActivatableItem()
        {
            return (NonActivatableItem
                ) RetrieveOnlyInstance(typeof (NonActivatableItem));
        }

        private ActivatableItem QueryActivatableItem()
        {
            return (ActivatableItem
                ) RetrieveOnlyInstance(typeof (ActivatableItem));
        }

        public class NonActivatableItem
        {
            public string name;

            public NonActivatableItem(string name_)
            {
                name = name_;
            }

            public NonActivatableItem()
            {
            }
        }

        public class ActivatableItem : IActivatable
        {
            [NonSerialized] private IActivator _activator;

            public NonActivatableItem child;
            public string name;

            public ActivatableItem(string name_, NonActivatableItem
                child_)
            {
                name = name_;
                child = child_;
            }

            public ActivatableItem()
            {
            }

            public virtual void Activate(ActivationPurpose purpose)
            {
                _activator.Activate(purpose);
            }

            public virtual void Bind(IActivator activator)
            {
                _activator = activator;
            }
        }

        private sealed class _IEventListener4_79
        {
            public void OnEvent(object sender, CancellableObjectEventArgs
                args)
            {
                var obj = args.Object;
                if (obj is NonActivatableItem)
                {
                    ((ICancellableEventArgs) args).Cancel();
                }
            }
        }

        private sealed class _IEventListener4_90
        {
            public void OnEvent(object sender, CancellableObjectEventArgs
                args)
            {
                ((ICancellableEventArgs) args).Cancel();
            }
        }
    }
}