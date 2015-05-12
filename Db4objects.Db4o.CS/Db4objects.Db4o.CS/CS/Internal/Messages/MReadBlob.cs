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
using Sharpen.IO;

namespace Db4objects.Db4o.CS.Internal.Messages
{
    public class MReadBlob : MsgBlob, IServerSideMessage
    {
        public virtual void ProcessAtServer()
        {
            try
            {
                var blobImpl = ServerGetBlobImpl();
                if (blobImpl != null)
                {
                    blobImpl.SetTrans(Transaction());
                    var file = blobImpl.ServerFile(null, false);
                    var length = (int) file.Length();
                    var sock = ServerMessageDispatcher().Socket();
                    Length.GetWriterForInt(Transaction(), length).Write(sock);
                    var fin = new FileInputStream(file);
                    Copy(fin, sock, false);
                    sock.Flush();
                    Ok.Write(sock);
                }
            }
            catch (Exception)
            {
                Write(Error);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void ProcessClient(Socket4Adapter sock)
        {
            var message = ReadMessage(MessageDispatcher(), Transaction(), sock);
            if (message.Equals(Length))
            {
                try
                {
                    _currentByte = 0;
                    _length = message.PayLoad().ReadInt();
                    _blob.GetStatusFrom(this);
                    _blob.SetStatus(Status.Processing);
                    Copy(sock, _blob.GetClientOutputStream(), _length, true);
                    message = ReadMessage(MessageDispatcher(), Transaction(), sock);
                    if (message.Equals(Ok))
                    {
                        _blob.SetStatus(Status.Completed);
                    }
                    else
                    {
                        _blob.SetStatus(Status.Error);
                    }
                }
                catch (Exception)
                {
                }
            }
            else
            {
                if (message.Equals(Error))
                {
                    _blob.SetStatus(Status.Error);
                }
            }
        }
    }
}