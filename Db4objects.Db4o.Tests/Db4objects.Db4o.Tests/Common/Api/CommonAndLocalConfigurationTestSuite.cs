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
using System;
using System.IO;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Config.Encoding;
using Db4objects.Db4o.CS;
using Db4objects.Db4o.CS.Config;
using Db4objects.Db4o.Diagnostic;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Config;
using Db4objects.Db4o.Internal.Ids;
using Db4objects.Db4o.IO;
using Db4oUnit;
using Db4oUnit.Fixtures;
using Sharpen;

namespace Db4objects.Db4o.Tests.Common.Api
{
    public class CommonAndLocalConfigurationTestSuite : FixtureBasedTestSuite
    {
        public override IFixtureProvider[] FixtureProviders()
        {
            return new[]
            {
                Subjects(new object[]
                {
                    Db4oEmbedded.NewConfiguration
                        (),
                    Db4oClientServer.NewClientConfiguration(), Db4oClientServer.NewServerConfiguration
                        ()
                })
            };
        }

        private IFixtureProvider Subjects(object[] subjects)
        {
            return new SubjectFixtureProvider(subjects);
        }

        public override Type[] TestUnits()
        {
            return new[]
            {
                typeof (BaseConfigurationProviderTestUnit
                    ),
                typeof (LocalConfigurationProviderTestUnit
                    )
            };
        }

        public static object Subject()
        {
            return SubjectFixtureProvider.Value();
        }

        public class BaseConfigurationProviderTestUnit : ITestCase
        {
            public virtual void Test()
            {
                var config = ((ICommonConfigurationProvider) Subject());
                var legacy = Db4oLegacyConfigurationBridge.AsLegacy(config);
                var common = config.Common;
                common.ActivationDepth = 42;
                Assert.AreEqual(42, legacy.ActivationDepth());
                Assert.AreEqual(42, common.ActivationDepth);
                // TODO: assert
                common.Add(new _IConfigurationItem_41());
                var alias = new TypeAlias("foo", "bar");
                common.AddAlias(alias);
                Assert.AreEqual("bar", legacy.ResolveAliasStoredName("foo"));
                Assert.AreEqual("foo", legacy.ResolveAliasRuntimeName("bar"));
                common.RemoveAlias(alias);
                Assert.AreEqual("foo", legacy.ResolveAliasStoredName("foo"));
                common.AllowVersionUpdates = false;
                Assert.IsFalse(legacy.AllowVersionUpdates());
                common.AutomaticShutDown = false;
                Assert.IsFalse(legacy.AutomaticShutDown());
                common.BTreeNodeSize = 42;
                Assert.AreEqual(42, legacy.BTreeNodeSize());
                common.Callbacks = false;
                Assert.AreEqual(CallBackMode.None, legacy.CallbackMode());
                common.CallConstructors = false;
                Assert.IsTrue(legacy.CallConstructors().DefiniteNo());
                common.DetectSchemaChanges = false;
                Assert.IsFalse(legacy.DetectSchemaChanges());
                var collector = new DiagnosticCollector();
                common.Diagnostic.AddListener(collector);
                IDiagnostic diagnostic = DummyDiagnostic();
                legacy.DiagnosticProcessor().OnDiagnostic(diagnostic);
                collector.Verify(new object[] {diagnostic});
                common.ExceptionsOnNotStorable = true;
                Assert.IsTrue(legacy.ExceptionsOnNotStorable());
                common.InternStrings = true;
                Assert.IsTrue(legacy.InternStrings());
                // TODO: assert
                common.MarkTransient("Foo");
                common.MessageLevel = 3;
                Assert.AreEqual(3, legacy.MessageLevel());
                var objectClass = common.ObjectClass(typeof (Item
                    ));
                objectClass.CascadeOnDelete(true);
                Assert.IsTrue(((Config4Class) legacy.ObjectClass(typeof (Item
                    ))).CascadeOnDelete().DefiniteYes());
                Assert.IsTrue(((Config4Class) common.ObjectClass(typeof (Item
                    ))).CascadeOnDelete().DefiniteYes());
                common.OptimizeNativeQueries = false;
                Assert.IsFalse(legacy.OptimizeNativeQueries());
                Assert.IsFalse(common.OptimizeNativeQueries);
                common.Queries.EvaluationMode(QueryEvaluationMode.Lazy);
                Assert.AreEqual(QueryEvaluationMode.Lazy, legacy.EvaluationMode());
                // TODO: test reflectWith()
                // TODO: this probably won't sharpen :/
                var outStream = Runtime.Out;
                common.OutStream = outStream;
                Assert.AreEqual(outStream, legacy.OutStream());
                IStringEncoding stringEncoding = new _IStringEncoding_113();
                common.StringEncoding = stringEncoding;
                Assert.AreEqual(stringEncoding, legacy.StringEncoding());
                common.TestConstructors = false;
                Assert.IsFalse(legacy.TestConstructors());
                common.TestConstructors = true;
                Assert.IsTrue(legacy.TestConstructors());
                common.UpdateDepth = 1024;
                Assert.AreEqual(1024, legacy.UpdateDepth());
                common.WeakReferences = false;
                Assert.IsFalse(legacy.WeakReferences());
                common.WeakReferenceCollectionInterval = 1024;
                Assert.AreEqual(1024, legacy.WeakReferenceCollectionInterval());
            }

