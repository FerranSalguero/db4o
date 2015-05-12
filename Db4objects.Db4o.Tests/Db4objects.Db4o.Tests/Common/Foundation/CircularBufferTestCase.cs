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

namespace Db4objects.Db4o.Tests.Common.Foundation
{
    public class CircularBufferTestCase : ITestCase
    {
        private const int BufferSize = 4;
        internal readonly CircularBuffer4 buffer = new CircularBuffer4(BufferSize);

        public virtual void TestAddFirstRemoveLast()
        {
            for (var i = 1; i < 11; ++i)
            {
                buffer.AddFirst(i);
                AssertRemoveLast(i);
            }
        }

        public virtual void TestAddFirstBounds()
        {
            FillBuffer();
            AssertIllegalAddFirst();
            buffer.RemoveLast();
            buffer.AddFirst(5);
            AssertIllegalAddFirst();
            buffer.RemoveLast();
            buffer.AddFirst(6);
        }

        private void AssertIllegalAddFirst()
        {
            Assert.Expect(typeof (InvalidOperationException), new _ICodeBlock_33(this));
        }

        public virtual void TestRemoveLastBounds()
        {
            for (var i = 0; i < 3; ++i)
            {
                AssertIllegalRemoveLast();
                buffer.AddFirst(1);
                buffer.AddFirst(3);
                AssertRemoveLast(1);
                AssertRemoveLast(3);
                AssertIllegalRemoveLast();
            }
        }

        private void AssertIllegalRemoveFirst()
        {
            Assert.Expect(typeof (InvalidOperationException), new _ICodeBlock_54(this));
        }

        private void AssertIllegalRemoveLast()
        {
            Assert.Expect(typeof (InvalidOperationException), new _ICodeBlock_62(this));
        }

        private void AssertRemoveLast(int value)
        {
            Assert.AreEqual(value, (int) buffer.RemoveLast());
        }

        public virtual void TestIterator()
        {
            for (var i = 0; i < 3; ++i)
            {
                AssertIterator(new object[] {});
                buffer.AddFirst(1);
                AssertIterator(new object[] {1});
                buffer.AddFirst(2);
                AssertIterator(new object[] {2, 1});
                buffer.RemoveLast();
                AssertIterator(new object[] {2});
                buffer.RemoveLast();
            }
        }

        public virtual void TestContains()
        {
            buffer.AddFirst(1);
            buffer.AddFirst(3);
            buffer.AddFirst(5);
            Assert.IsTrue(buffer.Contains(1));
            Assert.IsFalse(buffer.Contains(2));
            Assert.IsTrue(buffer.Contains(3));
            Assert.IsFalse(buffer.Contains(4));
            Assert.IsTrue(buffer.Contains(5));
        }

        public virtual void TestFullEmpty()
        {
            Assert.IsTrue(buffer.IsEmpty());
            Assert.IsFalse(buffer.IsFull());
            buffer.AddFirst(1);
            Assert.IsFalse(buffer.IsEmpty());
            Assert.IsFalse(buffer.IsFull());
            buffer.AddFirst(2);
            buffer.AddFirst(3);
            buffer.AddFirst(4);
            Assert.IsFalse(buffer.IsEmpty());
            Assert.IsTrue(buffer.IsFull());
            buffer.RemoveLast();
            Assert.IsFalse(buffer.IsEmpty());
            Assert.IsFalse(buffer.IsFull());
        }

        public virtual void TestSize()
        {
            for (var i = 0; i < 3; ++i)
            {
                AssertSize(0);
                for (var j = 0; j < BufferSize; ++j)
                {
                    buffer.AddFirst(j);
                    AssertSize(j + 1);
                }
                for (var j = 0; j < BufferSize; ++j)
                {
                    buffer.RemoveLast();
                    AssertSize(BufferSize - j - 1);
                }
            }
        }

        private void AssertSize(int expected)
        {
            Assert.AreEqual(expected, buffer.Size());
        }

        private void AssertIterator(object[] expected)
        {
            Iterator4Assert.AreEqual(expected, buffer.GetEnumerator());
        }

        public virtual void TestRemove()
        {
            AssertIllegalRemove(42);
            buffer.AddFirst(1);
            AssertRemove(1);
            FillBuffer();
            AssertRemovals(new[] {1, 2, 3, 4});
            FillBuffer();
            AssertRemovals(new[] {2, 3, 4, 1});
            FillBuffer();
            AssertRemovals(new[] {3, 2, 4, 1});
            FillBuffer();
            AssertRemovals(new[] {4, 3, 2, 1});
            FillBuffer();
            AssertRemovals(new[] {4, 1, 2, 3});
            FillBuffer();
            AssertRemoveLast(1);
            AssertRemoveLast(2);
            AssertRemoveLast(3);
            AssertRemoveLast(4);
        }

        private void AssertRemovals(int[] indexes)
        {
            for (var iIndex = 0; iIndex < indexes.Length; ++iIndex)
            {
                var i = indexes[iIndex];
                AssertRemove(i);
            }
            AssertIllegalRemoveLast();
            AssertIllegalRemoveFirst();
        }

        private void AssertRemove(int value)
        {
            Assert.IsTrue(buffer.Remove(value));
            AssertIllegalRemove(value);
        }

        private void AssertIllegalRemove(int value)
        {
            Assert.IsFalse(buffer.Remove(value));
        }

        private void FillBuffer()
        {
            for (var i = 1; i <= BufferSize; i++)
            {
                buffer.AddFirst(i);
            }
        }

        private sealed class _ICodeBlock_33 : ICodeBlock
        {
            private readonly CircularBufferTestCase _enclosing;

            public _ICodeBlock_33(CircularBufferTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Run()
            {
                _enclosing.buffer.AddFirst(3);
            }
        }

        private sealed class _ICodeBlock_54 : ICodeBlock
        {
            private readonly CircularBufferTestCase _enclosing;

            public _ICodeBlock_54(CircularBufferTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Run()
            {
                _enclosing.buffer.RemoveFirst();
            }
        }

        private sealed class _ICodeBlock_62 : ICodeBlock
        {
            private readonly CircularBufferTestCase _enclosing;

            public _ICodeBlock_62(CircularBufferTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Run()
            {
                _enclosing.buffer.RemoveLast();
            }
        }
    }
}