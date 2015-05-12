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
using Db4objects.Db4o.Activation;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.IO;
using Db4objects.Db4o.TA;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.TP
{
    public class CommitTimeStampsWithTPTestCase : AbstractDb4oTestCase
    {
        public virtual void TestWorksWithoutTP()
        {
            AssertUpdate(false);
        }

        private void AssertUpdate(bool isTP)
        {
            var item = new NamedItem
                ();
            Store(item);
            Commit();
            var firstTS = CommitTimestampFor(item);
            item.SetName("New Name");
            if (!isTP)
            {
                Store(item);
            }
            Commit();
            var secondTS = CommitTimestampFor(item);
            AssertChangesHaveBeenStored(Db());
            Assert.IsTrue(secondTS > firstTS);
        }

        private long CommitTimestampFor(NamedItem item)
        {
            return Db().Ext().GetObjectInfo(item).GetCommitTimestamp();
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestWorksWithTP()
        {
            Fixture().ConfigureAtRuntime(new _IRuntimeConfigureAction_38());
            Reopen();
            AssertUpdate(true);
        }

        private void AssertChangesHaveBeenStored(IObjectContainer container)
        {
            var session = container.Ext().OpenSession();
            try
            {
                var item = ((NamedItem
                    ) session.Query(typeof (NamedItem))[0]);
                Assert.AreEqual("New Name", item.GetName());
            }
            finally
            {
                session.Close();
            }
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.GenerateCommitTimestamps(true);
            config.Storage = new MemoryStorage();
        }

        private sealed class _IRuntimeConfigureAction_38 : IRuntimeConfigureAction
        {
            public void Apply(IConfiguration config)
            {
                config.Add(new TransparentPersistenceSupport());
            }
        }

        public class NamedItem : IActivatable
        {
            [NonSerialized] private IActivator _activator;

            private string name = "default";

            public virtual void Activate(ActivationPurpose purpose)
            {
                if (_activator != null)
                {
                    _activator.Activate(purpose);
                }
            }

            public virtual void Bind(IActivator activator)
            {
                if (_activator == activator)
                {
                    return;
                }
                if (activator != null && _activator != null)
                {
                    throw new InvalidOperationException();
                }
                _activator = activator;
            }

            public virtual void SetName(string name)
            {
                Activate(ActivationPurpose.Write);
                this.name = name;
            }

            public virtual string GetName()
            {
                Activate(ActivationPurpose.Read);
                return name;
            }
        }
    }
}