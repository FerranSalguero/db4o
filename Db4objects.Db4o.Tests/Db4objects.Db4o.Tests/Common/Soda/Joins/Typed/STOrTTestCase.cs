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

namespace Db4objects.Db4o.Tests.Common.Soda.Joins.Typed
{
    public class STOrTTestCase : SodaBaseTestCase
    {
        public int orInt;
        public string orString;

        public STOrTTestCase()
        {
        }

        private STOrTTestCase(int a_int, string a_string)
        {
            orInt = a_int;
            orString = a_string;
        }

        public override string ToString()
        {
            return "STOr: int:" + orInt + " str:" + orString;
        }

        public override object[] CreateData()
        {
            return new object[]
            {
                new STOrTTestCase
                    (0, "hi"),
                new STOrTTestCase(5, null
                    ),
                new STOrTTestCase(1000, "joho")
                , new STOrTTestCase(30000, "osoo")
                , new STOrTTestCase(int.MaxValue -
                                    1, null)
            };
        }

        public virtual void TestSmallerGreater()
        {
            var q = NewQuery();
            q.Constrain(new STOrTTestCase());
            var sub = q.Descend("orInt");
            sub.Constrain(30000).Greater().Or(sub.Constrain(5).Smaller());
            Expect(q, new[] {0, 4});
        }

        public virtual void TestGreaterGreater()
        {
            var q = NewQuery();
            q.Constrain(new STOrTTestCase());
            var sub = q.Descend("orInt");
            sub.Constrain(30000).Greater().Or(sub.Constrain(5).Greater());
            Expect(q, new[] {2, 3, 4});
        }

        public virtual void TestGreaterEquals()
        {
            var q = NewQuery();
            q.Constrain(new STOrTTestCase());
            var sub = q.Descend("orInt");
            sub.Constrain(1000).Greater().Or(sub.Constrain(0));
            Expect(q, new[] {0, 3, 4});
        }

        public virtual void TestEqualsNull()
        {
            var q = NewQuery();
            q.Constrain(new STOrTTestCase(1000,
                null));
            q.Descend("orInt").Constraints().Or(q.Descend("orString").Constrain(null));
            Expect(q, new[] {1, 2, 4});
        }

        public virtual void TestAndOrAnd()
        {
            var q = NewQuery();
            q.Constrain(new STOrTTestCase(0, null
                ));
            (q.Descend("orInt").Constrain(5).And(q.Descend("orString").Constrain(null))).Or(q
                .Descend("orInt").Constrain(1000).And(q.Descend("orString").Constrain("joho")));
            Expect(q, new[] {1, 2});
        }

        public virtual void TestOrAndOr()
        {
            var q = NewQuery();
            q.Constrain(new STOrTTestCase(0, null
                ));
            (q.Descend("orInt").Constrain(5).Or(q.Descend("orString").Constrain(null))).And(q
                .Descend("orInt").Constrain(int.MaxValue - 1).Or(q.Descend("orString").Constrain
                    ("joho")));
            SodaTestUtil.ExpectOne(q, _array[4]);
        }

        public virtual void TestOrOrAnd()
        {
            var q = NewQuery();
            q.Constrain(new STOrTTestCase(0, null
                ));
            (q.Descend("orInt").Constrain(int.MaxValue - 1).Or(q.Descend("orString").Constrain
                ("joho"))).Or(q.Descend("orInt").Constrain(5).And(q.Descend("orString").Constrain
                    (null)));
            Expect(q, new[] {1, 2, 4});
        }

        public virtual void TestMultiOrAnd()
        {
            var q = NewQuery();
            q.Constrain(new STOrTTestCase(0, null
                ));
            ((q.Descend("orInt").Constrain(int.MaxValue - 1).Or(q.Descend("orString").Constrain
                ("joho"))).Or(q.Descend("orInt").Constrain(5).And(q.Descend("orString").Constrain
                    ("joho")))).Or((q.Descend("orInt").Constrain(int.MaxValue - 1).Or(q.Descend("orString"
                        ).Constrain(null))).And(q.Descend("orInt").Constrain(5).Or(q.Descend("orString")
                            .Constrain(null))));
            Expect(q, new[] {1, 2, 4});
        }

