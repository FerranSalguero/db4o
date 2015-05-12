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
using Db4objects.Db4o.CS.Foundation;
using Db4objects.Db4o.CS.Internal.Messages;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Sharpen;
using Sharpen.Lang;

namespace Db4objects.Db4o.CS.Internal
{
    public sealed class ServerMessageDispatcherImpl : IServerMessageDispatcher, IRunnable
    {
        private readonly object _lock = new object();
        private readonly object _mainLock;
        private readonly ObjectServerImpl _server;
        private readonly Socket4Adapter _socket;
        internal readonly int _threadID;
        private readonly ClientTransactionHandle _transactionHandle;
        private bool _caresAboutCommitted;
        private bool _closeMessageSent;
        private CallbackObjectInfoCollections _committedInfo;
        private bool _isClosed;
        private bool _loggedin;
        private EventHandler<MessageEventArgs> _messageReceived;
        private Hashtable4 _queryResults;
        private Thread _thread;

        /// <exception cref="System.Exception"></exception>
        internal ServerMessageDispatcherImpl(ObjectServerImpl server, ClientTransactionHandle
            transactionHandle, ISocket4 socket4, int threadID, bool loggedIn, object mainLock
            )
        {
            _mainLock = mainLock;
            _transactionHandle = transactionHandle;
            _loggedin = loggedIn;
            _server = server;
            _threadID = threadID;
            _socket = new Socket4Adapter(socket4);
            _socket.SetSoTimeout(((Config4Impl) server.Configure()).TimeoutServerSocket());
        }

        public void Run()
        {
            _thread = Sharpen.Lang.Thread.CurrentThread();
            try
            {
                SetDispatcherName(string.Empty + _threadID);
                _server.WithEnvironment(new _IRunnable_152(this));
            }
            finally
            {
                Close();
            }
        }

        // TODO: Experiment with packetsize and noDelay
        // i_socket.setSendBufferSize(100);
        // i_socket.setTcpNoDelay(true);
        public bool Close()
        {
            return Close(ShutdownMode.Normal);
        }

        public bool Close(ShutdownMode mode)
        {
            lock (_lock)
            {
                if (!IsMessageDispatcherAlive())
                {
                    return true;
                }
                _isClosed = true;
            }
            lock (_mainLock)
            {
                _transactionHandle.ReleaseTransaction(mode);
                if (!mode.IsFatal())
                {
                    SendCloseMessage();
                }
                _transactionHandle.Close(mode);
                CloseSocket();
                RemoveFromServer();
                return true;
            }
        }

        public void CloseConnection()
        {
            lock (_lock)
            {
                if (!IsMessageDispatcherAlive())
                {
                    return;
                }
                _isClosed = true;
            }
            lock (_mainLock)
            {
                CloseSocket();
                RemoveFromServer();
            }
        }

        public bool IsMessageDispatcherAlive()
        {
            lock (_lock)
            {
                return !_isClosed;
            }
        }

        public Transaction Transaction()
        {
            return _transactionHandle.Transaction();
        }

        public bool ProcessMessage(Msg message)
        {
            if (IsMessageDispatcherAlive())
            {
                if (message is IMessageWithResponse)
                {
                    var msgWithResp = (IMessageWithResponse) message;
                    try
                    {
                        var reply = msgWithResp.ReplyFromServer();
                        Write(reply);
                    }
                    catch (Db4oRecoverableException exc)
                    {
                        WriteException(message, exc);
                        return true;
                    }
                    catch (Exception t)
                    {
                        Runtime.PrintStackTrace(t);
                        FatalShutDownServer(t);
                        return false;
                    }
                    try
                    {
                        msgWithResp.PostProcessAtServer();
                        return true;
                    }
                    catch (Exception exc)
                    {
                        Runtime.PrintStackTrace(exc);
                    }
                    return true;
                }
                try
                {
                    ((IServerSideMessage) message).ProcessAtServer();
                    return true;
                }
                catch (Db4oRecoverableException exc)
                {
                    Runtime.PrintStackTrace(exc);
                    return true;
                }
                catch (Exception t)
                {
                    Runtime.PrintStackTrace(t);
                    FatalShutDownServer(t);
                }
            }
            return false;
        }

        public ObjectServerImpl Server()
        {
            return _server;
        }

        public void QueryResultFinalized(int queryResultID)
        {
            _queryResults.Remove(queryResultID);
        }

        public void MapQueryResultToID(LazyClientObjectSetStub stub, int queryResultID)
        {
            if (_queryResults == null)
            {
                _queryResults = new Hashtable4();
            }
            _queryResults.Put(queryResultID, stub);
        }

        public LazyClientObjectSetStub QueryResultForID(int queryResultID)
        {
            return (LazyClientObjectSetStub) _queryResults.Get(queryResultID);
        }

        public void SwitchToFile(MSwitchToFile message)
        {
            lock (_mainLock)
            {
                var fileName = message.ReadString();
                try
                {
                    _transactionHandle.ReleaseTransaction(ShutdownMode.Normal);
                    _transactionHandle.AcquireTransactionForFile(fileName);
                    Write(Msg.Ok);
                }
                catch (Exception e)
                {
                    _transactionHandle.ReleaseTransaction(ShutdownMode.Normal);
                    Write(Msg.Error);
                }
            }
        }

