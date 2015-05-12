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
using Db4objects.Db4o.Events;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Internal.Callbacks;
using Db4objects.Db4o.Query;
using Sharpen.Lang;

namespace Db4objects.Db4o.Internal.Events
{
    /// <exclude></exclude>
    public class EventRegistryImpl : ICallbacks, IEventRegistry
    {
        protected EventHandler<ObjectInfoEventArgs> _activated;

        protected EventHandler<CancellableObjectEventArgs>
            _activating;

        protected EventHandler<ClassEventArgs> _classRegistered;
        protected EventHandler<ObjectContainerEventArgs> _closing;
        protected EventHandler<CommitEventArgs> _committed;
        protected EventHandler<CommitEventArgs> _committing;
        protected EventHandler<ObjectInfoEventArgs> _created;

        protected EventHandler<CancellableObjectEventArgs>
            _creating;

        protected EventHandler<ObjectInfoEventArgs> _deactivated;

        protected EventHandler<CancellableObjectEventArgs>
            _deactivating;

        protected EventHandler<ObjectInfoEventArgs> _deleted;

        protected EventHandler<CancellableObjectEventArgs>
            _deleting;

        protected EventHandler<ObjectInfoEventArgs> _instantiated;
        protected EventHandler<ObjectContainerEventArgs> _opened;
        protected EventHandler<QueryEventArgs> _queryFinished;
        protected EventHandler<QueryEventArgs> _queryStarted;
        protected EventHandler<ObjectInfoEventArgs> _updated;

        protected EventHandler<CancellableObjectEventArgs>
            _updating;

        // Callbacks implementation
        public virtual void QueryOnFinished(Transaction transaction, IQuery query)
        {
            if (!(_queryFinished != null))
            {
                return;
            }
            WithExceptionHandling(new _IRunnable_50(this, transaction, query));
        }

        public virtual void QueryOnStarted(Transaction transaction, IQuery query)
        {
            if (!(_queryStarted != null))
            {
                return;
            }
            WithExceptionHandling(new _IRunnable_59(this, transaction, query));
        }

        public virtual bool ObjectCanNew(Transaction transaction, object obj)
        {
            return TriggerCancellableObjectEventArgsInCallback(transaction, _creating, null,
                obj);
        }

        public virtual bool ObjectCanActivate(Transaction transaction, object obj)
        {
            return TriggerCancellableObjectEventArgsInCallback(transaction, _activating, null
                , obj);
        }

        public virtual bool ObjectCanUpdate(Transaction transaction, IObjectInfo objectInfo
            )
        {
            return TriggerCancellableObjectEventArgsInCallback(transaction, _updating, objectInfo
                , objectInfo.GetObject());
        }

        public virtual bool ObjectCanDelete(Transaction transaction, IObjectInfo objectInfo
            )
        {
            return TriggerCancellableObjectEventArgsInCallback(transaction, _deleting, objectInfo
                , objectInfo.GetObject());
        }

        public virtual bool ObjectCanDeactivate(Transaction transaction, IObjectInfo objectInfo
            )
        {
            return TriggerCancellableObjectEventArgsInCallback(transaction, _deactivating, objectInfo
                , objectInfo.GetObject());
        }

        public virtual void ObjectOnActivate(Transaction transaction, IObjectInfo obj)
        {
            TriggerObjectInfoEventInCallback(transaction, _activated, obj);
        }

        public virtual void ObjectOnNew(Transaction transaction, IObjectInfo obj)
        {
            TriggerObjectInfoEventInCallback(transaction, _created, obj);
        }

        public virtual void ObjectOnUpdate(Transaction transaction, IObjectInfo obj)
        {
            TriggerObjectInfoEventInCallback(transaction, _updated, obj);
        }

        public virtual void ObjectOnDelete(Transaction transaction, IObjectInfo obj)
        {
            TriggerObjectInfoEventInCallback(transaction, _deleted, obj);
        }

        public virtual void ClassOnRegistered(ClassMetadata clazz)
        {
            if (!(_classRegistered != null))
            {
                return;
            }
            WithExceptionHandling(new _IRunnable_104(this, clazz));
        }

        public virtual void ObjectOnDeactivate(Transaction transaction, IObjectInfo obj)
        {
            TriggerObjectInfoEventInCallback(transaction, _deactivated, obj);
        }

        public virtual void ObjectOnInstantiate(Transaction transaction, IObjectInfo obj)
        {
            TriggerObjectInfoEventInCallback(transaction, _instantiated, obj);
        }

        public virtual void CommitOnStarted(Transaction transaction, CallbackObjectInfoCollections
            objectInfoCollections)
        {
            if (!(_committing != null))
            {
                return;
            }
            WithExceptionHandlingInCallback(new _IRunnable_121(this, transaction, objectInfoCollections
                ));
        }

