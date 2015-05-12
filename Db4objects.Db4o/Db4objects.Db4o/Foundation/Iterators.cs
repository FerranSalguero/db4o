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
using System.Text;

namespace Db4objects.Db4o.Foundation
{
    /// <summary>Iterator primitives (concat, map, reduce, filter, etc...).</summary>
    /// <remarks>Iterator primitives (concat, map, reduce, filter, etc...).</remarks>
    /// <exclude></exclude>
    public partial class Iterators
    {
        /// <summary>
        ///     Constant indicating that the current element in a
        ///     <see cref="Map(IEnumerator, IFunction4)">Map(IEnumerator, IFunction4)</see>
        ///     operation
        ///     should be skipped.
        /// </summary>
        public static readonly object Skip = new object();

        public static readonly IEnumerator EmptyIterator = new _IEnumerator_20();
        public static readonly IEnumerable EmptyIterable = new _IEnumerable_34();
        internal static readonly object NoElement = new object();

        /// <summary>
        ///     Generates
        ///     <see cref="Tuple">Tuple</see>
        ///     items with indexes starting at 0.
        /// </summary>
        /// <param name="iterable">the iterable to be enumerated</param>
        public static IEnumerable Enumerate(IEnumerable iterable)
        {
            return new _IEnumerable_48(iterable);
        }

        public static bool Any(IEnumerator iterator, IPredicate4 condition)
        {
            while (iterator.MoveNext())
            {
                if (condition.Match(iterator.Current))
                {
                    return true;
                }
            }
            return false;
        }

        public static IEnumerator Concat(IEnumerator[] array)
        {
            return Concat(Iterate(array));
        }

        public static IEnumerator Concat(IEnumerator iterators)
        {
            return new CompositeIterator4(iterators);
        }

        public static IEnumerable Concat(IEnumerable[] iterables)
        {
            return Concat(Iterable(iterables));
        }

        public static IEnumerable Concat(IEnumerable iterables)
        {
            return new CompositeIterable4(iterables);
        }

        public static IEnumerator Concat(IEnumerator first, IEnumerator second)
        {
            return Concat(new[] {first, second});
        }

        public static IEnumerable ConcatMap(IEnumerable iterable, IFunction4 function)
        {
            return Concat(Map(iterable, function));
        }

        /// <summary>
        ///     Returns a new iterator which yields the result of applying the function
        ///     to every element in the original iterator.
        /// </summary>
        /// <remarks>
        ///     Returns a new iterator which yields the result of applying the function
        ///     to every element in the original iterator.
        ///     <see cref="Skip">Skip</see>
        ///     can be returned from function to indicate the current
        ///     element should be skipped.
        /// </remarks>
        /// <param name="iterator"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static IEnumerator Map(IEnumerator iterator, IFunction4 function)
        {
            return new FunctionApplicationIterator(iterator, function);
        }

        public static IEnumerator Map(object[] array, IFunction4 function)
        {
            return Map(new ArrayIterator4(array), function);
        }

        public static IEnumerator Filter(object[] array, IPredicate4 predicate)
        {
            return Filter(new ArrayIterator4(array), predicate);
        }

        public static IEnumerable Filter(IEnumerable source, IPredicate4 predicate)
        {
            return new _IEnumerable_112(source, predicate);
        }

        public static IEnumerator Filter(IEnumerator iterator, IPredicate4 predicate)
        {
            return new FilteredIterator(iterator, predicate);
        }

        public static IEnumerable SingletonIterable(object element)
        {
            return new _IEnumerable_124(element);
        }

        public static IEnumerable Append(IEnumerable front, object last)
        {
            return Concat(Iterable(new object[] {front, SingletonIterable(last)}));
        }

        public static IEnumerator Iterator(IEnumerable iterable)
        {
            return iterable.GetEnumerator();
        }

        public static IEnumerator Iterate(object[] array)
        {
            return new ArrayIterator4(array);
        }

        public static IEnumerator Revert(IEnumerator iterator)
        {
            iterator.Reset();
            List4 tail = null;
            while (iterator.MoveNext())
            {
                tail = new List4(tail, iterator.Current);
            }
            return Iterate(tail);
        }

        public static IEnumerator Iterate(List4 list)
        {
            if (list == null)
            {
                return EmptyIterator;
            }
            var collection = new Collection4();
            while (list != null)
            {
                collection.Add(list._element);
                list = list._next;
            }
            return collection.GetEnumerator();
        }

        public static int Size(IEnumerable iterable)
        {
            return Size(iterable.GetEnumerator());
        }

