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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Typehandlers;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.CLI1.Handlers
{
    public class EnumTypeHandlerTestCase : AbstractDb4oTestCase
    {
        public enum EnumAsByte : byte
        {
            First,
            Second,
            Third
        }

        public enum EnumAsInteger
        {
            First = -42,
            Second
        }

        public enum EnumAsLong : long
        {
            First = -42,
            Second = 37,
            Third
        }

        private static readonly Item[] _items =
        {
            new Item(EnumAsByte.First, EnumAsInteger.Second, EnumAsLong.Third),
            new Item(EnumAsByte.Second, EnumAsInteger.Second, EnumAsLong.Second),
            new Item(EnumAsByte.Third, EnumAsInteger.Second, EnumAsLong.First),
            new Item((EnumAsByte) 99, (EnumAsInteger) 98, (EnumAsLong) 97)
        };

        protected override void Configure(IConfiguration config)
        {
            base.Configure(config);
            config.RegisterTypeHandler(new EnumTypeHandlerPredicate(), new EnumTypeHandler());

            config.ObjectClass(typeof (Item)).ObjectField("_asByte").Indexed(true);
        }

        protected override void Store()
        {
            foreach (var item in _items)
            {
                Store(item);
            }
        }

        public void TestEnumsAreNotStoredAsObjects()
        {
            Assert.AreEqual(0, Db().Query<EnumAsByte>().Count);
        }

        public void TestNativeQuery()
        {
            AssertItem(EnumAsByte.Second, AsByteFinder(), AsByteSelectorFor);
            AssertItem(EnumAsByte.Third, AsByteFinder(), AsByteSelectorFor);
        }

        public void TestSODAQuery()
        {
            AssertAsByte();
            AssertAsLong();
        }

        public void TestInvalidEnumValue()
        {
            AssertItem((EnumAsByte) 99, AsByteFinder());
        }

        public void TestRetrieveAll()
        {
            AssertCanRetrieveAll();
        }

        public void TestQueryByExample()
        {
            var item = FindItemWithValue(EnumAsByte.Second);
            var result = Db().QueryByExample(item);
            Assert.AreEqual(1, result.Count);
            var itemFound = (Item) result[0];
            AssertItem(item, itemFound);
        }

        private void AssertItem(Item actual, Item template)
        {
            var expected = FindItemWithValue(template._asByte);
            Assert.AreEqual(expected, actual);
        }

        private static Item FindItemWithValue(EnumAsByte value)
        {
#if CF || SILVERLIGHT
    		foreach (Item item in _items)
    		{
    			if (item._asByte == value)
    			{
    				return item;
    			}
    		}

    		return null;
#else
            return Array.Find(_items, delegate(Item candidate) { return candidate._asByte == value; });
#endif
        }

        public void TestQueryByExampleAll()
        {
            // Just like in primitives, if enum 0 is used, the 
            // constraint is ignored.
            var item = new Item(EnumAsByte.First, 0, 0);
            var result = Db().QueryByExample(item);
            Assert.AreEqual(4, result.Count);
        }

        private void AssertCanRetrieveAll()
        {
            var query = NewQuery(typeof (Item));
            var result = query.Execute();
            Assert.AreEqual(_items.Length, result.Count);

            Iterator4Assert.SameContent(result.GetEnumerator(), _items.GetEnumerator());
        }

        public void TestDefragment()
        {
            Defragment();
            AssertCanRetrieveAll();
        }

        public void TestDelete()
        {
            var query = NewQuery(typeof (Item));
            var result = query.Execute();
            while (result.HasNext())
            {
                var item = (Item) result.Next();
                Db().Delete(item);
                Db().Delete(item._asInteger);
            }
        }

        public void TestIndexingLowLevel()
        {
            var container = Fixture().FileSession();
            var classMetadata = container.ClassMetadataForReflectClass(container.Reflector().ForClass(typeof (Item)));
            var fieldMetadata = classMetadata.FieldMetadataForName("_asByte");

            Assert.IsTrue(fieldMetadata.CanLoadByIndex(), "EnumTypeHandler should be indexable.");
            var index = fieldMetadata.GetIndex(container.SystemTransaction());
            Assert.IsNotNull(index, "No btree index found for enum field.");
        }

        public void TestIndexedQuery()
        {
            AssertQuery(EnumAsByte.Second);
            AssertQuery((EnumAsByte) 99);
        }

        private void AssertQuery(EnumAsByte constraint)
        {
            var query = NewQuery();
            query.Constrain(typeof (Item));
            query.Descend("_asByte").Constrain(constraint);

            var result = query.Execute();
            Assert.AreEqual(1, result.Count);
            AssertItem(FindItemWithValue(constraint), (Item) result[0]);
        }

        private void AssertAsLong()
        {
            Func<object, Item> itemFinder = delegate(object value)
            {
                return Find(_items,
                    delegate(Item candidate) { return candidate._asLong == (EnumAsLong) value; });
            };

            AssertItem(EnumAsLong.First, itemFinder);
            AssertItem(EnumAsLong.Second, itemFinder);
        }

        private void AssertAsByte()
        {
            AssertItem(EnumAsByte.Second, AsByteFinder());
            AssertItem(EnumAsByte.Third, AsByteFinder());
        }

        private void AssertItem<T>(T expectedEnumValue, Func<object, Item> itemFinder)
        {
            var query = NewQuery(typeof (Item));
            query.Descend(FieldNameFor(typeof (T))).Constrain(expectedEnumValue);

            var result = query.Execute();

            Assert.AreEqual(1, result.Count);
            var actual = (Item) result[0];
            Assert.IsNotNull(actual);

            var expected = itemFinder(expectedEnumValue);
            Assert.IsNotNull(expected);

            Assert.AreEqual(expected, actual);
        }

        private void AssertItem<T>(T expectedEnumValue, Func<object, Item> itemFinder,
            Func<T, Predicate<Item>> selectorBuilder)
        {
            var items = Db().Query(selectorBuilder(expectedEnumValue));
            Assert.AreEqual(1, items.Count);
            Assert.IsNotNull(items[0]);

            var expected = itemFinder(expectedEnumValue);
            Assert.IsNotNull(expected);

            Assert.AreEqual(expected, items[0]);
        }

        private static string FieldNameFor(Type type)
        {
            return type.Name.Replace("EnumA", "_a");
        }

        private static Predicate<Item> AsByteSelectorFor(EnumAsByte value)
        {
            return delegate(Item candidate) { return candidate._asByte == value; };
        }

        private static Func<object, Item> AsByteFinder()
        {
            return
                delegate(object value)
                {
                    return Find(_items, delegate(Item candidate) { return candidate._asByte == (EnumAsByte) value; });
                };
        }

        private static T Find<T>(T[] array, Predicate<T> expected)
        {
            for (var i = 0; i < array.Length; i++)
            {
                if (expected(array[i]))
                {
                    return array[i];
                }
            }
            return default(T);
        }

        private enum EnumAsUInt : uint
        {
            First = 42,
            Second
        }

        private enum EnumAsULong : ulong
        {
            First = 42,
            Second = 37
        }

        [Flags]
        private enum ByteFlags : byte
        {
            First = 0x01,
            Second = 0x02,
            Third = 0x04
        }

        private enum EnumAsSByte : sbyte
        {
            First = 2,
            Second
        }

        public class Item
        {
            public EnumAsByte _asByte;
            public EnumAsInteger _asInteger;
            public EnumAsLong _asLong;

            public Item(EnumAsByte asByte, EnumAsInteger asInteger, EnumAsLong asLong)
            {
                _asByte = asByte;
                _asInteger = asInteger;
                _asLong = asLong;
            }

            public override bool Equals(object obj)
            {
                var rhs = (Item) obj;
                if (rhs == null) return false;

                if (rhs.GetType() != GetType()) return false;

                return _asByte == rhs._asByte && _asInteger == rhs._asInteger && _asLong == rhs._asLong;
            }

            public override string ToString()
            {
                return _asByte + "/" + _asInteger + "/" + _asLong;
            }
        }
    }
}