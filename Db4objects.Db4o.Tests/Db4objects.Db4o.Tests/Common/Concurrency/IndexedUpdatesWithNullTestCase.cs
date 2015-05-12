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
using Db4oUnit;
using Db4oUnit.Extensions;
using Sharpen.Lang;

namespace Db4objects.Db4o.Tests.Common.Concurrency
{
    public class IndexedUpdatesWithNullTestCase : Db4oClientServerTestCase
    {
        public string str;

        public IndexedUpdatesWithNullTestCase()
        {
        }

        public IndexedUpdatesWithNullTestCase(string str)
        {
            this.str = str;
        }

        public static void Main(string[] args)
        {
            new IndexedUpdatesWithNullTestCase().RunConcurrency
                ();
        }

        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(this).ObjectField("str").Indexed(true);
        }

        protected override void Store()
        {
            Store(new IndexedUpdatesWithNullTestCase
                ("one"));
            Store(new IndexedUpdatesWithNullTestCase
                ("two"));
            Store(new IndexedUpdatesWithNullTestCase
                ("three"));
            Store(new IndexedUpdatesWithNullTestCase
                (null));
            Store(new IndexedUpdatesWithNullTestCase
                (null));
            Store(new IndexedUpdatesWithNullTestCase
                (null));
            Store(new IndexedUpdatesWithNullTestCase
                (null));
            Store(new IndexedUpdatesWithNullTestCase
                ("four"));
        }

        public virtual void Conc1(IExtObjectContainer oc)
        {
            var q = oc.Query();
            q.Constrain(typeof (IndexedUpdatesWithNullTestCase
                ));
            q.Descend("str").Constrain(null);
            var objectSet = q.Execute();
            Assert.AreEqual(4, objectSet.Count);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void Conc2(IExtObjectContainer oc)
        {
            var q = oc.Query();
            q.Constrain(typeof (IndexedUpdatesWithNullTestCase
                ));
            q.Descend("str").Constrain(null);
            var objectSet = q.Execute();
            if (objectSet.Count == 0)
            {
                // already set by other threads
                return;
            }
            Assert.AreEqual(4, objectSet.Count);
            // wait for other threads
            Thread.Sleep(500);
            while (objectSet.HasNext())
            {
                var iuwn = (IndexedUpdatesWithNullTestCase
                    ) objectSet.Next();
                iuwn.str = "hi";
                oc.Store(iuwn);
                Thread.Sleep(100);
            }
        }

        public virtual void Check2(IExtObjectContainer oc)
        {
            var q1 = oc.Query();
            q1.Constrain(typeof (IndexedUpdatesWithNullTestCase
                ));
            q1.Descend("str").Constrain(null);
            var objectSet1 = q1.Execute();
            Assert.AreEqual(0, objectSet1.Count);
            var q2 = oc.Query();
            q2.Constrain(typeof (IndexedUpdatesWithNullTestCase
                ));
            q2.Descend("str").Constrain("hi");
            var objectSet2 = q2.Execute();
            Assert.AreEqual(4, objectSet2.Count);
        }
    }
}

#endif // !SILVERLIGHT