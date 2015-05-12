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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Tests.Common.Api;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Ext
{
    public class UnavailableClassesWithTranslatorTestCase : TestWithTempFile, IOptOutNetworkingCS
    {
        public static void Main(string[] args)
        {
            new ConsoleTestRunner(typeof (UnavailableClassesWithTranslatorTestCase)).Run();
        }

        public virtual void Test()
        {
            Store(TempFile(), new HolderForClassWithTranslator
                (new TranslatedType()));
            AssertStoredClasses(TempFile());
        }

        private void AssertStoredClasses(string databaseFileName)
        {
            IObjectContainer db = Db4oEmbedded.OpenFile(ConfigExcludingStack(), databaseFileName
                );
            try
            {
                Assert.IsGreater(2, db.Ext().StoredClasses().Length);
            }
            finally
            {
                db.Close();
            }
        }

        private void Store(string databaseFileName, object obj)
        {
            IObjectContainer db = Db4oEmbedded.OpenFile(NewConfiguration(), databaseFileName);
            try
            {
                db.Store(obj);
                db.Ext().Purge(obj);
                Assert.AreEqual(obj, db.Query(obj.GetType()).Next());
            }
            finally
            {
                db.Close();
            }
        }

        private IEmbeddedConfiguration NewConfiguration()
        {
            var config = Db4oEmbedded.NewConfiguration();
            config.Common.ObjectClass(typeof (TranslatedType
                ).FullName).Translate(new Translator());
            return config;
        }

        private IEmbeddedConfiguration ConfigExcludingStack()
        {
            var config = NewConfiguration();
            config.Common.ReflectWith(new ExcludingReflector(new[]
            {
                typeof (TranslatedType
                    )
            }));
            return config;
        }

        public class HolderForClassWithTranslator
        {
            public TranslatedType _fieldWithTranslator;

            public HolderForClassWithTranslator(TranslatedType
                value)
            {
                _fieldWithTranslator = value;
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                {
                    return false;
                }
                if (GetType() != obj.GetType())
                {
                    return false;
                }
                var other = (HolderForClassWithTranslator
                    ) obj;
                return Check.ObjectsAreEqual(_fieldWithTranslator, other._fieldWithTranslator);
            }
        }

        public class TranslatedType
        {
            public override bool Equals(object obj)
            {
                if (null == obj)
                {
                    return false;
                }
                return GetType() == obj.GetType();
            }
        }

        private sealed class Translator : IObjectTranslator
        {
            public void OnActivate(IObjectContainer container, object applicationObject, object
                storedObject)
            {
            }

            public object OnStore(IObjectContainer container, object applicationObject)
            {
                return 42;
            }

            public Type StoredClass()
            {
                return typeof (int);
            }
        }
    }
}