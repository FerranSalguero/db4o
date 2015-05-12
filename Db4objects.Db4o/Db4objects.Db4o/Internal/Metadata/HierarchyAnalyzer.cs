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
using Db4objects.Db4o.Reflect;

namespace Db4objects.Db4o.Internal.Metadata
{
    /// <exclude></exclude>
    public class HierarchyAnalyzer
    {
        private readonly IReflectClass _objectClass;
        private readonly IReflectClass _runtimeClass;
        private readonly ClassMetadata _storedClass;

        public HierarchyAnalyzer(ClassMetadata storedClass, IReflectClass runtimeClass)
        {
            if (storedClass == null || runtimeClass == null)
            {
                throw new ArgumentNullException();
            }
            _storedClass = storedClass;
            _runtimeClass = runtimeClass;
            _objectClass = runtimeClass.Reflector().ForClass(typeof (object));
        }

        public virtual IList Analyze()
        {
            IList ancestors = new ArrayList();
            var storedAncestor = _storedClass.GetAncestor();
            var runtimeAncestor = _runtimeClass.GetSuperclass();
            while (storedAncestor != null)
            {
                if (runtimeAncestor == storedAncestor.ClassReflector())
                {
                    ancestors.Add(new Same(storedAncestor));
                }
                else
                {
                    do
                    {
                        ancestors.Add(new Removed(storedAncestor));
                        storedAncestor = storedAncestor.GetAncestor();
                        if (null == storedAncestor)
                        {
                            if (IsObject(runtimeAncestor))
                            {
                                return ancestors;
                            }
                            ThrowUnsupportedAdd(runtimeAncestor);
                        }
                        if (runtimeAncestor == storedAncestor.ClassReflector())
                        {
                            ancestors.Add(new Same(storedAncestor));
                            break;
                        }
                    } while (storedAncestor != null);
                }
                storedAncestor = storedAncestor.GetAncestor();
                runtimeAncestor = runtimeAncestor.GetSuperclass();
            }
            if (runtimeAncestor != null && (!IsObject(runtimeAncestor)))
            {
                ThrowUnsupportedAdd(runtimeAncestor);
            }
            return ancestors;
        }

        private void ThrowUnsupportedAdd(IReflectClass runtimeAncestor)
        {
            throw new InvalidOperationException("Unsupported class hierarchy change. Class "
                                                + runtimeAncestor.GetName() + " was added to hierarchy of " +
                                                _runtimeClass.GetName
                                                    ());
        }

        private bool IsObject(IReflectClass clazz)
        {
            return _objectClass == clazz;
        }

        public class Diff
        {
            private readonly ClassMetadata _classMetadata;

            public Diff(ClassMetadata classMetadata)
            {
                if (classMetadata == null)
                {
                    throw new ArgumentNullException();
                }
                _classMetadata = classMetadata;
            }

            public override bool Equals(object obj)
            {
                if (GetType() != obj.GetType())
                {
                    return false;
                }
                var other = (Diff) obj;
                return _classMetadata == other._classMetadata;
            }

            public override string ToString()
            {
                return ReflectPlatform.SimpleName(GetType()) + "(" + _classMetadata.GetName() + ")";
            }

            public virtual ClassMetadata ClassMetadata()
            {
                return _classMetadata;
            }

            public virtual bool IsRemoved()
            {
                return false;
            }
        }

        public class Same : Diff
        {
            public Same(ClassMetadata classMetadata) : base(classMetadata)
            {
            }
        }

        public class Removed : Diff
        {
            public Removed(ClassMetadata classMetadata) : base(classMetadata)
            {
            }

            public override bool IsRemoved()
            {
                return true;
            }
        }
    }
}