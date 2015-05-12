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
    public class BoolConstExpression : IExpression
    {
        public static readonly BoolConstExpression True
            = new BoolConstExpression(true);

        public static readonly BoolConstExpression False
            = new BoolConstExpression(false);

        private readonly bool _value;

        private BoolConstExpression(bool value)
        {
            _value = value;
        }

        public virtual void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public virtual bool Value()
        {
            return _value;
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public static BoolConstExpression Expr(bool value
            )
        {
            return (value ? True : False);
        }

        public virtual IExpression Negate()
        {
            return (_value ? False : True);
        }
    }
}