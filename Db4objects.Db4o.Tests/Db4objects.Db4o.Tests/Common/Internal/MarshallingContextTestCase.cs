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

using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Activation;
using Db4objects.Db4o.Internal.Marshall;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Internal
{
    public class MarshallingContextTestCase : AbstractDb4oTestCase
    {
        public static void Main(string[] arguments)
        {
            new MarshallingContextTestCase().RunSolo();
        }

        public virtual void TestStringItem()
        {
            var writtenItem = new StringItem
                ("one");
            var readItem = (StringItem
                ) WriteAndRead(writtenItem);
            Assert.AreEqual(writtenItem._name, readItem._name);
        }

        public virtual void TestStringIntItem()
        {
            var writtenItem = new StringIntItem
                ("one", 777);
            var readItem = (StringIntItem
                ) WriteAndRead(writtenItem);
            Assert.AreEqual(writtenItem._name, readItem._name);
            Assert.AreEqual(writtenItem._int, readItem._int);
        }

        public virtual void TestStringIntBooleanItem()
        {
            var writtenItem = new StringIntBooleanItem
                ("one", 777, true);
            var readItem = (StringIntBooleanItem
                ) WriteAndRead(writtenItem);
            Assert.AreEqual(writtenItem._name, readItem._name);
            Assert.AreEqual(writtenItem._int, readItem._int);
            Assert.AreEqual(writtenItem._bool, readItem._bool);
        }

        private object WriteAndRead(object obj)
        {
            var imaginativeID = 500;
            var @ref = new ObjectReference(ClassMetadataForObject(obj), imaginativeID
                );
            @ref.SetObject(obj);
            var marshallingContext = new MarshallingContext(Trans(), @ref, Container
                ().UpdateDepthProvider().ForDepth(int.MaxValue), true);
            Handlers4.Write(@ref.ClassMetadata().TypeHandler(), marshallingContext, obj);
            var pointer = marshallingContext.AllocateSlot();
            var buffer = marshallingContext.ToWriteBuffer(pointer);
            buffer.Seek(0);
            //        String str = new String(buffer._buffer);
            //        System.out.println(str);
            var unmarshallingContext = new UnmarshallingContext(Trans(), @ref
                , Const4.AddToIdTree, false);
            unmarshallingContext.Buffer(buffer);
            unmarshallingContext.ActivationDepth(new LegacyActivationDepth(5));
            return unmarshallingContext.Read();
        }

        private ClassMetadata ClassMetadataForObject(object obj)
        {
            return Container().ProduceClassMetadata(Reflector().ForObject(obj));
        }

        public class StringItem
        {
            public string _name;

            public StringItem(string name)
            {
                _name = name;
            }
        }

        public class StringIntItem
        {
            public int _int;
            public string _name;

            public StringIntItem(string name, int i)
            {
                _name = name;
                _int = i;
            }
        }

        public class StringIntBooleanItem
        {
            public bool _bool;
            public int _int;
            public string _name;

            public StringIntBooleanItem(string name, int i, bool @bool)
            {
                _name = name;
                _int = i;
                _bool = @bool;
            }
        }
    }
}