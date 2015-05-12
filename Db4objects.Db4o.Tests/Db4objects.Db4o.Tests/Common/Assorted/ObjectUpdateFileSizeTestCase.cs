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
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Assorted
{
    public class ObjectUpdateFileSizeTestCase : AbstractDb4oTestCase, IOptOutMultiSession
        , IOptOutDefragSolo
    {
        public static void Main(string[] args)
        {
            new ObjectUpdateFileSizeTestCase().RunAll();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            var item = new Item("foo"
                );
            Store(item);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestFileSize()
        {
            WarmUp();
            AssertFileSizeConstant();
        }

        /// <exception cref="System.Exception"></exception>
        private void AssertFileSizeConstant()
        {
            var beforeUpdate = DbSize();
            for (var j = 0; j < 10; j++)
            {
                Defragment();
                for (var i = 0; i < 15; ++i)
                {
                    UpdateItem();
                }
                Defragment();
                var afterUpdate = DbSize();
                Assert.IsSmaller(30, afterUpdate - beforeUpdate);
            }
        }

        /// <exception cref="System.Exception"></exception>
        /// <exception cref="System.IO.IOException"></exception>
        private void WarmUp()
        {
            for (var j = 0; j < 3; j++)
            {
                for (var i = 0; i < 3; ++i)
                {
                    UpdateItem();
                    Db().Commit();
                    Defragment();
                }
            }
        }

        /// <exception cref="System.Exception"></exception>
        /// <exception cref="System.IO.IOException"></exception>
        private void UpdateItem()
        {
            var item = ((Item) RetrieveOnlyInstance
                (typeof (Item)));
            Store(item);
            Db().Commit();
        }

        private long DbSize()
        {
            return Db().SystemInfo().TotalSize();
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