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

using System;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Query;
using Db4oUnit;
using Db4oUnit.Extensions;
using Sharpen;

namespace Db4objects.Db4o.Tests.Common.Regression
{
    /// <exclude></exclude>
    public class COR57TestCase : AbstractDb4oTestCase
    {
        public static void Main(string[] args)
        {
            new COR57TestCase().RunSolo();
        }

        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(typeof (Base)).ObjectField("name").Indexed(true);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            for (var i = 0; i < 5; i++)
            {
                var name = i.ToString();
                Db().Store(new Base(name));
                Db().Store(new BaseExt(name));
                Db().Store(new BaseExtExt(name));
            }
        }

        public virtual void TestQBE()
        {
            AssertQBE(1, new BaseExtExt("1"));
            AssertQBE(2, new BaseExt("1"));
            AssertQBE(3, new Base("1"));
        }

        public virtual void TestSODA()
        {
            AssertSODA(1, new BaseExtExt("1"));
            AssertSODA(2, new BaseExt("1"));
            AssertSODA(3, new Base("1"));
        }

        private void AssertSODA(int expectedCount, Base template)
        {
            AssertQueryResult(expectedCount, template, CreateSODA(template).Execute());
        }

        private IQuery CreateSODA(Base template)
        {
            var q = NewQuery(template.GetType());
            q.Descend("name").Constrain(template.name);
            return q;
        }

        private void AssertQBE(int expectedCount, Base template)
        {
            AssertQueryResult(expectedCount, template, Db().QueryByExample(template));
        }

        private void AssertQueryResult(int expectedCount, Base expectedTemplate
            , IObjectSet result)
        {
            Assert.AreEqual(expectedCount, result.Count, SimpleName(expectedTemplate.GetType(
                )));
            while (result.HasNext())
            {
                var actual = (Base) result.Next();
                Assert.AreEqual(expectedTemplate.name, actual.name);
                Assert.IsInstanceOf(expectedTemplate.GetType(), actual);
            }
        }

        private string SimpleName(Type c)
        {
            var name = c.FullName;
            return Runtime.Substring(name, name.LastIndexOf('$') + 1);
        }

        public class Base
        {
            public string name;

            public Base()
            {
            }

            public Base(string name_)
            {
                name = name_;
            }

            public override string ToString()
            {
                return GetType() + ":" + name;
            }
        }

        public class BaseExt : Base
        {
            public BaseExt()
            {
            }

            public BaseExt(string name_) : base(name_)
            {
            }
        }

        public class BaseExtExt : BaseExt
        {
            public BaseExtExt()
            {
            }

            public BaseExtExt(string name_) : base(name_)
            {
            }
        }
    }
}