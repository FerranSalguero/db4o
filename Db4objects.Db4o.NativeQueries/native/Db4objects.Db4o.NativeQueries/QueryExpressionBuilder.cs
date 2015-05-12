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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Cecil.FlowAnalysis;
using Cecil.FlowAnalysis.ActionFlow;
using Db4objects.Db4o.Activation;
using Db4objects.Db4o.Instrumentation.Cecil;
using Db4objects.Db4o.Internal.Query;
using Db4objects.Db4o.NativeQueries.Expr;
using Db4objects.Db4o.NativeQueries.Expr.Cmp;
using Db4objects.Db4o.NativeQueries.Expr.Cmp.Operand;
using Db4objects.Db4o.TA;
using Mono.Cecil;

namespace Db4objects.Db4o.NativeQueries
{
    using Ast = Cecil.FlowAnalysis.CodeStructure;
    using NQExpression = IExpression;

    /// <summary>
    ///     Build a Db4objects.Db4o.Nativequery.Expr tree out of a predicate method definition.
    /// </summary>
    public class QueryExpressionBuilder
    {
        protected static ICachingStrategy<string, AssemblyDefinition> _assemblyCachingStrategy =
            new SingleItemCachingStrategy<string, AssemblyDefinition>(
                delegate(string location) { return AssemblyDefinition.ReadAssembly(location); });

        protected static ICachingStrategy<MethodBase, IExpression> _expressionCachingStrategy =
            new SingleItemCachingStrategy<MethodBase, IExpression>(
                delegate(MethodBase method)
                {
                    var methodDef = GetMethodDefinition(method);
                    return AdjustBoxedValueTypes(FromMethodDefinition(methodDef));
                }
                );

        public NQExpression FromMethod(MethodBase method)
        {
            if (method == null) throw new ArgumentNullException("method");

            return GetCachedExpression(method);
        }

        private static NQExpression GetCachedExpression(MethodBase method)
        {
            return _expressionCachingStrategy.Get(method);
        }

        private static MethodDefinition GetMethodDefinition(MethodBase method)
        {
            var location = GetAssemblyLocation(method);
#if CF
			MethodDefinition methodDef = MethodDefinitionFor(method);
#else
            var assembly = _assemblyCachingStrategy.Get(location);

            var methodDef = (MethodDefinition) assembly.MainModule.LookupToken(method.MetadataToken);
#endif
            if (null == methodDef)
                UnsupportedPredicate(string.Format("Unable to load the definition of '{0}' from assembly '{1}'", method,
                    location));

            return methodDef;
        }

        private static MethodDefinition MethodDefinitionFor(MethodBase method)
        {
            var location = GetAssemblyLocation(method);
            var assembly = _assemblyCachingStrategy.Get(location);

#if CF
			TypeDefinition declaringType = FindTypeDefinition(assembly.MainModule, method.DeclaringType);
			if (declaringType == null)
			{
				return null;
			}

			foreach (MethodDefinition candidate in declaringType.Methods)
			{
				if (candidate.Name != method.Name) continue;
				if (candidate.Parameters.Count != method.GetParameters().Length) continue;
				if (!ParametersMatch(candidate.Parameters, GetParameterTypes(method, assembly.MainModule))) continue;
				{
					return candidate;
				}
			}

			return null;

#else
            return (MethodDefinition) assembly.MainModule.LookupToken(method.MetadataToken);
#endif
        }

        private static NQExpression AdjustBoxedValueTypes(NQExpression expression)
        {
            expression.Accept(new BoxedValueTypeProcessor());
            return expression;
        }

        private static IList<TypeReference> GetParameterTypes(MethodBase method, ModuleDefinition module)
        {
            IList<TypeReference> types = new List<TypeReference>();
            foreach (var parameter in ParametersFor(method))
            {
                types.Add(FindTypeDefinition(module, parameter.ParameterType));
            }

            return types;
        }

