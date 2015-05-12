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
using System.Collections.Generic;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.CLI2.Collections
{
    public class GenericDictionaryTestCase : AbstractDb4oTestCase
    {
        protected override void Store()
        {
            var dh1 = new DHolder1();
            dh1._name = "root";
            dh1.CreateDicts();
            dh1.CreateLinkList(8);

            var dh2 = new DHolder2();
            dh2.nDict1 = dh1.nDict1;
            dh2.nDict2 = dh1.nDict2;
            dh2.gDict1 = dh1.gDict1;
            dh2.gDict2 = dh1.gDict2;

            var dh1_2 = new DHolder1();
            dh1_2.CreateDicts();
            dh1_2._name = "update";

            var dh3 = new DHolder3();
            dh3._name = "identity";
            dh3.CreateDicts();

            Db().Store(dh1);
            Db().Store(dh2);
            Db().Store(dh1_2);
            Db().Store(dh3);
        }

        public void TestQuery()
        {
            var root = QueryForNamedHolder("root");
            root.CheckDictsBeforeUpdate();
        }

        public void TestUpdate()
        {
            var updateHolder = QueryForNamedHolder("update");
            updateHolder.UpdateDicts();
            updateHolder.StoreDicts(Db());
        }

        public void _TestGetByKey()
        {
            var q = NewQuery();
            q.Constrain(typeof (DHolder3));
            q.Descend("_name").Constrain("identity");
            var identityHolder = (DHolder3) q.Execute().Next();
            identityHolder.CheckDicts();
        }

        private DHolder1 QueryForNamedHolder(string name)
        {
            var holderList = Db().Query(delegate(DHolder1 holder) { return holder._name == name; });
            return holderList[0];
        }
    }

    public class DHolder1
    {
        public string _name;
        public DHolder1 _next;
        public IDictionary<DItem1, string> gDict1;
        public IDictionary<DItem2, string> gDict2;
        public IDictionary nDict1;
        public IDictionary nDict2;

        public void CreateLinkList(int length)
        {
            if (length < 1)
            {
                return;
            }
            _next = new DHolder1();
            _next._name = "Linked lHolder1 " + length;
            _next.CreateDicts();
            _next.CreateLinkList(length - 1);
        }

        public void UpdateDicts()
        {
            nDict1.Add(new DItem1("update"), "update");
            nDict2.Add(new DItem2("update"), "update");
            gDict1.Add(new DItem1("update"), "update");
            gDict2.Add(new DItem2("update"), "update");
        }

        public void CreateDicts()
        {
            nDict1 = new Dictionary<DItem1, string>();
#if SILVERLIGHT
			nDict2 = new Dictionary<DItem2, string>();
#else
            nDict2 = new SortedList<DItem2, string>();
#endif
            gDict1 = new Dictionary<DItem1, string>();
#if CF ||  SILVERLIGHT
			gDict2 = new Dictionary<DItem2, string>();
#else
            gDict2 = new SortedDictionary<DItem2, string>();
#endif

            nDict1.Add(new DItem1("n11"), "n11");
            nDict1.Add(new DItem1("n12"), "n12");

            nDict2.Add(new DItem2("n21"), "n21");
            nDict2.Add(new DItem2("n22"), "n22");
            nDict2.Add(new DItem2("n23"), "n23");

            gDict1.Add(new DItem1("g11"), "g11");
            gDict1.Add(new DItem1("g12"), "g12");
            gDict1.Add(new DItem1("g13"), "g13");

            gDict2.Add(new DItem2("g21"), "g21");
            gDict2.Add(new DItem2("g22"), "g22");
        }

        public void StoreDicts(IObjectContainer oc)
        {
            oc.Store(nDict1);
            oc.Store(nDict2);
            oc.Store(gDict1);
            oc.Store(gDict2);
        }

        public void CheckDictsBeforeUpdate()
        {
            CheckDict(nDict1, new object[] {new DItem1("n11"), new DItem1("n12")});
            CheckDict(nDict2, new object[] {new DItem2("n21"), new DItem2("n22"), new DItem2("n23")});
            CheckDict((IDictionary) gDict1, new object[] {new DItem1("g11"), new DItem1("g12"), new DItem1("g13")});
            CheckDict((IDictionary) gDict2, new object[] {new DItem2("g21"), new DItem2("g22")});
        }

        public void CheckDictsAfterUpdate()
        {
            CheckDict(nDict1, new object[] {new DItem1("n11"), new DItem1("n12"), new DItem1("update")});
            CheckDict(nDict2,
                new object[] {new DItem2("n21"), new DItem2("n22"), new DItem2("n23"), new DItem2("update")});
            CheckDict((IDictionary) gDict1,
                new object[] {new DItem1("g11"), new DItem1("g12"), new DItem1("g13"), new DItem1("update")});
            CheckDict((IDictionary) gDict2, new object[] {new DItem2("g21"), new DItem2("g22"), new DItem2("update")});
        }

        private void CheckDict(IDictionary dict, object[] expectedContent)
        {
            Assert.AreEqual(dict.Count, expectedContent.Length);
            for (var i = 0; i < expectedContent.Length; i++)
            {
                var named = expectedContent[i] as Named;
                var name = named.Name();
                var str = (string) dict[expectedContent[i]];
                Assert.AreEqual(str, name);
            }
        }
    }

    public class DHolder2
    {
        public IDictionary<DItem1, string> gDict1;
        public IDictionary<DItem2, string> gDict2;
        public IDictionary nDict1;
        public IDictionary nDict2;
    }

    public class DHolder3
    {
        public DItem3[] _items = {new DItem3("foo"), new DItem3("bar"), new DItem3("baz")};
        public string _name;
        public IDictionary<DItem3, string> gDict;
        public IDictionary nDict;

        public void CreateDicts()
        {
            nDict = new Hashtable();
            gDict = new Dictionary<DItem3, string>();

            foreach (var item in _items)
            {
                nDict.Add(item, item.Name());
                gDict.Add(item, item.Name());
            }
        }

        public void CheckDicts()
        {
            foreach (var key in nDict.Keys)
                Assert.IsNotNull(nDict[key]);

            foreach (var item in gDict.Keys)
            {
                try
                {
                    Assert.IsNotNull(gDict[item]);
                }
                catch (KeyNotFoundException)
                {
                    Assert.Fail();
                    return;
                }
            }
        }
    }

    public class DItem1 : Named, IComparable<DItem1>
    {
        public string _name;

        public DItem1()
        {
        }

        public DItem1(string name)
        {
            _name = name;
        }

        public int CompareTo(DItem1 other)
        {
            return _name.CompareTo(other._name);
        }

        public string Name()
        {
            return _name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var other = obj as DItem1;

            if (other == null)
            {
                return false;
            }

            return _name.Equals(other._name);
        }

        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }
    }

    public class DItem2 : Named, IComparable<DItem2>
    {
        public string _name;

        public DItem2()
        {
        }

        public DItem2(string name)
        {
            _name = name;
        }

        public int CompareTo(DItem2 other)
        {
            return _name.CompareTo(other._name);
        }

        public string Name()
        {
            return _name;
        }

        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var other = obj as DItem2;

            if (other == null)
            {
                return false;
            }

            return _name.Equals(other._name);
        }
    }

    public class DItem3 : Named
    {
        public string _name;

        public DItem3(string name)
        {
            _name = name;
        }

        public string Name()
        {
            return _name;
        }
    }

    public interface Named
    {
        string Name();
    }
}