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
using Db4objects.Db4o.TA;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.TA
{
    public abstract class TPFieldIndexConsistencyTestCaseBase : AbstractDb4oTestCase
    {
        protected static readonly string IdFieldName = "_id";

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.Add(new TransparentPersistenceSupport());
            config.ObjectClass(typeof (Item)).ObjectField(
                IdFieldName).Indexed(true);
        }

        protected virtual void AssertFieldIndex(int id)
        {
            var claxx = Reflector().ForClass(typeof (Item
                ));
            var classMetadata = FileSession().ClassMetadataForReflectClass(claxx);
            var field = classMetadata.FieldMetadataForName(IdFieldName);
            var indexRange = field.Search(Trans(), id);
            Assert.AreEqual(1, indexRange.Size());
        }

        protected virtual void AssertItemQuery(int id)
        {
            var query = NewQuery(typeof (Item));
            query.Descend(IdFieldName).Constrain(id);
            var result = query.Execute();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(id, ((Item) result.Next()).Id(
                ));
        }

        public class Item : IActivatable
        {
            [NonSerialized] private IActivator _activator;

            public int _id;

            public Item(int id)
            {
                _id = id;
            }

            public virtual void Activate(ActivationPurpose purpose)
            {
                if (_activator != null)
                {
                    _activator.Activate(purpose);
                }
            }

            public virtual void Bind(IActivator activator)
            {
                if (_activator != null && activator != null && _activator != activator)
                {
                    throw new InvalidOperationException();
                }
                _activator = activator;
            }

            public virtual int Id()
            {
                Activate(ActivationPurpose.Read);
                return _id;
            }

            public virtual void Id(int id)
            {
                Activate(ActivationPurpose.Write);
                _id = id;
            }
        }
    }
}