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
using Db4oTool.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Db4oTool.Tests.Core
{
    /// <summary>
    ///     Prepends Console.WriteLine("TRACE: " + method) to every method
    ///     in the assembly.
    /// </summary>
    public class TraceInstrumentation : AbstractAssemblyInstrumentation
    {
        protected override void ProcessMethod(MethodDefinition method)
        {
            if (!method.HasBody) return;

            var body = method.Body;
            var firstInstruction = body.Instructions[0];
            var il = body.GetILProcessor();

            // ldstr "TRACE: " + method
            il.InsertBefore(firstInstruction,
                il.Create(OpCodes.Ldstr, "TRACE: " + method));

            // call Console.WriteLine(string)
            var Console_WriteLine = Import(typeof (Console).GetMethod("WriteLine", new[] {typeof (string)}));
            il.InsertBefore(firstInstruction,
                il.Create(OpCodes.Call, Console_WriteLine));
        }
    }

    internal class CustomInstrumentationTestCase : SingleResourceTestCase
    {
        protected override string ResourceName
        {
            get { return "CustomInstrumentationSubject"; }
        }

        protected override string CommandLine
        {
            get { return "-instrumentation:Db4oTool.Tests.Core.TraceInstrumentation,Db4oTool.Tests"; }
        }
    }
}