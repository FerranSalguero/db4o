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
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Marshall;

namespace Db4objects.Db4o.Reflect.Generic
{
    /// <exclude></exclude>
    public class KnownClassesRepository
    {
        private static readonly Hashtable4 Primitives;
        private readonly IReflectClassBuilder _builder;
        private readonly Hashtable4 _classByID = new Hashtable4();
        private readonly Hashtable4 _classByName = new Hashtable4();
        private readonly Collection4 _classes = new Collection4();
        private readonly ListenerRegistry _listeners = ListenerRegistry.NewInstance();
        private Collection4 _pendingClasses = new Collection4();
        private ObjectContainerBase _stream;
        private Transaction _trans;

        static KnownClassesRepository()
        {
            Primitives = new Hashtable4();
            var primitiveArray = Platform4.PrimitiveTypes();
            for (var primitiveIndex = 0; primitiveIndex < primitiveArray.Length; ++primitiveIndex)
            {
                var primitive = primitiveArray[primitiveIndex];
                RegisterPrimitive(primitive);
            }
        }

        public KnownClassesRepository(IReflectClassBuilder builder)
        {
            _builder = builder;
        }

        private static void RegisterPrimitive(Type primitive)
        {
            Primitives.Put(ReflectPlatform.FullyQualifiedName(Platform4.NullableTypeFor(primitive
                )), primitive);
        }

        public virtual void SetTransaction(Transaction trans)
        {
            if (trans != null)
            {
                _trans = trans;
                _stream = trans.Container();
            }
        }

        public virtual void Register(IReflectClass clazz)
        {
            Register(clazz.GetName(), clazz);
        }

        public virtual IReflectClass ForID(int id)
        {
            lock (_stream.Lock())
            {
                if (_stream.Handlers.IsSystemHandler(id))
                {
                    return _stream.Handlers.ClassForID(id);
                }
                return EnsureClassAvailability(id);
            }
        }

        public virtual IReflectClass ForName(string className)
        {
            var clazz = LookupByName(className);
            if (clazz != null)
            {
                return clazz;
            }
            if (_stream == null)
            {
                return null;
            }
            lock (_stream.Lock())
            {
                if (_stream.ClassCollection() == null)
                {
                    return null;
                }
                var classID = _stream.ClassMetadataIdForName(className);
                if (classID <= 0)
                {
                    return null;
                }
                return InitializeClass(classID, className);
            }
        }

        private IReflectClass InitializeClass(int classID, string className)
        {
            var newClazz = EnsureClassInitialised(classID);
            _classByName.Put(className, newClazz);
            return newClazz;
        }

        private void ReadAll()
        {
            ForEachClassId(new _IProcedure4_102(this));
            ForEachClassId(new _IProcedure4_105(this));
        }

        private void ForEachClassId(IProcedure4 procedure)
        {
            for (var ids = _stream.ClassCollection().Ids(); ids.MoveNext();)
            {
                procedure.Apply((int) ids.Current);
            }
        }

        private IReflectClass EnsureClassAvailability(int id)
        {
            if (id == 0)
            {
                return null;
            }
            var ret = (IReflectClass) _classByID.Get(id);
            if (ret != null)
            {
                return ret;
            }
            ByteArrayBuffer classreader = _stream.ReadStatefulBufferById(_trans, id);
            var marshaller = MarshallerFamily()._class;
            var spec = marshaller.ReadSpec(_trans, classreader);
            var className = spec.Name();
            ret = LookupByName(className);
            if (ret != null)
            {
                _classByID.Put(id, ret);
                _pendingClasses.Add(id);
                return ret;
            }
            ReportMissingClass(className);
            ret = _builder.CreateClass(className, EnsureClassAvailability(spec.SuperClassID()
                ), spec.NumFields());
            // step 1 only add to _classByID, keep the class out of _classByName and _classes
            _classByID.Put(id, ret);
            _pendingClasses.Add(id);
            return ret;
        }

        private void ReportMissingClass(string className)
        {
            _stream.Handlers.DiagnosticProcessor().ClassMissed(className);
        }

        private void EnsureClassRead(int id)
        {
            var clazz = LookupByID(id);
            ByteArrayBuffer classreader = _stream.ReadStatefulBufferById(_trans, id);
            var classMarshaller = MarshallerFamily()._class;
            var classInfo = classMarshaller.ReadSpec(_trans, classreader);
            var className = classInfo.Name();
            // Having the class in the _classByName Map for now indicates
            // that the class is fully read. This is breakable if we start
            // returning GenericClass'es in other methods like forName
            // even if a native class has not been found
            if (LookupByName(className) != null)
            {
                return;
            }
            // step 2 add the class to _classByName and _classes to denote reading is completed
            Register(className, clazz);
            var numFields = classInfo.NumFields();
            var fields = _builder.FieldArray(numFields);
            var fieldMarshaller = MarshallerFamily()._field;
            for (var i = 0; i < numFields; i++)
            {
                var fieldInfo = fieldMarshaller.ReadSpec(_stream, classreader);
                var fieldName = fieldInfo.Name();
                var fieldClass = ReflectClassForFieldSpec(fieldInfo, _stream.Reflector(
                    ));
                if (null == fieldClass && (fieldInfo.IsField() && !fieldInfo.IsVirtual()))
                {
                    throw new InvalidOperationException("Could not read field type for '" + className
                                                        + "." + fieldName + "'");
                }
                fields[i] = _builder.CreateField(clazz, fieldName, fieldClass, fieldInfo.IsVirtual
                    (), fieldInfo.IsPrimitive(), fieldInfo.IsArray(), fieldInfo.IsNArray());
            }
            _builder.InitFields(clazz, fields);
        }

