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
using Db4objects.Db4o.Activation;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Events;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Activation;
using Db4objects.Db4o.Reflect;

namespace Db4objects.Db4o.TA
{
    /// <summary>
    ///     Configuration item that enables Transparent Activation Mode for this
    ///     session.
    /// </summary>
    /// <remarks>
    ///     Configuration item that enables Transparent Activation Mode for this
    ///     session. TA mode should be switched on explicitly for manual TA implementation:
    ///     <br /><br />
    ///     commonConfiguration.Add(new TransparentActivationSupport());
    /// </remarks>
    /// <seealso cref="Db4objects.Db4o.TA.TransparentPersistenceSupport" />
    public class TransparentActivationSupport : IConfigurationItem
    {
        // TODO: unbindOnClose should be configurable
        public virtual void Prepare(IConfiguration configuration)
        {
        }

        // Nothing to do...
        /// <summary>
        ///     Configures the just opened ObjectContainer by setting event listeners,
        ///     which will be triggered when activation or de-activation is required.
        /// </summary>
        /// <remarks>
        ///     Configures the just opened ObjectContainer by setting event listeners,
        ///     which will be triggered when activation or de-activation is required.
        /// </remarks>
        /// <param name="container">the ObjectContainer to configure</param>
        /// <seealso cref="TransparentPersistenceSupport.Apply(Db4objects.Db4o.Internal.IInternalObjectContainer)
        /// 	">
        ///     TransparentPersistenceSupport.Apply(Db4objects.Db4o.Internal.IInternalObjectContainer)
        /// </seealso>
        public virtual void Apply(IInternalObjectContainer container)
        {
            if (IsTransparentActivationEnabledOn(container))
            {
                return;
            }
            var provider = new TransparentActivationDepthProviderImpl
                ();
            SetActivationDepthProvider(container, provider);
            var registry = EventRegistryFor(container);
            registry.Instantiated += new _IEventListener4_45(this).OnEvent;
            registry.Created += new _IEventListener4_50(this).OnEvent;
            registry.Closing += new _IEventListener4_56(this).OnEvent;
            var processor = new TADiagnosticProcessor
                (this, container);
            registry.ClassRegistered += new _IEventListener4_67(processor).OnEvent;
        }

        public static bool IsTransparentActivationEnabledOn(IInternalObjectContainer container
            )
        {
            return ActivationProvider(container) is ITransparentActivationDepthProvider;
        }

        private void SetActivationDepthProvider(IInternalObjectContainer container, IActivationDepthProvider
            provider)
        {
            container.ConfigImpl.ActivationDepthProvider(provider);
        }

        private IEventRegistry EventRegistryFor(IObjectContainer container)
        {
            return EventRegistryFactory.ForObjectContainer(container);
        }

        private void UnbindAll(IInternalObjectContainer container)
        {
            var transaction = container.Transaction;
            // FIXME should that ever happen?
            if (transaction == null)
            {
                return;
            }
            var referenceSystem = transaction.ReferenceSystem();
            referenceSystem.TraverseReferences(new _IVisitor4_95(this));
        }

        private void Unbind(ObjectReference objectReference)
        {
            var obj = objectReference.GetObject();
            if (obj == null || !(obj is IActivatable))
            {
                return;
            }
            Bind(obj, null);
        }

        private void BindActivatableToActivator(ObjectEventArgs oea)
        {
            var obj = oea.Object;
            if (obj is IActivatable)
            {
                var transaction = (Transaction
                    ) oea.Transaction();
                var objectReference = transaction.ReferenceForObject(obj);
                Bind(obj, ActivatorForObject(transaction, objectReference));
            }
        }

        private void Bind(object activatable, IActivator activator)
        {
            ((IActivatable) activatable).Bind(activator);
        }

        private IActivator ActivatorForObject(Transaction transaction
            , ObjectReference objectReference)
        {
            if (IsEmbeddedClient(transaction))
            {
                return new TransactionalActivator(transaction, objectReference);
            }
            return objectReference;
        }

        private bool IsEmbeddedClient(Transaction transaction)
        {
            return IsEmbeddedClient(transaction.ObjectContainer());
        }

