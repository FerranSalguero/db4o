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

namespace Db4objects.Db4o.Internal.Marshall
{
    /// <exclude></exclude>
    public class AspectType
    {
        public static readonly AspectType Field = new AspectType
            (1);

        public static readonly AspectType Translator =
            new AspectType(2);

        public static readonly AspectType Typehandler =
            new AspectType(3);

        public readonly byte _id;

        private AspectType(byte id)
        {
            _id = id;
        }

        public static AspectType ForByte(byte b)
        {
            switch (b)
            {
                case 1:
                {
                    return Field;
                }

                case 2:
                {
                    return Translator;
                }

                case 3:
                {
                    return Typehandler;
                }

                default:
                {
                    throw new ArgumentException();
                }
            }
        }

        public virtual bool IsFieldMetadata()
        {
            return IsField() || IsTranslator();
        }

        public virtual bool IsTranslator()
        {
            return this == Translator;
        }

        public virtual bool IsField()
        {
            return this == Field;
        }
    }
}