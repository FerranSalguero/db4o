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
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal.Activation;
using Db4objects.Db4o.Internal.Callbacks;
using Db4objects.Db4o.Internal.Events;
using Db4objects.Db4o.Internal.Query;
using Db4objects.Db4o.Reflect;

namespace Db4objects.Db4o.Internal
{
    /// <exclude></exclude>
    public partial interface IInternalObjectContainer : IExtObjectContainer
    {
        ObjectContainerBase Container { get; }

        Transaction Transaction { get; }

        HandlerRegistry Handlers { get; }

        Config4Impl ConfigImpl { get; }

        bool IsClient { get; }

        void Callbacks(ICallbacks cb);
        ICallbacks Callbacks();
        NativeQueryHandler GetNativeQueryHandler();
        ClassMetadata ClassMetadataForReflectClass(IReflectClass reflectClass);
        ClassMetadata ClassMetadataForName(string name);
        ClassMetadata ClassMetadataForID(int id);
        object SyncExec(IClosure4 block);

        int InstanceCount(ClassMetadata clazz, Transaction trans
            );

        void StoreAll(Transaction trans, IEnumerator objects);
        IUpdateDepthProvider UpdateDepthProvider();
        EventRegistryImpl NewEventRegistry();
    }
}