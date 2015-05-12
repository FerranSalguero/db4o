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

using System.Collections;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Delete;
using Db4objects.Db4o.Internal.Handlers;
using Db4objects.Db4o.Internal.Marshall;
using Db4objects.Db4o.Marshall;

namespace Db4objects.Db4o.Typehandlers
{
    /// <summary>TypeHandler for Collections.</summary>
    /// <remarks>
    ///     TypeHandler for Collections.
    ///     On the .NET side, usage is restricted to instances of IList.
    /// </remarks>
    public partial class CollectionTypeHandler : IReferenceTypeHandler, ICascadingTypeHandler
        , IVariableLengthTypeHandler, IQueryableTypeHandler
    {
        public void CascadeActivation(IActivationContext context)
        {
            var all = ((ICollection) context.TargetObject()).GetEnumerator();
            while (all.MoveNext())
            {
                context.CascadeActivationToChild(all.Current);
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

        public virtual bool DescendsIntoMembers()
        {
            return true;
        }

        public virtual void Write(IWriteContext context, object obj)
        {
            var collection = (ICollection) obj;
            var elementHandler = DetectElementTypeHandler(Container(context), collection
                );
            WriteElementClassMetadataId(context, elementHandler);
            WriteElementCount(context, collection);
            WriteElements(context, collection, elementHandler);
        }

        public virtual void Activate(IReferenceActivationContext context)
        {
            var collection = (ICollection) ((UnmarshallingContext) context).PersistentObject
                ();
            ClearCollection(collection);
            var elementHandler = ReadElementTypeHandler(context, context);
            var elementCount = context.ReadInt();
            for (var i = 0; i < elementCount; i++)
            {
                var element = context.ReadObject(elementHandler);
                AddToCollection(collection, element);
            }
        }

        /// <exception cref="Db4objects.Db4o.Ext.Db4oIOException"></exception>
        public virtual void Delete(IDeleteContext context)
        {
            if (!context.CascadeDelete())
            {
                return;
            }
            var handler = ReadElementTypeHandler(context, context);
            var elementCount = context.ReadInt();
            for (var i = elementCount; i > 0; i--)
            {
                handler.Delete(context);
            }
        }

        public virtual void Defragment(IDefragmentContext context)
        {
            var handler = ReadElementTypeHandler(context, context);
            var elementCount = context.ReadInt();
            for (var i = 0; i < elementCount; i++)
            {
                handler.Defragment(context);
            }
        }

        public virtual IPreparedComparison PrepareComparison(IContext context, object obj
            )
        {
            // TODO Auto-generated method stub
            return null;
        }

        private void WriteElementCount(IWriteContext context, ICollection collection)
        {
            context.WriteInt(collection.Count);
        }

        private void WriteElements(IWriteContext context, ICollection collection, ITypeHandler4
            elementHandler)
        {
            var elements = collection.GetEnumerator();
            while (elements.MoveNext())
            {
                context.WriteObject(elementHandler, elements.Current);
            }
        }

        private ObjectContainerBase Container(IContext context)
        {
            return ((IInternalObjectContainer) context.ObjectContainer()).Container;
        }

        private void WriteElementClassMetadataId(IWriteContext context, ITypeHandler4 elementHandler
            )
        {
            context.WriteInt(0);
        }

        private ITypeHandler4 ReadElementTypeHandler(IReadBuffer buffer, IContext context
            )
        {
            buffer.ReadInt();
            return Container(context).Handlers.OpenTypeHandler();
        }

        private ITypeHandler4 DetectElementTypeHandler(IInternalObjectContainer container
            , ICollection collection)
        {
            return container.Handlers.OpenTypeHandler();
        }
    }
}