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
using Db4oUnit;
using Sharpen.Lang;

namespace Db4objects.Db4o.Tests.Common.Foundation
{
    public class EnvironmentsTestCase : ITestCase
    {
        // FIXME: db4ounit tests always run in an environment now (required to keep the test executor)
        public virtual void _testNoEnvironment()
        {
            Assert.Expect(typeof (InvalidOperationException), new _ICodeBlock_14());
        }

        public virtual void TestRunWith()
        {
            IWhatever whatever = new _IWhatever_22();
            IEnvironment environment = new _IEnvironment_23(whatever);
            var ran = ByRef.NewInstance();
            Environments.RunWith(environment, new _IRunnable_29(ran, whatever));
            Assert.IsTrue((((bool) ran.value)));
        }

        public virtual void TestNestedEnvironments()
        {
            IWhatever whatever = new _IWhatever_39();
            IEnvironment environment1 = new _IEnvironment_41(whatever);
            IEnvironment environment2 = new _IEnvironment_47();
            Environments.RunWith(environment1, new _IRunnable_53(whatever, environment2));
        }

        public interface IWhatever
        {
        }

        private sealed class _ICodeBlock_14 : ICodeBlock
        {
            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                Environments.My(typeof (IWhatever));
            }
        }

        private sealed class _IWhatever_22 : IWhatever
        {
        }

        private sealed class _IEnvironment_23 : IEnvironment
        {
            private readonly IWhatever whatever;

            public _IEnvironment_23(IWhatever whatever)
            {
                this.whatever = whatever;
            }

            public object Provide(Type service)
            {
                return whatever;
            }
        }

        private sealed class _IRunnable_29 : IRunnable
        {
            private readonly ByRef ran;
            private readonly IWhatever whatever;

            public _IRunnable_29(ByRef ran, IWhatever whatever)
            {
                this.ran = ran;
                this.whatever = whatever;
            }

            public void Run()
            {
                ran.value = true;
                Assert.AreSame(whatever, ((IWhatever) Environments.My(typeof (
                    IWhatever))));
            }
        }

        private sealed class _IWhatever_39 : IWhatever
        {
        }

        private sealed class _IEnvironment_41 : IEnvironment
        {
            private readonly IWhatever whatever;

            public _IEnvironment_41(IWhatever whatever)
            {
                this.whatever = whatever;
            }

            public object Provide(Type service)
            {
                return whatever;
            }
        }

        private sealed class _IEnvironment_47 : IEnvironment
        {
            public object Provide(Type service)
            {
                return null;
            }
        }

        private sealed class _IRunnable_53 : IRunnable
        {
            private readonly IEnvironment environment2;
            private readonly IWhatever whatever;

            public _IRunnable_53(IWhatever whatever, IEnvironment environment2
                )
            {
                this.whatever = whatever;
                this.environment2 = environment2;
            }

            public void Run()
            {
                Assert.AreSame(whatever, ((IWhatever) Environments.My(typeof (
                    IWhatever))));
                Environments.RunWith(environment2, new _IRunnable_56());
                Assert.AreSame(whatever, ((IWhatever) Environments.My(typeof (
                    IWhatever))));
            }

            private sealed class _IRunnable_56 : IRunnable
            {
                public void Run()
                {
                    Assert.IsNull(((IWhatever) Environments.My(typeof (IWhatever
                        ))));
                }
            }
        }
    }
}