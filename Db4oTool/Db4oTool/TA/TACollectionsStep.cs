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
using Db4objects.Db4o.Collections;
using Db4oTool.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Db4oTool.TA
{
    internal class TACollectionsStep : TAInstrumentationStepBase
    {
        private static readonly IDictionary<string, Type> _collectionReplacements;

        static TACollectionsStep()
        {
            _collectionReplacements = new Dictionary<string, Type>();

            _collectionReplacements[typeof (List<>).FullName] = typeof (ActivatableList<>);
            _collectionReplacements[typeof (Dictionary<,>).FullName] = typeof (ActivatableDictionary<,>);
        }

        public override void Process(MethodDefinition method)
        {
            InstrumentCollectionInstantiation(method);
            InstrumentConcreteCollectionCasts(method);
        }

        private void InstrumentConcreteCollectionCasts(MethodDefinition methodDefinition)
        {
            foreach (var cast in CastsToSupportedCollections(methodDefinition.Body))
            {
                var result = StackAnalyzer.IsConsumedBy(MethodCallOnSupportedCollections, cast,
                    methodDefinition.DeclaringType.Module);
                if (!result.Match)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Error: [{0}] Invalid use of cast result: '{1}'.\r\nCasts to {2} are only allowed for property access/method calls.",
                            methodDefinition,
                            DebugInformation.InstructionInformationFor(result.Consumer,
                                methodDefinition.Body.Instructions), cast.Operand));
                }

                var castTarget = (TypeReference) cast.Operand;
                ReplaceCastAndCalleeDeclaringType(cast, result.Consumer,
                    _collectionReplacements[castTarget.Resolve().FullName]);
            }
        }

        private void ReplaceCastAndCalleeDeclaringType(Instruction cast, Instruction originalCall, Type replacementType)
        {
            var originalTypeReference = (GenericInstanceType) cast.Operand;

            var replacementReferenceType = NewGenericInstanceTypeWithArgumentsFrom(Context.Import(replacementType),
                originalTypeReference);
            cast.Operand = replacementReferenceType;
            originalCall.Operand = MethodReferenceFor((MethodReference) originalCall.Operand, replacementReferenceType);
        }

        private static MethodReference MethodReferenceFor(MethodReference source, TypeReference declaringType)
        {
            var newMethod = new MethodReference(source.Name, source.ReturnType);
            newMethod.DeclaringType = declaringType;
            newMethod.HasThis = true;

            foreach (var param in source.Parameters)
            {
                newMethod.Parameters.Add(param);
            }

            return newMethod;
        }

        private static GenericInstanceType NewGenericInstanceTypeWithArgumentsFrom(TypeReference referenceType,
            GenericInstanceType argumentSource)
        {
            var replacementTypeReference = new GenericInstanceType(referenceType);
            foreach (var argument in argumentSource.GenericArguments)
            {
                replacementTypeReference.GenericArguments.Add(argument);
            }
            return replacementTypeReference;
        }

        private static bool MethodCallOnSupportedCollections(Instruction candidate)
        {
            if (candidate.OpCode != OpCodes.Call && candidate.OpCode != OpCodes.Callvirt) return false;

            var callee = ((MethodReference) candidate.Operand).Resolve();
            return HasReplacement(callee.DeclaringType.Resolve().FullName);
        }

        private static bool HasReplacement(string collectionConcreteType)
        {
            return _collectionReplacements.ContainsKey(collectionConcreteType);
        }

        private IEnumerable<Instruction> CastsToSupportedCollections(MethodBody body)
        {
            return InstrumentationUtil.Where(body, delegate(Instruction candidate)
            {
                if (candidate.OpCode != OpCodes.Castclass) return false;
                var target = candidate.Operand as GenericInstanceType;

                return target != null && HasReplacement(target.Resolve().FullName);
            });
        }

        private void InstrumentCollectionInstantiation(MethodDefinition methodDefinition)
        {
            foreach (var newObj in TAEnabledCollectionInstantiations(methodDefinition.Body))
            {
                var stackAnalysis = StackAnalyzer.IsConsumedBy(delegate { return true; }, newObj,
                    methodDefinition.DeclaringType.Module);
                if (IsAssignmentToConcreteType(stackAnalysis))
                {
                    Context.TraceWarning("[{0}] Assignment to concrete collection {1} ignored (offset: 0x{2:X2}).",
                        methodDefinition, InstantiatedType(newObj), newObj.Next.Offset);
                    continue;
                }

                ReplaceContructorWithConstructorFrom(newObj);
            }
        }

        private static string InstantiatedType(Instruction newObj)
        {
            var originalCtor = (MethodReference) newObj.Operand;

            var originalType = (GenericInstanceType) originalCtor.DeclaringType;

            return originalType.FullName;
        }

        private void ReplaceContructorWithConstructorFrom(Instruction newObj)
        {
            var originalCtor = (MethodReference) newObj.Operand;

            var originalList = (GenericInstanceType) originalCtor.DeclaringType;
            var declaringType =
                new GenericInstanceType(Context.Import(_collectionReplacements[originalList.Resolve().FullName]));

            foreach (var argument in originalList.GenericArguments)
            {
                declaringType.GenericArguments.Add(argument);
            }

            var newCtor = new MethodReference(".ctor", Context.Import(typeof (void)));
            newCtor.DeclaringType = declaringType;
            newCtor.HasThis = true;

            foreach (var parameter in originalCtor.Parameters)
            {
                newCtor.Parameters.Add(parameter);
            }

            newObj.Operand = newCtor;
        }

        private bool IsAssignmentToConcreteType(StackAnalysisResult stackAnalysis)
        {
            TypeReference assignmentTargetType;
            if (InstrumentationUtil.IsCallInstruction(stackAnalysis.Consumer))
            {
                assignmentTargetType = stackAnalysis.AssignedParameter().ParameterType;
            }
            else
            {
                var assignmentTarget = stackAnalysis.Consumer.Operand as FieldReference;
                if (assignmentTarget != null)
                {
                    assignmentTargetType = assignmentTarget.FieldType;
                }
                else
                {
                    var variableDefinition = stackAnalysis.Consumer.Operand as VariableReference;
                    if (variableDefinition != null)
                    {
                        assignmentTargetType = variableDefinition.VariableType;
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }
            }
            return HasReplacement(assignmentTargetType.GetElementType().FullName);
        }

        private static IEnumerable<Instruction> TAEnabledCollectionInstantiations(MethodBody methodBody)
        {
            return InstrumentationUtil.Where(methodBody, delegate(Instruction candidate)
            {
                if (candidate.OpCode != OpCodes.Newobj) return false;
                var ctor = (MethodReference) candidate.Operand;
                var declaringType = ctor.DeclaringType.Resolve();

                return declaringType.HasGenericParameters && _collectionReplacements.ContainsKey(declaringType.FullName);
            });
        }
    }
}