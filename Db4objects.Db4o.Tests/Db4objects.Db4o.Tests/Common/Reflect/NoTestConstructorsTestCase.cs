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

namespace Db4objects.Db4o.Tests.Common.Reflect
{
    public class NoTestConstructorsTestCase : AbstractDb4oTestCase
    {
        protected override void Db4oSetupBeforeStore()
        {
            Item.constructorCalls = 0;
        }

        protected override void Configure(IConfiguration config)
        {
            config.CallConstructors(true);
            config.TestConstructors(false);
        }

        protected override void Store()
        {
            Store(new Item());
            Assert.AreEqual(1, Item.constructorCalls);
        }

        public virtual void Test()
        {
            RetrieveOnlyInstance(typeof (Item));
            Assert.AreEqual(2, Item.constructorCalls);
        }

        public static void Main(string[] args)
        {
            new NoTestConstructorsTestCase().RunAll();
        }

        public class Item
        {
            public static int constructorCalls;

            public Item()
            {
                constructorCalls++;
            }
        }
    }
}