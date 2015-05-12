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

namespace Db4objects.Db4o.Reflect.Core
{
    public class ConstructorSupport
    {
        public static ReflectConstructorSpec CreateConstructor(IConstructorAwareReflectClass
            claxx, Type clazz, IReflectorConfiguration config, IReflectConstructor[] constructors
            )
        {
            if (claxx == null)
            {
                return ReflectConstructorSpec.InvalidConstructor;
            }
            if (claxx.IsAbstract() || claxx.IsInterface())
            {
                return ReflectConstructorSpec.InvalidConstructor;
            }
            if (!Platform4.CallConstructor())
            {
                var skipConstructor = !config.CallConstructor(claxx);
                if (!claxx.IsCollection())
                {
                    var serializableConstructor = SkipConstructor(claxx, skipConstructor
                        , config.TestConstructors());
                    if (serializableConstructor != null)
                    {
                        return new ReflectConstructorSpec(serializableConstructor, null);
                    }
                }
            }
            if (!config.TestConstructors())
            {
                return new ReflectConstructorSpec(new PlatformReflectConstructor(clazz), null);
            }
            if (ReflectPlatform.CreateInstance(clazz) != null)
            {
                return new ReflectConstructorSpec(new PlatformReflectConstructor(clazz), null);
            }
            var sortedConstructors = SortConstructorsByParamsCount(constructors);
            return FindConstructor(claxx, sortedConstructors);
        }

        private static ReflectConstructorSpec FindConstructor(IReflectClass claxx, Tree sortedConstructors
            )
        {
            if (sortedConstructors == null)
            {
                return ReflectConstructorSpec.InvalidConstructor;
            }
            IEnumerator iter = new TreeNodeIterator(sortedConstructors);
            while (iter.MoveNext())
            {
                var current = iter.Current;
                var constructor = (IReflectConstructor) ((TreeIntObject) current)._object;
                var args = NullArgumentsFor(constructor);
                var res = constructor.NewInstance(args);
                if (res != null)
                {
                    return new ReflectConstructorSpec(constructor, args);
                }
            }
            return ReflectConstructorSpec.InvalidConstructor;
        }

        private static object[] NullArgumentsFor(IReflectConstructor constructor)
        {
            var paramTypes = constructor.GetParameterTypes();
            var @params = new object[paramTypes.Length];
            for (var j = 0; j < @params.Length; j++)
            {
                @params[j] = paramTypes[j].NullValue();
            }
            return @params;
        }

        private static Tree SortConstructorsByParamsCount(IReflectConstructor[] constructors
            )
        {
            Tree sortedConstructors = null;
            // sort constructors by parameter count
            for (var i = 0; i < constructors.Length; i++)
            {
                var parameterCount = constructors[i].GetParameterTypes().Length;
                sortedConstructors = Tree.Add(sortedConstructors, new TreeIntObject(i + constructors
                    .Length*parameterCount, constructors[i]));
            }
            return sortedConstructors;
        }

        public static IReflectConstructor SkipConstructor(IConstructorAwareReflectClass claxx
            , bool skipConstructor, bool testConstructor)
        {
            if (!skipConstructor)
            {
                return null;
            }
            var serializableConstructor = claxx.GetSerializableConstructor();
            if (serializableConstructor == null)
            {
                return null;
            }
            if (!testConstructor || Deploy.csharp)
            {
                return serializableConstructor;
            }
            var obj = serializableConstructor.NewInstance(null);
            if (obj != null)
            {
                return serializableConstructor;
            }
            return null;
        }
    }
}