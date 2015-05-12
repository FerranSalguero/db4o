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

#if !SILVERLIGHT
using Db4oUnit.Extensions;
using Db4oUnit;
using System.Collections;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Instrumentation.Cecil;
using Db4objects.Db4o.NativeQueries.Optimization;
using Db4objects.Db4o.Query;
using Db4objects.Db4o.NativeQueries;
#endif
using Db4objects.Db4o.Instrumentation.Core;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.CLI1.NativeQueries
{
    public class AbstractNativeQueriesTestCase : AbstractDb4oTestCase
    {
#if !SILVERLIGHT
        protected void AssertNQResult(object predicate, params object[] expected)
        {
            var os = QueryFromPredicate(predicate).Execute();
            var actualString = ToString(os);
            Assert.AreEqual(expected.Length, os.Count, "Expected: " + ToString(expected) + ", Actual: " + actualString);

            foreach (var item in expected)
            {
                Assert.IsTrue(os.Contains(item), "Expected item: " + item + " but got: " + actualString);
            }
        }

        private string ToString(IEnumerable os)
        {
            return Iterators.ToString(os.GetEnumerator());
        }

        private IQuery QueryFromPredicate(object predicate)
        {
            var match = predicate.GetType().GetMethod("Match");
            var expression = (new QueryExpressionBuilder()).FromMethod(match);
            var q = NewQuery(match.GetParameters()[0].ParameterType);
            new SODAQueryBuilder().OptimizeQuery(expression, q, predicate, new DefaultNativeClassFactory(),
                new CecilReferenceResolver());
            return q;
        }
#endif
    }
}