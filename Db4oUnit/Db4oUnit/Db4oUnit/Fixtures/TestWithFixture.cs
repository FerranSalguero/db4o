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
using Sharpen.Lang;

namespace Db4oUnit.Fixtures
{
    public sealed class TestWithFixture : ITest
    {
        private readonly string _fixtureLabel;
        private readonly ITest _test;
        private readonly object _value;
        private readonly FixtureVariable _variable;

        public TestWithFixture(ITest test, FixtureVariable fixtureVariable, object fixtureValue
            ) : this(test, null, fixtureVariable, fixtureValue)
        {
        }

        public TestWithFixture(ITest test, string fixtureLabel, FixtureVariable fixtureVariable
            , object fixtureValue)
        {
            _test = test;
            _fixtureLabel = fixtureLabel;
            _variable = fixtureVariable;
            _value = fixtureValue;
        }

        public string Label()
        {
            var label = new ObjectByRef();
            RunDecorated(new _IRunnable_26(this, label));
            return (string) label.value;
        }

        public void Run()
        {
            RunDecorated(_test);
        }

        public bool IsLeafTest()
        {
            var isLeaf = new BooleanByRef();
            RunDecorated(new _IRunnable_58(this, isLeaf));
            return isLeaf.value;
        }

        public ITest Transmogrify(IFunction4 fun)
        {
            return ((ITest) fun.Apply(this));
        }

        public ITest Test()
        {
            return _test;
        }

        private void RunDecorated(IRunnable block)
        {
            _variable.With(Value(), block);
        }

        private object Value()
        {
            return _value is IDeferred4 ? ((IDeferred4) _value).Value() : _value;
        }

        private object FixtureLabel()
        {
            return (_fixtureLabel == null ? _value : _fixtureLabel);
        }

        private sealed class _IRunnable_26 : IRunnable
        {
            private readonly TestWithFixture _enclosing;
            private readonly ObjectByRef label;

            public _IRunnable_26(TestWithFixture _enclosing, ObjectByRef label)
            {
                this._enclosing = _enclosing;
                this.label = label;
            }

            public void Run()
            {
                label.value = "(" + _enclosing.FixtureLabel() + ") " + _enclosing._test
                    .Label();
            }
        }

        private sealed class _IRunnable_58 : IRunnable
        {
            private readonly TestWithFixture _enclosing;
            private readonly BooleanByRef isLeaf;

            public _IRunnable_58(TestWithFixture _enclosing, BooleanByRef isLeaf)
            {
                this._enclosing = _enclosing;
                this.isLeaf = isLeaf;
            }

            public void Run()
            {
                isLeaf.value = _enclosing._test.IsLeafTest();
            }
        }
    }
}