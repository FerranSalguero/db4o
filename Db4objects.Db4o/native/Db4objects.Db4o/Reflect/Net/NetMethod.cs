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

namespace Db4objects.Db4o.Reflect.Net
{
    public class NetMethod : IReflectMethod
    {
        private readonly IReflector _reflector;
        private readonly MethodInfo method;

        public NetMethod(IReflector reflector, MethodInfo method)
        {
            _reflector = reflector;
            this.method = method;
        }

        public IReflectClass GetReturnType()
        {
            return _reflector.ForClass(method.ReturnType);
        }

        public virtual object Invoke(object onObject, object[] parameters)
        {
            try
            {
                return method.Invoke(onObject, parameters);
            }
            catch (TargetInvocationException e)
            {
                throw new ReflectException(e.InnerException);
            }
#if CF
            catch (System.Exception e)
			{
                throw new Db4objects.Db4o.Internal.ReflectException(e);
            }
#endif
        }
    }
}