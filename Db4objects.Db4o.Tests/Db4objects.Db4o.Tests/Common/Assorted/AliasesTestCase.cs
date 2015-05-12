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
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Assorted
{
    public class AliasesTestCase : AbstractDb4oTestCase, IOptOutDefragSolo
    {
        private IAlias alias;
        private int id;

        public static void Main(string[] args)
        {
            new AliasesTestCase().RunSolo();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            AddACAlias();
            var bar = new CBar();
            bar.foo = "foo";
            bar.bar = "bar";
            Store(bar);
            id = (int) Db().GetID(bar);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestAccessByChildClass()
        {
            AddABAlias();
            var bar = (BBar) RetrieveOnlyInstance
                (typeof (BBar));
            AssertInstanceOK(bar);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestAccessByParentClass()
        {
            AddABAlias();
            var bar = (BBar) ((BFoo) RetrieveOnlyInstance
                (typeof (BFoo)));
            AssertInstanceOK(bar);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestAccessById()
        {
            AddABAlias();
            var bar = (BBar) Db().GetByID(id);
            Db().Activate(bar, 2);
            AssertInstanceOK(bar);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestAccessWithoutAlias()
        {
            RemoveAlias();
            var bar = (ABar) RetrieveOnlyInstance
                (typeof (ABar));
            AssertInstanceOK(bar);
        }

        private void AssertInstanceOK(BBar bar)
        {
            Assert.AreEqual("foo", bar.foo);
            Assert.AreEqual("bar", bar.bar);
        }

        private void AssertInstanceOK(ABar bar)
        {
            Assert.AreEqual("foo", bar.foo);
            Assert.AreEqual("bar", bar.bar);
        }

        /// <exception cref="System.Exception"></exception>
        private void AddABAlias()
        {
            AddAlias("A", "B");
        }

        /// <exception cref="System.Exception"></exception>
        private void AddACAlias()
        {
            AddAlias("A", "C");
        }

        /// <exception cref="System.Exception"></exception>
        private void AddAlias(string storedLetter, string runtimeLetter)
        {
            RemoveAlias();
            alias = CreateAlias(storedLetter, runtimeLetter);
            Fixture().ConfigureAtRuntime(new _IRuntimeConfigureAction_104(this));
            Reopen();
        }

        /// <exception cref="System.Exception"></exception>
        private void RemoveAlias()
        {
            if (alias != null)
            {
                Fixture().ConfigureAtRuntime(new _IRuntimeConfigureAction_114(this));
                alias = null;
            }
            Reopen();
        }

        private WildcardAlias CreateAlias(string storedLetter, string runtimeLetter)
        {
            var className = Reflector().ForObject(new ABar()).GetName();
            var storedPattern = className.Replace("ABar", storedLetter + "*");
            var runtimePattern = className.Replace("ABar", runtimeLetter + "*");
            return new WildcardAlias(storedPattern, runtimePattern);
        }

        public class AFoo
        {
            public string foo;
        }

        public class ABar : AFoo
        {
            public string bar;
        }

        public class BFoo
        {
            public string foo;
        }

        public class BBar : BFoo
        {
            public string bar;
        }

        public class CFoo
        {
            public string foo;
        }

        public class CBar : CFoo
        {
            public string bar;
        }

        private sealed class _IRuntimeConfigureAction_104 : IRuntimeConfigureAction
        {
            private readonly AliasesTestCase _enclosing;

            public _IRuntimeConfigureAction_104(AliasesTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Apply(IConfiguration config)
            {
                config.AddAlias(_enclosing.alias);
            }
        }

        private sealed class _IRuntimeConfigureAction_114 : IRuntimeConfigureAction
        {
            private readonly AliasesTestCase _enclosing;

            public _IRuntimeConfigureAction_114(AliasesTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Apply(IConfiguration config)
            {
                config.RemoveAlias(_enclosing.alias);
            }
        }
    }
}