        public virtual void CommitOnCompleted(Transaction transaction, CallbackObjectInfoCollections
            objectInfoCollections, bool isOwnCommit)
        {
            if (!(_committed != null))
            {
                return;
            }
            WithExceptionHandlingInCallback(new _IRunnable_132(this, transaction, objectInfoCollections
                , isOwnCommit));
        }

        public virtual void CloseOnStarted(IObjectContainer container)
        {
            if (!(_closing != null))
            {
                return;
            }
            WithExceptionHandlingInCallback(new _IRunnable_143(this, container));
        }

        public virtual void OpenOnFinished(IObjectContainer container)
        {
            if (!(_opened != null))
            {
                return;
            }
            WithExceptionHandlingInCallback(new _IRunnable_154(this, container));
        }

        // do nothing 
        public virtual bool CaresAboutCommitting()
        {
            return (_committing != null);
        }

        public virtual bool CaresAboutCommitted()
        {
            return (_committed != null);
        }

        public virtual bool CaresAboutDeleting()
        {
            return (_deleting != null);
        }

        public virtual bool CaresAboutDeleted()
        {
            return (_deleted != null);
        }

        public virtual event EventHandler<QueryEventArgs> QueryFinished
        {
            add
            {
                _queryFinished = (EventHandler<QueryEventArgs>) Delegate.Combine
                    (_queryFinished, value);
            }
            remove
            {
                _queryFinished = (EventHandler<QueryEventArgs>) Delegate.Remove
                    (_queryFinished, value);
            }
        }

        public virtual event EventHandler<QueryEventArgs> QueryStarted
        {
            add
            {
                _queryStarted = (EventHandler<QueryEventArgs>) Delegate.Combine
                    (_queryStarted, value);
            }
            remove
            {
                _queryStarted = (EventHandler<QueryEventArgs>) Delegate.Remove
                    (_queryStarted, value);
            }
        }

        public virtual event EventHandler<CancellableObjectEventArgs>
            Creating
            {
                add
                {
                    _creating = (EventHandler<CancellableObjectEventArgs>
                        ) Delegate.Combine(_creating, value);
                }
                remove
                {
                    _creating = (EventHandler<CancellableObjectEventArgs>
                        ) Delegate.Remove(_creating, value);
                }
            }

        public virtual event EventHandler<CancellableObjectEventArgs>
            Activating
            {
                add
                {
                    _activating = (EventHandler<CancellableObjectEventArgs>
                        ) Delegate.Combine(_activating, value);
                }
                remove
                {
                    _activating = (EventHandler<CancellableObjectEventArgs>
                        ) Delegate.Remove(_activating, value);
                }
            }

        public virtual event EventHandler<CancellableObjectEventArgs>
            Updating
            {
                add
                {
                    _updating = (EventHandler<CancellableObjectEventArgs>
                        ) Delegate.Combine(_updating, value);
                }
                remove
                {
                    _updating = (EventHandler<CancellableObjectEventArgs>
                        ) Delegate.Remove(_updating, value);
                }
            }

        public virtual event EventHandler<CancellableObjectEventArgs>
            Deleting
            {
                add
                {
                    _deleting = (EventHandler<CancellableObjectEventArgs>
                        ) Delegate.Combine(_deleting, value);
                }
                remove
                {
                    _deleting = (EventHandler<CancellableObjectEventArgs>
                        ) Delegate.Remove(_deleting, value);
                }
            }

        public virtual event EventHandler<CancellableObjectEventArgs>
            Deactivating
            {
                add
                {
                    _deactivating = (EventHandler<CancellableObjectEventArgs>
                        ) Delegate.Combine(_deactivating, value);
                }
                remove
                {
                    _deactivating = (EventHandler<CancellableObjectEventArgs>
                        ) Delegate.Remove(_deactivating, value);
                }
            }

        public virtual event EventHandler<ObjectInfoEventArgs>
            Created
            {
                add
                {
                    _created = (EventHandler<ObjectInfoEventArgs>) Delegate.Combine
                        (_created, value);
                }
                remove
                {
                    _created = (EventHandler<ObjectInfoEventArgs>) Delegate.Remove
                        (_created, value);
                }
            }

        public virtual event EventHandler<ObjectInfoEventArgs>
            Activated
            {
                add
                {
                    _activated = (EventHandler<ObjectInfoEventArgs>) Delegate.Combine
                        (_activated, value);
                }
                remove
                {
                    _activated = (EventHandler<ObjectInfoEventArgs>) Delegate.Remove
                        (_activated, value);
                }
            }

        public virtual event EventHandler<ObjectInfoEventArgs>
            Updated
            {
                add
                {
                    _updated = (EventHandler<ObjectInfoEventArgs>) Delegate.Combine
                        (_updated, value);
                }
                remove
                {
                    _updated = (EventHandler<ObjectInfoEventArgs>) Delegate.Remove
                        (_updated, value);
                }
            }

