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
using Db4objects.Db4o.Internal.References;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.References
{
    public class ReferenceSystemRegistryTestCase : ITestLifeCycle
    {
        private static readonly int TestId = 5;
        private IReferenceSystem _referenceSystem1;
        private IReferenceSystem _referenceSystem2;
        private ReferenceSystemRegistry _registry;

        /// <exception cref="System.Exception"></exception>
        public virtual void SetUp()
        {
            _registry = new ReferenceSystemRegistry();
            _referenceSystem1 = new TransactionalReferenceSystem();
            _referenceSystem2 = new TransactionalReferenceSystem();
            _registry.AddReferenceSystem(_referenceSystem1);
            _registry.AddReferenceSystem(_referenceSystem2);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TearDown()
        {
        }

        public virtual void TestRemoveId()
        {
            AddTestReferenceToBothSystems();
            _registry.RemoveId(TestId);
            AssertTestReferenceNotPresent();
        }

        public virtual void TestRemoveNull()
        {
            _registry.RemoveObject(null);
        }

        public virtual void TestRemoveObject()
        {
            var testReference = AddTestReferenceToBothSystems();
            _registry.RemoveObject(testReference.GetObject());
            AssertTestReferenceNotPresent();
        }

        public virtual void TestRemoveReference()
        {
            var testReference = AddTestReferenceToBothSystems();
            _registry.RemoveReference(testReference);
            AssertTestReferenceNotPresent();
        }

        public virtual void TestRemoveReferenceSystem()
        {
            AddTestReferenceToBothSystems();
            _registry.RemoveReferenceSystem(_referenceSystem1);
            _registry.RemoveId(TestId);
            Assert.IsNotNull(_referenceSystem1.ReferenceForId(TestId));
            Assert.IsNull(_referenceSystem2.ReferenceForId(TestId));
        }

        public virtual void TestRemoveByObjectReference()
        {
            var ref1 = NewObjectReference();
            _referenceSystem1.AddExistingReference(ref1);
            var ref2 = NewObjectReference();
            _referenceSystem2.AddExistingReference(ref2);
            _registry.RemoveReference(ref2);
            Assert.IsNotNull(_referenceSystem1.ReferenceForId(TestId));
            Assert.IsNull(_referenceSystem2.ReferenceForId(TestId));
        }

        private void AssertTestReferenceNotPresent()
        {
            Assert.IsNull(_referenceSystem1.ReferenceForId(TestId));
            Assert.IsNull(_referenceSystem2.ReferenceForId(TestId));
        }

        private ObjectReference AddTestReferenceToBothSystems()
        {
            var @ref = NewObjectReference();
            _referenceSystem1.AddExistingReference(@ref);
            _referenceSystem2.AddExistingReference(@ref);
            return @ref;
        }

        private ObjectReference NewObjectReference()
        {
            var @ref = new ObjectReference(TestId);
            @ref.SetObject(new object());
            return @ref;
        }
    }
}