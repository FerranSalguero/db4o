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

namespace Db4objects.Db4o.NativeQueries.Expr
{
    public class NotExpression : IExpression
    {
        private readonly IExpression _expr;

        public NotExpression(IExpression expr)
        {
            _expr = expr;
        }

        public virtual void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return "!(" + _expr + ")";
        }

        public virtual IExpression Expr()
        {
            return _expr;
        }

        public override bool Equals(object other)
        {
            if (this == other)
            {
                return true;
            }
            if (other == null || GetType() != other.GetType())
            {
                return false;
            }
            var casted = (NotExpression
                ) other;
            return _expr.Equals(casted._expr);
        }

        public override int GetHashCode()
        {
            return -_expr.GetHashCode();
        }
    }
}