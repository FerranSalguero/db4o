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

using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Assorted
{
    public class DatabaseUnicityTest : AbstractDb4oTestCase
    {
        public virtual void Test()
        {
            var ht = new Hashtable4();
            var container = Container();
            container.ShowInternalClasses(true);
            var q = Db().Query();
            q.Constrain(typeof (Db4oDatabase));
            var objectSet = q.Execute();
            while (objectSet.HasNext())
            {
                var identity = (Db4oDatabase) objectSet.Next();
                Assert.IsFalse(ht.ContainsKey(identity.i_signature));
                ht.Put(identity.i_signature, string.Empty);
            }
            container.ShowInternalClasses(false);
        }
    }
}