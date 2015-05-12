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
using Db4objects.Db4o.Typehandlers;

namespace Db4objects.Db4o.Internal.Activation
{
    /// <exclude></exclude>
    public class ActivationContext4 : IActivationContext
    {
        private readonly IActivationDepth _depth;
        private readonly object _targetObject;
        private readonly Transaction _transaction;

        public ActivationContext4(Transaction transaction, object
            obj, IActivationDepth depth)
        {
            if (null == obj)
            {
                throw new ArgumentNullException();
            }
            _transaction = transaction;
            _targetObject = obj;
            _depth = depth;
        }

        public virtual void CascadeActivationToTarget()
        {
            var context = ClassMetadata().DescendOnCascadingActivation()
                ? Descend
                    ()
                : this;
            CascadeActivation(context);
        }

        public virtual void CascadeActivationToChild(object obj)
        {
            if (obj == null)
            {
                return;
            }
            var cascadingContext = ForObject(obj);
            var classMetadata = cascadingContext.ClassMetadata
                ();
            if (classMetadata == null || !classMetadata.HasIdentity())
            {
                return;
            }
            CascadeActivation(cascadingContext.Descend());
        }

        public virtual ObjectContainerBase Container()
        {
            return _transaction.Container();
        }

        public virtual object TargetObject()
        {
            return _targetObject;
        }

        public virtual ClassMetadata ClassMetadata()
        {
            return Container().ClassMetadataForObject(_targetObject);
        }

        public virtual IActivationDepth Depth()
        {
            return _depth;
        }

        public virtual IObjectContainer ObjectContainer()
        {
            return Container();
        }

        public virtual Transaction Transaction()
        {
            return _transaction;
        }

        public virtual IActivationContext ForObject(object newTargetObject)
        {
            return new ActivationContext4(Transaction(),
                newTargetObject, Depth());
        }

        public virtual IActivationContext Descend()
        {
            return new ActivationContext4(Transaction(),
                TargetObject(), Depth().Descend(ClassMetadata()));
        }

        private void CascadeActivation(IActivationContext context)
        {
            var depth = context.Depth();
            if (!depth.RequiresActivation())
            {
                return;
            }
            if (depth.Mode().IsDeactivate())
            {
                Container().StillToDeactivate(_transaction, context.TargetObject(), depth, false);
            }
            else
            {
                // FIXME: [TA] do we really need to check for isValueType here?
                var classMetadata = context.ClassMetadata();
                if (classMetadata.IsStruct())
                {
                    classMetadata.CascadeActivation(context);
                }
                else
                {
                    Container().StillToActivate(context);
                }
            }
        }
    }
}