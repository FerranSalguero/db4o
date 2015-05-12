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

namespace Db4oUnit.Data
{
    /// <summary>Factory for infinite sequences of values.</summary>
    /// <remarks>Factory for infinite sequences of values.</remarks>
    public class Streams
    {
        private static readonly Random random = new Random();

        public static IEnumerable RandomIntegers()
        {
            return Iterators.Series(null, new _IFunction4_15());
        }

        public static IEnumerable RandomNaturals(int ceiling)
        {
            return Iterators.Series(null, new _IFunction4_23(ceiling));
        }

        public static IEnumerable RandomStrings()
        {
            var maxLength = 42;
            return Iterators.Map(RandomNaturals(maxLength), new _IFunction4_32());
        }

        private static string RandomString(int length)
        {
            return Iterators.Join(Generators.Take(length, PrintableCharacters()), string.Empty
                );
        }

        public static IEnumerable PrintableCharacters()
        {
            return Iterators.Filter(RandomCharacters(), new _IPredicate4_45());
        }

        public static IEnumerable RandomCharacters()
        {
            var maxCharInclusive = 'z';
            return Iterators.Map(RandomNaturals(1 + maxCharInclusive), new _IFunction4_75
                ());
        }

        private sealed class _IFunction4_15 : IFunction4
        {
            public object Apply(object arg)
            {
                return random.Next();
            }
        }

        private sealed class _IFunction4_23 : IFunction4
        {
            private readonly int ceiling;

            public _IFunction4_23(int ceiling)
            {
                this.ceiling = ceiling;
            }

            public object Apply(object arg)
            {
                return random.Next(ceiling);
            }
        }

        private sealed class _IFunction4_32 : IFunction4
        {
            public object Apply(object arg)
            {
                var length = ((int) arg);
                return RandomString(length);
            }
        }

        private sealed class _IPredicate4_45 : IPredicate4
        {
            public bool Match(object candidate)
            {
                var character = (char) candidate;
                return IsPrintable(character);
            }

            private bool IsPrintable(char value)
            {
                if (value >= 'a' && value <= 'z')
                {
                    return true;
                }
                if (value >= 'A' && value <= 'Z')
                {
                    return true;
                }
                if (value >= '0' && value <= '9')
                {
                    return true;
                }
                switch (value)
                {
                    case '_':
                    case ' ':
                    case '\r':
                    case '\n':
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private sealed class _IFunction4_75 : IFunction4
        {
            public object Apply(object value)
            {
                return (char) ((int) value);
            }
        }
    }
}