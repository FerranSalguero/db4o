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

#if !SILVERLIGHT
using System;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Qlin;
using Db4oUnit;
using Sharpen;

namespace Db4objects.Db4o.Tests.Common.Qlin
{
    public class PrototypesTestCase : ITestLifeCycle
    {
        private const bool IgnoreTransientFields = true;
        private const int RecursionDepth = 10;
        private Prototypes _prototypes;

        /// <exception cref="System.Exception"></exception>
        public virtual void SetUp()
        {
            _prototypes = new Prototypes(Prototypes.DefaultReflector(), RecursionDepth, IgnoreTransientFields
                );
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TearDown()
        {
        }

        public virtual void TestStringField()
        {
            var item = ((Item) Prototype(typeof (Item
                )));
            AssertPath(item, item._name, new[] {"_name"});
        }

        public virtual void TestStringMethod()
        {
            var item = ((Item) Prototype(typeof (Item
                )));
            AssertPath(item, item.Name(), new[] {"_name"});
        }

        public virtual void TestInstanceField()
        {
            var item = ((Item) Prototype(typeof (Item
                )));
            AssertPath(item, item._child, new[] {"_child"});
        }

        public virtual void TestInstanceMethod()
        {
            var item = ((Item) Prototype(typeof (Item
                )));
            AssertPath(item, item.Child(), new[] {"_child"});
        }

        public virtual void TestLevel2()
        {
            var item = ((Item) Prototype(typeof (Item
                )));
            AssertPath(item, item.Child().Name(), new[] {"_child", "_name"});
        }

        public virtual void TestCallingOwnFramework()
        {
            var testCase = ((PrototypesTestCase) Prototype(typeof (PrototypesTestCase
                )));
            AssertPath(testCase, testCase._prototypes, new[] {"_prototypes"});
        }

        public virtual void TestWildToString()
        {
            var testCase = ((PrototypesTestCase) Prototype(typeof (PrototypesTestCase
                )));
            AssertIsNull(testCase, testCase._prototypes.ToString());
        }

        // keep this method, it's helpful for new tests
        private void Print(object t, object expression)
        {
            var path = _prototypes.BackingFieldPath(t.GetType(), expression
                );
            if (path == null)
            {
                Print("null");
                return;
            }
            Print(Iterators.Join(path, "[", "]", ", "));
        }

        private void Print(string @string)
        {
            Runtime.Out.WriteLine(@string);
        }

        private void AssertIsNull(object t, object expression)
        {
            Assert.IsNull(_prototypes.BackingFieldPath(t.GetType(), expression));
        }

        private void AssertPath(object t, object expression, string[] expected)
        {
            var path = _prototypes.BackingFieldPath(t.GetType(), expression
                );
            // print(Iterators.join(path, "[", "]", ", "));
            path.Reset();
            Iterator4Assert.AreEqual(expected, path);
        }

        private object Prototype(Type clazz)
        {
            return _prototypes.PrototypeForClass(clazz);
        }

        public class Item
        {
            public Item _child;
            public string _name;
            public int myInt;

            public virtual string Name()
            {
                return _name;
            }

            public virtual Item Child()
            {
                return _child;
            }

            public override string ToString()
            {
                var str = "Item " + _name;
                if (_child != null)
                {
                    str += "\n  " + _child;
                }
                return str;
            }
        }
    }
}

#endif // !SILVERLIGHT