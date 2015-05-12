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

namespace Db4objects.Db4o.Tests.Common.Concurrency
{
    public class MultiLevelIndexTestCase : Db4oClientServerTestCase
    {
        public MultiLevelIndexTestCase _child;
        public int _i;
        public int _level;

        public static void Main(string[] args)
        {
            new MultiLevelIndexTestCase().RunConcurrency();
        }

        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(this).ObjectField("_child").Indexed(true);
            config.ObjectClass(this).ObjectField("_i").Indexed(true);
        }

        protected override void Store()
        {
            Store(3);
            Store(2);
            Store(5);
            Store(1);
            for (var i = 6; i < 103; i++)
            {
                Store(i);
            }
        }

        private void Store(int val)
        {
            var root = new MultiLevelIndexTestCase();
            root._i = val;
            root._child = new MultiLevelIndexTestCase();
            root._child._level = 1;
            root._child._i = -val;
            Store(root);
        }

        public virtual void Conc1(IExtObjectContainer oc)
        {
            var q = oc.Query();
            q.Constrain(typeof (MultiLevelIndexTestCase));
            q.Descend("_child").Descend("_i").Constrain(-102);
            var objectSet = q.Execute();
            Assert.AreEqual(1, objectSet.Count);
            var mli = (MultiLevelIndexTestCase) objectSet.Next();
            Assert.AreEqual(102, mli._i);
        }

        public virtual void Conc2(IExtObjectContainer oc, int seq)
        {
            oc.Configure().ObjectClass(typeof (MultiLevelIndexTestCase)).CascadeOnUpdate(true);
            var q = oc.Query();
            q.Constrain(typeof (MultiLevelIndexTestCase));
            q.Descend("_child").Descend("_i").Constrain(seq - 102);
            var objectSet = q.Execute();
            Assert.AreEqual(1, objectSet.Count);
            var mli = (MultiLevelIndexTestCase) objectSet.Next();
            Assert.AreEqual(102 - seq, mli._i);
            mli._child._i = -(seq + 201);
            oc.Store(mli);
        }

        public virtual void Check2(IExtObjectContainer oc)
        {
            var q = oc.Query();
            q.Constrain(typeof (MultiLevelIndexTestCase));
            q.Descend("_child").Descend("_i").Constrain(-200).Smaller();
            var objectSet = q.Execute();
            Assert.AreEqual(ThreadCount(), objectSet.Count);
        }
    }
}

#endif // !SILVERLIGHT