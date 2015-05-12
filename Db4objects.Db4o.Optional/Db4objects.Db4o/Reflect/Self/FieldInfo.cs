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

namespace Db4objects.Db4o.Reflect.Self
{
    public class FieldInfo
    {
        private readonly Type _clazz;
        private readonly bool _isPublic;
        private readonly bool _isStatic;
        private readonly bool _isTransient;
        private readonly string _name;

        public FieldInfo(string name, Type clazz, bool isPublic, bool isStatic, bool
            isTransient)
        {
            _name = name;
            _clazz = clazz;
            _isPublic = isPublic;
            _isStatic = isStatic;
            _isTransient = isTransient;
        }

        public virtual string Name()
        {
            return _name;
        }

        public virtual Type Type()
        {
            return _clazz;
        }

        public virtual bool IsPublic()
        {
            return _isPublic;
        }

        public virtual bool IsStatic()
        {
            return _isStatic;
        }

        public virtual bool IsTransient()
        {
            return _isTransient;
        }
    }
}