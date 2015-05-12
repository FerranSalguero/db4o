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
using Db4objects.Db4o.Query;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Concurrency
{
    public class CreateIndexInheritedTestCase : Db4oClientServerTestCase
    {
        public int i_int;

        public CreateIndexInheritedTestCase()
        {
        }

        public CreateIndexInheritedTestCase(int a_int)
        {
            i_int = a_int;
        }

        public static void Main(string[] args)
        {
            new CreateIndexInheritedTestCase().RunConcurrency
                ();
        }

        protected override void Store()
        {
            Store(new CreateIndexFor("a"));
            Store(new CreateIndexFor("c"));
            Store(new CreateIndexFor("b"));
            Store(new CreateIndexFor("f"));
            Store(new CreateIndexFor("e"));
            Store(new CreateIndexFor(1));
            Store(new CreateIndexFor(5));
            Store(new CreateIndexFor(7));
            Store(new CreateIndexFor(3));
            Store(new CreateIndexFor(2));
            Store(new CreateIndexFor(3));
        }

        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(typeof (CreateIndexInheritedTestCase
                )).ObjectField("i_int").Indexed(true);
            config.ObjectClass(typeof (CreateIndexFor)).ObjectField
                ("i_name").Indexed(true);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void Conc1(IExtObjectContainer oc)
        {
            TQueryB(oc);
            TQueryInts(oc, 5);
        }

        public virtual void Conc2(IExtObjectContainer oc)
        {
            oc.Store(new CreateIndexFor("d"));
            TQueryB(oc);
            TUpdateB(oc);
            oc.Store(new CreateIndexFor("z"));
            oc.Store(new CreateIndexFor("y"));
        }

        public virtual void Check2(IExtObjectContainer oc)
        {
            TQueryB(oc);
            TQueryInts(oc, 5 + ThreadCount()*3);
        }

        private void TQueryInts(IExtObjectContainer oc, int expectedZeroSize)
        {
            var q = oc.Query();
            q.Constrain(typeof (CreateIndexFor));
            q.Descend("i_int").Constrain(0);
            var zeroSize = q.Execute().Count;
            Assert.AreEqual(expectedZeroSize, zeroSize);
            q = oc.Query();
            q.Constrain(typeof (CreateIndexFor));
            q.Descend("i_int").Constrain(4).Greater().Equal();
            TExpectInts(q, new[] {5, 7});
            q = oc.Query();
            q.Constrain(typeof (CreateIndexFor));
            q.Descend("i_int").Constrain(4).Greater();
            TExpectInts(q, new[] {5, 7});
            q = oc.Query();
            q.Constrain(typeof (CreateIndexFor));
            q.Descend("i_int").Constrain(3).Greater();
            TExpectInts(q, new[] {5, 7});
            q = oc.Query();
            q.Constrain(typeof (CreateIndexFor));
            q.Descend("i_int").Constrain(3).Greater().Equal();
            TExpectInts(q, new[] {3, 3, 5, 7});
            q = oc.Query();
            q.Constrain(typeof (CreateIndexFor));
            q.Descend("i_int").Constrain(2).Greater().Equal();
            TExpectInts(q, new[] {2, 3, 3, 5, 7});
            q = oc.Query();
            q = oc.Query();
            q.Constrain(typeof (CreateIndexFor));
            q.Descend("i_int").Constrain(2).Greater();
            TExpectInts(q, new[] {3, 3, 5, 7});
            q = oc.Query();
            q.Constrain(typeof (CreateIndexFor));
            q.Descend("i_int").Constrain(1).Greater().Equal();
            TExpectInts(q, new[] {1, 2, 3, 3, 5, 7});
            q = oc.Query();
            q.Constrain(typeof (CreateIndexFor));
            q.Descend("i_int").Constrain(1).Greater();
            TExpectInts(q, new[] {2, 3, 3, 5, 7});
            q = oc.Query();
            q.Constrain(typeof (CreateIndexFor));
            q.Descend("i_int").Constrain(4).Smaller();
            TExpectInts(q, new[] {1, 2, 3, 3}, zeroSize);
            q = oc.Query();
            q.Constrain(typeof (CreateIndexFor));
            q.Descend("i_int").Constrain(4).Smaller().Equal();
            TExpectInts(q, new[] {1, 2, 3, 3}, zeroSize);
            q = oc.Query();
            q.Constrain(typeof (CreateIndexFor));
            q.Descend("i_int").Constrain(3).Smaller();
            TExpectInts(q, new[] {1, 2}, zeroSize);
            q = oc.Query();
            q.Constrain(typeof (CreateIndexFor));
            q.Descend("i_int").Constrain(3).Smaller().Equal();
            TExpectInts(q, new[] {1, 2, 3, 3}, zeroSize);
            q = oc.Query();
            q.Constrain(typeof (CreateIndexFor));
            q.Descend("i_int").Constrain(2).Smaller().Equal();
            TExpectInts(q, new[] {1, 2}, zeroSize);
            q = oc.Query();
            q = oc.Query();
            q.Constrain(typeof (CreateIndexFor));
            q.Descend("i_int").Constrain(2).Smaller();
            TExpectInts(q, new[] {1}, zeroSize);
            q = oc.Query();
            q.Constrain(typeof (CreateIndexFor));
            q.Descend("i_int").Constrain(1).Smaller().Equal();
            TExpectInts(q, new[] {1}, zeroSize);
            q = oc.Query();
            q.Constrain(typeof (CreateIndexFor));
            q.Descend("i_int").Constrain(1).Smaller();
            TExpectInts(q, new int[] {}, zeroSize);
        }

        private void TExpectInts(IQuery q, int[] ints, int zeroSize)
        {
            var res = q.Execute();
            Assert.AreEqual(ints.Length + zeroSize, res.Count);
            while (res.HasNext())
            {
                var ci = (CreateIndexFor
                    ) res.Next();
                for (var i = 0; i < ints.Length; i++)
                {
                    if (ints[i] == ci.i_int)
                    {
                        ints[i] = 0;
                        break;
                    }
                }
            }
            for (var i = 0; i < ints.Length; i++)
            {
                Assert.AreEqual(0, ints[i]);
            }
        }

        private void TExpectInts(IQuery q, int[] ints)
        {
            TExpectInts(q, ints, 0);
        }

        private void TQueryB(IExtObjectContainer oc)
        {
            var res = Query(oc, "b");
            Assert.AreEqual(1, res.Count);
            var ci = (CreateIndexFor
                ) res.Next();
            Assert.AreEqual("b", ci.i_name);
        }

        private void TUpdateB(IExtObjectContainer oc)
        {
            var res = Query(oc, "b");
            var ci = (CreateIndexFor
                ) res.Next();
            ci.i_name = "j";
            oc.Store(ci);
            res = Query(oc, "b");
            Assert.AreEqual(0, res.Count);
            res = Query(oc, "j");
            Assert.AreEqual(1, res.Count);
            ci.i_name = "b";
            oc.Store(ci);
            TQueryB(oc);
        }

        private IObjectSet Query(IExtObjectContainer oc, string n)
        {
            var q = oc.Query();
            q.Constrain(typeof (CreateIndexFor));
            q.Descend("i_name").Constrain(n);
            return q.Execute();
        }

        public class CreateIndexFor : CreateIndexInheritedTestCase
        {
            public string i_name;

            public CreateIndexFor()
            {
            }

            public CreateIndexFor(string name)
            {
                i_name = name;
            }

            public CreateIndexFor(int a_int) : base(a_int)
            {
            }
        }
    }
}

#endif // !SILVERLIGHT