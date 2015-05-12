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
using Db4objects.Db4o.Reflect.Net;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.CLI1.Reflect.Net
{
    public class CustomTransientAttribute : Attribute
    {
        public override string ToString()
        {
            return "CustomTransient";
        }
    }

    public class WhateverAttribute : Attribute
    {
    }

    public class NetFieldTestCase : ITestCase, ITestLifeCycle
    {
        public void SetUp()
        {
            NetField.ResetTransientMarkers();
        }

        public void TearDown()
        {
        }

        public void TestIsTransientRefusesRawFields()
        {
            AssertIsNotTransient("RawField");
        }

        public void TestIsTransientUnderstandsNonSerialized()
        {
            AssertIsTransient("NonSerializedField");
        }

        public void TestIsTransientUnderstandsDb4oTransient()
        {
            AssertIsTransient("Db4oTransientField");
        }

        public void TestIsTransientUnderstandsCustomTransient()
        {
            AssertIsNotTransient("CustomTransientField");
            AssertIsNotTransient("FieldWithCustomAttribute");

            NetField.MarkTransient(typeof (CustomTransientAttribute));

            AssertIsTransient("CustomTransientField");
            AssertIsNotTransient("FieldWithCustomAttribute");
        }

        private static void AssertIsNotTransient(string fieldName)
        {
            Assert.IsFalse(IsTransient(fieldName));
        }

        private static void AssertIsTransient(string fieldName)
        {
            Assert.IsTrue(IsTransient(fieldName));
        }

        private static bool IsTransient(string name)
        {
            return NetField.IsTransient(typeof (Item).GetField(name));
        }

        public class Item
        {
            [CustomTransient] public object CustomTransientField;

            [Transient] public object Db4oTransientField;

            [Whatever] public object FieldWithCustomAttribute;

            [NonSerialized] public object NonSerializedField;

            public object RawField;
        }
    }
}