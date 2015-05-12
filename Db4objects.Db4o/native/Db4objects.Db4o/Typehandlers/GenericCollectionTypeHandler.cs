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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Db4objects.Db4o.Collections;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Foundation.Collections;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Delete;
using Db4objects.Db4o.Internal.Handlers;
using Db4objects.Db4o.Internal.Marshall;
using Db4objects.Db4o.Marshall;

namespace Db4objects.Db4o.Typehandlers
{
    public class GenericCollectionTypeHandler : IReferenceTypeHandler, ICascadingTypeHandler, IVariableLengthTypeHandler,
        IQueryableTypeHandler
    {
        private static readonly Type[] _supportedCollections =
        {
            typeof (List<>),
            typeof (LinkedList<>),
            typeof (Stack<>),
            typeof (Queue<>),
            typeof (Collection<>),
            typeof (ActivatableList<>),
#if NET_3_5 && ! CF
            typeof (HashSet<>)
#endif
        };

        public void CascadeActivation(IActivationContext context)
        {
            var collection = ((IEnumerable) context.TargetObject());

            // TODO: detect the element type
            // and return immediately when it's a primitive

            foreach (var item in collection)
            {
                context.CascadeActivationToChild(item);
            }
        }

        public virtual ITypeHandler4 ReadCandidateHandler(QueryingReadContext context)
        {
            return this;
        }

        public virtual void CollectIDs(QueryingReadContext context)
        {
            var elementHandler = ReadElementTypeHandler(context, context);
            var elementCount = context.ReadInt();
            for (var i = 0; i < elementCount; i++)
            {
                context.ReadId(elementHandler);
            }
        }

        public bool DescendsIntoMembers()
        {
            return true;
        }

        public virtual void Write(IWriteContext context, object obj)
        {
            var initializer = CollectionInitializer.For(obj);
            var enumerable = (IEnumerable) obj;
            var elementType = DetectElementTypeErasingNullables(Container(context), enumerable);
            WriteElementTypeHandlerId(context, elementType);
            WriteElementCount(context, initializer);
            WriteElements(context, enumerable, elementType.TypeHandler());
        }

        public virtual void Activate(IReferenceActivationContext context)
        {
            var collection = context.PersistentObject();
            var initializer = CollectionInitializer.For(collection);
            initializer.Clear();

            ReadElements(context, initializer, ReadElementTypeHandler(context, context));

            initializer.FinishAdding();
        }

        public virtual void Delete(IDeleteContext context)
        {
            if (!context.CascadeDelete()) return;

            var handler = ReadElementTypeHandler(context, context);
            var elementCount = context.ReadInt();
            for (var i = elementCount; i > 0; i--)
            {
                handler.Delete(context);
            }
        }

        public virtual void Defragment(IDefragmentContext context)
        {
            DefragmentElementHandlerId(context);
            var handler = ReadElementTypeHandler(context, context);
            var elementCount = context.ReadInt();
            for (var i = 0; i < elementCount; i++)
            {
                context.Defragment(handler);
            }
        }

        public virtual IPreparedComparison PrepareComparison(IContext context, object obj)
        {
            return null;
        }

        private static void DefragmentElementHandlerId(IDefragmentContext context)
        {
            var offset = context.Offset();
            context.CopyID();
            context.Seek(offset);
        }

        private static ITypeHandler4 OpenTypeHandlerFrom(IContext context)
        {
            return context.Transaction().Container().Handlers.OpenTypeHandler();
        }

        private static void ReadElements(IReadContext context, ICollectionInitializer initializer,
            ITypeHandler4 elementHandler)
        {
            var elementCount = context.ReadInt();
            for (var i = 0; i < elementCount; i++)
            {
                initializer.Add(context.ReadObject(elementHandler));
            }
        }

        private static void WriteElementTypeHandlerId(IWriteContext context, ClassMetadata type)
        {
            context.WriteInt(type.GetID());
        }

        private static void WriteElementCount(IWriteBuffer context, ICollectionInitializer initializer)
        {
            context.WriteInt(initializer.Count());
        }

        private static void WriteElements(IWriteContext context, IEnumerable enumerable, ITypeHandler4 elementHandler)
        {
            var elements = enumerable.GetEnumerator();
            while (elements.MoveNext())
            {
                context.WriteObject(elementHandler, elements.Current);
            }
        }

        private static ObjectContainerBase Container(IContext context)
        {
            return ((IInternalObjectContainer) context.ObjectContainer()).Container;
        }

        private static ITypeHandler4 ReadElementTypeHandler(IReadBuffer buffer, IContext context)
        {
            var elementTypeId = buffer.ReadInt();
            if (elementTypeId == 0) return OpenTypeHandlerFrom(context);

            var elementHandler = Container(context).TypeHandlerForClassMetadataID(elementTypeId);
            return elementHandler ?? OpenTypeHandlerFrom(context);
        }

        private static ClassMetadata DetectElementTypeErasingNullables(ObjectContainerBase container,
            IEnumerable collection)
        {
            var elementType = ElementTypeOf(collection);
            if (IsNullableInstance(elementType))
            {
                return container.ClassMetadataForReflectClass(container.Handlers.IclassObject);
            }
            return container.ProduceClassMetadata(container.Reflector().ForClass(elementType));
        }

        private static bool IsNullableInstance(Type elementType)
        {
            return elementType.IsGenericType && (elementType.GetGenericTypeDefinition() == typeof (Nullable<>));
        }

        private static Type ElementTypeOf(IEnumerable collection)
        {
            var genericCollectionType = GenericCollectionTypeFor(collection.GetType());
            return genericCollectionType.GetGenericArguments()[0];
        }

        private static Type GenericCollectionTypeFor(Type type)
        {
            if (type == null)
            {
                throw new InvalidOperationException();
            }

            if (IsGenericCollectionType(type))
            {
                return type;
            }

            return GenericCollectionTypeFor(type.BaseType);
        }

        private static bool IsGenericCollectionType(Type type)
        {
            return type.IsGenericType && Array.IndexOf(_supportedCollections, type.GetGenericTypeDefinition()) >= 0;
        }

        public void RegisterSupportedTypesWith(Action<Type> registrationAction)
        {
            foreach (var collectionType in _supportedCollections)
            {
                registrationAction(collectionType);
            }
        }
    }
}