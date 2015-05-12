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
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Freespace
{
    public class BlockConfigurationFileSizeTestCase : FileSizeTestCaseBase
    {
        public static void Main(string[] args)
        {
            new BlockConfigurationFileSizeTestCase().RunSolo();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.BlockSize(8);
        }

        public virtual void Test()
        {
            Store(new Item("one"));
            Db().Commit();
            var initialSize = DatabaseFileSize();
            for (var i = 0; i < 100; i++)
            {
                Store(new Item("two"));
            }
            Db().Commit();
            var modifiedSize = DatabaseFileSize();
            var sizeIncrease = modifiedSize - initialSize;
            Assert.IsSmaller(30000, sizeIncrease);
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