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
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Querying
{
    public class CascadeDeleteArray : AbstractDb4oTestCase
    {
        public ArrayElem[] array;

        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(this).CascadeOnDelete(true);
        }

        protected override void Store()
        {
            var cda = new CascadeDeleteArray();
            cda.array = new[]
            {
                new ArrayElem
                    ("one"),
                new ArrayElem("two"), new ArrayElem
                    ("three")
            };
            Db().Store(cda);
        }

        public virtual void Test()
        {
            var cda = (CascadeDeleteArray) RetrieveOnlyInstance
                (GetType());
            Assert.AreEqual(3, CountOccurences(typeof (ArrayElem)));
            Db().Delete(cda);
            Assert.AreEqual(0, CountOccurences(typeof (ArrayElem)));
            Db().Rollback();
            Assert.AreEqual(3, CountOccurences(typeof (ArrayElem)));
            Db().Delete(cda);
            Assert.AreEqual(0, CountOccurences(typeof (ArrayElem)));
            Db().Commit();
            Assert.AreEqual(0, CountOccurences(typeof (ArrayElem)));
        }

        public class ArrayElem
        {
            public string name;

            public ArrayElem(string name)
            {
                this.name = name;
            }
        }
    }
}