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
using Db4objects.Db4o.Defragment;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation.IO;
using Db4objects.Db4o.Reflect;
using Db4oUnit;
using Db4oUnit.Extensions.Util;

namespace Db4objects.Db4o.Tests.Common.Defragment
{
    public class StoredClassFilterTestCase : DefragmentTestCaseBase
    {
        public static void Main(string[] args)
        {
            new ConsoleTestRunner(typeof (StoredClassFilterTestCase)).Run();
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void Test()
        {
            DeleteAllFiles();
            var fname = CreateDatabase();
            Defrag(fname);
            AssertStoredClasses(fname);
        }

        private void DeleteAllFiles()
        {
            File4.Delete(SourceFile());
            File4.Delete(BackupFile());
        }

        private void AssertStoredClasses(string fname)
        {
            IObjectContainer db = Db4oEmbedded.OpenFile(NewConfiguration(), fname);
            try
            {
                var knownClasses = db.Ext().KnownClasses();
                AssertKnownClasses(knownClasses);
            }
            finally
            {
                db.Close();
            }
        }

        private void AssertKnownClasses(IReflectClass[] knownClasses)
        {
            for (var i = 0; i < knownClasses.Length; i++)
            {
                Assert.AreNotEqual(FullyQualifiedName(typeof (SimpleClass
                    )), knownClasses[i].GetName());
            }
        }

        private string FullyQualifiedName(Type klass)
        {
            return CrossPlatformServices.FullyQualifiedName(klass);
        }

        /// <exception cref="System.IO.IOException"></exception>
        private void Defrag(string fname)
        {
            var config = new DefragmentConfig(fname);
            config.Db4oConfig(NewConfiguration());
            config.StoredClassFilter(IgnoreClassFilter(typeof (SimpleClass
                )));
            Db4o.Defragment.Defragment.Defrag(config);
        }

        private IStoredClassFilter IgnoreClassFilter(Type klass)
        {
            return new _IStoredClassFilter_67(this, klass);
        }

        private string CreateDatabase()
        {
            var fname = SourceFile();
            IObjectContainer db = Db4oEmbedded.OpenFile(NewConfiguration(), fname);
            try
            {
                db.Store(new SimpleClass("verySimple"));
                db.Commit();
            }
            finally
            {
                db.Close();
            }
            return fname;
        }

        public class SimpleClass
        {
            public string _simpleField;

            public SimpleClass(string simple)
            {
                _simpleField = simple;
            }
        }

        private sealed class _IStoredClassFilter_67 : IStoredClassFilter
        {
            private readonly StoredClassFilterTestCase _enclosing;
            private readonly Type klass;

            public _IStoredClassFilter_67(StoredClassFilterTestCase _enclosing, Type klass)
            {
                this._enclosing = _enclosing;
                this.klass = klass;
            }

            public bool Accept(IStoredClass storedClass)
            {
                return !storedClass.GetName().Equals(_enclosing.FullyQualifiedName(klass));
            }
        }
    }
}