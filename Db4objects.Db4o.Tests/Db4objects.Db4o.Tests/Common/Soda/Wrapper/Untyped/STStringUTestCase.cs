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

using Db4objects.Db4o.Tests.Common.Soda.Util;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Soda.Wrapper.Untyped
{
    public class STStringUTestCase : SodaBaseTestCase
    {
        public object str;

        public STStringUTestCase()
        {
        }

        public STStringUTestCase(string str)
        {
            this.str = str;
        }

        public override object[] CreateData()
        {
            return new object[]
            {
                new STStringUTestCase
                    (null),
                new STStringUTestCase(
                    "aaa"),
                new STStringUTestCase(
                    "bbb"),
                new STStringUTestCase(
                    "dod")
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
            q.Constrain(new STStringUTestCase
                ());
            q.Descend("str").Constrain("bbb");
            SodaTestUtil.ExpectOne(q, new STStringUTestCase
                ("bbb"));
        }

        public virtual void TestContains()
        {
            var q = NewQuery();
            q.Constrain(new STStringUTestCase
                ("od"));
            q.Descend("str").Constraints().Contains();
            SodaTestUtil.ExpectOne(q, new STStringUTestCase
                ("dod"));
        }

        public virtual void TestNotContains()
        {
            var q = NewQuery();
            q.Constrain(new STStringUTestCase
                ("od"));
            q.Descend("str").Constraints().Contains().Not();
            SodaTestUtil.Expect(q, new object[]
            {
                new STStringUTestCase
                    (null),
                new STStringUTestCase(
                    "aaa"),
                new STStringUTestCase(
                    "bbb")
            });
        }

        public virtual void TestLike()
        {
            var q = NewQuery();
            q.Constrain(new STStringUTestCase
                ("do"));
            q.Descend("str").Constraints().Like();
            SodaTestUtil.ExpectOne(q, new STStringUTestCase
                ("dod"));
            q = NewQuery();
            q.Constrain(new STStringUTestCase
                ("od"));
            q.Descend("str").Constraints().Like();
            SodaTestUtil.ExpectOne(q, _array[3]);
        }

        public virtual void TestNotLike()
        {
            var q = NewQuery();
            q.Constrain(new STStringUTestCase
                ("aaa"));
            q.Descend("str").Constraints().Like().Not();
            SodaTestUtil.Expect(q, new object[]
            {
                new STStringUTestCase
                    (null),
                new STStringUTestCase(
                    "bbb"),
                new STStringUTestCase(
                    "dod")
            });
            q = NewQuery();
            q.Constrain(new STStringUTestCase
                ("xxx"));
            q.Descend("str").Constraints().Like();
            Expect(q, new int[] {});
        }

        public virtual void TestIdentity()
        {
            var q = NewQuery();
            q.Constrain(new STStringUTestCase
                ("aaa"));
            var set = q.Execute();
            var identityConstraint
                = (STStringUTestCase) set.Next
                    ();
            identityConstraint.str = "hihs";
            q = NewQuery();
            q.Constrain(identityConstraint).Identity();
            identityConstraint.str = "aaa";
            SodaTestUtil.ExpectOne(q, new STStringUTestCase
                ("aaa"));
        }

        public virtual void TestNotIdentity()
        {
            var q = NewQuery();
            q.Constrain(new STStringUTestCase
                ("aaa"));
            var set = q.Execute();
            var identityConstraint
                = (STStringUTestCase) set.Next
                    ();
            identityConstraint.str = null;
            q = NewQuery();
            q.Constrain(identityConstraint).Identity().Not();
            identityConstraint.str = "aaa";
            SodaTestUtil.Expect(q, new object[]
            {
                new STStringUTestCase
                    (null),
                new STStringUTestCase(
                    "bbb"),
                new STStringUTestCase(
                    "dod")
            });
        }

        public virtual void TestNull()
        {
            var q = NewQuery();
            q.Constrain(new STStringUTestCase
                (null));
            q.Descend("str").Constrain(null);
            SodaTestUtil.ExpectOne(q, new STStringUTestCase
                (null));
        }

        public virtual void TestNotNull()
        {
            var q = NewQuery();
            q.Constrain(new STStringUTestCase
                (null));
            q.Descend("str").Constrain(null).Not();
            SodaTestUtil.Expect(q, new object[]
            {
                new STStringUTestCase
                    ("aaa"),
                new STStringUTestCase
                    ("bbb"),
                new STStringUTestCase
                    ("dod")
            });
        }

        public virtual void TestConstraints()
        {
            var q = NewQuery();
            q.Constrain(new STStringUTestCase
                ("aaa"));
            q.Constrain(new STStringUTestCase
                ("bbb"));
            var cs = q.Constraints();
            var csa = cs.ToArray();
            if (csa.Length != 2)
            {
                Assert.Fail("Constraints not returned");
            }
        }
    }
}