        private void Register(string className, IReflectClass clazz)
        {
            if (LookupByName(className) != null)
            {
                throw new ArgumentException();
            }
            _classByName.Put(className, clazz);
            _classes.Add(clazz);
            _listeners.NotifyListeners(clazz);
        }

        private IReflectClass ReflectClassForFieldSpec(RawFieldSpec fieldInfo, IReflector
            reflector)
        {
            if (fieldInfo.IsVirtualField())
            {
                return VirtualFieldByName(fieldInfo.Name()).ClassReflector(reflector);
            }
            var fieldTypeID = fieldInfo.FieldTypeID();
            switch (fieldTypeID)
            {
                case Handlers4.UntypedId:
                {
                    // need to take care of special handlers here
                    return ObjectClass();
                }

                case Handlers4.AnyArrayId:
                {
                    return ArrayClass(ObjectClass());
                }

                default:
                {
                    var fieldClass = ForID(fieldTypeID);
                    if (null != fieldClass)
                    {
                        return NormalizeFieldClass(fieldInfo, fieldClass);
                    }
                    break;
                }
            }
            return null;
        }

        private IReflectClass NormalizeFieldClass(RawFieldSpec fieldInfo, IReflectClass fieldClass
            )
        {
            // TODO: why the following line is necessary?
            var theClass = _stream.Reflector().ForName(fieldClass.GetName());
            if (fieldInfo.IsPrimitive())
            {
                theClass = PrimitiveClass(theClass);
            }
            if (fieldInfo.IsArray())
            {
                theClass = ArrayClass(theClass);
            }
            return theClass;
        }

        private IReflectClass ObjectClass()
        {
            return _stream.Reflector().ForClass(typeof (object));
        }

        private VirtualFieldMetadata VirtualFieldByName(string fieldName)
        {
            return _stream.Handlers.VirtualFieldByName(fieldName);
        }

        private MarshallerFamily MarshallerFamily()
        {
            return Internal.Marshall.MarshallerFamily.ForConverterVersion(_stream
                .ConverterVersion());
        }

        private IReflectClass EnsureClassInitialised(int id)
        {
            var ret = EnsureClassAvailability(id);
            while (_pendingClasses.Size() > 0)
            {
                var pending = _pendingClasses;
                _pendingClasses = new Collection4();
                var i = pending.GetEnumerator();
                while (i.MoveNext())
                {
                    EnsureClassRead(((int) i.Current));
                }
            }
            return ret;
        }

        public virtual IEnumerator Classes()
        {
            ReadAll();
            return _classes.GetEnumerator();
        }

        public virtual void Register(int id, IReflectClass clazz)
        {
            _classByID.Put(id, clazz);
        }

        public virtual IReflectClass LookupByID(int id)
        {
            return (IReflectClass) _classByID.Get(id);
        }

        public virtual IReflectClass LookupByName(string name)
        {
            return (IReflectClass) _classByName.Get(name);
        }

        private IReflectClass ArrayClass(IReflectClass clazz)
        {
            var proto = clazz.Reflector().Array().NewInstance(clazz, 0);
            return clazz.Reflector().ForObject(proto);
        }

        private IReflectClass PrimitiveClass(IReflectClass baseClass)
        {
            var primitive = (Type) Primitives.Get(baseClass.GetName());
            if (primitive != null)
            {
                return baseClass.Reflector().ForClass(primitive);
            }
            return baseClass;
        }

        public virtual void AddListener(IListener4 listener)
        {
            _listeners.Register(listener);
        }

        public virtual void RemoveListener(IListener4 listener)
        {
            _listeners.Remove(listener);
        }

        private sealed class _IProcedure4_102 : IProcedure4
        {
            private readonly KnownClassesRepository _enclosing;

            public _IProcedure4_102(KnownClassesRepository _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Apply(object id)
            {
                _enclosing.EnsureClassAvailability((((int) id)));
            }
        }

        private sealed class _IProcedure4_105 : IProcedure4
        {
            private readonly KnownClassesRepository _enclosing;

            public _IProcedure4_105(KnownClassesRepository _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Apply(object id)
            {
                _enclosing.EnsureClassRead((((int) id)));
            }
        }
    }
}