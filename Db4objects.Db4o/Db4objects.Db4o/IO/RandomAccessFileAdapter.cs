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
using System.IO;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Internal;
using Sharpen.IO;
using File = Sharpen.IO.File;

namespace Db4objects.Db4o.IO
{
    /// <summary>IO adapter for random access files.</summary>
    /// <remarks>IO adapter for random access files.</remarks>
    [Obsolete(@"Use  instead.")]
    public class RandomAccessFileAdapter : IoAdapter
    {
        private readonly RandomAccessFile _delegate;
        private readonly string _path;

        public RandomAccessFileAdapter()
        {
        }

        /// <exception cref="Db4objects.Db4o.Ext.Db4oIOException"></exception>
        protected RandomAccessFileAdapter(string path, bool lockFile, long initialLength,
            bool readOnly)
        {
            var ok = false;
            try
            {
                _path = new File(path).GetCanonicalPath();
                _delegate = RandomAccessFileFactory.NewRandomAccessFile(_path, readOnly, lockFile
                    );
                if (initialLength > 0)
                {
                    _delegate.Seek(initialLength - 1);
                    _delegate.Write(new byte[] {0});
                }
                ok = true;
            }
            catch (IOException e)
            {
                throw new Db4oIOException(e);
            }
            finally
            {
                if (!ok)
                {
                    Close();
                }
            }
        }

        /// <exception cref="Db4objects.Db4o.Ext.Db4oIOException"></exception>
        public override void Close()
        {
            // FIXME: This is a temporary quickfix for a bug in Android.
            //        Remove after Android has been fixed.
            try
            {
                if (_delegate != null)
                {
                    _delegate.Seek(0);
                }
            }
            catch (IOException)
            {
            }
            // ignore
            Platform4.UnlockFile(_path, _delegate);
            try
            {
                if (_delegate != null)
                {
                    _delegate.Close();
                }
            }
            catch (IOException e)
            {
                throw new Db4oIOException(e);
            }
        }

        public override void Delete(string path)
        {
            new File(path).Delete();
        }

        public override bool Exists(string path)
        {
            var existingFile = new File(path);
            return existingFile.Exists() && existingFile.Length() > 0;
        }

        /// <exception cref="Db4objects.Db4o.Ext.Db4oIOException"></exception>
        public override long GetLength()
        {
            try
            {
                return _delegate.Length();
            }
            catch (IOException e)
            {
                throw new Db4oIOException(e);
            }
        }

        /// <exception cref="Db4objects.Db4o.Ext.Db4oIOException"></exception>
        public override IoAdapter Open(string path, bool lockFile, long initialLength, bool
            readOnly)
        {
            return new RandomAccessFileAdapter(path, lockFile, initialLength
                , readOnly);
        }

        /// <exception cref="Db4objects.Db4o.Ext.Db4oIOException"></exception>
        public override int Read(byte[] bytes, int length)
        {
            try
            {
                return _delegate.Read(bytes, 0, length);
            }
            catch (IOException e)
            {
                throw new Db4oIOException(e);
            }
        }

        /// <exception cref="Db4objects.Db4o.Ext.Db4oIOException"></exception>
        public override void Seek(long pos)
        {
            if (DTrace.enabled)
            {
                DTrace.RegularSeek.Log(pos);
            }
            try
            {
                _delegate.Seek(pos);
            }
            catch (IOException e)
            {
                throw new Db4oIOException(e);
            }
        }

        /// <exception cref="Db4objects.Db4o.Ext.Db4oIOException"></exception>
        public override void Sync()
        {
            try
            {
                _delegate.GetFD().Sync();
            }
            catch (IOException e)
            {
                throw new Db4oIOException(e);
            }
        }

        /// <exception cref="Db4objects.Db4o.Ext.Db4oIOException"></exception>
        public override void Write(byte[] buffer, int length)
        {
            try
            {
                _delegate.Write(buffer, 0, length);
            }
            catch (IOException e)
            {
                throw new Db4oIOException(e);
            }
        }
    }
}