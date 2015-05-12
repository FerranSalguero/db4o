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
using Db4objects.Db4o.Events;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Concurrency
{
    public class CommittedCallbackRefreshTestCase : Db4oClientServerTestCase
    {
        private readonly object _lock = new object();
        private readonly int Count = 1;

        public static void Main(string[] arguments)
        {
            new CommittedCallbackRefreshTestCase().RunConcurrency();
        }

        protected override void Store()
        {
            for (var i = 0; i < Count; i++)
            {
                var name = "original" + i;
                Store(new Item(name, new SubItem
                    (name)));
            }
        }

        public virtual void Conc(IExtObjectContainer oc, int seq)
        {
            EventRegistry(oc).Committed += new _IEventListener4_74(oc).OnEvent;
            var items = new Item
                [Count];
            var objectSet = NewQuery(typeof (Item)).Execute
                ();
            var count = 0;
            while (objectSet.HasNext())
            {
                lock (_lock)
                {
                    items[count] = (Item) objectSet.Next();
                    items[count].Check();
                    count++;
                }
            }
            for (var i = 0; i < items.Length; i++)
            {
                lock (_lock)
                {
                    items[i].Update();
                    Store(items[i]._subItem);
                    Store(items[i]);
                }
                Db().Commit();
            }
            Runtime4.Sleep(1000);
            for (var i = 0; i < items.Length; i++)
            {
                lock (_lock)
                {
                    items[i].Check();
                }
            }
            Runtime4.Sleep(3000);
        }

        private IEventRegistry EventRegistry(IExtObjectContainer oc)
        {
            return EventRegistryFactory.ForObjectContainer(oc);
        }

        public class Item
        {
            public string _name;
            public SubItem _subItem;
            public int _updates;

            public Item(string name, SubItem subItem)
            {
                _name = name;
                _subItem = subItem;
            }

            public virtual void Update()
            {
                _updates++;
                _subItem._updates++;
            }

            public virtual void Check()
            {
                Assert.IsNotNull(_name);
                Assert.AreEqual(_name, _subItem._name);
                Assert.AreEqual(_updates, _subItem._updates);
            }
        }

        public class SubItem
        {
            public string _name;
            public int _updates;

            public SubItem(string name)
            {
                _name = name;
            }
        }

        private sealed class _IEventListener4_74
        {
            private readonly IExtObjectContainer oc;

            public _IEventListener4_74(IExtObjectContainer oc)
            {
                this.oc = oc;
            }

            public void OnEvent(object sender, CommitEventArgs args)
            {
                if (oc.IsClosed())
                {
                    return;
                }
                var updated = args.Updated;
                var infos = updated.GetEnumerator();
                while (infos.MoveNext())
                {
                    var info = (IObjectInfo) infos.Current;
                    var obj = info.GetObject();
                    oc.Refresh(obj, 2);
                }
            }
        }
    }
}

#endif // !SILVERLIGHT