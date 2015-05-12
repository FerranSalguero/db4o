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

using Db4objects.Db4o.Config;
using Db4objects.Db4o.Foundation;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Querying
{
    public class CascadeOnUpdateTestCase : AbstractDb4oTestCase
    {
        public object child;

        protected override void Configure(IConfiguration conf)
        {
            conf.ObjectClass(typeof (Holder)).CascadeOnUpdate(true);
        }

        protected override void Store()
        {
            var cou = new Holder(new Atom
                (new Atom("storedChild"), "stored"));
            Db().Store(cou);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void Test()
        {
            Foreach(GetType(), new _IVisitor4_55(this));
            Reopen();
            Foreach(GetType(), new _IVisitor4_66());
        }

        public class Holder
        {
            public object child;

            public Holder(object child)
            {
                this.child = child;
            }
        }

        public class Atom
        {
            public Atom child;
            public string name;

            public Atom()
            {
            }

            public Atom(Atom child)
            {
                this.child = child;
            }

            public Atom(string name)
            {
                this.name = name;
            }

            public Atom(Atom child, string name) : this(child)
            {
                this.name = name;
            }
        }

        private sealed class _IVisitor4_55 : IVisitor4
        {
            private readonly CascadeOnUpdateTestCase _enclosing;

            public _IVisitor4_55(CascadeOnUpdateTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Visit(object obj)
            {
                var cou = (Holder) obj;
                ((Atom) cou.child).name = "updated";
                ((Atom) cou.child).child.name = "updated";
                _enclosing.Db().Store(cou);
            }
        }

        private sealed class _IVisitor4_66 : IVisitor4
        {
            public void Visit(object obj)
            {
                var cou = (Holder) obj;
                var atom = (Atom) cou.child;
                Assert.AreEqual("updated", atom.name);
                Assert.AreNotEqual("updated", atom.child.name);
            }
        }
    }
}