        private static ParameterInfo[] ParametersFor(MethodBase method)
        {
            if (method.IsGenericMethod)
            {
                var methodInfo = (MethodInfo) method;
                return methodInfo.GetGenericMethodDefinition().GetParameters();
            }

            return method.DeclaringType.IsGenericType
                ? method.DeclaringType.GetGenericTypeDefinition().GetMethod(method.Name).GetParameters()
                : method.GetParameters();
        }

        private static TypeDefinition FindTypeDefinition(ModuleDefinition module, Type type)
        {
            return IsNested(type)
                ? FindNestedTypeDefinition(module, type)
                : FindTypeDefinition(module, type.IsGenericType ? type.Name : type.FullName);
        }

        private static bool IsNested(Type type)
        {
            return type.IsNestedPublic || type.IsNestedPrivate || type.IsNestedAssembly;
        }

        private static TypeDefinition FindNestedTypeDefinition(ModuleDefinition module, Type type)
        {
            foreach (var td in FindTypeDefinition(module, type.DeclaringType).NestedTypes)
            {
                if (td.Name == type.Name) return td;
            }
            return null;
        }

        private static TypeDefinition FindTypeDefinition(ModuleDefinition module, string name)
        {
            return module.GetType(name);
        }

        private static string GetAssemblyLocation(MethodBase method)
        {
            return method.DeclaringType.Module.FullyQualifiedName;
        }

        public static NQExpression FromMethodDefinition(MethodDefinition method)
        {
            ValidatePredicateMethodDefinition(method);

            var expression = GetQueryExpression(method);
            if (null == expression) UnsupportedPredicate("No expression found.");

            var visitor = new Visitor(method, new AssemblyResolver(_assemblyCachingStrategy));
            expression.Accept(visitor);
            return visitor.Expression;
        }

        private static void ValidatePredicateMethodDefinition(MethodDefinition method)
        {
            if (method == null)
                throw new ArgumentNullException("method");
            if (1 != method.Parameters.Count)
                UnsupportedPredicate("A predicate must take a single argument.");
            if (0 != method.Body.ExceptionHandlers.Count)
                UnsupportedPredicate("A predicate can not contain exception handlers.");
            if (method.ReturnType.FullName != typeof (bool).FullName)
                UnsupportedPredicate("A predicate must have a boolean return type.");
        }

        private static Ast.Expression GetQueryExpression(MethodDefinition method)
        {
            var afg = FlowGraphFactory.CreateActionFlowGraph(FlowGraphFactory.CreateControlFlowGraph(method));
            return GetQueryExpression(afg);
        }

        private static void UnsupportedPredicate(string reason)
        {
            throw new UnsupportedPredicateException(reason);
        }

        private static void UnsupportedExpression(Ast.Expression node)
        {
            UnsupportedPredicate("Unsupported expression: " + Ast.ExpressionPrinter.ToString(node));
        }

        private static Ast.Expression GetQueryExpression(ActionFlowGraph afg)
        {
            IDictionary<int, Ast.Expression> variables = new Dictionary<int, Ast.Expression>();
            var block = afg.Blocks[0];
            while (block != null)
            {
                switch (block.ActionType)
                {
                    case ActionType.Invoke:
                        var invokeBlock = (InvokeActionBlock) block;
                        var invocation = invokeBlock.Expression;
                        if (IsActivateInvocation(invocation)
                            || IsNoSideEffectIndirectActivationInvocation(invocation))
                        {
                            block = invokeBlock.Next;
                            break;
                        }

                        UnsupportedExpression(invocation);
                        break;

                    case ActionType.ConditionalBranch:
                        UnsupportedPredicate("Conditional blocks are not supported.");
                        break;

                    case ActionType.Branch:
                        block = ((BranchActionBlock) block).Target;
                        break;

                    case ActionType.Assign:
                    {
                        var assignBlock = (AssignActionBlock) block;
                        var assign = assignBlock.AssignExpression;
                        var variable = assign.Target as Ast.VariableReferenceExpression;
                        if (null == variable)
                        {
                            UnsupportedExpression(assign);
                        }
                        else
                        {
                            if (variables.ContainsKey(variable.Variable.Index))
                                UnsupportedExpression(assign.Expression);

                            variables.Add(variable.Variable.Index, assign.Expression);
                            block = assignBlock.Next;
                        }
                        break;
                    }

                    case ActionType.Return:
                    {
                        var expression = ((ReturnActionBlock) block).Expression;
                        var variable = expression as Ast.VariableReferenceExpression;
                        return null == variable
                            ? expression
                            : variables[variable.Variable.Index];
                    }
                }
            }
            return null;
        }

