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

namespace Db4objects.Db4o.Internal.Btree
{
    /// <exclude></exclude>
    public sealed class BTreePointer
    {
        private readonly int _index;
        private readonly BTreeNode _node;
        private readonly ByteArrayBuffer _nodeReader;
        private readonly Transaction _transaction;

        public BTreePointer(Transaction transaction, ByteArrayBuffer nodeReader, BTreeNode
            node, int index)
        {
            if (transaction == null || node == null)
            {
                throw new ArgumentNullException();
            }
            _transaction = transaction;
            _nodeReader = nodeReader;
            _node = node;
            _index = index;
        }

        public static BTreePointer Max(BTreePointer
            x, BTreePointer y)
        {
            if (x == null)
            {
                return x;
            }
            if (y == null)
            {
                return y;
            }
            if (x.CompareTo(y) > 0)
            {
                return x;
            }
            return y;
        }

        public static BTreePointer Min(BTreePointer
            x, BTreePointer y)
        {
            if (x == null)
            {
                return y;
            }
            if (y == null)
            {
                return x;
            }
            if (x.CompareTo(y) < 0)
            {
                return x;
            }
            return y;
        }

        public int Index()
        {
            return _index;
        }

        public BTreeNode Node()
        {
            return _node;
        }

        public object Key()
        {
            return _node.Key(_transaction, _nodeReader, _index);
        }

        public BTreePointer Next()
        {
            var indexInMyNode = _index + 1;
            while (indexInMyNode < _node.Count())
            {
                if (_node.IndexIsValid(_transaction, indexInMyNode))
                {
                    return new BTreePointer(_transaction, _nodeReader,
                        _node, indexInMyNode);
                }
                indexInMyNode++;
            }
            var newIndex = -1;
            var nextNode = _node;
            ByteArrayBuffer nextReader = null;
            while (newIndex == -1)
            {
                nextNode = nextNode.NextNode();
                if (nextNode == null)
                {
                    return null;
                }
                nextReader = nextNode.PrepareRead(_transaction);
                newIndex = nextNode.FirstKeyIndex(_transaction);
            }
            Btree().ConvertCacheEvictedNodesToReadMode();
            return new BTreePointer(_transaction, nextReader,
                nextNode, newIndex);
        }

        public BTreePointer Previous()
        {
            var indexInMyNode = _index - 1;
            while (indexInMyNode >= 0)
            {
                if (_node.IndexIsValid(_transaction, indexInMyNode))
                {
                    return new BTreePointer(_transaction, _nodeReader,
                        _node, indexInMyNode);
                }
                indexInMyNode--;
            }
            var newIndex = -1;
            var previousNode = _node;
            ByteArrayBuffer previousReader = null;
            while (newIndex == -1)
            {
                previousNode = previousNode.PreviousNode();
                if (previousNode == null)
                {
                    return null;
                }
                previousReader = previousNode.PrepareRead(_transaction);
                newIndex = previousNode.LastKeyIndex(_transaction);
            }
            return new BTreePointer(_transaction, previousReader
                , previousNode, newIndex);
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (!(obj is BTreePointer))
            {
                return false;
            }
            var other = (BTreePointer
                ) obj;
            if (_index != other._index)
            {
                return false;
            }
            return _node.Equals(other._node);
        }

        public override int GetHashCode()
        {
            return _node.GetHashCode();
        }

        public override string ToString()
        {
            return "BTreePointer(index=" + _index + ", node=" + _node + ")";
        }

        public int CompareTo(BTreePointer y)
        {
            if (null == y)
            {
                throw new ArgumentNullException();
            }
            if (Btree() != y.Btree())
            {
                throw new ArgumentException();
            }
            return Btree().CompareKeys(_transaction.Context(), Key(), y.Key());
        }

        private BTree Btree()
        {
            return _node.Btree();
        }

        public static bool LessThan(BTreePointer x, BTreePointer
            y)
        {
            return Min(x, y) == x && !Equals(x, y
                );
        }

        public static bool Equals(BTreePointer x, BTreePointer
            y)
        {
            if (x == null)
            {
                return y == null;
            }
            return x.Equals(y);
        }

        public bool IsValid()
        {
            return _node.IndexIsValid(_transaction, _index);
        }
    }
}