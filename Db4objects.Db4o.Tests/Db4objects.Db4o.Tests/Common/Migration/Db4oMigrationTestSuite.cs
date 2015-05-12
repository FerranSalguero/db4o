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
using Db4objects.Db4o.Tests.Common.Freespace;
using Db4objects.Db4o.Tests.Common.Handlers;
using Db4objects.Db4o.Tests.Util;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Migration
{
    public class Db4oMigrationTestSuite : ITestSuiteBuilder
    {
        public virtual IEnumerator GetEnumerator()
        {
            return new Db4oMigrationSuiteBuilder(TestCases(), Libraries()).GetEnumerator();
        }

        public static void Main(string[] args)
        {
            new ConsoleTestRunner(typeof (Db4oMigrationTestSuite)).Run();
        }

        protected virtual string[] Libraries()
        {
            if (true)
            {
                return Db4oMigrationSuiteBuilder.All;
            }
            if (true)
            {
                // run against specific libraries + the current one
                var javaPath = "db4o.archives/java1.2/db4o-5.7-java1.2.jar";
                var netPath = "db4o.archives/net-2.0/7.4/Db4objects.Db4o.dll";
                return new[] {WorkspaceServices.WorkspacePath(javaPath)};
            }
            return Db4oMigrationSuiteBuilder.Current;
        }

        protected virtual Type[] TestCases()
        {
            Type[] classes =
            {
                typeof (BooleanHandlerUpdateTestCase), typeof (ByteHandlerUpdateTestCase
                    ),
                typeof (CascadedDeleteFileFormatUpdateTestCase), typeof (CharHandlerUpdateTestCase
                    ),
                typeof (DateHandlerUpdateTestCase), typeof (DeletionUponFormatMigrationTestCase
                    ),
                typeof (DoubleHandlerUpdateTestCase), typeof (FloatHandlerUpdateTestCase), typeof (
                    IntHandlerUpdateTestCase),
                typeof (InterfaceHandlerUpdateTestCase), typeof (LongHandlerUpdateTestCase
                    ),
                typeof (MultiDimensionalArrayHandlerUpdateTestCase), typeof (NestedArrayUpdateTestCase
                    ),
                typeof (ObjectArrayUpdateTestCase), typeof (PlainObjectUpdateTestCase), typeof (
                    QueryingMigrationTestCase),
                typeof (ShortHandlerUpdateTestCase), typeof (StringHandlerUpdateTestCase
                    ),
                typeof (IxFreespaceMigrationTestCase), typeof (FreespaceManagerMigrationTestCase
                    ),
                typeof (CommitTimestampMigrationTestCase)
            };
            // EncryptedFileMigrationTestCase.class,  fails the 8.0 build, turned off temporarily
            // Order to run freespace/Encrypted tests last is
            // deliberate. Global configuration Db4o.configure()
            // is changed in the #setUp call and reused.
            return AddJavaTestCases(classes);
        }

        protected virtual Type[] AddJavaTestCases(Type[] classes)
        {
            Type[] javaTestCases = null;
            if (javaTestCases == null)
            {
                return classes;
            }
            var len = javaTestCases.Length;
            var allClasses = new Type[classes.Length + len];
            Array.Copy(javaTestCases, 0, allClasses, 0, len);
            Array.Copy(classes, 0, allClasses, len, classes.Length);
            return allClasses;
        }
    }
}