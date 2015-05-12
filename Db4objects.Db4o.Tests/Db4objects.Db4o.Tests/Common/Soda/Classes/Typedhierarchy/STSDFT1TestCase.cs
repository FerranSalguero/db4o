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

namespace Db4objects.Db4o.Tests.Common.Soda.Classes.Typedhierarchy
{
    /// <summary>SDFT: Same descendant field typed</summary>
    public class STSDFT1TestCase : SodaBaseTestCase
    {
        public override object[] CreateData()
        {
            return new object[]
            {
                new STSDFT1TestCase
                    (),
                new STSDFT2(), new STSDFT2("str1"), new STSDFT2("str2"), new STSDFT3(), new
                    STSDFT3("str1"),
                new STSDFT3("str3")
            };
        }

        public virtual void TestStrNull()
        {
            var q = NewQuery();
            q.Constrain(new STSDFT1TestCase
                ());
            q.Descend("foo").Constrain(null);
            Expect(q, new[] {0, 1, 4});
        }

        public virtual void TestStrVal()
        {
            var q = NewQuery();
            q.Constrain(typeof (STSDFT1TestCase
                ));
            q.Descend("foo").Constrain("str1");
            Expect(q, new[] {2, 5});
        }

        public virtual void TestOrValue()
        {
            var q = NewQuery();
            q.Constrain(typeof (STSDFT1TestCase
                ));
            var foo = q.Descend("foo");
            foo.Constrain("str1").Or(foo.Constrain("str2"));
            Expect(q, new[] {2, 3, 5});
        }

        public virtual void TestOrNull()
        {
            var q = NewQuery();
            q.Constrain(typeof (STSDFT1TestCase
                ));
            var foo = q.Descend("foo");
            foo.Constrain("str1").Or(foo.Constrain(null));
            Expect(q, new[] {0, 1, 2, 4, 5});
        }

        public virtual void TestTripleOrNull()
        {
            var q = NewQuery();
            q.Constrain(typeof (STSDFT1TestCase
                ));
            var foo = q.Descend("foo");
            foo.Constrain("str1").Or(foo.Constrain(null)).Or(foo.Constrain("str2"));
            Expect(q, new[] {0, 1, 2, 3, 4, 5});
        }
    }
}