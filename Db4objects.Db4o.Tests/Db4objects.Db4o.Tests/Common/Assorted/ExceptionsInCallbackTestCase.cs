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
using System.Collections;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Consistency;
using Db4objects.Db4o.Events;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Assorted
{
    /// <exclude></exclude>
    public class ExceptionsInCallbackTestCase : AbstractDb4oTestCase
    {
        public static void Main(string[] args)
        {
            new ExceptionsInCallbackTestCase().RunSolo();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(typeof (Holder)).CascadeOnUpdate(true
                );
            config.ObjectClass(typeof (Holder)).CascadeOnDelete(true
                );
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestExceptionInUpdateCallback()
        {
            var doThrow = new BooleanByRef();
            EventRegistryFactory.ForObjectContainer(Db()).Updated += new _IEventListener4_42(doThrow).OnEvent;
            var holder = new Holder
                ();
            var item = new Item();
            Store(holder);
            Store(item);
            Commit();
            doThrow.value = true;
            holder.list = new ArrayList();
            holder.list.Add(item);
            try
            {
                Db().Store(holder, int.MaxValue);
            }
            catch (Exception)
            {
            }
            // rex.printStackTrace();
            Checkdb();
            Commit();
            Checkdb();
            Reopen();
            Checkdb();
        }

        private void Checkdb()
        {
            var consistencyReport = new ConsistencyChecker((LocalObjectContainer
                ) Container()).CheckSlotConsistency();
            Assert.IsTrue(consistencyReport.Consistent(), consistencyReport.ToString());
        }

        public class Holder
        {
            public int i;
            public IList list;
        }

        public class Item
        {
        }

        private sealed class _IEventListener4_42
        {
            private readonly BooleanByRef doThrow;

            public _IEventListener4_42(BooleanByRef doThrow)
            {
                this.doThrow = doThrow;
            }

            public void OnEvent(object sender, ObjectInfoEventArgs args
                )
            {
                if (doThrow.value)
                {
                    if (args.Info.GetObject().GetType().Equals(typeof (Item
                        )))
                    {
                        throw new Exception();
                    }
                }
            }
        }
    }
}