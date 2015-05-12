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

using System.Collections;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Reflect;
using Db4objects.Db4o.Reflect.Generic;

namespace Db4objects.Db4o.CS.Internal
{
    public class ClassInfoHelper
    {
        private readonly Hashtable4 _classMetaTable = new Hashtable4();
        private readonly Config4Impl _config;
        private readonly Hashtable4 _genericClassTable = new Hashtable4();

        public ClassInfoHelper(Config4Impl config)
        {
            _config = config;
        }

        public virtual ClassInfo GetClassMeta(IReflectClass claxx)
        {
            if (IsObjectClass(claxx))
            {
                return ClassInfo.NewSystemClass(claxx.GetName());
            }
            var existing = LookupClassMeta(claxx.GetName());
            if (existing != null)
            {
                return existing;
            }
            return NewUserClassMeta(claxx);
        }

        private ClassInfo NewUserClassMeta(IReflectClass claxx)
        {
            var classMeta = ClassInfo.NewUserClass(claxx.GetName());
            classMeta.SetSuperClass(MapSuperclass(claxx));
            RegisterClassMeta(claxx.GetName(), classMeta);
            classMeta.SetFields(MapFields(claxx.GetDeclaredFields(), ShouldStoreTransientFields
                (claxx)));
            return classMeta;
        }

        private bool ShouldStoreTransientFields(IReflectClass claxx)
        {
            var configClass = _config.ConfigClass(claxx.GetName());
            return configClass == null ? false : configClass.StoreTransientFields();
        }

        private ClassInfo MapSuperclass(IReflectClass claxx)
        {
            var superClass = claxx.GetSuperclass();
            if (superClass != null)
            {
                return GetClassMeta(superClass);
            }
            return null;
        }

        private FieldInfo[] MapFields(IReflectField[] fields, bool shouldStoreTransientFields
            )
        {
            if (!shouldStoreTransientFields)
            {
                fields = FilterTransientFields(fields);
            }
            var fieldsMeta = new FieldInfo[fields.Length];
            for (var i = 0; i < fields.Length; ++i)
            {
                var field = fields[i];
                var isArray = field.GetFieldType().IsArray();
                var fieldClass = isArray
                    ? field.GetFieldType().GetComponentType()
                    : field
                        .GetFieldType();
                var isPrimitive = fieldClass.IsPrimitive();
                // TODO: need to handle NArray, currently it ignores NArray and alway sets NArray flag false.
                fieldsMeta[i] = new FieldInfo(field.GetName(), GetClassMeta(fieldClass), isPrimitive
                    , isArray, false);
            }
            return fieldsMeta;
        }

        private IReflectField[] FilterTransientFields(IReflectField[] fields)
        {
            IList filteredFields = new ArrayList();
            for (var fieldIndex = 0; fieldIndex < fields.Length; ++fieldIndex)
            {
                var field = fields[fieldIndex];
                if (!field.IsTransient())
                {
                    filteredFields.Add(field);
                }
            }
            return Sharpen.Collections.ToArray(filteredFields, new IReflectField
                [filteredFields.Count]);
        }

        private static bool IsObjectClass(IReflectClass claxx)
        {
            // TODO: We should send the whole class meta if we'd like to support
            // java and .net communication (We have this request in our user forum
            // http://developer.db4o.com/forums/thread/31504.aspx). If we only want
            // to support java & .net platform separately, then this method should
            // be moved to Platform4.
            //return className.startsWith("java.lang.Object") || className.startsWith("System.Object");
            return claxx.Reflector().ForClass(Const4.ClassObject) == claxx;
        }

        private ClassInfo LookupClassMeta(string className)
        {
            return (ClassInfo) _classMetaTable.Get(className);
        }

        private void RegisterClassMeta(string className, ClassInfo classMeta)
        {
            _classMetaTable.Put(className, classMeta);
        }

        public virtual GenericClass ClassMetaToGenericClass(GenericReflector reflector, ClassInfo
            classMeta)
        {
            if (classMeta.IsSystemClass())
            {
                return (GenericClass) reflector.ForName(classMeta.GetClassName());
            }
            var className = classMeta.GetClassName();
            // look up from generic class table.
            var genericClass = LookupGenericClass(className);
            if (genericClass != null)
            {
                return genericClass;
            }
            var reflectClass = reflector.ForName(className);
            if (reflectClass != null)
            {
                return (GenericClass) reflectClass;
            }
            GenericClass genericSuperClass = null;
            var superClassMeta = classMeta.GetSuperClass();
            if (superClassMeta != null)
            {
                genericSuperClass = ClassMetaToGenericClass(reflector, superClassMeta);
            }
            genericClass = new GenericClass(reflector, null, className, genericSuperClass);
            RegisterGenericClass(className, genericClass);
            var fields = classMeta.GetFields();
            var genericFields = new GenericField[fields.Length];
            for (var i = 0; i < fields.Length; ++i)
            {
                var fieldClassMeta = fields[i].GetFieldClass();
                var fieldName = fields[i].GetFieldName();
                var genericFieldClass = ClassMetaToGenericClass(reflector, fieldClassMeta
                    );
                genericFields[i] = new GenericField(fieldName, genericFieldClass, fields[i]._isPrimitive
                    );
            }
            genericClass.InitFields(genericFields);
            return genericClass;
        }

        private GenericClass LookupGenericClass(string className)
        {
            return (GenericClass) _genericClassTable.Get(className);
        }

        private void RegisterGenericClass(string className, GenericClass classMeta)
        {
            _genericClassTable.Put(className, classMeta);
            ((GenericReflector) classMeta.Reflector()).Register(classMeta);
        }
    }
}