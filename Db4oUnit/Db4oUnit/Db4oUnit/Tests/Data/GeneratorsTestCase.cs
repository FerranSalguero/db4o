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
using Db4oUnit.Data;

namespace Db4oUnit.Tests.Data
{
    public class GeneratorsTestCase : ITestCase
    {
        public virtual void TestArbitraryIntegerValues()
        {
            CheckArbitraryValuesOf(typeof (int));
        }

        public virtual void TestArbitraryStringValues()
        {
            CheckArbitraryValuesOf(typeof (string));
            Iterator4Assert.All(Generators.ArbitraryValuesOf(typeof (string)), new _IPredicate4_16
                ());
        }

        private void CheckArbitraryValuesOf(Type expectedType)
        {
            var values = Generators.ArbitraryValuesOf(expectedType);
            Assert.IsTrue(values.GetEnumerator().MoveNext());
            Iterator4Assert.AreInstanceOf(expectedType, values);
        }

        public virtual void TestTake()
        {
            string[] values = {"1", "2", "3"};
            var source = Iterators.Iterable(values);
            AssertTake(new object[0], 0, source);
            AssertTake(new object[] {"1"}, 1, source);
            AssertTake(new object[] {"1", "2"}, 2, source);
            AssertTake(values, 3, source);
            AssertTake(values, 4, source);
        }

        private void AssertTake(object[] expected, int count, IEnumerable source)
        {
            Iterator4Assert.AreEqual(expected, Generators.Take(count, source).GetEnumerator()
                );
        }

        private sealed class _IPredicate4_16 : IPredicate4
        {
            public bool Match(object candidate)
            {
                return IsValidString((string) candidate);
            }

            private bool IsValidString(string s)
            {
                for (var i = 0; i < s.Length; ++i)
                {
                    var ch = s[i];
                    if (!char.IsLetterOrDigit(ch) && !char.IsWhiteSpace(ch) && ch != '_')
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}