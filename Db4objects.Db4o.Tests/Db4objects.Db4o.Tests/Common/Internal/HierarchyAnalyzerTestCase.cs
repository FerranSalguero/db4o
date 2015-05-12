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
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Metadata;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Internal
{
    public class HierarchyAnalyzerTestCase : AbstractDb4oTestCase
    {
        public virtual void TestRemovedImmediateSuperclass()
        {
            AssertDiffBetween(typeof (DA), typeof (CBA
                ), new HierarchyAnalyzer.Diff[]
                {
                    new HierarchyAnalyzer.Removed(ProduceClassMetadata
                        (typeof (BA))),
                    new HierarchyAnalyzer.Same(ProduceClassMetadata
                        (typeof (A)))
                });
        }

        public virtual void TestRemoveTopLevelSuperclass()
        {
            AssertDiffBetween(typeof (E), typeof (BA
                ), new HierarchyAnalyzer.Diff[]
                {
                    new HierarchyAnalyzer.Removed(ProduceClassMetadata
                        (typeof (A)))
                });
        }

        public virtual void TestAddedImmediateSuperClass()
        {
            Assert.Expect(typeof (InvalidOperationException), new _ICodeBlock_50(this));
        }

        public virtual void TestAddedTopLevelSuperClass()
        {
            Assert.Expect(typeof (InvalidOperationException), new _ICodeBlock_58(this));
        }

        private void AssertDiffBetween(Type runtimeClass, Type storedClass, HierarchyAnalyzer.Diff
            [] expectedDiff)
        {
            var classMetadata = ProduceClassMetadata(storedClass);
            var reflectClass = ReflectClass(runtimeClass);
            var ancestors = new HierarchyAnalyzer(classMetadata, reflectClass).Analyze();
            AssertDiff(ancestors, expectedDiff);
        }

        private ClassMetadata ProduceClassMetadata(Type storedClass)
        {
            return Container().ProduceClassMetadata(ReflectClass(storedClass));
        }

        private void AssertDiff(IList actual, HierarchyAnalyzer.Diff[] expected)
        {
            Iterator4Assert.AreEqual(Iterators.Iterate(expected), Iterators.Iterator(actual));
        }

        public class A
        {
        }

        public class BA : A
        {
        }

        public class CBA : BA
        {
        }

        public class DA : A
        {
        }

        public class E
        {
        }

        private sealed class _ICodeBlock_50 : ICodeBlock
        {
            private readonly HierarchyAnalyzerTestCase _enclosing;

            public _ICodeBlock_50(HierarchyAnalyzerTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.AssertDiffBetween(typeof (CBA), typeof (DA
                    ), new HierarchyAnalyzer.Diff[] {});
            }
        }

        private sealed class _ICodeBlock_58 : ICodeBlock
        {
            private readonly HierarchyAnalyzerTestCase _enclosing;

            public _ICodeBlock_58(HierarchyAnalyzerTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.AssertDiffBetween(typeof (BA), typeof (E
                    ), new HierarchyAnalyzer.Diff[] {});
            }
        }
    }
}