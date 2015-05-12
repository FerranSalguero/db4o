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
using Db4oUnit.Fixtures;

namespace Db4objects.Db4o.Tests.CLI1.Soda
{
    internal interface IValueTypeFixture
    {
        object New(int index);
        Type Type();
        int Compare(object lhs, object rhs);
    }

    internal class ValueTypeFixture<T> : ILabeled, IValueTypeFixture where T : struct, IComparable<T>
    {
        private readonly Func<int, T> _valueExtractor;

        public ValueTypeFixture(Func<int, T> extractor)
        {
            _valueExtractor = extractor;
        }

        public string Label()
        {
            var genericTypeName = Type().Name;
            return genericTypeName.Substring(0, genericTypeName.Length - 2) + "<" + Type().GetGenericArguments()[0].Name +
                   ">";
        }

        public object New(int index)
        {
            return new Thing<T>("Item #" + index, _valueExtractor(index));
        }

        public Type Type()
        {
            return typeof (Thing<T>);
        }

        public int Compare(object lhs, object rhs)
        {
            var lhsThing = (Thing<T>) lhs;
            var rhsThing = (Thing<T>) rhs;

            return lhsThing._value.CompareTo(rhsThing._value);
        }
    }
}