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
using Db4objects.Db4o.Tests.Common.Soda.Util;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Soda.Classes.Simple
{
    public class STStringTestCase : SodaBaseTestCase, ISTInterface
    {
        public string str;

        public STStringTestCase()
        {
        }

        public STStringTestCase(string str)
        {
            this.str = str;
        }

        /// <summary>needed for STInterface test</summary>
        public virtual object ReturnSomething()
        {
            return str;
        }

        public override object[] CreateData()
        {
            return new object[]
            {
                new STStringTestCase
                    (null),
                new STStringTestCase("aaa"
                    ),
                new STStringTestCase("bbb"),
                new STStringTestCase("dod")
            };
        }

        public virtual void TestEquals()
        {
            var q = NewQuery();
            q.Constrain(_array[2]);
            SodaTestUtil.ExpectOne(q, _array[2]);
        }

        public virtual void TestNotEquals()
        {
            var q = NewQuery();
            q.Constrain(_array[2]);
            q.Descend("str").Constraints().Not();
            Expect(q, new[] {0, 1, 3});
        }

        public virtual void TestDescendantEquals()
        {
            var q = NewQuery();
            q.Constrain(new STStringTestCase
                ());
            q.Descend("str").Constrain("bbb");
            SodaTestUtil.ExpectOne(q, new STStringTestCase
                ("bbb"));
        }

        public virtual void TestContains()
        {
            var q = NewQuery();
            q.Constrain(new STStringTestCase
                ("od"));
            q.Descend("str").Constraints().Contains();
            SodaTestUtil.ExpectOne(q, new STStringTestCase
                ("dod"));
        }

        public virtual void TestNotContains()
        {
            var q = NewQuery();
            q.Constrain(new STStringTestCase
                ("od"));
            q.Descend("str").Constraints().Contains().Not();
            SodaTestUtil.Expect(q, new object[]
            {
                new STStringTestCase
                    (null),
                new STStringTestCase("aaa"
                    ),
                new STStringTestCase("bbb")
            }
                );
        }

        public virtual void TestLike()
        {
            var q = NewQuery();
            q.Constrain(new STStringTestCase
                ("do"));
            q.Descend("str").Constraints().Like();
            SodaTestUtil.ExpectOne(q, new STStringTestCase
                ("dod"));
            q = NewQuery();
            q.Constrain(new STStringTestCase
                ("od"));
            q.Descend("str").Constraints().Like();
            SodaTestUtil.ExpectOne(q, _array[3]);
        }

        public virtual void TestStartsWith()
        {
            var q = NewQuery();
            q.Constrain(new STStringTestCase
                ("do"));
            q.Descend("str").Constraints().StartsWith(true);
            SodaTestUtil.ExpectOne(q, new STStringTestCase
                ("dod"));
            q = NewQuery();
            q.Constrain(new STStringTestCase
                ("od"));
            q.Descend("str").Constraints().StartsWith(true);
            Expect(q, new int[] {});
            q = NewQuery();
            q.Constrain(new STStringTestCase
                ("dodo"));
            q.Descend("str").Constraints().StartsWith(true);
            Expect(q, new int[] {});
        }

        public virtual void TestEndsWith()
        {
            var q = NewQuery();
            q.Constrain(new STStringTestCase
                ("do"));
            q.Descend("str").Constraints().EndsWith(true);
            Expect(q, new int[] {});
            q = NewQuery();
            q.Constrain(new STStringTestCase
                ("od"));
            q.Descend("str").Constraints().EndsWith(true);
            SodaTestUtil.ExpectOne(q, new STStringTestCase
                ("dod"));
            q = NewQuery();
            q.Constrain(new STStringTestCase
                ("D"));
            q.Descend("str").Constraints().EndsWith(false);
            SodaTestUtil.ExpectOne(q, new STStringTestCase
                ("dod"));
            q = NewQuery();
            q.Constrain(new STStringTestCase
                ("dodo"));
            // COR-413
            q.Descend("str").Constraints().EndsWith(false);
            Expect(q, new int[] {});
        }

        public virtual void TestNotLike()
        {
            var q = NewQuery();
            q.Constrain(new STStringTestCase
                ("aaa"));
            q.Descend("str").Constraints().Like().Not();
            SodaTestUtil.Expect(q, new object[]
            {
                new STStringTestCase
                    (null),
                new STStringTestCase("bbb"
                    ),
                new STStringTestCase("dod")
            }
                );
            q = NewQuery();
            q.Constrain(new STStringTestCase
                ("xxx"));
            q.Descend("str").Constraints().Like();
            Expect(q, new int[] {});
        }

        public virtual void TestIdentity()
        {
            var q = NewQuery();
            q.Constrain(new STStringTestCase
                ("aaa"));
            var set = q.Execute();
            var identityConstraint
                = (STStringTestCase) set.Next();
            identityConstraint.str = "hihs";
            q = NewQuery();
            q.Constrain(identityConstraint).Identity();
            identityConstraint.str = "aaa";
            SodaTestUtil.ExpectOne(q, new STStringTestCase
                ("aaa"));
        }

        public virtual void TestNotIdentity()
        {
            var q = NewQuery();
            q.Constrain(new STStringTestCase
                ("aaa"));
            var set = q.Execute();
            var identityConstraint
                = (STStringTestCase) set.Next();
            identityConstraint.str = null;
            q = NewQuery();
            q.Constrain(identityConstraint).Identity().Not();
            identityConstraint.str = "aaa";
            SodaTestUtil.Expect(q, new object[]
            {
                new STStringTestCase
                    (null),
                new STStringTestCase("bbb"
                    ),
                new STStringTestCase("dod")
            }
                );
        }

        public virtual void TestNull()
        {
            var q = NewQuery();
            q.Constrain(new STStringTestCase
                (null));
            q.Descend("str").Constrain(null);
            SodaTestUtil.ExpectOne(q, new STStringTestCase
                (null));
        }

        public virtual void TestNotNull()
        {
            var q = NewQuery();
            q.Constrain(new STStringTestCase
                (null));
            q.Descend("str").Constrain(null).Not();
            SodaTestUtil.Expect(q, new object[]
            {
                new STStringTestCase
                    ("aaa"),
                new STStringTestCase("bbb"
                    ),
                new STStringTestCase("dod")
            }
                );
        }

        public virtual void TestConstraints()
        {
            var q = NewQuery();
            q.Constrain(new STStringTestCase
                ("aaa"));
            q.Constrain(new STStringTestCase
                ("bbb"));
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
            q.Constrain(new STStringTestCase
                (null));
            q.Constrain(new _IEvaluation_187());
            SodaTestUtil.ExpectOne(q, new STStringTestCase
                ("dod"));
        }

        public virtual void TestCaseInsenstiveContains()
        {
            var q = NewQuery();
            q.Constrain(typeof (STStringTestCase
                ));
            q.Constrain(new _IEvaluation_199());
            SodaTestUtil.ExpectOne(q, new STStringTestCase
                ("dod"));
        }

        private sealed class _IEvaluation_187 : IEvaluation
        {
            public void Evaluate(ICandidate candidate)
            {
                var sts = (STStringTestCase
                    ) candidate.GetObject();
                candidate.Include(sts.str.IndexOf("od") == 1);
            }
        }

        private sealed class _IEvaluation_199 : IEvaluation
        {
            public void Evaluate(ICandidate candidate)
            {
                var sts = (STStringTestCase
                    ) candidate.GetObject();
                candidate.Include(sts.str.ToLower().IndexOf("od") >= 0);
            }
        }
    }
}