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
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Soda
{
    public class QueryUnknownClassTestCase : AbstractDb4oTestCase
    {
        public virtual void TestQueryUnknownClass()
        {
            var query = NewQuery(typeof (Data));
            query.Descend("_id").Constrain(42);
            var result = query.Execute();
            Assert.AreEqual(0, result.Count);
        }

        public virtual void TestQueryUnknownClassInUnknownCollection()
        {
            var query = NewQuery(typeof (DataHolder));
            query.Descend("_data").Descend("_id").Constrain(42);
            var result = query.Execute();
            Assert.AreEqual(0, result.Count);
        }

        public virtual void _testQueryUnknownClassInCollection()
        {
            Store(new DataHolder(new Unrelated
                (42)));
            var query = NewQuery(typeof (DataHolder));
            query.Descend("_data").Descend("_id").Constrain(42);
            var result = query.Execute();
            Assert.AreEqual(0, result.Count);
        }

        public virtual void _testQueryUnknownClassInCollectionConjunction()
        {
            Store(new DataHolder(new Unrelated
                (42)));
            var query = NewQuery(typeof (DataHolder));
            query.Descend("_data").Descend("_id").Constrain(42).And(query.Descend("_data").Descend
                ("_uid").Constrain(42));
            var result = query.Execute();
            Assert.AreEqual(0, result.Count);
        }

        public virtual void TestQueryUnknownClassInCollectionDisjunction()
        {
            Store(new DataHolder(new Unrelated
                (42)));
            var query = NewQuery(typeof (DataHolder));
            query.Descend("_data").Descend("_id").Constrain(42).Or(query.Descend("_data").Descend
                ("_uid").Constrain(42));
            var result = query.Execute();
            Assert.AreEqual(1, result.Count);
        }

        public class Data
        {
            public int _id;

            public Data(int id)
            {
                _id = id;
            }
        }

        public class DataHolder
        {
            public ArrayList _data;

            public DataHolder(object data)
            {
                _data = new ArrayList();
                _data.Add(data);
            }
        }

        public class Unrelated
        {
            public int _uid;

            public Unrelated(int id)
            {
                _uid = id;
            }
        }
    }
}