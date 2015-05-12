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
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal.Metadata;
using Db4objects.Db4o.Reflect;

namespace Db4objects.Db4o.Internal
{
    /// <exclude></exclude>
    public sealed class ClassMetadataRepository : PersistentBase
    {
        private readonly PendingClassInits _classInits;
        private readonly IQueue4 _initClassMetadataOnUp;
        private readonly Transaction _systemTransaction;
        private Collection4 _classes;
        private Hashtable4 _classMetadataByBytes;
        private Hashtable4 _classMetadataByClass;
        private Hashtable4 _classMetadataByID;
        private Hashtable4 _classMetadataByName;
        private int _classMetadataCreationDepth;
        private Hashtable4 _creating;

        public ClassMetadataRepository(Transaction systemTransaction)
        {
            _systemTransaction = systemTransaction;
            _initClassMetadataOnUp = new NonblockingQueue();
            _classInits = new PendingClassInits(_systemTransaction);
        }

        public void AddClassMetadata(ClassMetadata clazz)
        {
            Container().SetDirtyInSystemTransaction(this);
            _classes.Add(clazz);
            if (clazz.StateUnread())
            {
                _classMetadataByBytes.Put(clazz.i_nameBytes, clazz);
            }
            else
            {
                _classMetadataByClass.Put(clazz.ClassReflector(), clazz);
            }
            RegisterClassMetadataById(clazz);
        }

        private void RegisterClassMetadataById(ClassMetadata clazz)
        {
            if (clazz.GetID() == 0)
            {
                clazz.Write(_systemTransaction);
            }
            _classMetadataByID.Put(clazz.GetID(), clazz);
        }

        private byte[] AsBytes(string str)
        {
            return Container().StringIO().Write(str);
        }

        public void AttachQueryNode(string fieldName, IVisitor4 visitor)
        {
            var i = Iterator();
            while (i.MoveNext())
            {
                var classMetadata = i.CurrentClass();
                if (!classMetadata.IsInternal())
                {
                    classMetadata.TraverseAllAspects(new _TraverseFieldCommand_65(fieldName, visitor,
                        classMetadata));
                }
            }
        }

        public void IterateTopLevelClasses(IVisitor4 visitor)
        {
            var i = Iterator();
            while (i.MoveNext())
            {
                var classMetadata = i.CurrentClass();
                if (!classMetadata.IsInternal())
                {
                    if (classMetadata.GetAncestor() == null)
                    {
                        visitor.Visit(classMetadata);
                    }
                }
            }
        }

        internal void CheckChanges()
        {
            var i = _classes.GetEnumerator();
            while (i.MoveNext())
            {
                ((ClassMetadata) i.Current).CheckChanges();
            }
        }

        internal bool CreateClassMetadata(ClassMetadata clazz, IReflectClass reflectClazz
            )
        {
            var result = false;
            _classMetadataCreationDepth++;
            try
            {
                var parentReflectClazz = reflectClazz.GetSuperclass();
                ClassMetadata parentClazz = null;
                if (parentReflectClazz != null && !parentReflectClazz.Equals(Container()._handlers
                    .IclassObject))
                {
                    parentClazz = ProduceClassMetadata(parentReflectClazz);
                }
                result = Container().CreateClassMetadata(clazz, reflectClazz, parentClazz);
            }
            finally
            {
                _classMetadataCreationDepth--;
            }
            InitClassMetadataOnUp();
            return result;
        }

        private void EnsureAllClassesRead()
        {
            var allClassesRead = false;
            while (!allClassesRead)
            {
                var unreadClasses = new Collection4();
                var numClasses = _classes.Size();
                var classIter = _classes.GetEnumerator();
                while (classIter.MoveNext())
                {
                    var clazz = (ClassMetadata) classIter.Current;
                    if (clazz.StateUnread())
                    {
                        unreadClasses.Add(clazz);
                    }
                }
                var unreadIter = unreadClasses.GetEnumerator();
                while (unreadIter.MoveNext())
                {
                    var clazz = (ClassMetadata) unreadIter.Current;
                    clazz = ReadClassMetadata(clazz, null);
                    if (clazz.ClassReflector() == null)
                    {
                        clazz.ForceRead();
                    }
                }
                allClassesRead = (_classes.Size() == numClasses);
            }
            ApplyReadAs();
        }

