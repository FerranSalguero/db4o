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

namespace Db4objects.Db4o.Tests.Common.Soda.Arrays.Typed
{
    public class STArrIntegerWTTestCase : SodaBaseTestCase
    {
        public int[] intArr;

        public STArrIntegerWTTestCase()
        {
        }

        public STArrIntegerWTTestCase(int[] arr)
        {
            intArr = arr;
        }

        public override object[] CreateData()
        {
            return new object[]
            {
                new STArrIntegerWTTestCase
                    (),
                new STArrIntegerWTTestCase(new
                    int[0]),
                new STArrIntegerWTTestCase
                    (new[] {0, 0}),
                new STArrIntegerWTTestCase
                    (new[] {1, 17, int.MaxValue - 1}),
                new STArrIntegerWTTestCase
                    (new[] {3, 17, 25, int.MaxValue - 2})
            };
        }

        public virtual void TestDefaultContainsOne()
        {
            var q = NewQuery();
            q.Constrain(new STArrIntegerWTTestCase
                (new[] {17}));
            Expect(q, new[] {3, 4});
        }

        public virtual void TestDefaultContainsTwo()
        {
            var q = NewQuery();
            q.Constrain(new STArrIntegerWTTestCase
                (new[] {17, 25}));
            Expect(q, new[] {4});
        }
    }
}