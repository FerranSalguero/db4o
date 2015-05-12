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
using Db4objects.Db4o.Internal.Btree;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Btree
{
    public class SearcherLowestHighestTestCase : ITestCase, ITestLifeCycle
    {
        private const int SearchFor = 9;

        private static readonly int[] EvenEvenValues =
        {
            4, 9, 9, 9, 9, 11, 13,
            17
        };

        private static readonly int[] EvenOddValues =
        {
            4, 5, 9, 9, 9, 11, 13,
            17
        };

        private static readonly int[] OddEvenValues = {4, 9, 9, 9, 9, 11, 13};
        private static readonly int[] OddOddValues = {4, 5, 9, 9, 9, 11, 13};
        private static readonly int[] NoMatchEven = {4, 5, 10, 10, 10, 11};
        private static readonly int[] NoMatchOdd = {4, 5, 10, 10, 10, 11, 13};

        private static readonly int[][] MatchValues =
        {
            EvenEvenValues, EvenOddValues
            , OddEvenValues, OddOddValues
        };

        private static readonly int[][] NoMatchValues =
        {
            NoMatchEven, NoMatchOdd
        };

        private static readonly SearchTarget[] AllTargets =
        {
            SearchTarget
                .Lowest,
            SearchTarget.Any, SearchTarget.Highest
        };

        private Searcher _searcher;

        /// <exception cref="System.Exception"></exception>
        public virtual void SetUp()
        {
            _searcher = null;
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TearDown()
        {
        }

        public virtual void TestMatch()
        {
            for (var i = 0; i < MatchValues.Length; i++)
            {
                var values = MatchValues[i];
                var lo = LowMatch(values);
                Search(values, SearchTarget.Lowest);
                Assert.AreEqual(lo, _searcher.Cursor());
                Assert.IsTrue(_searcher.FoundMatch());
                var hi = HighMatch(values);
                Search(values, SearchTarget.Highest);
                Assert.AreEqual(hi, _searcher.Cursor());
                Assert.IsTrue(_searcher.FoundMatch());
            }
        }

        public virtual void TestNoMatch()
        {
            for (var i = 0; i < NoMatchValues.Length; i++)
            {
                var values = NoMatchValues[i];
                var lo = LowMatch(values);
                Search(values, SearchTarget.Lowest);
                Assert.AreEqual(lo, _searcher.Cursor());
                Assert.IsFalse(_searcher.FoundMatch());
                var hi = HighMatch(values);
                Search(values, SearchTarget.Highest);
                Assert.AreEqual(hi, _searcher.Cursor());
                Assert.IsFalse(_searcher.FoundMatch());
            }
        }

        public virtual void TestEmpty()
        {
            int[] values = {};
            for (var i = 0; i < AllTargets.Length; i++)
            {
                Search(values, AllTargets[i]);
                Assert.AreEqual(0, _searcher.Cursor());
                Assert.IsFalse(_searcher.FoundMatch());
                Assert.IsFalse(_searcher.BeforeFirst());
                Assert.IsFalse(_searcher.AfterLast());
            }
        }

        public virtual void TestOneValueMatch()
        {
            int[] values = {9};
            for (var i = 0; i < AllTargets.Length; i++)
            {
                Search(values, AllTargets[i]);
                Assert.AreEqual(0, _searcher.Cursor());
                Assert.IsTrue(_searcher.FoundMatch());
                Assert.IsFalse(_searcher.BeforeFirst());
                Assert.IsFalse(_searcher.AfterLast());
            }
        }

        public virtual void TestOneValueLower()
        {
            int[] values = {8};
            for (var i = 0; i < AllTargets.Length; i++)
            {
                Search(values, AllTargets[i]);
                Assert.AreEqual(0, _searcher.Cursor());
                Assert.IsFalse(_searcher.FoundMatch());
                Assert.IsFalse(_searcher.BeforeFirst());
                Assert.IsTrue(_searcher.AfterLast());
            }
        }

        public virtual void TestOneValueHigher()
        {
            int[] values = {8};
            for (var i = 0; i < AllTargets.Length; i++)
            {
                Search(values, AllTargets[i]);
                Assert.AreEqual(0, _searcher.Cursor());
                Assert.IsFalse(_searcher.FoundMatch());
                Assert.IsFalse(_searcher.BeforeFirst());
                Assert.IsTrue(_searcher.AfterLast());
            }
        }

        public virtual void TestTwoValuesMatch()
        {
            int[] values = {9, 9};
            Search(values, SearchTarget.Lowest);
            Assert.AreEqual(0, _searcher.Cursor());
            Assert.IsTrue(_searcher.FoundMatch());
            Assert.IsFalse(_searcher.BeforeFirst());
            Assert.IsFalse(_searcher.AfterLast());
            Search(values, SearchTarget.Any);
            Assert.IsTrue(_searcher.FoundMatch());
            Assert.IsFalse(_searcher.BeforeFirst());
            Assert.IsFalse(_searcher.AfterLast());
            Search(values, SearchTarget.Highest);
            Assert.AreEqual(1, _searcher.Cursor());
            Assert.IsTrue(_searcher.FoundMatch());
            Assert.IsFalse(_searcher.BeforeFirst());
            Assert.IsFalse(_searcher.AfterLast());
        }

        public virtual void TestTwoValuesLowMatch()
        {
            int[] values = {9, 10};
            Search(values, SearchTarget.Lowest);
            Assert.AreEqual(0, _searcher.Cursor());
            Assert.IsTrue(_searcher.FoundMatch());
            Assert.IsFalse(_searcher.BeforeFirst());
            Assert.IsFalse(_searcher.AfterLast());
            Search(values, SearchTarget.Any);
            Assert.AreEqual(0, _searcher.Cursor());
            Assert.IsTrue(_searcher.FoundMatch());
            Assert.IsFalse(_searcher.BeforeFirst());
            Assert.IsFalse(_searcher.AfterLast());
            Search(values, SearchTarget.Highest);
            Assert.AreEqual(0, _searcher.Cursor());
            Assert.IsTrue(_searcher.FoundMatch());
            Assert.IsFalse(_searcher.BeforeFirst());
            Assert.IsFalse(_searcher.AfterLast());
        }

        public virtual void TestTwoValuesHighMatch()
        {
            int[] values = {6, 9};
            Search(values, SearchTarget.Lowest);
            Assert.AreEqual(1, _searcher.Cursor());
            Assert.IsTrue(_searcher.FoundMatch());
            Assert.IsFalse(_searcher.BeforeFirst());
            Assert.IsFalse(_searcher.AfterLast());
            Search(values, SearchTarget.Any);
            Assert.AreEqual(1, _searcher.Cursor());
            Assert.IsTrue(_searcher.FoundMatch());
            Assert.IsFalse(_searcher.BeforeFirst());
            Assert.IsFalse(_searcher.AfterLast());
            Search(values, SearchTarget.Highest);
            Assert.AreEqual(1, _searcher.Cursor());
            Assert.IsTrue(_searcher.FoundMatch());
            Assert.IsFalse(_searcher.BeforeFirst());
            Assert.IsFalse(_searcher.AfterLast());
        }

        public virtual void TestTwoValuesInBetween()
        {
            int[] values = {8, 10};
            Search(values, SearchTarget.Lowest);
            Assert.AreEqual(0, _searcher.Cursor());
            Assert.IsFalse(_searcher.FoundMatch());
            Assert.IsFalse(_searcher.BeforeFirst());
            Assert.IsFalse(_searcher.AfterLast());
            Search(values, SearchTarget.Any);
            Assert.AreEqual(0, _searcher.Cursor());
            Assert.IsFalse(_searcher.FoundMatch());
            Assert.IsFalse(_searcher.BeforeFirst());
            Assert.IsFalse(_searcher.AfterLast());
            Search(values, SearchTarget.Highest);
            Assert.AreEqual(0, _searcher.Cursor());
            Assert.IsFalse(_searcher.FoundMatch());
            Assert.IsFalse(_searcher.BeforeFirst());
            Assert.IsFalse(_searcher.AfterLast());
        }

        public virtual void TestTwoValuesLower()
        {
            int[] values = {7, 8};
            Search(values, SearchTarget.Lowest);
            Assert.AreEqual(1, _searcher.Cursor());
            Assert.IsFalse(_searcher.FoundMatch());
            Assert.IsFalse(_searcher.BeforeFirst());
            Assert.IsTrue(_searcher.AfterLast());
            Search(values, SearchTarget.Any);
            Assert.AreEqual(1, _searcher.Cursor());
            Assert.IsFalse(_searcher.FoundMatch());
            Assert.IsFalse(_searcher.BeforeFirst());
            Assert.IsTrue(_searcher.AfterLast());
            Search(values, SearchTarget.Highest);
            Assert.AreEqual(1, _searcher.Cursor());
            Assert.IsFalse(_searcher.FoundMatch());
            Assert.IsFalse(_searcher.BeforeFirst());
            Assert.IsTrue(_searcher.AfterLast());
        }

        public virtual void TestTwoValuesHigher()
        {
            int[] values = {10, 11};
            Search(values, SearchTarget.Lowest);
            Assert.AreEqual(0, _searcher.Cursor());
            Assert.IsFalse(_searcher.FoundMatch());
            Assert.IsTrue(_searcher.BeforeFirst());
            Assert.IsFalse(_searcher.AfterLast());
            Search(values, SearchTarget.Any);
            Assert.AreEqual(0, _searcher.Cursor());
            Assert.IsFalse(_searcher.FoundMatch());
            Assert.IsTrue(_searcher.BeforeFirst());
            Assert.IsFalse(_searcher.AfterLast());
            Search(values, SearchTarget.Highest);
            Assert.AreEqual(0, _searcher.Cursor());
            Assert.IsFalse(_searcher.FoundMatch());
            Assert.IsTrue(_searcher.BeforeFirst());
            Assert.IsFalse(_searcher.AfterLast());
        }

        private int Search(int[] values, SearchTarget target)
        {
            _searcher = new Searcher(target, values.Length);
            while (_searcher.Incomplete())
            {
                _searcher.ResultIs(values[_searcher.Cursor()] - SearchFor);
            }
            return _searcher.Cursor();
        }

        private int LowMatch(int[] values)
        {
            for (var i = 0; i < values.Length; i++)
            {
                if (values[i] == SearchFor)
                {
                    return i;
                }
                if (values[i] > SearchFor)
                {
                    if (i == 0)
                    {
                        return 0;
                    }
                    return i - 1;
                }
            }
            throw new ArgumentException("values");
        }

        private int HighMatch(int[] values)
        {
            for (var i = values.Length - 1; i >= 0; i--)
            {
                if (values[i] <= SearchFor)
                {
                    return i;
                }
            }
            throw new ArgumentException("values");
        }
    }
}