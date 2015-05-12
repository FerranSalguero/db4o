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

#if !SILVERLIGHT
using Db4objects.Db4o.Ext;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Concurrency
{
    public class QueryNonExistantTestCase : Db4oClientServerTestCase
    {
        internal QueryNonExistant1 member;

        public QueryNonExistantTestCase()
        {
        }

        public QueryNonExistantTestCase(bool createMembers)
        {
            // db4o constructor
            member = new QueryNonExistant1();
            member.member = new QueryNonExistant2();
            member.member.member = this;
        }

        public static void Main(string[] args)
        {
            new QueryNonExistantTestCase().RunConcurrency
                ();
        }

        // db4o constructor
        public virtual void Conc(IExtObjectContainer oc)
        {
            oc.QueryByExample((new QueryNonExistantTestCase
                (true)));
            AssertOccurrences(oc, typeof (QueryNonExistantTestCase
                ), 0);
            var q = oc.Query();
            q.Constrain(new QueryNonExistantTestCase
                (true));
            Assert.AreEqual(0, q.Execute().Count);
        }

        public class QueryNonExistant1
        {
            internal QueryNonExistant2 member;
        }

        public class QueryNonExistant2 : QueryNonExistant1
        {
            internal QueryNonExistantTestCase member;
        }
    }
}

#endif // !SILVERLIGHT