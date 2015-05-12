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

using System.IO;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation.IO;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Tests.Util;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;
using Db4oUnit.Extensions.Util;
using Sharpen;

namespace Db4objects.Db4o.Tests.Common.Exceptions
{
    /// <exclude></exclude>
    public class OldFormatExceptionTestCase : ITestCase, IOptOutNoFileSystemData, IOptOutWorkspaceIssue
    {
        public static void Main(string[] args)
        {
            new ConsoleTestRunner(typeof (OldFormatExceptionTestCase)).Run();
        }

        // It is also regression test for COR-634.
        /// <exception cref="System.Exception"></exception>
        public virtual void Test()
        {
            if (WorkspaceServices.WorkspaceRoot == null)
            {
                Runtime.Err.WriteLine("Build environment not available. Skipping test case..."
                    );
                return;
            }
            if (!File.Exists(SourceFile()))
            {
                Runtime.Err.WriteLine("Test source file " + SourceFile() + " not available. Skipping test case..."
                    );
                return;
            }
            var oldDatabaseFilePath = OldDatabaseFilePath();
            Assert.Expect(typeof (OldFormatException), new _ICodeBlock_43(this, oldDatabaseFilePath
                ));
            IObjectContainer container = null;
            try
            {
                container = Db4oFactory.OpenFile(NewConfiguration(true), oldDatabaseFilePath);
            }
            finally
            {
                if (container != null)
                {
                    container.Close();
                }
            }
        }

        private IConfiguration NewConfiguration(bool allowVersionUpdates)
        {
            var config = Db4oFactory.NewConfiguration();
            config.ReflectWith(Platform4.ReflectorForType(typeof (OldFormatExceptionTestCase))
                );
            config.AllowVersionUpdates(allowVersionUpdates);
            return config;
        }

        /// <exception cref="System.IO.IOException"></exception>
        protected virtual string OldDatabaseFilePath()
        {
            var oldFile = IOServices.BuildTempPath("old_db.db4o");
            File4.Copy(SourceFile(), oldFile);
            return oldFile;
        }

        private string SourceFile()
        {
            return WorkspaceServices.WorkspaceTestFilePath("db4oVersions/db4o_3.0.3");
        }

        private sealed class _ICodeBlock_43 : ICodeBlock
        {
            private readonly OldFormatExceptionTestCase _enclosing;
            private readonly string oldDatabaseFilePath;

            public _ICodeBlock_43(OldFormatExceptionTestCase _enclosing, string oldDatabaseFilePath
                )
            {
                this._enclosing = _enclosing;
                this.oldDatabaseFilePath = oldDatabaseFilePath;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                Db4oFactory.OpenFile(_enclosing.NewConfiguration(false), oldDatabaseFilePath
                    );
            }
        }
    }
}