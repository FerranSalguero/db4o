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
using System.Collections;
using System.IO;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.CS.Config;
using Db4objects.Db4o.CS.Foundation;
using Db4objects.Db4o.CS.Internal.Config;
using Db4objects.Db4o.CS.Internal.Messages;
using Db4objects.Db4o.Events;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Config;
using Db4objects.Db4o.Internal.Threading;
using Db4objects.Db4o.Types;
using Sharpen;
using Sharpen.Lang;

namespace Db4objects.Db4o.CS.Internal
{
    public class ObjectServerImpl : IObjectServerEvents, IObjectServer, IExtObjectServer
        , IRunnable, ITransientClass
    {
        private const int StartThreadWaitTimeout = 5000;
        private readonly ClassInfoHelper _classInfoHelper;
        private readonly BlockingQueue _committedInfosQueue = new BlockingQueue();
        private readonly Collection4 _dispatchers = new Collection4();
        private readonly bool _isEmbeddedServer;
        private readonly string _name;
        private readonly ServerConfigurationImpl _serverConfig;
        private readonly ISocket4Factory _socketFactory;
        private readonly Lock4 _startupLock = new Lock4();
        private readonly ClientTransactionPool _transactionPool;
        private bool _caresAboutCommitted;
        private EventHandler<ClientConnectionEventArgs> _clientConnected;
        private EventHandler<StringEventArgs> _clientDisconnected;
        private EventHandler<ServerClosedEventArgs> _closed;
        private CommittedCallbacksDispatcher _committedCallbacksDispatcher;
        private LocalObjectContainer _container;
        private int _port;
        private IServerSocket4 _serverSocket;
        private int i_threadIDGen = 1;

        public ObjectServerImpl(LocalObjectContainer container, IServerConfiguration serverConfig
            , int port) : this(container, (ServerConfigurationImpl) serverConfig, (port < 0
                ? 0
                : port), port == 0)
        {
        }

        private ObjectServerImpl(LocalObjectContainer container, ServerConfigurationImpl
            serverConfig, int port, bool isEmbeddedServer)
        {
            _isEmbeddedServer = isEmbeddedServer;
            _container = container;
            _serverConfig = serverConfig;
            _socketFactory = serverConfig.Networking.SocketFactory;
            _transactionPool = new ClientTransactionPool(container);
            _port = port;
            _name = "db4o ServerSocket FILE: " + container + "  PORT:" + _port;
            _container.SetServer(true);
            ConfigureObjectServer();
            _classInfoHelper = new ClassInfoHelper(Db4oClientServerLegacyConfigurationBridge
                .AsLegacy(serverConfig));
            _container.ClassCollection().CheckAllClassChanges();
            var ok = false;
            try
            {
                EnsureLoadStaticClass();
                StartCommittedCallbackThread(_committedInfosQueue);
                StartServer();
                if (_serverConfig != null)
                {
                    _serverConfig.ApplyConfigurationItems(this);
                }
                ok = true;
            }
            finally
            {
                if (!ok)
                {
                    Close();
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void Backup(string path)
        {
            _container.Backup(path);
        }

        public virtual IConfiguration Configure()
        {
            return Db4oClientServerLegacyConfigurationBridge.AsLegacy(_serverConfig);
        }

        public virtual IObjectContainer ObjectContainer()
        {
            return _container;
        }

        public virtual void RevokeAccess(string userName)
        {
            lock (this)
            {
                CheckClosed();
                lock (_container.Lock())
                {
                    DeleteUsers(userName);
                    _container.Commit();
                }
            }
        }

        public virtual int Port()
        {
            return _port;
        }

        public virtual int ClientCount()
        {
            lock (_dispatchers)
            {
                return _dispatchers.Size();
            }
        }

        /// <summary>System.IDisposable.Dispose()</summary>
        public virtual void Dispose()
        {
            Close();
        }

        public virtual bool Close()
        {
            lock (this)
            {
                return Close(ShutdownMode.Normal);
            }
        }

        public virtual IExtObjectServer Ext()
        {
            return this;
        }

        public virtual void GrantAccess(string userName, string password)
        {
            lock (this)
            {
                CheckClosed();
                lock (_container.Lock())
                {
                    var existing = GetUser(userName);
                    if (existing != null)
                    {
                        SetPassword(existing, password);
                    }
                    else
                    {
                        AddUser(userName, password);
                    }
                    _container.Commit();
                }
            }
        }

        public virtual IObjectContainer OpenClient()
        {
            lock (this)
            {
                CheckClosed();
                lock (_container.Lock())
                {
                    return new ObjectContainerSession(_container);
                }
            }
        }

        public virtual event EventHandler<ClientConnectionEventArgs> ClientConnected
        {
            add
            {
                _clientConnected = (EventHandler<ClientConnectionEventArgs>) Delegate.Combine
                    (_clientConnected, value);
            }
            remove
            {
                _clientConnected = (EventHandler<ClientConnectionEventArgs>) Delegate.Remove
                    (_clientConnected, value);
            }
        }

        public virtual event EventHandler<StringEventArgs>
            ClientDisconnected
            {
                add
                {
                    _clientDisconnected = (EventHandler<StringEventArgs>
                        ) Delegate.Combine(_clientDisconnected, value);
                }
                remove
                {
                    _clientDisconnected = (EventHandler<StringEventArgs>
                        ) Delegate.Remove(_clientDisconnected, value);
                }
            }

        public virtual event EventHandler<ServerClosedEventArgs> Closed
        {
            add
            {
                _closed = (EventHandler<ServerClosedEventArgs>) Delegate.Combine(_closed
                    , value);
            }
            remove
            {
                _closed = (EventHandler<ServerClosedEventArgs>) Delegate.Remove(_closed
                    , value);
            }
        }

        public virtual void Run()
        {
            LogListeningOnPort();
            NotifyThreadStarted();
            Listen();
        }

        private void StartServer()
        {
            if (IsEmbeddedServer())
            {
                return;
            }
            _startupLock.Run(new _IClosure4_101(this));
        }

        private void StartServerThread()
        {
            _startupLock.Run(new _IClosure4_120(this));
        }

        private IThreadPool4 ThreadPool()
        {
            return _container.ThreadPool();
        }

        private void StartServerSocket()
        {
            try
            {
                _serverSocket = _socketFactory.CreateServerSocket(_port);
                _port = _serverSocket.GetLocalPort();
            }
            catch (IOException e)
            {
                throw new Db4oIOException(e);
            }
            _serverSocket.SetSoTimeout(_serverConfig.TimeoutServerSocket);
        }

        private bool IsEmbeddedServer()
        {
            return _isEmbeddedServer;
        }

        private void EnsureLoadStaticClass()
        {
            _container.ProduceClassMetadata(_container._handlers.IclassStaticclass);
        }

        private void ConfigureObjectServer()
        {
            ((CommonConfigurationImpl) _serverConfig.Common).CallbackMode(CallBackMode.DeleteOnly
                );
            // the minimum activation depth of com.db4o.User.class should be 1.
            // Otherwise, we may get null password.
            _serverConfig.Common.ObjectClass(typeof (User)).MinimumActivationDepth(1);
        }

        internal void CheckClosed()
        {
            if (_container == null)
            {
                Exceptions4.ThrowRuntimeException(Db4o.Internal.Messages.ClosedOrOpenFailed
                    , _name);
            }
            _container.CheckClosed();
        }

        public virtual bool Close(ShutdownMode mode)
        {
            lock (this)
            {
                try
                {
                    CloseServerSocket();
                    StopCommittedCallbacksDispatcher();
                    CloseMessageDispatchers(mode);
                    return CloseFile(mode);
                }
                finally
                {
                    TriggerClosed();
                }
            }
        }

        private void StopCommittedCallbacksDispatcher()
        {
            if (_committedCallbacksDispatcher != null)
            {
                _committedCallbacksDispatcher.Stop();
            }
        }

        private bool CloseFile(ShutdownMode mode)
        {
            if (_container != null)
            {
                _transactionPool.Close(mode);
                _container = null;
            }
            return true;
        }

        private void CloseMessageDispatchers(ShutdownMode mode)
        {
            var i = IterateDispatchers();
            while (i.MoveNext())
            {
                try
                {
                    ((IServerMessageDispatcher) i.Current).Close(mode);
                }
                catch (Exception e)
                {
                    Runtime.PrintStackTrace(e);
                }
            }
        }

        public virtual IEnumerator IterateDispatchers()
        {
            lock (_dispatchers)
            {
                return new Collection4(_dispatchers).GetEnumerator();
            }
        }

        private void CloseServerSocket()
        {
            try
            {
                if (_serverSocket != null)
                {
                    _serverSocket.Close();
                }
            }
            catch (Exception)
            {
            }
            _serverSocket = null;
        }

        private ServerMessageDispatcherImpl FindThread(int a_threadID)
        {
            lock (_dispatchers)
            {
                var i = _dispatchers.GetEnumerator();
                while (i.MoveNext())
                {
                    var serverThread = (ServerMessageDispatcherImpl) i.Current;
                    if (serverThread._threadID == a_threadID)
                    {
                        return serverThread;
                    }
                }
            }
            return null;
        }

        internal virtual Transaction FindTransaction(int threadID)
        {
            var dispatcher = FindThread(threadID);
            return (dispatcher == null ? null : dispatcher.Transaction());
        }

        private void AddUser(string userName, string password)
        {
            _container.Store(new User(userName, password));
        }

        private void SetPassword(User existing, string password)
        {
            existing.password = password;
            _container.Store(existing);
        }

        public virtual User GetUser(string userName)
        {
            var result = QueryUsers(userName);
            if (!result.HasNext())
            {
                return null;
            }
            return (User) result.Next();
        }

        private IObjectSet QueryUsers(string userName)
        {
            _container.ShowInternalClasses(true);
            try
            {
                return _container.QueryByExample(new User(userName, null));
            }
            finally
            {
                _container.ShowInternalClasses(false);
            }
        }

        internal virtual void RemoveThread(ServerMessageDispatcherImpl dispatcher)
        {
            lock (_dispatchers)
            {
                _dispatchers.Remove(dispatcher);
                CheckCaresAboutCommitted();
            }
            TriggerClientDisconnected(dispatcher.Name);
        }

        private void DeleteUsers(string userName)
        {
            var set = QueryUsers(userName);
            while (set.HasNext())
            {
                _container.Delete(set.Next());
            }
        }

        private void StartCommittedCallbackThread(BlockingQueue committedInfosQueue)
        {
            if (IsEmbeddedServer())
            {
                return;
            }
            _committedCallbacksDispatcher = new CommittedCallbacksDispatcher(this, committedInfosQueue
                );
            ThreadPool().Start("Server commit callback dispatcher thread", _committedCallbacksDispatcher
                );
        }

        private void Listen()
        {
            // we are keeping a reference to container to avoid race conditions upon closing this server
            var threadContainer = _container;
            while (_serverSocket != null)
            {
                threadContainer.WithEnvironment(new _IRunnable_352(this, threadContainer));
            }
        }

        // CatchAll because we can get expected timeout exceptions
        // although we still want to continue to use the ServerSocket.
        // No nice way to catch a specific exception because 
        // SocketTimeOutException is JDK 1.4 and above.
        //e.printStackTrace();
        private void TriggerClientConnected(IServerMessageDispatcher messageDispatcher)
        {
            if (null != _clientConnected)
                _clientConnected(null, new ClientConnectionEventArgs
                    (messageDispatcher));
        }

        private void TriggerClientDisconnected(string clientName)
        {
            if (null != _clientDisconnected)
                _clientDisconnected(null, new StringEventArgs(clientName
                    ));
        }

        private void TriggerClosed()
        {
            if (null != _closed) _closed(null, new ServerClosedEventArgs());
        }

        private void NotifyThreadStarted()
        {
            _startupLock.Run(new _IClosure4_395(this));
        }

        private void LogListeningOnPort()
        {
            _container.LogMsg(Db4o.Internal.Messages.ServerListeningOnPort, string.Empty
                                                                            + _serverSocket.GetLocalPort());
        }

        private int NewThreadId()
        {
            return i_threadIDGen++;
        }

        private void AddServerMessageDispatcher(IServerMessageDispatcher dispatcher)
        {
            lock (_dispatchers)
            {
                _dispatchers.Add(dispatcher);
                CheckCaresAboutCommitted();
            }
            TriggerClientConnected(dispatcher);
        }

        public virtual void AddCommittedInfoMsg(MCommittedInfo message)
        {
            _committedInfosQueue.Add(message);
        }

        public virtual void BroadcastReplicationCommit(long timestamp, IList concurrentTimestamps
            )
        {
            var i = IterateDispatchers();
            while (i.MoveNext())
            {
                var dispatcher = (IServerMessageDispatcher) i.Current;
                var transaction = (LocalTransaction) dispatcher.Transaction();
                transaction.NotifyAboutOtherReplicationCommit(timestamp, concurrentTimestamps);
            }
        }

        public virtual void BroadcastMsg(Msg message, IBroadcastFilter filter)
        {
            var i = IterateDispatchers();
            while (i.MoveNext())
            {
                var dispatcher = (IServerMessageDispatcher) i.Current;
                if (filter.Accept(dispatcher))
                {
                    dispatcher.Write(message);
                }
            }
        }

        public virtual bool CaresAboutCommitted()
        {
            return _caresAboutCommitted;
        }

        public virtual void CheckCaresAboutCommitted()
        {
            _caresAboutCommitted = AnyDispatcherCaresAboutCommitted();
        }

        private bool AnyDispatcherCaresAboutCommitted()
        {
            var i = IterateDispatchers();
            while (i.MoveNext())
            {
                var dispatcher = (IServerMessageDispatcher) i.Current;
                if (dispatcher.CaresAboutCommitted())
                {
                    return true;
                }
            }
            return false;
        }

        public virtual ClassInfoHelper ClassInfoHelper()
        {
            return _classInfoHelper;
        }

        internal virtual void WithEnvironment(IRunnable runnable)
        {
            _container.WithEnvironment(runnable);
        }

        public virtual int TransactionCount()
        {
            return _transactionPool.OpenTransactionCount();
        }

        private sealed class _IClosure4_101 : IClosure4
        {
            private readonly ObjectServerImpl _enclosing;

            public _IClosure4_101(ObjectServerImpl _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public object Run()
            {
                _enclosing.StartServerSocket();
                _enclosing.StartServerThread();
                var started = false;
                while (!started)
                {
                    try
                    {
                        _enclosing._startupLock.Snooze(StartThreadWaitTimeout);
                        started = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                // not specialized to InterruptException for .NET conversion
                return null;
            }
        }

        private sealed class _IClosure4_120 : IClosure4
        {
            private readonly ObjectServerImpl _enclosing;

            public _IClosure4_120(ObjectServerImpl _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public object Run()
            {
                _enclosing.ThreadPool().Start(_enclosing._name, _enclosing);
                return null;
            }
        }

        private sealed class _IRunnable_352 : IRunnable
        {
            private readonly ObjectServerImpl _enclosing;
            private readonly LocalObjectContainer threadContainer;

            public _IRunnable_352(ObjectServerImpl _enclosing, LocalObjectContainer threadContainer
                )
            {
                this._enclosing = _enclosing;
                this.threadContainer = threadContainer;
            }

            public void Run()
            {
                try
                {
                    var socket = _enclosing._serverSocket.Accept();
                    var messageDispatcher = new ServerMessageDispatcherImpl(_enclosing,
                        new ClientTransactionHandle(_enclosing._transactionPool), socket
                        , _enclosing.NewThreadId(), false, threadContainer.Lock());
                    _enclosing.AddServerMessageDispatcher(messageDispatcher);
                    _enclosing.ThreadPool().Start("server message dispatcher (still initializing)"
                        , messageDispatcher);
                }
                catch (Exception)
                {
                }
            }
        }

        private sealed class _IClosure4_395 : IClosure4
        {
            private readonly ObjectServerImpl _enclosing;

            public _IClosure4_395(ObjectServerImpl _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public object Run()
            {
                _enclosing._startupLock.Awake();
                return null;
            }
        }
    }
}