        private static bool IsNoSideEffectIndirectActivationInvocation(Ast.MethodInvocationExpression invocation)
        {
            var methodDefinition = MethodDefinitionFor(invocation);
            if (null == methodDefinition) return false;
            var afg = FlowGraphFactory.CreateActionFlowGraph(FlowGraphFactory.CreateControlFlowGraph(methodDefinition));

            if (afg.Blocks.Count == 2 && afg.Blocks[0].ActionType == ActionType.Invoke)
            {
                var invocationBlock = (InvokeActionBlock) afg.Blocks[0];
                return IsActivateInvocation(invocationBlock.Expression);
            }

            return false;
        }

        private static MethodDefinition MethodDefinitionFor(Ast.MethodInvocationExpression invocation)
        {
            var methodRef = invocation.Target as Ast.MethodReferenceExpression;
            if (null == methodRef) return null;

            return GetMethodDefinition(methodRef);
        }

        private static bool IsActivateInvocation(Ast.MethodInvocationExpression invocation)
        {
            var methodRef = invocation.Target as Ast.MethodReferenceExpression;
            if (null == methodRef) return false;
            return IsActivateMethod(methodRef.Method);
        }

        private static bool IsActivateMethod(MethodReference method)
        {
            if (method.Name != "Activate") return false;
            return method.DeclaringType.FullName == typeof (IActivatable).FullName ||
                   IsOverridenActivateMethod(method);
        }

        private static bool IsOverridenActivateMethod(MethodReference method)
        {
            var declaringType = FindTypeDefinition(method.DeclaringType.Module, method.DeclaringType.FullName);
            if (!DeclaringTypeImplementsIActivatable(declaringType)) return false;
            if (method.Parameters.Count != 1 ||
                method.Parameters[0].ParameterType.FullName != typeof (ActivationPurpose).FullName) return false;

            return true;
        }

        private static bool DeclaringTypeImplementsIActivatable(TypeDefinition type)
        {
            foreach (var itf in type.Interfaces)
            {
                if (itf.FullName == typeof (IActivatable).FullName)
                {
                    return true;
                }
            }

            return false;
        }

        private static MethodDefinition GetMethodDefinition(Ast.MethodReferenceExpression methodRef)
        {
            var definition = methodRef.Method as MethodDefinition;
            return definition ?? LoadExternalMethodDefinition(methodRef);
        }

        private static MethodDefinition LoadExternalMethodDefinition(Ast.MethodReferenceExpression methodRef)
        {
            var method = methodRef.Method;
            var assemblyDef = new AssemblyResolver(_assemblyCachingStrategy).ForTypeReference(method.DeclaringType);
            var type = assemblyDef.MainModule.GetType(method.DeclaringType.FullName);
            return GetMethod(type, method);
        }

        private static MethodDefinition GetMethod(TypeDefinition type, MethodReference template)
        {
            foreach (var method in type.Methods)
            {
                if (method.Name != template.Name) continue;
                if (method.Parameters.Count != template.Parameters.Count) continue;
                if (!ParametersMatch(method.Parameters, template.Parameters)) continue;

                return method;
            }

            return null;
        }

#if CF
		private static bool ParametersMatch(Collection<ParameterDefinition> parameters, IList<TypeReference> templates)
		{
			return ParametersMatch(parameters, templates, delegate(ParameterDefinition candidate, TypeReference template)
			{
				return candidate.ParameterType.FullName == template.FullName;
			});
		}
#endif

