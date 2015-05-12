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
using System.Collections;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Instrumentation.Api;
using Db4objects.Db4o.Internal.Query;
using Db4objects.Db4o.NativeQueries.Expr;
using Db4objects.Db4o.NativeQueries.Expr.Cmp;
using Db4objects.Db4o.NativeQueries.Expr.Cmp.Operand;
using Db4objects.Db4o.NativeQueries.Optimization;
using Db4objects.Db4o.Query;

namespace Db4objects.Db4o.NativeQueries.Instrumentation
{
    public class SODAMethodBuilder
    {
        private const bool LogBytecode = false;
        public static readonly string OptimizeQueryMethodName = "optimizeQuery";
        private readonly ITypeEditor _editor;
        private IMethodBuilder _builder;
        private IMethodRef andRef;
        private IMethodRef constrainRef;
        private IMethodRef containsRef;
        private IMethodRef descendRef;
        private IMethodRef endsWithRef;
        private IMethodRef greaterRef;
        private IMethodRef identityRef;
        private IMethodRef notRef;
        private IMethodRef orRef;
        private IMethodRef smallerRef;
        private IMethodRef startsWithRef;

        public SODAMethodBuilder(ITypeEditor editor)
        {
            _editor = editor;
            BuildMethodReferences();
        }

        public virtual void InjectOptimization(IExpression expr)
        {
            _editor.AddInterface(TypeRef(typeof (IDb4oEnhancedFilter)));
            _builder = _editor.NewPublicMethod(PlatformName(OptimizeQueryMethodName), TypeRef
                (typeof (void)), new[] {TypeRef(typeof (IQuery))});
            var predicateClass = _editor.Type;
            expr.Accept(new SODAExpressionBuilder(this, predicateClass));
            _builder.Pop();
            _builder.EndMethod();
        }

        private ITypeRef TypeRef(Type type)
        {
            return _editor.References.ForType(type);
        }

        private string PlatformName(string name)
        {
            return NativeQueriesPlatform.ToPlatformName(name);
        }

        private void LoadArgument(int index)
        {
            _builder.LoadArgument(index);
        }

        private void Invoke(IMethodRef method)
        {
            _builder.Invoke(method, CallingConvention.Interface);
        }

        private void Ldc(object value)
        {
            _builder.Ldc(value);
        }

        private void BuildMethodReferences()
        {
            descendRef = MethodRef(typeof (IQuery), "descend", new[] {typeof (string)});
            constrainRef = MethodRef(typeof (IQuery), "constrain", new[]
            {
                typeof (object)
            });
            greaterRef = MethodRef(typeof (IConstraint), "greater", new Type[] {});
            smallerRef = MethodRef(typeof (IConstraint), "smaller", new Type[] {});
            containsRef = MethodRef(typeof (IConstraint), "contains", new Type[] {});
            startsWithRef = MethodRef(typeof (IConstraint), "startsWith", new[]
            {
                typeof (
                    bool)
            });
            endsWithRef = MethodRef(typeof (IConstraint), "endsWith", new[]
            {
                typeof (bool
                    )
            });
            notRef = MethodRef(typeof (IConstraint), "not", new Type[] {});
            andRef = MethodRef(typeof (IConstraint), "and", new[] {typeof (IConstraint)}
                );
            orRef = MethodRef(typeof (IConstraint), "or", new[] {typeof (IConstraint)});
            identityRef = MethodRef(typeof (IConstraint), "identity", new Type[] {});
        }

        private IMethodRef MethodRef(Type parent, string name, Type[] args)
        {
            try
            {
                return _editor.References.ForMethod(parent.GetMethod(PlatformName(name), args));
            }
            catch (Exception e)
            {
                throw new InstrumentationException(e);
            }
        }

        private class SODAExpressionBuilder : IExpressionVisitor
        {
            private readonly SODAMethodBuilder _enclosing;
            private readonly ITypeRef predicateClass;

            public SODAExpressionBuilder(SODAMethodBuilder _enclosing, ITypeRef predicateClass
                )
            {
                this._enclosing = _enclosing;
                this.predicateClass = predicateClass;
            }

            public virtual void Visit(AndExpression expression)
            {
                expression.Left().Accept(this);
                expression.Right().Accept(this);
                _enclosing.Invoke(_enclosing.andRef);
            }

            public virtual void Visit(BoolConstExpression expression)
            {
                LoadQuery();
            }

            public virtual void Visit(OrExpression expression)
            {
                expression.Left().Accept(this);
                expression.Right().Accept(this);
                _enclosing.Invoke(_enclosing.orRef);
            }

            public virtual void Visit(ComparisonExpression expression)
            {
                LoadQuery();
                Descend(FieldNames(expression.Left()));
                expression.Right().Accept(ComparisonEmitter());
                Constrain(expression.Op());
            }

            public virtual void Visit(NotExpression expression)
            {
                expression.Expr().Accept(this);
                _enclosing.Invoke(_enclosing.notRef);
            }

            //throw new RuntimeException("No boolean constants expected in parsed expression tree");
            private void LoadQuery()
            {
                _enclosing.LoadArgument(1);
            }

            private void Descend(IEnumerator fieldNames)
            {
                while (fieldNames.MoveNext())
                {
                    Descend(fieldNames.Current);
                }
            }

            private ComparisonBytecodeGeneratingVisitor ComparisonEmitter()
            {
                return new ComparisonBytecodeGeneratingVisitor(_enclosing._builder, predicateClass
                    );
            }

            private void Constrain(ComparisonOperator op)
            {
                _enclosing.Invoke(_enclosing.constrainRef);
                if (op.Equals(ComparisonOperator.ValueEquality))
                {
                    return;
                }
                if (op.Equals(ComparisonOperator.ReferenceEquality))
                {
                    _enclosing.Invoke(_enclosing.identityRef);
                    return;
                }
                if (op.Equals(ComparisonOperator.Greater))
                {
                    _enclosing.Invoke(_enclosing.greaterRef);
                    return;
                }
                if (op.Equals(ComparisonOperator.Smaller))
                {
                    _enclosing.Invoke(_enclosing.smallerRef);
                    return;
                }
                if (op.Equals(ComparisonOperator.Contains))
                {
                    _enclosing.Invoke(_enclosing.containsRef);
                    return;
                }
                if (op.Equals(ComparisonOperator.StartsWith))
                {
                    _enclosing.Ldc(1);
                    _enclosing.Invoke(_enclosing.startsWithRef);
                    return;
                }
                if (op.Equals(ComparisonOperator.EndsWith))
                {
                    _enclosing.Ldc(1);
                    _enclosing.Invoke(_enclosing.endsWithRef);
                    return;
                }
                throw new Exception("Cannot interpret constraint: " + op);
            }

            private void Descend(object fieldName)
            {
                _enclosing.Ldc(fieldName);
                _enclosing.Invoke(_enclosing.descendRef);
            }

            private IEnumerator FieldNames(FieldValue fieldValue)
            {
                var coll = new Collection4();
                IComparisonOperand curOp = fieldValue;
                while (curOp is FieldValue)
                {
                    var curField = (FieldValue) curOp;
                    coll.Prepend(curField.FieldName());
                    curOp = curField.Parent();
                }
                return coll.GetEnumerator();
            }
        }
    }
}