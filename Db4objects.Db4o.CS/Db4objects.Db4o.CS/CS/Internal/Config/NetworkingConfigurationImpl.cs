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
using Db4objects.Db4o.CS.Config;
using Db4objects.Db4o.CS.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Messaging;

namespace Db4objects.Db4o.CS.Internal.Config
{
    public class NetworkingConfigurationImpl : INetworkingConfiguration
    {
        protected readonly Config4Impl _config;

        internal NetworkingConfigurationImpl(Config4Impl config)
        {
            _config = config;
        }

        public virtual bool BatchMessages
        {
            set
            {
                var flag = value;
                _config.BatchMessages(flag);
            }
        }

        public virtual int MaxBatchQueueSize
        {
            set
            {
                var maxSize = value;
                _config.MaxBatchQueueSize(maxSize);
            }
        }

        public virtual bool SingleThreadedClient
        {
            set
            {
                var flag = value;
                _config.SingleThreadedClient(flag);
            }
        }

        public virtual IMessageRecipient MessageRecipient
        {
            set
            {
                var messageRecipient = value;
                _config.SetMessageRecipient(messageRecipient);
            }
        }

        public virtual IClientServerFactory ClientServerFactory
        {
            get
            {
                var configuredFactory = ((IClientServerFactory) My(typeof (IClientServerFactory
                    )));
                if (null == configuredFactory)
                {
                    return new StandardClientServerFactory();
                }
                return configuredFactory;
            }
            set
            {
                var factory = value;
                _config.EnvironmentContributions().Add(factory);
            }
        }

        public virtual ISocket4Factory SocketFactory
        {
            get
            {
                var configuredFactory = ((ISocket4Factory) My(typeof (ISocket4Factory))
                    );
                if (null == configuredFactory)
                {
                    return new StandardSocket4Factory();
                }
                return configuredFactory;
            }
            set
            {
                var factory = value;
                _config.EnvironmentContributions().Add(factory);
            }
        }

        public virtual Config4Impl Config()
        {
            return _config;
        }

        private object My(Type type)
        {
            var environmentContributions = _config.EnvironmentContributions();
            for (var i = environmentContributions.Count - 1; i >= 0; i--)
            {
                var o = environmentContributions[i];
                if (type.IsInstanceOfType(o))
                {
                    return o;
                }
            }
            return null;
        }
    }
}