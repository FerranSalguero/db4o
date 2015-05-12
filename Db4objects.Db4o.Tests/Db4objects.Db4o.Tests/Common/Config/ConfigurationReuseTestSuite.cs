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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.CS;
using Db4objects.Db4o.CS.Internal.Config;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.IO;
using Db4oUnit;
using Db4oUnit.Fixtures;
using Sharpen.Lang;

namespace Db4objects.Db4o.Tests.Common.Config
{
    /// <summary>Tests all combinations of configuration use/reuse scenarios.</summary>
    /// <remarks>Tests all combinations of configuration use/reuse scenarios.</remarks>
    public class ConfigurationReuseTestSuite : FixtureTestSuiteDescription
    {
        internal static readonly FixtureVariable ConfigurationUseFunction = FixtureVariable
            .NewInstance("Successul configuration use");

        internal static readonly FixtureVariable ConfigurationReuseProcedure = FixtureVariable
            .NewInstance("Configuration reuse attempt");

        public ConfigurationReuseTestSuite()
        {
            {
                FixtureProviders(new IFixtureProvider[]
                {
                    new SimpleFixtureProvider(ConfigurationUseFunction
                        , new object[]
                        {
                            new _IFunction4_26(), new _IFunction4_31(this), new _IFunction4_36
                                (this)
                        }),
                    new SimpleFixtureProvider(ConfigurationReuseProcedure, new object[]
                    {
                        new _IProcedure4_49(), new _IProcedure4_51(this), new _IProcedure4_53(this), new
                            _IProcedure4_61(this)
                    })
                });
                TestUnits(new[]
                {
                    typeof (ConfigurationReuseTestUnit
                        )
                });
            }
        }

        internal static IConfiguration NewInMemoryConfiguration()
        {
            var config = Db4oFactory.NewConfiguration();
            config.Storage = new MemoryStorage();
            return config;
        }

        protected virtual IObjectServer OpenServer(IConfiguration config, string databaseFileName
            , int port)
        {
            return Db4oClientServer.OpenServer(Db4oClientServerLegacyConfigurationBridge.AsServerConfiguration
                (config), databaseFileName, port);
        }

        protected virtual IObjectContainer OpenClient(IConfiguration config, string host,
            int port, string user, string password)
        {
            return Db4oClientServer.OpenClient(Db4oClientServerLegacyConfigurationBridge.AsClientConfiguration
                (config), host, port, user, password);
        }

        public class ConfigurationReuseTestUnit : ITestCase
        {
            // each function returns a block that disposes of any containers
            public virtual void Test()
            {
                var config = NewInMemoryConfiguration();
                var tearDownBlock = ((IRunnable) ((IFunction4) ConfigurationUseFunction.Value
                    ).Apply(config));
                try
                {
                    Assert.Expect(typeof (ArgumentException), new _ICodeBlock_79(config));
                }
                finally
                {
                    tearDownBlock.Run();
                }
            }

            private sealed class _ICodeBlock_79 : ICodeBlock
            {
                private readonly IConfiguration config;

                public _ICodeBlock_79(IConfiguration config)
                {
                    this.config = config;
                }

                /// <exception cref="System.Exception"></exception>
                public void Run()
                {
                    ((IProcedure4) ConfigurationReuseProcedure.Value).Apply
                        (config);
                }
            }
        }

        private sealed class _IFunction4_26 : IFunction4
        {
            public object Apply(object config)
            {
                var container = Db4oFactory.OpenFile(((IConfiguration) config), ".");
                return new _IRunnable_28(container);
            }

            private sealed class _IRunnable_28 : IRunnable
            {
                private readonly IObjectContainer container;

                public _IRunnable_28(IObjectContainer container)
                {
                    this.container = container;
                }

                public void Run()
                {
                    container.Close();
                }
            }
        }

        private sealed class _IFunction4_31 : IFunction4
        {
            private readonly ConfigurationReuseTestSuite _enclosing;

            public _IFunction4_31(ConfigurationReuseTestSuite _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public object Apply(object config)
            {
                var server = _enclosing.OpenServer(((IConfiguration) config), ".",
                    0);
                return new _IRunnable_33(server);
            }

            private sealed class _IRunnable_33 : IRunnable
            {
                private readonly IObjectServer server;

                public _IRunnable_33(IObjectServer server)
                {
                    this.server = server;
                }

                public void Run()
                {
                    server.Close();
                }
            }
        }

        private sealed class _IFunction4_36 : IFunction4
        {
            private readonly ConfigurationReuseTestSuite _enclosing;

            public _IFunction4_36(ConfigurationReuseTestSuite _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public object Apply(object config)
            {
                var serverConfig = Db4oFactory.NewConfiguration();
                serverConfig.Storage = new MemoryStorage();
                var server = _enclosing.OpenServer(serverConfig, ".", -1);
                server.GrantAccess("user", "password");
                var client = _enclosing.OpenClient(((IConfiguration) config), "localhost"
                    , server.Ext().Port(), "user", "password");
                return new _IRunnable_42(client, server);
            }

            private sealed class _IRunnable_42 : IRunnable
            {
                private readonly IObjectContainer client;
                private readonly IObjectServer server;

                public _IRunnable_42(IObjectContainer client, IObjectServer server)
                {
                    this.client = client;
                    this.server = server;
                }

                public void Run()
                {
                    client.Close();
                    server.Close();
                }
            }
        }

        private sealed class _IProcedure4_49 : IProcedure4
        {
            public void Apply(object config)
            {
                Db4oFactory.OpenFile(((IConfiguration) config), "..");
            }
        }

        private sealed class _IProcedure4_51 : IProcedure4
        {
            private readonly ConfigurationReuseTestSuite _enclosing;

            public _IProcedure4_51(ConfigurationReuseTestSuite _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Apply(object config)
            {
                _enclosing.OpenServer(((IConfiguration) config), "..", 0);
            }
        }

        private sealed class _IProcedure4_53 : IProcedure4
        {
            private readonly ConfigurationReuseTestSuite _enclosing;

            public _IProcedure4_53(ConfigurationReuseTestSuite _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Apply(object config)
            {
                var server = _enclosing.OpenServer(NewInMemoryConfiguration
                    (), "..", 0);
                try
                {
                    _enclosing.OpenClient(((IConfiguration) config), "localhost", server.Ext().Port
                        (), "user", "password");
                }
                finally
                {
                    server.Close();
                }
            }
        }

        private sealed class _IProcedure4_61 : IProcedure4
        {
            private readonly ConfigurationReuseTestSuite _enclosing;

            public _IProcedure4_61(ConfigurationReuseTestSuite _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Apply(object config)
            {
                _enclosing.OpenClient(((IConfiguration) config), "localhost", unchecked(0xdb40), "user", "password");
            }
        }
    }
}

#endif // !SILVERLIGHT