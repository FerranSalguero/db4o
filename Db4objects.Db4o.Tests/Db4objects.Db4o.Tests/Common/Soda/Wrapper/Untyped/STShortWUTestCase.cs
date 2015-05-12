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
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Soda.Wrapper.Untyped
{
    [Serializable]
    public class STShortWUTestCase : SodaBaseTestCase
    {
        internal static readonly string Descendant = "i_short";
        public object i_short;

        public STShortWUTestCase()
        {
        }

        private STShortWUTestCase(short a_short)
        {
            i_short = a_short;
        }

        public override object[] CreateData()
        {
            return new object[]
            {
                new STShortWUTestCase
                    (0),
                new STShortWUTestCase
                    (1),
                new STShortWUTestCase
                    (99),
                new STShortWUTestCase
                    (909)
            };
        }

        public virtual void TestEquals()
        {
            var q = NewQuery();
            q.Constrain(new STShortWUTestCase
                (0));
            // Primitive default values are ignored, so we need an 
            // additional constraint:
            q.Descend(Descendant).Constrain((short) 0);
            SodaTestUtil.ExpectOne(q, _array[0]);
        }

        public virtual void TestNotEquals()
        {
            var q = NewQuery();
            q.Constrain(_array[0]);
            q.Descend(Descendant).Constraints().Not();
            Expect(q, new[] {1, 2, 3});
        }

        public virtual void TestGreater()
        {
            var q = NewQuery();
            q.Constrain(new STShortWUTestCase
                (9));
            q.Descend(Descendant).Constraints().Greater();
            Expect(q, new[] {2, 3});
        }

        public virtual void TestSmaller()
        {
            var q = NewQuery();
            q.Constrain(new STShortWUTestCase
                (1));
            q.Descend(Descendant).Constraints().Smaller();
            SodaTestUtil.ExpectOne(q, _array[0]);
        }

        public virtual void TestContains()
        {
            var q = NewQuery();
            q.Constrain(new STShortWUTestCase
                (9));
            q.Descend(Descendant).Constraints().Contains();
            Expect(q, new[] {2, 3});
        }

        public virtual void TestNotContains()
        {
            var q = NewQuery();
            q.Constrain(new STShortWUTestCase
                (0));
            q.Descend(Descendant).Constraints().Contains().Not();
            Expect(q, new[] {1, 2});
        }

        public virtual void TestLike()
        {
            var q = NewQuery();
            q.Constrain(new STShortWUTestCase
                (90));
            q.Descend(Descendant).Constraints().Like();
            SodaTestUtil.ExpectOne(q, _array[3]);
            q = NewQuery();
            q.Constrain(new STShortWUTestCase
                (10));
            q.Descend(Descendant).Constraints().Like();
            Expect(q, new int[] {});
        }

        public virtual void TestNotLike()
        {
            var q = NewQuery();
            q.Constrain(new STShortWUTestCase
                (1));
            q.Descend(Descendant).Constraints().Like().Not();
            Expect(q, new[] {0, 2, 3});
        }

        public virtual void TestIdentity()
        {
            var q = NewQuery();
            q.Constrain(new STShortWUTestCase
                (1));
            var set = q.Execute();
            var identityConstraint
                = (STShortWUTestCase) set.Next
                    ();
            identityConstraint.i_short = (short) 9999;
            q = NewQuery();
            q.Constrain(identityConstraint).Identity();
            identityConstraint.i_short = (short) 1;
            SodaTestUtil.ExpectOne(q, _array[1]);
        }

        public virtual void TestNotIdentity()
        {
            var q = NewQuery();
            q.Constrain(new STShortWUTestCase
                (1));
            var set = q.Execute();
            var identityConstraint
                = (STShortWUTestCase) set.Next
                    ();
            identityConstraint.i_short = (short) 9080;
            q = NewQuery();
            q.Constrain(identityConstraint).Identity().Not();
            identityConstraint.i_short = (short) 1;
            Expect(q, new[] {0, 2, 3});
        }

        public virtual void TestConstraints()
        {
            var q = NewQuery();
            q.Constrain(new STShortWUTestCase
                (1));
            q.Constrain(new STShortWUTestCase
                (0));
            var cs = q.Constraints();
            var csa = cs.ToArray();
            if (csa.Length != 2)
            {
                Assert.Fail("Constraints not returned");
            }
        }

        public virtual void TestEvaluation()
        {
            var q = NewQuery();
            q.Constrain(new STShortWUTestCase
                ());
            q.Constrain(new _IEvaluation_139());
            Expect(q, new[] {2, 3});
        }

        private sealed class _IEvaluation_139 : IEvaluation
        {
            public void Evaluate(ICandidate candidate)
            {
                var sts = (STShortWUTestCase
                    ) candidate.GetObject();
                candidate.Include((((short) sts.i_short) + 2) > 100);
            }
        }
    }
}