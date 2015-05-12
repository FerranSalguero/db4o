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

namespace Db4objects.Db4o.Tests.Common.Assorted
{
    public class CommitTimestampTestCase : AbstractDb4oTestCase
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.GenerateCommitTimestamps(true);
        }

        public virtual void TestUpdateAndQuery()
        {
            var item1 = new Item();
            Store(item1);
            var item2 = new Item();
            Store(item2);
            Commit();
            var initialCommitTimestamp1 = Db().GetObjectInfo(item1).GetCommitTimestamp();
            var initialCommitTimestamp2 = Db().GetObjectInfo(item2).GetCommitTimestamp();
            Assert.AreEqual(initialCommitTimestamp1, initialCommitTimestamp2);
            Store(item2);
            Commit();
            var secondCommitTimestamp1 = Db().GetObjectInfo(item1).GetCommitTimestamp();
            var secondCommitTimestamp2 = Db().GetObjectInfo(item2).GetCommitTimestamp();
            Assert.AreEqual(initialCommitTimestamp1, secondCommitTimestamp1);
            Assert.AreNotEqual(initialCommitTimestamp2, secondCommitTimestamp2);
            AssertQueryForTimestamp(item1, initialCommitTimestamp1);
            AssertQueryForTimestamp(item2, secondCommitTimestamp2);
        }

        private void AssertQueryForTimestamp(Item expected, long
            timestamp)
        {
            var query = Db().Query();
            query.Constrain(typeof (Item));
            query.Descend(VirtualField.CommitTimestamp).Constrain(timestamp);
            var objectSet = query.Execute();
            Assert.AreEqual(1, objectSet.Count);
            var actual = (Item) objectSet.Next
                ();
            Assert.AreSame(expected, actual);
        }

        public class Item
        {
        }
    }
}