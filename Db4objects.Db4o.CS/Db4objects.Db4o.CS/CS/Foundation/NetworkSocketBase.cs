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
using System.Net.Sockets;
using Db4objects.Db4o.Internal;
using Sharpen;
using Sharpen.IO;
using Socket = Sharpen.Net.Socket;

namespace Db4objects.Db4o.CS.Foundation
{
    public abstract class NetworkSocketBase : ISocket4
    {
        private readonly string _hostName;
        private readonly IInputStream _in;
        private readonly IOutputStream _out;
        private readonly Socket _socket;

        /// <exception cref="System.IO.IOException"></exception>
        public NetworkSocketBase(Socket socket) : this(socket, null)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public NetworkSocketBase(Socket socket, string hostName)
        {
            _socket = socket;
            _hostName = hostName;
            _in = _socket.GetInputStream();
            _out = _socket.GetOutputStream();
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void Close()
        {
            _socket.Close();
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void Flush()
        {
            _out.Flush();
        }

        public virtual bool IsConnected()
        {
            return Platform4.IsConnected(_socket);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual int Read(byte[] a_bytes, int a_offset, int a_length)
        {
            var ret = _in.Read(a_bytes, a_offset, a_length);
            CheckEOF(ret);
            return ret;
        }

        public virtual void SetSoTimeout(int timeout)
        {
            try
            {
                _socket.SetSoTimeout(timeout);
            }
            catch (SocketException e)
            {
                Runtime.PrintStackTrace(e);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void Write(byte[] bytes, int off, int len)
        {
            _out.Write(bytes, off, len);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual ISocket4 OpenParallelSocket()
        {
            if (_hostName == null)
            {
                throw new InvalidOperationException();
            }
            return CreateParallelSocket(_hostName, _socket.GetPort());
        }

        /// <exception cref="System.IO.IOException"></exception>
        private void CheckEOF(int ret)
        {
            if (ret == -1)
            {
                throw new IOException();
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        protected abstract ISocket4 CreateParallelSocket(string hostName, int port);

        public override string ToString()
        {
            return _socket.ToString();
        }
    }
}