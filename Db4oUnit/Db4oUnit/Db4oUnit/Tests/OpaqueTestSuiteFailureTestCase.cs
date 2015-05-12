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

using Db4objects.Db4o.Foundation;

namespace Db4oUnit.Tests
{
    public class OpaqueTestSuiteFailureTestCase : ITestCase
    {
        public virtual void TestFailOnSetup()
        {
            var tearDownCalled = new BooleanByRef();
            var result = new TestResult();
            new TestRunner(Iterators.Iterable(new[]
            {
                new FailingTestSuite(true, false, tearDownCalled
                    )
            })).Run(result);
            Assert.AreEqual(0, result.TestCount);
            Assert.AreEqual(1, result.Failures.Count);
            Assert.IsFalse(tearDownCalled.value);
        }

        public virtual void TestFailOnTearDown()
        {
            var tearDownCalled = new BooleanByRef();
            var result = new TestResult();
            new TestRunner(Iterators.Iterable(new[]
            {
                new FailingTestSuite(false, true, tearDownCalled
                    )
            })).Run(result);
            Assert.AreEqual(1, result.TestCount);
            Assert.AreEqual(2, result.Failures.Count);
            Assert.IsTrue(tearDownCalled.value);
        }

        public class FailingTestSuite : OpaqueTestSuiteBase
        {
            private readonly bool _failOnSetUp;
            private readonly bool _failOnTeardown;
            private readonly BooleanByRef _tearDownCalled;

            public FailingTestSuite(bool failOnSetup, bool failOnTeardown, BooleanByRef tearDownCalled
                ) : this(failOnSetup, failOnTeardown, tearDownCalled, new _IClosure4_34())
            {
            }

            private FailingTestSuite(bool failOnSetup, bool failOnTeardown, BooleanByRef tearDownCalled
                , IClosure4 tests) : base(tests)
            {
                _failOnSetUp = failOnSetup;
                _failOnTeardown = failOnTeardown;
                _tearDownCalled = tearDownCalled;
            }

            /// <exception cref="System.Exception"></exception>
            protected override void SuiteSetUp()
            {
                if (_failOnSetUp)
                {
                    Assert.Fail();
                }
            }

            /// <exception cref="System.Exception"></exception>
            protected override void SuiteTearDown()
            {
                _tearDownCalled.value = true;
                if (_failOnTeardown)
                {
                    Assert.Fail();
                }
            }

            protected override OpaqueTestSuiteBase Transmogrified(IClosure4 tests)
            {
                return new FailingTestSuite(_failOnSetUp, _failOnTeardown
                    , _tearDownCalled, tests);
            }

            public override string Label()
            {
                return GetType().FullName;
            }

            private sealed class _IClosure4_34 : IClosure4
            {
                public object Run()
                {
                    return Iterators.Iterate(new[]
                    {
                        new FailingTest("fail", new AssertionException
                            ("fail"))
                    });
                }
            }
        }
    }
}