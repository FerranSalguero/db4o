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
using System.IO;
using System.Reflection;

namespace Db4oTool.Core
{
    public class DirectoryAssemblyResolver : IDisposable
    {
        private readonly string _directory;

        public DirectoryAssemblyResolver(string directory)
        {
            _directory = directory;
            CurrentDomain().AssemblyResolve += AppDomain_AssemblyResolve;
        }

        public void Dispose()
        {
            CurrentDomain().AssemblyResolve -= AppDomain_AssemblyResolve;
        }

        private Assembly AppDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var baseName = Path.Combine(_directory, SimpleName(args.Name));
            var found = ProbeFile(baseName + ".dll");
            if (found != null) return found;
            return ProbeFile(baseName + ".exe");
        }

        private string SimpleName(string assemblyName)
        {
            return assemblyName.Split(',')[0];
        }

        private Assembly ProbeFile(string fname)
        {
            if (!File.Exists(fname)) return null;
            return Assembly.LoadFile(fname);
        }

        private static AppDomain CurrentDomain()
        {
            return AppDomain.CurrentDomain;
        }
    }
}