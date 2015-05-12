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
using System.Collections.Generic;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Diagnostic;
using Db4objects.Db4o.Query;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Util;

namespace Db4objects.Db4o.Tests.CLI1.Handlers
{
    public abstract class ValueTypeHandlerTestCaseBase<T> : AbstractDb4oTestCase where T : struct, IComparable<T>
    {
        protected abstract ValueTypeHolder[] ObjectsToStore();
        protected abstract ValueTypeHolder[] ObjectsToOperateOn();

        protected virtual T UpdateValueFor(ValueTypeHolder holder)
        {
            return holder.Value;
        }

        protected IQuery NewDescendingQuery(Action<IQuery> action)
        {
            var query = NewQuery(typeof (ValueTypeHolder));
            var descendingQuery = query.Descend("Value");

            action(descendingQuery);

            return query;
        }

        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(typeof (ValueTypeHolder)).ObjectField("Value").Indexed(true);
            config.ObjectClass(typeof (ValueTypeHolder)).CascadeOnDelete(true);
        }

        protected override void Store()
        {
            foreach (var obj in ObjectsToStore())
            {
                Store(obj);
            }
        }

        public void TestNativeQuery()
        {
            foreach (var tbf in ObjectsToOperateOn())
            {
                AssertHolder(tbf.Value);
            }
        }

        public void TestSODAQuery()
        {
            foreach (var tbf in ObjectsToOperateOn())
            {
                AssertSODAQuery(tbf.Value);
            }
        }

        public void TestQueryByExample()
        {
            foreach (var tbf in ObjectsToOperateOn())
            {
                var holder = FindHolderWithValue(tbf.Value);
                var result = Db().QueryByExample(holder);
                Assert.AreEqual(1, result.Count);
                var found = (ValueTypeHolder) result[0];
                AssertHolder(holder, found);
            }
        }

        public void TestRetrieveAll()
        {
            AssertCanRetrieveAll();
        }

        public void TestNoClassIndex()
        {
            var storedClass = Db().StoredClass(typeof (T));
            Assert.AreEqual(0, storedClass.InstanceCount());
        }

        public void TestQueryOnUntypedField()
        {
            IList<ValueTypeHolder> holders = new List<ValueTypeHolder>(Flatten(ObjectsToStore()));

            var greatest = holders[0];
            var secondGreatest = holders[0];

            foreach (var holder in holders)
            {
                var actual = (IComparable) holder.Value;
                if (actual.CompareTo(greatest.Value) > 0)
                {
                    secondGreatest = greatest;
                    greatest = holder;
                }
                else if (actual.CompareTo(secondGreatest.Value) > 0)
                {
                    secondGreatest = holder;
                }
            }

            Assert.AreNotEqual(greatest, secondGreatest);

            var query = NewQuery(typeof (ValueTypeHolder));
            query.Descend("UntypedValue").Constrain(secondGreatest.Value).Greater();

            var result = query.Execute();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(greatest, result[0]);
        }

        public void TestDefragment()
        {
            Defragment();
            AssertCanRetrieveAll();
        }

        public void TestIndexingLowLevel()
        {
            var container = Fixture().FileSession();
            var classMetadata =
                container.ClassMetadataForReflectClass(container.Reflector().ForClass(typeof (ValueTypeHolder)));
            var fieldMetadata = classMetadata.FieldMetadataForName("Value");

            Assert.IsTrue(fieldMetadata.CanLoadByIndex(), WithTypeName("Typehandler for type {0} should be indexable."));
            var index = fieldMetadata.GetIndex(container.SystemTransaction());
            Assert.IsNotNull(index, WithTypeName("No btree index found for field of type {0} ."));
        }

        public void TestIndexedQuery()
        {
            var collector = DiagnosticCollectorFor<LoadedFromClassIndex>();

            var expected = ObjectsToOperateOn()[0];
            var actual = RetrieveHolderWithValue(expected.Value);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(0, collector.Diagnostics.Count, WithTypeName("Query should go through {0} indexes"));
        }

        public void TestUpdate()
        {
            var updated = RetrieveHolderWithValue(ObjectsToOperateOn()[0].Value);
            var newValue = UpdateValueFor(updated);
            Store(updated);

            Reopen();

            var actual = RetrieveHolderWithValue(newValue);
            Assert.AreEqual(updated, actual);
        }

        public void TestDelete()
        {
            var diagnosticCollector = DiagnosticCollectorFor<DeletionFailed>();

            var query = NewQuery(typeof (ValueTypeHolder));
            var result = query.Execute();
            while (result.HasNext())
            {
                var item = (ValueTypeHolder) result.Next();
                Db().Delete(item);
            }

            Assert.IsTrue(diagnosticCollector.Empty, diagnosticCollector.ToString());
        }

