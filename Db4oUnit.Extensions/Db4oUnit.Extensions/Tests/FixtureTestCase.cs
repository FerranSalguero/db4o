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
using Db4objects.Db4o.Foundation;
using Db4oUnit.Extensions.Fixtures;
using Db4oUnit.Extensions.Util;
using Db4oUnit.Mocking;
using Db4oUnit.Tests;
using Sharpen.Lang;

namespace Db4oUnit.Extensions.Tests
{
    public class FixtureTestCase : ITestCase
    {
        public virtual void TestSingleTestWithDifferentFixtures()
        {
            AssertSimpleDb4o(new Db4oInMemory());
            AssertSimpleDb4o(new Db4oSolo());
        }

        public virtual void TestMultipleTestsSingleFixture()
        {
            MultipleDb4oTestCase.ResetConfigureCalls();
            FrameworkTestCase.RunTestAndExpect(new Db4oTestSuiteBuilder(new Db4oInMemory(), typeof (
                MultipleDb4oTestCase)), 2, false);
            Assert.AreEqual(2, MultipleDb4oTestCase.ConfigureCalls());
        }

        public virtual void TestSelectiveFixture()
        {
            IDb4oFixture fixture = new ExcludingInMemoryFixture(this);
            var tests = new Db4oTestSuiteBuilder(fixture, new[]
            {
                typeof (AcceptedTestCase
                    ),
                typeof (NotAcceptedTestCase)
            }).GetEnumerator();
            var test = NextTest(tests);
            Assert.IsFalse(tests.MoveNext());
            FrameworkTestCase.RunTestAndExpect(test, 0);
        }

        private void AssertSimpleDb4o(IDb4oFixture fixture)
        {
            var tests = new Db4oTestSuiteBuilder(fixture, typeof (SimpleDb4oTestCase))
                .GetEnumerator();
            var test = NextTest(tests);
            var recorder = new MethodCallRecorder();
            SimpleDb4oTestCase.RecorderVariable.With(recorder, new _IRunnable_46(test));
            recorder.Verify(new[]
            {
                new MethodCall("fixture", new object[]
                {
                    fixture
                }),
                new MethodCall("configure", new[] {MethodCall.IgnoredArgument}), new
                    MethodCall("store", new object[] {}),
                new MethodCall("testResultSize", new object
                    [] {})
            });
        }

        private ITest NextTest(IEnumerator tests)
        {
            return (ITest) Iterators.Next(tests);
        }

        public virtual void TestInterfaceIsAvailable()
        {
            Assert.IsTrue(typeof (IDb4oTestCase).IsAssignableFrom(typeof (AbstractDb4oTestCase)
                ));
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestDeleteDir()
        {
            Directory.CreateDirectory("a/b/c");
            Assert.IsTrue(File.Exists("a"));
            IOUtil.DeleteDir("a");
            Assert.IsFalse(File.Exists("a"));
        }

        private sealed class ExcludingInMemoryFixture : Db4oInMemory
        {
            private readonly FixtureTestCase _enclosing;

            internal ExcludingInMemoryFixture(FixtureTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public override bool Accept(Type clazz)
            {
                return !typeof (IOptOutFromTestFixture).IsAssignableFrom(clazz);
            }
        }

        private sealed class _IRunnable_46 : IRunnable
        {
            private readonly ITest test;

            public _IRunnable_46(ITest test)
            {
                this.test = test;
            }

            public void Run()
            {
                FrameworkTestCase.RunTestAndExpect(test, 0);
            }
        }
    }
}