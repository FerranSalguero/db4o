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
using Db4objects.Db4o.Typehandlers.Internal;

namespace Db4objects.Db4o.Typehandlers
{
    /// <summary>Typehandler for classes that implement IDictionary.</summary>
    /// <remarks>Typehandler for classes that implement IDictionary.</remarks>
    public class MapTypeHandler : IReferenceTypeHandler, ICascadingTypeHandler, IVariableLengthTypeHandler
    {
        public void CascadeActivation(IActivationContext context)
        {
            var map = (IDictionary) context.TargetObject();
            var keys = (map).Keys.GetEnumerator();
            while (keys.MoveNext())
            {
                var key = keys.Current;
                context.CascadeActivationToChild(key);
                context.CascadeActivationToChild(map[key]);
            }
        }

        public virtual ITypeHandler4 ReadCandidateHandler(QueryingReadContext context)
        {
            return this;
        }

        public virtual void CollectIDs(QueryingReadContext context)
        {
            var handlers = ReadKeyValueTypeHandlers(context, context);
            var elementCount = context.ReadInt();
            for (var i = 0; i < elementCount; i++)
            {
                context.ReadId(handlers._keyHandler);
                context.SkipId(handlers._valueHandler);
            }
        }

        public virtual void Write(IWriteContext context, object obj)
        {
            var map = (IDictionary) obj;
            var handlers = DetectKeyValueTypeHandlers(Container(context), map
                );
            WriteClassMetadataIds(context, handlers);
            WriteElementCount(context, map);
            WriteElements(context, map, handlers);
        }

        public virtual void Activate(IReferenceActivationContext context)
        {
            var unmarshallingContext = (UnmarshallingContext) context;
            var map = (IDictionary) unmarshallingContext.PersistentObject();
            map.Clear();
            var handlers = ReadKeyValueTypeHandlers(context, context);
            var elementCount = context.ReadInt();
            for (var i = 0; i < elementCount; i++)
            {
                var key = unmarshallingContext.ReadFullyActivatedObjectForKeys(handlers._keyHandler
                    );
                if (key == null && !unmarshallingContext.LastReferenceReadWasReallyNull())
                {
                    continue;
                }
                var value = context.ReadObject(handlers._valueHandler);
                map[key] = value;
            }
        }

        /// <exception cref="Db4objects.Db4o.Ext.Db4oIOException"></exception>
        public virtual void Delete(IDeleteContext context)
        {
            if (!context.CascadeDelete())
            {
                return;
            }
            var handlers = ReadKeyValueTypeHandlers(context, context);
            var elementCount = context.ReadInt();
            for (var i = elementCount; i > 0; i--)
            {
                handlers._keyHandler.Delete(context);
                handlers._valueHandler.Delete(context);
            }
        }

        public virtual void Defragment(IDefragmentContext context)
        {
            var handlers = ReadKeyValueTypeHandlers(context, context);
            var elementCount = context.ReadInt();
            for (var i = elementCount; i > 0; i--)
            {
                context.Defragment(handlers._keyHandler);
                context.Defragment(handlers._valueHandler);
            }
        }

        public virtual IPreparedComparison PrepareComparison(IContext context, object obj
            )
        {
            // TODO Auto-generated method stub
            return null;
        }

        private void WriteElementCount(IWriteContext context, IDictionary map)
        {
            context.WriteInt(map.Count);
        }

        private void WriteElements(IWriteContext context, IDictionary map, KeyValueHandlerPair
            handlers)
        {
            IEnumerator elements = map.GetEnumerator();
            while (elements.MoveNext())
            {
                var entry = (DictionaryEntry) elements.Current;
                context.WriteObject(handlers._keyHandler, entry.Key);
                context.WriteObject(handlers._valueHandler, entry.Value);
            }
        }

        private ObjectContainerBase Container(IContext context)
        {
            return ((IInternalObjectContainer) context.ObjectContainer()).Container;
        }

        private void WriteClassMetadataIds(IWriteContext context, KeyValueHandlerPair handlers
            )
        {
            context.WriteInt(0);
            context.WriteInt(0);
        }

        private KeyValueHandlerPair ReadKeyValueTypeHandlers(IReadBuffer buffer, IContext
            context)
        {
            buffer.ReadInt();
            buffer.ReadInt();
            var untypedHandler = Container(context).Handlers.OpenTypeHandler
                ();
            return new KeyValueHandlerPair(untypedHandler, untypedHandler);
        }

        private KeyValueHandlerPair DetectKeyValueTypeHandlers(IInternalObjectContainer container
            , IDictionary map)
        {
            var untypedHandler = container.Handlers.OpenTypeHandler(
                );
            return new KeyValueHandlerPair(untypedHandler, untypedHandler);
        }
    }
}