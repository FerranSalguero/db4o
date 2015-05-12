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
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Messaging;
using Sharpen;

namespace Db4objects.Db4o.CS.Internal.Messages
{
    public sealed class MUserMessage : MsgObject, IServerSideMessage, IClientSideMessage
    {
        public bool ProcessAtClient()
        {
            return ProcessUserMessage();
        }

        public void ProcessAtServer()
        {
            ProcessUserMessage();
        }

        private bool ProcessUserMessage()
        {
            var recipient = MessageRecipient();
            if (recipient == null)
            {
                return true;
            }
            try
            {
                recipient.ProcessMessage(new MessageContextImpl(this), ReadUserMessage
                    ());
            }
            catch (Exception x)
            {
                // TODO: use MessageContext.sender() to send
                // error back to client
                Runtime.PrintStackTrace(x);
            }
            return true;
        }

        private object ReadUserMessage()
        {
            Unmarshall();
            return ((UserMessagePayload) ReadObjectFromPayLoad()).message;
        }

        private IMessageRecipient MessageRecipient()
        {
            return Config().MessageRecipient();
        }

        public Msg MarshallUserMessage(Transaction transaction, object message)
        {
            return GetWriter(Serializer.Marshall(transaction, new UserMessagePayload
                (message)));
        }

        private class MessageContextImpl : IMessageContext
        {
            private readonly MUserMessage _enclosing;

            internal MessageContextImpl(MUserMessage _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public virtual IMessageSender Sender
            {
                get { return new _IMessageSender_22(this); }
            }

            public virtual IObjectContainer Container
            {
                get { return Transaction.ObjectContainer(); }
            }

            public virtual Transaction Transaction
            {
                get { return _enclosing.Transaction(); }
            }

            private sealed class _IMessageSender_22 : IMessageSender
            {
                private readonly MessageContextImpl _enclosing;

                public _IMessageSender_22(MessageContextImpl _enclosing)
                {
                    this._enclosing = _enclosing;
                }

                public void Send(object message)
                {
                    _enclosing._enclosing.ServerMessageDispatcher().Write(UserMessage.MarshallUserMessage
                        (_enclosing.Transaction, message));
                }
            }
        }

        public sealed class UserMessagePayload
        {
            public object message;

            public UserMessagePayload()
            {
            }

            public UserMessagePayload(object message_)
            {
                message = message_;
            }
        }
    }
}