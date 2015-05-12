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
using System.Collections.Generic;
using Db4objects.Db4o.Query;

namespace Db4objects.Db4o.Linq.Internals
{
    internal class QueryBuilderContext
    {
        private readonly Stack<IConstraint> _constraints = new Stack<IConstraint>();
        private readonly Stack<IQuery> _descendStack = new Stack<IQuery>();
        private readonly IQuery _root;
        private Type _descendigFieldEnum;

        public QueryBuilderContext(IQuery root)
        {
            _root = root;
            CurrentQuery = _root;
        }

        public IQuery CurrentQuery { get; private set; }

        internal void PushDescendigFieldEnumType(Type descendigFieldEnum)
        {
            _descendigFieldEnum = descendigFieldEnum;
        }

        private Type PopDescendigFieldEnumType()
        {
            var returnType = _descendigFieldEnum;
            _descendigFieldEnum = null;

            return returnType;
        }

        public void PushConstraint(IConstraint constraint)
        {
            _constraints.Push(constraint);
        }

        public IConstraint PopConstraint()
        {
            return _constraints.Pop();
        }

        public void ApplyConstraint(Func<IConstraint, IConstraint> constraint)
        {
            PushConstraint(constraint(PopConstraint()));
        }

        internal object ResolveValue(object value)
        {
            var type = PopDescendigFieldEnumType();
            return (type != null) ? Enum.ToObject(type, value) : value;
        }

        public void Descend(string name)
        {
            CurrentQuery = CurrentQuery.Descend(name);
        }

        public void SaveQuery()
        {
            _descendStack.Push(CurrentQuery);
        }

        public void RestoreQuery()
        {
            CurrentQuery = _descendStack.Pop();
        }
    }
}