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
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Fatalerror
{
    public class FatalExceptionInNestedCallTestCase : AbstractDb4oTestCase
    {
        public static void Main(string[] arguments)
        {
            new FatalExceptionInNestedCallTestCase().RunSolo();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            var childItem = new Item
                (null, 1);
            var parentItem = new Item
                (childItem, 0);
            Store(parentItem);
        }

        public virtual void Test()
        {
        }

        public class Item
        {
            public Item _child;
            public int _depth;

            public Item()
            {
            }

            public Item(Item child, int depth)
            {
                _child = child;
                _depth = depth;
            }
        }

        [Serializable]
        public class FatalError : Exception
        {
        }
    }
}