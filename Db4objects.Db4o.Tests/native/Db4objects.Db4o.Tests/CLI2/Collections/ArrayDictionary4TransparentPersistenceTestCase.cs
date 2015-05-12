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
using Db4objects.Db4o.Tests.Common.TA;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.CLI2.Collections
{
    public class ArrayDictionary4TransparentPersistenceTestCase : ITestLifeCycle
    {
        private MockActivator activator;
        private ArrayDictionary4<string, int> dict;

        public void SetUp()
        {
            dict = CreateDictionary();
            activator = MockActivator.ActivatorFor(dict);
        }

        public void TearDown()
        {
        }

        public void TestRemove()
        {
            dict.Remove("foo");
            Assert.AreEqual(0, activator.WriteCount(), "removing non-existent element");

            dict.Remove("ltuae");
            Assert.AreEqual(1, activator.WriteCount());
        }

        public void TestRemovePair()
        {
            ICollection<KeyValuePair<string, int>> pairs = dict;
            Assert.IsFalse(pairs.Remove(new KeyValuePair<string, int>("ltuae", 41)));

            Assert.AreEqual(0, activator.WriteCount(), "removing non-existent element");

            Assert.IsTrue(pairs.Remove(new KeyValuePair<string, int>("ltuae", 42)));
            Assert.AreEqual(1, activator.WriteCount());
        }

        public void TestIndexer()
        {
            dict["ltuae"] = 44;
            Assert.AreEqual(1, activator.WriteCount(), "changing existing value");

            dict["2+2"] = 4;
            Assert.AreEqual(2, activator.WriteCount(), "adding new value");
        }

        private static ArrayDictionary4<string, int> CreateDictionary()
        {
            var dict = new ArrayDictionary4<string, int>();
            dict["ltuae"] = 42;
            return dict;
        }
    }
}