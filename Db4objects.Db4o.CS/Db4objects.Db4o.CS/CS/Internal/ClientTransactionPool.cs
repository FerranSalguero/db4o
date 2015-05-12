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
using Db4objects.Db4o.Internal;
using Sharpen;

namespace Db4objects.Db4o.CS.Internal
{
    public class ClientTransactionPool
    {
        private readonly Hashtable4 _fileName2Container;
        private readonly LocalObjectContainer _mainContainer;
        private readonly Hashtable4 _transaction2Container;
        private bool _closed;

        public ClientTransactionPool(LocalObjectContainer mainContainer)
        {
            // Transaction -> ContainerCount
            // String -> ContainerCount
            var mainEntry = new ContainerCount
                (mainContainer, 1);
            _transaction2Container = new Hashtable4();
            _fileName2Container = new Hashtable4();
            _fileName2Container.Put(mainContainer.FileName(), mainEntry);
            _mainContainer = mainContainer;
        }

        public virtual Transaction AcquireMain()
        {
            return Acquire(_mainContainer.FileName());
        }

        public virtual Transaction Acquire(string fileName)
        {
            lock (_mainContainer.Lock())
            {
                var entry = (ContainerCount
                    ) _fileName2Container.Get(fileName);
                if (entry == null)
                {
                    var container = (LocalObjectContainer) Db4oEmbedded.OpenFile(Db4oEmbedded
                        .NewConfiguration(), fileName);
                    container.ConfigImpl.SetMessageRecipient(_mainContainer.ConfigImpl.MessageRecipient
                        ());
                    entry = new ContainerCount(container);
                    _fileName2Container.Put(fileName, entry);
                }
                var transaction = entry.NewTransaction();
                var objectContainerSession = new ObjectContainerSession(entry.
                    Container(), transaction);
                transaction.SetOutSideRepresentation(objectContainerSession);
                _transaction2Container.Put(transaction, entry);
                return transaction;
            }
        }

        public virtual void Release(ShutdownMode mode, Transaction transaction, bool rollbackOnClose
            )
        {
            lock (_mainContainer.Lock())
            {
                var entry = (ContainerCount
                    ) _transaction2Container.Get(transaction);
                entry.Container().CloseTransaction(transaction, false, mode.IsFatal() ? false : rollbackOnClose
                    );
                _transaction2Container.Remove(transaction);
                entry.Release();
                if (entry.IsEmpty())
                {
                    _fileName2Container.Remove(entry.FileName());
                    try
                    {
                        entry.Close(mode);
                    }
                    catch (Exception t)
                    {
                        // If we are in fatal ShutdownMode close will
                        // throw but we want to continue shutting down
                        // all entries.
                        Runtime.PrintStackTrace(t);
                    }
                }
            }
        }

        public virtual void Close()
        {
            Close(ShutdownMode.Normal);
        }

        public virtual void Close(ShutdownMode mode)
        {
            lock (_mainContainer.Lock())
            {
                var entryIter = _fileName2Container.Iterator();
                while (entryIter.MoveNext())
                {
                    var hashEntry = (IEntry4) entryIter.Current;
                    var containerCount = (ContainerCount
                        ) hashEntry.Value();
                    try
                    {
                        containerCount.Close(mode);
                    }
                    catch (Exception t)
                    {
                        // If we are in fatal ShutdownMode close will
                        // throw but we want to continue shutting down
                        // all entries.
                        Runtime.PrintStackTrace(t);
                    }
                }
                _closed = true;
            }
        }

        public virtual int OpenTransactionCount()
        {
            return IsClosed() ? 0 : _transaction2Container.Size();
        }

        public virtual int OpenFileCount()
        {
            return IsClosed() ? 0 : _fileName2Container.Size();
        }

        public virtual bool IsClosed()
        {
            return _closed || _mainContainer.IsClosed();
        }

        public class ContainerCount
        {
            private LocalObjectContainer _container;
            private int _count;

            public ContainerCount(LocalObjectContainer container) : this(container, 0)
            {
            }

            public ContainerCount(LocalObjectContainer container, int count)
            {
                _container = container;
                _count = count;
            }

            public virtual LocalObjectContainer Container()
            {
                return _container;
            }

            public virtual bool IsEmpty()
            {
                return _count <= 0;
            }

            public virtual Transaction NewTransaction()
            {
                _count++;
                return _container.NewUserTransaction();
            }

            public virtual void Release()
            {
                if (_count == 0)
                {
                    throw new InvalidOperationException();
                }
                _count--;
            }

            public virtual string FileName()
            {
                return _container.FileName();
            }

            public virtual void Close(ShutdownMode mode)
            {
                if (!mode.IsFatal())
                {
                    _container.Close();
                    _container = null;
                    return;
                }
                _container.FatalShutdown(((ShutdownMode.FatalMode) mode).Exc());
            }

            public override int GetHashCode()
            {
                return FileName().GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (this == obj)
                {
                    return true;
                }
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }
                var other = (ContainerCount
                    ) obj;
                return FileName().Equals(other.FileName());
            }
        }
    }
}