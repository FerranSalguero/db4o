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

using Cecil.FlowAnalysis.Utilities;
using Db4oTool.Core;
using Db4oUnit;
using Mono.Cecil;

namespace Db4oTool.Tests.TA
{
    internal class TAInstrumentationAppliedMoreThanOnce : TATestCaseBase
    {
        public void Test()
        {
            var testAssembly = GenerateAssembly("TADoubleInstrumentationSubject");
            InstrumentAssembly(testAssembly);

            var instrumented = InstrumentedMethod(testAssembly);
            var before = FormatMethodBody(instrumented);

            InstrumentAssembly(testAssembly);

            var after = FormatMethodBody(instrumented);
            Assert.AreEqual(before, after);
        }

        private static MethodDefinition InstrumentedMethod(AssemblyDefinition testAssembly)
        {
            return CecilReflector.GetMethod(testAssembly.MainModule.GetType("InstrumentedType"), "InstrumentedMethod");
        }

        private static string FormatMethodBody(MethodDefinition instrumented)
        {
            return Formatter.FormatMethodBody(instrumented);
        }
    }
}