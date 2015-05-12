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
using System.Globalization;
using System.Reflection;
using Db4oUnit;
using Sharpen.Lang;

namespace Db4objects.Db4o.Tests.SharpenLang
{
    internal class Generic<T>
    {
        public class Inner
        {
            public class Inner2<S>
            {
            }
        }

        public class InnerGeneric<S>
        {
            public class Inner2
            {
            }

            public class Generic<G>
            {
            }
        }
    }

    internal class SimpleGenericType<T>
    {
        public T Value;
    }

    internal class GenericType<T1, T2>
    {
        public T1 First;
        public T2 Second;

        public GenericType(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }

        public class NestedInGeneric
        {
        }
    }

    internal class TypeReferenceTestCase : ITestCase
    {
        private class __Funny123Name_
        {
        }

        private class NestedType
        {
        }

        public void TestRoundTripOnOpenType()
        {
            AssertGenericType(
                typeof (Generic<>.Inner),
                typeof (Generic<>.Inner.Inner2<>),
                typeof (Generic<>.InnerGeneric<>));
        }

        public void TestRoundTripOnInnerGenericType()
        {
            AssertGenericType(
                typeof (Generic<int>.Inner),
                typeof (Generic<int>.Inner.Inner2<string[]>),
                typeof (Generic<int>.Inner.Inner2<Generic<int>.Inner>),
                typeof (Generic<int>.InnerGeneric<NestedType>),
                typeof (Generic<int[]>.InnerGeneric<NestedType>),
                typeof (Generic<int>.InnerGeneric<NestedType>.Inner2),
                typeof (Generic<int>.InnerGeneric<NestedType>.Generic<int>));
        }

        private static void AssertGenericType(params Type[] types)
        {
            foreach (var genericType in types)
            {
                EnsureRoundtrip(genericType);
            }
        }

        public void TestFunnyName()
        {
            EnsureRoundtrip(typeof (__Funny123Name_));
        }

        public void TestSimpleName()
        {
            var stringName = TypeReference.FromString("System.String");
            Assert.AreEqual("System.String", stringName.SimpleName);
            Assert.IsTrue(stringName.AssemblyName == null);
            Assert.AreEqual(typeof (string), stringName.Resolve());
        }

        public void TestVoidPointer()
        {
            var voidPointer = TypeReference.FromString("System.Void*");
            Assert.AreEqual("System.Void", voidPointer.SimpleName);
            Assert.IsTrue(voidPointer is PointerTypeReference);
            Assert.AreEqual(Type.GetType("System.Void*", true), voidPointer.Resolve());
        }

        public void TestNestedType()
        {
            var typeName = TypeReference.FromType(typeof (NestedType));
            Assert.AreEqual("Db4objects.Db4o.Tests.SharpenLang.TypeReferenceTestCase+NestedType", typeName.SimpleName);
            Assert.AreEqual(typeof (NestedType), typeName.Resolve());
        }

        public void TestWrongVersion()
        {
            var stringName = TypeReference.FromString("System.String, mscorlib, Version=1.14.27.0");
            Assert.AreEqual(typeof (string), stringName.Resolve());
        }

