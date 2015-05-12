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

using Db4objects.Db4o.Activation;
using Db4objects.Db4o.Events;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal.Activation;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.TA
{
    public partial class TransparentActivationSupportTestCase : TransparentActivationTestCaseBase
    {
        public static void Main(string[] args)
        {
            new TransparentActivationSupportTestCase().RunAll();
        }

        public virtual void TestActivationDepth()
        {
            Assert.IsInstanceOf(typeof (TransparentActivationDepthProviderImpl), Stream().ConfigImpl
                .ActivationDepthProvider());
        }

        public virtual void TestTransparentActivationDoesNotImplyTransparentUpdate()
        {
            var item = new Item
                ();
            Db().Store(item);
            Db().Commit();
            item.Update();
            var updated = CommitCapturingUpdatedObjects(Db());
            Assert.AreEqual(0, updated.Size());
        }

        private Collection4 CommitCapturingUpdatedObjects(IExtObjectContainer container)
        {
            var updated = new Collection4();
            EventRegistryFor(container).Updated += new _IEventListener4_51(updated).OnEvent;
            container.Commit();
            return updated;
        }

        public sealed partial class Item : ActivatableImpl
        {
            public void Update()
            {
                Activate(ActivationPurpose.Write);
            }
        }

        private sealed class _IEventListener4_51
        {
            private readonly Collection4 updated;

            public _IEventListener4_51(Collection4 updated)
            {
                this.updated = updated;
            }

            public void OnEvent(object sender, ObjectInfoEventArgs args
                )
            {
                ObjectEventArgs objectArgs = args;
                updated.Add(objectArgs.Object);
            }
        }
    }
}