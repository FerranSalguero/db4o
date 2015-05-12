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
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Config
{
    public class TransientConfigurationTestSuite : FixtureTestSuiteDescription
    {
        private static readonly FixtureVariable ClassConfig = FixtureVariable.NewInstance
            ("Class");

        public TransientConfigurationTestSuite()
        {
            {
                TestUnits(new[]
                {
                    typeof (TransientConfigurationTestUnit
                        )
                });
                FixtureProviders(new IFixtureProvider[]
                {
                    new SimpleFixtureProvider(ClassConfig,
                        new object[] {true, false})
                });
            }
        }

        public class TransientConfigurationTestUnit : AbstractDb4oTestCase
        {
            private const int TransientValue = 42;
            private const int PersistentValue = unchecked(0xdb40);

            /// <exception cref="System.Exception"></exception>
            protected override void Configure(IConfiguration config)
            {
                config.ObjectClass(typeof (Item)).StoreTransientFields
                    ((((bool) ClassConfig.Value)));
            }

            /// <exception cref="System.Exception"></exception>
            protected override void Store()
            {
                Store(new Item(TransientValue, PersistentValue));
            }

            public virtual void TestRetrieval()
            {
                var instance = ((Item
                    ) RetrieveOnlyInstance(typeof (Item)));
                Assert.AreEqual(PersistentValue, instance._persistentField);
                Assert.AreEqual(ExpectedTransientValue(), instance._transientField);
            }

            private int ExpectedTransientValue()
            {
                return (((bool) ClassConfig.Value)) ? TransientValue : 0;
            }
        }

        public class Item
        {
            public int _persistentField;

            [NonSerialized] public int _transientField;

            public Item(int transientValue, int persistentValue)
            {
                _transientField = transientValue;
                _persistentField = persistentValue;
            }
        }
    }
}