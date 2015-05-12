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

using Db4objects.Db4o.Tests.Common.Soda.Util;

namespace Db4objects.Db4o.Tests.Common.Soda.Classes.Untypedhierarchy
{
    /// <summary>
    ///     epaul:
    ///     Shows a bug.
    /// </summary>
    /// <remarks>
    ///     epaul:
    ///     Shows a bug.
    ///     carlrosenberger:
    ///     Fixed!
    ///     The error was due to the the behaviour of STCompare.java.
    ///     It compared the syntetic fields in inner classes also.
    ///     I changed the behaviour to neglect all fields that
    ///     contain a "$".
    /// </remarks>
    /// <author>
    ///     <a href="mailto:Paul-Ebermann@gmx.de">Paul Ebermann</a>
    /// </author>
    /// <version>0.1</version>
    public class STInnerClassesTestCase : SodaBaseTestCase
    {
        public override object[] CreateData()
        {
            return new object[]
            {
                new Parent(this, new Child
                    (this, "Example")),
                new Parent(this, new Child
                    (this, "no Example"))
            };
        }

        /// <summary>Only</summary>
        public virtual void TestNothing()
        {
            var q = NewQuery();
            q.Descend("child");
            SodaTestUtil.Expect(q, _array);
        }

        public class Parent
        {
            private readonly STInnerClassesTestCase _enclosing;
            public object child;

            public Parent(STInnerClassesTestCase _enclosing, object o)
            {
                this._enclosing = _enclosing;
                // Generierter package-Name
                child = o;
            }

            public Parent(STInnerClassesTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public override string ToString()
            {
                return "Parent[" + child + "]";
            }
        }

        public class Child
        {
            private readonly STInnerClassesTestCase _enclosing;
            public object childFirst;

            public Child(STInnerClassesTestCase _enclosing, object o)
            {
                this._enclosing = _enclosing;
                childFirst = o;
            }

            public Child(STInnerClassesTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public override string ToString()
            {
                return "Child[" + childFirst + "]";
            }
        }
    }
}