        private static bool ParametersMatch(IList<ParameterDefinition> parameters, IList<ParameterDefinition> templates)
        {
            return ParametersMatch(parameters, templates,
                delegate(ParameterDefinition candidate, ParameterDefinition template)
                {
                    return candidate.ParameterType.FullName == template.ParameterType.FullName;
                });
        }

        private static bool ParametersMatch<T>(IList<ParameterDefinition> parameters, IList<T> templates,
            ParameterMatch<T> predicate)
        {
            if (parameters.Count != templates.Count) return false;

            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                if (!predicate(parameter, templates[i])) return false;
            }

            return true;
        }

        private delegate bool ParameterMatch<T>(ParameterDefinition candidate, T template);

        private class Visitor : Ast.AbstractCodeStructureVisitor
        {
            private readonly IList _methodDefinitionStack = new ArrayList();
            private readonly CecilReferenceProvider _referenceProvider;
            private object _current;
            private int _insideCandidate;

            public Visitor(MethodDefinition topLevelMethod, AssemblyResolver resolver)
            {
                EnterMethodDefinition(topLevelMethod);
                var assembly = resolver.ForType(topLevelMethod.DeclaringType);
                _referenceProvider = CecilReferenceProvider.ForModule(assembly.MainModule);
            }

            public NQExpression Expression
            {
                get
                {
                    var value = _current as ConstValue;
                    if (null != value)
                    {
                        return ToNQExpression(value);
                    }
                    return (NQExpression) _current;
                }
            }

            private bool InsideCandidate
            {
                get { return _insideCandidate > 0; }
            }

            private void EnterMethodDefinition(MethodDefinition method)
            {
                _methodDefinitionStack.Add(method);
            }

            private void LeaveMethodDefinition(MethodDefinition method)
            {
                var lastIndex = _methodDefinitionStack.Count - 1;
                var popped = _methodDefinitionStack[lastIndex];
                Debug.Assert(method == popped);
                _methodDefinitionStack.RemoveAt(lastIndex);
            }

            private static NQExpression ToNQExpression(ConstValue value)
            {
                if (IsTrue(value.Value())) return BoolConstExpression.True;
                return BoolConstExpression.False;
            }

            private static bool IsTrue(object o)
            {
                return ((IConvertible) o).ToBoolean(null);
            }

            public override void Visit(Ast.CastExpression node)
            {
                node.Target.Accept(this);
            }

            public override void Visit(Ast.AssignExpression node)
            {
                UnsupportedExpression(node);
            }

            public override void Visit(Ast.VariableReferenceExpression node)
            {
                UnsupportedExpression(node);
            }

            public override void Visit(Ast.ArgumentReferenceExpression node)
            {
                UnsupportedExpression(node);
            }

            public override void Visit(Ast.UnaryExpression node)
            {
                switch (node.Operator)
                {
                    case Ast.UnaryOperator.Not:
                        Visit(node.Operand);
                        Negate();
                        break;

                    default:
                        UnsupportedExpression(node);
                        break;
                }
            }

            public override void Visit(Ast.BinaryExpression node)
            {
                switch (node.Operator)
                {
                    case Ast.BinaryOperator.ValueEquality:
                        PushComparison(node.Left, node.Right, ComparisonOperator.ValueEquality);
                        break;

                    case Ast.BinaryOperator.ValueInequality:
                        PushComparison(node.Left, node.Right, ComparisonOperator.ValueEquality);
                        Negate();
                        break;

                    case Ast.BinaryOperator.LessThan:
                        PushComparison(node.Left, node.Right, ComparisonOperator.Smaller);
                        break;

                    case Ast.BinaryOperator.GreaterThan:
                        PushComparison(node.Left, node.Right, ComparisonOperator.Greater);
                        break;

                    case Ast.BinaryOperator.GreaterThanOrEqual:
                        PushComparison(node.Left, node.Right, ComparisonOperator.Smaller);
                        Negate();
                        break;

                    case Ast.BinaryOperator.LessThanOrEqual:
                        PushComparison(node.Left, node.Right, ComparisonOperator.Greater);
                        Negate();
                        break;

                    case Ast.BinaryOperator.LogicalOr:
                        Push(new OrExpression(Convert(node.Left), Convert(node.Right)));
                        break;

                    case Ast.BinaryOperator.LogicalAnd:
                        Push(new AndExpression(Convert(node.Left), Convert(node.Right)));
                        break;

                    default:
                        UnsupportedExpression(node);
                        break;
                }
            }

