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
using Db4objects.Db4o.Query;
using Db4objects.Db4o.Tests.Common.Soda.Util;

namespace Db4objects.Db4o.Tests.Common.Soda.Experiments
{
    public class STIdentityEvaluationTestCase : SodaBaseTestCase
    {
        public Helper helper;

        public STIdentityEvaluationTestCase()
        {
        }

        public STIdentityEvaluationTestCase(Helper h)
        {
            helper = h;
        }

        public override object[] CreateData()
        {
            var helperA = new Helper
                ("aaa");
            return new object[]
            {
                new STIdentityEvaluationTestCase
                    (null),
                new STIdentityEvaluationTestCase
                    (helperA),
                new STIdentityEvaluationTestCase
                    (helperA),
                new STIdentityEvaluationTestCase
                    (helperA),
                new STIdentityEvaluationTestCase
                    (new HelperDerivate("bbb")),
                new STIdentityEvaluationTestCase
                    (new Helper("dod"))
            };
        }

        public virtual void Test()
        {
            var q = NewQuery();
            q.Constrain(new Helper("aaa"));
            var os = q.Execute();
            var helperA = (Helper
                ) os.Next();
            q = NewQuery();
            q.Constrain(typeof (STIdentityEvaluationTestCase
                ));
            q.Descend("helper").Constrain(helperA).Identity();
            q.Constrain(new AcceptAllEvaluation());
            Expect(q, new[] {1, 2, 3});
        }

        public virtual void TestMemberClassConstraint()
        {
            var q = NewQuery();
            q.Constrain(typeof (STIdentityEvaluationTestCase
                ));
            q.Descend("helper").Constrain(typeof (HelperDerivate)
                );
            Expect(q, new[] {4});
        }

        [Serializable]
        public class AcceptAllEvaluation : IEvaluation
        {
            public virtual void Evaluate(ICandidate candidate)
            {
                candidate.Include(true);
            }
        }

        public class Helper
        {
            public string hString;

            public Helper()
            {
            }

            public Helper(string str)
            {
                hString = str;
            }
        }

        public class HelperDerivate : Helper
        {
            public HelperDerivate()
            {
            }

            public HelperDerivate(string str) : base(str)
            {
            }
        }
    }
}