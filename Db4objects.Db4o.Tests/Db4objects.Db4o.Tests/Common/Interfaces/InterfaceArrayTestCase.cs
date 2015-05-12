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

using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Interfaces
{
    public class InterfaceArrayTestCase : AbstractDb4oTestCase
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(new Bar(new IFoo[]
            {
                new FooImpl
                    ()
            }));
        }

        public virtual void Test()
        {
            Assert.AreEqual(1, ((Bar) RetrieveOnlyInstance(typeof (Bar
                ))).foos.Length);
        }

        public interface IFoo
        {
        }

        public class FooImpl : IFoo
        {
        }

        public class Bar
        {
            public IFoo[] foos;

            public Bar(IFoo[] foos)
            {
                this.foos = foos;
            }
        }
    }
}