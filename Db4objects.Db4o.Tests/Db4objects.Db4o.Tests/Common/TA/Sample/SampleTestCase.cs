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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Reflect;
using Db4objects.Db4o.TA;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;
using Sharpen;

namespace Db4objects.Db4o.Tests.Common.TA.Sample
{
    public class SampleTestCase : AbstractDb4oTestCase, IOptOutTA, IOptOutDefragSolo
    {
        private long countryID;
        private Db4oUUID countryUUID;
        private long customerID;
        private Db4oUUID customerUUID;

        public static void Main(string[] args)
        {
            new SampleTestCase().RunAll();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.Add(new TransparentActivationSupport());
            config.GenerateUUIDs(ConfigScope.Globally);
        }

        protected override void Store()
        {
            var customer = new Customer();
            customer._name = "db4objects";
            var address = new Address();
            customer._addresses = new[] {address};
            var country = new Country();
            address._country = country;
            address._firstLine = "Suite 350";
            var state = new State();
            country._states = new[] {state};
            state._name = "California";
            var city = new City();
            state._cities = new[] {city};
            Store(customer);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Db4oSetupAfterStore()
        {
            object customer = ((Customer) RetrieveOnlyInstance(typeof (Customer)));
            object country = ((Country) RetrieveOnlyInstance(typeof (Country)));
            customerID = Db().GetID(customer);
            countryID = Db().GetID(country);
            customerUUID = Db().GetObjectInfo(customer).GetUUID();
            countryUUID = Db().GetObjectInfo(country).GetUUID();
            Reopen();
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestRetrieveNonActivatable()
        {
            CheckGraphActivation((Customer) RetrieveOnlyInstance(typeof (Customer))
                );
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestRetrieveActivatable()
        {
            CheckGraphActivation((Country) RetrieveOnlyInstance(typeof (Country)));
        }

        public virtual void TestPeekPersisted()
        {
            Run(new _IActAndAssert_75(this));
        }

        public virtual void TestFullActivation()
        {
            Run(new _IActAndAssert_87(this));
        }

        public virtual void TestRefresh()
        {
            Run(new _IActAndAssert_100(this));
        }

        public virtual void TestDeactivate()
        {
            Run(new _IActAndAssert_122(this));
        }

        public virtual void TestGetById()
        {
            AssertIsDeactivated(CountryByID());
            AssertIsDeactivated(CustomerByID());
        }

        public virtual void TestGetByUUID()
        {
            AssertIsDeactivated(Db().GetByUUID(countryUUID));
            AssertIsDeactivated(Db().GetByUUID(customerUUID));
        }

        public virtual IEnumerator IterateGraphStringFields(object obj)
        {
            return Iterators.Concat(Iterators.Map(IterateGraph(obj), new _IFunction4_146(this
                )));
        }

        internal virtual Customer CustomerByID()
        {
            return (Customer) Db().GetByID(customerID);
        }

        internal virtual Country CountryByID()
        {
            return (Country) Db().GetByID(countryID);
        }

        // A small evil multimethod hack to have "Do What I mean" behaviour. 
        internal virtual IEnumerator IterateGraph(object obj)
        {
            if (obj is IEnumerator)
            {
                return (IEnumerator) obj;
            }
            if (obj is Customer)
            {
                return IterateGraph((Customer) obj);
            }
            return IterateGraph((Country) obj);
        }

        private IEnumerator IterateGraph(Customer customer)
        {
            return new CompositeIterator4(new[]
            {
                IterateGraph(customer._addresses
                    [0]._country),
                new ArrayIterator4(new object[]
                {
                    customer._addresses[0]._country
                    , customer._addresses[0], customer
                })
            });
        }

        private IEnumerator IterateGraph(Country country)
        {
            return new ArrayIterator4(new object[]
            {
                country._states[0]._cities[0], country._states
                    [0],
                country
            });
        }

        /// <exception cref="System.Exception"></exception>
        private void CheckGraphActivation(Customer customer)
        {
            AssertIsActivated(customer);
            AssertIsNotNull(customer, "_name");
            AssertIsNotNull(customer, "_addresses");
            var address = customer._addresses[0];
            AssertIsActivated(address);
            AssertIsNotNull(address, "_firstLine");
            AssertIsNotNull(address, "_country");
            CheckGraphActivation(address._country);
        }

        /// <exception cref="System.Exception"></exception>
        private void CheckGraphActivation(Country country)
        {
            AssertIsDeactivated(country);
            AssertIsNull(country, "_states");
            var state = country.GetState("94403");
            AssertIsActivated(state);
            AssertIsNotNull(country, "_states");
            AssertIsNotNull(state, "_name");
            AssertIsNotNull(state, "_cities");
            var city = state._cities[0];
            Assert.IsNotNull(city);
            AssertIsDeactivated(city);
            AssertIsNull(city, "_name");
        }

        internal virtual void AssertIsDeactivated(object obj)
        {
            var i = IterateFieldValues(obj);
            while (i.MoveNext())
            {
                Assert.IsNull(i.Current);
            }
            Assert.IsFalse(Db().IsActive(obj));
            Assert.IsTrue(Db().IsStored(obj));
        }

        private void AssertIsActivated(object obj)
        {
            var i = IterateFieldValues(obj);
            while (i.MoveNext())
            {
                Assert.IsNotNull(i.Current);
            }
            Assert.IsTrue(Db().IsActive(obj));
            Assert.IsTrue(Db().IsStored(obj));
        }

        internal virtual IEnumerator IterateStringFieldsOnObject(object obj)
        {
            var stringClass = Reflector().ForClass(typeof (string));
            return MapPersistentFields(obj, new _IFunction4_236(stringClass, obj));
        }

        private IEnumerator IterateFieldValues(object obj)
        {
            return MapPersistentFields(obj, new _IFunction4_248(obj));
        }

        private IEnumerator MapPersistentFields(object obj, IFunction4 function)
        {
            return Iterators.Map(IteratePersistentFields(obj), function);
        }

        private IEnumerator IteratePersistentFields(object obj)
        {
            return Iterators.Filter(DeclaredFields(obj), new _IPredicate4_266());
        }

        private IReflectField[] DeclaredFields(object obj)
        {
            var claxx = Reflector().ForObject(obj);
            return claxx.GetDeclaredFields();
        }

        /// <exception cref="System.Exception"></exception>
        private void AssertIsNull(object obj, string fieldName)
        {
            Assert.IsTrue(FieldIsNull(obj, fieldName));
        }

        /// <exception cref="System.Exception"></exception>
        private void AssertIsNotNull(object obj, string fieldName)
        {
            Assert.IsFalse(FieldIsNull(obj, fieldName));
        }

        /// <exception cref="System.Exception"></exception>
        private bool FieldIsNull(object obj, string fieldName)
        {
            var clazz = obj.GetType();
            var field = Runtime.GetDeclaredField(clazz, fieldName);
            Assert.IsNotNull(field);
            return field.GetValue(obj) == null;
        }

        private void Run(IActAndAssert actAndAssert)
        {
            Run(actAndAssert, CustomerByID());
            Run(actAndAssert, CountryByID());
        }

        private void Run(IActAndAssert actAndAssert, object obj)
        {
            var i = IterateGraph(actAndAssert.ActOnRoot(obj));
            while (i.MoveNext())
            {
                actAndAssert.AssertOnLeaves(i.Current);
            }
        }

        private sealed class _IActAndAssert_75 : IActAndAssert
        {
            private readonly SampleTestCase _enclosing;

            public _IActAndAssert_75(SampleTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public object ActOnRoot(object obj)
            {
                return _enclosing.Db().PeekPersisted(obj, int.MaxValue, true);
            }

            public void AssertOnLeaves(object obj)
            {
                Assert.IsNotNull(obj);
                Assert.IsFalse(_enclosing.Db().IsStored(obj));
            }
        }

        private sealed class _IActAndAssert_87 : IActAndAssert
        {
            private readonly SampleTestCase _enclosing;

            public _IActAndAssert_87(SampleTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public object ActOnRoot(object obj)
            {
                _enclosing.Db().Activate(obj, int.MaxValue);
                return obj;
            }

            public void AssertOnLeaves(object obj)
            {
                Assert.IsNotNull(obj);
                Assert.IsTrue(_enclosing.Db().IsStored(obj));
            }
        }

        private sealed class _IActAndAssert_100 : IActAndAssert
        {
            private readonly SampleTestCase _enclosing;

            public _IActAndAssert_100(SampleTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public object ActOnRoot(object obj)
            {
                _enclosing.Db().Activate(obj, int.MaxValue);
                var i = _enclosing.IterateGraphStringFields(obj);
                while (i.MoveNext())
                {
                    var fieldOnObject = (FieldOnObject) i.Current;
                    fieldOnObject._field.Set(fieldOnObject._object, "modified");
                }
                _enclosing.Db().Refresh(obj, int.MaxValue);
                return obj;
            }

            public void AssertOnLeaves(object obj)
            {
                var i = _enclosing.IterateStringFieldsOnObject(obj);
                while (i.MoveNext())
                {
                    var fieldOnObject = (FieldOnObject) i.Current;
                    Assert.AreNotEqual("modified", fieldOnObject._field.Get(fieldOnObject._object));
                }
            }
        }

        private sealed class _IActAndAssert_122 : IActAndAssert
        {
            private readonly SampleTestCase _enclosing;

            public _IActAndAssert_122(SampleTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public object ActOnRoot(object obj)
            {
                _enclosing.Db().Activate(obj, int.MaxValue);
                var graph = _enclosing.IterateGraph(obj);
                _enclosing.Db().Deactivate(obj, int.MaxValue);
                return graph;
            }

            public void AssertOnLeaves(object obj)
            {
                _enclosing.AssertIsDeactivated(obj);
            }
        }

        private sealed class _IFunction4_146 : IFunction4
        {
            private readonly SampleTestCase _enclosing;

            public _IFunction4_146(SampleTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public object Apply(object current)
            {
                return _enclosing.IterateStringFieldsOnObject(current);
            }
        }

        private sealed class _IFunction4_236 : IFunction4
        {
            private readonly object obj;
            private readonly IReflectClass stringClass;

            public _IFunction4_236(IReflectClass stringClass, object obj)
            {
                this.stringClass = stringClass;
                this.obj = obj;
            }

            public object Apply(object current)
            {
                var field = (IReflectField) current;
                if (field.GetFieldType() != stringClass)
                {
                    return Iterators.Skip;
                }
                return new FieldOnObject(field, obj);
            }
        }

        private sealed class _IFunction4_248 : IFunction4
        {
            private readonly object obj;

            public _IFunction4_248(object obj)
            {
                this.obj = obj;
            }

            public object Apply(object current)
            {
                var field = (IReflectField) current;
                try
                {
                    return field.Get(obj);
                }
                catch (Exception e)
                {
                    throw new Db4oException(e);
                }
            }
        }

        private sealed class _IPredicate4_266 : IPredicate4
        {
            public bool Match(object candidate)
            {
                var field = (IReflectField) candidate;
                return !field.IsTransient() && !field.IsStatic();
            }
        }

        public class FieldOnObject
        {
            public readonly IReflectField _field;
            public readonly object _object;

            public FieldOnObject(IReflectField field, object obj)
            {
                _field = field;
                _object = obj;
            }
        }

        public interface IActAndAssert
        {
            object ActOnRoot(object obj);
            void AssertOnLeaves(object obj);
        }
    }
}