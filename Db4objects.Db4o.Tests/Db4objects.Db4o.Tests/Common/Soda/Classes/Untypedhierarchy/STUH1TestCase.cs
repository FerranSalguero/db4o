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

using Db4objects.Db4o.Tests.Common.Soda.Classes.Typedhierarchy;
using Db4objects.Db4o.Tests.Common.Soda.Util;

namespace Db4objects.Db4o.Tests.Common.Soda.Classes.Untypedhierarchy
{
    /// <summary>UH: Untyped Hierarchy</summary>
    public class STUH1TestCase : SodaBaseTestCase
    {
        public object foo1;
        public object h2;

        public STUH1TestCase()
        {
        }

        public STUH1TestCase(STUH2 a2)
        {
            h2 = a2;
        }

        public STUH1TestCase(string str)
        {
            foo1 = str;
        }

        public STUH1TestCase(STUH2 a2, string str)
        {
            h2 = a2;
            foo1 = str;
        }

        public override object[] CreateData()
        {
            return new object[]
            {
                new STUH1TestCase
                    (),
                new STUH1TestCase
                    ("str1"),
                new STUH1TestCase
                    (new STUH2()),
                new STUH1TestCase
                    (new STUH2("str2")),
                new STUH1TestCase
                    (new STUH2(new STUH3("str3"))),
                new STUH1TestCase
                    (new STUH2(new STUH3("str3"), "str2"))
            };
        }

        public virtual void TestStrNull()
        {
            var q = NewQuery();
            q.Constrain(new STUH1TestCase
                ());
            q.Descend("foo1").Constrain(null);
            Expect(q, new[] {0, 2, 3, 4, 5});
        }

        public virtual void TestBothNull()
        {
            var q = NewQuery();
            q.Constrain(new STUH1TestCase
                ());
            q.Descend("foo1").Constrain(null);
            q.Descend("h2").Constrain(null);
            SodaTestUtil.ExpectOne(q, _array[0]);
        }

        public virtual void TestDescendantNotNull()
        {
            var q = NewQuery();
            q.Constrain(new STUH1TestCase
                ());
            q.Descend("h2").Constrain(null).Not();
            Expect(q, new[] {2, 3, 4, 5});
        }

        public virtual void TestDescendantDescendantNotNull()
        {
            var q = NewQuery();
            q.Constrain(new STUH1TestCase
                ());
            q.Descend("h2").Descend("h3").Constrain(null).Not();
            Expect(q, new[] {4, 5});
        }

        public virtual void TestDescendantExists()
        {
            var q = NewQuery();
            q.Constrain(_array[2]);
            Expect(q, new[] {2, 3, 4, 5});
        }

        public virtual void TestDescendantValue()
        {
            var q = NewQuery();
            q.Constrain(_array[3]);
            Expect(q, new[] {3, 5});
        }

        public virtual void TestDescendantDescendantExists()
        {
            var q = NewQuery();
            q.Constrain(new STUH1TestCase
                (new STUH2(new STUH3())));
            Expect(q, new[] {4, 5});
        }

        public virtual void TestDescendantDescendantValue()
        {
            var q = NewQuery();
            q.Constrain(new STUH1TestCase
                (new STUH2(new STUH3("str3"))));
            Expect(q, new[] {4, 5});
        }

        public virtual void TestDescendantDescendantStringPath()
        {
            var q = NewQuery();
            q.Constrain(new STUH1TestCase
                ());
            q.Descend("h2").Descend("h3").Descend("foo3").Constrain("str3");
            Expect(q, new[] {4, 5});
        }

        public virtual void TestSequentialAddition()
        {
            var q = NewQuery();
            q.Constrain(new STUH1TestCase
                ());
            var cur = q.Descend("h2");
            cur.Constrain(new STUH2());
            cur.Descend("foo2").Constrain("str2");
            cur = cur.Descend("h3");
            cur.Constrain(new STUH3());
            cur.Descend("foo3").Constrain("str3");
            SodaTestUtil.ExpectOne(q, _array[5]);
        }

        public virtual void TestTwoLevelOr()
        {
            var q = NewQuery();
            q.Constrain(new STUH1TestCase
                ("str1"));
            q.Descend("foo1").Constraints().Or(q.Descend("h2").Descend("h3").Descend("foo3").
                Constrain("str3"));
            Expect(q, new[] {1, 4, 5});
        }

        public virtual void TestThreeLevelOr()
        {
            var q = NewQuery();
            q.Constrain(new STUH1TestCase
                ("str1"));
            q.Descend("foo1").Constraints().Or(q.Descend("h2").Descend("foo2").Constrain("str2"
                )).Or(q.Descend("h2").Descend("h3").Descend("foo3").Constrain("str3"));
            Expect(q, new[] {1, 3, 4, 5});
        }

        public virtual void TestNonExistentDescendant()
        {
            var q = NewQuery();
            var constraint
                = new STUH1TestCase(
                    );
            constraint.foo1 = new STETH2();
            q.Constrain(constraint);
            Expect(q, new int[] {});
        }
    }
}