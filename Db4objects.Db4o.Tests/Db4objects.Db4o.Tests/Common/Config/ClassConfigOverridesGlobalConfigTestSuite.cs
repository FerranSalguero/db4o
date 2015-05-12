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
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Config
{
    public class ClassConfigOverridesGlobalConfigTestSuite : FixtureTestSuiteDescription
    {
        private static readonly FixtureVariable GlobalConfig = FixtureVariable.NewInstance
            ("global");

        private static readonly FixtureVariable ClassConfig = FixtureVariable.NewInstance
            ("class");

        public ClassConfigOverridesGlobalConfigTestSuite()
        {
            {
                TestUnits(new[]
                {
                    typeof (ClassConfigOverridesGlobalConfigTestUnit
                        )
                });
                FixtureProviders(new IFixtureProvider[]
                {
                    new SimpleFixtureProvider(GlobalConfig,
                        new object[]
                        {
                            ConfigScope.Globally, ConfigScope.Individually, ConfigScope.Disabled
                        }),
                    new SimpleFixtureProvider(ClassConfig, new object[]
                    {
                        TernaryBool.Yes, TernaryBool
                            .No,
                        TernaryBool.Unspecified
                    })
                });
            }
        }

        public class ClassConfigOverridesGlobalConfigTestUnit : AbstractDb4oTestCase
        {
            protected override void Configure(IConfiguration config)
            {
                config.GenerateUUIDs(((ConfigScope) GlobalConfig.Value));
                if (!((TernaryBool) ClassConfig.Value).IsUnspecified())
                {
                    config.ObjectClass(typeof (Item
                        )).GenerateUUIDs(((TernaryBool) ClassConfig.Value).BooleanValue(true));
                }
            }

            /// <exception cref="System.Exception"></exception>
            protected override void Store()
            {
                Store(new Item
                    ());
            }

            public virtual void TestNoUUIDIsGenerated()
            {
                var
                    item = ((Item
                        ) RetrieveOnlyInstance(typeof (Item
                            )));
                var objectInfo = Db().Ext().GetObjectInfo(item);
                if (!((TernaryBool) ClassConfig.Value).IsUnspecified())
                {
                    AssertGeneration(objectInfo, ((TernaryBool) ClassConfig.Value).BooleanValue(true)
                                                 && ((ConfigScope) GlobalConfig.Value) != ConfigScope.Disabled);
                }
                else
                {
                    AssertGeneration(objectInfo, ((ConfigScope) GlobalConfig.Value) == ConfigScope.Globally
                        );
                }
            }

            private void AssertGeneration(IObjectInfo objectInfo, bool expectGeneration)
            {
                if (expectGeneration)
                {
                    Assert.IsNotNull(objectInfo.GetUUID());
                }
                else
                {
                    Assert.IsNull(objectInfo.GetUUID());
                    Assert.AreEqual(0L, objectInfo.GetCommitTimestamp());
                }
            }

            public class Item
            {
            }
        }
    }
}