        private ValueTypeHolder FindHolderWithValue(T value)
        {
#if CF || SILVERLIGHT
    		foreach (ValueTypeHolder holder in ObjectsToStore())
    		{
    			if (holder.Value.CompareTo(value) == 0)
    			{
    				return holder;
    			}
    		}

    		return null;
#else
            return Array.Find(ObjectsToStore(),
                delegate(ValueTypeHolder candidate) { return candidate.Value.CompareTo(value) == 0; });
#endif
        }

        private void AssertCanRetrieveAll()
        {
            var query = NewQuery(typeof (ValueTypeHolder));
            var result = query.Execute();

            var expected = ObjectsToStore();
            Iterator4Assert.SameContent(Flatten(expected).GetEnumerator(), result.GetEnumerator());
        }

        private static IEnumerable<ValueTypeHolder> Flatten(IEnumerable<ValueTypeHolder> holders)
        {
            foreach (var holder in holders)
            {
                yield return holder;
                if (holder.Parent != null)
                {
                    yield return holder.Parent;
                }
            }
        }

        private void AssertSODAQuery(T value)
        {
            var query = NewQuery();
            query.Constrain(typeof (ValueTypeHolder));
            query.Descend("Value").Constrain(value);

            var result = query.Execute();
            Assert.AreEqual(1, result.Count);
            AssertHolder(FindHolderWithValue(value), (ValueTypeHolder) result[0]);
        }

        private void AssertHolder(ValueTypeHolder actual, ValueTypeHolder template)
        {
            var expected = FindHolderWithValue(template.Value);
            Assert.AreEqual(expected, actual);
        }

        private void AssertHolder(T expectedValue)
        {
            var items = Db()
                .Query(delegate(ValueTypeHolder candidate) { return candidate.Value.Equals(expectedValue); });
            Assert.AreEqual(1, items.Count);
            Assert.IsNotNull(items[0]);

            var expected = Find(ObjectsToStore(),
                delegate(ValueTypeHolder candidate) { return candidate.Value.Equals(expectedValue); });
            Assert.IsNotNull(expected);

            Assert.AreEqual(expected, items[0]);
        }

        private static ValueTypeHolder Find(ValueTypeHolder[] array, Predicate<ValueTypeHolder> expected)
        {
            for (var i = 0; i < array.Length; i++)
            {
                if (expected(array[i]))
                {
                    return array[i];
                }
            }
            return null;
        }

        protected ValueTypeHolder RetrieveHolderWithValue(T value)
        {
            var result = RetrieveHoldersWith(value);
            Assert.AreEqual(1, result.Count);
            var actual = (ValueTypeHolder) result[0];
            Assert.AreEqual(value, actual.Value);

            return actual;
        }

        protected IObjectSet RetrieveHoldersWith(params T[] values)
        {
            IConstraint lastConstraint = null;

            var query = NewQuery(typeof (ValueTypeHolder));
            foreach (var value in values)
            {
                var constraint = query.Descend("Value").Constrain(value);
                if (lastConstraint != null)
                {
                    lastConstraint.Or(constraint);
                }

                lastConstraint = constraint;
            }

            return query.Execute();
        }

        private DiagnosticCollector<D> DiagnosticCollectorFor<D>()
        {
            var collector = new DiagnosticCollector<D>();
            Db().Configure().Diagnostic().AddListener(collector);

            return collector;
        }

        private static string WithTypeName(string format)
        {
            return string.Format(format, typeof (T).Name);
        }

        public class ValueTypeHolder
        {
            public ValueTypeHolder Parent;
            public object UntypedValue;
            public T Value;

            public ValueTypeHolder(T value)
            {
                Value = value;
                UntypedValue = value;
            }

            public ValueTypeHolder(T value, ValueTypeHolder parent) : this(value)
            {
                Parent = parent;
            }

            public override bool Equals(object obj)
            {
                var rhs = obj as ValueTypeHolder;
                if (rhs == null) return false;

                if (rhs.GetType() != GetType()) return false;

                return (rhs.Value.CompareTo(Value) == 0) && CompareParent(rhs);
            }

            private bool CompareParent(ValueTypeHolder rhs)
            {
                return Parent == null
                    ? rhs.Parent == null
                    : Parent.Equals(rhs.Parent);
            }

            public override int GetHashCode()
            {
                return Value.GetHashCode() + (Parent != null ? Parent.GetHashCode() : 0);
            }

            public override string ToString()
            {
                return "[" + typeof (T).Name + "]" + Value + " Parent {" + (Parent != null ? Parent.ToString() : "") +
                       "}";
            }
        }
    }
}