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
using Db4objects.Db4o.Foundation;

namespace Db4objects.Db4o.CS.Internal.Objectexchange
{
    public class SlotCollector
    {
        private readonly int _depth;
        private readonly IReferenceCollector _referenceCollector;
        private readonly ISlotAccessor _slotAccessor;
        private readonly IDictionary referenceCache = new Hashtable();

        public SlotCollector(int depth, IReferenceCollector collector, ISlotAccessor accessor
            )
        {
            if (depth < 1)
            {
                throw new ArgumentException();
            }
            _depth = depth;
            _slotAccessor = accessor;
            _referenceCollector = collector;
        }

        public virtual IList Collect(IEnumerator roots)
        {
            return ChildSlotsFor(roots);
        }

        private IList ChildSlotsFor(IEnumerator slots)
        {
            var result = new ArrayList();
            CollectSlots(slots, result, _depth);
            return result;
        }

        private void CollectSlots(IEnumerator ids, ArrayList result, int currentDepth)
        {
            while (ids.MoveNext())
            {
                var id = (((int) ids.Current));
                if (!ContainsSlotFor(result, id))
                {
                    result.Add(IdSlotPairFor(id));
                }
                if (currentDepth > 1)
                {
                    var childIds = CollectChildIdsFor(id);
                    CollectSlots(childIds, result, currentDepth - 1);
                }
            }
        }

        private bool ContainsSlotFor(ArrayList result, int id)
        {
            for (var pairIter = result.GetEnumerator(); pairIter.MoveNext();)
            {
                var pair = ((Pair) pairIter.Current);
                if ((((int) pair.first)) == id)
                {
                    return true;
                }
            }
            return false;
        }

        private IEnumerator CollectChildIdsFor(int id)
        {
            var references = ((Collection4) referenceCache[id]);
            if (null == references)
            {
                references = new Collection4(_referenceCollector.ReferencesFrom(id));
                referenceCache[id] = references;
            }
            return references.GetEnumerator();
        }

        private Pair IdSlotPairFor(int id)
        {
            return Pair.Of(id, _slotAccessor.CurrentSlotOfID(id));
        }
    }
}