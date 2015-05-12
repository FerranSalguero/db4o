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

namespace Db4objects.Db4o.Tests.Common.Soda.Arrays.Object
{
    public class STArrIntegerONTestCase : SodaBaseTestCase
    {
        public object intArr;

        public STArrIntegerONTestCase()
        {
        }

        public STArrIntegerONTestCase(int[][][] arr)
        {
            intArr = arr;
        }

        public override object[] CreateData()
        {
            var arr = new
                STArrIntegerONTestCase[5];
            arr[0] = new STArrIntegerONTestCase
                ();
            int[][][] content = {};
            arr[1] = new STArrIntegerONTestCase
                (content);
            content = new[] {new[] {new int[3], new int[3]}};
            content[0][0][1] = 0;
            content[0][1][0] = 0;
            arr[2] = new STArrIntegerONTestCase
                (content);
            content = new[] {new[] {new int[3], new int[3]}};
            content[0][0][0] = 1;
            content[0][1][0] = 17;
            content[0][1][1] = int.MaxValue - 1;
            arr[3] = new STArrIntegerONTestCase
                (content);
            content = new[] {new[] {new int[2], new int[2]}};
            content[0][0][0] = 3;
            content[0][0][1] = 17;
            content[0][1][0] = 25;
            content[0][1][1] = int.MaxValue - 2;
            arr[4] = new STArrIntegerONTestCase
                (content);
            var ret = new object[arr.Length];
            Array.Copy(arr, 0, ret, 0, arr.Length);
            return ret;
        }

        public virtual void TestDefaultContainsOne()
        {
            var q = NewQuery();
            int[][][] content = {new[] {new int[1]}};
            content[0][0][0] = 17;
            q.Constrain(new STArrIntegerONTestCase
                (content));
            Expect(q, new[] {3, 4});
        }

        public virtual void TestDefaultContainsTwo()
        {
            var q = NewQuery();
            int[][][] content =
            {
                new[] {new int[1]}, new[]
                {
                    new
                        int[1]
                }
            };
            content[0][0][0] = 17;
            content[1][0][0] = 25;
            q.Constrain(new STArrIntegerONTestCase
                (content));
            Expect(q, new[] {4});
        }

        public virtual void TestDescendOne()
        {
            var q = NewQuery();
            q.Constrain(typeof (STArrIntegerONTestCase
                ));
            q.Descend("intArr").Constrain(17);
            Expect(q, new[] {3, 4});
        }

        public virtual void TestDescendTwo()
        {
            var q = NewQuery();
            q.Constrain(typeof (STArrIntegerONTestCase
                ));
            var qElements = q.Descend("intArr");
            qElements.Constrain(17);
            qElements.Constrain(25);
            Expect(q, new[] {4});
        }

        public virtual void TestDescendSmaller()
        {
            var q = NewQuery();
            q.Constrain(typeof (STArrIntegerONTestCase
                ));
            var qElements = q.Descend("intArr");
            qElements.Constrain(3).Smaller();
            Expect(q, new[] {2, 3});
        }

        public virtual void TestDescendNotSmaller()
        {
            var q = NewQuery();
            q.Constrain(typeof (STArrIntegerONTestCase
                ));
            var qElements = q.Descend("intArr");
            qElements.Constrain(3).Smaller();
            Expect(q, new[] {2, 3});
        }
    }
}