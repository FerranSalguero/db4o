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

using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Assorted
{
    public class DeleteUpdateTestCase : Db4oClientServerTestCase
    {
        public static void Main(string[] args)
        {
            new DeleteUpdateTestCase().RunNetworking();
        }

        protected override void Store()
        {
            Store(new SimpleObject("hello", 1));
        }

        public virtual void _testDS()
        {
            var oc1 = OpenNewSession();
            var oc2 = OpenNewSession();
            var oc3 = OpenNewSession();
            try
            {
                var o1 = (SimpleObject) RetrieveOnlyInstance(oc1, typeof (SimpleObject
                    ));
                oc1.Delete(o1);
                var o2 = (SimpleObject) RetrieveOnlyInstance(oc2, typeof (SimpleObject
                    ));
                Assert.AreEqual("hello", o2.GetS());
                o2.SetS("o2");
                oc2.Store(o2);
                oc1.Commit();
                oc2.Commit();
                o1 = (SimpleObject) RetrieveOnlyInstance(oc1, typeof (SimpleObject));
                oc1.Refresh(o1, int.MaxValue);
                Assert.AreEqual("o2", o1.GetS());
                o2 = (SimpleObject) RetrieveOnlyInstance(oc2, typeof (SimpleObject));
                oc2.Refresh(o2, int.MaxValue);
                Assert.AreEqual("o2", o2.GetS());
                var o3 = (SimpleObject) RetrieveOnlyInstance(oc3, typeof (SimpleObject
                    ));
                oc1.Refresh(o1, int.MaxValue);
                Assert.AreEqual("o2", o3.GetS());
            }
            finally
            {
                oc1.Close();
                oc2.Close();
                oc3.Close();
            }
        }

        public virtual void TestSD()
        {
            var oc1 = OpenNewSession();
            var oc2 = OpenNewSession();
            var oc3 = OpenNewSession();
            try
            {
                var o1 = (SimpleObject) RetrieveOnlyInstance(oc1, typeof (SimpleObject
                    ));
                oc1.Delete(o1);
                var o2 = (SimpleObject) RetrieveOnlyInstance(oc2, typeof (SimpleObject
                    ));
                Assert.AreEqual("hello", o2.GetS());
                o2.SetS("o2");
                oc2.Store(o2);
                oc2.Commit();
                oc1.Commit();
                AssertOccurrences(oc1, typeof (SimpleObject), 0);
                AssertOccurrences(oc2, typeof (SimpleObject), 0);
                AssertOccurrences(oc3, typeof (SimpleObject), 0);
            }
            finally
            {
                oc1.Close();
                oc2.Close();
                oc3.Close();
            }
        }
    }
}