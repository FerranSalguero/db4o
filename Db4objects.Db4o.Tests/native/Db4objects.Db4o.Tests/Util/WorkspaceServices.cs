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
using Db4oUnit;
using Db4oUnit.Extensions.Util;
using Sharpen;

namespace Db4objects.Db4o.Tests.Util
{
    internal class WorkspaceServices
    {
        public static string WorkspaceRoot
        {
            get { return IOServices.FindParentDirectory("db4obuild"); }
        }

        public static string JavacPath()
        {
            return ReadMachinePathProperty("file.compiler.jdk1.3");
        }

        public static string JavaPath()
        {
            return ReadMachinePathProperty("file.jvm.jdk1.5");
        }

        public static string ReadMachineProperty(string property)
        {
            return ReadProperty(MachinePropertiesPath(), property);
        }

        public static string ReadMachinePathProperty(string property)
        {
            var path = ReadMachineProperty(property);
            Assert.IsTrue(File.Exists(path), string.Format("File '{0}' could not be found ({1}).", path, property));
            return path;
        }

        public static string ReadProperty(string fname, string property)
        {
            return ReadProperty(fname, property, false);
        }

        public static string ReadProperty(string fname, string property, bool lenient)
        {
            var value = FindProperty(fname, property);
            if (value != null) return value;
            if (lenient) return null;
            throw new ArgumentException("property '" + property + "' not found in '" + fname + "'");
        }

        private static string FindProperty(string fname, string property)
        {
            using (var reader = File.OpenText(fname))
            {
                string line = null;
                while (null != (line = reader.ReadLine()))
                {
                    if (line.StartsWith(property))
                    {
                        return line.Substring(property.Length + 1);
                    }
                }
            }
            return null;
        }

        public static string MachinePropertiesPath()
        {
            var fileName = Runtime.GetEnvironmentVariable("DB4O_MACHINE_PROPERTIES", "machine.properties");
            var path = WorkspacePath("db4obuild/" + fileName);
            Assert.IsTrue(File.Exists(path));
            return path;
        }

        public static string WorkspacePath(string fname)
        {
            var root = WorkspaceRoot;
            return null == root ? null : Path.Combine(root, fname);
        }

        public static string WorkspaceTestFilePath(string fname)
        {
            var testFolder = WorkspaceLocations.GetTestFolder();
            if (testFolder == null) return null;
            return Path.Combine(testFolder, fname);
        }
    }
}