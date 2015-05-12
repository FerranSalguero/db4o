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
using System.Collections;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.IO;
using Db4oUnit;
using Sharpen.Lang;

namespace Db4objects.Db4o.Tests.Common.Assorted
{
    public class ConcurrentRenameTestCase : ITestLifeCycle
    {
        internal const int NumIterations = 500;
        private static readonly string DatabaseFileName = string.Empty;
        private readonly MemoryStorage _storage = new MemoryStorage();

        /// <exception cref="System.Exception"></exception>
        public virtual void SetUp()
        {
            var db = OpenDatabase();
            db.Store(new QueryItem());
            db.Store(new RenameItem());
            db.Close();
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TearDown()
        {
        }

        public static void Main(string[] args)
        {
            new ConsoleTestRunner(typeof (ConcurrentRenameTestCase)).Run();
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void Test()
        {
            var db = OpenDatabase();
            IList exceptions = new ArrayList();
            Thread[] threads =
            {
                new Thread(new QueryRunner
                    (db, exceptions), "ConcurrentRenameTestCase.test Thread[0]"),
                new Thread(new RenameRunner
                    (db, exceptions), "ConcurrentRenameTestCase.test Thread[1]")
            };
            for (var threadIndex = 0; threadIndex < threads.Length; ++threadIndex)
            {
                var thread = threads[threadIndex];
                thread.Start();
            }
            for (var threadIndex = 0; threadIndex < threads.Length; ++threadIndex)
            {
                var thread = threads[threadIndex];
                thread.Join();
            }
            db.Close();
            Assert.AreEqual(0, exceptions.Count);
        }

        private IEmbeddedObjectContainer OpenDatabase()
        {
            var config = Db4oEmbedded.NewConfiguration();
            config.File.Storage = _storage;
            return Db4oEmbedded.OpenFile(config, DatabaseFileName);
        }

        public class QueryItem
        {
        }

        public class RenameItem
        {
        }

        public abstract class RunnerBase : IRunnable
        {
            private readonly IObjectContainer _db;
            private readonly IList _exceptions;

            protected RunnerBase(IObjectContainer db, IList exceptions)
            {
                _db = db;
                _exceptions = exceptions;
            }

            public virtual void Run()
            {
                try
                {
                    for (var i = 0; i < NumIterations; i++)
                    {
                        Exercise(_db);
                        Runtime4.Sleep(1);
                    }
                }
                catch (Exception ex)
                {
                    lock (_exceptions)
                    {
                        _exceptions.Add(ex);
                    }
                }
            }

            protected abstract void Exercise(IObjectContainer db);
        }

        public class QueryRunner : RunnerBase
        {
            public QueryRunner(IObjectContainer db, IList exceptions) : base(db, exceptions)
            {
            }

            protected override void Exercise(IObjectContainer db)
            {
                Assert.AreEqual(1, db.Query(typeof (QueryItem)).Count);
                var newItem = new QueryItem
                    ();
                db.Store(newItem);
                db.Commit();
                db.Delete(newItem);
                db.Commit();
            }
        }

        public class RenameRunner : RunnerBase
        {
            private static readonly string OriginalName = ReflectPlatform.FullyQualifiedName(
                typeof (RenameItem));

            private static readonly string NewName = OriginalName + "X";

            public RenameRunner(IObjectContainer db, IList exceptions) : base(db, exceptions)
            {
            }

            protected override void Exercise(IObjectContainer db)
            {
                RenameClass(db, OriginalName, NewName);
                RenameClass(db, NewName, OriginalName);
            }

            private void RenameClass(IObjectContainer db, string originalName, string newName
                )
            {
                var storedClass = db.Ext().StoredClass(originalName);
                storedClass.Rename(newName);
            }
        }
    }
}