            // TODO: test registerTypeHandler()
            private DiagnosticBase DummyDiagnostic()
            {
                return new _DiagnosticBase_143();
            }

            public sealed class Item
            {
            }

            private sealed class _IConfigurationItem_41 : IConfigurationItem
            {
                public void Apply(IInternalObjectContainer container)
                {
                }

                public void Prepare(IConfiguration configuration)
                {
                }
            }

            private sealed class _IStringEncoding_113 : IStringEncoding
            {
                public string Decode(byte[] bytes, int start, int length)
                {
                    return null;
                }

                public byte[] Encode(string str)
                {
                    return null;
                }
            }

            private sealed class _DiagnosticBase_143 : DiagnosticBase
            {
                public override string Problem()
                {
                    return null;
                }

                public override object Reason()
                {
                    return null;
                }

                public override string Solution()
                {
                    return null;
                }
            }
        }

        public class LocalConfigurationProviderTestUnit : ITestCase
        {
            /// <exception cref="System.Exception"></exception>
            public virtual void Test()
            {
                if (Subject() is IClientConfiguration)
                {
                    return;
                }
                var config = ((IFileConfigurationProvider) Subject());
                var fileConfig = config.File;
                var legacyConfig = Db4oLegacyConfigurationBridge.AsLegacy(config);
                fileConfig.BlockSize = 42;
                Assert.AreEqual(42, legacyConfig.BlockSize());
                fileConfig.DatabaseGrowthSize = 42;
                Assert.AreEqual(42, legacyConfig.DatabaseGrowthSize());
                fileConfig.DisableCommitRecovery();
                Assert.IsTrue(legacyConfig.CommitRecoveryDisabled());
                fileConfig.Freespace.DiscardSmallerThan(8);
                Assert.AreEqual(8, legacyConfig.DiscardFreeSpace());
                fileConfig.GenerateUUIDs = ConfigScope.Globally;
                Assert.AreEqual(ConfigScope.Globally, legacyConfig.GenerateUUIDs());
                fileConfig.GenerateCommitTimestamps = true;
                Assert.IsTrue(legacyConfig.GenerateCommitTimestamps().DefiniteYes());
                IStorage storageFactory = new FileStorage();
                fileConfig.Storage = storageFactory;
                Assert.AreSame(storageFactory, legacyConfig.Storage);
                Assert.AreSame(storageFactory, fileConfig.Storage);
                fileConfig.LockDatabaseFile = true;
                Assert.IsTrue(legacyConfig.LockFile());
                fileConfig.ReserveStorageSpace = 1024;
                Assert.AreEqual(1024, legacyConfig.ReservedStorageSpace());
                fileConfig.BlobPath = Path.GetTempPath();
                Assert.AreEqual(Path.GetTempPath(), legacyConfig.BlobPath());
                fileConfig.ReadOnly = true;
                Assert.IsTrue(legacyConfig.IsReadOnly());
                var cacheProvider = ((ICacheConfigurationProvider) Subject
                    ());
                var cache = cacheProvider.Cache;
                var idSystemConfigurationProvider = ((IIdSystemConfigurationProvider
                    ) Subject());
                var idSystemConfiguration = idSystemConfigurationProvider.IdSystem;
                Assert.AreEqual(StandardIdSystemFactory.Default, legacyConfig.IdSystemType());
                idSystemConfiguration.UseStackedBTreeSystem();
                Assert.AreEqual(StandardIdSystemFactory.StackedBtree, legacyConfig.IdSystemType()
                    );
                idSystemConfiguration.UsePointerBasedSystem();
                Assert.AreEqual(StandardIdSystemFactory.PointerBased, legacyConfig.IdSystemType()
                    );
            }

            public virtual void TestUnspecifiedUpdateDepthIsIllegal()
            {
                var common = ((ICommonConfigurationProvider) Subject());
                Assert.Expect(typeof (ArgumentException), new _ICodeBlock_218(common));
            }

            private sealed class _ICodeBlock_218 : ICodeBlock
            {
                private readonly ICommonConfigurationProvider common;

                public _ICodeBlock_218(ICommonConfigurationProvider common)
                {
                    this.common = common;
                }

                /// <exception cref="System.Exception"></exception>
                public void Run()
                {
                    common.Common.UpdateDepth = Const4.Unspecified;
                }
            }
        }
    }
}

#endif // !SILVERLIGHT