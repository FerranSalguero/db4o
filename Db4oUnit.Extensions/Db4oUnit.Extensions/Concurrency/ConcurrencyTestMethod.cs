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
using System.Reflection;
using Db4objects.Db4o.Ext;
using Db4oUnit.Extensions.Fixtures;
using Db4oUnit.Fixtures;
using Sharpen;
using Sharpen.Lang;

namespace Db4oUnit.Extensions.Concurrency
{
    public class ConcurrencyTestMethod : TestMethod
    {
        internal Exception[] failures;
        private Thread[] threads;

        public ConcurrencyTestMethod(object instance, MethodInfo method) : base(instance,
            method)
        {
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Invoke()
        {
            var toTest = Subject();
            var method = GetMethod();
            InvokeConcurrencyMethod(toTest, method);
        }

        private AbstractDb4oTestCase Subject()
        {
            return (AbstractDb4oTestCase) GetSubject();
        }

        /// <exception cref="System.Exception"></exception>
        private void InvokeConcurrencyMethod(AbstractDb4oTestCase toTest, MethodInfo method
            )
        {
            var parameters = Runtime.GetParameterTypes(method);
            var hasSequenceParameter = false;
            if (parameters.Length == 2)
            {
                // ExtObjectContainer, seq
                hasSequenceParameter = true;
            }
            var threadCount = toTest.ThreadCount();
            threads = new Thread[threadCount];
            failures = new Exception[threadCount];
            for (var i = 0; i < threadCount; ++i)
            {
                threads[i] = new Thread(new RunnableTestMethod(this, toTest
                    , method, i, hasSequenceParameter), "ConcurrencyTestMethod.invokeConcurrencyMethod Thread["
                                                        + i + "]");
            }
            // start threads simultaneously
            for (var i = 0; i < threadCount; ++i)
            {
                threads[i].Start();
            }
            // wait for the threads to end
            for (var i = 0; i < threadCount; ++i)
            {
                threads[i].Join();
            }
            // check if any of the threads ended abnormally
            for (var i = 0; i < threadCount; ++i)
            {
                if (failures[i] != null)
                {
                    // TODO: show all failures by throwing another kind of exception.
                    throw failures[i];
                }
            }
            // check test result
            CheckConcurrencyMethod(toTest, method.Name);
        }

        /// <exception cref="System.Exception"></exception>
        private void CheckConcurrencyMethod(AbstractDb4oTestCase toTest, string testMethodName
            )
        {
            var checkMethod = CheckMethodFor(toTest.GetType(), testMethodName);
            if (null == checkMethod)
            {
                return;
            }
            // pass ExtObjectContainer as a param to check method
            var oc = Fixture().Db();
            try
            {
                checkMethod.Invoke(toTest, new object[] {oc});
            }
            finally
            {
                oc.Close();
            }
        }

        private MethodInfo CheckMethodFor(Type testClass, string testMethodName)
        {
            try
            {
                Type[] types = {typeof (IExtObjectContainer)};
                return Runtime.GetDeclaredMethod(testClass, ConcurrencyConventions.CheckMethodNameFor
                    (testMethodName), types);
            }
            catch (Exception)
            {
                // if checkMethod is not availble, return as success
                return null;
            }
        }

        internal virtual IMultiSessionFixture Fixture()
        {
            return ((IMultiSessionFixture) AbstractDb4oTestCase.Fixture());
        }

        internal class RunnableTestMethod : Contextful, IRunnable
        {
            private readonly ConcurrencyTestMethod _enclosing;
            private readonly MethodInfo method;
            private readonly int seq;
            private readonly bool showSeq;
            private readonly AbstractDb4oTestCase toTest;

            internal RunnableTestMethod(ConcurrencyTestMethod _enclosing, AbstractDb4oTestCase
                toTest, MethodInfo method, int seq, bool showSeq)
            {
                this._enclosing = _enclosing;
                this.toTest = toTest;
                this.method = method;
                this.seq = seq;
                this.showSeq = showSeq;
            }

            public virtual void Run()
            {
                Run(new _IRunnable_115(this));
            }

            internal virtual void RunMethod()
            {
                IExtObjectContainer oc = null;
                try
                {
                    oc = _enclosing.Fixture().OpenNewSession(toTest);
                    object[] args;
                    if (showSeq)
                    {
                        args = new object[2];
                        args[0] = oc;
                        args[1] = seq;
                    }
                    else
                    {
                        args = new object[1];
                        args[0] = oc;
                    }
                    method.Invoke(toTest, args);
                }
                catch (Exception e)
                {
                    _enclosing.failures[seq] = e;
                }
                finally
                {
                    if (oc != null)
                    {
                        oc.Close();
                    }
                }
            }

            private sealed class _IRunnable_115 : IRunnable
            {
                private readonly RunnableTestMethod _enclosing;

                public _IRunnable_115(RunnableTestMethod _enclosing)
                {
                    this._enclosing = _enclosing;
                }

                public void Run()
                {
                    _enclosing.RunMethod();
                }
            }
        }
    }
}