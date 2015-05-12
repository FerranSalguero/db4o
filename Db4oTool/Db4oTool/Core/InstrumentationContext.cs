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
using System.Diagnostics;
using System.Reflection;
using Mono.Cecil;

namespace Db4oTool.Core
{
    public class InstrumentationContext
    {
        public InstrumentationContext(Configuration configuration, AssemblyDefinition assembly)
        {
            Init(configuration, assembly);
        }

        public InstrumentationContext(Configuration configuration)
        {
            Init(configuration, LoadAssembly(configuration));
        }

        public string AlternateAssemblyLocation { get; private set; }
        public Configuration Configuration { get; private set; }

        public TraceSwitch TraceSwitch
        {
            get { return Configuration.TraceSwitch; }
        }

        public AssemblyDefinition Assembly { get; private set; }

        public string AssemblyLocation
        {
            get { return Assembly.MainModule.FullyQualifiedName; }
        }

        public TypeReference Import(Type type)
        {
            return Assembly.MainModule.Import(type);
        }

        public MethodReference Import(MethodBase method)
        {
            return Assembly.MainModule.Import(method);
        }

        public void SaveAssembly()
        {
            var parameters = WriterParametersFor(PreserveDebugInfo());
            Assembly.Write(AssemblyLocation, parameters);
        }

        private WriterParameters WriterParametersFor(bool preserveDebugInfo)
        {
            var parameters = new WriterParameters();
            parameters.WriteSymbols = preserveDebugInfo;
            return parameters;
        }

        private bool PreserveDebugInfo()
        {
            return Configuration.PreserveDebugInfo;
        }

        public void TraceWarning(string message, params object[] args)
        {
            if (TraceSwitch.TraceWarning)
            {
                Trace.WriteLine(string.Format(message, args));
            }
        }

        public void TraceInfo(string message, params object[] args)
        {
            if (TraceSwitch.TraceInfo)
            {
                Trace.WriteLine(string.Format(message, args));
            }
        }

        public void TraceVerbose(string format, params object[] args)
        {
            if (TraceSwitch.TraceVerbose)
            {
                Trace.WriteLine(string.Format(format, args));
            }
        }

        public bool Accept(TypeDefinition typedef)
        {
            return Configuration.Accept(typedef);
        }

        public bool IsAssemblySigned()
        {
            return Assembly.Name.HasPublicKey;
        }

        private void Init(Configuration configuration, AssemblyDefinition assembly)
        {
            Configuration = configuration;

            ConfigureCompactFrameworkAssemblyPath(assembly);
            SetupAssembly(assembly);
        }

        private void ConfigureCompactFrameworkAssemblyPath(AssemblyDefinition assembly)
        {
            AlternateAssemblyLocation = CompactFrameworkServices.FolderFor(assembly);
        }

        private static AssemblyDefinition LoadAssembly(Configuration configuration)
        {
            return AssemblyDefinition.ReadAssembly(configuration.AssemblyLocation);
        }

        private void SetupAssembly(AssemblyDefinition assembly)
        {
            Assembly = assembly;

            if (PreserveDebugInfo())
            {
                Assembly.MainModule.ReadSymbols();
            }
        }
    }
}