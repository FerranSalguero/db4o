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
using Db4objects.Db4o.Internal;
using Sharpen;

namespace Db4objects.Db4o.Reflect.Self
{
    public class SelfClass : IReflectClass
    {
        private static readonly SelfField[] EmptyFields = new SelfField[0];
        private readonly Type _class;
        private readonly IReflector _parentReflector;
        private readonly SelfReflectionRegistry _registry;
        private SelfField[] _fields;
        private bool _isAbstract;
        private Type _superClass;

        public SelfClass(IReflector parentReflector, SelfReflectionRegistry registry, Type
            clazz)
        {
            // public SelfClass() {
            // super();
            // }
            _parentReflector = parentReflector;
            _registry = registry;
            _class = clazz;
        }

        public virtual IReflector Reflector()
        {
            return _parentReflector;
        }

        public virtual IReflectClass GetComponentType()
        {
            if (!IsArray())
            {
                return null;
            }
            return _parentReflector.ForClass(_registry.ComponentType(_class));
        }

        public virtual IReflectField[] GetDeclaredFields()
        {
            EnsureClassInfoLoaded();
            return _fields;
        }

        public virtual IReflectField GetDeclaredField(string name)
        {
            EnsureClassInfoLoaded();
            for (var idx = 0; idx < _fields.Length; idx++)
            {
                if (_fields[idx].GetName().Equals(name))
                {
                    return _fields[idx];
                }
            }
            return null;
        }

        public virtual IReflectClass GetDelegate()
        {
            return this;
        }

        public virtual IReflectMethod GetMethod(string methodName, IReflectClass[] paramClasses
            )
        {
            // TODO !!!!
            return null;
        }

        public virtual string GetName()
        {
            return _class.FullName;
        }

        public virtual IReflectClass GetSuperclass()
        {
            EnsureClassInfoLoaded();
            if (_superClass == null)
            {
                return null;
            }
            return _parentReflector.ForClass(_superClass);
        }

        public virtual bool IsAbstract()
        {
            EnsureClassInfoLoaded();
            return _isAbstract || IsInterface();
        }

        public virtual bool IsArray()
        {
            return _class.IsArray;
        }

        public virtual bool IsAssignableFrom(IReflectClass type)
        {
            if (!(type is SelfClass))
            {
                return false;
            }
            return _class.IsAssignableFrom(((SelfClass) type).GetJavaClass
                ());
        }

        public virtual bool IsCollection()
        {
            return _parentReflector.IsCollection(this);
        }

        public virtual bool IsInstance(object obj)
        {
            return _class.IsInstanceOfType(obj);
        }

        public virtual bool IsInterface()
        {
            return _class.IsInterface;
        }

        public virtual bool IsPrimitive()
        {
            return _registry.IsPrimitive(_class);
        }

        public virtual object NewInstance()
        {
            try
            {
                return Activator.CreateInstance(_class);
            }
            catch (Exception e)
            {
                Runtime.PrintStackTrace(e);
            }
            // Specialized exceptions break conversion to .NET
            //           
            //        
            //            
            // } catch (InstantiationException e) {
            // e.printStackTrace();
            // } catch (IllegalAccessException e) {
            // e.printStackTrace();
            // }
            return null;
        }

        public virtual object NullValue()
        {
            return null;
        }

        public virtual bool EnsureCanBeInstantiated()
        {
            return true;
        }

        public virtual bool IsImmutable()
        {
            return IsPrimitive() || Platform4.IsSimple(_class);
        }

        // TODO: Is this needed at all?
        public virtual Type GetJavaClass()
        {
            return _class;
        }

        private void EnsureClassInfoLoaded()
        {
            if (_fields == null)
            {
                var classInfo = _registry.InfoFor(_class);
                if (classInfo == null)
                {
                    _fields = EmptyFields;
                    return;
                }
                _superClass = classInfo.SuperClass();
                _isAbstract = classInfo.IsAbstract();
                var fieldInfo = classInfo.FieldInfo();
                if (fieldInfo == null)
                {
                    _fields = EmptyFields;
                    return;
                }
                _fields = new SelfField[fieldInfo.Length];
                for (var idx = 0; idx < fieldInfo.Length; idx++)
                {
                    _fields[idx] = SelfFieldFor(fieldInfo[idx]);
                }
            }
        }

        private SelfField SelfFieldFor(FieldInfo fieldInfo)
        {
            return new SelfField(fieldInfo.Name(), _parentReflector.ForClass(fieldInfo.Type()
                ), this, _registry);
        }
    }
}