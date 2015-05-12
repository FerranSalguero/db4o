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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Tests.Common.Api;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Config
{
    public class GlobalVsNonStaticConfigurationTestCase : Db4oTestWithTempFile
    {
        public static void Main(string[] args)
        {
            new ConsoleTestRunner(typeof (GlobalVsNonStaticConfigurationTestCase)).Run();
        }

        public virtual void TestOpenWithNonStaticConfiguration()
        {
            var config1 = NewConfiguration();
            config1.File.ReadOnly = true;
            Assert.Expect(typeof (DatabaseReadOnlyException), new _ICodeBlock_30(this, config1
                ));
            var config2 = NewConfiguration();
            IObjectContainer db2 = Db4oEmbedded.OpenFile(config2, TempFile());
            try
            {
                db2.Store(new Data(2));
                Assert.AreEqual(1, db2.Query(typeof (Data))
                    .Count);
            }
            finally
            {
                db2.Close();
            }
        }

#if !SILVERLIGHT
        [Obsolete(@"using deprecated api")]
        public virtual void TestOpenWithStaticConfiguration()
        {
            Db4oFactory.Configure().ReadOnly(true);
            Assert.Expect(typeof (DatabaseReadOnlyException), new _ICodeBlock_53(this));
            Db4oFactory.Configure().ReadOnly(false);
            var db = Db4oFactory.OpenFile(TempFile());
            db.Store(new Data(1));
            db.Close();
            db = Db4oFactory.OpenFile(TempFile());
            Assert.AreEqual(1, db.Query(typeof (Data)).
                Count);
            db.Close();
        }
#endif // !SILVERLIGHT

        public virtual void TestIndependentObjectConfigs()
        {
            var config = NewConfiguration();
            var objectConfig = config.Common.ObjectClass(typeof (Data
                ));
            objectConfig.Translate(new TNull());
            var otherConfig = NewConfiguration();
            Assert.AreNotSame(config, otherConfig);
            var otherObjectConfig = (Config4Class) otherConfig.Common.ObjectClass(typeof (
                Data));
            Assert.AreNotSame(objectConfig, otherObjectConfig);
            Assert.IsNull(otherObjectConfig.GetTranslator());
        }

        public class Data
        {
            public int id;

            public Data(int id)
            {
                this.id = id;
            }
        }

        private sealed class _ICodeBlock_30 : ICodeBlock
        {
            private readonly GlobalVsNonStaticConfigurationTestCase _enclosing;
            private readonly IEmbeddedConfiguration config1;

            public _ICodeBlock_30(GlobalVsNonStaticConfigurationTestCase _enclosing, IEmbeddedConfiguration
                config1)
            {
                this._enclosing = _enclosing;
                this.config1 = config1;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                Db4oEmbedded.OpenFile(config1, _enclosing.TempFile());
            }
        }

        private sealed class _ICodeBlock_53 : ICodeBlock
        {
            private readonly GlobalVsNonStaticConfigurationTestCase _enclosing;

            public _ICodeBlock_53(GlobalVsNonStaticConfigurationTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                Db4oFactory.OpenFile(_enclosing.TempFile());
            }
        }
    }
}