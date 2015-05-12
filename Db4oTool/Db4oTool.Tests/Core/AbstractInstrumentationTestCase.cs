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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Db4objects.Db4o;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Query;
using Db4oUnit;
using Db4oUnit.Extensions.Util;

namespace Db4oTool.Tests.Core
{
    public abstract class AbstractInstrumentationTestCase : ITestSuiteBuilder
    {
        public const string DatabaseFile = "subject.db4o";

        protected virtual bool AcceptsDebugMode
        {
            get { return true; }
        }

        protected virtual bool AcceptsReleaseMode
        {
            get { return true; }
        }

        protected string TestSuiteLabel
        {
            get { return GetType().FullName; }
        }

        protected abstract string[] Resources { get; }

        protected virtual Assembly[] Dependencies
        {
            get
            {
                return new[]
                {
                    typeof (IObjectContainer).Assembly,
                    typeof (Assert).Assembly,
                    typeof (DiagnosticCollector<>).Assembly,
                    GetType().Assembly
                };
            }
        }

        public IEnumerator GetEnumerator()
        {
            try
            {
                return BuildFromInstrumentedAssembly();
            }
            catch (Exception x)
            {
                return new ITest[] {new FailingTest(TestSuiteLabel, x)}.GetEnumerator();
            }
        }

        private IEnumerator BuildFromInstrumentedAssembly()
        {
            return ProduceTestCases().GetEnumerator();
        }

        private IEnumerable<ITest> ProduceTestCases()
        {
            Assert.IsTrue(AcceptsDebugMode || AcceptsReleaseMode);

            if (AcceptsDebugMode)
                foreach (var test in ProduceTestCases(true))
                    yield return test;

            if (AcceptsReleaseMode)
                foreach (var test in ProduceTestCases(false))
                    yield return test;
        }

        private IEnumerable<ITest> ProduceTestCases(bool debugInfo)
        {
            Exception error = null;
            var references = Dependencies;
            foreach (var resource in Resources)
            {
                if (null != error)
                {
                    yield return new FailingTest(resource, error);
                    continue;
                }

                var assemblyPath = EmitAndInstrumentAssemblyFromResource(resource, references, debugInfo, out error);
                if (null != error)
                {
                    yield return new FailingTest(resource, error);
                    error = new Exception("The sibling resource '" + resource + "' has errors.", error);
                    continue;
                }

                var type = GetTestCaseType(assemblyPath, resource);
                IEnumerable suite = type.IsSubclassOf(typeof (InstrumentedTestCase))
                    ? new InstrumentationTestSuiteBuilder(this, type)
                    : new ReflectionTestSuiteBuilder(type);

                foreach (var test in suite)
                {
                    yield return (ITest) test;
                }

                if (ShouldVerify(resource))
                {
                    yield return new VerifyAssemblyTest(assemblyPath);
                }

                references = ArrayServices.Append(references, type.Assembly);
            }
        }

        private string EmitAndInstrumentAssemblyFromResource(string resource, Assembly[] references, bool debugInfo,
            out Exception error)
        {
            string assemblyPath = null;
            try
            {
                CompilationServices.Debug.Using(debugInfo, delegate
                {
                    assemblyPath = EmitAssemblyFromResource(resource, references);
//               		Console.WriteLine("Assembly emitted to: {0}", assemblyPath);
                    Assert.IsTrue(File.Exists(assemblyPath));

                    InstrumentAssembly(assemblyPath);
                });
                error = null;
            }
            catch (Exception x)
            {
                error = x;
            }
            return assemblyPath;
        }

        protected virtual bool ShouldVerify(string resource)
        {
            return true;
        }

        protected abstract void InstrumentAssembly(string location);

        protected virtual void OnQueryExecution(object sender, QueryExecutionEventArgs args)
        {
            throw new NotImplementedException();
        }

        protected virtual void OnQueryOptimizationFailure(object sender, QueryOptimizationFailureEventArgs args)
        {
            throw new ApplicationException(args.Reason.Message, args.Reason);
        }

        private Type GetTestCaseType(string assemblyName, string resource)
        {
            var assembly = Assembly.LoadFrom(assemblyName);
            return assembly.GetType(resource, true);
        }

        private IObjectContainer OpenDatabase()
        {
            if (File.Exists(DatabaseFile)) File.Delete(DatabaseFile);
            var container = Db4oFactory.OpenFile(DatabaseFile);
            var handler = ((ObjectContainerBase) container).GetNativeQueryHandler();
            handler.QueryExecution += OnQueryExecution;
            handler.QueryOptimizationFailure += OnQueryOptimizationFailure;
            return container;
        }

        protected virtual string EmitAssemblyFromResource(string resource, Assembly[] references)
        {
            CopyDependenciesToTemp();
            var resourceName = ResourceServices.CompleteResourceName(GetType(), resource);
            return CompilationServices.EmitAssemblyFromResource(resourceName, references);
        }

        protected virtual void CopyDependenciesToTemp()
        {
            foreach (var dependency in Dependencies)
            {
                ShellUtilities.CopyAssemblyToTemp(dependency);
            }
        }

        private class InstrumentedTestMethod : TestMethod
        {
            private readonly AbstractInstrumentationTestCase _testCase;

            public InstrumentedTestMethod(AbstractInstrumentationTestCase testCase, object subject, MethodInfo method)
                : base(subject, method)
            {
                _testCase = testCase;
            }

            protected override void SetUp()
            {
                SetUpAssemblyResolver();
                SetUpContainer();
                base.SetUp();
            }

            protected override void TearDown()
            {
                try
                {
                    base.TearDown();
                }
                finally
                {
                    TearDownContainer();
                    TearDownAssemblyResolver();
                }
            }

            private void SetUpAssemblyResolver()
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            }

            private void TearDownAssemblyResolver()
            {
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            }

            private void SetUpContainer()
            {
                ((InstrumentedTestCase) GetSubject()).Container = _testCase.OpenDatabase();
            }

            private void TearDownContainer()
            {
                var testCase = (InstrumentedTestCase) GetSubject();
                testCase.Container.Close();
                testCase.Container = null;
            }

            private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().Name == args.Name) return assembly;
                }
                return null;
            }
        }

        private class InstrumentationTestSuiteBuilder : ReflectionTestSuiteBuilder
        {
            private readonly AbstractInstrumentationTestCase _testCase;

            public InstrumentationTestSuiteBuilder(AbstractInstrumentationTestCase testCase, Type clazz)
                : base(clazz)
            {
                _testCase = testCase;
            }

            protected override ITest CreateTest(object instance, MethodInfo method)
            {
                return new InstrumentedTestMethod(_testCase, instance, method);
            }
        }
    }
}