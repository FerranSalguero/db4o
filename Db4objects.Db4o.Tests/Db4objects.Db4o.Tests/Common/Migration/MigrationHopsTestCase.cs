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

using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Reflect.Generic;
using Db4objects.Db4o.Tests.Common.Api;
using Db4objects.Db4o.Tests.Common.Handlers;
using Db4oUnit;
using Db4oUnit.Extensions;
using Sharpen;

namespace Db4objects.Db4o.Tests.Common.Migration
{
    public class MigrationHopsTestCase : TestWithTempFile, IOptOutWorkspaceIssue
    {
        private Db4oLibraryEnvironmentProvider _environmentProvider;

        /// <exception cref="System.Exception"></exception>
        public virtual void Test()
        {
            var originalEnv = EnvironmentForVersion("6.0");
            originalEnv.InvokeInstanceMethod(typeof (Tester), "createDatabase", TempFile());
            string[] hopArray = {"6.4", "7.4", CurrentVersion()};
            for (var hopIndex = 0; hopIndex < hopArray.Length; ++hopIndex)
            {
                var hop = hopArray[hopIndex];
                var hopEnvironment = EnvironmentForVersion(hop);
                Assert.AreEqual(originalEnv.Version(), InvokeTesterMethodOn(hopEnvironment, "currentVersion"
                    ));
            }
            var config = Db4oEmbedded.NewConfiguration();
            config.Common.ReflectWith(new ExcludingReflector(new[]
            {
                typeof (Item
                    )
            }));
            var container = Db4oEmbedded.OpenFile(config, TempFile());
            try
            {
                var query = container.Query();
                query.Constrain(typeof (Item));
                var item = query.Execute()[0];
                Assert.AreEqual(originalEnv.Version(), ((GenericObject) item).Get(0));
            }
            finally
            {
                container.Close();
            }
        }

        private string CurrentVersion()
        {
            return Db4oVersion.Major + "." + Db4oVersion.Minor;
        }

        /// <exception cref="System.Exception"></exception>
        private object InvokeTesterMethodOn(Db4oLibraryEnvironment env74, string methodName
            )
        {
            return env74.InvokeInstanceMethod(typeof (Tester), methodName, TempFile());
        }

        /// <exception cref="System.IO.IOException"></exception>
        private Db4oLibraryEnvironment EnvironmentForVersion(string version)
        {
            return new Db4oLibrarian(_environmentProvider).ForVersion(version).environment;
        }

        /// <exception cref="System.Exception"></exception>
        public override void SetUp()
        {
            base.SetUp();
            _environmentProvider = new Db4oLibraryEnvironmentProvider(PathProvider.TestCasePath
                ());
        }

        /// <exception cref="System.Exception"></exception>
        public override void TearDown()
        {
            _environmentProvider.DisposeAll();
            base.TearDown();
        }

        public class Item
        {
            public string version;

            public Item()
            {
            }

            public Item(string version)
            {
                this.version = version;
            }
        }

        public class Tester
        {
            public virtual void CreateDatabase(string filename)
            {
                WithContainer(filename, new _IFunction4_36());
            }

            public virtual string CurrentVersion(string filename)
            {
                return ((string) WithContainer(filename, new _IFunction4_46(this)));
            }

            public virtual string CurrentVersion(IObjectContainer container)
            {
                return ((Item) container.Query(
                    typeof (Item)).Next()).version;
            }

            private static object WithContainer(string filename, IFunction4 block)
            {
                var container = Db4oFactory.OpenFile(filename);
                try
                {
                    return block.Apply(container);
                }
                finally
                {
                    container.Close();
                }
            }

            private sealed class _IFunction4_36 : IFunction4
            {
                public object Apply(object container)
                {
                    var adapter = ObjectContainerAdapterFactory.ForVersion(1, 1);
                    adapter.ForContainer((IExtObjectContainer) ((IObjectContainer) container));
                    adapter.Store(new Item(Runtime.Substring(Db4oFactory
                        .Version(), 5)));
                    return null;
                }
            }

            private sealed class _IFunction4_46 : IFunction4
            {
                private readonly Tester _enclosing;

                public _IFunction4_46(Tester _enclosing)
                {
                    this._enclosing = _enclosing;
                }

                public object Apply(object container)
                {
                    return _enclosing.CurrentVersion(((IObjectContainer) container));
                }
            }
        }
    }
}