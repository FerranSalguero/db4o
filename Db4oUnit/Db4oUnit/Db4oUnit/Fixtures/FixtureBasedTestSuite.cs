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

namespace Db4oUnit.Fixtures
{
    /// <summary>
    ///     TODO: experiment with ParallelTestRunner that uses a thread pool to run tests in parallel
    ///     TODO: FixtureProviders must accept the index of a specific fixture to run with (to make it easy to reproduce a
    ///     failure)
    /// </summary>
    public abstract class FixtureBasedTestSuite : ITestSuiteBuilder
    {
        private static readonly int[] AllCombinations = null;

        public virtual IEnumerator GetEnumerator()
        {
            var providers = FixtureProviders();
            var decorators = FixtureDecoratorsFor(providers);
            var testsXdecorators = Iterators.CrossProduct(new[]
            {
                Tests()
                , Iterators.CrossProduct(decorators)
            });
            return Iterators.Map(testsXdecorators, new _IFunction4_35(this)).GetEnumerator();
        }

        public abstract Type[] TestUnits();
        public abstract IFixtureProvider[] FixtureProviders();

        public virtual int[] CombinationToRun()
        {
            return AllCombinations;
        }

        private IEnumerable FixtureDecoratorsFor(IFixtureProvider[] providers)
        {
            var combination = CombinationToRun();
            return combination == AllCombinations
                ? AllFixtureDecoratorsFor(providers)
                : CombinationFixtureDecoratorsFor
                    (providers, combination);
        }

        private IEnumerable CombinationFixtureDecoratorsFor(IFixtureProvider[] providers,
            int[] combination)
        {
            Assert.AreEqual(providers.Length, combination.Length,
                "Number of indexes in combinationToRun should match number of providers"
                );
            var decorators = Iterators.Map(Iterators.Enumerate(Iterators.Iterable(providers
                )), new _IFunction4_54(combination));
            return decorators;
        }

        private IEnumerable AllFixtureDecoratorsFor(IFixtureProvider[] providers)
        {
            var decorators = Iterators.Map(Iterators.Iterable(providers), new _IFunction4_74
                ());
            return decorators;
        }

        private IEnumerable Tests()
        {
            var units = TestUnits();
            if (units == null || units.Length == 0)
            {
                throw new InvalidOperationException(GetType() + " has no TestUnits.");
            }
            return new ReflectionTestSuiteBuilder(units);
        }

        private ITest Decorate(ITest test, IEnumerator decorators)
        {
            while (decorators.MoveNext())
            {
                test = ((ITestDecorator) decorators.Current).Decorate(test);
            }
            return test;
        }

        private sealed class _IFunction4_35 : IFunction4
        {
            private readonly FixtureBasedTestSuite _enclosing;

            public _IFunction4_35(FixtureBasedTestSuite _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public object Apply(object arg)
            {
                var tuple = ((IEnumerable) arg).GetEnumerator();
                var test = (ITest) Iterators.Next(tuple);
                var decorators = (IEnumerable) Iterators.Next(tuple);
                return _enclosing.Decorate(test, decorators.GetEnumerator());
            }
        }

        private sealed class _IFunction4_54 : IFunction4
        {
            private readonly int[] combination;

            public _IFunction4_54(int[] combination)
            {
                this.combination = combination;
            }

            public object Apply(object arg)
            {
                var providerTuple = (EnumerateIterator.Tuple) arg;
                var provider = (IFixtureProvider) providerTuple.value;
                var wantedIndex = combination[providerTuple.index];
                return Iterators.Map(Iterators.Enumerate(provider), new _IFunction4_59(wantedIndex
                    , provider));
            }

            private sealed class _IFunction4_59 : IFunction4
            {
                private readonly IFixtureProvider provider;
                private readonly int wantedIndex;

                public _IFunction4_59(int wantedIndex, IFixtureProvider provider)
                {
                    this.wantedIndex = wantedIndex;
                    this.provider = provider;
                }

                public object Apply(object arg)
                {
                    var tuple = (EnumerateIterator.Tuple) arg;
                    if (tuple.index != wantedIndex)
                    {
                        return Iterators.Skip;
                    }
                    return new FixtureDecorator(provider.Variable(), tuple.value, tuple.index);
                }
            }
        }

        private sealed class _IFunction4_74 : IFunction4
        {
            public object Apply(object arg)
            {
                var provider = (IFixtureProvider) arg;
                return Iterators.Map(Iterators.Enumerate(provider), new _IFunction4_77(provider));
            }

            private sealed class _IFunction4_77 : IFunction4
            {
                private readonly IFixtureProvider provider;

                public _IFunction4_77(IFixtureProvider provider)
                {
                    this.provider = provider;
                }

                public object Apply(object arg)
                {
                    var tuple = (EnumerateIterator.Tuple) arg;
                    return new FixtureDecorator(provider.Variable(), tuple.value, tuple.index);
                }
            }
        }
    }
}