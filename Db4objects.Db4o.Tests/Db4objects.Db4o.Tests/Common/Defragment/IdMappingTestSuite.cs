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
using System.Collections;
using Db4objects.Db4o.Defragment;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Ids;
using Db4objects.Db4o.Internal.Slots;
using Db4objects.Db4o.Tests.Common.Api;
using Db4oUnit;
using Db4oUnit.Extensions.Fixtures;
using Db4oUnit.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Defragment
{
    public partial class IdMappingTestSuite : FixtureBasedTestSuite
    {
        private static readonly FixtureVariable _fixture = FixtureVariable.NewInstance("IdMapping"
            );

        public static void Main(string[] args)
        {
            new ConsoleTestRunner(typeof (IdMappingTestSuite)).Run();
        }

        public override Type[] TestUnits()
        {
            return new[] {typeof (IdMappingTestCase)};
        }

        public class IdMappingTestCase : TestWithTempFile, IOptOutMultiSession
        {
            private IIdMapping _idMapping;

            /// <exception cref="System.Exception"></exception>
            public override void SetUp()
            {
                _idMapping = ((IIdMapping) ((IFunction4) _fixture.Value).Apply(TempFile()));
                _idMapping.Open();
            }

            /// <exception cref="System.Exception"></exception>
            public override void TearDown()
            {
                _idMapping.Close();
                base.TearDown();
            }

            public virtual void TestSimpleMapping()
            {
                AssertMapping(true);
                AssertMapping(false);
            }

            private void AssertMapping(bool useClassId)
            {
                _idMapping.MapId(1, 2, useClassId);
                var mappedId = _idMapping.MappedId(1);
                Assert.AreEqual(2, mappedId);
            }

            public virtual void TestSlotMapping()
            {
                IList expected = new ArrayList();
                expected.Add(new TestableIdSlotMapping(1, 10, 100));
                expected.Add(new TestableIdSlotMapping(4, 44, 400));
                expected.Add(new TestableIdSlotMapping(8, 800, 888));
                for (var testableIdSlotMappingIter = expected.GetEnumerator();
                    testableIdSlotMappingIter
                        .MoveNext();)
                {
                    var testableIdSlotMapping = ((TestableIdSlotMapping
                        ) testableIdSlotMappingIter.Current);
                    _idMapping.MapId(testableIdSlotMapping._id, testableIdSlotMapping.Slot());
                }
                IList actual = new ArrayList();
                _idMapping.SlotChanges().Accept(new _IVisitor4_66(actual));
                IteratorAssert.SameContent(expected, actual);
            }

            private sealed class _IVisitor4_66 : IVisitor4
            {
                private readonly IList actual;

                public _IVisitor4_66(IList actual)
                {
                    this.actual = actual;
                }

                public void Visit(object slotChange)
                {
                    Assert.IsTrue(((SlotChange) slotChange).SlotModified());
                    var slot = ((SlotChange) slotChange).NewSlot();
                    actual.Add(new TestableIdSlotMapping(((TreeInt) slotChange)._key
                        , slot.Address(), slot.Length()));
                }
            }
        }

        public class TestableIdSlotMapping : IdSlotMapping
        {
            public TestableIdSlotMapping(int id, int address, int length) : base(id, address,
                length)
            {
            }

            public override bool Equals(object obj)
            {
                var other = (TestableIdSlotMapping
                    ) obj;
                return _id == other._id && _address == other._address && _length == other._length;
            }
        }

        private class DatabaseIdMappingProvider : IFunction4
        {
            private readonly IdMappingTestSuite _enclosing;

            internal DatabaseIdMappingProvider(IdMappingTestSuite _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public virtual object Apply(object fileName)
            {
                return new DatabaseIdMapping(((string) fileName));
            }
        }

        private class InMemoryIdMappingProvider : IFunction4
        {
            private readonly IdMappingTestSuite _enclosing;

            internal InMemoryIdMappingProvider(IdMappingTestSuite _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public virtual object Apply(object fileName)
            {
                return new InMemoryIdMapping();
            }
        }
    }
}