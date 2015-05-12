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
    public class CascadeOnActivateTestCase : Db4oClientServerTestCase
    {
        public static void Main(string[] args)
        {
            new CascadeOnActivateTestCase().RunConcurrency();
        }

        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(typeof (Item)).CascadeOnActivate(true
                );
        }

        protected override void Store()
        {
            var item = new Item();
            item.name = "1";
            item.child = new Item();
            item.child.name = "2";
            item.child.child = new Item();
            item.child.child.name = "3";
            Store(item);
        }

        public virtual void Conc(IExtObjectContainer oc)
        {
            var q = oc.Query();
            q.Constrain(typeof (Item));
            q.Descend("name").Constrain("1");
            var os = q.Execute();
            var item = (Item) os.Next();
            var item3 = item.child.child;
            Assert.AreEqual("3", item3.name);
            oc.Deactivate(item, int.MaxValue);
            Assert.IsNull(item3.name);
            oc.Activate(item, 1);
            Assert.AreEqual("3", item3.name);
        }

        public class Item
        {
            public Item child;
            public string name;
        }
    }
}

#endif // !SILVERLIGHT