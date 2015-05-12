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
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Concurrency
{
    public class CascadeDeleteDeletedTestCase : Db4oClientServerTestCase
    {
        public static void Main(string[] args)
        {
            new CascadeDeleteDeletedTestCase().RunConcurrency();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Db4oSetupBeforeStore()
        {
            ConfigureThreadCount(10);
        }

        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(typeof (Item)).CascadeOnDelete(true
                );
        }

        protected override void Store()
        {
            var oc = Db();
            MembersFirst(oc, "membersFirst commit");
            MembersFirst(oc, "membersFirst");
            TwoRef(oc, "twoRef");
            TwoRef(oc, "twoRef commit");
            TwoRef(oc, "twoRef delete");
            TwoRef(oc, "twoRef delete commit");
        }

        private void MembersFirst(IExtObjectContainer oc, string name)
        {
            var item = new Item(name
                );
            item.untypedMember = new CddMember();
            item.typedMember = new CddMember();
            oc.Store(item);
        }

        private void TwoRef(IExtObjectContainer oc, string name)
        {
            var item1 = new Item(name
                );
            item1.untypedMember = new CddMember();
            item1.typedMember = new CddMember();
            var item2 = new Item(name
                );
            item2.untypedMember = item1.untypedMember;
            item2.typedMember = item1.typedMember;
            oc.Store(item1);
            oc.Store(item2);
        }

        public virtual void Conc(IExtObjectContainer oc, int seq)
        {
            if (seq == 0)
            {
                TMembersFirst(oc, "membersFirst commit");
            }
            else
            {
                if (seq == 1)
                {
                    TMembersFirst(oc, "membersFirst");
                }
                else
                {
                    if (seq == 2)
                    {
                        TTwoRef(oc, "twoRef");
                    }
                    else
                    {
                        if (seq == 3)
                        {
                            TTwoRef(oc, "twoRef commit");
                        }
                        else
                        {
                            if (seq == 4)
                            {
                                TTwoRef(oc, "twoRef delete");
                            }
                            else
                            {
                                if (seq == 5)
                                {
                                    TTwoRef(oc, "twoRef delete commit");
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual void Check(IExtObjectContainer oc)
        {
            Assert.AreEqual(0, CountOccurences(oc, typeof (CddMember
                )));
        }

        private void TMembersFirst(IExtObjectContainer oc, string name)
        {
            var commit = name.IndexOf("commit") > 1;
            var q = oc.Query();
            q.Constrain(typeof (Item));
            q.Descend("name").Constrain(name);
            var objectSet = q.Execute();
            var cdd = (Item) objectSet
                .Next();
            oc.Delete(cdd.untypedMember);
            oc.Delete(cdd.typedMember);
            if (commit)
            {
                oc.Commit();
            }
            oc.Delete(cdd);
            if (!commit)
            {
                oc.Commit();
            }
        }

        private void TTwoRef(IExtObjectContainer oc, string name)
        {
            var commit = name.IndexOf("commit") > 1;
            var delete = name.IndexOf("delete") > 1;
            var q = oc.Query();
            q.Constrain(typeof (Item));
            q.Descend("name").Constrain(name);
            var objectSet = q.Execute();
            var item1 = (Item) objectSet
                .Next();
            var item2 = (Item) objectSet
                .Next();
            if (delete)
            {
                oc.Delete(item1.untypedMember);
                oc.Delete(item1.typedMember);
            }
            oc.Delete(item1);
            if (commit)
            {
                oc.Commit();
            }
            oc.Delete(item2);
            if (!commit)
            {
                oc.Commit();
            }
        }

        public class Item
        {
            public string name;
            public CddMember typedMember;
            public object untypedMember;

            public Item(string name)
            {
                this.name = name;
            }
        }

        public class CddMember
        {
            public string name;
        }
    }
}

#endif // !SILVERLIGHT