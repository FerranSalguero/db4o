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

namespace Db4objects.Db4o.Foundation
{
    /// <summary>yes/no/dontknow data type</summary>
    /// <exclude></exclude>
    [Serializable]
    public sealed class TernaryBool
    {
        private const int NoId = -1;
        private const int YesId = 1;
        private const int UnspecifiedId = 0;

        public static readonly TernaryBool No = new TernaryBool
            (NoId);

        public static readonly TernaryBool Yes = new TernaryBool
            (YesId);

        public static readonly TernaryBool Unspecified = new TernaryBool
            (UnspecifiedId);

        private readonly int _value;

        private TernaryBool(int value)
        {
            _value = value;
        }

        public bool BooleanValue(bool defaultValue)
        {
            switch (_value)
            {
                case NoId:
                {
                    return false;
                }

                case YesId:
                {
                    return true;
                }

                default:
                {
                    return defaultValue;
                    break;
                }
            }
        }

        public bool IsUnspecified()
        {
            return this == Unspecified;
        }

        public bool DefiniteYes()
        {
            return this == Yes;
        }

        public bool DefiniteNo()
        {
            return this == No;
        }

        public static TernaryBool ForBoolean(bool value)
        {
            return (value ? Yes : No);
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
            var tb = (TernaryBool
                ) obj;
            return _value == tb._value;
        }

        public override int GetHashCode()
        {
            return _value;
        }

        private object ReadResolve()
        {
            switch (_value)
            {
                case NoId:
                {
                    return No;
                }

                case YesId:
                {
                    return Yes;
                }

                default:
                {
                    return Unspecified;
                    break;
                }
            }
        }

        public override string ToString()
        {
            switch (_value)
            {
                case NoId:
                {
                    return "NO";
                }

                case YesId:
                {
                    return "YES";
                }

                default:
                {
                    return "UNSPECIFIED";
                    break;
                }
            }
        }
    }
}