        internal bool FieldExists(string field)
        {
            var i = Iterator();
            while (i.MoveNext())
            {
                if (i.CurrentClass().FieldMetadataForName(field) != null)
                {
                    return true;
                }
            }
            return false;
        }

        public Collection4 ForInterface(IReflectClass claxx)
        {
            var col = new Collection4();
            var i = Iterator();
            while (i.MoveNext())
            {
                var clazz = i.CurrentClass();
                var candidate = clazz.ClassReflector();
                if (!candidate.IsInterface())
                {
                    if (claxx.IsAssignableFrom(candidate))
                    {
                        col.Add(clazz);
                        var j = new Collection4(col).GetEnumerator();
                        while (j.MoveNext())
                        {
                            var existing = (ClassMetadata) j.Current;
                            if (existing != clazz)
                            {
                                var higher = clazz.GetHigherHierarchy(existing);
                                if (higher != null)
                                {
                                    if (higher == clazz)
                                    {
                                        col.Remove(existing);
                                    }
                                    else
                                    {
                                        col.Remove(clazz);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return col;
        }

        public override byte GetIdentifier()
        {
            return Const4.Yapclasscollection;
        }

        internal ClassMetadata GetActiveClassMetadata(IReflectClass reflectClazz)
        {
            return (ClassMetadata) _classMetadataByClass.Get(reflectClazz);
        }

        internal ClassMetadata ClassMetadataForReflectClass(IReflectClass reflectClazz)
        {
            var cached = (ClassMetadata) _classMetadataByClass.Get(reflectClazz);
            if (cached != null)
            {
                return cached;
            }
            var byName = (ClassMetadata) _classMetadataByName.Get(reflectClazz.GetName
                ());
            if (byName != null)
            {
                return byName;
            }
            return ReadClassMetadata(reflectClazz);
        }

        private ClassMetadata ReadClassMetadata(IReflectClass reflectClazz)
        {
            var clazz = (ClassMetadata) _classMetadataByBytes.Remove(GetNameBytes(reflectClazz
                .GetName()));
            if (clazz == null)
            {
                return null;
            }
            return ReadClassMetadata(clazz, reflectClazz);
        }

        internal ClassMetadata ProduceClassMetadata(IReflectClass reflectClazz)
        {
            var classMetadata = ClassMetadataForReflectClass(reflectClazz);
            if (classMetadata != null)
            {
                return classMetadata;
            }
            var classBeingCreated = (ClassMetadata) _creating.Get(reflectClazz);
            if (classBeingCreated != null)
            {
                return classBeingCreated;
            }
            var newClassMetadata = new ClassMetadata(Container(), reflectClazz);
            _creating.Put(reflectClazz, newClassMetadata);
            try
            {
                if (!CreateClassMetadata(newClassMetadata, reflectClazz))
                {
                    return null;
                }
                // ObjectContainerBase#createClassMetadata may add the ClassMetadata already,
                // so we have to check again
                if (!IsRegistered(reflectClazz))
                {
                    AddClassMetadata(newClassMetadata);
                    _classInits.Process(newClassMetadata);
                }
                else
                {
                    RegisterClassMetadataById(newClassMetadata);
                    if (newClassMetadata.AspectsAreNull())
                    {
                        _classInits.Process(newClassMetadata);
                    }
                }
                Container().SetDirtyInSystemTransaction(this);
            }
            finally
            {
                _creating.Remove(reflectClazz);
            }
            return newClassMetadata;
        }

        private bool IsRegistered(IReflectClass reflectClazz)
        {
            return _classMetadataByClass.Get(reflectClazz) != null;
        }

        internal ClassMetadata ClassMetadataForId(int id)
        {
            var classMetadata = (ClassMetadata) _classMetadataByID.Get(id);
            if (null == classMetadata)
            {
                return null;
            }
            return ReadClassMetadata(classMetadata, null);
        }

        public int ClassMetadataIdForName(string name)
        {
            var classMetadata = (ClassMetadata) _classMetadataByBytes.Get(GetNameBytes
                (name));
            if (classMetadata == null)
            {
                classMetadata = FindInitializedClassByName(name);
            }
            if (classMetadata != null)
            {
                return classMetadata.GetID();
            }
            return 0;
        }

        public ClassMetadata GetClassMetadata(string name)
        {
            var classMetadata = (ClassMetadata) _classMetadataByBytes.Remove(GetNameBytes
                (name));
            if (classMetadata == null)
            {
                classMetadata = FindInitializedClassByName(name);
            }
            if (classMetadata != null)
            {
                classMetadata = ReadClassMetadata(classMetadata, null);
            }
            return classMetadata;
        }

        private ClassMetadata FindInitializedClassByName(string name)
        {
            var classMetadata = (ClassMetadata) _classMetadataByName.Get(name);
            if (classMetadata != null)
            {
                return classMetadata;
            }
            var i = Iterator();
            while (i.MoveNext())
            {
                classMetadata = (ClassMetadata) i.Current;
                if (name.Equals(classMetadata.GetName()))
                {
                    _classMetadataByName.Put(name, classMetadata);
                    return classMetadata;
                }
            }
            return null;
        }

        public int GetClassMetadataID(string name)
        {
            var clazz = (ClassMetadata) _classMetadataByBytes.Get(GetNameBytes(name)
                );
            if (clazz != null)
            {
                return clazz.GetID();
            }
            return 0;
        }

        internal byte[] GetNameBytes(string name)
        {
            return AsBytes(ResolveAliasRuntimeName(name));
        }

        private string ResolveAliasRuntimeName(string name)
        {
            return Container().ConfigImpl.ResolveAliasRuntimeName(name);
        }

        public void InitOnUp(Transaction systemTrans)
        {
            _classMetadataCreationDepth++;
            systemTrans.Container().ShowInternalClasses(true);
            try
            {
                var i = _classes.GetEnumerator();
                while (i.MoveNext())
                {
                    ((ClassMetadata) i.Current).InitOnUp(systemTrans);
                }
            }
            finally
            {
                systemTrans.Container().ShowInternalClasses(false);
                _classMetadataCreationDepth--;
            }
            InitClassMetadataOnUp();
        }

        internal void InitTables(int size)
        {
            _classes = new Collection4();
            _classMetadataByBytes = new Hashtable4(size);
            if (size < 16)
            {
                size = 16;
            }
            _classMetadataByClass = new Hashtable4(size);
            _classMetadataByName = new Hashtable4(size);
            _classMetadataByID = new Hashtable4(size);
            _creating = new Hashtable4(1);
        }

        private void InitClassMetadataOnUp()
        {
            if (_classMetadataCreationDepth != 0)
            {
                return;
            }
            var clazz = (ClassMetadata) _initClassMetadataOnUp.Next();
            while (clazz != null)
            {
                clazz.InitOnUp(_systemTransaction);
                clazz = (ClassMetadata) _initClassMetadataOnUp.Next();
            }
        }

        public ClassMetadataIterator Iterator()
        {
            return new ClassMetadataIterator(this, new ArrayIterator4(_classes.ToArray()));
        }

        public IEnumerator Ids()
        {
            return new ClassIDIterator(_classes);
        }

        public override int OwnLength()
        {
            return Const4.ObjectLength + Const4.IntLength + (_classes.Size()*Const4.IdLength
                );
        }

        internal void Purge()
        {
            var i = _classes.GetEnumerator();
            while (i.MoveNext())
            {
                ((ClassMetadata) i.Current).Purge();
            }
        }

        public override void ReadThis(Transaction trans, ByteArrayBuffer buffer)
        {
            var classCount = buffer.ReadInt();
            InitTables(classCount);
            var container = Container();
            var ids = ReadMetadataIds(buffer, classCount);
            var metadataSlots = container.ReadSlotBuffers(trans, ids);
            for (var i = 0; i < classCount; ++i)
            {
                var classMetadata = new ClassMetadata(container, null);
                classMetadata.SetID(ids[i]);
                _classes.Add(classMetadata);
                _classMetadataByID.Put(ids[i], classMetadata);
                var name = classMetadata.ReadName1(trans, metadataSlots[i]);
                if (name != null)
                {
                    _classMetadataByBytes.Put(name, classMetadata);
                }
            }
            ApplyReadAs();
        }

        private int[] ReadMetadataIds(ByteArrayBuffer buffer, int classCount)
        {
            var ids = new int[classCount];
            for (var i = 0; i < classCount; ++i)
            {
                ids[i] = buffer.ReadInt();
            }
            return ids;
        }

        internal Hashtable4 ClassByBytes()
        {
            return _classMetadataByBytes;
        }

        private void ApplyReadAs()
        {
            var readAs = Container().ConfigImpl.ReadAs();
            var i = readAs.Iterator();
            while (i.MoveNext())
            {
                var entry = (IEntry4) i.Current;
                var dbName = (string) entry.Key();
                var useName = (string) entry.Value();
                var dbbytes = GetNameBytes(dbName);
                var useBytes = GetNameBytes(useName);
                if (ClassByBytes().Get(useBytes) == null)
                {
                    var clazz = (ClassMetadata) ClassByBytes().Get(dbbytes);
                    if (clazz != null)
                    {
                        clazz.i_nameBytes = useBytes;
                        clazz.SetConfig(ConfigClass(dbName));
                        ClassByBytes().Remove(dbbytes);
                        ClassByBytes().Put(useBytes, clazz);
                    }
                }
            }
        }

        private Config4Class ConfigClass(string name)
        {
            return Container().ConfigImpl.ConfigClass(name);
        }

        public ClassMetadata ReadClassMetadata(ClassMetadata classMetadata, IReflectClass
            clazz)
        {
            if (classMetadata == null)
            {
                throw new ArgumentNullException();
            }
            if (!classMetadata.StateUnread())
            {
                return classMetadata;
            }
            _classMetadataCreationDepth++;
            try
            {
                classMetadata.ResolveNameConfigAndReflector(this, clazz);
                var claxx = classMetadata.ClassReflector();
                if (claxx != null)
                {
                    _classMetadataByClass.Put(claxx, classMetadata);
                    classMetadata.ReadThis();
                    classMetadata.CheckChanges();
                    _initClassMetadataOnUp.Add(classMetadata);
                }
            }
            finally
            {
                _classMetadataCreationDepth--;
            }
            InitClassMetadataOnUp();
            return classMetadata;
        }

        public void CheckAllClassChanges()
        {
            var i = _classMetadataByID.Keys();
            while (i.MoveNext())
            {
                var classMetadataID = ((int) i.Current);
                ClassMetadataForId(classMetadataID);
            }
        }

        public void RefreshClasses()
        {
            var rereader = new ClassMetadataRepository(_systemTransaction
                );
            rereader._id = _id;
            rereader.Read(Container().SystemTransaction());
            var i = rereader._classes.GetEnumerator();
            while (i.MoveNext())
            {
                var clazz = (ClassMetadata) i.Current;
                RefreshClass(clazz);
            }
            i = _classes.GetEnumerator();
            while (i.MoveNext())
            {
                var clazz = (ClassMetadata) i.Current;
                clazz.Refresh();
            }
        }

        private void RefreshClass(ClassMetadata clazz)
        {
            if (_classMetadataByID.Get(clazz.GetID()) == null)
            {
                _classes.Add(clazz);
                _classMetadataByID.Put(clazz.GetID(), clazz);
                RefreshClassCache(clazz, null);
            }
        }

        public void RefreshClassCache(ClassMetadata clazz, IReflectClass oldReflector)
        {
            if (clazz.StateUnread())
            {
                _classMetadataByBytes.Put(clazz.ReadName(_systemTransaction), clazz);
            }
            else
            {
                if (oldReflector != null)
                {
                    _classMetadataByClass.Remove(oldReflector);
                }
                _classMetadataByClass.Put(clazz.ClassReflector(), clazz);
            }
        }

        internal void ReReadClassMetadata(ClassMetadata clazz)
        {
            if (clazz != null)
            {
                ReReadClassMetadata(clazz._ancestor);
                clazz.ReadName(_systemTransaction);
                clazz.ForceRead();
                clazz.SetStateClean();
                clazz.BitFalse(Const4.CheckedChanges);
                clazz.BitFalse(Const4.Reading);
                clazz.BitFalse(Const4.Continue);
                clazz.BitFalse(Const4.Dead);
                clazz.CheckChanges();
            }
        }

        public IStoredClass[] StoredClasses()
        {
            EnsureAllClassesRead();
            var sclasses = new IStoredClass[_classes.Size()];
            _classes.ToArray(sclasses);
            return sclasses;
        }

        public void WriteAllClasses()
        {
            var deadClasses = new Collection4();
            var storedClasses = StoredClasses();
            for (var i = 0; i < storedClasses.Length; i++)
            {
                var clazz = (ClassMetadata) storedClasses[i];
                clazz.SetStateDirty();
                if (clazz.StateDead())
                {
                    deadClasses.Add(clazz);
                    clazz.SetStateOK();
                }
            }
            for (var i = 0; i < storedClasses.Length; i++)
            {
                var clazz = (ClassMetadata) storedClasses[i];
                clazz.Write(_systemTransaction);
            }
            var it = deadClasses.GetEnumerator();
            while (it.MoveNext())
            {
                ((ClassMetadata) it.Current).SetStateDead();
            }
        }

        public override void WriteThis(Transaction trans, ByteArrayBuffer buffer)
        {
            buffer.WriteInt(_classes.Size());
            var i = _classes.GetEnumerator();
            while (i.MoveNext())
            {
                buffer.WriteIDOf(trans, i.Current);
            }
        }

        public override string ToString()
        {
            var str = "Active:\n";
            var i = _classes.GetEnumerator();
            while (i.MoveNext())
            {
                var clazz = (ClassMetadata) i.Current;
                str += clazz.GetID() + " " + clazz + "\n";
            }
            return str;
        }

        internal ObjectContainerBase Container()
        {
            return _systemTransaction.Container();
        }

        public override void SetID(int id)
        {
            if (Container().IsClient)
            {
                base.SetID(id);
                return;
            }
            if (_id == 0)
            {
                SystemData().ClassCollectionID(id);
            }
            base.SetID(id);
        }

        private SystemData SystemData()
        {
            return LocalSystemTransaction().LocalContainer().SystemData();
        }

        private LocalTransaction LocalSystemTransaction()
        {
            return ((LocalTransaction) _systemTransaction);
        }

        public void ClassMetadataNameResolved(ClassMetadata classMetadata, byte[] nameBytes
            )
        {
            _classMetadataByBytes.Remove(nameBytes);
            _classMetadataByName.Put(classMetadata.GetName(), classMetadata);
        }

        private sealed class _TraverseFieldCommand_65 : TraverseFieldCommand
        {
            private readonly ClassMetadata classMetadata;
            private readonly string fieldName;
            private readonly IVisitor4 visitor;

            public _TraverseFieldCommand_65(string fieldName, IVisitor4 visitor, ClassMetadata
                classMetadata)
            {
                this.fieldName = fieldName;
                this.visitor = visitor;
                this.classMetadata = classMetadata;
            }

            protected override void Process(FieldMetadata field)
            {
                if (field.CanAddToQuery(fieldName))
                {
                    visitor.Visit(new object[] {classMetadata, field});
                }
            }
        }

        private class ClassIDIterator : MappingIterator
        {
            public ClassIDIterator(Collection4 classes) : base(classes.GetEnumerator())
            {
            }

            protected override object Map(object current)
            {
                return ((ClassMetadata) current).GetID();
            }
        }
    }
}