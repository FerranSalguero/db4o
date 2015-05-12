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
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Classindex;
using Sharpen.Util;

namespace Db4objects.Db4o.Consistency
{
    /// <exclude></exclude>
    public sealed class ConsistencyCheckerUtil
    {
        private ConsistencyCheckerUtil()
        {
        }

        public static IDictionary TypesFor(LocalObjectContainer db, ISet ids
            )
        {
            IDictionary id2clazzes = new Hashtable();
            var iter = db.ClassCollection().Iterator();
            while (iter.MoveNext())
            {
                for (var idIter = ids.GetEnumerator(); idIter.MoveNext();)
                {
                    var id = ((int) idIter.Current);
                    var clazz = iter.CurrentClass();
                    var btree = BTreeClassIndexStrategy.Btree(clazz);
                    if (btree.Search(db.SystemTransaction(), id) != null)
                    {
                        var clazzes = ((ISet) id2clazzes[id]);
                        if (clazzes == null)
                        {
                            clazzes = new HashSet();
                            id2clazzes[id] = clazzes;
                        }
                        clazzes.Add(clazz);
                    }
                }
            }
            IDictionary id2clazz = new Hashtable();
            for (var idIter = id2clazzes.Keys.GetEnumerator(); idIter.MoveNext();)
            {
                var id = ((int) idIter.Current);
                var clazzes = ((ISet) id2clazzes[id]);
                ClassMetadata mostSpecific = null;
                for (var curClazzIter = clazzes.GetEnumerator(); curClazzIter.MoveNext();)
                {
                    var curClazz = ((ClassMetadata) curClazzIter.Current);
                    for (var cmpClazzIter = clazzes.GetEnumerator(); cmpClazzIter.MoveNext();)
                    {
                        var cmpClazz = ((ClassMetadata) cmpClazzIter.Current);
                        if (curClazz.Equals(cmpClazz._ancestor))
                        {
                            goto OUTER_continue;
                        }
                    }
                    mostSpecific = curClazz;
                    break;
                    OUTER_continue:
                    ;
                }
                id2clazz[id] = mostSpecific;
            }
            return id2clazz;
        }
    }
}