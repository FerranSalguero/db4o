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
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Foundation
{
    /// <exclude></exclude>
    public class IteratorsTestCase : ITestCase
    {
        public static void Main(string[] args)
        {
            new ConsoleTestRunner(typeof (IteratorsTestCase)).Run();
        }

        public virtual void TestRange()
        {
            Iterator4Assert.AreEqual(new object[] {}, Iterators.Range(1, 1));
            Iterator4Assert.AreEqual(new object[] {1}, Iterators.Range(1, 2));
            Iterator4Assert.AreEqual(new object[] {1, 2}, Iterators.Range(1, 3));
            Iterator4Assert.AreEqual(new object[] {-2, -1, 0, 1, 2}, Iterators.Range(-2, 3)
                );
            Assert.Expect(typeof (ArgumentException), new _ICodeBlock_24());
        }

        public virtual void TestIterateSingle()
        {
            var i = Iterators.SingletonIterator("foo");
            Assert.IsTrue(i.MoveNext());
            Assert.AreEqual("foo", i.Current);
            Assert.IsFalse(i.MoveNext());
            Assert.Expect(typeof (InvalidOperationException), new _ICodeBlock_37(i));
        }

        public virtual void TestEnumerate()
        {
            var e = Iterators.Enumerate(Iterators.Iterable(new object[] {"1", "2"})
                );
            var iterator = e.GetEnumerator();
            var first = (EnumerateIterator.Tuple) Iterators.Next(iterator);
            var second = (EnumerateIterator.Tuple) Iterators.Next(iterator
                );
            Assert.AreEqual(0, first.index);
            Assert.AreEqual("1", first.value);
            Assert.AreEqual(1, second.index);
            Assert.AreEqual("2", second.value);
            Assert.IsFalse(iterator.MoveNext());
        }

        public virtual void TestCrossProduct()
        {
            IEnumerable[] source =
            {
                Iterable(new object[] {"1", "2"}), Iterable
                    (new object[] {"3", "4"}),
                Iterable(new object[] {"5", "6"})
            };
            string[] expected =
            {
                "[1, 3, 5]", "[1, 3, 6]", "[1, 4, 5]", "[1, 4, 6]"
                , "[2, 3, 5]", "[2, 3, 6]", "[2, 4, 5]", "[2, 4, 6]"
            };
            var iterator = Iterators.CrossProduct(source).GetEnumerator();
            Iterator4Assert.AreEqual(expected, Iterators.Map(iterator, new _IFunction4_75()));
        }

        private IEnumerable Iterable(object[] objects)
        {
            return Iterators.Iterable(objects);
        }

        public virtual void TestFlatten()
        {
            var iterator = Iterate(new object[]
            {
                "1", "2", Iterate(new object[]
                {
                    Iterate
                        (new object[] {"3", "4"}),
                    Iterators.EmptyIterator, Iterators.EmptyIterator, "5"
                }),
                Iterators.EmptyIterator, "6"
            });
            Iterator4Assert.AreEqual(new object[] {"1", "2", "3", "4", "5", "6"}, Iterators
                .Flatten(iterator));
        }

        internal virtual IEnumerator Iterate(object[] values)
        {
            return Iterators.Iterate(values);
        }

        public virtual void TestFilter()
        {
            AssertFilter(new[] {"bar", "baz"}, new[]
            {
                "foo", "bar", "baz", "zong"
            }, new _IPredicate4_116());
            AssertFilter(new[] {"foo", "bar"}, new[] {"foo", "bar"}, new _IPredicate4_124
                ());
            AssertFilter(new string[0], new[] {"foo", "bar"}, new _IPredicate4_133()
                );
        }

        private void AssertFilter(string[] expected, string[] actual, IPredicate4 filter)
        {
            Iterator4Assert.AreEqual(expected, Iterators.Filter(actual, filter));
        }

        public virtual void TestMap()
        {
            int[] array = {1, 2, 3};
            var args = new Collection4();
            var iterator = Iterators.Map(IntArrays4.NewIterator(array), new _IFunction4_149
                (args));
            Assert.IsNotNull(iterator);
            Assert.AreEqual(0, args.Size());
            for (var i = 0; i < array.Length; ++i)
            {
                Assert.IsTrue(iterator.MoveNext());
                Assert.AreEqual(i + 1, args.Size());
                Assert.AreEqual(array[i]*2, iterator.Current);
            }
        }

        public virtual void TestEmptyIterator()
        {
            var i = Iterators.EmptyIterator;
            Assert.IsFalse(i.MoveNext());
            i.Reset();
        }

        private sealed class _ICodeBlock_24 : ICodeBlock
        {
            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                Iterators.Range(2, 1);
            }
        }

        private sealed class _ICodeBlock_37 : ICodeBlock
        {
            private readonly IEnumerator i;

            public _ICodeBlock_37(IEnumerator i)
            {
                this.i = i;
            }

            public void Run()
            {
                Assert.IsNotNull(i.Current);
            }
        }

        private sealed class _IFunction4_75 : IFunction4
        {
            public object Apply(object arg)
            {
                return Iterators.ToString((IEnumerable) arg);
            }
        }

        private sealed class _IPredicate4_116 : IPredicate4
        {
            public bool Match(object candidate)
            {
                return ((string) candidate).StartsWith("b");
            }
        }

        private sealed class _IPredicate4_124 : IPredicate4
        {
            public bool Match(object candidate)
            {
                return true;
            }
        }

        private sealed class _IPredicate4_133 : IPredicate4
        {
            public bool Match(object candidate)
            {
                return false;
            }
        }

        private sealed class _IFunction4_149 : IFunction4
        {
            private readonly Collection4 args;

            public _IFunction4_149(Collection4 args)
            {
                this.args = args;
            }

            public object Apply(object arg)
            {
                args.Add(arg);
                return ((int) arg)*2;
            }
        }
    }
}