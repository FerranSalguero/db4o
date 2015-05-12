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
using Db4objects.Db4o.Activation;
using Db4objects.Db4o.TA;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.TA
{
    public class LinkedArrays : ICanAssertActivationDepth
    {
        public ActivatableItem[] _activatableItemArray;
        public int[] _intArray;
        public bool _isRoot;
        public Item[] _itemArray;
        public LinkedArrays[] _linkedArrays;
        public LinkedArrays _next;
        public object _objectArray;
        public string[] _stringArray;
        public object[] _untypedArray;

        public virtual void AssertActivationDepth(int depth, bool transparent)
        {
            NullAssert(_next, depth);
            NullAssert(_objectArray, depth);
            NullAssert(_untypedArray, depth);
            NullAssert(_stringArray, depth);
            NullAssert(_intArray, depth);
            NullAssert(_itemArray, depth);
            NullAssert(_linkedArrays, depth);
            NullAssert(_activatableItemArray, depth);
            if (depth < 1)
            {
                return;
            }
            Assert.IsNotNull(_stringArray[0]);
            Assert.IsGreater(0, _intArray[0]);
            RecurseAssertActivationDepth(((object[]) _objectArray)[0], depth, transparent);
            RecurseAssertActivationDepth(_untypedArray[0], depth, transparent);
            RecurseAssertActivationDepth(_itemArray[0], depth, transparent);
            RecurseAssertActivationDepth(_activatableItemArray[0], depth, transparent);
            RecurseAssertActivationDepth(_linkedArrays[0], depth, transparent);
        }

        public static LinkedArrays NewLinkedArrayRoot(int depth)
        {
            var root = NewLinkedArrays(depth);
            root._isRoot = true;
            return root;
        }

        public static LinkedArrays NewLinkedArrays(int depth)
        {
            if (depth < 1)
            {
                return null;
            }
            var la = new LinkedArrays();
            depth--;
            if (depth < 1)
            {
                return la;
            }
            la._next = NewLinkedArrays(depth);
            la._objectArray = new object[] {NewItem(depth)};
            la._untypedArray = new object[] {NewItem(depth)};
            la._stringArray = new[] {depth.ToString()};
            la._intArray = new[] {depth + 1};
            la._itemArray = new[] {NewItem(depth)};
            la._activatableItemArray = new[]
            {
                NewActivatableItem
                    (depth)
            };
            la._linkedArrays = new[] {NewLinkedArrays(depth)};
            return la;
        }

        private static Item NewItem(int depth)
        {
            if (depth < 1)
            {
                return null;
            }
            return new Item(depth);
        }

        private static ActivatableItem NewActivatableItem(int depth)
        {
            if (depth < 1)
            {
                return null;
            }
            return new ActivatableItem(depth);
        }

        internal static void RecurseAssertActivationDepth(object obj, int depth, bool transparent
            )
        {
            NullAssert(obj, depth);
            if (obj == null)
            {
                return;
            }
            ((ICanAssertActivationDepth) obj).AssertActivationDepth(depth - 1, transparent);
        }

        internal static void NullAssert(object obj, int depth)
        {
            if (depth < 1)
            {
                Assert.IsNull(obj);
            }
            else
            {
                Assert.IsNotNull(obj);
            }
        }

        public class Item : ICanAssertActivationDepth
        {
            public LinkedArrays _linkedArrays;
            public string _name;

            public Item()
            {
            }

            public Item(int depth)
            {
                if (depth > 1)
                {
                    _name = depth.ToString();
                    _linkedArrays = NewLinkedArrays(depth - 1);
                }
            }

            public virtual void AssertActivationDepth(int depth, bool transparent)
            {
                NullAssert(_name, depth);
                NullAssert(_linkedArrays, depth);
                if (depth < 1)
                {
                    return;
                }
                RecurseAssertActivationDepth(_linkedArrays, depth, transparent);
            }
        }

        public class ActivatableItem : IActivatable, ICanAssertActivationDepth
        {
            [NonSerialized] private IActivator _activator;

            public LinkedArrays _linkedArrays;
            public string _name;

            public ActivatableItem()
            {
            }

            public ActivatableItem(int depth)
            {
                if (depth > 1)
                {
                    _name = depth.ToString();
                    _linkedArrays = NewLinkedArrays(depth - 1);
                }
            }

            public virtual void Activate(ActivationPurpose purpose)
            {
                if (_activator != null)
                {
                    _activator.Activate(purpose);
                }
            }

            public virtual void Bind(IActivator activator)
            {
                _activator = activator;
            }

            public virtual void AssertActivationDepth(int depth, bool transparent)
            {
                if (transparent)
                {
                    Assert.IsNull(_name);
                    Assert.IsNull(_linkedArrays);
                    return;
                }
                NullAssert(_name, depth);
                NullAssert(_linkedArrays, depth);
                if (depth < 1)
                {
                    return;
                }
                RecurseAssertActivationDepth(_linkedArrays, depth, transparent);
            }
        }
    }
}