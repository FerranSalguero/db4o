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

using System.IO;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.IO;
using Db4objects.Db4o.Tests.Common.Api;
using Db4oUnit;
using Sharpen.IO;

namespace Db4objects.Db4o.Tests.Common.IO
{
    public class RandomAccessFileFactoryTestCase : TestWithTempFile
    {
        /// <exception cref="System.IO.IOException"></exception>
        public virtual void TestLockDatabaseFileFalse()
        {
            var container = OpenObjectContainer(false);
            var raf = RandomAccessFileFactory.NewRandomAccessFile(TempFile(), false
                , false);
            var bytes = new byte[1];
            raf.Read(bytes);
            raf.Close();
            container.Close();
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void TestLockDatabaseFileTrue()
        {
            var container = OpenObjectContainer(true);
            if (!Platform4.NeedsLockFileThread())
            {
                Assert.Expect(typeof (DatabaseFileLockedException), new _ICodeBlock_31(this));
            }
            container.Close();
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void TestReadOnlyLocked()
        {
            var bytes = new byte[1];
            var raf = RandomAccessFileFactory.NewRandomAccessFile(TempFile(), true
                , true);
            Assert.Expect(typeof (IOException), new _ICodeBlock_43(raf, bytes));
            raf.Close();
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void TestReadOnlyUnLocked()
        {
            var bytes = new byte[1];
            var raf = RandomAccessFileFactory.NewRandomAccessFile(TempFile(), true
                , false);
            Assert.Expect(typeof (IOException), new _ICodeBlock_54(raf, bytes));
            raf.Close();
        }

        private IObjectContainer OpenObjectContainer(bool lockDatabaseFile)
        {
            var config = Db4oEmbedded.NewConfiguration();
            config.File.LockDatabaseFile = lockDatabaseFile;
            return Db4oEmbedded.OpenFile(config, TempFile());
        }

        private sealed class _ICodeBlock_31 : ICodeBlock
        {
            private readonly RandomAccessFileFactoryTestCase _enclosing;

            public _ICodeBlock_31(RandomAccessFileFactoryTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                RandomAccessFileFactory.NewRandomAccessFile(_enclosing.TempFile(), false, true
                    );
            }
        }

        private sealed class _ICodeBlock_43 : ICodeBlock
        {
            private readonly byte[] bytes;
            private readonly RandomAccessFile raf;

            public _ICodeBlock_43(RandomAccessFile raf, byte[] bytes)
            {
                this.raf = raf;
                this.bytes = bytes;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                raf.Write(bytes);
            }
        }

        private sealed class _ICodeBlock_54 : ICodeBlock
        {
            private readonly byte[] bytes;
            private readonly RandomAccessFile raf;

            public _ICodeBlock_54(RandomAccessFile raf, byte[] bytes)
            {
                this.raf = raf;
                this.bytes = bytes;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                raf.Write(bytes);
            }
        }
    }
}