        public virtual event EventHandler<ObjectInfoEventArgs>
            Deleted
            {
                add
                {
                    _deleted = (EventHandler<ObjectInfoEventArgs>) Delegate.Combine
                        (_deleted, value);
                }
                remove
                {
                    _deleted = (EventHandler<ObjectInfoEventArgs>) Delegate.Remove
                        (_deleted, value);
                }
            }

        public virtual event EventHandler<ObjectInfoEventArgs>
            Deactivated
            {
                add
                {
                    _deactivated = (EventHandler<ObjectInfoEventArgs>) Delegate.Combine
                        (_deactivated, value);
                }
                remove
                {
                    _deactivated = (EventHandler<ObjectInfoEventArgs>) Delegate.Remove
                        (_deactivated, value);
                }
            }

        public virtual event EventHandler<CommitEventArgs>
            Committing
            {
                add
                {
                    _committing = (EventHandler<CommitEventArgs>) Delegate.Combine
                        (_committing, value);
                }
                remove
                {
                    _committing = (EventHandler<CommitEventArgs>) Delegate.Remove
                        (_committing, value);
                }
            }

        public virtual event EventHandler<CommitEventArgs>
            Committed
            {
                add
                {
                    _committed = (EventHandler<CommitEventArgs>) Delegate.Combine
                        (_committed, value);
                    OnCommittedListenerAdded();
                }
                remove
                {
                    _committed = (EventHandler<CommitEventArgs>) Delegate.Remove
                        (_committed, value);
                }
            }

        public virtual event EventHandler<ClassEventArgs> ClassRegistered
        {
            add
            {
                _classRegistered = (EventHandler<ClassEventArgs>) Delegate.Combine
                    (_classRegistered, value);
            }
            remove
            {
                _classRegistered = (EventHandler<ClassEventArgs>) Delegate.Remove
                    (_classRegistered, value);
            }
        }

        public virtual event EventHandler<ObjectInfoEventArgs>
            Instantiated
            {
                add
                {
                    _instantiated = (EventHandler<ObjectInfoEventArgs>)
                        Delegate.Combine(_instantiated, value);
                }
                remove
                {
                    _instantiated = (EventHandler<ObjectInfoEventArgs>)
                        Delegate.Remove(_instantiated, value);
                }
            }

        public virtual event EventHandler<ObjectContainerEventArgs>
            Closing
            {
                add
                {
                    _closing = (EventHandler<ObjectContainerEventArgs>)
                        Delegate.Combine(_closing, value);
                }
                remove
                {
                    _closing = (EventHandler<ObjectContainerEventArgs>)
                        Delegate.Remove(_closing, value);
                }
            }

        public virtual event EventHandler<ObjectContainerEventArgs>
            Opened
            {
                add
                {
                    _opened = (EventHandler<ObjectContainerEventArgs>) Delegate.Combine
                        (_opened, value);
                }
                remove
                {
                    _opened = (EventHandler<ObjectContainerEventArgs>) Delegate.Remove
                        (_opened, value);
                }
            }

        protected virtual void OnCommittedListenerAdded()
        {
        }

        internal virtual bool TriggerCancellableObjectEventArgsInCallback(Transaction transaction
            , EventHandler<CancellableObjectEventArgs> e, IObjectInfo objectInfo, object
                o)
        {
            if (!(e != null))
            {
                return true;
            }
            var args = new CancellableObjectEventArgs(transaction, objectInfo
                , o);
            WithExceptionHandlingInCallback(new _IRunnable_258(e, args));
            return !args.IsCancelled;
        }

        internal virtual void TriggerObjectInfoEventInCallback(Transaction transaction, EventHandler<
            ObjectInfoEventArgs> e, IObjectInfo o)
        {
            if (!(e != null))
            {
                return;
            }
            WithExceptionHandlingInCallback(new _IRunnable_270(e, transaction, o));
        }

        private void WithExceptionHandlingInCallback(IRunnable runnable)
        {
            try
            {
                InCallback.Run(runnable);
            }
            catch (Db4oException e)
            {
                throw;
            }
            catch (Exception x)
            {
                throw new EventException(x);
            }
        }

        private void WithExceptionHandling(IRunnable runnable)
        {
            try
            {
                runnable.Run();
            }
            catch (Db4oException e)
            {
                throw;
            }
            catch (Exception x)
            {
                throw new EventException(x);
            }
        }

        private sealed class _IRunnable_50 : IRunnable
        {
            private readonly EventRegistryImpl _enclosing;
            private readonly IQuery query;
            private readonly Transaction transaction;

            public _IRunnable_50(EventRegistryImpl _enclosing, Transaction transaction, IQuery
                query)
            {
                this._enclosing = _enclosing;
                this.transaction = transaction;
                this.query = query;
            }

