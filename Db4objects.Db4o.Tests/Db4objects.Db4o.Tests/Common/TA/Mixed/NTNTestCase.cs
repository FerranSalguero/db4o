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

using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.TA.Mixed
{
    /// <exclude></exclude>
    public class NTNTestCase : ItemTestCaseBase
    {
        public static void Main(string[] args)
        {
            new NTNTestCase().RunAll();
        }

        /// <exception cref="System.Exception"></exception>
        protected override object CreateItem()
        {
            return new NTNItem(42);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void AssertRetrievedItem(object obj)
        {
            var item = (NTNItem) obj;
            Assert.IsNotNull(item.tnItem);
            Assert.IsNull(item.tnItem.list);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void AssertItemValue(object obj)
        {
            var item = (NTNItem) obj;
            Assert.AreEqual(LinkedList.NewList(42), item.tnItem.Value());
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestDeactivateDepth()
        {
            var item = (NTNItem) RetrieveOnlyInstance();
            var tnItem = item.tnItem;
            tnItem.Value();
            Assert.IsNotNull(tnItem.list);
            // item.tnItem.list
            Db().Deactivate(item, 2);
            // FIXME: failure 
            // Assert.isNull(tnItem.list);
            Db().Activate(item, 42);
            Db().Deactivate(item, 10);
            Assert.IsNull(tnItem.list);
        }
    }
}