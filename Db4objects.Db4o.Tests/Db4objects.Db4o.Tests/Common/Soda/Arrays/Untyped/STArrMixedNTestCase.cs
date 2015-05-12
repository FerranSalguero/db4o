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

namespace Db4objects.Db4o.Tests.Common.Soda.Arrays.Untyped
{
    public class STArrMixedNTestCase : SodaBaseTestCase
    {
        public object[][][] arr;

        public STArrMixedNTestCase()
        {
        }

        public STArrMixedNTestCase(object[][][] arr)
        {
            this.arr = arr;
        }

        public override object[] CreateData()
        {
            var arrMixed =
                new STArrMixedNTestCase[5];
            arrMixed[0] = new STArrMixedNTestCase
                ();
            object[][][] content = {new[] {new object[2]}};
            arrMixed[1] = new STArrMixedNTestCase
                (content);
            content = new[] {new[] {new object[3], new object[3]}, new[] {new object[3], new object[3]}};
            arrMixed[2] = new STArrMixedNTestCase
                (content);
            content = new[] {new[] {new object[3], new object[3]}, new[] {new object[3], new object[3]}};
            content[0][0][1] = "foo";
            content[0][1][0] = "bar";
            content[0][1][2] = "fly";
            content[1][0][0] = false;
            arrMixed[3] = new STArrMixedNTestCase
                (content);
            content = new[] {new[] {new object[3], new object[3]}, new[] {new object[3], new object[3]}};
            content[0][0][0] = "bar";
            content[0][1][0] = "wohay";
            content[0][1][1] = "johy";
            content[1][0][0] = 12;
            arrMixed[4] = new STArrMixedNTestCase
                (content);
            var ret = new object[arrMixed.Length];
            Array.Copy(arrMixed, 0, ret, 0, arrMixed.Length);
            return ret;
        }

        public virtual void TestDefaultContainsString()
        {
            var q = NewQuery();
            object[][][] content = {new[] {new object[1]}};
            content[0][0][0] = "bar";
            q.Constrain(new STArrMixedNTestCase
                (content));
            Expect(q, new[] {3, 4});
        }

        public virtual void TestDefaultContainsInteger()
        {
            var q = NewQuery();
            object[][][] content = {new[] {new object[1]}};
            content[0][0][0] = 12;
            q.Constrain(new STArrMixedNTestCase
                (content));
            Expect(q, new[] {4});
        }

        public virtual void TestDefaultContainsBoolean()
        {
            var q = NewQuery();
            object[][][] content = {new[] {new object[1]}};
            content[0][0][0] = false;
            q.Constrain(new STArrMixedNTestCase
                (content));
            Expect(q, new[] {3});
        }

        public virtual void TestDefaultContainsTwo()
        {
            var q = NewQuery();
            object[][][] content = {new[] {new object[1]}, new[] {new object[1]}};
            content[0][0][0] = "bar";
            content[1][0][0] = 12;
            q.Constrain(new STArrMixedNTestCase
                (content));
            Expect(q, new[] {4});
        }

        public virtual void TestDescendOne()
        {
            var q = NewQuery();
            q.Constrain(typeof (STArrMixedNTestCase
                ));
            q.Descend("arr").Constrain("bar");
            Expect(q, new[] {3, 4});
        }

        public virtual void TestDescendTwo()
        {
            var q = NewQuery();
            q.Constrain(typeof (STArrMixedNTestCase
                ));
            var qElements = q.Descend("arr");
            qElements.Constrain("foo");
            qElements.Constrain("bar");
            Expect(q, new[] {3});
        }

        public virtual void TestDescendOneNot()
        {
            var q = NewQuery();
            q.Constrain(typeof (STArrMixedNTestCase
                ));
            q.Descend("arr").Constrain("bar").Not();
            Expect(q, new[] {0, 1, 2});
        }

        public virtual void TestDescendTwoNot()
        {
            var q = NewQuery();
            q.Constrain(typeof (STArrMixedNTestCase
                ));
            var qElements = q.Descend("arr");
            qElements.Constrain("foo").Not();
            qElements.Constrain("bar").Not();
            Expect(q, new[] {0, 1, 2});
        }
    }
}