        public void SwitchToMainFile()
        {
            lock (_mainLock)
            {
                _transactionHandle.ReleaseTransaction(ShutdownMode.Normal);
                Write(Msg.Ok);
            }
        }

        public void UseTransaction(MUseTransaction message)
        {
            var threadID = message.ReadInt();
            var transToUse = _server.FindTransaction(threadID
                );
            _transactionHandle.Transaction(transToUse);
        }

        public bool Write(Msg msg)
        {
            lock (_lock)
            {
                if (!IsMessageDispatcherAlive())
                {
                    return false;
                }
                return msg.Write(_socket);
            }
        }

        public Socket4Adapter Socket()
        {
            return _socket;
        }

        public string Name { get; private set; }

        public void SetDispatcherName(string name)
        {
            Name = name;
            Thread().SetName("db4o server message dispatcher " + name);
        }

        public int DispatcherID()
        {
            return _threadID;
        }

        public void Login()
        {
            _loggedin = true;
        }

        public bool CaresAboutCommitted()
        {
            return _caresAboutCommitted;
        }

        public void CaresAboutCommitted(bool care)
        {
            _caresAboutCommitted = true;
            Server().CheckCaresAboutCommitted();
        }

        public CallbackObjectInfoCollections CommittedInfo()
        {
            return _committedInfo;
        }

        public void DispatchCommitted(CallbackObjectInfoCollections committedInfo)
        {
            _committedInfo = committedInfo;
        }

        public bool WillDispatchCommitted()
        {
            return Server().CaresAboutCommitted();
        }

        public ClassInfoHelper ClassInfoHelper()
        {
            return Server().ClassInfoHelper();
        }

        /// <summary>EventArgs =&gt; MessageEventArgs</summary>
        public event EventHandler<MessageEventArgs> MessageReceived
        {
            add
            {
                _messageReceived = (EventHandler<MessageEventArgs>) Delegate.Combine
                    (_messageReceived, value);
            }
            remove
            {
                _messageReceived = (EventHandler<MessageEventArgs>) Delegate.Remove(
                    _messageReceived, value);
            }
        }

        /// <exception cref="System.Exception"></exception>
        public void Join()
        {
            Thread().Join();
        }

        private void SendCloseMessage()
        {
            try
            {
                if (!_closeMessageSent)
                {
                    _closeMessageSent = true;
                    Write(Msg.Close);
                }
            }
            catch (Exception e)
            {
            }
        }

        private void RemoveFromServer()
        {
            try
            {
                _server.RemoveThread(this);
            }
            catch (Exception e)
            {
            }
        }

        private void CloseSocket()
        {
            try
            {
                if (_socket != null)
                {
                    _socket.Close();
                }
            }
            catch (Db4oIOException e)
            {
            }
        }

        private void MessageLoop()
        {
            while (IsMessageDispatcherAlive())
            {
                try
                {
                    if (!MessageProcessor())
                    {
                        return;
                    }
                }
                catch (Db4oIOException e)
                {
                    if (DTrace.enabled)
                    {
                        DTrace.AddToClassIndex.Log(e.ToString());
                    }
                    return;
                }
            }
        }

        /// <exception cref="Db4objects.Db4o.Ext.Db4oIOException"></exception>
        private bool MessageProcessor()
        {
            var message = Msg.ReadMessage(this, Transaction(), _socket);
            if (message == null)
            {
                return true;
            }
            TriggerMessageReceived(message);
            if (!_loggedin && !Msg.Login.Equals(message))
            {
                return true;
            }
            // TODO: COR-885 - message may process against closed server
            // Checking aliveness just makes the issue less likely to occur. Naive synchronization against main lock is prohibitive.        
            return ProcessMessage(message);
        }

        private void FatalShutDownServer(Exception origExc)
        {
            new FatalServerShutdown(_server, origExc);
        }

        private void WriteException(Msg message, Exception exc)
        {
            if (!(message is IMessageWithResponse))
            {
                Runtime.PrintStackTrace(exc);
                return;
            }
            if (!(exc is Exception))
            {
                exc = new Db4oException(exc);
            }
            // Writing exceptions can produce ClassMetadata in
            // the main ObjectContainer.
            lock (_mainLock)
            {
                message.WriteException(exc);
            }
        }

        private void TriggerMessageReceived(IMessage message)
        {
            if (null != _messageReceived)
                _messageReceived(null, new MessageEventArgs(message
                    ));
        }

        private Thread Thread()
        {
            if (null == _thread)
            {
                throw new InvalidOperationException();
            }
            return _thread;
        }

        private sealed class _IRunnable_152 : IRunnable
        {
            private readonly ServerMessageDispatcherImpl _enclosing;

            public _IRunnable_152(ServerMessageDispatcherImpl _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Run()
            {
                _enclosing.MessageLoop();
            }
        }
    }
}