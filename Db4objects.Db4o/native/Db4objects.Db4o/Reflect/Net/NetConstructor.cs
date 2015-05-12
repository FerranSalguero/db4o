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

using System.Reflection;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Reflect.Core;
using Sharpen;

namespace Db4objects.Db4o.Reflect.Net
{
    /// <remarks>Reflection implementation for Constructor to map to .NET reflection.</remarks>
    public class NetConstructor : IReflectConstructor
    {
        private readonly ConstructorInfo constructor;
        private readonly IReflector reflector;

        public NetConstructor(IReflector reflector, ConstructorInfo
            constructor)
        {
            this.reflector = reflector;
            this.constructor = constructor;
            Platform4.SetAccessible(constructor);
        }

        public virtual IReflectClass[] GetParameterTypes()
        {
            return NetReflector.ToMeta(reflector, Runtime.GetParameterTypes(constructor));
        }

        public virtual object NewInstance(object[] parameters)
        {
            try
            {
                return constructor.Invoke(parameters);
            }
            catch
            {
                return null;
            }
        }
    }
}