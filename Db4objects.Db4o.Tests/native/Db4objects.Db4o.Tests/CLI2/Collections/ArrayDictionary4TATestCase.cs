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

using System.Collections.Generic;
using Db4objects.Db4o.Collections;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Tests.Common.TA;
using Db4oUnit;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.CLI2.Collections
{
    public class ArrayDictionary4TATestCase : TransparentActivationTestCaseBase, IOptOutSilverlight
    {
        protected override void Store()
        {
            IDictionary<string, int> dict = new ArrayDictionary4<string, int>();
            ArrayDictionary4Asserter.PutData(dict);
            Store(dict);
        }

        private IDictionary<string, int> RetrieveOnlyInstance()
        {
            var dict = (IDictionary<string, int>) RetrieveOnlyInstance(typeof (ArrayDictionary4<string, int>));
            AssertRetrievedItem(dict);
            return dict;
        }

        private void AssertRetrievedItem(IDictionary<string, int> dict)
        {
#if CF
            Assert.IsFalse(Db().IsActive(dict));
            string[] keys = (string[])  Reflection4.GetFieldValue(dict, "_keys");
            AssertInitalArray(keys, 16);
            int[] values = (int[])  Reflection4.GetFieldValue(dict, "_values");
            AssertInitalArray(values, 16);
#else
            Assert.IsNull(Reflection4.GetFieldValue(dict, "_keys"));
            Assert.IsNull(Reflection4.GetFieldValue(dict, "_values"));
#endif
            Assert.AreEqual(default(int), Reflection4.GetFieldValue(dict, "_size"));
        }

#if CF
        private void AssertInitalArray<T>(T[] array, int length)
        {
            Assert.AreEqual(length, array.Length);
            foreach (T value in array)
            {
                Assert.AreEqual(default(T), value);
            }
        }
#endif

        public void TestItemGet()
        {
            var dict = RetrieveOnlyInstance();
            ArrayDictionary4Asserter.AssertItemGet(dict);
        }

        public void TestItemSet()
        {
            var dict = RetrieveOnlyInstance();
            ArrayDictionary4Asserter.AssertItemSet(dict);
        }

        public void TestKeys()
        {
            var dict = RetrieveOnlyInstance();
            ArrayDictionary4Asserter.AssertKeys(dict);
        }

        public void TestValues()
        {
            var dict = RetrieveOnlyInstance();
            ArrayDictionary4Asserter.AssertValues(dict);
        }

        public void TestAdd()
        {
            var dict = RetrieveOnlyInstance();
            ArrayDictionary4Asserter.AssertAdd(dict);
        }

        public void TestContainsKey()
        {
            var dict = RetrieveOnlyInstance();
            ArrayDictionary4Asserter.TestContainsKey(dict);
        }

        public void TestRemove()
        {
            var dict = RetrieveOnlyInstance();
            ArrayDictionary4Asserter.AssertRemove(dict);
        }

        public void TestTryGetValue()
        {
            var dict = RetrieveOnlyInstance();
            ArrayDictionary4Asserter.AssertTryGetValue(dict);
        }

        public void TestCount()
        {
            var dict = RetrieveOnlyInstance();
            ArrayDictionary4Asserter.AssertCount(dict);
        }

        public void TestIsReadOnly()
        {
            var dict = RetrieveOnlyInstance();
            ArrayDictionary4Asserter.AssertIsReadOnly(dict);
        }

        public void TestAddKeyValuePair()
        {
            var dict = RetrieveOnlyInstance();
            ArrayDictionary4Asserter.AssertAddKeyValuePair(dict);
        }

        public void TestContains()
        {
            var dict = RetrieveOnlyInstance();
            ArrayDictionary4Asserter.AssertContains(dict);
        }

        public void TestCopyTo()
        {
            var dict = RetrieveOnlyInstance();
            ArrayDictionary4Asserter.AssertCopyTo(dict);
        }

        public void TestRemoveKeyValuePair()
        {
            var dict = RetrieveOnlyInstance();
            ArrayDictionary4Asserter.AssertRemoveKeyValuePair(dict);
        }

        public void TestClear()
        {
            var dict = RetrieveOnlyInstance();
            ArrayDictionary4Asserter.AssertClear(dict);
        }

        public void TestGetEnumerator()
        {
            var dict = RetrieveOnlyInstance();
            ArrayDictionary4Asserter.AssertGetEnumerator(dict);
        }
    }
}