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
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Query;
using Db4objects.Db4o.IO;
using Db4oUnit;
using Sharpen;

namespace Db4objects.Db4o.Tests.Common.Internal.Query
{
    public class SodaQueryComparatorBenchmark
    {
        private const int ObjectCount = 10000;
        private const int Iterations = 10;

        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            for (var i = 0; i < 2; ++i)
            {
                BenchmarkOneField();
                BenchmarkTwoFields();
            }
        }

        private static void BenchmarkTwoFields()
        {
            var sqc = Time(new _IProcedure4_55());
            Runtime.Out.WriteLine(" SQC(2): " + sqc + "ms");
            var soda = Time(new _IProcedure4_77());
            Runtime.Out.WriteLine("SODA(2): " + soda + "ms");
        }

        private static void BenchmarkOneField()
        {
            var sqc = Time(new _IProcedure4_91());
            Runtime.Out.WriteLine(" SQC(1): " + sqc + "ms");
            var soda = Time(new _IProcedure4_112());
            Runtime.Out.WriteLine("SODA(1): " + soda + "ms");
        }

        protected static void ConsumeAll(IEnumerable items)
        {
            for (var itemIter = items.GetEnumerator(); itemIter.MoveNext();)
            {
                var item = itemIter.Current;
                Assert.IsNotNull(item);
            }
        }

        private static long Time(IProcedure4 procedure4)
        {
            var storage = new PagingMemoryStorage();
            StoreItems(storage);
            StopWatch stopWatch = new AutoStopWatch();
            for (var i = 0; i < Iterations; ++i)
            {
                ApplyProcedure(storage, procedure4);
            }
            return stopWatch.Peek();
        }

        private static void ApplyProcedure(PagingMemoryStorage storage, IProcedure4 procedure4
            )
        {
            var config = Db4oEmbedded.NewConfiguration();
            config.File.Storage = storage;
            var container = Db4oEmbedded.OpenFile(config, "benchmark.db4o"
                );
            try
            {
                procedure4.Apply(container);
            }
            finally
            {
                container.Close();
            }
        }

        private static void StoreItems(PagingMemoryStorage storage)
        {
            var config = Db4oEmbedded.NewConfiguration();
            config.File.Storage = storage;
            var container = Db4oEmbedded.OpenFile(config, "benchmark.db4o"
                );
            try
            {
                for (var i = 0; i < ObjectCount; ++i)
                {
                    container.Store(new Item(i, "Item " + i, new ItemChild
                        ("Child " + i)));
                }
            }
            finally
            {
                container.Close();
            }
        }

        public class Item
        {
            public ItemChild child;
            public int id;
            public string name;

            public Item(int id, string name, ItemChild child)
            {
                this.id = id;
                this.name = name;
                this.child = child;
            }
        }

        public class ItemChild
        {
            public string name;

            public ItemChild(string name)
            {
                this.name = name;
            }
        }

        private sealed class _IProcedure4_55 : IProcedure4
        {
            public void Apply(object container)
            {
                var localContainer = (LocalObjectContainer) ((IObjectContainer) container
                    );
                var comparator = new SodaQueryComparator(localContainer, typeof (Item
                    ), new[]
                    {
                        new SodaQueryComparator.Ordering(SodaQueryComparator.Direction
                            .Ascending, new[] {"name"}),
                        new SodaQueryComparator.Ordering(SodaQueryComparator.Direction
                            .Descending, new[] {"child", "name"})
                    });
                var query = ((IObjectContainer) container).Query();
                query.Constrain(typeof (Item));
                var sortedIds = comparator.Sort(query.Execute().Ext().GetIDs());
                for (var idIter = sortedIds.GetEnumerator(); idIter.MoveNext();)
                {
                    var id = ((int) idIter.Current);
                    Assert.IsNull(localContainer.GetActivatedObjectFromCache(localContainer.Transaction
                        , id));
                }
            }
        }

        private sealed class _IProcedure4_77 : IProcedure4
        {
            public void Apply(object container)
            {
                var query = ((IObjectContainer) container).Query();
                query.Constrain(typeof (Item));
                query.Descend("name").OrderAscending();
                query.Descend("child").Descend("name").OrderDescending();
                ConsumeAll(query.Execute());
            }
        }

        private sealed class _IProcedure4_91 : IProcedure4
        {
            public void Apply(object container)
            {
                var localContainer = (LocalObjectContainer) ((IObjectContainer) container
                    );
                var comparator = new SodaQueryComparator(localContainer, typeof (Item
                    ), new[]
                    {
                        new SodaQueryComparator.Ordering(SodaQueryComparator.Direction
                            .Ascending, new[] {"name"})
                    });
                var query = ((IObjectContainer) container).Query();
                query.Constrain(typeof (Item));
                var sortedIds = comparator.Sort(query.Execute().Ext().GetIDs());
                for (var idIter = sortedIds.GetEnumerator(); idIter.MoveNext();)
                {
                    var id = ((int) idIter.Current);
                    Assert.IsNull(localContainer.GetActivatedObjectFromCache(localContainer.Transaction
                        , id));
                }
            }
        }

        private sealed class _IProcedure4_112 : IProcedure4
        {
            public void Apply(object container)
            {
                var query = ((IObjectContainer) container).Query();
                query.Constrain(typeof (Item));
                query.Descend("name").OrderAscending();
                ConsumeAll(query.Execute());
            }
        }
    }
}