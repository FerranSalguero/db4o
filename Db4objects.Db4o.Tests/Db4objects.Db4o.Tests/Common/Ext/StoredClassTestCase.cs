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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Ext
{
    public class StoredClassTestCase : AbstractDb4oTestCase
    {
        private static readonly string FieldName = "_name";
        private static readonly string ItemName = "item";
        private long _id;

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(typeof (Item)).ObjectField(FieldName).Indexed
                (true);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            var item = new Item(ItemName);
            Store(item);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Db4oSetupAfterStore()
        {
            _id = Db().GetID(((Item) RetrieveOnlyInstance(typeof (Item
                ))));
        }

        public virtual void TestUnknownStoredClass()
        {
            Assert.IsNull(StoredClass(GetType()));
        }

        public virtual void TestStoredClassImpl()
        {
            Assert.IsInstanceOf(typeof (StoredClassImpl), ItemStoredClass());
        }

        public virtual void TestGetIds()
        {
            var itemClass = ItemStoredClass();
            var ids = itemClass.GetIDs();
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(_id, ids[0]);
        }

        public virtual void TestGetName()
        {
            var itemClass = ItemStoredClass();
            Assert.AreEqual(Reflector().ForClass(typeof (Item)).GetName(),
                itemClass.GetName());
        }

        public virtual void TestGetParentStoredClass()
        {
            var itemClass = ItemStoredClass();
            var parentStoredClass = itemClass.GetParentStoredClass();
            Assert.AreEqual(Reflector().ForClass(typeof (ItemParent)).GetName
                (), parentStoredClass.GetName());
            Assert.AreEqual(parentStoredClass, Db().StoredClass(typeof (ItemParent
                )));
        }

        public virtual void TestGetStoredFields()
        {
            AssertStoredField(typeof (Item), FieldName, ItemName, typeof (string
                ), true, false);
            AssertStoredField(typeof (ItemParent), "_array", null, typeof (
                string), false, true);
            var itemStoredClass = ItemStoredClass();
            var storedField = itemStoredClass.StoredField(FieldName, null);
            var sameStoredField = itemStoredClass.GetStoredFields()[0];
            var otherStoredField = StoredClass(typeof (ItemParent
                )).GetStoredFields()[0];
            Assert.EqualsAndHashcode(storedField, sameStoredField, otherStoredField);
            Assert.IsNull(itemStoredClass.StoredField(string.Empty, null));
        }

        private void AssertStoredField(Type objectClass, string fieldName, object expectedFieldValue
            , Type expectedFieldType, bool hasIndex, bool isArray)
        {
            var storedClass = StoredClass(objectClass);
            var storedFields = storedClass.GetStoredFields();
            Assert.AreEqual(1, storedFields.Length);
            var storedField = storedFields[0];
            Assert.AreEqual(fieldName, storedField.GetName());
            var storedFieldByName = storedClass.StoredField(fieldName, expectedFieldType
                );
            Assert.AreEqual(storedField, storedFieldByName);
            var item = RetrieveOnlyInstance(objectClass);
            Assert.AreEqual(expectedFieldValue, storedField.Get(item));
            var fieldType = storedField.GetStoredType();
            Assert.AreEqual(Reflector().ForClass(expectedFieldType), fieldType);
            Assert.AreEqual(isArray, storedField.IsArray());
            if (IsMultiSession())
            {
                return;
            }
            Assert.AreEqual(hasIndex, storedField.HasIndex());
            // FIXME: test rename
            if (!hasIndex)
            {
                Assert.Expect(typeof (Exception), new _ICodeBlock_113(storedField));
            }
            else
            {
                var count = new IntByRef();
                storedField.TraverseValues(new _IVisitor4_123(count, expectedFieldValue));
                Assert.AreEqual(1, count.value);
            }
        }

        public virtual void TestEqualsAndHashCode()
        {
            var clazz = ItemStoredClass();
            var same = ItemStoredClass();
            var other = Db().StoredClass(typeof (ItemParent));
            Assert.EqualsAndHashcode(clazz, same, other);
        }

        private IStoredClass ItemStoredClass()
        {
            return StoredClass(typeof (Item));
        }

        private IStoredClass StoredClass(Type clazz)
        {
            return Db().StoredClass(clazz);
        }

        public class ItemParent
        {
            public string[] _array;
        }

        public class Item : ItemParent
        {
            public string _name;

            public Item(string name)
            {
                _name = name;
            }
        }

        private sealed class _ICodeBlock_113 : ICodeBlock
        {
            private readonly IStoredField storedField;

            public _ICodeBlock_113(IStoredField storedField)
            {
                this.storedField = storedField;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                storedField.TraverseValues(new _IVisitor4_115());
            }

            private sealed class _IVisitor4_115 : IVisitor4
            {
                public void Visit(object obj)
                {
                }
            }
        }

        private sealed class _IVisitor4_123 : IVisitor4
        {
            private readonly IntByRef count;
            private readonly object expectedFieldValue;

            public _IVisitor4_123(IntByRef count, object expectedFieldValue)
            {
                this.count = count;
                this.expectedFieldValue = expectedFieldValue;
            }

            public void Visit(object obj)
            {
                count.value++;
                Assert.AreEqual(expectedFieldValue, obj);
            }
        }
    }
}