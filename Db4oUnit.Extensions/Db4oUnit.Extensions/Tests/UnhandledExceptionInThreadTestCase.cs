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
using Db4objects.Db4o.Internal;
using Db4oUnit.Extensions.Fixtures;
using Sharpen.Lang;

namespace Db4oUnit.Extensions.Tests
{
    public class UnhandledExceptionInThreadTestCase : ITestCase
    {
        public virtual void TestSolo()
        {
            var suite = new Db4oTestSuiteBuilder(new Db4oInMemory(), typeof (
                ExceptionThrowingTestCase));
            var result = new TestResult();
            new TestRunner(suite).Run(result);
            Assert.AreEqual(1, result.Failures.Count);
        }

        public class ExceptionThrowingTestCase : AbstractDb4oTestCase
        {
            public virtual void Test()
            {
                Container().ThreadPool().Start(ReflectPlatform.SimpleName(typeof (UnhandledExceptionInThreadTestCase
                    )) + " Throwing Exception Thread", new _IRunnable_15());
            }

            private sealed class _IRunnable_15 : IRunnable
            {
                public void Run()
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}