            private void Negate()
            {
                var top = (NQExpression) Pop();
                var topNot = top as NotExpression;
                if (topNot != null)
                {
                    Push(topNot.Expr());
                    return;
                }
                Push(new NotExpression(top));
            }

            private void PushComparison(Ast.Expression lhs, Ast.Expression rhs, ComparisonOperator op)
            {
                Visit(lhs);
                var left = Pop();
                Visit(rhs);
                var right = Pop();

                var areOperandsSwapped = IsCandidateFieldValue(right);
                if (areOperandsSwapped)
                {
                    var temp = left;
                    left = right;
                    right = temp;
                }

                AssertType(left, typeof (FieldValue), lhs);
                AssertType(right, typeof (IComparisonOperand), rhs);

                Push(new ComparisonExpression((FieldValue) left, (IComparisonOperand) right, op));

                if (areOperandsSwapped && !op.IsSymmetric())
                {
                    Negate();
                }
            }

            private static bool IsCandidateFieldValue(object o)
            {
                var value = o as FieldValue;
                if (value == null) return false;
                return value.Root() is CandidateFieldRoot;
            }

            public override void Visit(Ast.MethodInvocationExpression node)
            {
                var methodRef = node.Target as Ast.MethodReferenceExpression;
                if (null == methodRef)
                    UnsupportedExpression(node);

                var method = methodRef.Method;
                if (IsOperator(method))
                {
                    ProcessOperatorMethodInvocation(node, method);
                    return;
                }

                if (IsSystemString(method.DeclaringType))
                {
                    ProcessStringMethod(node, methodRef);
                    return;
                }

                ProcessRegularMethodInvocation(node, methodRef);
            }

            private static bool IsSystemString(TypeReference type)
            {
                return type.FullName == "System.String";
            }

            private void ProcessStringMethod(Ast.MethodInvocationExpression node,
                Ast.MethodReferenceExpression methodRef)
            {
                var method = methodRef.Method;

                if (method.Parameters.Count != 1
                    || !IsSystemString(method.Parameters[0].ParameterType))
                {
                    UnsupportedExpression(methodRef);
                }

                switch (method.Name)
                {
                    case "Contains":
                        PushComparison(methodRef.Target, node.Arguments[0], ComparisonOperator.Contains);
                        break;

                    case "StartsWith":
                        PushComparison(methodRef.Target, node.Arguments[0], ComparisonOperator.StartsWith);
                        break;

                    case "EndsWith":
                        PushComparison(methodRef.Target, node.Arguments[0], ComparisonOperator.EndsWith);
                        break;

                    case "Equals":
                        PushComparison(methodRef.Target, node.Arguments[0], ComparisonOperator.ValueEquality);
                        break;

                    default:
                        UnsupportedExpression(methodRef);
                        break;
                }
            }

            private void ProcessRegularMethodInvocation(Ast.MethodInvocationExpression node,
                Ast.MethodReferenceExpression methodRef)
            {
                if (node.Arguments.Count != 0)
                    UnsupportedExpression(node);

                var target = methodRef.Target;
                switch (target.CodeElementType)
                {
                    case Ast.CodeElementType.ThisReferenceExpression:
                        if (!InsideCandidate)
                            UnsupportedExpression(node);
                        ProcessCandidateMethodInvocation(node, methodRef);
                        break;

                    case Ast.CodeElementType.ArgumentReferenceExpression:
                        ProcessCandidateMethodInvocation(node, methodRef);
                        break;

                    default:
                        Push(ToFieldValue(target));
                        ProcessCandidateMethodInvocation(node, methodRef);
                        break;
                }
            }

