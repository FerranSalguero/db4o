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
using System.Collections;
using Db4objects.Db4o.Internal;
using Sharpen.Util;

namespace Db4objects.Db4o.Filestats
{
    /// <exclude></exclude>
    public class ClassNode
    {
        private readonly ClassMetadata _clazz;
        private readonly ISet _subClasses = new HashSet();

        public ClassNode(ClassMetadata clazz)
        {
            _clazz = clazz;
        }

        public static ISet BuildHierarchy(ClassMetadataRepository repository
            )
        {
            var classIter = repository.Iterator();
            IDictionary nodes = new Hashtable();
            ISet roots = new HashSet();
            while (classIter.MoveNext())
            {
                var clazz = classIter.CurrentClass();
                var node = new ClassNode
                    (clazz);
                nodes[clazz.GetName()] = node;
                if (clazz.GetAncestor() == null)
                {
                    roots.Add(node);
                }
            }
            for (var nodeIter = nodes.Values.GetEnumerator(); nodeIter.MoveNext();)
            {
                var node = ((ClassNode)
                    nodeIter.Current);
                var ancestor = node.ClassMetadata().GetAncestor
                    ();
                if (ancestor != null)
                {
                    ((ClassNode) nodes[ancestor.GetName()]).AddSubClass(node
                        );
                }
            }
            return roots;
        }

        public virtual ClassMetadata ClassMetadata()
        {
            return _clazz;
        }

        internal virtual void AddSubClass(ClassNode node)
        {
            _subClasses.Add(node);
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return _clazz.GetName().Equals(((ClassNode) obj)._clazz.
                GetName());
        }

        public override int GetHashCode()
        {
            return _clazz.GetName().GetHashCode();
        }

        public virtual IEnumerable SubClasses()
        {
            return _subClasses;
        }
    }
}

#endif // !SILVERLIGHT