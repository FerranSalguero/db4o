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
using Db4oUnit.Fixtures;
using Sharpen.Lang;

namespace Db4oUnit
{
    public class ContextfulTest : Contextful, ITest
    {
        private readonly ITestFactory _factory;

        public ContextfulTest(ITestFactory factory)
        {
            _factory = factory;
        }

        public virtual string Label()
        {
            return (string) Run(new _IClosure4_18(this));
        }

        public virtual bool IsLeafTest()
        {
            return ((bool) Run(new _IClosure4_26(this)));
        }

        public virtual void Run()
        {
            Run(new _IRunnable_34(this));
        }

        public virtual ITest Transmogrify(IFunction4 fun)
        {
            return ((ITest) fun.Apply(this));
        }

        private ITest TestInstance()
        {
            return _factory.NewInstance();
        }

        private sealed class _IClosure4_18 : IClosure4
        {
            private readonly ContextfulTest _enclosing;

            public _IClosure4_18(ContextfulTest _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public object Run()
            {
                return _enclosing.TestInstance().Label();
            }
        }

        private sealed class _IClosure4_26 : IClosure4
        {
            private readonly ContextfulTest _enclosing;

            public _IClosure4_26(ContextfulTest _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public object Run()
            {
                return _enclosing.TestInstance().IsLeafTest();
            }
        }

        private sealed class _IRunnable_34 : IRunnable
        {
            private readonly ContextfulTest _enclosing;

            public _IRunnable_34(ContextfulTest _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Run()
            {
                _enclosing.TestInstance().Run();
            }
        }
    }
}