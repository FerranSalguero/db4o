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
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Defragment
{
    public class DefragInheritedFieldIndexTestCase : AbstractDb4oTestCase, IOptOutMultiSession
    {
        private static readonly string FieldName = "_name";
        private static readonly string[] Names = {"Foo", "Bar", "Baz"};

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(typeof (ParentItem)).ObjectField
                (FieldName).Indexed(true);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            for (var nameIndex = 0; nameIndex < Names.Length; ++nameIndex)
            {
                var name = Names[nameIndex];
                Store(new ChildItem(name));
            }
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestDefragInheritedFieldIndex()
        {
            AssertQueryByIndex();
            Defragment();
            AssertQueryByIndex();
        }

        private void AssertQueryByIndex()
        {
            var query = NewQuery(typeof (ChildItem));
            query.Descend(FieldName).Constrain(Names[0]);
            var result = query.Execute();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(Names[0], ((ChildItem) result.Next
                ())._name);
        }

        public class ParentItem
        {
            public string _name;

            public ParentItem(string name)
            {
                _name = name;
            }
        }

        public class ChildItem : ParentItem
        {
            public ChildItem(string name) : base(name)
            {
            }
        }
    }
}