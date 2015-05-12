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

#if !CF && !SILVERLIGHT

using System;
using System.Reflection;
using System.Reflection.Emit;
using Db4objects.Db4o.Activation;
using Db4objects.Db4o.Internal.Caching;
using Db4objects.Db4o.Linq.Caching;
using Db4objects.Db4o.Linq.Internals;
using Mono.Reflection;

namespace Db4objects.Db4o.Linq.CodeAnalysis
{
    internal class ReflectionMethodAnalyser : IMethodAnalyser
    {
        private static readonly ICache4<MethodInfo, FieldInfo> _fieldCache =
            CacheFactory<MethodInfo, FieldInfo>.For(CacheFactory.New2QXCache(5));

        private static readonly ILPattern _getterPattern =
            ILPattern.Sequence(
                ILPattern.Optional(ActivateCall()),
                BackingField(),
                ILPattern.Optional(
                    OpCodes.Stloc_0,
                    OpCodes.Br_S,
                    OpCodes.Ldloc_0),
                ILPattern.OpCode(OpCodes.Ret));

        private readonly MethodInfo _method;

        private ReflectionMethodAnalyser(MethodInfo method)
        {
            _method = method;
        }

        public void Run(QueryBuilderRecorder recorder)
        {
            RecordField(recorder, GetBackingField(_method));
        }

        private static ILPattern ActivateCall()
        {
            return new ActivateCallPattern();
        }

        private static ILPattern BackingField()
        {
            return new BackingFieldPattern();
        }

        private static MatchContext MatchGetter(MethodInfo method)
        {
            return ILPattern.Match(method, _getterPattern);
        }

        private static void RecordField(QueryBuilderRecorder recorder, FieldInfo field)
        {
            recorder.Add(ctx =>
            {
                ctx.Descend(field.Name);
                ctx.PushDescendigFieldEnumType(field.FieldType.IsEnum ? field.FieldType : null);
            });
        }

        private static FieldInfo GetBackingField(MethodInfo method)
        {
            return _fieldCache.Produce(method, ResolveBackingField);
        }

        private static FieldInfo ResolveBackingField(MethodInfo method)
        {
            var context = MatchGetter(method);
            if (!context.IsMatch) throw new QueryOptimizationException("Analysed method is not a simple getter");

            return GetFieldFromContext(context);
        }

        private static FieldInfo GetFieldFromContext(MatchContext context)
        {
            object data;
            if (!context.TryGetData(BackingFieldPattern.BackingFieldKey, out data)) throw new NotSupportedException();

            return (FieldInfo) data;
        }

        public static IMethodAnalyser FromMethod(MethodInfo method)
        {
            return new ReflectionMethodAnalyser(method);
        }

        private class ActivateCallPattern : ILPattern
        {
            private static readonly ILPattern pattern = Sequence(
                Optional(OpCodes.Nop),
                OpCode(OpCodes.Ldarg_0),
                OpCode(OpCodes.Ldc_I4_0),
                Either(
                    OpCode(OpCodes.Call),
                    OpCode(OpCodes.Callvirt)));

            public override void Match(MatchContext context)
            {
                pattern.Match(context);
                if (!context.IsMatch) return;

                var match = GetLastMatchingInstruction(context);
                var method = (MethodInfo) match.Operand;
                if (!IsActivateCall(method)) context.IsMatch = false;
            }

            private static bool IsActivateCall(MethodInfo method)
            {
                if (method == null) return false;
                if (method.IsStatic) return false;
                if (method.Name != "Activate") return false;

                var parameters = method.GetParameters();
                if (parameters.Length != 1) return false;
                if (parameters[0].ParameterType != typeof (ActivationPurpose)) return false;

                return true;
            }
        }

        private class BackingFieldPattern : ILPattern
        {
            public static readonly object BackingFieldKey = new object();

            private static readonly ILPattern pattern = Sequence(
                Optional(OpCodes.Nop),
                OpCode(OpCodes.Ldarg_0),
                OpCode(OpCodes.Ldfld));

            public override void Match(MatchContext context)
            {
                pattern.Match(context);
                if (!context.IsMatch) return;

                var match = GetLastMatchingInstruction(context);
                var field = (FieldInfo) match.Operand;
                context.AddData(BackingFieldKey, field);
            }
        }
    }
}

#endif