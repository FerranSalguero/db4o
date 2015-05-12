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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Delete;
using Db4objects.Db4o.Internal.Handlers;
using Db4objects.Db4o.Internal.Marshall;
using Db4objects.Db4o.Marshall;
using Db4objects.Db4o.Reflect;
using Db4objects.Db4o.Typehandlers;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Handlers
{
    public class CustomTypeHandlerTestCase : AbstractDb4oTestCase
    {
        private static readonly int[] Data = {1, 2};

        public static void Main(string[] arguments)
        {
            new CustomTypeHandlerTestCase().RunSolo();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            RegisterTypeHandler(config, typeof (Item), new CustomItemTypeHandler
                (this));
            RegisterTypeHandler(config, typeof (ItemGrandChild), new
                CustomItemGrandChildTypeHandler(this));
        }

        private void RegisterTypeHandler(IConfiguration config, Type clazz, ITypeHandler4
            typeHandler)
        {
            var reflector = ((Config4Impl) config).Reflector();
            var itemClass = reflector.ForClass(clazz);
            ITypeHandlerPredicate predicate = new _ITypeHandlerPredicate_229(itemClass);
            config.RegisterTypeHandler(predicate, typeHandler);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(StoredItem());
            Store(StoredItemChild());
            Store(StoredItemGrandChild());
        }

        public virtual void TestRetrieveOnlyInstance()
        {
            Assert.AreEqual(StoredItem(), RetrieveItemOfClass(typeof (Item
                )));
        }

        public virtual void TestChildClass()
        {
            Assert.AreEqual(StoredItemChild(), RetrieveItemOfClass(typeof (ItemChild
                )));
        }

        public virtual void TestGrandChildClass()
        {
            Assert.AreEqual(StoredItemGrandChild(), RetrieveItemOfClass(typeof (ItemGrandChild
                )));
        }

        public virtual void TestStoredFields()
        {
            var storedClass = Db().StoredClass(typeof (Item
                ));
            var storedFields = storedClass.GetStoredFields();
            Assert.AreEqual(0, storedFields.Length);
        }

        private Item RetrieveItemOfClass(Type class1)
        {
            var q = NewQuery(class1);
            var retrievedItem = (Item) q.
                Execute().Next();
            return retrievedItem;
        }

        private Item StoredItem()
        {
            return new Item(Data);
        }

        private Item StoredItemChild()
        {
            return new ItemChild("child", Data);
        }

        private Item StoredItemGrandChild()
        {
            return new ItemGrandChild(25, "child", Data);
        }

        internal virtual IReflectClass ItemClass()
        {
            return Reflector().ForClass(typeof (Item));
        }

        private sealed class CustomItemTypeHandler : IReferenceTypeHandler, ICascadingTypeHandler
            , IVariableLengthTypeHandler
        {
            private readonly CustomTypeHandlerTestCase _enclosing;

            internal CustomItemTypeHandler(CustomTypeHandlerTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void CascadeActivation(IActivationContext context)
            {
            }

            public void CollectIDs(QueryingReadContext context)
            {
            }

            public ITypeHandler4 ReadCandidateHandler(QueryingReadContext context)
            {
                return null;
            }

            public void Write(IWriteContext context, object obj)
            {
                var item = (Item) obj;
                if (item.numbers == null)
                {
                    context.WriteInt(-1);
                    return;
                }
                context.WriteInt(item.numbers.Length);
                for (var i = 0; i < item.numbers.Length; i++)
                {
                    context.WriteInt(item.numbers[i]);
                }
            }

            public void Activate(IReferenceActivationContext context)
            {
                var item = (Item) ((UnmarshallingContext
                    ) context).PersistentObject();
                var elementCount = context.ReadInt();
                if (elementCount == -1)
                {
                    return;
                }
                item.numbers = new int[elementCount];
                for (var i = 0; i < item.numbers.Length; i++)
                {
                    item.numbers[i] = context.ReadInt();
                }
            }

            /// <exception cref="Db4objects.Db4o.Ext.Db4oIOException"></exception>
            public void Delete(IDeleteContext context)
            {
            }

            public void Defragment(IDefragmentContext context)
            {
            }

            public IPreparedComparison PrepareComparison(IContext context, object obj)
            {
                return new _IPreparedComparison_33();
            }

            private sealed class _IPreparedComparison_33 : IPreparedComparison
            {
                public int CompareTo(object obj)
                {
                    return 0;
                }
            }
        }

        private sealed class CustomItemGrandChildTypeHandler : IReferenceTypeHandler, ICascadingTypeHandler
            , IVariableLengthTypeHandler
        {
            private readonly CustomTypeHandlerTestCase _enclosing;

            internal CustomItemGrandChildTypeHandler(CustomTypeHandlerTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void CascadeActivation(IActivationContext context)
            {
            }

            // TODO Auto-generated method stub
            public void CollectIDs(QueryingReadContext context)
            {
            }

            // TODO Auto-generated method stub
            public ITypeHandler4 ReadCandidateHandler(QueryingReadContext context)
            {
                // TODO Auto-generated method stub
                return null;
            }

            public void Write(IWriteContext context, object obj)
            {
                var item = (ItemGrandChild
                    ) obj;
                context.WriteInt(item.age);
                context.WriteInt(100);
            }

            public void Activate(IReferenceActivationContext context)
            {
                var item = (ItemGrandChild
                    ) context.PersistentObject();
                item.age = context.ReadInt();
                var check = context.ReadInt();
                if (check != 100)
                {
                    throw new InvalidOperationException();
                }
            }

            /// <exception cref="Db4objects.Db4o.Ext.Db4oIOException"></exception>
            public void Delete(IDeleteContext context)
            {
            }

            public void Defragment(IDefragmentContext context)
            {
            }

            public IPreparedComparison PrepareComparison(IContext context, object obj)
            {
                return new _IPreparedComparison_87();
            }

            private sealed class _IPreparedComparison_87 : IPreparedComparison
            {
                public int CompareTo(object obj)
                {
                    return 0;
                }
            }
        }

        public class Item
        {
            public int[] numbers;

            public Item(int[] numbers_)
            {
                numbers = numbers_;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Item))
                {
                    return false;
                }
                return AreEqual(numbers, ((Item) obj).numbers);
            }

            private bool AreEqual(int[] expected, int[] actual)
            {
                if (expected == null)
                {
                    return actual == null;
                }
                if (actual == null)
                {
                    return false;
                }
                if (expected.Length != actual.Length)
                {
                    return false;
                }
                for (var i = 0; i < expected.Length; i++)
                {
                    if (expected[i] != actual[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public class ItemChild : Item
        {
            public string name;

            public ItemChild(string name_, int[] numbers_) : base(numbers_)
            {
                name = name_;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is ItemChild))
                {
                    return false;
                }
                var other = (ItemChild)
                    obj;
                if (name == null)
                {
                    if (other.name != null)
                    {
                        return false;
                    }
                    return base.Equals(obj);
                }
                if (!name.Equals(other.name))
                {
                    return false;
                }
                return base.Equals(obj);
            }
        }

        public class ItemGrandChild : ItemChild
        {
            public int age;

            public ItemGrandChild(int age_, string name_, int[] numbers_) : base(name_, numbers_
                )
            {
                age = age_;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is ItemGrandChild))
                {
                    return false;
                }
                var other = (ItemGrandChild
                    ) obj;
                if (age != other.age)
                {
                    return false;
                }
                return base.Equals(obj);
            }
        }

        private sealed class _ITypeHandlerPredicate_229 : ITypeHandlerPredicate
        {
            private readonly IReflectClass itemClass;

            public _ITypeHandlerPredicate_229(IReflectClass itemClass)
            {
                this.itemClass = itemClass;
            }

            public bool Match(IReflectClass classReflector)
            {
                return itemClass.Equals(classReflector);
            }
        }
    }
}