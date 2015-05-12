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

namespace Db4objects.Db4o.Foundation
{
    /// <exclude></exclude>
    public abstract class AbstractTreeIterator : IEnumerator
    {
        private readonly Tree _tree;
        private Stack4 _stack;

        public AbstractTreeIterator(Tree tree)
        {
            _tree = tree;
        }

        public virtual object Current
        {
            get
            {
                if (_stack == null)
                {
                    throw new InvalidOperationException();
                }
                var tree = Peek();
                if (tree == null)
                {
                    return null;
                }
                return CurrentValue(tree);
            }
        }

        public virtual void Reset()
        {
            _stack = null;
        }

        public virtual bool MoveNext()
        {
            if (_stack == null)
            {
                InitStack();
                return _stack != null;
            }
            var current = Peek();
            if (current == null)
            {
                return false;
            }
            if (PushPreceding(current._subsequent))
            {
                return true;
            }
            while (true)
            {
                _stack.Pop();
                var parent = Peek();
                if (parent == null)
                {
                    return false;
                }
                if (current == parent._preceding)
                {
                    return true;
                }
                current = parent;
            }
        }

        private Tree Peek()
        {
            return (Tree) _stack.Peek();
        }

        private void InitStack()
        {
            if (_tree == null)
            {
                return;
            }
            _stack = new Stack4();
            PushPreceding(_tree);
        }

        private bool PushPreceding(Tree node)
        {
            if (node == null)
            {
                return false;
            }
            while (node != null)
            {
                _stack.Push(node);
                node = node._preceding;
            }
            return true;
        }

        protected abstract object CurrentValue(Tree tree);
    }
}