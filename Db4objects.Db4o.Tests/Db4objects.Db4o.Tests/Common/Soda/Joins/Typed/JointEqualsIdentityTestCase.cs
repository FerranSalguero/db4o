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

using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Soda.Joins.Typed
{
    public class JointEqualsIdentityTestCase : AbstractDb4oTestCase
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            var subjectA = new TestSubject
                ("A", null);
            var subjectB = new TestSubject
                ("B", subjectA);
            var subjectC = new TestSubject
                ("C", subjectA);
            Store(subjectA);
            Store(subjectB);
            Store(subjectC);
        }

        public virtual void TestJointEqualsIdentity()
        {
            var child = RetrieveChild();
            var query = NewQuery(typeof (TestSubject));
            var constraint = query.Descend("_name").Constrain("B").Equal();
            constraint.And(query.Descend("_child").Constrain(child).Identity());
            Assert.AreEqual(1, query.Execute().Count);
        }

        private TestSubject RetrieveChild()
        {
            var query = NewQuery(typeof (TestSubject));
            query.Descend("_child").Constrain(null);
            return (TestSubject) query.Execute().Next();
        }

        public class TestSubject
        {
            public TestSubject _child;
            public string _name;

            public TestSubject(string name, TestSubject child)
            {
                _name = name;
                _child = child;
            }
        }
    }
}