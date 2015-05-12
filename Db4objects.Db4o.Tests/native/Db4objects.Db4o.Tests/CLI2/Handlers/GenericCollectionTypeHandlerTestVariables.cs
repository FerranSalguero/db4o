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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Db4oUnit.Fixtures;

namespace Db4objects.Db4o.Tests.CLI2.Handlers
{
    public static class GenericCollectionTypeHandlerTestVariables
    {
        public static readonly FixtureVariable CollectionImplementation = new FixtureVariable("collections");

        public static readonly IFixtureProvider CollectionFixtureProvider = new SimpleFixtureProvider(
            CollectionImplementation,
            new object[]
            {
                new LinkedListItemFactory(),
                new ListItemFactory(),
                new UntypedLinkedListItemFactory(),
                new StackItemFactory(),
                new QueueItemFactory(),
                new ObjectModelCollectionItemFactory()
            });

        public static readonly GenericCollectionTestElementSpec<string> StringElementSpec =
            new GenericCollectionTestElementSpec<string>(new[] {"zero", "one"}, "two", "zzz");

        public static readonly GenericCollectionTestElementSpec<int> IntElementSpec =
            new GenericCollectionTestElementSpec<int>(new[] {0, 1}, 2, int.MaxValue);

        public static readonly GenericCollectionTestElementSpec<int?> NullableIntElementSpec =
            new GenericCollectionTestElementSpec<int?>(new int?[] {0, null}, 2, int.MaxValue);

        public static readonly GenericCollectionTestElementSpec<ValueTypeTest> ValueTypeElementSpec =
            new GenericCollectionTestElementSpec<ValueTypeTest>(new ValueTypeTest[] {0, 1}, 2, int.MaxValue);

        public static readonly GenericCollectionTestElementSpec<object> ObjectTypeElementSpec =
            new GenericCollectionTestElementSpec<object>(new object[] {0, 1}, 2, int.MaxValue);

        public static readonly GenericCollectionTestElementSpec<int[]> IntArrayTypeElementSpec =
            new GenericCollectionTestElementSpec<int[]>(new[] {new[] {0}, new[] {1}}, new[] {2}, new[] {int.MaxValue});

        public static readonly FixtureVariable ElementSpec = new FixtureVariable("elements");

        public static readonly IFixtureProvider ElementsFixtureProvider = new SimpleFixtureProvider(
            ElementSpec,
            new object[]
            {
                StringElementSpec,
                IntElementSpec,
                NullableIntElementSpec,
                //IntArrayTypeElementSpec, // fails: old / new
                ValueTypeElementSpec,
                ObjectTypeElementSpec,
                new GenericCollectionTestElementSpec<FirstClassElement>(
                    new[] {new FirstClassElement(0), new FirstClassElement(1)}, new FirstClassElement(2), null)
            }
            );

        public struct ValueTypeTest
        {
            public readonly int _id;

            public ValueTypeTest(int id)
            {
                _id = id;
            }

            public static implicit operator ValueTypeTest(int id)
            {
                return new ValueTypeTest(id);
            }

            public override string ToString()
            {
                return _id.ToString();
            }
        }

        public class FirstClassElement
        {
            public int _id;

            public FirstClassElement(int id)
            {
                _id = id;
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
                var other = (FirstClassElement) obj;
                return _id == other._id;
            }

            public override int GetHashCode()
            {
                return _id;
            }

            public override string ToString()
            {
                return "FCE#" + _id;
            }
        }

        public class ListItemFactory : GenericCollectionTestFactory
        {
            public override object NewItem<T>()
            {
                return new Item<T>();
            }

            public override Type ContainerType()
            {
                return typeof (List<>);
            }

            public override string Label()
            {
                return "List<>";
            }

            public class Item<T>
            {
                public List<T> _coll = new List<T>();
            }
        }

        public class ObjectModelCollectionItemFactory : GenericCollectionTestFactory
        {
            public override object NewItem<T>()
            {
                return new Item<T>();
            }

            public override Type ContainerType()
            {
                return typeof (Collection<>);
            }

            public override string Label()
            {
                return "ObjectModel.Collection<>";
            }

            public class Item<T>
            {
                public Collection<T> _coll = new Collection<T>();
            }
        }

        public class LinkedListItemFactory : GenericCollectionTestFactory
        {
            public override object NewItem<T>()
            {
                return new Item<T>();
            }

            public override Type ContainerType()
            {
                return typeof (LinkedList<>);
            }

            public override string Label()
            {
                return "LinkedList<>";
            }

            public class Item<T>
            {
                public LinkedList<T> _coll = new LinkedList<T>();
            }
        }

        public class UntypedLinkedListItemFactory : GenericCollectionTestFactory
        {
            public override object NewItem<T>()
            {
                return new Item<T>();
            }

            public override Type ContainerType()
            {
                return typeof (LinkedList<>);
            }

            public override string Label()
            {
                return "LinkedList<>(object)";
            }

            public class Item<T>
            {
                public object _coll = new LinkedList<T>();
            }
        }

        public class StackItemFactory : GenericCollectionTestFactory
        {
            public override object NewItem<T>()
            {
                return new Item<T>();
            }

            public override Type ContainerType()
            {
                return typeof (Stack<>);
            }

            public override string Label()
            {
                return "Stack<>()";
            }

            public class Item<T>
            {
                public Stack<T> _coll = new Stack<T>();
            }
        }

        public class QueueItemFactory : GenericCollectionTestFactory
        {
            public override object NewItem<T>()
            {
                return new Item<T>();
            }

            public override Type ContainerType()
            {
                return typeof (Queue<>);
            }

            public override string Label()
            {
                return "Queue<>()";
            }

            public class Item<T>
            {
                public Queue<T> _coll = new Queue<T>();
            }
        }
    }
}