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
using Db4objects.Db4o;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Defragment;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Threading;

namespace Db4oUnit.Extensions.Fixtures
{
    public abstract class AbstractDb4oFixture : IDb4oFixture
    {
        private IConfiguration _configuration;
        private IFixtureConfiguration _fixtureConfiguration;
        private IList _uncaughtExceptions;

        protected AbstractDb4oFixture()
        {
            ResetUncaughtExceptions();
        }

        public virtual void FixtureConfiguration(IFixtureConfiguration fc)
        {
            _fixtureConfiguration = fc;
        }

        public virtual IList UncaughtExceptions()
        {
            return _uncaughtExceptions;
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void Reopen(IDb4oTestCase testInstance)
        {
            Close();
            Open(testInstance);
        }

        public virtual IConfiguration Config()
        {
            if (_configuration == null)
            {
                _configuration = NewConfiguration();
            }
            return _configuration;
        }

        public virtual void Clean()
        {
            DoClean();
            ResetConfig();
            ResetUncaughtExceptions();
        }

        public abstract bool Accept(Type clazz);

        public virtual void ResetConfig()
        {
            _configuration = null;
        }

        public abstract string Label();
        public abstract void Close();
        public abstract void ConfigureAtRuntime(IRuntimeConfigureAction arg1);
        public abstract IExtObjectContainer Db();
        public abstract void Defragment();
        public abstract LocalObjectContainer FileSession();
        public abstract void Open(IDb4oTestCase arg1);

        private void ResetUncaughtExceptions()
        {
            _uncaughtExceptions = new ArrayList();
        }

        protected virtual void ListenToUncaughtExceptions(IThreadPool4 threadPool)
        {
            if (null == threadPool)
            {
                return;
            }
            // mocks don't have thread pools
            threadPool.UncaughtException += new _IEventListener4_42(this).OnEvent;
        }

        protected abstract void DoClean();

        /// <summary>
        ///     Method can be overridden in subclasses with special instantiation requirements (oSGI for instance).
        /// </summary>
        /// <remarks>
        ///     Method can be overridden in subclasses with special instantiation requirements (oSGI for instance).
        /// </remarks>
        /// <returns></returns>
        protected virtual IConfiguration NewConfiguration()
        {
            return Db4oFactory.NewConfiguration();
        }

        /// <exception cref="System.Exception"></exception>
        protected virtual void Defragment(string fileName)
        {
            var targetFile = fileName + ".defrag.backup";
            var defragConfig = new DefragmentConfig(fileName, targetFile);
            defragConfig.ForceBackupDelete(true);
            defragConfig.Db4oConfig(CloneConfiguration());
            Db4objects.Db4o.Defragment.Defragment.Defrag(defragConfig);
        }

        protected virtual string BuildLabel(string label)
        {
            if (null == _fixtureConfiguration)
            {
                return label;
            }
            return label + " - " + _fixtureConfiguration.GetLabel();
        }

        protected virtual void ApplyFixtureConfiguration(IDb4oTestCase testInstance, IConfiguration
            config)
        {
            if (null == _fixtureConfiguration)
            {
                return;
            }
            _fixtureConfiguration.Configure(testInstance, config);
        }

        public override string ToString()
        {
            return Label();
        }

        protected virtual Config4Impl CloneConfiguration()
        {
            return CloneDb4oConfiguration((Config4Impl) Config());
        }

        protected virtual Config4Impl CloneDb4oConfiguration(IConfiguration config)
        {
            return (Config4Impl) ((Config4Impl) config).DeepClone(this);
        }

        protected virtual IThreadPool4 ThreadPoolFor(IObjectContainer container)
        {
            if (container is ObjectContainerBase)
            {
                return ((ObjectContainerBase) container).ThreadPool();
            }
            return null;
        }

        private sealed class _IEventListener4_42
        {
            private readonly AbstractDb4oFixture _enclosing;

            public _IEventListener4_42(AbstractDb4oFixture _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void OnEvent(object sender, UncaughtExceptionEventArgs args)
            {
                _enclosing._uncaughtExceptions.Add(args.Exception
                    );
            }
        }
    }
}