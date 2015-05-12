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

using Db4oUnit.Fixtures;

namespace Db4oUnit.Tests.Fixtures
{
    public class Set4TestUnit : ITestLifeCycle
    {
        private readonly object[] data = MultiValueFixtureProvider.Value();
        private readonly ISet4 subject = (ISet4) SubjectFixtureProvider.Value();

        public virtual void SetUp()
        {
            for (var i = 0; i < data.Length; ++i)
            {
                var element = data[i];
                subject.Add(element);
            }
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TearDown()
        {
        }

        public virtual void TestSize()
        {
            Assert.AreEqual(data.Length, subject.Size());
        }

        public virtual void TestContains()
        {
            for (var i = 0; i < data.Length; ++i)
            {
                var element = data[i];
                Assert.IsTrue(subject.Contains(element));
            }
        }
    }
}