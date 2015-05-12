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

namespace Db4objects.Db4o.Tests.Common.Soda.Arrays.Untyped
{
    public class STArrStringUTestCase : SodaBaseTestCase
    {
        public object[] strArr;

        public STArrStringUTestCase()
        {
        }

        public STArrStringUTestCase(object[] arr)
        {
            strArr = arr;
        }

        public override object[] CreateData()
        {
            return new object[]
            {
                new STArrStringUTestCase
                    (),
                new STArrStringUTestCase(new
                    object[] {null}),
                new STArrStringUTestCase
                    (new object[] {null, null}),
                new STArrStringUTestCase
                    (new object[] {"foo", "bar", "fly"}),
                new STArrStringUTestCase
                    (new object[] {null, "bar", "wohay", "johy"})
            };
        }

        public virtual void TestDefaultContainsOne()
        {
            var q = NewQuery();
            q.Constrain(new STArrStringUTestCase
                (new object[] {"bar"}));
            Expect(q, new[] {3, 4});
        }

        public virtual void TestDefaultContainsTwo()
        {
            var q = NewQuery();
            q.Constrain(new STArrStringUTestCase
                (new object[] {"foo", "bar"}));
            Expect(q, new[] {3});
        }

        public virtual void TestDescendOne()
        {
            var q = NewQuery();
            q.Constrain(typeof (STArrStringUTestCase
                ));
            q.Descend("strArr").Constrain("bar");
            Expect(q, new[] {3, 4});
        }

        public virtual void TestDescendTwo()
        {
            var q = NewQuery();
            q.Constrain(typeof (STArrStringUTestCase
                ));
            var qElements = q.Descend("strArr");
            qElements.Constrain("foo");
            qElements.Constrain("bar");
            Expect(q, new[] {3});
        }

        public virtual void TestDescendOneNot()
        {
            var q = NewQuery();
            q.Constrain(typeof (STArrStringUTestCase
                ));
            q.Descend("strArr").Constrain("bar").Not();
            Expect(q, new[] {0, 1, 2});
        }

        public virtual void TestDescendTwoNot()
        {
            var q = NewQuery();
            q.Constrain(typeof (STArrStringUTestCase
                ));
            var qElements = q.Descend("strArr");
            qElements.Constrain("foo").Not();
            qElements.Constrain("bar").Not();
            Expect(q, new[] {0, 1, 2});
        }
    }
}