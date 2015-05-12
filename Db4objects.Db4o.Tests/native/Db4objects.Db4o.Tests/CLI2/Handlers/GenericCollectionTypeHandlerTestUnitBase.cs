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
using System.Collections.Generic;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.native.Db4objects.Db4o.Internal;
using Db4objects.Db4o.Query;
using Db4objects.Db4o.Typehandlers;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.CLI2.Handlers
{
    public abstract class GenericCollectionTypeHandlerTestUnitBase : AbstractDb4oTestCase
    {
        protected ICollectionHelper _helper;

        protected override void Configure(IConfiguration config)
        {
            var collectionHandler = new GenericCollectionTypeHandler();

            RegisterHandlerFor(config, typeof (List<int>), collectionHandler);
            RegisterHandlerFor(config, typeof (LinkedList<int>), collectionHandler);
            RegisterHandlerFor(config, typeof (Stack<int>), collectionHandler);
            RegisterHandlerFor(config, typeof (Queue<int>), collectionHandler);

            _helper = NewCollectionHelper();
            config.ObjectClass(_helper.ItemType).CascadeOnDelete(true);
        }

        private static void RegisterHandlerFor(IConfiguration config, Type type, ITypeHandler4 collectionHandler)
        {
            var configImpl = (Config4Impl) config;
            var handler = configImpl.TypeHandlerForClass(configImpl.Reflector().ForClass(type), 0);

            if (handler == null)
            {
                config.RegisterTypeHandler(new GenericTypeHandlerPredicate(type.GetGenericTypeDefinition()),
                    collectionHandler);
                return;
            }

            if (!(handler is GenericCollectionTypeHandler))
            {
                throw new InvalidOperationException();
            }
        }

        protected override void Store()
        {
            Store(_helper.NewItem());
        }

        protected void AssertQueryResult(IList result, int count)
        {
            Assert.AreEqual(count, result.Count);
            _helper.AssertCollection(result[0]);
        }

        protected void AssertQuery(bool successful, object element, bool withContains)
        {
            var q = NewQuery(_helper.ItemType);
            var constraint = q.Descend(GenericCollectionTestFactory.FieldName).Constrain(element);
            if (withContains)
            {
                constraint.Contains();
            }
            AssertQueryResult(q, successful);
        }

        protected virtual void AssertQueryResult(IQuery q, bool successful)
        {
            if (successful)
            {
                AssertSuccessfulQueryResult(q);
            }
            else
            {
                AssertEmptyQueryResult(q);
            }
        }

        protected static void AssertEmptyQueryResult(IQuery q)
        {
            var set = q.Execute();
            Assert.AreEqual(0, set.Count);
        }

        protected void AssertSuccessfulQueryResult(IQuery q)
        {
            var set = q.Execute();
            Assert.AreEqual(1, set.Count);

            _helper.AssertCollection(set.Next());
        }

        protected void AssertCompareItems(object element, bool successful)
        {
            var item = _helper.NewItem(element);
            var q = NewQuery();
            q.Constrain(item);
            AssertQueryResult(q, successful);
        }

        protected static ICollectionHelper NewCollectionHelper()
        {
            var type = GenericCollectionTypeHandlerTestVariables.ElementSpec.Value.GetType().GetGenericArguments()[0];
            return (ICollectionHelper) Activator.CreateInstance(typeof (CollectionHelper<>).MakeGenericType(type));
        }

        protected static int Count(IEnumerable items)
        {
            var count = 0;
            for (var iterator = items.GetEnumerator(); iterator.MoveNext(); count++)
                ;

            return count;
        }

        protected static object FirstElement(IEnumerable items)
        {
            var enumerator = items.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                throw new ArgumentException("Collection has zero items.", "items");
            }

            return enumerator.Current;
        }

        protected static Type CollectionItemType()
        {
            return GenericCollectionTypeHandlerTestVariables.ElementSpec.Value.GetType().GetGenericArguments()[0];
        }

        protected static ICollection CollectionFor(object item)
        {
            return (ICollection) item.GetType().GetField(GenericCollectionTestFactory.FieldName).GetValue(item);
        }
    }
}