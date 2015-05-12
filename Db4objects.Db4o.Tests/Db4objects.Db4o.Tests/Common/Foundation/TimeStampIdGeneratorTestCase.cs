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

using Db4objects.Db4o.Foundation;
using Db4oUnit;
using Db4oUnit.Extensions.Util;

namespace Db4objects.Db4o.Tests.Common.Foundation
{
    public class TimeStampIdGeneratorTestCase : ITestCase
    {
        public virtual void TestObjectCounterPartOnlyUses6Bits()
        {
            var ids = GenerateIds();
            for (var i = 1; i < ids.Length; i++)
            {
                Assert.IsGreater(ids[i] - 1, ids[i]);
                var creationTime = TimeStampIdGenerator.IdToMilliseconds(ids[i]);
                var timePart = TimeStampIdGenerator.MillisecondsToId(creationTime);
                var objectCounter = ids[i] - timePart;
                // 6 bits
                Assert.IsSmallerOrEqual(Binary.LongForBits(6), objectCounter);
            }
        }

        private long[] GenerateIds()
        {
            var count = 500;
            var generator = new TimeStampIdGenerator();
            var ids = new long[count];
            for (var i = 0; i < ids.Length; i++)
            {
                ids[i] = generator.Generate();
            }
            return ids;
        }

        public virtual void TestContinousIncrement()
        {
            var generator = new TimeStampIdGenerator();
            AssertContinousIncrement(generator);
        }

        private void AssertContinousIncrement(TimeStampIdGenerator generator)
        {
            var oldId = generator.Generate();
            for (var i = 0; i < 1000000; i++)
            {
                var newId = generator.Generate();
                Assert.IsGreater(oldId, newId);
                oldId = newId;
            }
        }

        public virtual void TestTimeStaysTheSame()
        {
            TimeStampIdGenerator generatorWithSameTime = new _TimeStampIdGenerator_51();
            AssertContinousIncrement(generatorWithSameTime);
        }

        private sealed class _TimeStampIdGenerator_51 : TimeStampIdGenerator
        {
            protected override long Now()
            {
                return 1;
            }
        }
    }
}