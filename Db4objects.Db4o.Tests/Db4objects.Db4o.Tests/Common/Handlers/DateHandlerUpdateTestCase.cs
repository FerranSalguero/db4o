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
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Marshall;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Handlers
{
    public class DateHandlerUpdateTestCase : HandlerUpdateTestCaseBase
    {
        private static readonly DateTime[] data =
        {
            new DateTime(DatePlatform
                .MinDate),
            new DateTime(DatePlatform.MinDate + 1), new DateTime(1191972104500L),
            new DateTime(DatePlatform.MaxDate - 1), new DateTime(DatePlatform.MaxDate)
        };

        public static void Main(string[] args)
        {
            new ConsoleTestRunner(typeof (DateHandlerUpdateTestCase)).Run();
        }

        protected override void AssertArrays(IExtObjectContainer objectContainer, object
            obj)
        {
            var itemArrays = (ItemArrays
                ) obj;
            var dateArray = (DateTime[]) itemArrays._arrayInObject;
            for (var i = 0; i < data.Length; i++)
            {
                AssertAreEqual(data[i], itemArrays._dateArray[i]);
                AssertAreEqual(data[i], (DateTime) itemArrays._untypedObjectArray[i]);
                AssertAreEqual(data[i], dateArray[i]);
            }
            // Assert.isNull(itemArrays._dateArray[data.length]);
            Assert.IsNull(itemArrays._untypedObjectArray[data.Length]);
            // FIXME: We are not signalling null for Dates in typed arrays in 
            //        the current handler format:        
            Assert.AreEqual(EmptyValue(), dateArray[data.Length]);
        }

        protected override void AssertValues(IExtObjectContainer objectContainer, object[]
            values)
        {
            for (var i = 0; i < data.Length; i++)
            {
                var item = (Item) values[i];
                AssertAreEqual(data[i], item._date);
                AssertAreEqual(data[i], (DateTime) item._untyped);
            }
            var emptyItem = (Item) values
                [values.Length - 1];
            Assert.AreEqual(EmptyValue(), emptyItem._date);
            Assert.IsNull(emptyItem._untyped);
        }

        private object EmptyValue()
        {
            return Platform4.ReflectorForType(typeof (DateTime)).ForClass(typeof (DateTime)).NullValue
                ();
        }

        private void AssertAreEqual(DateTime expected, DateTime actual)
        {
            if (expected.Equals(new DateTime(DatePlatform.MaxDate)) && Db4oHandlerVersion() ==
                0)
            {
                // Bug in the oldest format: It treats a Long.MAX_VALUE date as null. 
                expected = MarshallingConstants0.NullDate;
            }
            Assert.AreEqual(expected, actual);
        }

        protected override object CreateArrays()
        {
            var itemArrays = new ItemArrays
                ();
            itemArrays._dateArray = new DateTime[data.Length + 1];
            Array.Copy(data, 0, itemArrays._dateArray, 0, data.Length);
            itemArrays._untypedObjectArray = new object[data.Length + 1];
            Array.Copy(data, 0, itemArrays._untypedObjectArray, 0, data.Length);
            var dateArray = new DateTime[data.Length + 1];
            Array.Copy(data, 0, dateArray, 0, data.Length);
            itemArrays._arrayInObject = dateArray;
            return itemArrays;
        }

        protected override object[] CreateValues()
        {
            var values = new Item[data
                .Length + 1];
            for (var i = 0; i < data.Length; i++)
            {
                var item = new Item();
                item._date = data[i];
                item._untyped = data[i];
                values[i] = item;
            }
            values[values.Length - 1] = new Item();
            return values;
        }

        protected override string TypeName()
        {
            return "date";
        }

        public class Item
        {
            public DateTime _date;
            public object _untyped;
        }

        public class ItemArrays
        {
            public object _arrayInObject;
            public DateTime[] _dateArray;
            public object[] _untypedObjectArray;
        }
    }
}