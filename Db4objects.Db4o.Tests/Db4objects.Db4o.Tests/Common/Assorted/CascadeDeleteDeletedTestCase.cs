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

using Db4objects.Db4o.Config;
using Db4objects.Db4o.Ext;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Assorted
{
    public class CascadeDeleteDeletedTestCase : Db4oClientServerTestCase
    {
        public string name;
        public CddMember typedMember;
        public object untypedMember;

        public CascadeDeleteDeletedTestCase()
        {
        }

        public CascadeDeleteDeletedTestCase(string name)
        {
            this.name = name;
        }

        public static void Main(string[] args)
        {
            new CascadeDeleteDeletedTestCase().RunNetworking
                ();
        }

        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(this).CascadeOnDelete(true);
        }

        protected override void Store()
        {
            var oc = Db();
            MembersFirst(oc, "membersFirst commit");
            MembersFirst(oc, "membersFirst");
            TwoRef(oc, "twoRef");
            TwoRef(oc, "twoRef commit");
            TwoRef(oc, "twoRef delete");
            TwoRef(oc, "twoRef delete commit");
        }

        private void MembersFirst(IExtObjectContainer oc, string name)
        {
            var cdd = new CascadeDeleteDeletedTestCase
                (name);
            cdd.untypedMember = new CddMember();
            cdd.typedMember = new CddMember();
            oc.Store(cdd);
        }

        private void TwoRef(IExtObjectContainer oc, string name)
        {
            var cdd = new CascadeDeleteDeletedTestCase
                (name);
            cdd.untypedMember = new CddMember();
            cdd.typedMember = new CddMember();
            var cdd2 = new CascadeDeleteDeletedTestCase
                (name);
            cdd2.untypedMember = cdd.untypedMember;
            cdd2.typedMember = cdd.typedMember;
            oc.Store(cdd);
            oc.Store(cdd2);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void _testDeleteDeleted()
        {
            var total = 10;
            var CddMemberCount = 12;
            var containers = new IExtObjectContainer[total];
            IExtObjectContainer oc = null;
            try
            {
                for (var i = 0; i < total; i++)
                {
                    containers[i] = OpenNewSession();
                    AssertOccurrences(containers[i], typeof (CddMember),
                        CddMemberCount);
                }
                for (var i = 0; i < total; i++)
                {
                    DeleteAll(containers[i], typeof (CddMember));
                }
                oc = OpenNewSession();
                AssertOccurrences(oc, typeof (CddMember), CddMemberCount
                    );
                // ocs[0] deleted all CddMember objects, and committed the change
                containers[0].Commit();
                containers[0].Close();
                // FIXME: following assertion fails
                AssertOccurrences(oc, typeof (CddMember), 0);
                for (var i = 1; i < total; i++)
                {
                    containers[i].Close();
                }
                AssertOccurrences(oc, typeof (CddMember), 0);
            }
            finally
            {
                if (oc != null)
                {
                    oc.Close();
                }
                for (var i = 0; i < total; i++)
                {
                    if (containers[i] != null)
                    {
                        containers[i].Close();
                    }
                }
            }
        }

        public class CddMember
        {
            public string name;
        }
    }
}