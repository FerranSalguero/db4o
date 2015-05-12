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

using Db4objects.Db4o.Activation;
using Db4objects.Db4o.TA;
using Db4oTool.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Db4oTool.TA
{
    internal class ActivateMethodEmitter : MethodEmitter
    {
        public ActivateMethodEmitter(InstrumentationContext context, FieldDefinition field) : base(context, field)
        {
        }

        public MethodDefinition Emit()
        {
            var activate =
                NewExplicitMethod(typeof (IActivatable).GetMethod("Activate", new[] {typeof (ActivationPurpose)}));

            var cil = activate.Body.GetILProcessor();
            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldfld, _activatorField);

            var ret = cil.Create(OpCodes.Ret);

            cil.Emit(OpCodes.Brfalse, ret);

            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldfld, _activatorField);
            cil.Emit(OpCodes.Ldarg_1);
            cil.Emit(OpCodes.Callvirt,
                _context.Import(typeof (IActivator).GetMethod("Activate", new[] {typeof (ActivationPurpose)})));

            cil.Append(ret);

            return activate;
        }
    }
}