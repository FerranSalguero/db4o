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
using Db4objects.Db4o;
using Db4objects.Db4o.Instrumentation.Api;
using Db4objects.Db4o.Instrumentation.Cecil;
using Db4objects.Db4o.Internal.Query;
using Db4objects.Db4o.NativeQueries.Expr;
using Db4objects.Db4o.NativeQueries.Expr.Cmp.Operand;
using Db4objects.Db4o.Query;
using Db4oTool.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Db4oTool.NQ
{
    internal class DelegateQueryProcessor
    {
        private readonly InstrumentationContext _context;
        private readonly DelegateOptimizer _optimizer;
        private readonly ILPattern _predicateCreationPattern = ILPattern.Sequence(OpCodes.Newobj, OpCodes.Ldftn);
        private readonly CecilReflector _reflector;
        private readonly ILPattern _staticFieldPattern = CreateStaticFieldPattern();

        public DelegateQueryProcessor(InstrumentationContext context, DelegateOptimizer optimizer)
        {
            _context = context;
            _optimizer = optimizer;
            _reflector = new CecilReflector(_context);
        }

        public void Process(MethodDefinition parent, Instruction queryInvocation)
        {
            var il = parent.Body.GetILProcessor();
            if (IsCachedStaticFieldPattern(queryInvocation))
            {
                _context.TraceVerbose("static delegate field pattern found in {0}", parent.Name);
                ProcessCachedStaticFieldPattern(il, queryInvocation);
            }
            else if (IsPredicateCreationPattern(queryInvocation))
            {
                _context.TraceVerbose("simple delegate pattern found in {0}", parent.Name);
                ProcessPredicateCreationPattern(il, queryInvocation);
            }
            else
            {
                _context.TraceWarning("Unknown query invocation pattern on method: {0}!", parent);
            }
        }

        private void ProcessPredicateCreationPattern(ILProcessor il, Instruction queryInvocation)
        {
            var predicateReference = GetMethodReferenceFromInlinePredicatePattern(queryInvocation);
            var predicateMethod = Resolve(predicateReference);

            var expression = _optimizer.GetExpression(predicateMethod);
            if (expression == null)
            {
                return;
            }

            IDictionary<FieldReference, FieldDefinition> fields;
            var syntheticPredicate = NewSyntheticPredicateFor(expression, predicateMethod, out fields);

            expression.Accept(new UpdateFieldReferences(fields));

            _optimizer.OptimizePredicate(syntheticPredicate, predicateMethod, expression);

            RemovePreviousInstrunctions(il, queryInvocation, 2);

            InjectSyntheticPredicateInstantiation(
                queryInvocation,
                il,
                syntheticPredicate,
                fields.Keys,
                predicateReference.DeclaringType);

            ReplaceByExecuteEnhancedFilter(queryInvocation);
        }

        private TypeDefinition NewSyntheticPredicateFor(IExpression expression, MemberReference predicateMethod,
            out IDictionary<FieldReference, FieldDefinition> fields)
        {
            var syntheticPredicate = NewSyntheticPredicateFor(predicateMethod);

            var queryFields = CollectAccessedFields(expression);
            fields = AddFields(syntheticPredicate, queryFields);
            if (RequiresSyntheticPredicateInitialization(fields))
            {
                AddConstructor(syntheticPredicate, fields);
            }

            return syntheticPredicate;
        }

        private static bool RequiresSyntheticPredicateInitialization<T>(ICollection<T> fields)
        {
            return fields.Count > 0;
        }

        private void InjectSyntheticPredicateInstantiation(Instruction queryInvocation, ILProcessor cil,
            TypeDefinition syntheticPredicate, ICollection<FieldReference> fieldValuesReferences,
            TypeReference closureType)
        {
            var closureObjVar = new VariableDefinition(closureType);
            cil.Body.Variables.Add(closureObjVar);

            var ip = cil.Create(OpCodes.Stloc, closureObjVar);
            cil.InsertBefore(queryInvocation, ip);

            var ctorIndex = 0;
            if (RequiresSyntheticPredicateInitialization(fieldValuesReferences))
            {
                PushParameters(cil, closureObjVar, ip, fieldValuesReferences);
                ctorIndex = 1;
            }

            var newObj = cil.Create(OpCodes.Newobj, FindConstructor(syntheticPredicate, ctorIndex));
            cil.InsertBefore(queryInvocation, newObj);
        }

        private void PushParameters(ILProcessor il, VariableDefinition closureObj, Instruction ip,
            IEnumerable<FieldReference> fieldValuesReferences)
        {
            foreach (var fieldReference in fieldValuesReferences)
            {
                var instruction = il.Create(OpCodes.Ldloc, closureObj);
                il.InsertAfter(ip, instruction);

                if (IsPublicField(fieldReference))
                {
                    il.InsertAfter(instruction, il.Create(OpCodes.Ldfld, fieldReference));
                }
                else
                {
                    ip = PushFieldContentsUsingReflection(fieldReference, il, instruction);
                }
                ip = ip.Next.Next;
            }
        }

        private bool IsPublicField(MemberReference reference)
        {
            var parentType = _reflector.ResolveTypeReference(reference.DeclaringType);
            return (CecilReflector.GetField(parentType, reference.Name).Attributes & FieldAttributes.Public) ==
                   FieldAttributes.Public;
        }

        /**
         * Expects that the object reference is already in the stack
         */

        private Instruction PushFieldContentsUsingReflection(FieldReference fieldReference, ILProcessor cil,
            Instruction ip)
        {
            var ldstr = cil.Create(OpCodes.Ldstr, fieldReference.Name);
            cil.InsertAfter(ip, ldstr);
            cil.InsertAfter(
                ldstr,
                cil.Create(OpCodes.Call, ImportReflectionGetter(fieldReference.FieldType)));

            return ip;
        }

        private MethodReference ImportReflectionGetter(TypeReference extent)
        {
            var queryPlatformType = typeof (PredicatePlatform);
            var getFieldMethod =
                _context.Import(queryPlatformType.GetMethod("GetField", new[] {typeof (object), typeof (string)}));
            return InstantiateGenericMethod(getFieldMethod, extent);
        }

        private static void RemovePreviousInstrunctions(ILProcessor il, Instruction instruction, int n)
        {
            while (n-- > 0)
            {
                il.Remove(instruction.Previous);
            }
        }

        private void AddConstructor(TypeDefinition type, IDictionary<FieldReference, FieldDefinition> fields)
        {
            const MethodAttributes methodAttributes =
                MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.Public;
            var ctor = new MethodDefinition(".ctor", methodAttributes, Import(typeof (void)));

            AddMethodParameters(ctor, fields.Values);

            var cil = ctor.Body.GetILProcessor();
            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Call, DefaultObjectConstructor());

            EmitFieldInitialization(ctor, fields.Values, ctor.Parameters);

            cil.Emit(OpCodes.Ret);

            type.Methods.Add(ctor);
        }

        private static void EmitFieldInitialization(MethodDefinition ctor, IEnumerable<FieldDefinition> fields,
            IList<ParameterDefinition> parameters)
        {
            var cil = ctor.Body.GetILProcessor();

            var i = 0;
            foreach (var fieldReference in fields)
            {
                cil.Emit(OpCodes.Ldarg_0);
                cil.Emit(OpCodes.Ldarg, parameters[i++]);
                cil.Emit(OpCodes.Stfld, fieldReference);
            }
        }

        private static void AddMethodParameters(IMethodSignature method, IEnumerable<FieldDefinition> fields)
        {
            foreach (var parameter in fields)
            {
                method.Parameters.Add(
                    new ParameterDefinition(parameter.Name, ParameterAttributes.None, parameter.FieldType));
            }
        }

        private static IDictionary<FieldReference, FieldDefinition> AddFields(TypeDefinition type,
            IEnumerable<IFieldRef> fields)
        {
            var fieldMap = new Dictionary<FieldReference, FieldDefinition>();
            foreach (var field in fields)
            {
                var cecilFieldRef = (CecilFieldRef) field;
                var fieldDefinition = CloneField((FieldDefinition) cecilFieldRef.Reference);
                fieldMap.Add(cecilFieldRef.Reference, fieldDefinition);
                type.Fields.Add(fieldDefinition);
            }

            return fieldMap;
        }

        private static FieldDefinition CloneField(FieldDefinition subject)
        {
            var clone = new FieldDefinition(subject.Name, subject.Attributes, subject.FieldType);

            if (subject.HasConstant)
                clone.Constant = subject.Constant;

            if (subject.HasLayoutInfo)
                clone.Offset = subject.Offset;

            return clone;
        }

        private static IList<IFieldRef> CollectAccessedFields(IExpression expression)
        {
            var fieldCollector = new FieldCollectorVisitor();
            expression.Accept(fieldCollector);

            return fieldCollector.Fields;
        }

        private void ProcessCachedStaticFieldPattern(ILProcessor il, Instruction queryInvocation)
        {
            var predicateReference = GetMethodReferenceFromStaticFieldPattern(queryInvocation);
            var predicateMethod = Resolve(predicateReference);

            var expression = _optimizer.GetExpression(predicateMethod);
            if (null == expression) return;

            var syntheticPredicate = NewSyntheticPredicateFor(predicateMethod);
            _optimizer.OptimizePredicate(syntheticPredicate, predicateMethod, expression);

            var newObj = il.Create(OpCodes.Newobj, FindConstructor(syntheticPredicate, 0));
            il.Replace(queryInvocation.Previous, newObj);

            ReplaceByExecuteEnhancedFilter(queryInvocation);
        }

        private TypeDefinition NewSyntheticPredicateFor(MemberReference predicate)
        {
            var module = MainModule();
            var type = new TypeDefinition(predicate.DeclaringType.Namespace, "Db4o$Predicate$" + module.Types.Count,
                TypeAttributes.Sealed | TypeAttributes.NotPublic, Import(typeof (object)));

            type.Methods.Add(CreateDefaultConstructor());

            module.Types.Add(type);

            return type;
        }

        private MethodDefinition CreateDefaultConstructor()
        {
            var ctor = new MethodDefinition(
                ".ctor",
                MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.Public,
                Import(typeof (void)));

            var worker = ctor.Body.GetILProcessor();
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Call, DefaultObjectConstructor());
            worker.Emit(OpCodes.Ret);
            return ctor;
        }

        private MethodReference DefaultObjectConstructor()
        {
            return _context.Import(typeof (object).GetConstructors()[0]);
        }

        private TypeReference Import(Type type)
        {
            return _context.Import(type);
        }

        private ModuleDefinition MainModule()
        {
            return _context.Assembly.MainModule;
        }

        private void ReplaceByExecuteEnhancedFilter(Instruction queryInvocation)
        {
            queryInvocation.OpCode = OpCodes.Call;
            queryInvocation.Operand = InstantiateGenericMethod(ExecuteEnhancedFilterMethod(),
                GetQueryCallExtent(queryInvocation));
        }

        private static MethodReference FindConstructor(TypeDefinition type, int index)
        {
            return GetConstructors(type)[index];
        }

        private static List<MethodDefinition> GetConstructors(TypeDefinition type)
        {
            var constructors = new List<MethodDefinition>();
            foreach (var method in type.Methods)
            {
                if (method.IsConstructor) constructors.Add(method);
            }
            return constructors;
        }

        private MethodReference ExecuteEnhancedFilterMethod()
        {
            return
                _context.Import(typeof (NativeQueryHandler).GetMethod("ExecuteEnhancedFilter",
                    new[] {typeof (IObjectContainer), typeof (IDb4oEnhancedFilter)}));
        }

        private static MethodDefinition Resolve(MethodReference methodRef)
        {
            var methodDefinition = methodRef as MethodDefinition;
            if (methodDefinition != null)
                return methodDefinition;

            var assemblyDefinition = AssemblyDefinition.ReadAssembly(methodRef.DeclaringType.Module.FullyQualifiedName);
            var type = assemblyDefinition.MainModule.GetType(methodRef.DeclaringType.Name);
            return CecilReflector.GetMethod(type, methodRef);
        }

        private static MethodReference GetMethodReferenceFromInlinePredicatePattern(Instruction queryInvocation)
        {
            return (MethodReference) GetNthPrevious(queryInvocation, 2).Operand;
        }

        private bool IsPredicateCreationPattern(Instruction queryInvocation)
        {
            return _predicateCreationPattern.IsBackwardsMatch(queryInvocation);
        }

        private static MethodReference InstantiateGenericMethod(MethodReference methodReference, TypeReference extent)
        {
            var instance = new GenericInstanceMethod(methodReference);
            instance.GenericArguments.Add(extent);
            return instance;
        }

        private static TypeReference GetQueryCallExtent(Instruction queryInvocation)
        {
            var method = (GenericInstanceMethod) queryInvocation.Operand;
            return method.GenericArguments[0];
        }

        private static MethodReference GetMethodReferenceFromStaticFieldPattern(Instruction instr)
        {
            return (MethodReference) GetFirstPrevious(instr, OpCodes.Ldftn).Operand;
        }

        private static Instruction GetFirstPrevious(Instruction instr, OpCode opcode)
        {
            var previous = instr;
            while (previous != null)
            {
                if (previous.OpCode == opcode) return previous;
                previous = previous.Previous;
            }
            throw new ArgumentException("No previous " + opcode + " instruction found");
        }

        private static Instruction GetNthPrevious(Instruction instr, int n)
        {
            var previous = instr;
            for (var i = 0; i < n; ++i)
            {
                previous = previous.Previous;
            }
            return previous;
        }

        private static ILPattern CreateStaticFieldPattern()
        {
            // ldsfld (br_s)? stsfld newobj ldftn ldnull (brtrue_s | brtrue) ldsfld
            return ILPattern.Sequence(
                ILPattern.Instruction(OpCodes.Ldsfld),
                ILPattern.Optional(OpCodes.Br_S),
                ILPattern.Instruction(OpCodes.Stsfld),
                ILPattern.Instruction(OpCodes.Newobj),
                ILPattern.Instruction(OpCodes.Ldftn),
                ILPattern.Instruction(OpCodes.Ldnull),
                ILPattern.Alternation(OpCodes.Brtrue, OpCodes.Brtrue_S),
                ILPattern.Instruction(OpCodes.Ldsfld));
        }

        private bool IsCachedStaticFieldPattern(Instruction instr)
        {
            return _staticFieldPattern.IsBackwardsMatch(instr);
        }
    }

    public class AbstractExpressionVisitor : IExpressionVisitor, IComparisonOperandVisitor
    {
        private void VisitBinaryExpression(BinaryExpression expression)
        {
            expression.Right().Accept(this);
            expression.Left().Accept(this);
        }

        #region IExpressionVisitor

        public virtual void Visit(AndExpression expression)
        {
            VisitBinaryExpression(expression);
        }

        public virtual void Visit(OrExpression expression)
        {
            VisitBinaryExpression(expression);
        }

        public virtual void Visit(NotExpression expression)
        {
            expression.Expr().Accept(this);
        }

        public virtual void Visit(ComparisonExpression expression)
        {
            expression.Right().Accept(this);
        }

        public virtual void Visit(BoolConstExpression expression)
        {
        }

        #endregion

        #region IComparisonOperandVisitor

        public virtual void Visit(ArithmeticExpression operand)
        {
            operand.Left().Accept(this);
            operand.Right().Accept(this);
        }

        public virtual void Visit(ConstValue operand)
        {
        }

        public virtual void Visit(FieldValue operand)
        {
        }

        public virtual void Visit(CandidateFieldRoot root)
        {
        }

        public virtual void Visit(PredicateFieldRoot root)
        {
        }

        public virtual void Visit(StaticFieldRoot root)
        {
        }

        public virtual void Visit(ArrayAccessValue operand)
        {
        }

        public virtual void Visit(MethodCallValue value)
        {
        }

        #endregion
    }

    internal class UpdateFieldReferences : AbstractExpressionVisitor
    {
        private readonly IDictionary<FieldReference, FieldDefinition> _fields;

        public UpdateFieldReferences(IDictionary<FieldReference, FieldDefinition> fields)
        {
            _fields = fields;
        }

        public override void Visit(FieldValue operand)
        {
            var cecilFieldRef = (CecilFieldRef) operand.Field;
            cecilFieldRef.Reference = _fields[Resolve(operand.Field)];
        }

        private static FieldReference Resolve(IFieldRef fieldRef)
        {
            return ((CecilFieldRef) fieldRef).Field;
        }
    }

    internal class FieldCollectorVisitor : AbstractExpressionVisitor
    {
        private readonly IList<IFieldRef> _fields = new List<IFieldRef>();

        public IList<IFieldRef> Fields
        {
            get { return _fields; }
        }

        #region IExpressionVisitor

        public override void Visit(BoolConstExpression expression)
        {
            //TODO: ???
        }

        #endregion

        #region IComparisonOperandVisitor

        public override void Visit(FieldValue operand)
        {
            _fields.Add(operand.Field);
        }

        #endregion
    }
}