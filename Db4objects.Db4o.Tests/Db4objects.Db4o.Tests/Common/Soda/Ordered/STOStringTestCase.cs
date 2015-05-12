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

namespace Db4objects.Db4o.Tests.Common.Soda.Ordered
{
    public class STOStringTestCase : SodaBaseTestCase
    {
        public string foo;

        public STOStringTestCase()
        {
        }

        public STOStringTestCase(string str)
        {
            foo = str;
        }

        public override string ToString()
        {
            return foo;
        }

        public override object[] CreateData()
        {
            return new object[]
            {
                new STOStringTestCase
                    (null),
                new STOStringTestCase("bbb"),
                new STOStringTestCase("dod"), new STOStringTestCase
                    ("aaa"),
                new STOStringTestCase("Xbb"),
                new STOStringTestCase("bbq")
            };
        }

        public virtual void TestAscending()
        {
            var q = NewQuery();
            q.Constrain(typeof (STOStringTestCase));
            q.Descend("foo").OrderAscending();
            ExpectOrdered(q, new[] {0, 4, 3, 1, 5, 2});
        }

        public virtual void TestDescending()
        {
            var q = NewQuery();
            q.Constrain(typeof (STOStringTestCase));
            q.Descend("foo").OrderDescending();
            ExpectOrdered(q, new[] {2, 5, 1, 3, 4, 0});
        }

        public virtual void TestAscendingLike()
        {
            var q = NewQuery();
            q.Constrain(typeof (STOStringTestCase));
            var qStr = q.Descend("foo");
            qStr.Constrain("b").Like();
            qStr.OrderAscending();
            ExpectOrdered(q, new[] {4, 1, 5});
        }

        public virtual void TestDescendingContains()
        {
            var q = NewQuery();
            q.Constrain(typeof (STOStringTestCase));
            var qStr = q.Descend("foo");
            qStr.Constrain("b").Contains();
            qStr.OrderDescending();
            ExpectOrdered(q, new[] {5, 1, 4});
        }
    }
}