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
using Db4objects.Db4o.Ext;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Exceptions
{
    public class DatabaseReadonlyExceptionTestCase : AbstractDb4oTestCase, IOptOutTA,
        IOptOutInMemory, IOptOutDefragSolo
    {
        public static void Main(string[] args)
        {
            new DatabaseReadonlyExceptionTestCase().RunAll();
        }

        public virtual void TestRollback()
        {
            ConfigReadOnly();
            Assert.Expect(typeof (DatabaseReadOnlyException), new _ICodeBlock_21(this));
        }

        public virtual void TestCommit()
        {
            ConfigReadOnly();
            Assert.Expect(typeof (DatabaseReadOnlyException), new _ICodeBlock_30(this));
        }

        public virtual void TestSet()
        {
            ConfigReadOnly();
            Assert.Expect(typeof (DatabaseReadOnlyException), new _ICodeBlock_39(this));
        }

        public virtual void TestDelete()
        {
            ConfigReadOnly();
            Assert.Expect(typeof (DatabaseReadOnlyException), new _ICodeBlock_48(this));
        }

        public virtual void TestNewFile()
        {
            Assert.Expect(typeof (DatabaseReadOnlyException), new _ICodeBlock_56(this));
        }

        public virtual void TestReserveStorage()
        {
            ConfigReadOnly();
            var exceptionType = IsMultiSession() && !IsEmbedded()
                ? typeof (NotSupportedException
                    )
                : typeof (DatabaseReadOnlyException);
            Assert.Expect(exceptionType, new _ICodeBlock_70(this));
        }

        public virtual void TestStoredClasses()
        {
            ConfigReadOnly();
            Db().StoredClasses();
        }

        private void ConfigReadOnly()
        {
            Db().Configure().ReadOnly(true);
        }

        private sealed class _ICodeBlock_21 : ICodeBlock
        {
            private readonly DatabaseReadonlyExceptionTestCase _enclosing;

            public _ICodeBlock_21(DatabaseReadonlyExceptionTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.Db().Rollback();
            }
        }

        private sealed class _ICodeBlock_30 : ICodeBlock
        {
            private readonly DatabaseReadonlyExceptionTestCase _enclosing;

            public _ICodeBlock_30(DatabaseReadonlyExceptionTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.Db().Commit();
            }
        }

        private sealed class _ICodeBlock_39 : ICodeBlock
        {
            private readonly DatabaseReadonlyExceptionTestCase _enclosing;

            public _ICodeBlock_39(DatabaseReadonlyExceptionTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.Db().Store(new Item());
            }
        }

        private sealed class _ICodeBlock_48 : ICodeBlock
        {
            private readonly DatabaseReadonlyExceptionTestCase _enclosing;

            public _ICodeBlock_48(DatabaseReadonlyExceptionTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.Db().Delete(new Item());
            }
        }

        private sealed class _ICodeBlock_56 : ICodeBlock
        {
            private readonly DatabaseReadonlyExceptionTestCase _enclosing;

            public _ICodeBlock_56(DatabaseReadonlyExceptionTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                Fixture().Close();
                Fixture().Clean();
                Fixture().Config().ReadOnly(true);
                Fixture().Open(_enclosing);
            }
        }

        private sealed class _ICodeBlock_70 : ICodeBlock
        {
            private readonly DatabaseReadonlyExceptionTestCase _enclosing;

            public _ICodeBlock_70(DatabaseReadonlyExceptionTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.Db().Configure().ReserveStorageSpace(1);
            }
        }
    }
}