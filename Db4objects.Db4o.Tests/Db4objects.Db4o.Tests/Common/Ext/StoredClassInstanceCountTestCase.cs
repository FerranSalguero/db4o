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
using Db4objects.Db4o.Ext;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Ext
{
    public class StoredClassInstanceCountTestCase : AbstractDb4oTestCase
    {
        private const int CountA = 5;

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            for (var idx = 0; idx < CountA; idx++)
            {
                Store(new ItemA());
            }
            Store(new ItemB());
        }

        public virtual void TestInstanceCount()
        {
            AssertInstanceCount(typeof (ItemA), CountA);
            AssertInstanceCount(typeof (ItemB), 1);
            Store(new ItemA());
            DeleteAll(typeof (ItemB));
            AssertInstanceCount(typeof (ItemA), CountA + 1);
            AssertInstanceCount(typeof (ItemB), 0);
        }

        public virtual void TestTransactionalInstanceCount()
        {
            if (!IsMultiSession())
            {
                return;
            }
            var otherClient = OpenNewSession();
            Store(new ItemA());
            DeleteAll(typeof (ItemB));
            AssertInstanceCount(Db(), typeof (ItemA), CountA
                                                      + 1);
            AssertInstanceCount(Db(), typeof (ItemB), 0);
            AssertInstanceCount(otherClient, typeof (ItemA),
                CountA);
            AssertInstanceCount(otherClient, typeof (ItemB),
                1);
            Db().Commit();
            AssertInstanceCount(Db(), typeof (ItemA), CountA
                                                      + 1);
            AssertInstanceCount(Db(), typeof (ItemB), 0);
            AssertInstanceCount(otherClient, typeof (ItemA),
                CountA + 1);
            AssertInstanceCount(otherClient, typeof (ItemB),
                0);
            otherClient.Commit();
            otherClient.Store(new ItemB());
            AssertInstanceCount(Db(), typeof (ItemB), 0);
            AssertInstanceCount(otherClient, typeof (ItemB),
                1);
            otherClient.Commit();
            AssertInstanceCount(Db(), typeof (ItemB), 1);
            AssertInstanceCount(otherClient, typeof (ItemB),
                1);
            otherClient.Close();
        }

        private void AssertInstanceCount(Type clazz, int expectedCount)
        {
            AssertInstanceCount(Db(), clazz, expectedCount);
        }

        private void AssertInstanceCount(IExtObjectContainer container, Type clazz, int expectedCount
            )
        {
            var storedClazz = container.Ext().StoredClass(clazz);
            Assert.AreEqual(expectedCount, storedClazz.InstanceCount());
        }

        public static void Main(string[] args)
        {
            new StoredClassInstanceCountTestCase().RunAll();
        }

        public class ItemA
        {
        }

        public class ItemB
        {
        }
    }
}