            public void Run()
            {
                if (null != _enclosing._queryFinished)
                    _enclosing._queryFinished(null,
                        new QueryEventArgs(transaction, query));
            }
        }

        private sealed class _IRunnable_59 : IRunnable
        {
            private readonly EventRegistryImpl _enclosing;
            private readonly IQuery query;
            private readonly Transaction transaction;

            public _IRunnable_59(EventRegistryImpl _enclosing, Transaction transaction, IQuery
                query)
            {
                this._enclosing = _enclosing;
                this.transaction = transaction;
                this.query = query;
            }

            public void Run()
            {
                if (null != _enclosing._queryStarted)
                    _enclosing._queryStarted(null, new
                        QueryEventArgs(transaction, query));
            }
        }

        private sealed class _IRunnable_104 : IRunnable
        {
            private readonly EventRegistryImpl _enclosing;
            private readonly ClassMetadata clazz;

            public _IRunnable_104(EventRegistryImpl _enclosing, ClassMetadata clazz)
            {
                this._enclosing = _enclosing;
                this.clazz = clazz;
            }

            public void Run()
            {
                if (null != _enclosing._classRegistered)
                    _enclosing._classRegistered(null,
                        new ClassEventArgs(clazz));
            }
        }

        private sealed class _IRunnable_121 : IRunnable
        {
            private readonly EventRegistryImpl _enclosing;
            private readonly CallbackObjectInfoCollections objectInfoCollections;
            private readonly Transaction transaction;

            public _IRunnable_121(EventRegistryImpl _enclosing, Transaction transaction, CallbackObjectInfoCollections
                objectInfoCollections)
            {
                this._enclosing = _enclosing;
                this.transaction = transaction;
                this.objectInfoCollections = objectInfoCollections;
            }

            public void Run()
            {
                if (null != _enclosing._committing)
                    _enclosing._committing(null, new CommitEventArgs
                        (transaction, objectInfoCollections, false));
            }
        }

        private sealed class _IRunnable_132 : IRunnable
        {
            private readonly EventRegistryImpl _enclosing;
            private readonly bool isOwnCommit;
            private readonly CallbackObjectInfoCollections objectInfoCollections;
            private readonly Transaction transaction;

            public _IRunnable_132(EventRegistryImpl _enclosing, Transaction transaction, CallbackObjectInfoCollections
                objectInfoCollections, bool isOwnCommit)
            {
                this._enclosing = _enclosing;
                this.transaction = transaction;
                this.objectInfoCollections = objectInfoCollections;
                this.isOwnCommit = isOwnCommit;
            }

            public void Run()
            {
                if (null != _enclosing._committed)
                    _enclosing._committed(null, new CommitEventArgs
                        (transaction, objectInfoCollections, isOwnCommit));
            }
        }

        private sealed class _IRunnable_143 : IRunnable
        {
            private readonly EventRegistryImpl _enclosing;
            private readonly IObjectContainer container;

            public _IRunnable_143(EventRegistryImpl _enclosing, IObjectContainer container)
            {
                this._enclosing = _enclosing;
                this.container = container;
            }

            public void Run()
            {
                if (null != _enclosing._closing)
                    _enclosing._closing(null, new ObjectContainerEventArgs
                        (container));
            }
        }

        private sealed class _IRunnable_154 : IRunnable
        {
            private readonly EventRegistryImpl _enclosing;
            private readonly IObjectContainer container;

            public _IRunnable_154(EventRegistryImpl _enclosing, IObjectContainer container)
            {
                this._enclosing = _enclosing;
                this.container = container;
            }

            public void Run()
            {
                if (null != _enclosing._opened)
                    _enclosing._opened(null, new ObjectContainerEventArgs
                        (container));
            }
        }

        private sealed class _IRunnable_258 : IRunnable
        {
            private readonly CancellableObjectEventArgs args;
            private readonly EventHandler<CancellableObjectEventArgs> e;

            public _IRunnable_258(EventHandler<CancellableObjectEventArgs> e, CancellableObjectEventArgs
                args)
            {
                this.e = e;
                this.args = args;
            }

            public void Run()
            {
                if (null != e) e(null, args);
            }
        }

        private sealed class _IRunnable_270 : IRunnable
        {
            private readonly EventHandler<ObjectInfoEventArgs> e;
            private readonly IObjectInfo o;
            private readonly Transaction transaction;

            public _IRunnable_270(EventHandler<ObjectInfoEventArgs> e, Transaction transaction
                , IObjectInfo o)
            {
                this.e = e;
                this.transaction = transaction;
                this.o = o;
            }

            public void Run()
            {
                if (null != e) e(null, new ObjectInfoEventArgs(transaction, o));
            }
        }
    }
}