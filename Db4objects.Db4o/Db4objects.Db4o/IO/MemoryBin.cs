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
using Sharpen.Lang;

namespace Db4objects.Db4o.IO
{
    public class MemoryBin : IBin
    {
        private readonly IGrowthStrategy _growthStrategy;
        private byte[] _bytes;
        private int _length;

        public MemoryBin(int initialSize, IGrowthStrategy growthStrategy) : this(new byte
            [initialSize], growthStrategy)
        {
        }

        public MemoryBin(byte[] bytes, IGrowthStrategy growthStrategy)
        {
            _bytes = bytes;
            _length = bytes.Length;
            _growthStrategy = growthStrategy;
        }

        public virtual long Length()
        {
            return _length;
        }

        /// <exception cref="Db4objects.Db4o.Ext.Db4oIOException"></exception>
        public virtual int Read(long pos, byte[] bytes, int length)
        {
            var avail = _length - pos;
            if (avail <= 0)
            {
                return -1;
            }
            var read = Math.Min((int) avail, length);
            Array.Copy(_bytes, (int) pos, bytes, 0, read);
            return read;
        }

        /// <exception cref="Db4objects.Db4o.Ext.Db4oIOException"></exception>
        public virtual void Sync()
        {
        }

        public virtual int SyncRead(long position, byte[] bytes, int bytesToRead)
        {
            return Read(position, bytes, bytesToRead);
        }

        public virtual void Close()
        {
        }

        /// <summary>for internal processing only.</summary>
        /// <remarks>for internal processing only.</remarks>
        /// <exception cref="Db4objects.Db4o.Ext.Db4oIOException"></exception>
        public virtual void Write(long pos, byte[] buffer, int length)
        {
            if (pos + length > _bytes.Length)
            {
                var newSize = _growthStrategy.NewSize(_bytes.Length, pos + length);
                //			if (pos + length > newSize) {
                //				newSize = pos + length;
                //			}
                var temp = new byte[(int) newSize];
                Array.Copy(_bytes, 0, temp, 0, _length);
                _bytes = temp;
            }
            Array.Copy(buffer, 0, _bytes, (int) pos, length);
            pos += length;
            if (pos > _length)
            {
                _length = (int) pos;
            }
        }

        public virtual void Sync(IRunnable runnable)
        {
            runnable.Run();
        }

        public virtual long BufferSize()
        {
            return _bytes.Length;
        }

        /// <summary>
        ///     Returns a copy of the raw data contained in this bin for external processing.
        /// </summary>
        /// <remarks>
        ///     Returns a copy of the raw data contained in this bin for external processing.
        ///     Access to the data is not guarded by synchronisation. If this method is called
        ///     while the MemoryBin is in use, it is possible that the returned byte array is
        ///     not consistent.
        /// </remarks>
        public virtual byte[] Data()
        {
            var data = new byte[_length];
            Array.Copy(_bytes, 0, data, 0, _length);
            return data;
        }
    }
}