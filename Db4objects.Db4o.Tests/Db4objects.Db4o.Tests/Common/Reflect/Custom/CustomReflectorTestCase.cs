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
using System.IO;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Reflect.Custom
{
    /// <summary>
    ///     This test case serves two purposes:
    ///     1) testing the reflector API
    ///     2) documenting a common use case for the reflector API which is adapting an external
    ///     data model to db4o's internal OO based mechanism.
    /// </summary>
    /// <remarks>
    ///     This test case serves two purposes:
    ///     1) testing the reflector API
    ///     2) documenting a common use case for the reflector API which is adapting an external
    ///     data model to db4o's internal OO based mechanism.
    ///     See CustomReflector, CustomClassRepository, CustomClass, CustomField and CustomUidField
    ///     for details.
    /// </remarks>
    public class CustomReflectorTestCase : ITestCase, ITestLifeCycle
    {
        private static readonly string CatClass = "Cat";

        private static readonly string[] CatFieldNames =
        {
            "name", "troubleMakingScore"
        };

        private static readonly string[] CatFieldTypes = {"string", "int"};
        private static readonly string PersonClass = "Person";
        private static readonly string[] PersonFieldNames = {"name"};
        private static readonly string[] PersonFieldTypes = {"string"};

        private static readonly PersistentEntry[] CatEntries =
        {
            new
                PersistentEntry(CatClass, "0", new object[] {"Biro-Biro", 9}),
            new PersistentEntry
                (CatClass, "1", new object[] {"Samira", 4}),
            new PersistentEntry(CatClass, "2"
                , new object[] {"Ivo", 2})
        };

        private static readonly PersistentEntry[] PersonEntries =
        {
            new PersistentEntry(PersonClass, "10", new object[] {"Eric Idle"}), new PersistentEntry
                (PersonClass, "11", new object[] {"John Cleese"})
        };

        internal PersistenceContext _context;
        internal Db4oPersistenceProvider _provider;

        public virtual void SetUp()
        {
            Purge();
            InitializeContext();
            InitializeProvider();
            CreateEntryClass(CatClass, CatFieldNames, CatFieldTypes);
            CreateIndex(CatClass, CatFieldNames[0]);
            RestartProvider();
            CreateEntryClass(PersonClass, PersonFieldNames, PersonFieldTypes);
            RestartProvider();
            InsertEntries();
            RestartProvider();
        }

        public virtual void TearDown()
        {
            ShutdownProvider(true);
            _context = null;
        }

        public virtual void TestUpdate()
        {
            var entry = new PersistentEntry(CatClass, CatEntries[0].uid, new object
                [] {"Birinho", 10});
            Update(entry);
            RestartProvider();
            //exerciseSelectByField(entry, CAT_FIELD_NAMES);
            var expected = Copy(CatEntries);
            expected[0] = entry;
            AssertEntries(expected, SelectAll(CatClass));
        }

        public virtual void TestSelectAll()
        {
            AssertEntries(PersonEntries, SelectAll(PersonClass));
            AssertEntries(CatEntries, SelectAll(CatClass));
        }

        public virtual void TestSelectByField()
        {
            ExerciseSelectByField(CatEntries, CatFieldNames);
            ExerciseSelectByField(PersonEntries, PersonFieldNames);
        }

        public virtual void TestSelectByFields()
        {
            var existing = CatEntries[0];
            var newEntry = new PersistentEntry(CatClass, 3, new[]
            {
                existing
                    .fieldValues[0],
                10
            });
            Insert(newEntry);
            var found = SelectByField(existing.className, CatFieldNames[0], existing.
                fieldValues[0]);
            AssertEntries(new[] {existing, newEntry}, found);
            AssertSingleEntry(existing, Select(existing.className, CatFieldNames, existing.fieldValues
                ));
            AssertSingleEntry(newEntry, Select(newEntry.className, CatFieldNames, newEntry.fieldValues
                ));
        }

        public virtual void TestDropIndex()
        {
            DropIndex(CatClass, CatFieldNames[0]);
            var field = FieldMetadata(CatClass, CatFieldNames
                [0]);
            Assert.IsFalse(field.HasIndex());
        }

        public virtual void TestFieldIndex()
        {
            var field0 = FieldMetadata(CatClass, CatFieldNames
                [0]);
            Assert.IsTrue(field0.HasIndex());
            var field1 = FieldMetadata(CatClass, CatFieldNames
                [1]);
            Assert.IsFalse(field1.HasIndex());
        }

        private FieldMetadata FieldMetadata(string className, string
            fieldName)
        {
            var meta = ClassMetadataForName(className);
            var field0 = meta.FieldMetadataForName(fieldName
                );
            return field0;
        }

        private void Update(PersistentEntry entry)
        {
            _provider.Update(_context, entry);
        }

        private void AssertEntries(PersistentEntry[] expected, IEnumerator actual)
        {
            var checklist = new Collection4(actual);
            Assert.AreEqual(expected.Length, checklist.Size());
            for (var i = 0; i < expected.Length; ++i)
            {
                var e = expected[i];
                var a = EntryByUid(checklist.GetEnumerator(), e.uid);
                if (a != null)
                {
                    AssertEqualEntries(e, a);
                    checklist.Remove(a);
                }
            }
            Assert.IsTrue(checklist.IsEmpty(), checklist.ToString());
        }

        private PersistentEntry EntryByUid(IEnumerator iterator, object uid)
        {
            while (iterator.MoveNext())
            {
                var e = (PersistentEntry) iterator.Current;
                if (uid.Equals(e.uid))
                {
                    return e;
                }
            }
            return null;
        }

        private ClassMetadata ClassMetadataForName(string className)
        {
            var container = (IInternalObjectContainer) _provider.DataContainer
                (_context);
            return container.ClassMetadataForReflectClass(container.Reflector().ForName(className
                ));
        }

        private void ExerciseSelectByField(PersistentEntry[] entries, string[] fieldNames
            )
        {
            for (var i = 0; i < entries.Length; ++i)
            {
                ExerciseSelectByField(entries[i], fieldNames);
            }
        }

        private void ExerciseSelectByField(PersistentEntry expected, string[] fieldNames)
        {
            for (var i = 0; i < fieldNames.Length; ++i)
            {
                var found = SelectByField(expected.className, fieldNames[i], expected.fieldValues
                    [i]);
                AssertSingleEntry(expected, found);
            }
        }

        private void AssertSingleEntry(PersistentEntry expected, IEnumerator found)
        {
            Assert.IsTrue(found.MoveNext(), "Expecting entry '" + expected + "'");
            var actual = (PersistentEntry) found.Current;
            AssertEqualEntries(expected, actual);
            Assert.IsFalse(found.MoveNext(), "Expecting only '" + expected + "'");
        }

        private void InitializeContext()
        {
            _context = new PersistenceContext(DataFile());
        }

        private void InitializeProvider()
        {
            _provider = new Db4oPersistenceProvider();
            _provider.InitContext(_context);
        }

        private void InsertEntries()
        {
            InsertEntries(CatEntries);
            InsertEntries(PersonEntries);
        }

        private void InsertEntries(PersistentEntry[] entries)
        {
            var entry = new PersistentEntry(null, null, null);
            for (var i = 0; i < entries.Length; ++i)
            {
                entry.className = entries[i].className;
                entry.uid = entries[i].uid;
                entry.fieldValues = entries[i].fieldValues;
                // reuse entries so the provider can't assume
                // anything about identity
                Insert(entry);
            }
        }

        private void AssertEqualEntries(PersistentEntry expected, PersistentEntry actual)
        {
            Assert.AreEqual(expected.className, actual.className);
            Assert.AreEqual(expected.uid, actual.uid);
            ArrayAssert.AreEqual(expected.fieldValues, actual.fieldValues);
        }

        private IEnumerator SelectByField(string className, string fieldName, object fieldValue
            )
        {
            return Select(className, new[] {fieldName}, new[] {fieldValue});
        }

        private IEnumerator Select(string className, string[] fieldNames, object[] fieldValues
            )
        {
            return Select(new PersistentEntryTemplate(className, fieldNames, fieldValues));
        }

        private IEnumerator SelectAll(string className)
        {
            return Select(className, new string[0], new object[0]);
        }

        private IEnumerator Select(PersistentEntryTemplate template)
        {
            return _provider.Select(_context, template);
        }

        private void Insert(PersistentEntry entry)
        {
            _provider.Insert(_context, entry);
        }

        private void CreateIndex(string className, string fieldName)
        {
            _provider.CreateIndex(_context, className, fieldName);
        }

        private void DropIndex(string className, string fieldName)
        {
            _provider.DropIndex(_context, className, fieldName);
        }

        private void CreateEntryClass(string className, string[] fieldNames, string[] fieldTypes
            )
        {
            _provider.CreateEntryClass(_context, className, fieldNames, fieldTypes);
        }

        private void ShutdownProvider(bool purge)
        {
            if (_provider != null)
            {
                _provider.CloseContext(_context);
            }
            if (purge)
            {
                Purge();
            }
            _provider = null;
        }

        internal virtual void Purge()
        {
            new Db4oPersistenceProvider().Purge(DataFile());
        }

        internal virtual void RestartProvider()
        {
            ShutdownProvider(false);
            InitializeProvider();
        }

        private string DataFile()
        {
            return Path.Combine(Path.GetTempPath(), "CustomReflector.db4o");
        }

        private PersistentEntry[] Copy(PersistentEntry[] entries)
        {
            var clone = new PersistentEntry[entries.Length];
            Array.Copy(entries, 0, clone, 0, clone.Length);
            return clone;
        }
    }
}