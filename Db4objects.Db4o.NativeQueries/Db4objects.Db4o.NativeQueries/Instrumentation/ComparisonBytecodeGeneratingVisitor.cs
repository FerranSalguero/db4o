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
using Db4objects.Db4o.Instrumentation.Api;
using Db4objects.Db4o.NativeQueries.Expr.Cmp;
using Db4objects.Db4o.NativeQueries.Expr.Cmp.Operand;

namespace Db4objects.Db4o.NativeQueries.Instrumentation
{
    internal class ComparisonBytecodeGeneratingVisitor : IComparisonOperandVisitor
    {
        private readonly IMethodBuilder _methodBuilder;
        private readonly ITypeRef _predicateClass;
        private bool _inArithmetic;
        private ITypeRef _opClass;
        private ITypeRef _staticRoot;

        public ComparisonBytecodeGeneratingVisitor(IMethodBuilder methodBuilder, ITypeRef
            predicateClass)
        {
            _methodBuilder = methodBuilder;
            _predicateClass = predicateClass;
        }

        public virtual void Visit(ConstValue operand)
        {
            var value = operand.Value();
            if (value != null)
            {
                _opClass = TypeRef(value.GetType());
            }
            _methodBuilder.Ldc(value);
            if (value != null)
            {
                Box(_opClass, !_inArithmetic);
            }
        }

        public virtual void Visit(FieldValue fieldValue)
        {
            var lastFieldClass = fieldValue.Field.Type;
            var needConversion = lastFieldClass.IsPrimitive;
            fieldValue.Parent().Accept(this);
            if (_staticRoot != null)
            {
                _methodBuilder.LoadStaticField(fieldValue.Field);
                _staticRoot = null;
                return;
            }
            _methodBuilder.LoadField(fieldValue.Field);
            Box(lastFieldClass, !_inArithmetic && needConversion);
        }

        public virtual void Visit(CandidateFieldRoot root)
        {
            _methodBuilder.LoadArgument(1);
        }

        public virtual void Visit(PredicateFieldRoot root)
        {
            _methodBuilder.LoadArgument(0);
        }

        public virtual void Visit(StaticFieldRoot root)
        {
            _staticRoot = root.Type;
        }

        public virtual void Visit(ArrayAccessValue operand)
        {
            var cmpType = DeduceFieldClass(operand.Parent()).ElementType;
            operand.Parent().Accept(this);
            var outerInArithmetic = _inArithmetic;
            _inArithmetic = true;
            operand.Index().Accept(this);
            _inArithmetic = outerInArithmetic;
            _methodBuilder.LoadArrayElement(cmpType);
            Box(cmpType, !_inArithmetic);
        }

        public virtual void Visit(MethodCallValue operand)
        {
            var method = operand.Method;
            var retType = method.ReturnType;
            // FIXME: this should be handled within conversions
            var needConversion = retType.IsPrimitive;
            operand.Parent().Accept(this);
            var oldInArithmetic = _inArithmetic;
            for (var paramIdx = 0; paramIdx < operand.Args.Length; paramIdx++)
            {
                _inArithmetic = operand.Method.ParamTypes[paramIdx].IsPrimitive;
                operand.Args[paramIdx].Accept(this);
            }
            _inArithmetic = oldInArithmetic;
            _methodBuilder.Invoke(method, operand.CallingConvention);
            Box(retType, !_inArithmetic && needConversion);
        }

        public virtual void Visit(ArithmeticExpression operand)
        {
            var oldInArithmetic = _inArithmetic;
            _inArithmetic = true;
            operand.Left().Accept(this);
            operand.Right().Accept(this);
            var operandType = ArithmeticType(operand);
            switch (operand.Op().Id())
            {
                case ArithmeticOperator.AddId:
                {
                    _methodBuilder.Add(operandType);
                    break;
                }

                case ArithmeticOperator.SubtractId:
                {
                    _methodBuilder.Subtract(operandType);
                    break;
                }

                case ArithmeticOperator.MultiplyId:
                {
                    _methodBuilder.Multiply(operandType);
                    break;
                }

                case ArithmeticOperator.DivideId:
                {
                    _methodBuilder.Divide(operandType);
                    break;
                }

                case ArithmeticOperator.ModuloId:
                {
                    _methodBuilder.Modulo(operandType);
                    break;
                }

                default:
                {
                    throw new Exception("Unknown operand: " + operand.Op());
                }
            }
            Box(_opClass, !oldInArithmetic);
            _inArithmetic = oldInArithmetic;
        }

        private ITypeRef TypeRef(Type type)
        {
            return _methodBuilder.References.ForType(type);
        }

        // FIXME: need to map dX,fX,...
        private void Box(ITypeRef boxedType, bool canApply)
        {
            if (!canApply)
            {
                return;
            }
            _methodBuilder.Box(boxedType);
        }

        private ITypeRef DeduceFieldClass(IComparisonOperand fieldValue)
        {
            var visitor = new TypeDeducingVisitor(_methodBuilder.References,
                _predicateClass);
            fieldValue.Accept(visitor);
            return visitor.OperandClass();
        }

        private ITypeRef ArithmeticType(IComparisonOperand operand)
        {
            if (operand is ConstValue)
            {
                return PrimitiveType(((ConstValue) operand).Value().GetType());
            }
            if (operand is FieldValue)
            {
                return ((FieldValue) operand).Field.Type;
            }
            if (operand is ArithmeticExpression)
            {
                var expr = (ArithmeticExpression) operand;
                var left = ArithmeticType(expr.Left());
                var right = ArithmeticType(expr.Right());
                if (left == DoubleType() || right == DoubleType())
                {
                    return DoubleType();
                }
                if (left == FloatType() || right == FloatType())
                {
                    return FloatType();
                }
                if (left == LongType() || right == LongType())
                {
                    return LongType();
                }
                return IntType();
            }
            return null;
        }

        private ITypeRef PrimitiveType(Type klass)
        {
            if (klass == typeof (int) || klass == typeof (short) || klass == typeof (bool) || klass
                == typeof (byte))
            {
                return IntType();
            }
            if (klass == typeof (double))
            {
                return DoubleType();
            }
            if (klass == typeof (float))
            {
                return FloatType();
            }
            if (klass == typeof (long))
            {
                return LongType();
            }
            return TypeRef(klass);
        }

        private ITypeRef IntType()
        {
            return TypeRef(typeof (int));
        }

        private ITypeRef LongType()
        {
            return TypeRef(typeof (long));
        }

        private ITypeRef FloatType()
        {
            return TypeRef(typeof (float));
        }

        private ITypeRef DoubleType()
        {
            return TypeRef(typeof (double));
        }
    }
}