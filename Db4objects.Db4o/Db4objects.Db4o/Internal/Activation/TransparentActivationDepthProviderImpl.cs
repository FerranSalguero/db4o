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
using Db4objects.Db4o.Events;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.TA;

namespace Db4objects.Db4o.Internal.Activation
{
    public class TransparentActivationDepthProviderImpl : IActivationDepthProvider, ITransparentActivationDepthProvider
    {
        private readonly TransactionLocal _objectsModifiedInTransaction;
        private IRollbackStrategy _rollbackStrategy;
        public bool _transparentPersistenceIsEnabled;

        public TransparentActivationDepthProviderImpl()
        {
            _objectsModifiedInTransaction = new _TransactionLocal_73(this);
        }

        public virtual IActivationDepth ActivationDepth(int depth, ActivationMode mode)
        {
            if (int.MaxValue == depth)
            {
                return new FullActivationDepth(mode);
            }
            return new FixedActivationDepth(depth, mode);
        }

        public virtual IActivationDepth ActivationDepthFor(ClassMetadata classMetadata, ActivationMode
            mode)
        {
            if (IsTAAware(classMetadata))
            {
                return new NonDescendingActivationDepth(mode);
            }
            if (mode.IsPrefetch())
            {
                return new FixedActivationDepth(1, mode);
            }
            return new DescendingActivationDepth(this, mode);
        }

        public virtual void EnableTransparentPersistenceSupportFor(IInternalObjectContainer
            container, IRollbackStrategy rollbackStrategy)
        {
            FlushOnQueryStarted(container);
            _rollbackStrategy = rollbackStrategy;
            _transparentPersistenceIsEnabled = true;
        }

        public virtual void AddModified(object @object, Transaction transaction)
        {
            if (!_transparentPersistenceIsEnabled)
            {
                return;
            }
            ObjectsModifiedIn(transaction).Add(@object);
        }

        public virtual void RemoveModified(object @object, Transaction transaction)
        {
            if (!_transparentPersistenceIsEnabled)
            {
                return;
            }
            ObjectsModifiedIn(transaction).Remove(@object);
        }

        private bool IsTAAware(ClassMetadata classMetadata)
        {
            var reflector = classMetadata.Reflector();
            return reflector.ForClass(typeof (IActivatable)).IsAssignableFrom(classMetadata.ClassReflector
                ());
        }

        private void FlushOnQueryStarted(IInternalObjectContainer container)
        {
            var registry = EventRegistryFactory.ForObjectContainer(container);
            registry.QueryStarted += new _IEventListener4_46(this).OnEvent;
        }

        protected virtual Transaction TransactionFrom(EventArgs args)
        {
            return (Transaction) ((TransactionalEventArgs) args).Transaction();
        }

        private ObjectsModifiedInTransaction ObjectsModifiedIn
            (Transaction transaction)
        {
            return ((ObjectsModifiedInTransaction) transaction
                .Get(_objectsModifiedInTransaction).value);
        }

        private sealed class _IEventListener4_46
        {
            private readonly TransparentActivationDepthProviderImpl _enclosing;

            public _IEventListener4_46(TransparentActivationDepthProviderImpl _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void OnEvent(object sender, QueryEventArgs args)
            {
                _enclosing.ObjectsModifiedIn(_enclosing.TransactionFrom(args)).Flush();
            }
        }

        private sealed class _TransactionLocal_73 : TransactionLocal
        {
            private readonly TransparentActivationDepthProviderImpl _enclosing;

            public _TransactionLocal_73(TransparentActivationDepthProviderImpl _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public override object InitialValueFor(Transaction transaction)
            {
                var objectsModifiedInTransaction
                    = new ObjectsModifiedInTransaction(transaction
                        );
                transaction.AddTransactionListener(new _ITransactionListener_77(this, objectsModifiedInTransaction
                    ));
                return objectsModifiedInTransaction;
            }

            private sealed class _ITransactionListener_77 : ITransactionListener
            {
                private readonly _TransactionLocal_73 _enclosing;

                private readonly ObjectsModifiedInTransaction
                    objectsModifiedInTransaction;

                public _ITransactionListener_77(_TransactionLocal_73 _enclosing, ObjectsModifiedInTransaction
                    objectsModifiedInTransaction)
                {
                    this._enclosing = _enclosing;
                    this.objectsModifiedInTransaction = objectsModifiedInTransaction;
                }

                public void PostRollback()
                {
                    objectsModifiedInTransaction.Rollback(_enclosing._enclosing._rollbackStrategy
                        );
                }

                public void PreCommit()
                {
                    objectsModifiedInTransaction.Flush();
                }
            }
        }

        private sealed class ObjectsModifiedInTransaction
        {
            private readonly IdentitySet4 _modified = new IdentitySet4();
            private readonly IdentitySet4 _removedAfterModified = new IdentitySet4();
            private readonly Transaction _transaction;

            public ObjectsModifiedInTransaction(Transaction transaction)
            {
                _transaction = transaction;
            }

            public void Add(object @object)
            {
                if (Contains(@object))
                {
                    return;
                }
                _modified.Add(@object);
            }

            public void Remove(object @object)
            {
                if (!Contains(@object))
                {
                    return;
                }
                _modified.Remove(@object);
                _removedAfterModified.Add(@object);
            }

            private bool Contains(object @object)
            {
                return _modified.Contains(@object);
            }

            public void Flush()
            {
                StoreModifiedObjects();
                _modified.Clear();
            }

            private void StoreModifiedObjects()
            {
                var container = _transaction.Container();
                container.StoreAll(_transaction, _modified.ValuesIterator(), container.UpdateDepthProvider
                    ().Unspecified(new _IModifiedObjectQuery_132(this)));
                _transaction.ProcessDeletes();
            }

            public void Rollback(IRollbackStrategy rollbackStrategy)
            {
                ApplyRollbackStrategy(rollbackStrategy);
                _modified.Clear();
            }

            private void ApplyRollbackStrategy(IRollbackStrategy rollbackStrategy)
            {
                if (null == rollbackStrategy)
                {
                    return;
                }
                ApplyRollbackStrategy(rollbackStrategy, _modified.ValuesIterator());
                ApplyRollbackStrategy(rollbackStrategy, _removedAfterModified.ValuesIterator());
            }

            private void ApplyRollbackStrategy(IRollbackStrategy rollbackStrategy, IEnumerator
                values)
            {
                var objectContainer = _transaction.ObjectContainer();
                while (values.MoveNext())
                {
                    rollbackStrategy.Rollback(objectContainer, values.Current);
                }
            }

            private sealed class _IModifiedObjectQuery_132 : IModifiedObjectQuery
            {
                private readonly ObjectsModifiedInTransaction _enclosing;

                public _IModifiedObjectQuery_132(ObjectsModifiedInTransaction _enclosing)
                {
                    this._enclosing = _enclosing;
                }

                public bool IsModified(object obj)
                {
                    return _enclosing.Contains(obj);
                }
            }
        }
    }
}