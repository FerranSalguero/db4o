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
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Query;
using Db4objects.Db4o.Query;
using Db4objects.Db4o.Tests.Util;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.CLI1.NativeQueries
{
#if !CF
    public class Author
    {
        public Author(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; private set; }
        public string Name { get; private set; }
    }

    public class OuterAuthor
    {
        public class InnerAuthor : Author
        {
            public InnerAuthor(int id, string name)
                : base(id, name)
            {
            }
        }
    }

    public class MultipleAssemblySupportTestCase : AbstractDb4oTestCase, IOptOutMultiSession
    {
#if !SILVERLIGHT
        protected override void Store()
        {
            Store(new Author(1, "Kurt Vonnegut"));
            Store(new Author(2, "Kilgore Trout"));
            Store(new OuterAuthor.InnerAuthor(3, "Joao Saramago"));
            Store(new OuterAuthor.InnerAuthor(4, "Douglas Adams"));
        }

        public void TestPredicateAccessingTopLevelType()
        {
            var predicateCode = @"
using Db4objects.Db4o.Query;
using Db4objects.Db4o.Tests.CLI1.NativeQueries;

public class AuthorNamePredicate : Predicate
{
	public bool Match(Author candidate)
	{
		return candidate.Name == ""Kilgore Trout"";
	}
}
";
            AssertPredicate(2, predicateCode, "AuthorNamePredicate");
        }

        public void TestPredicateAccessingNestedType()
        {
            var predicateCode = @"
using Db4objects.Db4o.Query;
using Db4objects.Db4o.Tests.CLI1.NativeQueries;

public class InnerAuthorNamePredicate : Predicate
{
	public bool Match(OuterAuthor.InnerAuthor candidate)
	{
		return candidate.Name == ""Joao Saramago"" && candidate.Id > 1;
	}
}
";
            AssertPredicate(3, predicateCode, "InnerAuthorNamePredicate");
        }

        private void AssertPredicate(int expectedId, string predicateCode, string predicateTypeName)
        {
            var assembly = EmitAssemblyAndLoad(predicateTypeName + ".dll", predicateCode);

            var predicate = (Predicate) Activator.CreateInstance(assembly.GetType(predicateTypeName));

            Db().Configure().OptimizeNativeQueries(true);

            var handler = GetNativeQueryHandler(Db());
            handler.QueryExecution += OnQueryExecution;
            try
            {
                var os = Db().Query(predicate);
                Assert.AreEqual(1, os.Count);
                Assert.AreEqual(expectedId, ((Author) os.Next()).Id);
            }
            finally
            {
                handler.QueryExecution -= OnQueryExecution;
            }
        }

        private static NativeQueryHandler GetNativeQueryHandler(IObjectContainer container)
        {
            return ((ObjectContainerBase) container).GetNativeQueryHandler();
        }

        private static Assembly EmitAssemblyAndLoad(string assemblyName, string code)
        {
            var assemblyFile = Path.Combine(Path.GetTempPath(), assemblyName);
            CompilationServices.EmitAssembly(assemblyFile, code);
            return Assembly.LoadFrom(assemblyFile);
        }

        private void OnQueryExecution(object sender, QueryExecutionEventArgs args)
        {
            Assert.AreEqual(QueryExecutionKind.DynamicallyOptimized, args.ExecutionKind);
        }
#endif
    }
#endif
}