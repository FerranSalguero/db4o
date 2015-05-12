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

namespace Db4objects.Db4o.Tests.Common.Concurrency
{
    public class CascadeOnUpdateTestCase : Db4oClientServerTestCase
    {
        private const int AtomCount = 10;

        public static void Main(string[] args)
        {
            new CascadeOnUpdateTestCase().RunConcurrency();
        }

        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(typeof (Item)).CascadeOnUpdate(true);
            config.ObjectClass(typeof (Atom)).CascadeOnUpdate(true);
        }

        protected override void Store()
        {
            var item = new Item();
            item.child = new Atom[AtomCount];
            for (var i = 0; i < AtomCount; i++)
            {
                item.child[i] = new Atom(new Atom("storedChild"), "stored");
            }
            Store(item);
        }

        public virtual void Conc(IExtObjectContainer oc, int seq)
        {
            var item = (Item
                ) RetrieveOnlyInstance(oc, typeof (Item));
            for (var i = 0; i < AtomCount; i++)
            {
                item.child[i].name = "updated" + seq;
                item.child[i].child.name = "updated" + seq;
                oc.Store(item);
            }
        }

        public virtual void Check(IExtObjectContainer oc)
        {
            var item = (Item
                ) RetrieveOnlyInstance(typeof (Item));
            var name = item.child[0].name;
            Assert.IsTrue(name.StartsWith("updated"));
            for (var i = 0; i < AtomCount; i++)
            {
                Assert.AreEqual(name, item.child[i].name);
                Assert.AreEqual(name, item.child[i].child.name);
            }
        }

        public class Item
        {
            public Atom[] child;
        }
    }
}

#endif // !SILVERLIGHT