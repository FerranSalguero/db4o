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

namespace Db4objects.Db4o.Foundation
{
    public class PausableBlockingQueue : BlockingQueue, IPausableBlockingQueue4
    {
        private volatile bool _paused;

        public virtual bool Pause()
        {
            if (_paused)
            {
                return false;
            }
            _paused = true;
            return true;
        }

        public virtual bool Resume()
        {
            return (((bool) _lock.Run(new _IClosure4_17(this))));
        }

        public virtual bool IsPaused()
        {
            return _paused;
        }

        public virtual object TryNext()
        {
            return _lock.Run(new _IClosure4_46(this));
        }

        /// <exception cref="Db4objects.Db4o.Foundation.BlockingQueueStoppedException"></exception>
        protected override bool UnsafeWaitForNext(long timeout)
        {
            var hasNext = base.UnsafeWaitForNext(timeout);
            while (_paused && !_stopped)
            {
                _lock.Snooze(timeout);
            }
            if (_stopped)
            {
                throw new BlockingQueueStoppedException();
            }
            return hasNext;
        }

        private sealed class _IClosure4_17 : IClosure4
        {
            private readonly PausableBlockingQueue _enclosing;

            public _IClosure4_17(PausableBlockingQueue _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public object Run()
            {
                if (!_enclosing._paused)
                {
                    return false;
                }
                _enclosing._paused = false;
                _enclosing._lock.Awake();
                return true;
            }
        }

        private sealed class _IClosure4_46 : IClosure4
        {
            private readonly PausableBlockingQueue _enclosing;

            public _IClosure4_46(PausableBlockingQueue _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public object Run()
            {
                return _enclosing.IsPaused()
                    ? null
                    : _enclosing.HasNext()
                        ? _enclosing
                            .Next()
                        : null;
            }
        }
    }
}