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
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Text;

namespace Db4oTool.Tests.Core
{
    /// <summary>
    ///     Compilation helper.
    /// </summary>
    public class CompilationServices
    {
        public static readonly ContextVariable<bool> Debug = new ContextVariable<bool>(true);
        public static readonly ContextVariable<bool> Unsafe = new ContextVariable<bool>(false);
        public static readonly ContextVariable<SignConfiguration> KeyFile = new ContextVariable<SignConfiguration>(null);
        public static readonly ContextVariable<string> ExtraParameters = new ContextVariable<string>("");

        private CompilationServices()
        {
        }

        public static void EmitAssembly(string assemblyFileName, Assembly[] references, params string[] sourceFiles)
        {
            var basePath = Path.GetDirectoryName(assemblyFileName);
            CreateDirectoryIfNeeded(basePath);
            CompileFromFile(assemblyFileName, references, sourceFiles);
        }

        public static void CreateDirectoryIfNeeded(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static CompilerInfo GetCSharpCompilerInfo()
        {
            return CodeDomProvider.GetCompilerInfo(CodeDomProvider.GetLanguageFromExtension(".cs"));
        }

        private static CodeDomProvider GetCSharpCodeDomProvider()
        {
            return GetCSharpCompilerInfo().CreateProvider();
        }

        private static CompilerParameters CreateDefaultCompilerParameters()
        {
            return GetCSharpCompilerInfo().CreateDefaultCompilerParameters();
        }

        public static void CompileFromFile(string assemblyFName, Assembly[] references, params string[] sourceFiles)
        {
            using (var provider = GetCSharpCodeDomProvider())
            {
                var parameters = CreateDefaultCompilerParameters();
                // TODO: run test cases in both modes (optimized and debug)
                parameters.IncludeDebugInformation = Debug.Value;
                parameters.OutputAssembly = assemblyFName;

                if (Unsafe.Value) parameters.CompilerOptions = "/unsafe";
                if (KeyFile.Value != null)
                {
                    parameters.CompilerOptions += " /keyfile:" + KeyFile.Value.KeyFile;
                    parameters.CompilerOptions += " /delaysign" + (KeyFile.Value.DelaySign ? '+' : '-');
                }

                parameters.CompilerOptions += " " + ExtraParameters.Value;

                foreach (var reference in references)
                {
                    parameters.ReferencedAssemblies.Add(reference.ManifestModule.FullyQualifiedName);
                }
                var results = provider.CompileAssemblyFromFile(parameters, sourceFiles);
                if (ContainsErrors(results.Errors))
                {
                    throw new ApplicationException(GetErrorString(results.Errors));
                }
            }
        }

        private static bool ContainsErrors(CompilerErrorCollection errors)
        {
            foreach (CompilerError error in errors)
            {
                if (!error.IsWarning)
                {
                    return true;
                }
            }
            return false;
        }

        public static string EmitAssemblyFromResource(string resourceName, params Assembly[] references)
        {
            Action<string> noOpSourceHandler = delegate { };
            return EmitAssemblyFromResource(resourceName, noOpSourceHandler, references);
        }

        public static string EmitAssemblyFromResource(string resourceName, Action<string> sourceHandler,
            Assembly[] references)
        {
            var assemblyFileName = Path.Combine(ShellUtilities.GetTempPath(),
                resourceName + (Debug.Value ? ".Debug.dll" : ".dll"));
            var sourceFileName = Path.Combine(ShellUtilities.GetTempPath(), resourceName);
            File.WriteAllText(sourceFileName, ResourceServices.GetResourceAsString(resourceName));
            DeleteAssemblyAndPdb(assemblyFileName);
            EmitAssembly(assemblyFileName, references, sourceFileName);

            sourceHandler(sourceFileName);

            return assemblyFileName;
        }

        private static void DeleteAssemblyAndPdb(string path)
        {
            ShellUtilities.DeleteFile(Path.ChangeExtension(path, ".pdb"));
            ShellUtilities.DeleteFile(path);
        }

        private static string GetErrorString(CompilerErrorCollection errors)
        {
            var builder = new StringBuilder();
            foreach (CompilerError error in errors)
            {
                builder.Append(error);
                builder.Append(Environment.NewLine);
            }
            return builder.ToString();
        }
    }
}