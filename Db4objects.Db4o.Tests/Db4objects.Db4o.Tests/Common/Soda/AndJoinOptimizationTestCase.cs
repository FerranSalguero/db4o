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

using Db4objects.Db4o.Internal.Query.Processor;
using Db4objects.Db4o.Query;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Soda
{
    public class AndJoinOptimizationTestCase : AbstractDb4oTestCase
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(new Data(1, "a"));
            Store(new Data(1, "b"));
            Store(new Data(2, "a"));
            Store(new Data(2, "b"));
        }

        public virtual void TestAndQuery()
        {
            var query = NewQuery(typeof (Data));
            query.Descend("_id").Constrain(1).And(query.Descend("_name").Constrain("a"));
            AssertJoins(query);
            Assert.AreEqual(1, query.Execute().Count);
            AssertNoJoins(query);
        }

        public virtual void TestOrQuery()
        {
            var query = NewQuery(typeof (Data));
            query.Descend("_id").Constrain(1).Or(query.Descend("_name").Constrain("a"));
            AssertJoins(query);
            Assert.AreEqual(3, query.Execute().Count);
            AssertJoins(query);
        }

        private void AssertNoJoins(IQuery query)
        {
            Assert.IsFalse(HasJoins(query));
        }

        private void AssertJoins(IQuery query)
        {
            Assert.IsTrue(HasJoins(query));
        }

        private bool HasJoins(IQuery query)
        {
            var constrIter = ((QQuery) query).IterateConstraints();
            while (constrIter.MoveNext())
            {
                if (HasJoins((QCon) constrIter.Current))
                {
                    return true;
                }
            }
            return false;
        }

        private bool HasJoins(QCon con)
        {
            if (con.HasJoins())
            {
                return true;
            }
            var childIter = con.IterateChildren();
            while (childIter.MoveNext())
            {
                if (HasJoins((QCon) childIter.Current))
                {
                    return true;
                }
            }
            return false;
        }

        public class Data
        {
            public int _id;
            public string _name;

            public Data(int id, string name)
            {
                _id = id;
                _name = name;
            }
        }
    }
}