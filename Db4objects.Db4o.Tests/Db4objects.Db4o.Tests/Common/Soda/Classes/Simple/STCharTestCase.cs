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
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Soda.Classes.Simple
{
    public class STCharTestCase : SodaBaseTestCase
    {
        internal static readonly string Descendant = "i_char";
        public char i_char;

        public STCharTestCase()
        {
        }

        private STCharTestCase(char a_char)
        {
            i_char = a_char;
        }

        public override object[] CreateData()
        {
            return new object[]
            {
                new STCharTestCase
                    ((char) 0),
                new STCharTestCase((
                    char) 1),
                new STCharTestCase((char
                    ) 99),
                new STCharTestCase((char)
                    909)
            };
        }

        public virtual void TestEquals()
        {
            var q = NewQuery();
            q.Constrain(new STCharTestCase((
                char) 0));
            // Primitive default values are ignored, so we need an 
            // additional constraint:
            q.Descend(Descendant).Constrain((char) 0);
            SodaTestUtil.ExpectOne(q, _array[0]);
        }

        public virtual void TestNotEquals()
        {
            var q = NewQuery();
            q.Constrain(_array[0]);
            q.Descend(Descendant).Constrain((char) 0).Not();
            Expect(q, new[] {1, 2, 3});
        }

        public virtual void TestGreater()
        {
            var q = NewQuery();
            q.Constrain(new STCharTestCase((
                char) 9));
            q.Descend(Descendant).Constraints().Greater();
            Expect(q, new[] {2, 3});
        }

        public virtual void TestSmaller()
        {
            var q = NewQuery();
            q.Constrain(new STCharTestCase((
                char) 1));
            q.Descend(Descendant).Constraints().Smaller();
            SodaTestUtil.ExpectOne(q, _array[0]);
        }

        public virtual void TestIdentity()
        {
            var q = NewQuery();
            q.Constrain(new STCharTestCase((
                char) 1));
            var set = q.Execute();
            var identityConstraint
                = (STCharTestCase) set.Next();
            identityConstraint.i_char = (char) 9999;
            q = NewQuery();
            q.Constrain(identityConstraint).Identity();
            identityConstraint.i_char = (char) 1;
            SodaTestUtil.ExpectOne(q, _array[1]);
        }

        public virtual void TestNotIdentity()
        {
            var q = NewQuery();
            q.Constrain(new STCharTestCase((
                char) 1));
            var set = q.Execute();
            var identityConstraint
                = (STCharTestCase) set.Next();
            identityConstraint.i_char = (char) 9080;
            q = NewQuery();
            q.Constrain(identityConstraint).Identity().Not();
            identityConstraint.i_char = (char) 1;
            Expect(q, new[] {0, 2, 3});
        }

        public virtual void TestConstraints()
        {
            var q = NewQuery();
            q.Constrain(new STCharTestCase((
                char) 1));
            q.Constrain(new STCharTestCase((
                char) 0));
            var cs = q.Constraints();
            var csa = cs.ToArray();
            if (csa.Length != 2)
            {
                Assert.Fail("Constraints not returned");
            }
        }
    }
}