        public static object Next(IEnumerator iterator)
        {
            if (!iterator.MoveNext())
            {
                throw new InvalidOperationException();
            }
            return iterator.Current;
        }

        public static int Size(IEnumerator iterator)
        {
            var count = 0;
            while (iterator.MoveNext())
            {
                ++count;
            }
            return count;
        }

        public static string ToString(IEnumerable i)
        {
            return ToString(i.GetEnumerator());
        }

        public static string ToString(IEnumerator i)
        {
            return Join(i, "[", "]", ", ");
        }

        public static string Join(IEnumerable i, string separator)
        {
            return Join(i.GetEnumerator(), separator);
        }

        public static string Join(IEnumerator i, string separator)
        {
            return Join(i, string.Empty, string.Empty, separator);
        }

        public static string Join(IEnumerator i, string prefix, string suffix, string separator
            )
        {
            var sb = new StringBuilder();
            sb.Append(prefix);
            if (i.MoveNext())
            {
                sb.Append(i.Current);
                while (i.MoveNext())
                {
                    sb.Append(separator);
                    sb.Append(i.Current);
                }
            }
            sb.Append(suffix);
            return sb.ToString();
        }

        public static object[] ToArray(IEnumerator tests)
        {
            return ToArray(tests, new _IArrayFactory_230());
        }

        public static object[] ToArray(IEnumerator tests, IArrayFactory factory)
        {
            var elements = new Collection4(tests);
            return elements.ToArray(factory.NewArray(elements.Size()));
        }

        /// <summary>Yields a flat sequence of elements.</summary>
        /// <remarks>
        ///     Yields a flat sequence of elements. Any
        ///     <see cref="IEnumerable">IEnumerable</see>
        ///     or
        ///     <see cref="IEnumerator">IEnumerator</see>
        ///     found in the original sequence is recursively flattened.
        /// </remarks>
        /// <param name="iterator">original sequence</param>
        public static IEnumerator Flatten(IEnumerator iterator)
        {
            return new FlatteningIterator(iterator);
        }

        public static IEnumerable Map(IEnumerable iterable, IFunction4 function)
        {
            return new _IEnumerable_253(iterable, function);
        }

        public static IEnumerable CrossProduct(IEnumerable iterables)
        {
            return CrossProduct((IEnumerable[]) ToArray(iterables.GetEnumerator(), new _IArrayFactory_261
                ()));
        }

        public static IEnumerable CrossProduct(IEnumerable[] iterables)
        {
            return CrossProduct(iterables, 0, EmptyIterable);
        }

        private static IEnumerable CrossProduct(IEnumerable[] iterables, int level, IEnumerable
            row)
        {
            if (level == iterables.Length - 1)
            {
                return Map(iterables[level], new _IFunction4_276(row));
            }
            return ConcatMap(iterables[level], new _IFunction4_284(iterables, level, row));
        }

        public static IEnumerable Iterable(object[] objects)
        {
            return new _IEnumerable_292(objects);
        }

        public static IEnumerator SingletonIterator(object element)
        {
            return new SingleValueIterator(element);
        }

        public static IEnumerable Iterable(IEnumerator iterator)
        {
            return new _IEnumerable_304(iterator);
        }

        public static IEnumerator Copy(IEnumerator iterator)
        {
            return new Collection4(iterator).GetEnumerator();
        }

        public static IEnumerator Take(int count, IEnumerator iterator)
        {
            return new _IEnumerator_316(count, iterator);
        }

        public static IEnumerator Range(int fromInclusive, int toExclusive)
        {
            if (toExclusive < fromInclusive)
            {
                throw new ArgumentException();
            }
            return Take(toExclusive - fromInclusive, Series(fromInclusive - 1, new _IFunction4_350
                ()).GetEnumerator());
        }

        public static IEnumerable Series(object seed, IFunction4 function)
        {
            return new _IEnumerable_356(seed, function);
        }

        private sealed class _IEnumerator_20 : IEnumerator
        {
            public object Current
            {
                get { throw new InvalidOperationException(); }
            }

            public bool MoveNext()
            {
                return false;
            }

            public void Reset()
            {
            }
        }

        private sealed class _IEnumerable_34 : IEnumerable
        {
            // do nothing
            public IEnumerator GetEnumerator()
            {
                return EmptyIterator;
            }
        }

        private sealed class _IEnumerable_48 : IEnumerable
        {
            private readonly IEnumerable iterable;

            public _IEnumerable_48(IEnumerable iterable)
            {
                this.iterable = iterable;
            }

