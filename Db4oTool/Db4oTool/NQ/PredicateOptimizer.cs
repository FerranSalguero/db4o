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

using Db4objects.Db4o.Query;
using Db4oTool.Core;
using Mono.Cecil;

namespace Db4oTool.NQ
{
    public class PredicateOptimizer : AbstractOptimizer
    {
        protected override void ProcessType(TypeDefinition type)
        {
            if (IsPredicateClass(type))
            {
                InstrumentPredicateClass(type);
            }
        }

        private void InstrumentPredicateClass(TypeDefinition type)
        {
            ++_processedCount;

            var match = GetMatchMethod(type);
            var e = GetExpression(match);
            if (null == e) return;

            OptimizePredicate(type, match, e);
        }

        private static MethodDefinition GetMatchMethod(TypeDefinition type)
        {
            return CecilReflector.GetMethod(type, "Match");
        }

        private static bool IsPredicateClass(TypeReference typeRef)
        {
            var type = typeRef as TypeDefinition;
            if (null == type) return false;
            var baseType = type.BaseType;
            if (null == baseType) return false;
            if (typeof (Predicate).FullName == baseType.FullName) return true;
            return IsPredicateClass(baseType);
        }

        protected override string TargetName(int processedCount)
        {
            return string.Format("predicate class{0}", processedCount == 1 ? "" : "es");
        }
    }
}