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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Tests.Common.Api;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Refactor
{
    public abstract class AccessFieldTestCaseBase : Db4oTestWithTempFile
    {
        /// <exception cref="System.Exception"></exception>
        public override void SetUp()
        {
            WithDatabase(new _IDatabaseAction_14(this));
        }

        protected virtual void RenameClass(Type origClazz, string targetName)
        {
            var config = NewConfiguration();
            config.Common.ObjectClass(origClazz).Rename(targetName);
            WithDatabase(config, new _IDatabaseAction_24());
        }

        // do nothing
        protected abstract object NewOriginalData();

        protected virtual void AssertField(Type targetClazz, string fieldName, Type fieldType
            , object fieldValue)
        {
            WithDatabase(new _IDatabaseAction_35(targetClazz, fieldName, fieldType, fieldValue
                ));
        }

        private void WithDatabase(IDatabaseAction action)
        {
            WithDatabase(NewConfiguration(), action);
        }

        private void WithDatabase(IEmbeddedConfiguration config, IDatabaseAction
            action)
        {
            IObjectContainer db = Db4oEmbedded.OpenFile(config, TempFile());
            try
            {
                action.RunWith(db);
            }
            finally
            {
                db.Close();
            }
        }

        private sealed class _IDatabaseAction_14 : IDatabaseAction
        {
            private readonly AccessFieldTestCaseBase _enclosing;

            public _IDatabaseAction_14(AccessFieldTestCaseBase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void RunWith(IObjectContainer db)
            {
                db.Store(_enclosing.NewOriginalData());
            }
        }

        private sealed class _IDatabaseAction_24 : IDatabaseAction
        {
            public void RunWith(IObjectContainer db)
            {
            }
        }

        private sealed class _IDatabaseAction_35 : IDatabaseAction
        {
            private readonly string fieldName;
            private readonly Type fieldType;
            private readonly object fieldValue;
            private readonly Type targetClazz;

            public _IDatabaseAction_35(Type targetClazz, string fieldName, Type fieldType, object
                fieldValue)
            {
                this.targetClazz = targetClazz;
                this.fieldName = fieldName;
                this.fieldType = fieldType;
                this.fieldValue = fieldValue;
            }

            public void RunWith(IObjectContainer db)
            {
                var storedClass = db.Ext().StoredClass(targetClazz);
                var storedField = storedClass.StoredField(fieldName, fieldType);
                var result = db.Query(targetClazz);
                Assert.AreEqual(1, result.Count);
                var obj = result.Next();
                var value = storedField.Get(obj);
                Assert.AreEqual(fieldValue, value);
            }
        }

        private interface IDatabaseAction
        {
            void RunWith(IObjectContainer db);
        }
    }
}