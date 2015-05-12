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
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;
using Db4oUnit.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Querying
{
    public class NoClassIndexQueryTestSuite : FixtureBasedTestSuite, IDb4oTestCase
    {
        private static readonly FixtureVariable queryMode = FixtureVariable.NewInstance("queryMode"
            );

        public override IFixtureProvider[] FixtureProviders()
        {
            return new IFixtureProvider[]
            {
                new Db4oFixtureProvider(), new SimpleFixtureProvider
                    (queryMode, new[]
                    {
                        new LabeledQueryMode
                            (QueryEvaluationMode.Immediate),
                        new LabeledQueryMode
                            (QueryEvaluationMode.Snapshot),
                        new LabeledQueryMode(
                            QueryEvaluationMode.Lazy)
                    })
            };
        }

        public override Type[] TestUnits()
        {
            return new[] {typeof (NoClassIndexQueryTestUnit)};
        }

        public class NoClassIndexQueryTestUnit : AbstractDb4oTestCase
        {
            /// <exception cref="System.Exception"></exception>
            protected override void Configure(IConfiguration config)
            {
                config.ObjectClass(typeof (Item
                    )).Indexed(false);
                config.Queries().EvaluationMode(((LabeledQueryMode) queryMode
                    .Value).Mode());
            }

            /// <exception cref="System.Exception"></exception>
            protected override void Store()
            {
                Store(new Item());
            }

            public virtual void Test()
            {
                var query = Db().Query(typeof (Item
                    ));
                Assert.AreEqual(0, query.Count);
            }

            public class Item
            {
            }
        }

        public class LabeledQueryMode : ILabeled
        {
            private readonly QueryEvaluationMode _mode;

            public LabeledQueryMode(QueryEvaluationMode mode)
            {
                _mode = mode;
            }

            public virtual string Label()
            {
                return _mode.ToString();
            }

            public virtual QueryEvaluationMode Mode()
            {
                return _mode;
            }
        }
    }
}