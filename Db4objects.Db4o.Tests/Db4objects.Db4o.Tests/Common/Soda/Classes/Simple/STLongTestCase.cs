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
using Db4objects.Db4o.Tests.Common.Soda.Util;

namespace Db4objects.Db4o.Tests.Common.Soda.Classes.Simple
{
    public class STLongTestCase : SodaBaseTestCase
    {
        public long i_long;

        public STLongTestCase()
        {
        }

        private STLongTestCase(long a_long)
        {
            i_long = a_long;
        }

        public override object[] CreateData()
        {
            return new object[]
            {
                new STLongTestCase
                    (long.MinValue),
                new STLongTestCase
                    (-1),
                new STLongTestCase(0), new
                    STLongTestCase(long.MaxValue -
                                   1)
            };
        }

        public virtual void TestEquals()
        {
            var q = NewQuery();
            q.Constrain(new STLongTestCase(long.MinValue
                ));
            SodaTestUtil.Expect(q, new object[]
            {
                new STLongTestCase
                    (long.MinValue)
            });
        }

        public virtual void TestGreater()
        {
            var q = NewQuery();
            q.Constrain(new STLongTestCase(-
                1));
            q.Descend("i_long").Constraints().Greater();
            Expect(q, new[] {2, 3});
        }

        public virtual void TestSmaller()
        {
            var q = NewQuery();
            q.Constrain(new STLongTestCase(1
                ));
            q.Descend("i_long").Constraints().Smaller();
            Expect(q, new[] {0, 1, 2});
        }

        public virtual void TestBetween()
        {
            var q = NewQuery();
            q.Constrain(new STLongTestCase()
                );
            var sub = q.Descend("i_long");
            sub.Constrain(Convert.ToInt64(-3)).Greater();
            sub.Constrain(Convert.ToInt64(3)).Smaller();
            Expect(q, new[] {1, 2});
        }

        public virtual void TestAnd()
        {
            var q = NewQuery();
            q.Constrain(new STLongTestCase()
                );
            var sub = q.Descend("i_long");
            sub.Constrain(Convert.ToInt64(-3)).Greater().And(sub.Constrain(Convert.ToInt64
                (3)).Smaller());
            Expect(q, new[] {1, 2});
        }

        public virtual void TestOr()
        {
            var q = NewQuery();
            q.Constrain(new STLongTestCase()
                );
            var sub = q.Descend("i_long");
            sub.Constrain(Convert.ToInt64(3)).Greater().Or(sub.Constrain(Convert.ToInt64
                (-3)).Smaller());
            Expect(q, new[] {0, 3});
        }
    }
}