            public IEnumerator GetEnumerator()
            {
                return new EnumerateIterator(iterable.GetEnumerator());
            }
        }

        private sealed class _IEnumerable_112 : IEnumerable
        {
            private readonly IPredicate4 predicate;
            private readonly IEnumerable source;

            public _IEnumerable_112(IEnumerable source, IPredicate4 predicate)
            {
                this.source = source;
                this.predicate = predicate;
            }

            public IEnumerator GetEnumerator()
            {
                return Filter(source.GetEnumerator(), predicate);
            }
        }

        private sealed class _IEnumerable_124 : IEnumerable
        {
            private readonly object element;

            public _IEnumerable_124(object element)
            {
                this.element = element;
            }

            public IEnumerator GetEnumerator()
            {
                return SingletonIterator(element);
            }
        }

        private sealed class _IArrayFactory_230 : IArrayFactory
        {
            public object[] NewArray(int size)
            {
                return new object[size];
            }
        }

        private sealed class _IEnumerable_253 : IEnumerable
        {
            private readonly IFunction4 function;
            private readonly IEnumerable iterable;

            public _IEnumerable_253(IEnumerable iterable, IFunction4 function)
            {
                this.iterable = iterable;
                this.function = function;
            }

            public IEnumerator GetEnumerator()
            {
                return Map(iterable.GetEnumerator(), function);
            }
        }

        private sealed class _IArrayFactory_261 : IArrayFactory
        {
            public object[] NewArray(int size)
            {
                return new IEnumerable[size];
            }
        }

        private sealed class _IFunction4_276 : IFunction4
        {
            private readonly IEnumerable row;

            public _IFunction4_276(IEnumerable row)
            {
                this.row = row;
            }

            public object Apply(object arg)
            {
                return Append(row, arg);
            }
        }

        private sealed class _IFunction4_284 : IFunction4
        {
            private readonly IEnumerable[] iterables;
            private readonly int level;
            private readonly IEnumerable row;

            public _IFunction4_284(IEnumerable[] iterables, int level, IEnumerable row)
            {
                this.iterables = iterables;
                this.level = level;
                this.row = row;
            }

            public object Apply(object arg)
            {
                return CrossProduct(iterables, level + 1, Append(row, arg));
            }
        }

        private sealed class _IEnumerable_292 : IEnumerable
        {
            private readonly object[] objects;

            public _IEnumerable_292(object[] objects)
            {
                this.objects = objects;
            }

            public IEnumerator GetEnumerator()
            {
                return Iterate(objects);
            }
        }

        private sealed class _IEnumerable_304 : IEnumerable
        {
            private readonly IEnumerator iterator;

            public _IEnumerable_304(IEnumerator iterator)
            {
                this.iterator = iterator;
            }

            public IEnumerator GetEnumerator()
            {
                return iterator;
            }
        }

        private sealed class _IEnumerator_316 : IEnumerator
        {
            private readonly int count;
            private readonly IEnumerator iterator;
            private int _taken;

            public _IEnumerator_316(int count, IEnumerator iterator)
            {
                this.count = count;
                this.iterator = iterator;
                _taken = 0;
            }

            public object Current
            {
                get
                {
                    if (_taken > count)
                    {
                        throw new InvalidOperationException();
                    }
                    return iterator.Current;
                }
            }

            public bool MoveNext()
            {
                if (_taken < count)
                {
                    if (!iterator.MoveNext())
                    {
                        _taken = count;
                        return false;
                    }
                    ++_taken;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }

        private sealed class _IFunction4_350 : IFunction4
        {
            public object Apply(object i)
            {
                return (((int) i)) + 1;
            }
        }

        private sealed class _IEnumerable_356 : IEnumerable
        {
            private readonly IFunction4 function;
            private readonly object seed;

            public _IEnumerable_356(object seed, IFunction4 function)
            {
                this.seed = seed;
                this.function = function;
            }

            public IEnumerator GetEnumerator()
            {
                return new _IEnumerator_358(seed, function);
            }

            private sealed class _IEnumerator_358 : IEnumerator
            {
                private readonly IFunction4 function;
                private readonly object seed;

                public _IEnumerator_358(object seed, IFunction4 function)
                {
                    this.seed = seed;
                    this.function = function;
                    Current = seed;
                }

                public object Current { get; private set; }

                public bool MoveNext()
                {
                    Current = function.Apply(Current);
                    return true;
                }

                public void Reset()
                {
                    Current = seed;
                }
            }
        }
    }
}