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
using Db4objects.Db4o.Query;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.CS
{
    public class NoTestConstructorsQEStringCmpTestCase : AbstractDb4oTestCase, IOptOutAllButNetworkingCS
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.CallConstructors(true);
            config.TestConstructors(false);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(new Item("abc"));
        }

        public virtual void TestStartsWith()
        {
            AssertSingleItem("a", new _IConstraintModifier_35());
        }

        public virtual void TestEndsWith()
        {
            AssertSingleItem("c", new _IConstraintModifier_43());
        }

        public virtual void TestContains()
        {
            AssertSingleItem("b", new _IConstraintModifier_51());
        }

        private void AssertSingleItem(string pattern, IConstraintModifier
            modifier)
        {
            var query = BaseQuery(pattern, modifier);
            var result = query.Execute();
            Assert.AreEqual(1, result.Count);
        }

        private IQuery BaseQuery(string pattern, IConstraintModifier
            modifier)
        {
            var query = NewQuery();
            query.Constrain(typeof (Item));
            var constraint = query.Descend("_name").Constrain(pattern);
            modifier.Modify(constraint);
            return query;
        }

        public static void Main(string[] args)
        {
            new NoTestConstructorsQEStringCmpTestCase().RunNetworking();
        }

        public class Item
        {
            public string _name;

            public Item(string name)
            {
                _name = name;
            }
        }

        private interface IConstraintModifier
        {
            void Modify(IConstraint constraint);
        }

        private sealed class _IConstraintModifier_35 : IConstraintModifier
        {
            public void Modify(IConstraint constraint)
            {
                constraint.StartsWith(false);
            }
        }

        private sealed class _IConstraintModifier_43 : IConstraintModifier
        {
            public void Modify(IConstraint constraint)
            {
                constraint.EndsWith(false);
            }
        }

        private sealed class _IConstraintModifier_51 : IConstraintModifier
        {
            public void Modify(IConstraint constraint)
            {
                constraint.Contains();
            }
        }
    }
}

#endif // !SILVERLIGHT