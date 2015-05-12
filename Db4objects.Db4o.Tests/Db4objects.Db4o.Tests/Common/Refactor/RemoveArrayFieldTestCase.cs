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
using Db4oUnit.Extensions.Util;

namespace Db4objects.Db4o.Tests.Common.Refactor
{
    public class RemoveArrayFieldTestCase : AbstractDb4oTestCase, IOptOutDefragSolo
    {
        /// <exception cref="System.Exception"></exception>
        public virtual void TestRemoveArrayField()
        {
            var dataA = new DataBefore
                ("a", true, new object[] {"X"});
            var dataB = new DataBefore
                ("b", false, new object[0]);
            Store(dataA);
            Store(dataB);
            var oc = Fixture().Config().ObjectClass(typeof (DataBefore
                ));
            // we must use ReflectPlatform here as the string must include
            // the assembly name in .net
            oc.Rename(CrossPlatformServices.FullyQualifiedName(typeof (DataAfter
                )));
            Reopen();
            var query = NewQuery(typeof (DataAfter));
            query.Descend("name").Constrain("a");
            var result = query.Execute();
            Assert.AreEqual(1, result.Count);
            var data = (DataAfter) result
                .Next();
            Assert.AreEqual(dataA.name, data.name);
            Assert.AreEqual(dataA.status, data.status);
        }

        public class DataBefore
        {
            public object[] array;
            public string name;
            public bool status;

            public DataBefore(string name, bool status, object[] array)
            {
                this.name = name;
                this.status = status;
                this.array = array;
            }
        }

        public class DataAfter
        {
            public string name;
            public bool status;

            public DataAfter(string name, bool status)
            {
                this.name = name;
                this.status = status;
            }
        }
    }
}