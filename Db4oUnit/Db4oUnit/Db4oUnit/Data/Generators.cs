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
using Sharpen;

namespace Db4oUnit.Data
{
    public partial class Generators
    {
        public static IEnumerable ArbitraryValuesOf(Type type)
        {
            var platformSpecific = PlatformSpecificArbitraryValuesOf(type);
            if (null != platformSpecific)
            {
                return platformSpecific;
            }
            if (type == typeof (int))
            {
                return Take(10, Streams.RandomIntegers());
            }
            if (type == typeof (string))
            {
                return Take(10, Streams.RandomStrings());
            }
            throw new NotImplementedException("No generator for type " + type);
        }

        internal static IEnumerable Trace(IEnumerable source)
        {
            return Iterators.Map(source, new _IFunction4_32());
        }

        public static IEnumerable Take(int count, IEnumerable source)
        {
            return new _IEnumerable_41(source, count);
        }

        private sealed class _IFunction4_32 : IFunction4
        {
            public object Apply(object value)
            {
                Runtime.Out.WriteLine(value);
                return value;
            }
        }

        private sealed class _IEnumerable_41 : IEnumerable
        {
            private readonly int count;
            private readonly IEnumerable source;

            public _IEnumerable_41(IEnumerable source, int count)
            {
                this.source = source;
                this.count = count;
            }

            public IEnumerator GetEnumerator()
            {
                return new _IEnumerator_43(source, count);
            }

            private sealed class _IEnumerator_43 : IEnumerator
            {
                private readonly int count;
                private readonly IEnumerable source;
                private IEnumerator _delegate;
                private int _taken;

                public _IEnumerator_43(IEnumerable source, int count)
                {
                    this.source = source;
                    this.count = count;
                    _taken = 0;
                    _delegate = source.GetEnumerator();
                }

                public object Current
                {
                    get
                    {
                        if (_taken > count)
                        {
                            throw new InvalidOperationException();
                        }
                        return _delegate.Current;
                    }
                }

                public bool MoveNext()
                {
                    if (_taken < count)
                    {
                        if (!_delegate.MoveNext())
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
                    _taken = 0;
                    _delegate = source.GetEnumerator();
                }
            }
        }
    }
}