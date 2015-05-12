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

using Db4objects.Db4o.Config;
using Db4objects.Db4o.Internal.Activation;
using Db4objects.Db4o.TA;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Staging
{
    public class TAUnavailableClassAtServer : AbstractDb4oTestCase, ICustomClientServerConfiguration
        , IOptOutAllButNetworkingCS
    {
        /// <exception cref="System.Exception"></exception>
        public virtual void ConfigureServer(IConfiguration config)
        {
            config.ReflectWith(new ExcludingReflector(new[]
            {
                typeof (Child
                    ),
                typeof (ParentWithMultipleChilds), typeof (ParentWithSingleChild
                    )
            }));
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void ConfigureClient(IConfiguration config)
        {
            config.Add(new TransparentActivationSupport());
        }

        public static void Main(string[] args)
        {
            new TAUnavailableClassAtServer().RunNetworking();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(new ParentWithMultipleChilds(this, new[] {new Child(this, 42)}));
            Store(new ParentWithSingleChild(this, new Child
                (this, 43)));
        }

        public virtual void TestChildArray()
        {
            var client1 = OpenNewSession();
            var query = client1.Query();
            query.Constrain(typeof (ParentWithMultipleChilds));
            var result = query.Execute();
            Assert.IsTrue(result.HasNext());
            var parent = (ParentWithMultipleChilds
                ) result.Next();
            Assert.IsNotNull(parent.Children());
            client1.Close();
        }

        public virtual void TestSingleChild()
        {
            var client1 = OpenNewSession();
            var query = client1.Query();
            query.Constrain(typeof (ParentWithSingleChild));
            var result = query.Execute();
            Assert.IsTrue(result.HasNext());
            var parent = (ParentWithSingleChild
                ) result.Next();
            Assert.AreEqual(43, parent.Child().Value());
            client1.Close();
        }

        public class ParentWithMultipleChilds
        {
            private readonly TAUnavailableClassAtServer _enclosing;

            private Child[] _children = new Child
                [0];

            public ParentWithMultipleChilds(TAUnavailableClassAtServer _enclosing, Child
                [] children)
            {
                this._enclosing = _enclosing;
                _children = children;
            }

            public virtual Child[] Children()
            {
                return _children;
            }

            public virtual void Children(Child[] children)
            {
                _children = children;
            }
        }

        public class ParentWithSingleChild
        {
            private readonly TAUnavailableClassAtServer _enclosing;
            private Child _child;

            public ParentWithSingleChild(TAUnavailableClassAtServer _enclosing, Child
                child)
            {
                this._enclosing = _enclosing;
                _child = child;
            }

            public virtual Child Child()
            {
                return _child;
            }

            public virtual void Child(Child child)
            {
                _child = child;
            }
        }

        public class Child : ActivatableBase
        {
            private readonly TAUnavailableClassAtServer _enclosing;
            private int _value;

            public Child(TAUnavailableClassAtServer _enclosing, int value)
            {
                this._enclosing = _enclosing;
                _value = value;
            }

            public virtual int Value()
            {
                ActivateForRead();
                return _value;
            }

            public virtual void Value(int value)
            {
                ActivateForWrite();
                _value = value;
            }
        }
    }
}