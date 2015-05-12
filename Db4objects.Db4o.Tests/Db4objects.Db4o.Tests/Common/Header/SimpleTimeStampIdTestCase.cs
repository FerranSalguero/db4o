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
using Db4objects.Db4o.Internal;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Header
{
    public class SimpleTimeStampIdTestCase : AbstractDb4oTestCase, IOptOutMultiSession
    {
        public static void Main(string[] arguments)
        {
            new SimpleTimeStampIdTestCase().RunSolo();
        }

        protected override void Configure(IConfiguration config)
        {
            var objectClass = config.ObjectClass(typeof (STSItem
                ));
            objectClass.GenerateUUIDs(true);
            config.GenerateCommitTimestamps(true);
        }

        protected override void Store()
        {
            Db().Store(new STSItem("one"));
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void Test()
        {
            var item = (STSItem) Db().
                QueryByExample(typeof (STSItem)).Next();
            var version = Db().GetObjectInfo(item).GetCommitTimestamp();
            Assert.IsGreater(0, version);
            Assert.IsGreaterOrEqual(version, CurrentVersion());
            Reopen();
            var item2 = new STSItem("two"
                );
            Db().Store(item2);
            Db().Commit();
            var secondVersion = Db().GetObjectInfo(item2).GetCommitTimestamp();
            Assert.IsGreater(version, secondVersion);
            Assert.IsGreaterOrEqual(secondVersion, CurrentVersion());
        }

        private long CurrentVersion()
        {
            return ((LocalObjectContainer) Db()).CurrentVersion();
        }

        public class STSItem
        {
            public string _name;

            public STSItem()
            {
            }

            public STSItem(string name)
            {
                _name = name;
            }
        }
    }
}