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
    public class TList : IObjectTranslator
    {
        public void OnActivate(IObjectContainer objectContainer, object obj, object members)
        {
            var list = (IList) obj;
            list.Clear();
            if (members != null)
            {
                var elements = (object[]) members;
                for (var i = 0; i < elements.Length; i++)
                {
                    list.Add(elements[i]);
                }
            }
        }

        public object OnStore(IObjectContainer objectContainer, object obj)
        {
            var list = (IList) obj;
            var elements = new object[list.Count];
            for (var i = 0; i < list.Count; i++)
            {
                elements[i] = list[i];
            }
            return elements;
        }

        public Type StoredClass()
        {
            return typeof (object[]);
        }
    }
}