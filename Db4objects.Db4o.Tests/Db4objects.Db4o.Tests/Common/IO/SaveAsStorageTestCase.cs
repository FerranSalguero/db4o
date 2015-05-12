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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Foundation.IO;
using Db4objects.Db4o.IO;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;
using Sharpen;

namespace Db4objects.Db4o.Tests.Common.IO
{
    public class SaveAsStorageTestCase : AbstractDb4oTestCase, IOptOutMultiSession, IOptOutInMemory
        , IOptOutNoFileSystemData, IOptOutSilverlight
    {
        private readonly SaveAsStorage _storage = new SaveAsStorage(new CachingStorage(new
            FileStorage()));

        public static void Main(string[] args)
        {
            new SaveAsStorageTestCase().RunSolo();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.Storage = _storage;
        }

        public virtual void TestExistingFileWillNotBeOverWritten()
        {
            Db().Store(new Item(1));
            var oldFileName = FileSession().FileName();
            var newPath = new ByRef();
            try
            {
                newPath.value = Path.GetTempFileName();
                Assert.IsTrue(File.Exists(((string) newPath.value)));
                Assert.Expect(typeof (InvalidOperationException), new _ICodeBlock_34(this, oldFileName
                    , newPath));
                AssertItems(Db(), 1);
            }
            finally
            {
                File4.Delete(((string) newPath.value));
            }
        }

        private void AssertItems(string fileName, int count)
        {
            var objectContainer = Db4oEmbedded.OpenFile(fileName);
            AssertItems(objectContainer, count);
            objectContainer.Close();
        }

        private void AssertItems(IObjectContainer objectContainer, int count)
        {
            var items = objectContainer.Query(typeof (Item));
            Assert.AreEqual(count, items.Count);
            Assert.AreEqual(count, items.Count);
            var countCheck = 0;
            for (var itemIter = items.GetEnumerator(); itemIter.MoveNext();)
            {
                var item = ((Item) itemIter.Current);
                Assert.IsGreater(0, item._id);
                countCheck++;
            }
            Assert.AreEqual(count, countCheck);
        }

        public virtual void TestUnknownBin()
        {
            Db().Store(new Item(1));
            Assert.Expect(typeof (InvalidOperationException), new _ICodeBlock_68(this));
            AssertItems(Db(), 1);
        }

        public virtual void TestSaveAsTwice()
        {
            Db().Store(new Item(1));
            Db().Commit();
            var oldFileName = FileSession().FileName();
            var firstNewFileName = SaveOldAs(oldFileName);
            AssertItems(oldFileName, 1);
            Db().Store(new Item(2));
            Db().Commit();
            var secondNewFileName = SaveOldAs(firstNewFileName);
            AssertItems(firstNewFileName, 2);
            Db().Store(new Item(3));
            AssertItems(Db(), 3);
            Db().Commit();
            Db().Close();
            AssertItems(secondNewFileName, 3);
        }

        public virtual void TestPartialPersistence()
        {
            var oldFileName = FileSession().FileName();
            Db().Store(new Item(1));
            Db().Commit();
            Db().Store(new Item(2));
            string newPath = null;
            try
            {
                newPath = SaveOldAs(oldFileName);
                var items = Db().Query(typeof (Item));
                Assert.AreEqual(2, items.Count);
                Db().Store(new Item(3));
                Db().Close();
                AssertItems(oldFileName, 1);
                AssertItems(newPath, 3);
            }
            catch (Exception e)
            {
                Runtime.PrintStackTrace(e);
            }
            finally
            {
                File4.Delete(newPath);
            }
        }

        private string SaveOldAs(string oldFileName)
        {
            string newPath;
            newPath = Path.GetTempFileName();
            File4.Delete(newPath);
            _storage.SaveAs(oldFileName, newPath);
            return newPath;
        }

        private sealed class _ICodeBlock_34 : ICodeBlock
        {
            private readonly SaveAsStorageTestCase _enclosing;
            private readonly ByRef newPath;
            private readonly string oldFileName;

            public _ICodeBlock_34(SaveAsStorageTestCase _enclosing, string oldFileName, ByRef
                newPath)
            {
                this._enclosing = _enclosing;
                this.oldFileName = oldFileName;
                this.newPath = newPath;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing._storage.SaveAs(oldFileName, ((string) newPath.value));
            }
        }

        private sealed class _ICodeBlock_68 : ICodeBlock
        {
            private readonly SaveAsStorageTestCase _enclosing;

            public _ICodeBlock_68(SaveAsStorageTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing._storage.SaveAs("unknown", "unknown");
            }
        }

        public class Item
        {
            public int _id;

            public Item(int id)
            {
                _id = id;
            }
        }
    }
}