            private void ProcessOperatorMethodInvocation(Ast.MethodInvocationExpression node,
                MemberReference methodReference)
            {
                switch (methodReference.Name)
                {
                    case "op_Equality":
                        PushComparison(node.Arguments[0], node.Arguments[1], ComparisonOperator.ValueEquality);
                        break;

                    case "op_Inequality":
                        PushComparison(node.Arguments[0], node.Arguments[1], ComparisonOperator.ValueEquality);
                        Negate();
                        break;

                    // XXX: check if the operations below are really supported for the
                    // data types in question
                    case "op_GreaterThanOrEqual":
                        PushComparison(node.Arguments[0], node.Arguments[1], ComparisonOperator.Smaller);
                        Negate();
                        break;

                    case "op_LessThanOrEqual":
                        PushComparison(node.Arguments[0], node.Arguments[1], ComparisonOperator.Greater);
                        Negate();
                        break;

                    case "op_LessThan":
                        PushComparison(node.Arguments[0], node.Arguments[1], ComparisonOperator.Smaller);
                        break;

                    case "op_GreaterThan":
                        PushComparison(node.Arguments[0], node.Arguments[1], ComparisonOperator.Greater);
                        break;

                    default:
                        UnsupportedExpression(node);
                        break;
                }
            }

            private void ProcessCandidateMethodInvocation(Ast.Expression methodInvocationExpression,
                Ast.MethodReferenceExpression methodRef)
            {
                var method = GetMethodDefinition(methodRef);
                if (null == method)
                    UnsupportedExpression(methodInvocationExpression);

                AssertMethodCanBeVisited(methodInvocationExpression, method);

                var expression = GetQueryExpression(method);
                if (null == expression)
                    UnsupportedExpression(methodInvocationExpression);

                EnterCandidateMethod(method);
                try
                {
                    Visit(expression);
                }
                finally
                {
                    LeaveCandidateMethod(method);
                }
            }

            private void AssertMethodCanBeVisited(Ast.Expression methodInvocationExpression, MethodDefinition method)
            {
                if (_methodDefinitionStack.Contains(method))
                    UnsupportedExpression(methodInvocationExpression);
            }

            private void EnterCandidateMethod(MethodDefinition method)
            {
                EnterMethodDefinition(method);
                ++_insideCandidate;
            }

            private void LeaveCandidateMethod(MethodDefinition method)
            {
                --_insideCandidate;
                LeaveMethodDefinition(method);
            }

            private static bool IsOperator(MethodReference method)
            {
                return !method.HasThis && method.Name.StartsWith("op_") && 2 == method.Parameters.Count;
            }

            public override void Visit(Ast.FieldReferenceExpression node)
            {
                PushFieldValueForTarget(node, node.Target);
            }

            private void PushFieldValueForTarget(Ast.FieldReferenceExpression node, Ast.Expression target)
            {
                switch (target.CodeElementType)
                {
                    case Ast.CodeElementType.ArgumentReferenceExpression:
                        PushFieldValue(CandidateFieldRoot.Instance, node.Field);
                        break;

                    case Ast.CodeElementType.ThisReferenceExpression:
                        if (InsideCandidate)
                        {
                            if (_current != null)
                            {
                                var current = PopFieldValue(node);
                                PushFieldValue(current, node.Field);
                            }
                            else
                            {
                                PushFieldValue(CandidateFieldRoot.Instance, node.Field);
                            }
                        }
                        else
                        {
                            PushFieldValue(PredicateFieldRoot.Instance, node.Field);
                        }
                        break;

                    case Ast.CodeElementType.MethodInvocationExpression:
                    case Ast.CodeElementType.FieldReferenceExpression:
                        var value = ToFieldValue(target);
                        PushFieldValue(value, node.Field);
                        break;

                    case Ast.CodeElementType.CastExpression:
                        PushFieldValueForTarget(node, ((Ast.CastExpression) node.Target).Target);
                        break;

                    default:
                        UnsupportedExpression(node);
                        break;
                }
            }

