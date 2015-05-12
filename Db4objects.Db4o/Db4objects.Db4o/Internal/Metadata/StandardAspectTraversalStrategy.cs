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

namespace Db4objects.Db4o.Internal.Metadata
{
    /// <exclude></exclude>
    public class StandardAspectTraversalStrategy : IAspectTraversalStrategy
    {
        private readonly ClassMetadata _classMetadata;

        public StandardAspectTraversalStrategy(ClassMetadata classMetadata)
        {
            _classMetadata = classMetadata;
        }

        public virtual void TraverseAllAspects(ITraverseAspectCommand command)
        {
            var classMetadata = _classMetadata;
            var currentSlot = 0;
            while (classMetadata != null)
            {
                var aspectCount = command.DeclaredAspectCount(classMetadata);
                for (var i = 0; i < aspectCount && !command.Cancelled(); i++)
                {
                    command.ProcessAspect(classMetadata._aspects[i], currentSlot);
                    currentSlot++;
                }
                if (command.Cancelled())
                {
                    return;
                }
                classMetadata = classMetadata._ancestor;
            }
        }
    }
}