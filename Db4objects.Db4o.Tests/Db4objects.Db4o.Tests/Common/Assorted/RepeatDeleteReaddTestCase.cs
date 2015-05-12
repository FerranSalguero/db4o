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
using System.Text;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Tests.Common.Api;
using Db4oUnit;
using Sharpen.IO;

namespace Db4objects.Db4o.Tests.Common.Assorted
{
    public class RepeatDeleteReaddTestCase : Db4oTestWithTempFile
    {
        private const int NumItemsPerClass = 10;
        private const int DeleteRatio = 3;
        private readonly int NumRuns = 10;

        public static void Main(string[] args)
        {
            new ConsoleTestRunner(typeof (RepeatDeleteReaddTestCase)).Run();
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void Test()
        {
            for (var idx = 0; idx < NumRuns; idx++)
            {
                AssertRun();
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        private void AssertRun()
        {
            var fileName = TempFile();
            new File(fileName).Delete();
            CreateDatabase(fileName);
            AssertCanRead(fileName);
            new File(fileName).Delete();
        }

        private void CreateDatabase(string fileName)
        {
            IObjectContainer db = Db4oEmbedded.OpenFile(Config(), fileName);
            var removed = new Collection4();
            for (var idx = 0; idx < NumItemsPerClass; idx++)
            {
                var itemA = new ItemA(idx);
                var itemB = new ItemB(FillStr
                    ('x', idx));
                db.Store(itemA);
                db.Store(itemB);
                if ((idx%DeleteRatio) == 0)
                {
                    removed.Add(itemA);
                    removed.Add(itemB);
                }
            }
            db.Commit();
            DeleteAndReadd(db, removed);
            db.Close();
        }

        private void DeleteAndReadd(IObjectContainer db, Collection4 removed)
        {
            var removeIter = removed.GetEnumerator();
            while (removeIter.MoveNext())
            {
                var cur = removeIter.Current;
                db.Delete(cur);
            }
            db.Commit();
            var readdIter = removed.GetEnumerator();
            while (readdIter.MoveNext())
            {
                var cur = readdIter.Current;
                db.Store(cur);
            }
            db.Commit();
        }

        private void AssertCanRead(string fileName)
        {
            IObjectContainer db = Db4oEmbedded.OpenFile(Config(), fileName);
            AssertResults(db);
            db.Close();
        }

        private void AssertResults(IObjectContainer db)
        {
            AssertResult(db, typeof (ItemA));
            AssertResult(db, typeof (ItemB));
        }

        private void AssertResult(IObjectContainer db, Type clazz)
        {
            var result = db.Query(clazz);
            Assert.AreEqual(NumItemsPerClass, result.Count);
            while (result.HasNext())
            {
                Assert.IsInstanceOf(clazz, result.Next());
            }
        }

        private IEmbeddedConfiguration Config()
        {
            var config = NewConfiguration();
            config.Common.ReflectWith(Platform4.ReflectorForType(typeof (ItemA
                )));
            return config;
        }

        private string FillStr(char ch, int len)
        {
            var buf = new StringBuilder();
            for (var idx = 0; idx < len; idx++)
            {
                buf.Append(ch);
            }
            return buf.ToString();
        }

        public class ItemA
        {
            public int _id;

            public ItemA(int id)
            {
                _id = id;
            }

            public override string ToString()
            {
                return "A" + _id;
            }
        }

        public class ItemB
        {
            public string _name;

            public ItemB(string name)
            {
                _name = name;
            }

            public override string ToString()
            {
                return "A" + _name;
            }
        }
    }
}