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
using Db4objects.Db4o.Events;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Reflect;
using Db4objects.Db4o.Reflect.Core;

namespace Db4objects.Db4o.Constraints
{
    /// <summary>Configures a field of a class to allow unique values only.</summary>
    /// <remarks>
    ///     Configures a field of a class to allow unique values only. In C/S mode, this configuration
    ///     should be set on the server side only.
    /// </remarks>
    public class UniqueFieldValueConstraint : IConfigurationItem
    {
        protected readonly object _clazz;
        protected readonly string _fieldName;

        /// <summary>constructor to create a UniqueFieldValueConstraint.</summary>
        /// <remarks>constructor to create a UniqueFieldValueConstraint.</remarks>
        /// <param name="clazz">
        ///     can be a class (Java) / Type (.NET) / instance of the class / fully qualified class name
        /// </param>
        /// <param name="fieldName">the name of the field that is to be unique.</param>
        public UniqueFieldValueConstraint(object clazz, string fieldName)
        {
            _clazz = clazz;
            _fieldName = fieldName;
        }

        public virtual void Prepare(IConfiguration configuration)
        {
        }

        // Nothing to do...
        /// <summary>internal method, public for implementation reasons.</summary>
        /// <remarks>internal method, public for implementation reasons.</remarks>
        public virtual void Apply(IInternalObjectContainer objectContainer)
        {
            if (objectContainer.IsClient)
            {
                throw new InvalidOperationException(GetType().FullName + " should be configured on the server."
                    );
            }
            EventRegistryFactory.ForObjectContainer(objectContainer).Committing +=
                new _IEventListener4_47(this, objectContainer).OnEvent;
        }

        private IReflectClass ReflectorFor(Transaction trans, object obj)
        {
            return trans.Container().Reflector().ForObject(obj);
        }

        private sealed class _IEventListener4_47
        {
            private readonly UniqueFieldValueConstraint _enclosing;
            private readonly IInternalObjectContainer objectContainer;
            private FieldMetadata _fieldMetaData;

            public _IEventListener4_47(UniqueFieldValueConstraint _enclosing, IInternalObjectContainer
                objectContainer)
            {
                this._enclosing = _enclosing;
                this.objectContainer = objectContainer;
            }

            private void EnsureSingleOccurence(Transaction trans, IObjectInfoCollection col)
            {
                var i = col.GetEnumerator();
                while (i.MoveNext())
                {
                    var objectInfo = (IObjectInfo) i.Current;
                    if (ReflectClass() != _enclosing.ReflectorFor(trans, objectInfo.GetObject
                        ()))
                    {
                        continue;
                    }
                    var obj = ObjectFor(trans, objectInfo);
                    var fieldValue = FieldMetadata().GetOn(trans, obj);
                    if (fieldValue == null)
                    {
                        continue;
                    }
                    var range = FieldMetadata().Search(trans, fieldValue);
                    if (range.Size() > 1)
                    {
                        throw new UniqueFieldValueConstraintViolationException(ClassMetadata().GetName
                            (), FieldMetadata().GetName());
                    }
                }
            }

            private bool IsClassMetadataAvailable()
            {
                return null != ClassMetadata();
            }

            private FieldMetadata FieldMetadata()
            {
                if (_fieldMetaData != null)
                {
                    return _fieldMetaData;
                }
                _fieldMetaData = ClassMetadata().FieldMetadataForName(_enclosing._fieldName
                    );
                return _fieldMetaData;
            }

            private ClassMetadata ClassMetadata()
            {
                return objectContainer.ClassMetadataForReflectClass(ReflectClass());
            }

            private IReflectClass ReflectClass()
            {
                return ReflectorUtils.ReflectClassFor(objectContainer.Reflector(), _enclosing
                    ._clazz);
            }

            public void OnEvent(object sender, CommitEventArgs args)
            {
                if (!IsClassMetadataAvailable())
                {
                    return;
                }
                var commitEventArgs = args;
                var trans = (Transaction) commitEventArgs.Transaction();
                EnsureSingleOccurence(trans, commitEventArgs.Added);
                EnsureSingleOccurence(trans, commitEventArgs.Updated);
            }

            private object ObjectFor(Transaction trans, IObjectInfo info)
            {
                var id = (int) info.GetInternalID();
                var @ref = HardObjectReference.PeekPersisted(trans, id, 1);
                return @ref._object;
            }
        }
    }
}