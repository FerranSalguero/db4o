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
using Db4objects.Db4o.Diagnostic;
using Db4objects.Db4o.Tests.Common.Api;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Assorted
{
    public class InvalidOffsetInDeleteTestCase : Db4oTestWithTempFile, IDiagnosticListener
    {
        public virtual void OnDiagnostic(IDiagnostic d)
        {
            if (d is DeletionFailed)
            {
                Assert.Fail("No deletion failed diagnostic message expected.");
            }
        }

        public virtual void Test()
        {
            var config = NewConfiguration();
            Configure(config);
            IObjectContainer objectContainer = Db4oEmbedded.OpenFile(config, TempFile());
            var item = new Item(
                );
            item._itemName = "item";
            item._parentName = "parent";
            objectContainer.Store(item);
            objectContainer.Close();
            config = NewConfiguration();
            Configure(config);
            objectContainer = Db4oEmbedded.OpenFile(config, TempFile());
            var query = objectContainer.Query();
            query.Constrain(typeof (Item));
            var objectSet = query.Execute();
            item = (Item) objectSet.Next();
            objectContainer.Store(item);
            objectContainer.Close();
        }

        private void Configure(IEmbeddedConfiguration config)
        {
            config.Common.Diagnostic.AddListener(this);
            config.File.GenerateCommitTimestamps = true;
            config.File.GenerateUUIDs = ConfigScope.Globally;
            config.Common.ObjectClass(typeof (Item)).ObjectField
                ("_itemName").Indexed(true);
            config.Common.ObjectClass(typeof (Parent)).ObjectField
                ("_parentName").Indexed(true);
        }

        public class Parent
        {
            public string _parentName;
        }

        public class Item : Parent
        {
            public string _itemName;
        }
    }
}