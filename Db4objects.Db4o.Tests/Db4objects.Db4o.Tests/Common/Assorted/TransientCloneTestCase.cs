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

using System.Collections;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Assorted
{
    public class TransientCloneTestCase : AbstractDb4oTestCase
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            var item = new Item();
            item.list = new ArrayList();
            item.list.Add(new Atom("listAtom"));
            item.list.Add(item);
            item.ht = new Hashtable();
            item.ht["htc"] = new Molecule("htAtom");
            item.ht["recurse"] = item;
            item.str = "str";
            item.myInt = 100;
            item.molecules = new Molecule[3];
            for (var i = 0; i < item.molecules.Length; i++)
            {
                item.molecules[i] = new Molecule("arr" + i);
                item.molecules[i].child = new Atom("arr" + i);
                item.molecules[i].child.child = new Atom("arrc" + i);
            }
            Store(item);
        }

        public virtual void Test()
        {
            var item = ((Item) RetrieveOnlyInstance
                (typeof (Item)));
            Db().Activate(item, int.MaxValue);
            var originalValues = PeekPersisted(false);
            Cmp(item, originalValues);
            Db().Deactivate(item, int.MaxValue);
            var modified = PeekPersisted(false);
            Cmp(originalValues, modified);
            Db().Activate(item, int.MaxValue);
            modified.str = "changed";
            modified.molecules[0].name = "changed";
            item.str = "changed";
            item.molecules[0].name = "changed";
            Db().Store(item.molecules[0]);
            Db().Store(item);
            var tc = PeekPersisted(true);
            Cmp(originalValues, tc);
            tc = PeekPersisted(false);
            Cmp(modified, tc);
            Db().Commit();
            tc = PeekPersisted(true);
            Cmp(modified, tc);
        }

        private void Cmp(Item to, Item tc)
        {
            Assert.IsTrue(tc != to);
            Assert.IsTrue(tc.list != to);
            Assert.IsTrue(tc.list.Count == to.list.Count);
            var i = tc.list.GetEnumerator();
            var tca = ((Atom) Next(i));
            var j = to.list.GetEnumerator();
            var tct = ((Atom) Next(j));
            Assert.IsTrue(tca != tct);
            Assert.IsTrue(tca.name.Equals(tct.name));
            Assert.AreSame(Next(i), tc);
            Assert.AreSame(Next(j), to);
            Assert.IsTrue(tc.ht != to.ht);
            var tcm = (Molecule) tc.ht["htc"];
            var tom = (Molecule) to.ht["htc"];
            Assert.IsTrue(tcm != tom);
            Assert.IsTrue(tcm.name.Equals(tom.name));
            Assert.AreSame(tc.ht["recurse"], tc);
            Assert.AreSame(to.ht["recurse"], to);
            Assert.AreEqual(to.str, tc.str);
            Assert.IsTrue(tc.str.Equals(to.str));
            Assert.IsTrue(tc.myInt == to.myInt);
            Assert.IsTrue(tc.molecules.Length == to.molecules.Length);
            Assert.IsTrue(tc.molecules.Length == to.molecules.Length);
            tcm = tc.molecules[0];
            tom = to.molecules[0];
            Assert.IsTrue(tcm != tom);
            Assert.IsTrue(tcm.name.Equals(tom.name));
            Assert.IsTrue(tcm.child != tom.child);
            Assert.IsTrue(tcm.child.name.Equals(tom.child.name));
        }

        private object Next(IEnumerator i)
        {
            Assert.IsTrue(i.MoveNext());
            return i.Current;
        }

        private Item PeekPersisted(bool committed)
        {
            var oc = Db();
            return ((Item) oc.PeekPersisted(((Item
                ) RetrieveOnlyInstance(typeof (Item))), int.MaxValue, committed
                ));
        }

        public class Item
        {
            public Hashtable ht;
            public IList list;
            public Molecule[] molecules;
            public int myInt;
            public string str;
        }
    }
}