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
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Concurrency
{
    public class RefreshTestCase : Db4oClientServerTestCase
    {
        public RefreshTestCase child;
        public string name;

        public RefreshTestCase()
        {
        }

        public RefreshTestCase(string name, RefreshTestCase
            child)
        {
            this.name = name;
            this.child = child;
        }

        public static void Main(string[] args)
        {
            new RefreshTestCase().RunConcurrency();
        }

        protected override void Store()
        {
            var r3 = new RefreshTestCase
                ("o3", null);
            var r2 = new RefreshTestCase
                ("o2", r3);
            var r1 = new RefreshTestCase
                ("o1", r2);
            Store(r1);
        }

        public virtual void Conc(IExtObjectContainer oc)
        {
            var r11 = GetRoot(oc);
            r11.name = "cc";
            oc.Refresh(r11, 0);
            Assert.AreEqual("cc", r11.name);
            oc.Refresh(r11, 1);
            Assert.AreEqual("o1", r11.name);
            r11.child.name = "cc";
            oc.Refresh(r11, 1);
            Assert.AreEqual("cc", r11.child.name);
            oc.Refresh(r11, 2);
            Assert.AreEqual("o2", r11.child.name);
        }

        private RefreshTestCase GetRoot(IObjectContainer
            oc)
        {
            return GetByName(oc, "o1");
        }

        private RefreshTestCase GetByName(IObjectContainer
            oc, string name)
        {
            var q = oc.Query();
            q.Constrain(typeof (RefreshTestCase));
            q.Descend("name").Constrain(name);
            var objectSet = q.Execute();
            return (RefreshTestCase) objectSet.Next();
        }
    }
}

#endif // !SILVERLIGHT