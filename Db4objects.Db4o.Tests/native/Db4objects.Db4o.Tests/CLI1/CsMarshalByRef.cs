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
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.CLI1
{
    /// <summary>
    ///     Summary description for CsMarshalByRef.
    /// </summary>
    public class CsMarshalByRef : AbstractDb4oTestCase
    {
        protected override void Store()
        {
            var item = new Item();
            item._field = "foo";
            item._placeHolder = 42;
            Store(item);
        }

        public void Test()
        {
            var item = (Item) RetrieveOnlyInstance(typeof (Item));
            Assert.AreEqual("foo", item._field);
            Assert.AreEqual(42, item._placeHolder);
        }

        public class Item : MarshalByRefObject
        {
            public string _field;
            public int _placeHolder;
        }
    }
}

#endif