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
using Db4objects.Db4o.Reflect.Core;
using Sharpen.Lang;

namespace Db4objects.Db4o.Reflect.Net
{
    public class NetArray : AbstractReflectArray
    {
        public NetArray(IReflector reflector) : base(reflector)
        {
        }

        private static Type GetNetType(IReflectClass clazz)
        {
            return ((NetClass) clazz).GetNetType();
        }

        public override void Analyze(object obj, ArrayInfo info)
        {
            info.Nullable(IsNullableType(obj.GetType()));
        }

        private bool IsNullableType(Type type)
        {
            if (type.IsArray)
            {
                return IsNullableType(type.GetElementType());
            }

            var underlyingType = Nullable.GetUnderlyingType(type);
            return underlyingType != null;
        }

        public override object NewInstance(IReflectClass componentType, ArrayInfo info)
        {
            var type = GetNetType(componentType);
            if (info.Nullable())
            {
                type = NullableType(type);
            }
            var multiDimensionalInfo = info as MultidimensionalArrayInfo;
            if (multiDimensionalInfo == null)
            {
                return Array.CreateInstance(type, info.ElementCount());
            }
            var dimensions = multiDimensionalInfo.Dimensions();
            if (dimensions.Length == 1)
            {
                return UnfoldArrayCreation(type, dimensions, 0);
            }
            return UnfoldArrayCreation(GetArrayType(type, dimensions.Length - 1), dimensions, 0);
        }

        private Type NullableType(Type type)
        {
            if (IsNullableType(type))
            {
                return type;
            }
            return typeof (Nullable<>).MakeGenericType(type);
        }

        public override object NewInstance(IReflectClass componentType, int[] dimensions)
        {
            var type = GetNetType(componentType);
            if (dimensions.Length == 1)
            {
                return UnfoldArrayCreation(type, dimensions, 0);
            }

            return UnfoldArrayCreation(GetArrayType(type, dimensions.Length - 1), dimensions, 0);
        }

        private static object UnfoldArrayCreation(Type type, int[] dimensions, int dimensionIndex)
        {
            var length = dimensions[dimensionIndex];
            var array = Array.CreateInstance(type, length);
            if (dimensionIndex == dimensions.Length - 1)
            {
                return array;
            }
            for (var i = 0; i < length; ++i)
            {
                var value = UnfoldArrayCreation(type.GetElementType(), dimensions, dimensionIndex + 1);
                array.SetValue(value, i);
            }
            return array;
        }

        private static Type GetArrayType(Type type, int dimensions)
        {
            if (dimensions < 1) throw new ArgumentOutOfRangeException("dimensions");

            var arrayType = MakeArrayType(type);
            for (var i = 1; i < dimensions; ++i)
            {
                arrayType = MakeArrayType(arrayType);
            }
            return arrayType;
        }

        private static Type MakeArrayType(Type type)
        {
            return ArrayTypeReference.MakeArrayType(type, 1);
        }

        public override object NewInstance(IReflectClass componentType, int length)
        {
            return Array.CreateInstance(GetNetType(componentType), length);
        }
    }
}