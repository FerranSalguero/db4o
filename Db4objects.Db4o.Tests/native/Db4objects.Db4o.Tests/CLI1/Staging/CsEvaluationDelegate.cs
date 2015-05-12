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

/**
 *  COR-1432
 */

namespace Db4objects.Db4o.Tests.CLI1
{
    public class CsEvaluationDelegate : AbstractDb4oTestCase
    {
        internal CsEvaluationDelegate child;
        internal string name;

        protected override void Store()
        {
            name = "one";
            Store(this);
            var se1 = new CsEvaluationDelegate();
            se1.child = new CsEvaluationDelegate();
            se1.child.name = "three";
            se1.name = "two";
            Store(se1);
            se1 = new CsEvaluationDelegate();
            se1.child = new CsEvaluationDelegate();
            se1.child.name = "five";
            se1.name = "four";
            Store(se1);
        }

        public void TestStaticMethodDelegate()
        {
            RunEvaluationDelegateTest(Evaluate);
        }

        public void TestInstanceMethodDelegate()
        {
            RunEvaluationDelegateTest(new NameCondition("three").Evaluate);
        }

        private void RunEvaluationDelegateTest(EvaluationDelegate evaluation)
        {
            var q1 = NewQuery();
            var cq1 = q1;
            q1.Constrain(GetType());
            cq1 = cq1.Descend("child");
            cq1.Constrain(evaluation);
            var os = q1.Execute();
            Assert.AreEqual(1, os.Count);
            var se = (CsEvaluationDelegate) os.Next();
            Assert.AreEqual("two", se.name);
        }

        public static void Evaluate(ICandidate candidate)
        {
            var obj = ((CsEvaluationDelegate) candidate.GetObject());
            candidate.Include(obj.name.Equals("three"));
        }

        private class NameCondition
        {
            private readonly string _name;

            public NameCondition(string name)
            {
                _name = name;
            }

            public void Evaluate(ICandidate candidate)
            {
                var obj = ((CsEvaluationDelegate) candidate.GetObject());
                candidate.Include(obj.name.Equals(_name));
            }
        }
    }
}