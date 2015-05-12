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
using Db4objects.Db4o.Internal;
using Sharpen.Lang;

namespace Db4objects.Db4o.Foundation
{
    public partial class Environments
    {
        private static readonly DynamicVariable _current = DynamicVariable.NewInstance();

        public static object My(Type service)
        {
            var environment = Current();
            if (null == environment)
            {
                throw new InvalidOperationException();
            }
            return environment.Provide(service);
        }

        private static IEnvironment Current()
        {
            return ((IEnvironment) _current.Value);
        }

        public static void RunWith(IEnvironment environment, IRunnable runnable)
        {
            _current.With(environment, runnable);
        }

        public static IEnvironment NewClosedEnvironment(object[] bindings)
        {
            return new _IEnvironment_32(bindings);
        }

        public static IEnvironment NewCachingEnvironmentFor(IEnvironment environment)
        {
            return new _IEnvironment_48(environment);
        }

        public static IEnvironment NewConventionBasedEnvironment(object[] bindings)
        {
            return NewCachingEnvironmentFor(Compose(new[]
            {
                NewClosedEnvironment
                    (bindings),
                new ConventionBasedEnvironment()
            }));
        }

        public static IEnvironment NewConventionBasedEnvironment()
        {
            return NewCachingEnvironmentFor(new ConventionBasedEnvironment());
        }

        public static IEnvironment Compose(IEnvironment[] environments)
        {
            return new _IEnvironment_75(environments);
        }

        private sealed class _IEnvironment_32 : IEnvironment
        {
            private readonly object[] bindings;

            public _IEnvironment_32(object[] bindings)
            {
                this.bindings = bindings;
            }

            public object Provide(Type service)
            {
                for (var bindingIndex = 0; bindingIndex < bindings.Length; ++bindingIndex)
                {
                    var binding = bindings[bindingIndex];
                    if (service.IsInstanceOfType(binding))
                    {
                        return binding;
                    }
                }
                return null;
            }
        }

        private sealed class _IEnvironment_48 : IEnvironment
        {
            private readonly IDictionary _bindings;
            private readonly IEnvironment environment;

            public _IEnvironment_48(IEnvironment environment)
            {
                this.environment = environment;
                _bindings = new Hashtable();
            }

            public object Provide(Type service)
            {
                var existing = _bindings[service];
                if (null != existing)
                {
                    return existing;
                }
                var binding = environment.Provide(service);
                if (null == binding)
                {
                    return null;
                }
                _bindings[service] = binding;
                return binding;
            }
        }

        private sealed class _IEnvironment_75 : IEnvironment
        {
            private readonly IEnvironment[] environments;

            public _IEnvironment_75(IEnvironment[] environments)
            {
                this.environments = environments;
            }

            public object Provide(Type service)
            {
                for (var eIndex = 0; eIndex < environments.Length; ++eIndex)
                {
                    var e = environments[eIndex];
                    var binding = e.Provide(service);
                    if (null != binding)
                    {
                        return binding;
                    }
                }
                return null;
            }
        }

        private sealed class ConventionBasedEnvironment : IEnvironment
        {
            public object Provide(Type service)
            {
                return Resolve(service);
            }

            /// <summary>
            ///     Resolves a service interface to its default implementation using the
            ///     db4o namespace convention:
            ///     interface foo.bar.Baz
            ///     default implementation foo.internal.bar.BazImpl
            /// </summary>
            /// <returns>the convention based type name for the requested service</returns>
            private object Resolve(Type service)
            {
                var className = DefaultImplementationFor(service);
                var binding = ReflectPlatform.CreateInstance(className);
                if (null == binding)
                {
                    throw new ArgumentException("Cant find default implementation for " + service + ": " + className);
                }
                return binding;
            }
        }
    }
}