        public virtual void TestNotSmallerGreater()
        {
            var q = NewQuery();
            q.Constrain(new STOrTTestCase());
            var sub = q.Descend("orInt");
            (sub.Constrain(30000).Greater().Or(sub.Constrain(5).Smaller())).Not();
            Expect(q, new[] {1, 2, 3});
        }

        public virtual void TestNotGreaterGreater()
        {
            var q = NewQuery();
            q.Constrain(new STOrTTestCase());
            var sub = q.Descend("orInt");
            (sub.Constrain(30000).Greater().Or(sub.Constrain(5).Greater())).Not();
            Expect(q, new[] {0, 1});
        }

        public virtual void TestNotGreaterEquals()
        {
            var q = NewQuery();
            q.Constrain(new STOrTTestCase());
            var sub = q.Descend("orInt");
            (sub.Constrain(1000).Greater().Or(sub.Constrain(0))).Not();
            Expect(q, new[] {1, 2});
        }

        public virtual void TestNotEqualsNull()
        {
            var q = NewQuery();
            q.Constrain(new STOrTTestCase(1000,
                null));
            (q.Descend("orInt").Constraints().Or(q.Descend("orString").Constrain(null))).Not(
                );
            Expect(q, new[] {0, 3});
        }

        public virtual void TestNotAndOrAnd()
        {
            var q = NewQuery();
            q.Constrain(new STOrTTestCase(0, null
                ));
            (q.Descend("orInt").Constrain(5).And(q.Descend("orString").Constrain(null))).Or(q
                .Descend("orInt").Constrain(1000).And(q.Descend("orString").Constrain("joho"))).
                Not();
            Expect(q, new[] {0, 3, 4});
        }

        public virtual void TestNotOrAndOr()
        {
            var q = NewQuery();
            q.Constrain(new STOrTTestCase(0, null
                ));
            (q.Descend("orInt").Constrain(5).Or(q.Descend("orString").Constrain(null))).And(q
                .Descend("orInt").Constrain(int.MaxValue - 1).Or(q.Descend("orString").Constrain
                    ("joho"))).Not();
            Expect(q, new[] {0, 1, 2, 3});
        }

        public virtual void TestNotOrOrAnd()
        {
            var q = NewQuery();
            q.Constrain(new STOrTTestCase(0, null
                ));
            (q.Descend("orInt").Constrain(int.MaxValue - 1).Or(q.Descend("orString").Constrain
                ("joho"))).Or(q.Descend("orInt").Constrain(5).And(q.Descend("orString").Constrain
                    (null))).Not();
            Expect(q, new[] {0, 3});
        }

        public virtual void TestNotMultiOrAnd()
        {
            var q = NewQuery();
            q.Constrain(new STOrTTestCase(0, null
                ));
            ((q.Descend("orInt").Constrain(int.MaxValue - 1).Or(q.Descend("orString").Constrain
                ("joho"))).Or(q.Descend("orInt").Constrain(5).And(q.Descend("orString").Constrain
                    ("joho")))).Or((q.Descend("orInt").Constrain(int.MaxValue - 1).Or(q.Descend("orString"
                        ).Constrain(null))).And(q.Descend("orInt").Constrain(5).Or(q.Descend("orString")
                            .Constrain(null)))).Not();
            Expect(q, new[] {0, 3});
        }

        public virtual void TestOrNotAndOr()
        {
            var q = NewQuery();
            q.Constrain(new STOrTTestCase(0, null
                ));
            (q.Descend("orInt").Constrain(int.MaxValue - 1).Or(q.Descend("orString").Constrain
                ("joho"))).Not().And(q.Descend("orInt").Constrain(5).Or(q.Descend("orString").Constrain
                    (null)));
            Expect(q, new[] {1});
        }

        public virtual void TestAndNotAndAnd()
        {
            var q = NewQuery();
            q.Constrain(new STOrTTestCase(0, null
                ));
            (q.Descend("orInt").Constrain(int.MaxValue - 1).And(q.Descend("orString").Constrain
                (null))).Not().And(q.Descend("orInt").Constrain(5).Or(q.Descend("orString").Constrain
                    ("osoo")));
            Expect(q, new[] {1, 3});
        }
    }
}