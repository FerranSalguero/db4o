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

namespace Db4objects.Db4o.NativeQueries.Expr.Cmp.Operand
{
    public class ArithmeticExpression : IComparisonOperand
    {
        private readonly IComparisonOperand _left;
        private readonly ArithmeticOperator _op;
        private readonly IComparisonOperand _right;

        public ArithmeticExpression(IComparisonOperand left, IComparisonOperand right, ArithmeticOperator
            op)
        {
            _op = op;
            _left = left;
            _right = right;
        }

        public virtual void Accept(IComparisonOperandVisitor visitor)
        {
            visitor.Visit(this);
        }

        public virtual IComparisonOperand Left()
        {
            return _left;
        }

        public virtual IComparisonOperand Right()
        {
            return _right;
        }

        public virtual ArithmeticOperator Op()
        {
            return _op;
        }

        public override string ToString()
        {
            return "(" + _left + _op + _right + ")";
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }
            var casted = (ArithmeticExpression
                ) obj;
            return _left.Equals(casted._left) && _right.Equals(casted._right) && _op.Equals(casted
                ._op);
        }

        public override int GetHashCode()
        {
            var hc = _left.GetHashCode();
            hc *= 29 + _right.GetHashCode();
            hc *= 29 + _op.GetHashCode();
            return hc;
        }
    }
}