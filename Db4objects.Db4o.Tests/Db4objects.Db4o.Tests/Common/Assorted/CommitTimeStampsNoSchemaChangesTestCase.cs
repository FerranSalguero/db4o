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

namespace Db4objects.Db4o.Tests.Common.Assorted
{
    public class CommitTimeStampsNoSchemaChangesTestCase : AbstractDb4oTestCase
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.GenerateCommitTimestamps(true);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(new Holder());
            IntializeCSClasses();
        }

        private void IntializeCSClasses()
        {
            for (var holderIter = Db().Query(typeof (Holder
                )).GetEnumerator();
                holderIter.MoveNext();)
            {
                var holder = ((Holder
                    ) holderIter.Current);
                Db().GetID(holder);
            }
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestCommitTimestampsNoSchemaDetection()
        {
            Fixture().ConfigureAtRuntime(new _IRuntimeConfigureAction_30());
            Reopen();
            Store(new Holder());
            Commit();
            for (var holderIter = Db().Query(typeof (Holder
                )).GetEnumerator();
                holderIter.MoveNext();)
            {
                var holder = ((Holder
                    ) holderIter.Current);
                var objectInfo = Db().Ext().GetObjectInfo(holder);
                var ts = objectInfo.GetCommitTimestamp();
                Assert.IsGreater(0, ts);
            }
        }

        private sealed class _IRuntimeConfigureAction_30 : IRuntimeConfigureAction
        {
            public void Apply(IConfiguration config)
            {
                config.DetectSchemaChanges(false);
                config.GenerateCommitTimestamps(true);
            }
        }

        public class Holder
        {
            public string data = "data";
        }
    }
}