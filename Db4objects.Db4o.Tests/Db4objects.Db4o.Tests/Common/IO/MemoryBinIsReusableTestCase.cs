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
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.IO;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.IO
{
    public class MemoryBinIsReusableTestCase : ITestCase
    {
        private static readonly string ItemName = "foo";
        private static readonly string BinUri = "mybin";

        public virtual void Test()
        {
            var origStorage = new MemoryStorage();
            var origConfig = Config(origStorage);
            IObjectContainer origDb = Db4oEmbedded.OpenFile(origConfig, BinUri);
            origDb.Store(new Item(ItemName));
            origDb.Close();
            var origBin = origStorage.Bin(BinUri);
            var data = origBin.Data();
            Assert.AreEqual(data.Length, origBin.Length());
            var newBin = new MemoryBin(data, new DoublingGrowthStrategy());
            var newStorage = new MemoryStorage();
            newStorage.Bin(BinUri, newBin);
            IObjectContainer newDb = Db4oEmbedded.OpenFile(Config(newStorage), BinUri);
            var result = newDb.Query(typeof (Item));
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ItemName, ((Item) result.Next())._name
                );
            newDb.Close();
        }

        private IEmbeddedConfiguration Config(MemoryStorage storage)
        {
            var config = Db4oEmbedded.NewConfiguration();
            config.File.Storage = storage;
            config.Common.ReflectWith(Platform4.ReflectorForType(typeof (Item
                )));
            return config;
        }

        public class Item
        {
            public string _name;

            public Item(string name)
            {
                _name = name;
            }
        }
    }
}