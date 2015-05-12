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
using System.Reflection;
using Db4objects.Db4o.Internal;
using Db4oUnit.Extensions;
using Sharpen;

namespace Db4objects.Db4o.Tests.Common.Soda.Util
{
    public class TCompare
    {
        private TCompare()
        {
        }

        public static bool IsEqual(object a_compare, object a_with)
        {
            return IsEqual(a_compare, a_with, null, new ArrayList());
        }

        private static bool IsEqual(object a_compare, object a_with, string a_path, ArrayList
            a_list)
        {
            if (a_compare == null)
            {
                return a_with == null;
            }
            if (a_with == null)
            {
                return false;
            }
            var clazz = a_compare.GetType();
            if (clazz != a_with.GetType())
            {
                return false;
            }
            if (Platform4.IsSimple(clazz))
            {
                return a_compare.Equals(a_with);
            }
            // takes care of repeating calls to the same object
            if (a_list.Contains(a_compare))
            {
                return true;
            }
            a_list.Add(a_compare);
            if (a_compare.GetType().IsArray)
            {
                return AreArraysEqual(NormalizeNArray(a_compare), NormalizeNArray(a_with), a_path
                    , a_list);
            }
            if (HasPublicConstructor(a_compare.GetType()))
            {
                return AreFieldsEqual(a_compare, a_with, a_path, a_list);
            }
            return a_compare.Equals(a_with);
        }

        private static bool AreFieldsEqual(object a_compare, object a_with, string a_path
            , ArrayList a_list)
        {
            var path = GetPath(a_compare, a_with, a_path);
            var fields = Runtime.GetDeclaredFields(a_compare.GetType());
            for (var i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (Db4oUnitPlatform.IsUserField(field))
                {
                    Platform4.SetAccessible(field);
                    try
                    {
                        if (!IsFieldEqual(field, a_compare, a_with, path, a_list))
                        {
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        Runtime.Err.WriteLine("TCompare failure executing path:" + path);
                        Runtime.PrintStackTrace(e);
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool IsFieldEqual(FieldInfo field, object a_compare, object a_with
            , string path, ArrayList a_list)
        {
            var compare = GetFieldValue(field, a_compare);
            var with = GetFieldValue(field, a_with);
            return IsEqual(compare, with, path + field.Name + ":", a_list);
        }

        private static object GetFieldValue(FieldInfo field, object obj)
        {
            try
            {
                return field.GetValue(obj);
            }
            catch (MemberAccessException)
            {
                // probably JDK 1
                // never mind this field
                return null;
            }
        }

        private static bool AreArraysEqual(object compare, object with, string path, ArrayList
            a_list)
        {
            var len = Runtime.GetArrayLength(compare);
            if (len != Runtime.GetArrayLength(with))
            {
                return false;
            }
            for (var j = 0; j < len; j++)
            {
                var elementCompare = Runtime.GetArrayValue(compare, j);
                var elementWith = Runtime.GetArrayValue(with, j);
                if (!IsEqual(elementCompare, elementWith, path, a_list))
                {
                    return false;
                }
            }
            return true;
        }

        private static string GetPath(object a_compare, object a_with, string a_path)
        {
            if (a_path != null && a_path.Length > 0)
            {
                return a_path;
            }
            if (a_compare != null)
            {
                return a_compare.GetType().FullName + ":";
            }
            if (a_with != null)
            {
                return a_with.GetType().FullName + ":";
            }
            return a_path;
        }

        internal static bool HasPublicConstructor(Type a_class)
        {
            if (a_class != typeof (string))
            {
                try
                {
                    return Activator.CreateInstance(a_class) != null;
                }
                catch
                {
                }
            }
            return false;
        }

        internal static object NormalizeNArray(object a_object)
        {
            if (Runtime.GetArrayLength(a_object) > 0)
            {
                var first = Runtime.GetArrayValue(a_object, 0);
                if (first != null && first.GetType().IsArray)
                {
                    var dim = ArrayDimensions(a_object);
                    object all = new object[ArrayElementCount(dim)];
                    NormalizeNArray1(a_object, all, 0, dim, 0);
                    return all;
                }
            }
            return a_object;
        }

        internal static int NormalizeNArray1(object a_object, object a_all, int a_next, int
            [] a_dim, int a_index)
        {
            if (a_index == a_dim.Length - 1)
            {
                for (var i = 0; i < a_dim[a_index]; i++)
                {
                    Runtime.SetArrayValue(a_all, a_next++, Runtime.GetArrayValue(a_object
                        , i));
                }
            }
            else
            {
                for (var i = 0; i < a_dim[a_index]; i++)
                {
                    a_next = NormalizeNArray1(Runtime.GetArrayValue(a_object, i), a_all, a_next
                        , a_dim, a_index + 1);
                }
            }
            return a_next;
        }

        internal static int[] ArrayDimensions(object a_object)
        {
            var count = 0;
            for (var clazz = a_object.GetType();
                clazz.IsArray;
                clazz = clazz.GetElementType
                    ())
            {
                count++;
            }
            var dim = new int[count];
            for (var i = 0; i < count; i++)
            {
                dim[i] = Runtime.GetArrayLength(a_object);
                a_object = Runtime.GetArrayValue(a_object, 0);
            }
            return dim;
        }

        internal static int ArrayElementCount(int[] a_dim)
        {
            var elements = a_dim[0];
            for (var i = 1; i < a_dim.Length; i++)
            {
                elements *= a_dim[i];
            }
            return elements;
        }
    }
}