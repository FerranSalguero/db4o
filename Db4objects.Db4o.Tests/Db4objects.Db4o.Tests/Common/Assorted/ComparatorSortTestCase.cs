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
using Db4objects.Db4o.Query;
using Db4oUnit;
using Db4oUnit.Extensions;
using Sharpen;

namespace Db4objects.Db4o.Tests.Common.Assorted
{
    public class ComparatorSortTestCase : AbstractDb4oTestCase
    {
        protected override void Configure(IConfiguration config)
        {
            config.ExceptionsOnNotStorable(true);
        }

        protected override void Store()
        {
            for (var i = 0; i < 4; i++)
            {
                Store(new Item(i, (3 - i).ToString()));
            }
        }

        public virtual void TestByIdAscending()
        {
            AssertIdOrder(new AscendingIdComparator(), new[]
            {
                0,
                1, 2, 3
            });
        }

        public virtual void TestByIdAscendingConstrained()
        {
            var query = NewItemQuery();
            query.Descend("_id").Constrain(3).Smaller();
            AssertIdOrder(query, new AscendingIdComparator(), new[
                ] {0, 1, 2});
        }

        public virtual void TestByIdAscendingNQ()
        {
            var result = Db().Query(new SmallerThanThreePredicate
                (), new AscendingIdComparator());
            AssertIdOrder(result, new[] {0, 1, 2});
        }

        public virtual void TestByIdDescending()
        {
            AssertIdOrder(new DescendingIdComparator(), new[]
            {
                3,
                2, 1, 0
            });
        }

        public virtual void TestByIdDescendingConstrained()
        {
            var query = NewItemQuery();
            query.Descend("_id").Constrain(3).Smaller();
            AssertIdOrder(query, new DescendingIdComparator(), new[] {2, 1, 0});
        }

        public virtual void TestByIdDescendingNQ()
        {
            var result = Db().Query(new SmallerThanThreePredicate
                (), new DescendingIdComparator());
            AssertIdOrder(result, new[] {2, 1, 0});
        }

        public virtual void TestByIdOddEven()
        {
            AssertIdOrder(new OddEvenIdComparator(), new[]
            {
                0, 2,
                1, 3
            });
        }

        public virtual void TestByIdOddEvenConstrained()
        {
            var query = NewItemQuery();
            query.Descend("_id").Constrain(3).Smaller();
            AssertIdOrder(query, new OddEvenIdComparator(), new[]
            {0, 2, 1});
        }

        public virtual void TestByIdOddEvenNQ()
        {
            var result = Db().Query(new SmallerThanThreePredicate
                (), new OddEvenIdComparator());
            AssertIdOrder(result, new[] {0, 2, 1});
        }

        public virtual void TestByNameAscending()
        {
            AssertIdOrder(new AscendingNameComparator(), new[]
            {
                3
                , 2, 1, 0
            });
        }

        public virtual void TestByNameAscendingConstrained()
        {
            var query = NewItemQuery();
            query.Descend("_id").Constrain(3).Smaller();
            AssertIdOrder(query, new AscendingNameComparator(), new[] {2, 1, 0});
        }

        public virtual void TestByNameAscendingNQ()
        {
            var result = Db().Query(new SmallerThanThreePredicate
                (), new AscendingNameComparator());
            AssertIdOrder(result, new[] {2, 1, 0});
        }

        private void AssertIdOrder(IQueryComparator comparator, int[] ids)
        {
            var query = NewItemQuery();
            AssertIdOrder(query, comparator, ids);
        }

        private IQuery NewItemQuery()
        {
            return NewQuery(typeof (Item));
        }

        private void AssertIdOrder(IQuery query, IQueryComparator comparator, int[] ids)
        {
            query.SortBy(comparator);
            var result = query.Execute();
            AssertIdOrder(result, ids);
        }

        private void AssertIdOrder(IObjectSet result, int[] ids)
        {
            Assert.AreEqual(ids.Length, result.Count);
            for (var idx = 0; idx < ids.Length; idx++)
            {
                Assert.AreEqual(ids[idx], ((Item) result.Next())._id);
            }
        }

        [Serializable]
        public class AscendingIdComparator : IQueryComparator
        {
            public virtual int Compare(object first, object second)
            {
                return ((Item) first)._id - ((Item) second
                    )._id;
            }
        }

        [Serializable]
        public class DescendingIdComparator : IQueryComparator
        {
            public virtual int Compare(object first, object second)
            {
                return ((Item) second)._id - ((Item)
                    first)._id;
            }
        }

        [Serializable]
        public class OddEvenIdComparator : IQueryComparator
        {
            public virtual int Compare(object first, object second)
            {
                var idA = ((Item) first)._id;
                var idB = ((Item) second)._id;
                var modA = idA%2;
                var modB = idB%2;
                if (modA != modB)
                {
                    return modA - modB;
                }
                return idA - idB;
            }
        }

        [Serializable]
        public class AscendingNameComparator : IQueryComparator
        {
            public virtual int Compare(object first, object second)
            {
                return Runtime.CompareOrdinal(((Item) first)._name,
                    ((Item) second)._name);
            }
        }

        [Serializable]
        public class SmallerThanThreePredicate : Predicate
        {
            public virtual bool Match(Item candidate)
            {
                return candidate._id < 3;
            }
        }

        public class Item
        {
            public int _id;
            public string _name;

            public Item() : this(0, null)
            {
            }

            public Item(int id, string name)
            {
                _id = id;
                _name = name;
            }
        }
    }
}