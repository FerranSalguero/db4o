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
using System.IO;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Tests.Util;
using Db4oUnit.Extensions.Fixtures;
using Db4oUnit.Extensions.Util;

namespace Db4objects.Db4o.Tests.CLI1.Aliases
{
    /// <summary>
    /// </summary>
    public class JavaFromNetAliasesTestCase : BaseAliasesTestCase, IOptOutMultiSession, IOptOutSilverlight
    {
        private string GetTypeName(Type type)
        {
            return type.FullName + ", " + type.Assembly.GetName().Name;
        }

#if !CF
        public void TestAccessingJavaFromDotnet()
        {
            if (!JavaServices.CanRunJavaCompatibilityTests())
            {
                return;
            }

            GenerateJavaData();
            using (var container = OpenJavaDataFile())
            {
                AssertAliasedData(container);
            }
        }

        public void TestUpdatingAliasedDataSameSession()
        {
            if (!JavaServices.CanRunJavaCompatibilityTests())
            {
                return;
            }

            GenerateJavaData();
            using (var container = OpenJavaDataFile())
            {
                var newNames = UpdateAliasedData(container);
                AssertAliasedData(QueryAliasedData(container), newNames);
            }
        }

        public void TestUpdatingAliasedDataDifferentSession()
        {
            if (!JavaServices.CanRunJavaCompatibilityTests())
            {
                return;
            }

            GenerateJavaData();
            var newNames = UpdateAliasedData();
            using (var container = OpenJavaDataFile())
            {
                AssertAliasedData(QueryAliasedData(container), newNames);
            }
        }

        private string[] UpdateAliasedData()
        {
            using (var container = OpenJavaDataFile())
            {
                return UpdateAliasedData(container);
            }
        }

        private string[] UpdateAliasedData(IObjectContainer container)
        {
            var newNames = new List<string>();
            foreach (IPerson person in QueryAliasedData(container))
            {
                var newName = person.Name + "*";
                person.Name = newName;
                container.Store(person);
                newNames.Add(newName);
            }

            // new item
            var newItemName = "orestes";
            container.Store(CreateAliasedData(newItemName));
            newNames.Add(newItemName);

            container.Commit();
            return newNames.ToArray();
        }

        private void ConfigureAliases(IConfiguration configuration)
        {
            configuration.AddAlias(new TypeAlias("com.db4o.test.aliases.Person2", GetTypeName(GetAliasedDataType())));
            //	        configuration.AddAlias(
            //	            new WildcardAlias(
            //	                "com.db4o.test.aliases.*",
            //	                CurrentNamespace + ".*, " + CurrentAssemblyName));
            configuration.AddAlias(
                new TypeAlias("com.db4o.ext.Db4oDatabase", GetTypeName(typeof (Db4oDatabase))));
        }

        private IObjectContainer OpenJavaDataFile()
        {
            var configuration = Db4oFactory.NewConfiguration();
            ConfigureAliases(configuration);
            return Db4oFactory.OpenFile(configuration, GetJavaDataFile());
        }

        private static string GetJavaDataFile()
        {
            return IOServices.BuildTempPath("java.yap");
        }

        private void GenerateJavaData()
        {
            DeleteOldDataFile();
            CompileJavaProgram();
            RunJavaProgram();
        }

        private static void RunJavaProgram()
        {
            var stdout = JavaServices.java("com.db4o.test.aliases.Program", GetJavaDataFile());
            Console.WriteLine(stdout);
        }

        private static void DeleteOldDataFile()
        {
            File.Delete(GetJavaDataFile());
        }

        private void CompileJavaProgram()
        {
            var code = @"
package com.db4o.test.aliases;

import com.db4o.*;

class Person2 {
	String _name;
	public Person2(String name) {
		_name = name;
	}
}

public class Program {
	public static void main(String[] args) {
		String fname = args[0];
		ObjectContainer container = Db4o.openFile(fname);
		container.store(new Person2(""Homer Simpson""));
		container.store(new Person2(""John Cleese""));
		container.close();
		System.out.println(""success"");
	}
}";

            JavaServices.ResetJavaTempPath();
            var stdout = JavaServices.CompileJavaCode("com/db4o/test/aliases/Program.java", code);
            Console.WriteLine(stdout);
        }

#endif
    }
}