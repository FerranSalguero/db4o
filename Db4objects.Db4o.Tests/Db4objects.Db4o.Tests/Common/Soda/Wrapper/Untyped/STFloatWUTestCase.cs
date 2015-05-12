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

namespace Db4objects.Db4o.Tests.Common.Soda.Wrapper.Untyped
{
    public class STFloatWUTestCase : SodaBaseTestCase
    {
        public object i_float;

        public STFloatWUTestCase()
        {
        }

        private STFloatWUTestCase(float a_float)
        {
            i_float = a_float;
        }

        public override object[] CreateData()
        {
            return new object[]
            {
                new STFloatWUTestCase
                    (float.MinValue),
                new STFloatWUTestCase
                    ((float) 0.0000123),
                new STFloatWUTestCase
                    ((float) 1.345),
                new STFloatWUTestCase
                    (float.MaxValue)
            };
        }

        public virtual void TestEquals()
        {
            var q = NewQuery();
            q.Constrain(_array[0]);
            SodaTestUtil.ExpectOne(q, _array[0]);
        }

        public virtual void TestGreater()
        {
            var q = NewQuery();
            q.Constrain(new STFloatWUTestCase
                ((float) 0.1));
            q.Descend("i_float").Constraints().Greater();
            Expect(q, new[] {2, 3});
        }

        public virtual void TestSmaller()
        {
            var q = NewQuery();
            q.Constrain(new STFloatWUTestCase
                ((float) 1.5));
            q.Descend("i_float").Constraints().Smaller();
            Expect(q, new[] {0, 1, 2});
        }
    }
}