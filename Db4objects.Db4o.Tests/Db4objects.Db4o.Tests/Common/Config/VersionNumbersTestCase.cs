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
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Config
{
    public class VersionNumbersTestCase : AbstractDb4oTestCase
    {
        private static readonly string Original = "original";
        private static readonly string Newer = "newer";

        public static void Main(string[] args)
        {
            new VersionNumbersTestCase().RunAll();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.GenerateCommitTimestamps(true);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(new Item(Original));
        }

        public virtual void TestVersionIncrease()
        {
            var item = (Item
                ) RetrieveOnlyInstance(typeof (Item));
            var objectInfo = Db().GetObjectInfo(item);
            var version1 = objectInfo.GetCommitTimestamp();
            item._name = "modified";
            Db().Store(item);
            Db().Commit();
            var version2 = objectInfo.GetCommitTimestamp();
            Assert.IsGreater(version1, version2);
            Db().Store(item);
            Db().Commit();
            objectInfo = Db().GetObjectInfo(item);
            var version3 = objectInfo.GetCommitTimestamp();
            Assert.IsGreater(version2, version3);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestTransactionConsistentVersion()
        {
            Store(new Item(Newer));
            Db().Commit();
            var newer = ItemByName(Newer);
            var original = ItemByName(Original);
            Assert.IsGreater(Version(original), Version(newer));
            newer._name += " modified";
            original._name += " modified";
            Store(newer);
            Store(original);
            Db().Commit();
            Assert.AreEqual(Version(newer), Version(original));
            Reopen();
            newer = ItemByName(newer._name);
            original = ItemByName(original._name);
            Assert.AreEqual(Version(newer), Version(original));
        }

        private long Version(object obj)
        {
            return Db().GetObjectInfo(obj).GetCommitTimestamp();
        }

        private Item ItemByName(string @string)
        {
            var q = Db().Query();
            q.Constrain(typeof (Item));
            q.Descend("_name").Constrain(@string);
            var @object = q.Execute().Next();
            return (Item) @object;
        }

        public virtual void TestQueryForVersionNumber()
        {
            Store(new Item(Newer));
            Db().Commit();
            var newer = ItemByName(Newer);
            var version = Version(newer);
            var query = Db().Query();
            query.Descend(VirtualField.CommitTimestamp).Constrain(version).Smaller().Not();
            var set = query.Execute();
            Assert.AreEqual(1, set.Count);
            Assert.AreSame(newer, ((Item) set.Next()));
        }

        public class Item
        {
            public string _name;

            public Item(string _name)
            {
                this._name = _name;
            }
        }
    }
}