            private void PushFieldValue(IComparisonOperandAnchor value, FieldReference field)
            {
                Push(new FieldValue(value, _referenceProvider.ForCecilField(field)));
            }

            public override void Visit(Ast.LiteralExpression node)
            {
                Push(new ConstValue(node.Value));
            }

            private NQExpression Convert(Ast.Expression node)
            {
                return ReconstructNullComparisonIfNecessary(node);
            }

            private NQExpression ReconstructNullComparisonIfNecessary(Ast.Expression node)
            {
                Visit(node);

                var top = Pop();
                var fieldValue = top as FieldValue;
                if (fieldValue == null)
                {
                    AssertType(top, typeof (NQExpression), node);
                    return (NQExpression) top;
                }

                return
                    new NotExpression(
                        new ComparisonExpression(
                            fieldValue,
                            new ConstValue(null),
                            ComparisonOperator.ValueEquality));
            }

            private FieldValue ToFieldValue(Ast.Expression node)
            {
                Visit(node);
                return PopFieldValue(node);
            }

            private FieldValue PopFieldValue(Ast.Expression node)
            {
                return (FieldValue) Pop(node, typeof (FieldValue));
            }

            private void Push(object value)
            {
                Assert(_current == null, "expression stack must be empty before Push");
                _current = value;
            }

            private object Pop(Ast.Expression node, Type expectedType)
            {
                var value = Pop();
                AssertType(value, expectedType, node);
                return value;
            }

            private static void AssertType(object value, Type expectedType, Ast.Expression sourceExpression)
            {
                var actualType = value.GetType();
                if (!expectedType.IsAssignableFrom(actualType))
                {
                    UnsupportedPredicate(
                        string.Format("Unsupported expression: {0}. Unexpected type on stack. Expected: {1}, Got: {2}.",
                            Ast.ExpressionPrinter.ToString(sourceExpression), expectedType, actualType));
                }
            }

            private object Pop()
            {
                Assert(_current != null, "expression stack is empty");
                var value = _current;
                _current = null;
                return value;
            }

            private static void Assert(bool condition, string message)
            {
                Debug.Assert(condition, message);
            }
        }
    }

    internal class BoxedValueTypeProcessor : TraversingExpressionVisitor
    {
        public override void Visit(ComparisonExpression expression)
        {
            var fieldType = GetFieldType(expression.Left());
            if (!fieldType.IsValueType) return;

            var constValue = expression.Right() as ConstValue;
            if (constValue == null) return;

            AdjustConstValue(fieldType, constValue);
        }

        private static TypeReference GetFieldType(FieldValue field)
        {
            return ((CecilFieldRef) field.Field).FieldType;
        }

        private static void AdjustConstValue(TypeReference typeRef, ConstValue constValue)
        {
            var value = constValue.Value();
            if (!value.GetType().IsValueType) return;

            var type = ResolveTypeReference(typeRef);
            if (!type.IsEnum || value.GetType() == type) return;

            constValue.Value(Enum.ToObject(type, value));
        }

        private static Type ResolveTypeReference(TypeReference typeRef)
        {
            var assembly = LoadAssembly(typeRef.Scope);
            return assembly.GetType(typeRef.FullName.Replace('/', '+'), true);
        }

        private static Assembly LoadAssembly(IMetadataScope scope)
        {
            var nameRef = scope as AssemblyNameReference;
            if (null != nameRef) return Assembly.Load(nameRef.FullName);
            var moduleDef = scope as ModuleDefinition;
            return LoadAssembly(moduleDef.Assembly.Name);
        }
    }
}