        public void TestAssemblyNameWithSpaces()
        {
            var typeReference =
                TypeReference.FromString("Foo, Business Objects, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            Assert.AreEqual("Foo", typeReference.SimpleName);
            Assert.AreEqual("Business Objects", typeReference.AssemblyName.Name);
        }

        public void TestAssemblyQualifiedName()
        {
            var assemblyNameString = "mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=969db8053d3322ac";
            var typeReference =
                TypeReference.FromString(
                    "System.String, " + assemblyNameString);
            Assert.AreEqual("System.String", typeReference.SimpleName);

            var assemblyName = new AssemblyName();
            assemblyName.Name = "mscorlib";
            assemblyName.Version = new Version(2, 0, 0, 0);
            assemblyName.CultureInfo = CultureInfo.InvariantCulture;
            assemblyName.SetPublicKeyToken(ParsePublicKeyToken("969db8053d3322ac"));
            Assert.AreEqual(assemblyName.FullName, typeReference.AssemblyName.FullName, "string.Assembly.FullName");
        }

        private static byte[] ParsePublicKeyToken(string token)
        {
            var len = token.Length/2;
            var bytes = new byte[len];
            for (var i = 0; i < len; ++i)
            {
                bytes[i] = byte.Parse(token.Substring(i*2, 2), NumberStyles.HexNumber);
            }
            return bytes;
        }

        public void TestSimpleArray()
        {
            EnsureRoundtrip(typeof (byte[]));
        }

        private static void EnsureRoundtrip(Type type)
        {
            var typeName = TypeReference.FromType(type);
            Assert.AreEqual(type, typeName.Resolve(), type.FullName);
        }

        public void TestJagged2DArray()
        {
            EnsureRoundtrip(typeof (byte[][]));
        }

        public void TestNumberAssemblyQualifiedName()
        {
            var assemblyNameString = "4ofus, Version=1.2.3.4, Culture=neutral";
            var typeReference = TypeReference.FromString("ForOfUs.Foo, " + assemblyNameString);

            Assert.AreEqual("ForOfUs.Foo", typeReference.SimpleName);

            var assemblyName = new AssemblyName();
            assemblyName.Name = "4ofus";
            assemblyName.Version = new Version(1, 2, 3, 4);
            assemblyName.CultureInfo = CultureInfo.InvariantCulture;
            Assert.AreEqual(assemblyName.FullName, typeReference.AssemblyName.FullName, "string.Assembly.FullName");
        }

        public void TestWeirdAssemblyQualifiedName()
        {
            var weirdAssemblyNames = new[]
            {
                "4starting-with-number",
                "{starting-with-open-brace",
                "}starting-with-close-brace",
                "1starting-with-number",
                "`starting-with-apostrophe1",
                "´starting-with-apostrophe2",
                "'starting-with-single-quotation-mark",
                "^starting-with-caret"
                //"with-comma-in-the\\,middle", // Not supported yet
                //"\\,starting-with-comma", // Not supported yet
            };

            foreach (var simpleName in weirdAssemblyNames)
            {
                var assemblyNameString = simpleName + ", Version=1.2.3.4, Culture=neutral";
                var typeReference = TypeReference.FromString("Namespace.TypeName, " + assemblyNameString);

                Assert.AreEqual("Namespace.TypeName", typeReference.SimpleName);

                var assemblyName = new AssemblyName();
                assemblyName.Name = simpleName;
                assemblyName.Version = new Version(1, 2, 3, 4);
                assemblyName.CultureInfo = CultureInfo.InvariantCulture;

                Assert.AreEqual(assemblyName.FullName, typeReference.AssemblyName.FullName, simpleName);
            }
        }

#if MONO
        public void _TestJaggedXDArray() { 
#else
        public void TestJaggedXDArray()
        {
#endif
            EnsureRoundtrip(typeof (byte[][][,]));
        }

        private class NestedGeneric<Key, Value>
        {
        }

#if CF
		public void _TestDeepGenericTypeName()
#else
        public void TestDeepGenericTypeName()
#endif
        {
            EnsureRoundtrip(typeof (Dictionary<string, List<string>>));
            EnsureRoundtrip(typeof (Dictionary<string, List<List<string>>>));

            EnsureRoundtrip(typeof (Dictionary<string, List<List<NestedType>>>));
            EnsureRoundtrip(typeof (NestedGeneric<string, List<string>[]>));
            EnsureRoundtrip(typeof (NestedGeneric<string, List<string>>[]));

            EnsureRoundtrip(typeof (GenericType<string, List<string>>.NestedInGeneric));
        }

        public void TestGenericArrays()
        {
            EnsureRoundtrip(typeof (SimpleGenericType<string>));
            EnsureRoundtrip(typeof (SimpleGenericType<int>[]));
            EnsureRoundtrip(typeof (SimpleGenericType<int>[,]));
            EnsureRoundtrip(typeof (SimpleGenericType<int>[][]));
#if !MONO
            EnsureRoundtrip(typeof (SimpleGenericType<int>[][,,]));
#endif
        }

        public void TestGenericOfArrays()
        {
            EnsureRoundtrip(typeof (SimpleGenericType<string[]>));
            EnsureRoundtrip(typeof (SimpleGenericType<string[]>[]));
#if !MONO
            EnsureRoundtrip(typeof (SimpleGenericType<string[,]>[][]));
#endif
            EnsureRoundtrip(typeof (SimpleGenericType<string[][]>[]));
            EnsureRoundtrip(typeof (SimpleGenericType<string[][]>[][]));
#if !MONO
            EnsureRoundtrip(typeof (SimpleGenericType<SimpleGenericType<string[][]>[][,]>[][]));
#endif
        }

        public void TestUnversionedGenericName()
        {
            var simpleAssemblyName = GetExecutingAssemblySimpleName();
            var t = typeof (GenericType<int, GenericType<int, string>>);
            var tn = TypeReference.FromString(t.AssemblyQualifiedName);
            Assert.AreEqual(
                "Db4objects.Db4o.Tests.SharpenLang.GenericType`2[[System.Int32, mscorlib], [Db4objects.Db4o.Tests.SharpenLang.GenericType`2[[System.Int32, mscorlib], [System.String, mscorlib]], " +
                simpleAssemblyName + "]], " + simpleAssemblyName,
                tn.GetUnversionedName());
        }

        public void TestGenericName()
        {
            var o = new GenericType<int, string>(3, "42");
            var t = Type.GetType(o.GetType().FullName);

            var stringName = TypeReference.FromString(typeof (string).AssemblyQualifiedName);

            var genericTypeName = (GenericTypeReference) TypeReference.FromString(t.AssemblyQualifiedName);
            Assert.AreEqual("Db4objects.Db4o.Tests.SharpenLang.GenericType`2", genericTypeName.SimpleName);
            Assert.AreEqual(2, genericTypeName.GenericArguments.Length);

            Assert.AreEqual(TypeReference.FromString(typeof (int).AssemblyQualifiedName),
                genericTypeName.GenericArguments[0]);
            Assert.AreEqual(stringName, genericTypeName.GenericArguments[1]);

            var complexType = typeof (GenericType<string, GenericType<int, string>>);
            var complexTypeName = (GenericTypeReference) TypeReference.FromString(complexType.AssemblyQualifiedName);
            Assert.AreEqual(genericTypeName.SimpleName, complexTypeName.SimpleName);
            Assert.AreEqual(genericTypeName.AssemblyName.FullName, complexTypeName.AssemblyName.FullName);
            Assert.AreEqual(2, complexTypeName.GenericArguments.Length);
            Assert.AreEqual(stringName, complexTypeName.GenericArguments[0]);
            Assert.AreEqual(genericTypeName, complexTypeName.GenericArguments[1]);

            Assert.AreEqual(typeof (string), TypeReference.FromString("System.String, mscorlib").Resolve());
            Assert.AreEqual(t,
                TypeReference.FromString(
                    "Db4objects.Db4o.Tests.SharpenLang.GenericType`2[[System.Int32, mscorlib],[System.String, mscorlib]], " +
                    GetExecutingAssemblySimpleName()).Resolve());
        }

        private static string GetExecutingAssemblySimpleName()
        {
            return Assembly.GetExecutingAssembly().GetName().Name;
        }
    }
}