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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Ext;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Concurrency
{
    public class CascadeDeleteFalseTestCase : Db4oClientServerTestCase
    {
        public static void Main(string[] args)
        {
            new CascadeDeleteFalseTestCase().RunConcurrency();
        }

        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(typeof (Item)).CascadeOnDelete(true);
            config.ObjectClass(typeof (Item)).ObjectField("h3").CascadeOnDelete
                (false);
        }

        protected override void Store()
        {
            var item = new Item();
            item.h1 = new CascadeDeleteFalseHelper();
            item.h2 = new CascadeDeleteFalseHelper();
            item.h3 = new CascadeDeleteFalseHelper();
            Store(item);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void ConcDelete(IExtObjectContainer oc)
        {
            var os = oc.Query(typeof (Item));
            if (os.Count == 0)
            {
                // the object has been deleted
                return;
            }
            if (!os.HasNext())
            {
                // object can be deleted after query 
                return;
            }
            var cdf = (Item) os.Next();
            // sleep 1000 ms, waiting for other threads.
            // Thread.sleep(500);
            oc.Delete(cdf);
            oc.Commit();
            AssertOccurrences(oc, typeof (Item), 0);
            AssertOccurrences(oc, typeof (CascadeDeleteFalseHelper)
                , 1);
        }

        public virtual void CheckDelete(IExtObjectContainer oc)
        {
            AssertOccurrences(oc, typeof (CascadeDeleteFalseTestCase), 0);
            AssertOccurrences(oc, typeof (CascadeDeleteFalseHelper)
                , 1);
        }

        public class Item
        {
            public CascadeDeleteFalseHelper h1;
            public CascadeDeleteFalseHelper h2;
            public CascadeDeleteFalseHelper h3;
        }

        public class CascadeDeleteFalseHelper
        {
        }
    }
}

#endif // !SILVERLIGHT