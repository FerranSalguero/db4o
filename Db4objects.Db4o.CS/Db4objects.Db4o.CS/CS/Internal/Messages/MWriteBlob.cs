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
using Db4objects.Db4o.Internal.Activation;
using Sharpen;
using Sharpen.IO;

namespace Db4objects.Db4o.CS.Internal.Messages
{
    public class MWriteBlob : MsgBlob, IServerSideMessage
    {
        public virtual void ProcessAtServer()
        {
            try
            {
                var blobImpl = ServerGetBlobImpl();
                if (blobImpl != null)
                {
                    blobImpl.SetTrans(Transaction());
                    var file = blobImpl.ServerFile(null, true);
                    var sock = ServerMessageDispatcher().Socket();
                    Ok.Write(sock);
                    var fout = new FileOutputStream(file);
                    Copy(sock, fout, blobImpl.GetLength(), false);
                    Ok.Write(sock);
                }
            }
            catch (Exception)
            {
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void ProcessClient(Socket4Adapter sock)
        {
            var message = ReadMessage(MessageDispatcher(), Transaction(), sock);
            if (message.Equals(Ok))
            {
                try
                {
                    _currentByte = 0;
                    _length = _blob.GetLength();
                    _blob.GetStatusFrom(this);
                    _blob.SetStatus(Status.Processing);
                    var inBlob = _blob.GetClientInputStream();
                    Copy(inBlob, sock, true);
                    sock.Flush();
                    message = ReadMessage(MessageDispatcher(), Transaction(), sock);
                    if (message.Equals(Ok))
                    {
                        // make sure to load the filename to i_blob
                        // to allow client databasefile switching
                        Container().Deactivate(Transaction(), _blob, int.MaxValue);
                        Container().Activate(Transaction(), _blob, new FullActivationDepth());
                        _blob.SetStatus(Status.Completed);
                    }
                    else
                    {
                        _blob.SetStatus(Status.Error);
                    }
                }
                catch (Exception e)
                {
                    Runtime.PrintStackTrace(e);
                }
            }
        }
    }
}