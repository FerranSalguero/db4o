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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Tests.Common.Persistent;
using Db4oUnit;
using Db4oUnit.Extensions;
using Sharpen.Lang;

namespace Db4objects.Db4o.Tests.Common.Concurrency
{
    public class IndexedByIdentityTestCase : Db4oClientServerTestCase
    {
        internal const int Count = 10;
        public Atom atom;

        public static void Main(string[] args)
        {
            new IndexedByIdentityTestCase().RunConcurrency();
        }

        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(this).ObjectField("atom").Indexed(true);
            config.ObjectClass(typeof (IndexedByIdentityTestCase)).CascadeOnUpdate(true);
        }

        protected override void Store()
        {
            for (var i = 0; i < Count; i++)
            {
                var ibi = new IndexedByIdentityTestCase();
                ibi.atom = new Atom("ibi" + i);
                Store(ibi);
            }
        }

        public virtual void ConcRead(IExtObjectContainer oc)
        {
            for (var i = 0; i < Count; i++)
            {
                var q = oc.Query();
                q.Constrain(typeof (Atom));
                q.Descend("name").Constrain("ibi" + i);
                var objectSet = q.Execute();
                Assert.AreEqual(1, objectSet.Count);
                var child = (Atom) objectSet.Next();
                q = oc.Query();
                q.Constrain(typeof (IndexedByIdentityTestCase));
                q.Descend("atom").Constrain(child).Identity();
                objectSet = q.Execute();
                Assert.AreEqual(1, objectSet.Count);
                var ibi = (IndexedByIdentityTestCase) objectSet.Next();
                Assert.AreSame(child, ibi.atom);
            }
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void ConcUpdate(IExtObjectContainer oc, int seq)
        {
            var q = oc.Query();
            q.Constrain(typeof (IndexedByIdentityTestCase));
            var os = q.Execute();
            Assert.AreEqual(Count, os.Count);
            while (os.HasNext())
            {
                var idi = (IndexedByIdentityTestCase) os.Next();
                idi.atom.name = "updated" + seq;
                oc.Store(idi);
                Thread.Sleep(100);
            }
        }

        public virtual void CheckUpdate(IExtObjectContainer oc)
        {
            var q = oc.Query();
            q.Constrain(typeof (IndexedByIdentityTestCase));
            var os = q.Execute();
            Assert.AreEqual(Count, os.Count);
            string expected = null;
            while (os.HasNext())
            {
                var idi = (IndexedByIdentityTestCase) os.Next();
                if (expected == null)
                {
                    expected = idi.atom.name;
                    Assert.IsTrue(expected.StartsWith("updated"));
                    Assert.IsTrue(expected.Length > "updated".Length);
                }
                Assert.AreEqual(expected, idi.atom.name);
            }
        }
    }
}

#endif // !SILVERLIGHT