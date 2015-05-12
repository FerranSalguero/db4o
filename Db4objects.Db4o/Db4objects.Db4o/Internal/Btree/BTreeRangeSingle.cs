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
using System.Collections;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal.Btree.Algebra;

namespace Db4objects.Db4o.Internal.Btree
{
    /// <exclude></exclude>
    public class BTreeRangeSingle : IBTreeRange
    {
        public static readonly IComparison4 Comparison = new _IComparison4_14();
        private readonly BTree _btree;
        private readonly BTreePointer _end;
        private readonly BTreePointer _first;
        private readonly Transaction _transaction;

        public BTreeRangeSingle(Transaction transaction, BTree btree
            , BTreePointer first, BTreePointer end)
        {
            if (transaction == null || btree == null)
            {
                throw new ArgumentNullException();
            }
            _transaction = transaction;
            _btree = btree;
            _first = first;
            _end = end;
        }

        public virtual void Accept(IBTreeRangeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public virtual bool IsEmpty()
        {
            return BTreePointer.Equals(_first, _end);
        }

        public virtual int Size()
        {
            if (IsEmpty())
            {
                return 0;
            }
            // TODO: This was an attempt to improve size calculation.
            //       Since all nodes are read, there is no improvement.        
            //        BTreeNode currentNode = _first.node();
            //        int sizeOnFirst = currentNode.count() - _first.index();
            //
            //        BTreeNode endNode = _end == null ? null : _end.node();
            //        int substractForEnd = 
            //            (endNode == null) ? 0 : (endNode.count() -  _end.index());
            //        
            //        int size = sizeOnFirst - substractForEnd;
            //        while(! currentNode.equals(endNode)){
            //            currentNode = currentNode.nextNode();
            //            if(currentNode == null){
            //                break;
            //            }
            //            currentNode.prepareRead(transaction());
            //            size += currentNode.count(); 
            //        }
            //        return size;
            var size = 0;
            var i = Keys();
            while (i.MoveNext())
            {
                ++size;
            }
            return size;
        }

        public virtual IEnumerator Pointers()
        {
            return new BTreeRangePointerIterator(this);
        }

        public virtual IEnumerator Keys()
        {
            return new BTreeRangeKeyIterator(this);
        }

        public virtual IBTreeRange Greater()
        {
            return NewBTreeRangeSingle(_end, null);
        }

        public virtual IBTreeRange Union(IBTreeRange other)
        {
            if (null == other)
            {
                throw new ArgumentNullException();
            }
            return new BTreeRangeSingleUnion(this).Dispatch(other);
        }

        public virtual IBTreeRange ExtendToFirst()
        {
            return NewBTreeRangeSingle(FirstBTreePointer(), _end);
        }

        public virtual IBTreeRange ExtendToLast()
        {
            return NewBTreeRangeSingle(_first, null);
        }

        public virtual IBTreeRange Smaller()
        {
            return NewBTreeRangeSingle(FirstBTreePointer(), _first);
        }

        public virtual IBTreeRange Intersect(IBTreeRange range)
        {
            if (null == range)
            {
                throw new ArgumentNullException();
            }
            return new BTreeRangeSingleIntersect(this).Dispatch(range);
        }

        public virtual IBTreeRange ExtendToLastOf(IBTreeRange range)
        {
            var rangeImpl = CheckRangeArgument(range
                );
            return NewBTreeRangeSingle(_first, rangeImpl._end);
        }

        public virtual BTreePointer LastPointer()
        {
            if (_end == null)
            {
                return Btree().LastPointer(Transaction());
            }
            return _end.Previous();
        }

        public BTreePointer End()
        {
            return _end;
        }

        public virtual Transaction Transaction()
        {
            return _transaction;
        }

        public virtual BTreePointer First()
        {
            return _first;
        }

        public virtual bool Adjacent(BTreeRangeSingle range
            )
        {
            return BTreePointer.Equals(_end, range._first) || BTreePointer.Equals(range._end,
                _first);
        }

        public virtual bool Overlaps(BTreeRangeSingle range
            )
        {
            return FirstOverlaps(this, range) || FirstOverlaps(range, this);
        }

        private bool FirstOverlaps(BTreeRangeSingle x, BTreeRangeSingle
            y)
        {
            return BTreePointer.LessThan(y._first, x._end) && BTreePointer.LessThan(x._first,
                y._end);
        }

        public virtual BTreeRangeSingle NewBTreeRangeSingle
            (BTreePointer first, BTreePointer end)
        {
            return new BTreeRangeSingle(Transaction(), _btree,
                first, end);
        }

        public virtual IBTreeRange NewEmptyRange()
        {
            return NewBTreeRangeSingle(null, null);
        }

        private BTreePointer FirstBTreePointer()
        {
            return Btree().FirstPointer(Transaction());
        }

        private BTree Btree()
        {
            return _btree;
        }

        public override string ToString()
        {
            return "BTreeRangeSingle(first=" + _first + ", end=" + _end + ")";
        }

        private BTreeRangeSingle CheckRangeArgument(IBTreeRange
            range)
        {
            if (null == range)
            {
                throw new ArgumentNullException();
            }
            var rangeImpl = (BTreeRangeSingle
                ) range;
            if (Btree() != rangeImpl.Btree())
            {
                throw new ArgumentException();
            }
            return rangeImpl;
        }

        private sealed class _IComparison4_14 : IComparison4
        {
            public int Compare(object x, object y)
            {
                var xRange = (BTreeRangeSingle
                    ) x;
                var yRange = (BTreeRangeSingle
                    ) y;
                return xRange.First().CompareTo(yRange.First());
            }
        }
    }
}