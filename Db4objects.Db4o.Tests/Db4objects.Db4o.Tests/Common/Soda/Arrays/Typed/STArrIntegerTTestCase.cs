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
    public class STArrIntegerTTestCase : SodaBaseTestCase
    {
        public int[] intArr;

        public STArrIntegerTTestCase()
        {
        }

        public STArrIntegerTTestCase(int[] arr)
        {
            intArr = arr;
        }

        public static void Main(string[] args)
        {
            new STArrIntegerTTestCase().RunSolo
                ();
        }

        public override object[] CreateData()
        {
            return new object[]
            {
                new STArrIntegerTTestCase
                    (),
                new STArrIntegerTTestCase(new
                    int[0]),
                new STArrIntegerTTestCase
                    (new[] {0, 0}),
                new STArrIntegerTTestCase
                    (new[] {1, 17, int.MaxValue - 1}),
                new STArrIntegerTTestCase
                    (new[] {3, 17, 25, int.MaxValue - 2})
            };
        }

        public virtual void _testDefaultContainsOne()
        {
            var q = NewQuery();
            q.Constrain(new STArrIntegerTTestCase
                (new[] {17}));
            Expect(q, new[] {3, 4});
        }

        public virtual void _testDefaultContainsTwo()
        {
            var q = NewQuery();
            q.Constrain(new STArrIntegerTTestCase
                (new[] {17, 25}));
            Expect(q, new[] {4});
        }

        public virtual void TestDescendOne()
        {
            var q = NewQuery();
            q.Constrain(typeof (STArrIntegerTTestCase
                ));
            q.Descend("intArr").Constrain(17);
            Expect(q, new[] {3, 4});
        }

        public virtual void TestDescendTwo()
        {
            var q = NewQuery();
            q.Constrain(typeof (STArrIntegerTTestCase
                ));
            var qElements = q.Descend("intArr");
            qElements.Constrain(17);
            qElements.Constrain(25);
            Expect(q, new[] {4});
        }

        public virtual void TestDescendSmaller()
        {
            var q = NewQuery();
            q.Constrain(typeof (STArrIntegerTTestCase
                ));
            var qElements = q.Descend("intArr");
            qElements.Constrain(3).Smaller();
            Expect(q, new[] {2, 3});
        }

        public virtual void TestDescendNotSmaller()
        {
            var q = NewQuery();
            q.Constrain(typeof (STArrIntegerTTestCase
                ));
            var qElements = q.Descend("intArr");
            qElements.Constrain(3).Smaller();
            Expect(q, new[] {2, 3});
        }
    }
}