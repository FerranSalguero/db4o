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

namespace Db4objects.Db4o.Internal.Query
{
    public class NQOptimizationInfo
    {
        private readonly string _message;
        private readonly object _optimized;
        private readonly Predicate _predicate;

        public NQOptimizationInfo(Predicate predicate, string message
            , object optimized)
        {
            _predicate = predicate;
            _message = message;
            _optimized = optimized;
        }

        public virtual string Message()
        {
            return _message;
        }

        public virtual object Optimized()
        {
            return _optimized;
        }

        public virtual Predicate Predicate()
        {
            return _predicate;
        }

        public override string ToString()
        {
            return Message() + "/" + Optimized();
        }
    }
}