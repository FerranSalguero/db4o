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
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Delete;
using Db4objects.Db4o.Internal.Handlers;
using Db4objects.Db4o.Internal.Marshall;
using Db4objects.Db4o.Marshall;
using Db4objects.Db4o.Tests.Common.Api;
using Db4objects.Db4o.Typehandlers;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Migration
{
    public class FieldsToTypeHandlerMigrationTestCase : Db4oTestWithTempFile
    {
        internal ItemTypeHandler _typeHandler;

        public static void Main(string[] args)
        {
            new ConsoleTestRunner(typeof (FieldsToTypeHandlerMigrationTestCase)).Run();
        }

        public virtual void TestMigration()
        {
            _typeHandler = null;
            Store(new Item(42));
            var item = RetrieveOnlyItemInstance();
            Assert.AreEqual(42, item._id);
            AssertItemStoredField(42);
            _typeHandler = new ItemTypeHandler();
            item = RetrieveOnlyItemInstance();
            Assert.AreEqual(42, item._id);
            AssertTypeHandlerCalls(0, 0);
            AssertItemStoredField(42);
            UpdateItem();
            AssertTypeHandlerCalls(1, 0);
            AssertItemStoredField(null);
            item = RetrieveOnlyItemInstance();
            Assert.AreEqual(42, item._id);
            AssertTypeHandlerCalls(0, 1);
            AssertItemStoredField(null);
        }

        public virtual void TestTypeHandler()
        {
            _typeHandler = new ItemTypeHandler();
            Store(new Item(42));
            AssertTypeHandlerCalls(1, 0);
            var item = RetrieveOnlyItemInstance();
            Assert.AreEqual(42, item._id);
            AssertTypeHandlerCalls(0, 1);
            UpdateItem();
            AssertTypeHandlerCalls(1, 1);
        }

        private void AssertItemStoredField(object expectedValue)
        {
            var db = OpenContainer();
            try
            {
                var objectSet = db.Query(typeof (Item)
                    );
                Assert.AreEqual(1, objectSet.Count);
                var item = (Item
                    ) objectSet.Next();
                var storedField = db.Ext().StoredClass(typeof (Item
                    )).StoredField("_id", null);
                var actualValue = storedField.Get(item);
                Assert.AreEqual(expectedValue, actualValue);
            }
            finally
            {
                db.Close();
            }
        }

        private void AssertTypeHandlerCalls(int writeCalls, int readCalls)
        {
            Assert.AreEqual(writeCalls, _typeHandler.WriteCalls());
            Assert.AreEqual(readCalls, _typeHandler.ReadCalls());
        }

        private Item RetrieveOnlyItemInstance()
        {
            var db = OpenContainer();
            try
            {
                var objectSet = db.Query(typeof (Item)
                    );
                Assert.AreEqual(1, objectSet.Count);
                var item = (Item
                    ) objectSet.Next();
                return item;
            }
            finally
            {
                db.Close();
            }
        }

        private void Store(Item item)
        {
            var db = OpenContainer();
            try
            {
                db.Store(item);
            }
            finally
            {
                db.Close();
            }
        }

        private void UpdateItem()
        {
            var db = OpenContainer();
            try
            {
                var objectSet = db.Query(typeof (Item)
                    );
                db.Store(objectSet.Next());
            }
            finally
            {
                db.Close();
            }
        }

        private IObjectContainer OpenContainer()
        {
            if (_typeHandler != null)
            {
                _typeHandler.Reset();
            }
            var configuration = NewConfiguration();
            if (_typeHandler != null)
            {
                configuration.Common.RegisterTypeHandler(new SingleClassTypeHandlerPredicate(typeof (
                    Item)), _typeHandler);
            }
            return Db4oEmbedded.OpenFile(configuration, TempFile());
        }

        public virtual void Defragment(IDefragmentContext context)
        {
        }

        // TODO Auto-generated method stub
        /// <exception cref="Db4objects.Db4o.Ext.Db4oIOException"></exception>
        public virtual void Delete(IDeleteContext context)
        {
        }

        // TODO Auto-generated method stub
        public virtual object Read(IReadContext context)
        {
            // TODO Auto-generated method stub
            return null;
        }

        public virtual void Write(IWriteContext context, object obj)
        {
        }

        // TODO Auto-generated method stub
        public virtual IPreparedComparison PrepareComparison(IContext context, object obj
            )
        {
            // TODO Auto-generated method stub
            return null;
        }

        public class Item
        {
            public int _id;

            public Item(int id)
            {
                _id = id;
            }
        }

        public class ItemTypeHandler : IReferenceTypeHandler, ICascadingTypeHandler, IVariableLengthTypeHandler
        {
            private int _readCalls;
            private int _writeCalls;

            public virtual void CascadeActivation(IActivationContext context)
            {
                throw new NotImplementedException();
            }

            public virtual void CollectIDs(QueryingReadContext context)
            {
                throw new NotImplementedException();
            }

            public virtual ITypeHandler4 ReadCandidateHandler(QueryingReadContext context)
            {
                throw new NotImplementedException();
            }

            public virtual void Defragment(IDefragmentContext context)
            {
                throw new NotImplementedException();
            }

            /// <exception cref="Db4objects.Db4o.Ext.Db4oIOException"></exception>
            public virtual void Delete(IDeleteContext context)
            {
                throw new NotImplementedException();
            }

            public virtual void Activate(IReferenceActivationContext context)
            {
                _readCalls++;
                var item = (Item
                    ) ((UnmarshallingContext) context).PersistentObject();
                item._id = context.ReadInt() - 42;
            }

            public virtual void Write(IWriteContext context, object obj)
            {
                _writeCalls++;
                var item = (Item
                    ) obj;
                context.WriteInt(item._id + 42);
            }

            public virtual IPreparedComparison PrepareComparison(IContext context, object obj
                )
            {
                throw new NotImplementedException();
            }

            public virtual int WriteCalls()
            {
                return _writeCalls;
            }

            public virtual int ReadCalls()
            {
                return _readCalls;
            }

            public virtual void Reset()
            {
                _writeCalls = 0;
                _readCalls = 0;
            }
        }
    }
}