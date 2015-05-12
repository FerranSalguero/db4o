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
    public class STArrStringONTestCase : SodaBaseTestCase
    {
        public object strArr;

        public STArrStringONTestCase()
        {
        }

        public STArrStringONTestCase(object[][][] arr)
        {
            strArr = arr;
        }

        public override object[] CreateData()
        {
            var arr = new
                STArrStringONTestCase[5];
            arr[0] = new STArrStringONTestCase
                ();
            string[][][] content = {new[] {new string[2]}};
            arr[1] = new STArrStringONTestCase
                (content);
            content = new[] {new[] {new string[3], new string[3]}};
            arr[2] = new STArrStringONTestCase
                (content);
            content = new[] {new[] {new string[3], new string[3]}};
            content[0][0][1] = "foo";
            content[0][1][0] = "bar";
            content[0][1][2] = "fly";
            arr[3] = new STArrStringONTestCase
                (content);
            content = new[] {new[] {new string[3], new string[3]}};
            content[0][0][0] = "bar";
            content[0][1][0] = "wohay";
            content[0][1][1] = "johy";
            arr[4] = new STArrStringONTestCase
                (content);
            var ret = new object[arr.Length];
            Array.Copy(arr, 0, ret, 0, arr.Length);
            return ret;
        }

        public virtual void TestDefaultContainsOne()
        {
            var q = NewQuery();
            string[][][] content = {new[] {new string[1]}};
            content[0][0][0] = "bar";
            q.Constrain(new STArrStringONTestCase
                (content));
            Expect(q, new[] {3, 4});
        }

        public virtual void TestDefaultContainsTwo()
        {
            var q = NewQuery();
            string[][][] content = {new[] {new string[1]}, new[] {new string[1]}};
            content[0][0][0] = "bar";
            content[1][0][0] = "foo";
            q.Constrain(new STArrStringONTestCase
                (content));
            Expect(q, new[] {3});
        }

        public virtual void TestDescendOne()
        {
            var q = NewQuery();
            q.Constrain(typeof (STArrStringONTestCase
                ));
            q.Descend("strArr").Constrain("bar");
            Expect(q, new[] {3, 4});
        }

        public virtual void TestDescendTwo()
        {
            var q = NewQuery();
            q.Constrain(typeof (STArrStringONTestCase
                ));
            var qElements = q.Descend("strArr");
            qElements.Constrain("foo");
            qElements.Constrain("bar");
            Expect(q, new[] {3});
        }

        public virtual void TestDescendOneNot()
        {
            var q = NewQuery();
            q.Constrain(typeof (STArrStringONTestCase
                ));
            q.Descend("strArr").Constrain("bar").Not();
            Expect(q, new[] {0, 1, 2});
        }

        public virtual void TestDescendTwoNot()
        {
            var q = NewQuery();
            q.Constrain(typeof (STArrStringONTestCase
                ));
            var qElements = q.Descend("strArr");
            qElements.Constrain("foo").Not();
            qElements.Constrain("bar").Not();
            Expect(q, new[] {0, 1, 2});
        }
    }
}