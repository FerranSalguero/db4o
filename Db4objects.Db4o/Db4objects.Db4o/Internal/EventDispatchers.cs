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
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Reflect;
using Sharpen;

namespace Db4objects.Db4o.Internal
{
    /// <exclude></exclude>
    public sealed class EventDispatchers
    {
        internal const int CanDelete = 0;
        internal const int Delete = 1;
        internal const int Activate = 2;
        internal const int Deactivate = 3;
        internal const int New = 4;
        public const int Update = 5;
        internal const int CanActivate = 6;
        internal const int CanDeactivate = 7;
        internal const int CanNew = 8;
        internal const int CanUpdate = 9;
        internal const int DeleteCount = 2;
        internal const int Count = 10;

        public static readonly IEventDispatcher NullDispatcher = new _IEventDispatcher_11
            ();

        private static readonly string[] events =
        {
            "objectCanDelete", "objectOnDelete"
            , "objectOnActivate", "objectOnDeactivate", "objectOnNew", "objectOnUpdate", "objectCanActivate"
            , "objectCanDeactivate", "objectCanNew", "objectCanUpdate"
        };

        public static IEventDispatcher ForClass(ObjectContainerBase container, IReflectClass
            classReflector)
        {
            if (container == null || classReflector == null)
            {
                throw new ArgumentNullException();
            }
            if (!container.DispatchsEvents())
            {
                return NullDispatcher;
            }
            var count = EventCountFor(container);
            if (count == 0)
            {
                return NullDispatcher;
            }
            var handlers = EventHandlerTableFor(container, classReflector);
            return HasEventHandler(handlers)
                ? new EventDispatcherImpl(handlers
                    )
                : NullDispatcher;
        }

        private static IReflectMethod[] EventHandlerTableFor(ObjectContainerBase container
            , IReflectClass classReflector)
        {
            IReflectClass[] parameterClasses =
            {
                container._handlers.IclassObjectcontainer
            };
            var methods = new IReflectMethod[Count];
            for (var i = Count - 1; i >= 0; i--)
            {
                var method = classReflector.GetMethod(events[i], parameterClasses);
                if (null == method)
                {
                    method = classReflector.GetMethod(ToPascalCase(events[i]), parameterClasses);
                }
                if (method != null)
                {
                    methods[i] = method;
                }
            }
            return methods;
        }

        private static bool HasEventHandler(IReflectMethod[] methods)
        {
            return Iterators.Any(Iterators.Iterate(methods), new _IPredicate4_118());
        }

        private static int EventCountFor(ObjectContainerBase container)
        {
            var callbackMode = container.ConfigImpl.CallbackMode();
            if (callbackMode == CallBackMode.All)
            {
                return Count;
            }
            if (callbackMode == CallBackMode.DeleteOnly)
            {
                return DeleteCount;
            }
            return 0;
        }

        private static string ToPascalCase(string name)
        {
            return Runtime.Substring(name, 0, 1).ToUpper() + Runtime.Substring
                (name, 1);
        }

        private sealed class _IEventDispatcher_11 : IEventDispatcher
        {
            public bool Dispatch(Transaction trans, object obj, int eventID)
            {
                return true;
            }

            public bool HasEventRegistered(int eventID)
            {
                return false;
            }
        }

        private class EventDispatcherImpl : IEventDispatcher
        {
            private readonly IReflectMethod[] methods;

            public EventDispatcherImpl(IReflectMethod[] methods_)
            {
                methods = methods_;
            }

            public virtual bool HasEventRegistered(int eventID)
            {
                return methods[eventID] != null;
            }

            public virtual bool Dispatch(Transaction trans, object obj, int eventID)
            {
                if (methods[eventID] == null)
                {
                    return true;
                }
                object[] parameters = {trans.ObjectContainer()};
                var container = trans.Container();
                var stackDepth = container.StackDepth();
                var topLevelCallId = container.TopLevelCallId();
                container.StackDepth(0);
                try
                {
                    var res = methods[eventID].Invoke(obj, parameters);
                    if (res is bool)
                    {
                        return ((bool) res);
                    }
                }
                finally
                {
                    container.StackDepth(stackDepth);
                    container.TopLevelCallId(topLevelCallId);
                }
                return true;
            }
        }

        private sealed class _IPredicate4_118 : IPredicate4
        {
            public bool Match(object candidate)
            {
                return candidate != null;
            }
        }
    }
}