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

using System;
using System.Collections;

namespace Db4objects.Db4o.Config
{
    /// <exclude />
    public class TStack : IObjectTranslator
    {
        public void OnActivate(IObjectContainer objectContainer, object obj, object members)
        {
            var stack = (Stack) obj;
            if (members != null)
            {
                var elements = (object[]) members;
                for (var i = elements.Length - 1; i >= 0; i--)
                {
                    stack.Push(elements[i]);
                }
            }
        }

        public object OnStore(IObjectContainer objectContainer, object obj)
        {
            var stack = (Stack) obj;
            var count = stack.Count;
            var elements = new object[count];
            var e = stack.GetEnumerator();
            e.Reset();
            for (var i = 0; i < count; i++)
            {
                e.MoveNext();
                elements[i] = e.Current;
            }
            return elements;
        }

        public Type StoredClass()
        {
            return typeof (object[]);
        }
    }
}

#endif