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

#if !SILVERLIGHT
using System;
using Db4objects.Db4o;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.CS.Config;
using Db4objects.Db4o.CS.Internal.Config;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Threading;
using Db4oUnit.Extensions.Util;
using Sharpen.IO;

namespace Db4oUnit.Extensions.Fixtures
{
    public class Db4oNetworking : AbstractDb4oFixture, IDb4oClientServerFixture
    {
        private const int ThreadpoolTimeout = 3000;
        protected static readonly string File = "Db4oClientServer.db4o";
        public static readonly string Host = "127.0.0.1";
        public static readonly string Username = "db4o";
        public static readonly string Password = Username;
        private readonly IClientServerFactory _csFactory;
        private readonly File _file;
        private readonly string _label;
        private IExtObjectContainer _objectContainer;
        private int _port;
        private IObjectServer _server;
        private IConfiguration _serverConfig;

        public Db4oNetworking(IClientServerFactory csFactory, string label)
        {
            _csFactory = csFactory != null ? csFactory : DefaultClientServerFactory();
            _file = new File(FilePath());
            _label = label;
        }

        public Db4oNetworking(string label) : this(null, label)
        {
        }

        public Db4oNetworking() : this("C/S")
        {
        }

        /// <exception cref="System.Exception"></exception>
        public override void Open(IDb4oTestCase testInstance)
        {
            OpenServerFor(testInstance);
            OpenClientFor(testInstance);
            ListenToUncaughtExceptions();
        }

        /// <exception cref="System.Exception"></exception>
        public virtual IExtObjectContainer OpenNewSession(IDb4oTestCase testInstance)
        {
            var config = ClientConfigFor(testInstance);
            return OpenClientWith(config);
        }

        /// <exception cref="System.Exception"></exception>
        public override void Close()
        {
            if (null != _objectContainer)
            {
                var clientThreadPool = ClientThreadPool();
                _objectContainer.Close();
                _objectContainer = null;
                if (null != clientThreadPool)
                {
                    clientThreadPool.Join(ThreadpoolTimeout);
                }
            }
            CloseServer();
        }

        public override IExtObjectContainer Db()
        {
            return _objectContainer;
        }

        public virtual IObjectServer Server()
        {
            return _server;
        }

        /// <summary>
        ///     Does not accept a clazz which is assignable from OptOutCS, or not
        ///     assignable from Db4oTestCase.
        /// </summary>
        /// <remarks>
        ///     Does not accept a clazz which is assignable from OptOutCS, or not
        ///     assignable from Db4oTestCase.
        /// </remarks>
        /// <returns>
        ///     returns false if the clazz is assignable from OptOutCS, or not
        ///     assignable from Db4oTestCase. Otherwise, returns true.
        /// </returns>
        public override bool Accept(Type clazz)
        {
            if (!typeof (IDb4oTestCase).IsAssignableFrom(clazz))
            {
                return false;
            }
            if (typeof (IOptOutMultiSession).IsAssignableFrom(clazz))
            {
                return false;
            }
            if (typeof (IOptOutNetworkingCS).IsAssignableFrom(clazz))
            {
                return false;
            }
            return true;
        }

        public override LocalObjectContainer FileSession()
        {
            return (LocalObjectContainer) _server.Ext().ObjectContainer();
        }

        /// <exception cref="System.Exception"></exception>
        public override void Defragment()
        {
            Defragment(FilePath());
        }

        public override string Label()
        {
            return BuildLabel(_label);
        }

        public virtual int ServerPort()
        {
            return _port;
        }

        public override void ConfigureAtRuntime(IRuntimeConfigureAction action)
        {
            action.Apply(Config());
            action.Apply(_serverConfig);
        }

        private IClientServerFactory DefaultClientServerFactory()
        {
            return new StandardClientServerFactory();
        }

        private void ListenToUncaughtExceptions()
        {
            ListenToUncaughtExceptions(ServerThreadPool());
            var clientThreadPool = ClientThreadPool();
            if (null != clientThreadPool)
            {
                ListenToUncaughtExceptions(clientThreadPool);
            }
        }

        private IThreadPool4 ClientThreadPool()
        {
            return ThreadPoolFor(_objectContainer);
        }

        private IThreadPool4 ServerThreadPool()
        {
            return ThreadPoolFor(_server.Ext().ObjectContainer());
        }

        /// <exception cref="System.Exception"></exception>
        private void OpenClientFor(IDb4oTestCase testInstance)
        {
            var config = ClientConfigFor(testInstance);
            _objectContainer = OpenClientWith(config);
        }

        /// <exception cref="System.Exception"></exception>
        private IConfiguration ClientConfigFor(IDb4oTestCase testInstance)
        {
            if (RequiresCustomConfiguration(testInstance))
            {
                var customServerConfig = NewConfiguration();
                ((ICustomClientServerConfiguration) testInstance).ConfigureClient(customServerConfig
                    );
                return customServerConfig;
            }
            IConfiguration config = CloneConfiguration();
            ApplyFixtureConfiguration(testInstance, config);
            return config;
        }

        private IExtObjectContainer OpenSocketClient(IConfiguration config)
        {
            return _csFactory.OpenClient(AsClientConfiguration(config), Host, _port, Username
                , Password).Ext();
        }

        private IExtObjectContainer OpenClientWith(IConfiguration config)
        {
            return OpenSocketClient(config);
        }

        /// <exception cref="System.Exception"></exception>
        private void OpenServerFor(IDb4oTestCase testInstance)
        {
            _serverConfig = ServerConfigFor(testInstance);
            _server = _csFactory.OpenServer(AsServerConfiguration(_serverConfig), _file.GetAbsolutePath
                (), -1);
            _port = _server.Ext().Port();
            _server.GrantAccess(Username, Password);
        }

        /// <exception cref="System.Exception"></exception>
        private IConfiguration ServerConfigFor(IDb4oTestCase testInstance)
        {
            if (RequiresCustomConfiguration(testInstance))
            {
                var customServerConfig = NewConfiguration();
                ((ICustomClientServerConfiguration) testInstance).ConfigureServer(customServerConfig
                    );
                return customServerConfig;
            }
            return CloneConfiguration();
        }

        private bool RequiresCustomConfiguration(IDb4oTestCase testInstance)
        {
            if (testInstance is ICustomClientServerConfiguration)
            {
                return true;
            }
            return false;
        }

        /// <exception cref="System.Exception"></exception>
        private void CloseServer()
        {
            if (null != _server)
            {
                var serverThreadPool = ServerThreadPool();
                _server.Close();
                _server = null;
                if (null != serverThreadPool)
                {
                    serverThreadPool.Join(ThreadpoolTimeout);
                }
            }
        }

        protected override void DoClean()
        {
            _file.Delete();
        }

        private static string FilePath()
        {
            return CrossPlatformServices.DatabasePath(File);
        }

        private IClientConfiguration AsClientConfiguration(IConfiguration serverConfig)
        {
            return Db4oClientServerLegacyConfigurationBridge.AsClientConfiguration(serverConfig
                );
        }

        private IServerConfiguration AsServerConfiguration(IConfiguration serverConfig)
        {
            return Db4oClientServerLegacyConfigurationBridge.AsServerConfiguration(serverConfig
                );
        }
    }
}

#endif // !SILVERLIGHT