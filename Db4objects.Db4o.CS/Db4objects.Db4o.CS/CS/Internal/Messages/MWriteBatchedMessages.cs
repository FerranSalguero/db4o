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

using Db4objects.Db4o.Internal;

namespace Db4objects.Db4o.CS.Internal.Messages
{
    public class MWriteBatchedMessages : MsgD, IServerSideMessage
    {
        public void ProcessAtServer()
        {
            var dispatcher = (IServerMessageDispatcher) MessageDispatcher
                ();
            var count = ReadInt();
            var ta = Transaction();
            for (var i = 0; i < count; i++)
            {
                var writer = _payLoad.ReadStatefulBuffer();
                var messageId = writer.ReadInt();
                var message = GetMessage(messageId);
                var clonedMessage = message.PublicClone();
                clonedMessage.SetMessageDispatcher(MessageDispatcher());
                clonedMessage.SetTransaction(ta);
                if (clonedMessage is MsgD)
                {
                    var msgd = (MsgD) clonedMessage;
                    msgd.PayLoad(writer);
                    if (msgd.PayLoad() != null)
                    {
                        msgd.PayLoad().IncrementOffset(Const4.IntLength);
                        var t = CheckParentTransaction(ta, msgd.PayLoad());
                        msgd.SetTransaction(t);
                        dispatcher.ProcessMessage(msgd);
                    }
                }
                else
                {
                    dispatcher.ProcessMessage(clonedMessage);
                }
            }
        }
    }
}