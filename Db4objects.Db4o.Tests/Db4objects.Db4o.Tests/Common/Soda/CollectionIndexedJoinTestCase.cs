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

using System.Collections;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Tests.Util;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Soda
{
    public class CollectionIndexedJoinTestCase : AbstractDb4oTestCase
    {
        private const int Numentries = 3;
        private static readonly string Collectionfieldname = "_data";
        private static readonly string Idfieldname = "_id";

        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(typeof (Data)).ObjectField(Idfieldname
                ).Indexed(true);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            for (var i = 0; i < Numentries; i++)
            {
                Store(new DataHolder(i));
            }
        }

        public virtual void TestIndexedOrTwo()
        {
            AssertIndexedOr(new[] {0, 1, -1}, 2);
        }

        private void AssertIndexedOr(int[] values, int expectedResultCount)
        {
            var config = new TestConfig
                (values.Length);
            while (config.MoveNext())
            {
                AssertIndexedOr(values, expectedResultCount, config.RootIndex(), config.ConnectLeft
                    ());
            }
        }

        public virtual void TestIndexedOrAll()
        {
            AssertIndexedOr(new[] {0, 1, 2}, 3);
        }

        public virtual void TestTwoJoinLegs()
        {
            var query = NewQuery(typeof (DataHolder)).Descend
                (Collectionfieldname);
            var left = query.Descend(Idfieldname).Constrain(0);
            left.Or(query.Descend(Idfieldname).Constrain(1));
            var right = query.Descend(Idfieldname).Constrain(2);
            right.Or(query.Descend(Idfieldname).Constrain(-1));
            left.Or(right);
            var result = query.Execute();
            Assert.AreEqual(3, result.Count);
        }

        public virtual void AssertIndexedOr(int[] values, int expectedResultCount, int rootIdx
            , bool connectLeft)
        {
            var query = NewQuery(typeof (DataHolder)).Descend
                (Collectionfieldname);
            var constraint = query.Descend(Idfieldname).Constrain(values[rootIdx]);
            for (var idx = 0; idx < values.Length; idx++)
            {
                if (idx != rootIdx)
                {
                    var curConstraint = query.Descend(Idfieldname).Constrain(values[idx]);
                    if (connectLeft)
                    {
                        constraint.Or(curConstraint);
                    }
                    else
                    {
                        curConstraint.Or(constraint);
                    }
                }
            }
            var result = query.Execute();
            Assert.AreEqual(expectedResultCount, result.Count);
        }

        public class DataHolder
        {
            public ArrayList _data;

            public DataHolder(int id)
            {
                _data = new ArrayList();
                _data.Add(new Data(id));
            }
        }

        public class Data
        {
            public int _id;

            public Data(int id)
            {
                _id = id;
            }
        }

        private class TestConfig : PermutingTestConfig
        {
            public TestConfig(int numValues) : base(new[]
            {
                new object[]
                {
                    0, numValues
                       - 1
                },
                new object[] {false, true}
            })
            {
            }

            public virtual int RootIndex()
            {
                return ((int) Current(0));
            }

            public virtual bool ConnectLeft()
            {
                return ((bool) Current(1));
            }
        }
    }
}