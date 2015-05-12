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
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Foundation.IO;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Config;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;
using Db4oUnit.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Acid
{
    /// <exclude></exclude>
    public class CrashSimulatingTestSuite : FixtureBasedTestSuite, IOptOutVerySlow
    {
        internal const bool Verbose = false;
        private static readonly FixtureVariable UseCache = new FixtureVariable();
        private static readonly FixtureVariable UseLogfile = new FixtureVariable();
        private static readonly FixtureVariable WriteTrash = new FixtureVariable();
        private static readonly FixtureVariable IdSystem = new FixtureVariable();
        private static readonly FixtureVariable FreespaceManager = new FixtureVariable();

        private IFixtureProvider[] SingleConfig()
        {
            return new IFixtureProvider[]
            {
                new SimpleFixtureProvider(UseCache, new[] {new LabeledBoolean("no cache", false)}),
                new SimpleFixtureProvider
                    (UseLogfile, new[]
                    {
                        new LabeledBoolean
                            ("no logfile", false)
                    }),
                new SimpleFixtureProvider(WriteTrash, new[] {new LabeledBoolean("write trash", true)}),
                new SimpleFixtureProvider
                    (FreespaceManager, new LabeledConfig[]
                    {
                        new _LabeledConfig_44
                            ("BTreeFreespaceManager")
                    }),
                new SimpleFixtureProvider(IdSystem, new LabeledConfig
                    [] {new _LabeledConfig_52("BTreeIdSystem")})
            };
        }

        // Db4oLegacyConfigurationBridge.asIdSystemConfiguration(config).useInMemorySystem();
        // Db4oLegacyConfigurationBridge.asIdSystemConfiguration(config).usePointerBasedSystem();
        public override IFixtureProvider[] FixtureProviders()
        {
            //		if(true){
            //			return singleConfig();
            //		}
            return new IFixtureProvider[]
            {
                new SimpleFixtureProvider(UseCache, new[]
                {
                    new LabeledBoolean("cached", true), new LabeledBoolean
                        ("no cache", false)
                }),
                new SimpleFixtureProvider(UseLogfile, new[]
                {
                    new LabeledBoolean("logfile", true), new LabeledBoolean
                        ("no logfile", false)
                }),
                new SimpleFixtureProvider(WriteTrash, new[]
                {
                    new LabeledBoolean("write trash", true), new LabeledBoolean
                        ("don't write trash", false)
                }),
                new SimpleFixtureProvider(FreespaceManager, new
                    LabeledConfig[]
                {
                    new _LabeledConfig_76("InMemoryFreespaceManager"
                        ),
                    new _LabeledConfig_80("BTreeFreespaceManager")
                }),
                new SimpleFixtureProvider(
                    IdSystem, new LabeledConfig[]
                    {
                        new _LabeledConfig_88("PointerBasedIdSystem"
                            ),
                        new _LabeledConfig_92("BTreeIdSystem"), new _LabeledConfig_96("InMemoryIdSystem"
                            )
                    })
            };
        }

        public override Type[] TestUnits()
        {
            return new[] {typeof (CrashSimulatingTestCase)};
        }

        private sealed class _LabeledConfig_44 : LabeledConfig
        {
            public _LabeledConfig_44(string baseArg1) : base(baseArg1)
            {
            }

            public override void Configure(Config4Impl config)
            {
                // config.freespace().useRamSystem();
                config.Freespace().UseBTreeSystem();
            }
        }

        private sealed class _LabeledConfig_52 : LabeledConfig
        {
            public _LabeledConfig_52(string baseArg1) : base(baseArg1)
            {
            }

            public override void Configure(Config4Impl config)
            {
                Db4oLegacyConfigurationBridge.AsIdSystemConfiguration(config).UseStackedBTreeSystem
                    ();
            }
        }

        private sealed class _LabeledConfig_76 : LabeledConfig
        {
            public _LabeledConfig_76(string baseArg1) : base(baseArg1)
            {
            }

            public override void Configure(Config4Impl config)
            {
                config.Freespace().UseRamSystem();
            }
        }

        private sealed class _LabeledConfig_80 : LabeledConfig
        {
            public _LabeledConfig_80(string baseArg1) : base(baseArg1)
            {
            }

            public override void Configure(Config4Impl config)
            {
                config.Freespace().UseBTreeSystem();
            }
        }

        private sealed class _LabeledConfig_88 : LabeledConfig
        {
            public _LabeledConfig_88(string baseArg1) : base(baseArg1)
            {
            }

            public override void Configure(Config4Impl config)
            {
                Db4oLegacyConfigurationBridge.AsIdSystemConfiguration(config).UsePointerBasedSystem
                    ();
            }
        }

        private sealed class _LabeledConfig_92 : LabeledConfig
        {
            public _LabeledConfig_92(string baseArg1) : base(baseArg1)
            {
            }

            public override void Configure(Config4Impl config)
            {
                Db4oLegacyConfigurationBridge.AsIdSystemConfiguration(config).UseStackedBTreeSystem
                    ();
            }
        }

        private sealed class _LabeledConfig_96 : LabeledConfig
        {
            public _LabeledConfig_96(string baseArg1) : base(baseArg1)
            {
            }

            public override void Configure(Config4Impl config)
            {
                Db4oLegacyConfigurationBridge.AsIdSystemConfiguration(config).UseInMemorySystem();
            }
        }

        public class CrashSimulatingTestCase : ITestCase, IOptOutMultiSession, IOptOutVerySlow
        {
            // The cache may touch more bytes than the ones we modified.
            // We should be safe even if we don't get this test to pass.
            // The log file is not a public API yet anyway.
            // It's only needed for the PointerBasedIdSystem
            // With the new BTreeIdSystem it's not likely to become important
            // so we can safely ignore the failing write trash case.
            private IConfiguration BaseConfig(bool useLogFile)
            {
                var config = (Config4Impl) Db4oFactory.NewConfiguration();
                config.ObjectClass(typeof (CrashData)).ObjectField("_name"
                    ).Indexed(true);
                config.ReflectWith(Platform4.ReflectorForType(typeof (CrashSimulatingTestCase
                    )));
                config.BTreeNodeSize(4);
                config.LockDatabaseFile(false);
                config.FileBasedTransactionLog(useLogFile);
                ((LabeledConfig) IdSystem.Value).Configure(config);
                ((LabeledConfig) FreespaceManager.Value).Configure(config
                    );
                return config;
            }

            private void CheckFiles(bool useLogFile, string fileName, string infix, int count
                )
            {
                for (var i = 1; i <= count; i++)
                {
                    var versionedFileName = fileName + infix + i;
                    var oc = Db4oFactory.OpenFile(BaseConfig(useLogFile), versionedFileName
                        );
                    try
                    {
                        if (!StateBeforeCommit(oc))
                        {
                            if (!StateAfterFirstCommit(oc))
                            {
                                Assert.IsTrue(StateAfterSecondCommit(oc));
                            }
                        }
                    }
                    finally
                    {
                        oc.Close();
                    }
                }
            }

            private bool StateBeforeCommit(IObjectContainer oc)
            {
                return Expect(oc, new[] {"one", "two", "three"});
            }

            private bool StateAfterFirstCommit(IObjectContainer oc)
            {
                return Expect(oc, new[]
                {
                    "one", "two", "four", "five", "six", "seven", "eight"
                    , "nine", "10", "11", "12", "13", "14"
                });
            }

            private bool StateAfterSecondCommit(IObjectContainer oc)
            {
                return Expect(oc, new[] {"10", "13"});
            }

            private bool Expect(IObjectContainer container, string[] names)
            {
                var expected = new Collection4(names);
                var actual = container.Query(typeof (CrashData));
                while (actual.HasNext())
                {
                    var current = (CrashData)
                        actual.Next();
                    if (!expected.Remove(current._name))
                    {
                        return false;
                    }
                }
                return expected.IsEmpty();
            }

            /// <exception cref="System.IO.IOException"></exception>
            private void CreateFile(IConfiguration config, string fileName)
            {
                var oc = Db4oFactory.OpenFile(config, fileName);
                try
                {
                    Populate(oc);
                }
                finally
                {
                    oc.Close();
                }
                File4.Copy(fileName, fileName + "0");
            }

            private void Populate(IObjectContainer container)
            {
                for (var i = 0; i < 10; i++)
                {
                    container.Store(new Item("delme"));
                }
                var one = new CrashData(null
                    , "one");
                var two = new CrashData(one
                    , "two");
                var three = new CrashData
                    (one, "three");
                container.Store(one);
                container.Store(two);
                container.Store(three);
                container.Commit();
                var objectSet = container.Query(typeof (Item));
                while (objectSet.HasNext())
                {
                    container.Delete(objectSet.Next());
                }
            }
        }

        public class CrashData
        {
            public string _name;
            public CrashData _next;

            public CrashData(CrashData next_, string name)
            {
                _next = next_;
                _name = name;
            }

            public override string ToString()
            {
                return _name + " -> " + _next;
            }
        }

        public class Item
        {
            public string name;

            public Item()
            {
            }

            public Item(string name_)
            {
                name = name_;
            }

            public virtual string GetName()
            {
                return name;
            }

            public virtual void SetName(string name_)
            {
                name = name_;
            }
        }

        public class LabeledBoolean : ILabeled
        {
            private readonly string _label;
            private readonly bool _value;

            public LabeledBoolean(string label, bool value)
            {
                _label = label;
                _value = value;
            }

            public virtual string Label()
            {
                return _label;
            }

            public virtual bool BooleanValue()
            {
                return _value;
            }
        }

        public abstract class LabeledConfig : ILabeled
        {
            private readonly string _label;

            public LabeledConfig(string label)
            {
                _label = label;
            }

            public virtual string Label()
            {
                return _label;
            }

            public abstract void Configure(Config4Impl config);
        }
    }
}