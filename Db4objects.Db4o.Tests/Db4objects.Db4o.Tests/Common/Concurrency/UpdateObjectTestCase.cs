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
using Db4objects.Db4o.Tests.Common.Persistent;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Concurrency
{
    public class UpdateObjectTestCase : Db4oClientServerTestCase
    {
        private static readonly string testString = "simple test string";
        private static readonly int Count = 100;

        public static void Main(string[] args)
        {
            new UpdateObjectTestCase().RunConcurrency();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            for (var i = 0; i < Count; i++)
            {
                Store(new SimpleObject(testString + i, i));
            }
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void ConcUpdateSameObject(IExtObjectContainer oc, int seq)
        {
            var query = oc.Query();
            query.Descend("_s").Constrain(testString + Count/2);
            var result = query.Execute();
            Assert.AreEqual(1, result.Count);
            var o = (SimpleObject) result.Next();
            o.SetI(Count + seq);
            oc.Store(o);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void CheckUpdateSameObject(IExtObjectContainer oc)
        {
            var query = oc.Query();
            query.Descend("_s").Constrain(testString + Count/2);
            var result = query.Execute();
            Assert.AreEqual(1, result.Count);
            var o = (SimpleObject) result.Next();
            var i = o.GetI();
            Assert.IsTrue(Count <= i && i < Count + ThreadCount());
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void ConcUpdateDifferentObject(IExtObjectContainer oc, int seq)
        {
            var query = oc.Query();
            query.Descend("_s").Constrain(testString + seq).And(query.Descend("_i").Constrain
                (seq));
            var result = query.Execute();
            Assert.AreEqual(1, result.Count);
            var o = (SimpleObject) result.Next();
            o.SetI(seq + Count);
            oc.Store(o);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void CheckUpdateDifferentObject(IExtObjectContainer oc)
        {
            var result = oc.Query(typeof (SimpleObject));
            Assert.AreEqual(Count, result.Count);
            while (result.HasNext())
            {
                var o = (SimpleObject) result.Next();
                var i = o.GetI();
                if (i >= Count)
                {
                    i -= Count;
                }
                Assert.AreEqual(testString + i, o.GetS());
            }
        }
    }
}

#endif // !SILVERLIGHT