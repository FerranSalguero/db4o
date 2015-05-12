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
using Db4objects.Db4o.Foundation;
using Db4oUnit.Fixtures;
using Db4oUnit.Mocking;

namespace Db4oUnit.Tests.Fixtures
{
    public class FixtureBasedTestSuiteTestCase : ITestCase
    {
        internal static FixtureVariable RecorderFixture = FixtureVariable.NewInstance("recorder"
            );

        internal static FixtureVariable Fixture1 = new FixtureVariable("f1");
        internal static FixtureVariable Fixture2 = new FixtureVariable("f2");

        public virtual void Test()
        {
            var recorder = new MethodCallRecorder();
            Run(new _FixtureBasedTestSuite_45(recorder));
            //		System.out.println(CodeGenerator.generateMethodCallArray(recorder));
            recorder.Verify(new[]
            {
                new MethodCall("testFoo", new object[]
                {
                    "f11"
                    , "f21"
                }),
                new MethodCall("testFoo", new object[] {"f11", "f22"}), new MethodCall
                    ("testFoo", new object[] {"f12", "f21"}),
                new MethodCall("testFoo", new object
                    [] {"f12", "f22"}),
                new MethodCall("testBar", new object[] {"f11", "f21"}),
                new MethodCall("testBar", new object[] {"f11", "f22"}), new MethodCall("testBar"
                    , new object[] {"f12", "f21"}),
                new MethodCall("testBar", new object[]
                {
                    "f12"
                    , "f22"
                })
            });
        }

        public virtual void TestCombinationToRun()
        {
            var recorder = new MethodCallRecorder();
            Run(new _FixtureBasedTestSuite_78(recorder));
            //		System.out.println(CodeGenerator.generateMethodCallArray(recorder));
            recorder.Verify(new[]
            {
                new MethodCall("testFoo", new object[]
                {
                    "f11"
                    , "f22"
                }),
                new MethodCall("testBar", new object[] {"f11", "f22"})
            });
        }

        public virtual void TestInvalidCombinationToRun()
        {
            Assert.Expect(typeof (AssertionException), new _ICodeBlock_107(this));
        }

        private void RunInvalidCombination()
        {
            Run(new _FixtureBasedTestSuite_115());
        }

        private void Run(FixtureBasedTestSuite suite)
        {
            var result = new TestResult();
            new TestRunner(suite).Run(result);
            if (result.Failures.Count > 0)
            {
                Assert.Fail(Iterators.ToString(result.Failures));
            }
        }

        public virtual void TestLabel()
        {
            FixtureBasedTestSuite suite = new _FixtureBasedTestSuite_142();
            var labels = Iterators.Map(suite, new _IFunction4_154());
            Iterator4Assert.AreEqual(new object[]
            {
                TestLabel("testFoo", 0, 0), TestLabel("testFoo"
                    , 1, 0),
                TestLabel("testFoo", 0, 1), TestLabel("testFoo", 1, 1), TestLabel("testBar"
                    , 0, 0),
                TestLabel("testBar", 1, 0), TestLabel("testBar", 0, 1), TestLabel("testBar"
                    , 1, 1)
            }, labels.GetEnumerator());
        }

        private string TestLabel(string testMethod, int fixture1Index, int fixture2Index)
        {
            var prefix = "(f2[" + fixture1Index + "]) (f1[" + fixture2Index + "]) ";
            return prefix + typeof (TestUnit).FullName + "." + testMethod;
        }

        public sealed class TestUnit : ITestCase
        {
            private readonly object fixture1 = Fixture1.Value;
            private readonly object fixture2 = Fixture2.Value;

            public void TestFoo()
            {
                Record("testFoo");
            }

            public void TestBar()
            {
                Record("testBar");
            }

            private void Record(string test)
            {
                Recorder().Record(new MethodCall(test, new[] {fixture1, fixture2}));
            }

            private MethodCallRecorder Recorder()
            {
                return ((MethodCallRecorder) RecorderFixture.Value);
            }
        }

        private sealed class _FixtureBasedTestSuite_45 : FixtureBasedTestSuite
        {
            private readonly MethodCallRecorder recorder;

            public _FixtureBasedTestSuite_45(MethodCallRecorder recorder)
            {
                this.recorder = recorder;
            }

            public override IFixtureProvider[] FixtureProviders()
            {
                return new IFixtureProvider[]
                {
                    new SimpleFixtureProvider(RecorderFixture, new object[] {recorder}),
                    new SimpleFixtureProvider(Fixture1, new object[] {"f11", "f12"}),
                    new SimpleFixtureProvider(Fixture2, new object[] {"f21", "f22"})
                };
            }

            public override Type[] TestUnits()
            {
                return new[] {typeof (TestUnit)};
            }
        }

        private sealed class _FixtureBasedTestSuite_78 : FixtureBasedTestSuite
        {
            private readonly MethodCallRecorder recorder;

            public _FixtureBasedTestSuite_78(MethodCallRecorder recorder)
            {
                this.recorder = recorder;
            }

            public override IFixtureProvider[] FixtureProviders()
            {
                return new IFixtureProvider[]
                {
                    new SimpleFixtureProvider(RecorderFixture, new object[] {recorder}),
                    new SimpleFixtureProvider(Fixture1, new object[] {"f11", "f12"}),
                    new SimpleFixtureProvider(Fixture2, new object[] {"f21", "f22"})
                };
            }

            public override Type[] TestUnits()
            {
                return new[] {typeof (TestUnit)};
            }

            public override int[] CombinationToRun()
            {
                return new[] {0, 0, 1};
            }
        }

        private sealed class _ICodeBlock_107 : ICodeBlock
        {
            private readonly FixtureBasedTestSuiteTestCase _enclosing;

            public _ICodeBlock_107(FixtureBasedTestSuiteTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Run()
            {
                _enclosing.RunInvalidCombination();
            }
        }

        private sealed class _FixtureBasedTestSuite_115 : FixtureBasedTestSuite
        {
            public override IFixtureProvider[] FixtureProviders()
            {
                return new IFixtureProvider[]
                {
                    new SimpleFixtureProvider(Fixture1, new object[] {"f11", "f12"}),
                    new SimpleFixtureProvider(Fixture2, new object[] {"f21", "f22"})
                };
            }

            public override Type[] TestUnits()
            {
                return new[] {typeof (TestUnit)};
            }

            public override int[] CombinationToRun()
            {
                return new[] {0};
            }
        }

        private sealed class _FixtureBasedTestSuite_142 : FixtureBasedTestSuite
        {
            public override IFixtureProvider[] FixtureProviders()
            {
                return new IFixtureProvider[]
                {
                    new SimpleFixtureProvider(Fixture1, new object[] {"f11", "f12"}),
                    new SimpleFixtureProvider(Fixture2, new object[] {"f21", "f22"})
                };
            }

            public override Type[] TestUnits()
            {
                return new[] {typeof (TestUnit)};
            }
        }

        private sealed class _IFunction4_154 : IFunction4
        {
            public object Apply(object arg)
            {
                return ((ITest) arg).Label();
            }
        }
    }
}