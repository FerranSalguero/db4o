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
using Db4oUnit.Mocking;

namespace Db4oUnit.Tests
{
    public class CompositeTestListenerTestCase : ITestCase
    {
        public virtual void Test()
        {
            var recorder = new MethodCallRecorder();
            var listener = new CompositeTestListener(new ListenerRecorder
                ("first", recorder), new ListenerRecorder("second"
                    , recorder));
            var test = new RunsGreen();
            var failure = new Exception();
            listener.RunStarted();
            listener.TestStarted(test);
            listener.TestFailed(test, failure);
            listener.RunFinished();
            recorder.Verify(new[]
            {
                Call("first.runStarted"), Call("second.runStarted"
                    ),
                Call("first.testStarted", test), Call("second.testStarted", test), Call("first.testFailed"
                    , test, failure),
                Call("second.testFailed", test, failure), Call("first.runFinished"
                    ),
                Call("second.runFinished")
            });
        }

        private MethodCall Call(string method, object arg0, Exception arg1)
        {
            return new MethodCall(method, new[] {arg0, arg1});
        }

        private MethodCall Call(string method, object arg)
        {
            return new MethodCall(method, new[] {arg});
        }

        private MethodCall Call(string method)
        {
            return new MethodCall(method, new object[] {});
        }

        internal sealed class ListenerRecorder : ITestListener
        {
            private readonly string _label;
            private readonly MethodCallRecorder _recorder;

            public ListenerRecorder(string label, MethodCallRecorder recorder)
            {
                _label = label;
                _recorder = recorder;
            }

            public void RunFinished()
            {
                Record("runFinished");
            }

            public void RunStarted()
            {
                Record("runStarted");
            }

            public void TestFailed(ITest test, Exception failure)
            {
                Record("testFailed", new object[] {test, failure});
            }

            public void TestStarted(ITest test)
            {
                Record("testStarted", new object[] {test});
            }

            public void Failure(string msg, Exception failure)
            {
                Record("failure", new object[] {msg, failure});
            }

            private void Record(string name)
            {
                Record(name, new object[0]);
            }

            private void Record(string name, object[] args)
            {
                _recorder.Record(new MethodCall(_label + "." + name, args));
            }
        }
    }
}