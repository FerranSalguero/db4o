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

namespace Db4objects.Db4o.CS.Internal
{
    public class ClassInfo
    {
        public string _className;
        public FieldInfo[] _fields;
        public bool _isSystemClass;
        public ClassInfo _superClass;

        public ClassInfo()
        {
        }

        private ClassInfo(string className, bool systemClass)
        {
            _className = className;
            _isSystemClass = systemClass;
        }

        public static ClassInfo NewSystemClass(string className
            )
        {
            return new ClassInfo(className, true);
        }

        public static ClassInfo NewUserClass(string className
            )
        {
            return new ClassInfo(className, false);
        }

        public virtual FieldInfo[] GetFields()
        {
            return _fields;
        }

        public virtual void SetFields(FieldInfo[] fields)
        {
            _fields = fields;
        }

        public virtual ClassInfo GetSuperClass()
        {
            return _superClass;
        }

        public virtual void SetSuperClass(ClassInfo superClass
            )
        {
            _superClass = superClass;
        }

        public virtual string GetClassName()
        {
            return _className;
        }

        public virtual bool IsSystemClass()
        {
            return _isSystemClass;
        }
    }
}