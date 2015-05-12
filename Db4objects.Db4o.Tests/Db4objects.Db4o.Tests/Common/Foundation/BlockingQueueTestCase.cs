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
using Sharpen;
using Sharpen.Lang;

namespace Db4objects.Db4o.Tests.Common.Foundation
{
    public class BlockingQueueTestCase : Queue4TestCaseBase
    {
        public virtual void TestIterator()
        {
            IQueue4 queue = new BlockingQueue();
            string[] data = {"a", "b", "c", "d"};
            for (var idx = 0; idx < data.Length; idx++)
            {
                AssertIterator(queue, data, idx);
                queue.Add(data[idx]);
                AssertIterator(queue, data, idx + 1);
            }
        }

        public virtual void TestNext()
        {
            IQueue4 queue = new BlockingQueue();
            string[] data = {"a", "b", "c", "d"};
            queue.Add(data[0]);
            Assert.AreSame(data[0], queue.Next());
            queue.Add(data[1]);
            queue.Add(data[2]);
            Assert.AreSame(data[1], queue.Next());
            Assert.AreSame(data[2], queue.Next());
        }

        public virtual void TestTimeoutNext()
        {
            var queue = new BlockingQueue();
            Assert.IsNull(AssertTakeAtLeast(200, new _IClosure4_35(queue)));
            var obj = new object();
            queue.Add(obj);
            Assert.AreSame(obj, AssertTakeLessThan(50, new _IClosure4_46(queue)));
            Assert.IsNull(AssertTakeAtLeast(200, new _IClosure4_53(queue)));
        }

        public virtual void TestDrainTo()
        {
            var queue = new BlockingQueue();
            queue.Add(new object());
            queue.Add(new object());
            var list = new Collection4();
            Assert.AreEqual(2, queue.DrainTo(list));
            Assert.AreEqual(2, list.Size());
            Assert.IsFalse(queue.HasNext());
        }

        private object AssertTakeLessThan(long time, IClosure4 runnable)
        {
            var before = Runtime.CurrentTimeMillis();
            var ret = runnable.Run();
            Assert.IsSmallerOrEqual(time, Runtime.CurrentTimeMillis() - before);
            return ret;
        }

        private object AssertTakeAtLeast(long time, IClosure4 runnable)
        {
            var before = Runtime.CurrentTimeMillis();
            var ret = runnable.Run();
            Assert.IsGreaterOrEqual(time, Runtime.CurrentTimeMillis() - before);
            return ret;
        }

        public virtual void TestBlocking()
        {
            IQueue4 queue = new BlockingQueue();
            string[] data = {"a", "b", "c", "d"};
            queue.Add(data[0]);
            Assert.AreSame(data[0], queue.Next());
            var notifyThread = new NotifyThread
                (queue, data[1]);
            notifyThread.Start();
            var start = Runtime.CurrentTimeMillis();
            Assert.AreSame(data[1], queue.Next());
            var end = Runtime.CurrentTimeMillis();
            Assert.IsGreater(500, end - start);
        }

        public virtual void TestStop()
        {
            var queue = new BlockingQueue();
            string[] data = {"a", "b", "c", "d"};
            queue.Add(data[0]);
            Assert.AreSame(data[0], queue.Next());
            var notifyThread = new StopThread
                (queue);
            notifyThread.Start();
            Assert.Expect(typeof (BlockingQueueStoppedException), new _ICodeBlock_110(queue));
        }

        private sealed class _IClosure4_35 : IClosure4
        {
            private readonly BlockingQueue queue;

            public _IClosure4_35(BlockingQueue queue)
            {
                this.queue = queue;
            }

            public object Run()
            {
                return queue.Next(200);
            }
        }

        private sealed class _IClosure4_46 : IClosure4
        {
            private readonly BlockingQueue queue;

            public _IClosure4_46(BlockingQueue queue)
            {
                this.queue = queue;
            }

            public object Run()
            {
                return queue.Next(200);
            }
        }

        private sealed class _IClosure4_53 : IClosure4
        {
            private readonly BlockingQueue queue;

            public _IClosure4_53(BlockingQueue queue)
            {
                this.queue = queue;
            }

            public object Run()
            {
                return queue.Next(200);
            }
        }

        private sealed class _ICodeBlock_110 : ICodeBlock
        {
            private readonly BlockingQueue queue;

            public _ICodeBlock_110(BlockingQueue queue)
            {
                this.queue = queue;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                queue.Next();
            }
        }

        private class NotifyThread : Thread
        {
            private readonly object _data;
            private readonly IQueue4 _queue;

            internal NotifyThread(IQueue4 queue, object data)
            {
                _queue = queue;
                _data = data;
            }

            public override void Run()
            {
                try
                {
                    Sleep(2000);
                }
                catch (Exception)
                {
                }
                _queue.Add(_data);
            }
        }

        private class StopThread : Thread
        {
            private readonly BlockingQueue _queue;

            internal StopThread(BlockingQueue queue)
            {
                _queue = queue;
            }

            public override void Run()
            {
                try
                {
                    Sleep(2000);
                }
                catch (Exception)
                {
                }
                _queue.Stop();
            }
        }
    }
}