        internal virtual Transaction Transaction(EventArgs args)
        {
            return (Transaction) ((TransactionalEventArgs) args).Transaction
                ();
        }

        protected static IActivationDepthProvider ActivationProvider(IInternalObjectContainer
            container)
        {
            return container.ConfigImpl.ActivationDepthProvider();
        }

        private bool IsEmbeddedClient(IObjectContainer objectContainer)
        {
            return objectContainer is ObjectContainerSession;
        }

        private sealed class _IEventListener4_45
        {
            private readonly TransparentActivationSupport _enclosing;

            public _IEventListener4_45(TransparentActivationSupport _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void OnEvent(object sender, ObjectInfoEventArgs args
                )
            {
                _enclosing.BindActivatableToActivator(args);
            }
        }

        private sealed class _IEventListener4_50
        {
            private readonly TransparentActivationSupport _enclosing;

            public _IEventListener4_50(TransparentActivationSupport _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void OnEvent(object sender, ObjectInfoEventArgs args
                )
            {
                _enclosing.BindActivatableToActivator(args);
            }
        }

        private sealed class _IEventListener4_56
        {
            private readonly TransparentActivationSupport _enclosing;

            public _IEventListener4_56(TransparentActivationSupport _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void OnEvent(object sender, ObjectContainerEventArgs
                args)
            {
                var objectContainer = (IInternalObjectContainer) args.ObjectContainer;
                _enclosing.UnbindAll(objectContainer);
                if (!_enclosing.IsEmbeddedClient(objectContainer))
                {
                    _enclosing.SetActivationDepthProvider(objectContainer, null);
                }
            }
        }

        private sealed class _IEventListener4_67
        {
            private readonly TADiagnosticProcessor processor;

            public _IEventListener4_67(TADiagnosticProcessor processor
                )
            {
                this.processor = processor;
            }

            public void OnEvent(object sender, ClassEventArgs args)
            {
                var cea = args;
                processor.OnClassRegistered(cea.ClassMetadata());
            }
        }

        private sealed class _IVisitor4_95 : IVisitor4
        {
            private readonly TransparentActivationSupport _enclosing;

            public _IVisitor4_95(TransparentActivationSupport _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Visit(object obj)
            {
                _enclosing.Unbind((ObjectReference) obj);
            }
        }

        private sealed class TADiagnosticProcessor
        {
            private readonly IInternalObjectContainer _container;
            private readonly TransparentActivationSupport _enclosing;

            public TADiagnosticProcessor(TransparentActivationSupport _enclosing, IInternalObjectContainer
                container)
            {
                this._enclosing = _enclosing;
                _container = container;
            }

            public void OnClassRegistered(ClassMetadata clazz)
            {
                // if(Platform4.isDb4oClass(clazz.getName())) {
                // return;
                // }
                var reflectClass = clazz.ClassReflector();
                if (ActivatableClass().IsAssignableFrom(reflectClass))
                {
                    return;
                }
                if (HasNoActivatingFields(reflectClass))
                {
                    return;
                }
                var diagnostic = new NotTransparentActivationEnabled(
                    clazz);
                var processor = _container.Handlers.DiagnosticProcessor();
                processor.OnDiagnostic(diagnostic);
            }

            private IReflectClass ActivatableClass()
            {
                return _container.Reflector().ForClass(typeof (IActivatable));
            }

            private bool HasNoActivatingFields(IReflectClass clazz)
            {
                var curClass = clazz;
                while (curClass != null)
                {
                    var fields = curClass.GetDeclaredFields();
                    if (!HasNoActivatingFields(fields))
                    {
                        return false;
                    }
                    curClass = curClass.GetSuperclass();
                }
                return true;
            }

            private bool HasNoActivatingFields(IReflectField[] fields)
            {
                for (var i = 0; i < fields.Length; i++)
                {
                    if (IsActivating(fields[i]))
                    {
                        return false;
                    }
                }
                return true;
            }

            private bool IsActivating(IReflectField field)
            {
                var fieldType = field.GetFieldType();
                return fieldType != null && !fieldType.IsPrimitive();
            }
        }
    }
}