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

using Db4objects.Db4o.Query;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Soda
{
    public class SortMultipleTestCase : AbstractDb4oTestCase
    {
        private static readonly Data[] TestData =
        {
            new Data(1, 2, 4), new Data(1, 4,
                3),
            new Data(2, 4, 2), new Data(3, 1,
                4),
            new Data(4, 3, 1), new Data(4, 1,
                3)
        };

        // COR-18
        public static void Main(string[] arguments)
        {
            new SortMultipleTestCase().RunSolo();
        }

        // 0
        // 1
        // 2
        // 3
        // 4
        // 5
        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            for (var dataIdx = 0; dataIdx < TestData.Length; dataIdx++)
            {
                Store(TestData[dataIdx]);
            }
        }

        public virtual void TestSortFirstThenSecondAfterOr()
        {
            var query = NewQuery(typeof (Data));
            query.Descend("_first").Constrain(2).Smaller().Or(query.Descend("_second").Constrain
                (2).Greater());
            query.Descend("_first").OrderAscending();
            query.Descend("_second").OrderAscending();
            AssertSortOrder(query, new[] {0, 1, 2, 4});
        }

        public virtual void TestSortFirstThenSecond()
        {
            var query = NewQuery(typeof (Data));
            query.Descend("_first").OrderAscending();
            query.Descend("_second").OrderAscending();
            AssertSortOrder(query, new[] {0, 1, 2, 3, 5, 4});
        }

        public virtual void TestSortSecondThenFirst()
        {
            var query = NewQuery(typeof (Data));
            query.Descend("_second").OrderAscending();
            query.Descend("_first").OrderAscending();
            AssertSortOrder(query, new[] {3, 5, 0, 4, 1, 2});
        }

        public virtual void TestSortThirdThenFirst()
        {
            var query = NewQuery(typeof (Data));
            query.Descend("_third").Descend("_value").OrderAscending();
            query.Descend("_first").OrderAscending();
            AssertSortOrder(query, new[] {4, 2, 1, 5, 0, 3});
        }

        public virtual void TestSortThirdThenSecond()
        {
            var query = NewQuery(typeof (Data));
            query.Descend("_third").Descend("_value").OrderAscending();
            query.Descend("_second").OrderAscending();
            AssertSortOrder(query, new[] {4, 2, 5, 1, 3, 0});
        }

        public virtual void TestSortSecondThenThird()
        {
            var query = NewQuery(typeof (Data));
            query.Descend("_second").OrderAscending();
            query.Descend("_third").Descend("_value").OrderAscending();
            AssertSortOrder(query, new[] {5, 3, 0, 4, 2, 1});
        }

        private void AssertSortOrder(IQuery query, int[] expectedIndexes)
        {
            var result = query.Execute();
            Assert.AreEqual(expectedIndexes.Length, result.Count);
            for (var i = 0; i < expectedIndexes.Length; i++)
            {
                Assert.AreEqual(TestData[expectedIndexes[i]], result.Next());
            }
        }

        public class IntHolder
        {
            public int _value;

            public IntHolder(int value)
            {
                _value = value;
            }

            public override bool Equals(object obj)
            {
                if (this == obj)
                {
                    return true;
                }
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }
                var intHolder = (IntHolder) obj;
                return _value == intHolder._value;
            }

            public override int GetHashCode()
            {
                return _value;
            }

            public override string ToString()
            {
                return _value.ToString();
            }
        }

        public class Data
        {
            public int _first;
            public int _second;
            public IntHolder _third;

            public Data(int first, int second, int third)
            {
                _first = first;
                _second = second;
                _third = new IntHolder(third);
            }

            public override bool Equals(object obj)
            {
                if (this == obj)
                {
                    return true;
                }
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }
                var data = (Data) obj;
                return _first == data._first && _second == data._second && _third.Equals(data._third
                    );
            }

            public override int GetHashCode()
            {
                var hc = _first;
                hc *= 29 + _second;
                hc *= 29 + _third.GetHashCode();
                return hc;
            }

            public override string ToString()
            {
                return _first + "/" + _second + "/" + _third;
            }
        }
    }
}