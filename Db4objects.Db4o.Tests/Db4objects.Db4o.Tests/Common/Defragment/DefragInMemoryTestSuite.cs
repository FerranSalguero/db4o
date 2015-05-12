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
using Db4objects.Db4o.Defragment;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.IO;
using Db4objects.Db4o.Query;
using Db4objects.Db4o.Tests.Common.Api;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Defragment
{
    public class DefragInMemoryTestSuite : FixtureBasedTestSuite
    {
        private static readonly FixtureVariable StorageSpecFixture = new FixtureVariable(
            );

        public override IFixtureProvider[] FixtureProviders()
        {
            return new IFixtureProvider[]
            {
                new SimpleFixtureProvider(StorageSpecFixture, new
                    object[]
                {
                    new StorageSpec("memory", null), new StorageSpec
                        ("file", Db4oUnitPlatform.NewPersistentStorage())
                })
            };
        }

        public override Type[] TestUnits()
        {
            return new[] {typeof (DefragInMemoryTestUnit)};
        }

        private class StorageSpec : ILabeled
        {
            private readonly string _label;
            private readonly IStorage _storage;

            public StorageSpec(string label, IStorage storage)
            {
                _label = label;
                _storage = storage;
            }

            public virtual string Label()
            {
                return _label;
            }

            public virtual IStorage Storage(IStorage storage)
            {
                return _storage == null ? storage : _storage;
            }
        }

        public class DefragInMemoryTestUnit : TestWithTempFile
        {
            private const int NumItems = 100;
            protected static readonly string Uri = "database";
            private MemoryStorage _memoryStorage;

            /// <exception cref="System.Exception"></exception>
            public virtual void TestInMemoryDefragment()
            {
                Store();
                Defrag();
                Assert.IsSmaller(BackupLength(), _memoryStorage.Bin(Uri).Length());
                Retrieve();
            }

            private long BackupLength()
            {
                var backupBin = BackupStorage().Open(new BinConfiguration(TempFile(), true, 0, true
                    ));
                var backupLength = backupBin.Length();
                backupBin.Sync();
                backupBin.Close();
                return backupLength;
            }

            private DefragmentConfig DefragmentConfig(MemoryStorage storage)
            {
                var defragConfig = new DefragmentConfig(Uri, TempFile(), new InMemoryIdMapping
                    ());
                defragConfig.Db4oConfig(Config(storage));
                defragConfig.BackupStorage(BackupStorage());
                return defragConfig;
            }

            private IStorage BackupStorage()
            {
                return ((StorageSpec) StorageSpecFixture.Value).Storage(_memoryStorage
                    );
            }

            private IEmbeddedConfiguration Config(IStorage storage)
            {
                var config = Db4oEmbedded.NewConfiguration();
                config.Common.ReflectWith(Platform4.ReflectorForType(typeof (Item
                    )));
                config.File.Storage = storage;
                return config;
            }

            /// <exception cref="System.IO.IOException"></exception>
            private void Defrag()
            {
                var defragConfig = DefragmentConfig(_memoryStorage);
                Db4o.Defragment.Defragment.Defrag(defragConfig);
            }

            private void Store()
            {
                IObjectContainer db = Db4oEmbedded.OpenFile(Config(_memoryStorage), Uri);
                for (var itemId = 0; itemId < NumItems; itemId++)
                {
                    db.Store(new Item(itemId));
                }
                db.Commit();
                var result = db.Query(new EvenIdItemsPredicate
                    ());
                while (result.HasNext())
                {
                    db.Delete(((Item) result.Next()));
                }
                db.Close();
            }

            private void Retrieve()
            {
                IObjectContainer db = Db4oEmbedded.OpenFile(Config(_memoryStorage), Uri);
                var result = db.Query(typeof (Item
                    ));
                Assert.AreEqual(NumItems/2, result.Count);
                while (result.HasNext())
                {
                    Assert.IsTrue((((Item) result.Next(
                        ))._id%2) == 1);
                }
                db.Close();
            }

            /// <exception cref="System.Exception"></exception>
            public override void SetUp()
            {
                _memoryStorage = new MemoryStorage();
            }

            /// <exception cref="System.Exception"></exception>
            public override void TearDown()
            {
                BackupStorage().Delete(TempFile());
                base.TearDown();
            }

            public class Item
            {
                public int _id;

                public Item()
                {
                }

                public Item(int id)
                {
                    _id = id;
                }
            }

            [Serializable]
            public class EvenIdItemsPredicate : Predicate
            {
                public virtual bool Match(Item item
                    )
                {
                    return (item._id%2) == 0;
                }
            }
        }
    }
}