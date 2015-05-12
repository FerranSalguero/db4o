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

using Db4objects.Db4o.Internal;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Btree
{
    public class BTreeFreeTestCase : BTreeTestCaseBase
    {
        private static readonly int[] Values = {1, 2, 5, 7, 8, 9, 12};

        public static void Main(string[] args)
        {
            new BTreeFreeTestCase().RunSolo();
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void Test()
        {
            Add(Values);
            Trans().Commit();
            BTreeAssert.AssertAllSlotsFreed(FileTransaction(), _btree, new _ICodeBlock_22(this
                ));
        }

        private LocalTransaction FileTransaction()
        {
            return ((LocalTransaction) Trans());
        }

        private sealed class _ICodeBlock_22 : ICodeBlock
        {
            private readonly BTreeFreeTestCase _enclosing;

            public _ICodeBlock_22(BTreeFreeTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing._btree.Free((LocalTransaction) _enclosing.SystemTrans());
                _enclosing.SystemTrans().Commit();
            }
        }
    }
}