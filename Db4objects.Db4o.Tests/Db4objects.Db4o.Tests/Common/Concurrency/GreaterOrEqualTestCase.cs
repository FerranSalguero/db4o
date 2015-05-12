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

#if !SILVERLIGHT
using Db4objects.Db4o.Ext;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Concurrency
{
    public class GreaterOrEqualTestCase : Db4oClientServerTestCase
    {
        public int val;

        public GreaterOrEqualTestCase()
        {
        }

        public GreaterOrEqualTestCase(int val)
        {
            this.val = val;
        }

        public static void Main(string[] args)
        {
            new GreaterOrEqualTestCase().RunConcurrency
                ();
        }

        protected override void Store()
        {
            Store(new GreaterOrEqualTestCase(1));
            Store(new GreaterOrEqualTestCase(2));
            Store(new GreaterOrEqualTestCase(3));
            Store(new GreaterOrEqualTestCase(4));
            Store(new GreaterOrEqualTestCase(5));
        }

        public virtual void Conc(IExtObjectContainer oc)
        {
            int[] expect = {3, 4, 5};
            var q = oc.Query();
            q.Constrain(typeof (GreaterOrEqualTestCase
                ));
            q.Descend("val").Constrain(3).Greater().Equal();
            var res = q.Execute();
            while (res.HasNext())
            {
                var r = (GreaterOrEqualTestCase
                    ) res.Next();
                for (var i = 0; i < expect.Length; i++)
                {
                    if (expect[i] == r.val)
                    {
                        expect[i] = 0;
                    }
                }
            }
            for (var i = 0; i < expect.Length; i++)
            {
                Assert.AreEqual(0, expect[i]);
            }
        }
    }
}

#endif // !SILVERLIGHT