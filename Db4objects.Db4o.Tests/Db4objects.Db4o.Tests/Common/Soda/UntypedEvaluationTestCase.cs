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
using Db4objects.Db4o.Query;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Soda
{
    public class UntypedEvaluationTestCase : AbstractDb4oTestCase
    {
        private static readonly Type Extent = typeof (object);

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(new Data(42));
        }

        public virtual void TestUntypedRaw()
        {
            var query = NewQuery(Extent);
            Assert.AreEqual(1, query.Execute().Count);
        }

        public virtual void TestUntypedEvaluationNone()
        {
            var query = NewQuery(Extent);
            query.Constrain(new UntypedEvaluation(false));
            Assert.AreEqual(0, query.Execute().Count);
        }

        public virtual void TestUntypedEvaluationAll()
        {
            var query = NewQuery(Extent);
            query.Constrain(new UntypedEvaluation(true));
            Assert.AreEqual(1, query.Execute().Count);
        }

        public class Data
        {
            public int _id;

            public Data(int id)
            {
                // replace with Data.class -> green
                _id = id;
            }
        }

        [Serializable]
        public class UntypedEvaluation : IEvaluation
        {
            public bool _value;

            public UntypedEvaluation(bool value)
            {
                _value = value;
            }

            public virtual void Evaluate(ICandidate candidate)
            {
                candidate.Include(_value);
            }
        }
    }
}