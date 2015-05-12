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

namespace Db4objects.Db4o.Config
{
    /// <exclude />
    public class TDictionary : IObjectTranslator
    {
        public void OnActivate(IObjectContainer objectContainer, object obj, object members)
        {
            var dict = (IDictionary) obj;
            dict.Clear();
            if (members != null)
            {
                var entries = (Entry[]) members;
                for (var i = 0; i < entries.Length; i++)
                {
                    if (entries[i].key != null && entries[i].value != null)
                    {
                        dict[entries[i].key] = entries[i].value;
                    }
                }
            }
        }

        public object OnStore(IObjectContainer objectContainer, object obj)
        {
            var dict = (IDictionary) obj;
            var entries = new Entry[dict.Count];
            var e = dict.GetEnumerator();
            e.Reset();
            for (var i = 0; i < dict.Count; i++)
            {
                e.MoveNext();
                entries[i] = new Entry();
                entries[i].key = e.Key;
                entries[i].value = e.Value;
            }
            return entries;
        }

        public Type StoredClass()
        {
            return typeof (Entry[]);
        }
    }
}