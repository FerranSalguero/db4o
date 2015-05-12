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
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Tests.Common.Api;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Internal
{
    public class EmbeddedClientObjectContainerTestCase : Db4oTestWithTempFile
    {
        private static readonly string FieldName = "_name";
        private static readonly string OriginalName = "original";
        private static readonly string ChangedName = "changed";
        protected IExtObjectContainer _client1;
        protected IExtObjectContainer _client2;
        private LocalObjectContainer _server;

        public virtual void TestReferenceSystemIsolation()
        {
            var item = new Item
                ("one");
            _client1.Store(item);
            _client1.Commit();
            var client2Item = RetrieveItemFromClient2(
                );
            Assert.AreNotSame(item, client2Item);
        }

        public virtual void TestSetAndCommitIsolation()
        {
            var item = new Item
                ("one");
            _client1.Store(item);
            AssertItemCount(_client2, 0);
            _client1.Commit();
            AssertItemCount(_client2, 1);
        }

        public virtual void TestActivate()
        {
            var storedItem = StoreItemToClient1AndCommit
                ();
            var id = _client1.GetID(storedItem);
            var retrievedItem = (Item
                ) _client2.GetByID(id);
            Assert.IsNull(retrievedItem._name);
            Assert.IsFalse(_client2.IsActive(retrievedItem));
            _client2.Activate(retrievedItem, 1);
            Assert.AreEqual(OriginalName, retrievedItem._name);
            Assert.IsTrue(_client2.IsActive(retrievedItem));
        }

        public virtual void TestBackup()
        {
            Assert.Expect(typeof (NotSupportedException), new _ICodeBlock_84(this));
        }

        public virtual void TestBindIsolation()
        {
            var storedItem = StoreItemToClient1AndCommit
                ();
            var id = _client1.GetID(storedItem);
            var retrievedItem = RetrieveItemFromClient2
                ();
            var boundItem = new Item
                (ChangedName);
            _client1.Bind(boundItem, id);
            Assert.AreSame(boundItem, _client1.GetByID(id));
            Assert.AreSame(retrievedItem, _client2.GetByID(id));
        }

        public virtual void TestClose()
        {
            Transaction trans = null;
            lock (_server.Lock())
            {
                trans = _server.NewUserTransaction();
            }
            var referenceSystem = trans.ReferenceSystem();
            var client = new ObjectContainerSession(_server, trans);
            // FIXME: Need to unregister reference system also
            //        for crashed clients that never get closed. 
            client.Close();
            // should have been removed on close.
            var wasNotRemovedYet = _server.ReferenceSystemRegistry().RemoveReferenceSystem(referenceSystem
                );
            Assert.IsFalse(wasNotRemovedYet);
        }

        public virtual void TestCommitOnClose()
        {
            var storedItem = StoreItemToClient1AndCommit
                ();
            storedItem._name = ChangedName;
            _client1.Store(storedItem);
            _client1.Close();
            var retrievedItem = RetrieveItemFromClient2
                ();
            Assert.AreEqual(ChangedName, retrievedItem._name);
        }

        public virtual void TestConfigure()
        {
            Assert.IsNotNull(_client1.Configure());
        }

        public virtual void TestDeactivate()
        {
            var item = StoreItemToClient1AndCommit();
            var holder = new ItemHolder
                (item);
            _client1.Store(holder);
            _client1.Commit();
            _client1.Deactivate(holder, 1);
            Assert.IsNull(holder._item);
        }

        public virtual void TestDelete()
        {
            var item = StoreItemToClient1AndCommit();
            Assert.IsTrue(_client1.IsStored(item));
            _client1.Delete(item);
            Assert.IsFalse(_client1.IsStored(item));
        }

        public virtual void TestDescendIsolation()
        {
            var storedItem = StoreItemToClient1AndCommit
                ();
            storedItem._name = ChangedName;
            _client1.Store(storedItem);
            var id = (int) _client1.GetID(storedItem);
            var retrievedItem = _client2.GetByID(id);
            Assert.IsNotNull(retrievedItem);
            var descendValue = _client2.Descend(retrievedItem, new[] {FieldName});
            Assert.AreEqual(OriginalName, descendValue);
            _client1.Commit();
            descendValue = _client2.Descend(retrievedItem, new[] {FieldName});
            Assert.AreEqual(ChangedName, descendValue);
        }

        public virtual void TestExt()
        {
            Assert.IsInstanceOf(typeof (IExtObjectContainer), _client1.Ext());
        }

        public virtual void TestGet()
        {
            var storedItem = StoreItemToClient1AndCommit
                ();
            var retrievedItem = _client1.QueryByExample(new Item
                ()).Next();
            Assert.AreSame(storedItem, retrievedItem);
        }

        public virtual void TestGetID()
        {
            var storedItem = StoreItemToClient1AndCommit
                ();
            var id = _client1.GetID(storedItem);
            Assert.IsGreater(1, id);
        }

        public virtual void TestGetByID()
        {
            var storedItem = StoreItemToClient1AndCommit
                ();
            var id = _client1.GetID(storedItem);
            Assert.AreSame(storedItem, _client1.GetByID(id));
        }

        public virtual void TestGetObjectInfo()
        {
            var storedItem = StoreItemToClient1AndCommit
                ();
            var objectInfo = _client1.GetObjectInfo(storedItem);
            Assert.IsNotNull(objectInfo);
        }

        public virtual void TestGetByUUID()
        {
            var storedItem = StoreItemToClient1AndCommit
                ();
            var objectInfo = _client1.GetObjectInfo(storedItem);
            var retrievedItem = _client1.GetByUUID(objectInfo.GetUUID());
            Assert.AreSame(storedItem, retrievedItem);
            retrievedItem = _client2.GetByUUID(objectInfo.GetUUID());
            Assert.AreNotSame(storedItem, retrievedItem);
        }

        public virtual void TestIdenity()
        {
            var identity1 = _client1.Identity();
            Assert.IsNotNull(identity1);
            var identity2 = _client2.Identity();
            Assert.IsNotNull(identity2);
            // TODO: Db4oDatabase is shared between embedded clients.
            // This should work, since there is an automatic bind
            // replacement. Replication test cases will tell.
            Assert.AreSame(identity1, identity2);
        }

        public virtual void TestIsCached()
        {
            var storedItem = StoreItemToClient1AndCommit
                ();
            var id = _client1.GetID(storedItem);
            Assert.IsFalse(_client2.IsCached(id));
            var retrievedItem = (Item
                ) _client2.GetByID(id);
            Assert.IsNotNull(retrievedItem);
            Assert.IsTrue(_client2.IsCached(id));
        }

        public virtual void TestIsClosed()
        {
            _client1.Close();
            Assert.IsTrue(_client1.IsClosed());
        }

        public virtual void TestIsStored()
        {
            var storedItem = StoreItemToClient1AndCommit
                ();
            Assert.IsTrue(_client1.IsStored(storedItem));
            Assert.IsFalse(_client2.IsStored(storedItem));
        }

        public virtual void TestKnownClasses()
        {
            var knownClasses = _client1.KnownClasses();
            var itemClass = _client1.Reflector().ForClass(typeof (Item
                ));
            ArrayAssert.ContainsByIdentity(knownClasses, new[] {itemClass});
        }

        public virtual void TestLock()
        {
            Assert.AreSame(_server.Lock(), _client1.Lock());
        }

        public virtual void TestPeekPersisted()
        {
            var storedItem = StoreItemToClient1AndCommit
                ();
            storedItem._name = ChangedName;
            _client1.Store(storedItem);
            var peekedItem = (Item) _client1.PeekPersisted(storedItem,
                2, true);
            Assert.IsNotNull(peekedItem);
            Assert.AreNotSame(peekedItem, storedItem);
            Assert.AreEqual(OriginalName, peekedItem._name);
            peekedItem = (Item
                ) _client1.PeekPersisted(storedItem, 2, false);
            Assert.IsNotNull(peekedItem);
            Assert.AreNotSame(peekedItem, storedItem);
            Assert.AreEqual(ChangedName, peekedItem._name);
            var retrievedItem = RetrieveItemFromClient2
                ();
            peekedItem = (Item
                ) _client2.PeekPersisted(retrievedItem, 2, false);
            Assert.IsNotNull(peekedItem);
            Assert.AreNotSame(peekedItem, retrievedItem);
            Assert.AreEqual(OriginalName, peekedItem._name);
        }

        public virtual void TestPurge()
        {
            var storedItem = StoreItemToClient1AndCommit
                ();
            Assert.IsTrue(_client1.IsStored(storedItem));
            _client1.Purge(storedItem);
            Assert.IsFalse(_client1.IsStored(storedItem));
        }

        public virtual void TestReflector()
        {
            Assert.IsNotNull(_client1.Reflector());
        }

        public virtual void TestRefresh()
        {
            var storedItem = StoreItemToClient1AndCommit
                ();
            storedItem._name = ChangedName;
            _client1.Refresh(storedItem, 2);
            Assert.AreEqual(OriginalName, storedItem._name);
        }

        public virtual void TestRollback()
        {
            var storedItem = StoreItemToClient1AndCommit
                ();
            storedItem._name = ChangedName;
            _client1.Store(storedItem);
            _client1.Rollback();
            _client1.Commit();
            var retrievedItem = RetrieveItemFromClient2
                ();
            Assert.AreEqual(OriginalName, retrievedItem._name);
        }

        public virtual void TestSetSemaphore()
        {
            var semaphoreName = "sem";
            Assert.IsTrue(_client1.SetSemaphore(semaphoreName, 0));
            Assert.IsFalse(_client2.SetSemaphore(semaphoreName, 0));
            _client1.ReleaseSemaphore(semaphoreName);
            Assert.IsTrue(_client2.SetSemaphore(semaphoreName, 0));
            _client2.Close();
            Assert.IsTrue(_client1.SetSemaphore(semaphoreName, 0));
        }

        public virtual void TestSetWithDepth()
        {
            var item = StoreItemToClient1AndCommit();
            var holder = new ItemHolder
                (item);
            _client1.Store(holder);
            _client1.Commit();
            item._name = ChangedName;
            _client1.Store(holder, 3);
            _client1.Refresh(holder, 3);
            Assert.AreEqual(ChangedName, item._name);
        }

        public virtual void TestStoredFieldIsolation()
        {
            var storedItem = StoreItemToClient1AndCommit
                ();
            storedItem._name = ChangedName;
            _client1.Store(storedItem);
            var retrievedItem = RetrieveItemFromClient2
                ();
            var storedClass = _client2.StoredClass(typeof (Item
                ));
            var storedField = storedClass.StoredField(FieldName, null);
            var retrievedName = storedField.Get(retrievedItem);
            Assert.AreEqual(OriginalName, retrievedName);
            _client1.Commit();
            retrievedName = storedField.Get(retrievedItem);
            Assert.AreEqual(ChangedName, retrievedName);
        }

        public virtual void TestStoredClasses()
        {
            StoreItemToClient1AndCommit();
            var storedClasses = _client1.StoredClasses();
            var storedClass = _client1.StoredClass(typeof (Item
                ));
            ArrayAssert.ContainsByEquality(storedClasses, new object[] {storedClass});
        }

        public virtual void TestSystemInfo()
        {
            var systemInfo = _client1.SystemInfo();
            Assert.IsNotNull(systemInfo);
            Assert.IsGreater(1, systemInfo.TotalSize());
        }

        public virtual void TestVersion()
        {
            StoreItemToClient1AndCommit();
            Assert.IsGreater(1, _client1.Version());
        }

        private void AssertItemCount(IExtObjectContainer client, int count)
        {
            var query = client.Query();
            query.Constrain(typeof (Item));
            var result = query.Execute();
            Assert.AreEqual(count, result.Count);
        }

        protected virtual Item StoreItemToClient1AndCommit
            ()
        {
            var storedItem = new Item
                (OriginalName);
            _client1.Store(storedItem);
            _client1.Commit();
            return storedItem;
        }

        private Item RetrieveItemFromClient2()
        {
            var query = _client2.Query();
            query.Constrain(typeof (Item));
            var objectSet = query.Execute();
            var retrievedItem = (Item
                ) objectSet.Next();
            return retrievedItem;
        }

        /// <exception cref="System.Exception"></exception>
        public override void SetUp()
        {
            var config = NewConfiguration();
            config.Common.ObjectClass(typeof (Item)).GenerateUUIDs
                (true);
            _server = (LocalObjectContainer) Db4oEmbedded.OpenFile(config, TempFile());
            _client1 = _server.OpenSession().Ext();
            _client2 = _server.OpenSession().Ext();
        }

        /// <exception cref="System.Exception"></exception>
        public override void TearDown()
        {
            _client1.Close();
            _client2.Close();
            _server.Close();
            base.TearDown();
        }

        public class ItemHolder
        {
            public Item _item;

            public ItemHolder(Item item)
            {
                _item = item;
            }
        }

        public class Item
        {
            public string _name;

            public Item()
            {
            }

            public Item(string name)
            {
                _name = name;
            }
        }

        private sealed class _ICodeBlock_84 : ICodeBlock
        {
            private readonly EmbeddedClientObjectContainerTestCase _enclosing;

            public _ICodeBlock_84(EmbeddedClientObjectContainerTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing._client1.Backup(string.Empty);
            }
        }
    }
}