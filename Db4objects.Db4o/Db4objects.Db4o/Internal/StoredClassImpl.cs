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
using Db4objects.Db4o.Foundation;

namespace Db4objects.Db4o.Internal
{
    /// <exclude></exclude>
    public class StoredClassImpl : IStoredClass
    {
        private readonly ClassMetadata _classMetadata;
        private readonly Transaction _transaction;

        public StoredClassImpl(Transaction transaction, ClassMetadata classMetadata)
        {
            if (classMetadata == null)
            {
                throw new ArgumentException();
            }
            _transaction = transaction;
            _classMetadata = classMetadata;
        }

        public virtual long[] GetIDs()
        {
            return _classMetadata.GetIDs(_transaction);
        }

        public virtual string GetName()
        {
            return _classMetadata.GetName();
        }

        public virtual IStoredClass GetParentStoredClass()
        {
            var parentClassMetadata = _classMetadata.GetAncestor();
            if (parentClassMetadata == null)
            {
                return null;
            }
            return new StoredClassImpl(_transaction, parentClassMetadata
                );
        }

        public virtual IStoredField[] GetStoredFields()
        {
            var fieldMetadata = _classMetadata.GetStoredFields();
            var storedFields = new IStoredField[fieldMetadata.Length];
            for (var i = 0; i < fieldMetadata.Length; i++)
            {
                storedFields[i] = new StoredFieldImpl(_transaction, (FieldMetadata) fieldMetadata[
                    i]);
            }
            return storedFields;
        }

        public virtual bool HasClassIndex()
        {
            return _classMetadata.HasClassIndex();
        }

        public virtual void Rename(string newName)
        {
            var container = (IInternalObjectContainer) _transaction.ObjectContainer
                ();
            container.SyncExec(new _IClosure4_56(this, newName));
        }

        public virtual IStoredField StoredField(string name, object type)
        {
            var fieldMetadata = (FieldMetadata) _classMetadata.StoredField(name, type
                );
            if (fieldMetadata == null)
            {
                return null;
            }
            return new StoredFieldImpl(_transaction, fieldMetadata);
        }

        public virtual int InstanceCount()
        {
            return _classMetadata.InstanceCount(_transaction);
        }

        public override int GetHashCode()
        {
            return _classMetadata.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (GetType() != obj.GetType())
            {
                return false;
            }
            return _classMetadata.Equals(((StoredClassImpl) obj)._classMetadata
                );
        }

        public override string ToString()
        {
            return "StoredClass(" + _classMetadata + ")";
        }

        private sealed class _IClosure4_56 : IClosure4
        {
            private readonly StoredClassImpl _enclosing;
            private readonly string newName;

            public _IClosure4_56(StoredClassImpl _enclosing, string newName)
            {
                this._enclosing = _enclosing;
                this.newName = newName;
            }

            public object Run()
            {
                _enclosing._classMetadata.Rename(newName);
                return null;
            }
        }
    }
}