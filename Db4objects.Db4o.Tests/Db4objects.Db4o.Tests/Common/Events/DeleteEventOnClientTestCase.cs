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
using Db4objects.Db4o.Events;
using Db4oUnit;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Events
{
    public class DeleteEventOnClientTestCase : EventsTestCaseBase, IOptOutSolo
    {
        public static void Main(string[] args)
        {
            new DeleteEventOnClientTestCase().RunAll();
        }

        public virtual void TestAttachingToDeletingEventThrows()
        {
            if (IsEmbedded())
            {
                return;
            }
            Assert.Expect(typeof (ArgumentException), new _ICodeBlock_17(this));
        }

        public virtual void TestAttachingToDeleteEventThrows()
        {
            if (IsEmbedded())
            {
                return;
            }
            Assert.Expect(typeof (ArgumentException), new _ICodeBlock_30(this));
        }

        private sealed class _ICodeBlock_17 : ICodeBlock
        {
            private readonly DeleteEventOnClientTestCase _enclosing;

            public _ICodeBlock_17(DeleteEventOnClientTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.EventRegistry().Deleting += new _IEventListener4_19().OnEvent;
            }

            private sealed class _IEventListener4_19
            {
                public void OnEvent(object sender, CancellableObjectEventArgs
                    args)
                {
                }
            }
        }

        private sealed class _ICodeBlock_30 : ICodeBlock
        {
            private readonly DeleteEventOnClientTestCase _enclosing;

            public _ICodeBlock_30(DeleteEventOnClientTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.EventRegistry().Deleted += new _IEventListener4_32().OnEvent;
            }

            private sealed class _IEventListener4_32
            {
                public void OnEvent(object sender, ObjectInfoEventArgs args
                    )
                {
                }
            }
        }
    }
}