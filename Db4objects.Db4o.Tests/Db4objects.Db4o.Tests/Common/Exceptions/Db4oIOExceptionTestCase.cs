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

using Db4objects.Db4o.Config;
using Db4objects.Db4o.Ext;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Exceptions
{
    public class Db4oIOExceptionTestCase : Db4oIOExceptionTestCaseBase
    {
        public static void Main(string[] args)
        {
            new Db4oIOExceptionTestCase().RunSolo();
        }

        protected override void Configure(IConfiguration config)
        {
            base.Configure(config);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestActivate()
        {
            Store(new Item(3));
            Fixture().Config().ActivationDepth(1);
            Fixture().Reopen(this);
            var item = (Item
                ) RetrieveOnlyInstance(typeof (Item));
            Assert.Expect(typeof (Db4oIOException), new _ICodeBlock_25(this, item));
        }

        public virtual void TestClose()
        {
            Assert.Expect(typeof (Db4oIOException), new _ICodeBlock_34(this));
        }

        public virtual void TestCommit()
        {
            Store(new Item(0));
            Assert.Expect(typeof (Db4oIOException), new _ICodeBlock_44(this));
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestDelete()
        {
            Store(new Item(3));
            var item = (Item
                ) RetrieveOnlyInstance(typeof (Item));
            Assert.Expect(typeof (Db4oIOException), new _ICodeBlock_55(this, item));
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestGet()
        {
            Store(new Item(3));
            Assert.Expect(typeof (Db4oIOException), new _ICodeBlock_65(this));
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestGetAll()
        {
            Store(new Item(3));
            Assert.Expect(typeof (Db4oIOException), new _ICodeBlock_75(this));
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestQuery()
        {
            Store(new Item(3));
            Assert.Expect(typeof (Db4oIOException), new _ICodeBlock_88(this));
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestRollback()
        {
            Store(new Item(3));
            Assert.Expect(typeof (Db4oIOException), new _ICodeBlock_98(this));
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestSet()
        {
            Assert.Expect(typeof (Db4oIOException), new _ICodeBlock_107(this));
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestGetByUUID()
        {
            Fixture().Config().GenerateUUIDs(ConfigScope.Globally);
            Fixture().Reopen(this);
            var item = new Item(1);
            Store(item);
            var uuid = Db().GetObjectInfo(item).GetUUID();
            Fixture().Reopen(this);
            Assert.Expect(typeof (Db4oIOException), new _ICodeBlock_122(this, uuid));
        }

        private sealed class _ICodeBlock_25 : ICodeBlock
        {
            private readonly Db4oIOExceptionTestCase _enclosing;
            private readonly Item item;

            public _ICodeBlock_25(Db4oIOExceptionTestCase _enclosing, Item
                item)
            {
                this._enclosing = _enclosing;
                this.item = item;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.TriggerException(true);
                _enclosing.Db().Activate(item, 3);
            }
        }

        private sealed class _ICodeBlock_34 : ICodeBlock
        {
            private readonly Db4oIOExceptionTestCase _enclosing;

            public _ICodeBlock_34(Db4oIOExceptionTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.TriggerException(true);
                _enclosing.Db().Close();
            }
        }

        private sealed class _ICodeBlock_44 : ICodeBlock
        {
            private readonly Db4oIOExceptionTestCase _enclosing;

            public _ICodeBlock_44(Db4oIOExceptionTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.TriggerException(true);
                _enclosing.Db().Commit();
            }
        }

        private sealed class _ICodeBlock_55 : ICodeBlock
        {
            private readonly Db4oIOExceptionTestCase _enclosing;
            private readonly Item item;

            public _ICodeBlock_55(Db4oIOExceptionTestCase _enclosing, Item
                item)
            {
                this._enclosing = _enclosing;
                this.item = item;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.TriggerException(true);
                _enclosing.Db().Delete(item);
            }
        }

        private sealed class _ICodeBlock_65 : ICodeBlock
        {
            private readonly Db4oIOExceptionTestCase _enclosing;

            public _ICodeBlock_65(Db4oIOExceptionTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.TriggerException(true);
                _enclosing.Db().QueryByExample(typeof (Item));
            }
        }

        private sealed class _ICodeBlock_75 : ICodeBlock
        {
            private readonly Db4oIOExceptionTestCase _enclosing;

            public _ICodeBlock_75(Db4oIOExceptionTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.TriggerException(true);
                var os = _enclosing.Db().QueryByExample(null);
                while (os.HasNext())
                {
                    os.Next();
                }
            }
        }

        private sealed class _ICodeBlock_88 : ICodeBlock
        {
            private readonly Db4oIOExceptionTestCase _enclosing;

            public _ICodeBlock_88(Db4oIOExceptionTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.TriggerException(true);
                _enclosing.Db().Query(typeof (Item));
            }
        }

        private sealed class _ICodeBlock_98 : ICodeBlock
        {
            private readonly Db4oIOExceptionTestCase _enclosing;

            public _ICodeBlock_98(Db4oIOExceptionTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.TriggerException(true);
                _enclosing.Db().Rollback();
            }
        }

        private sealed class _ICodeBlock_107 : ICodeBlock
        {
            private readonly Db4oIOExceptionTestCase _enclosing;

            public _ICodeBlock_107(Db4oIOExceptionTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.TriggerException(true);
                _enclosing.Db().Store(new Item(3));
            }
        }

        private sealed class _ICodeBlock_122 : ICodeBlock
        {
            private readonly Db4oIOExceptionTestCase _enclosing;
            private readonly Db4oUUID uuid;

            public _ICodeBlock_122(Db4oIOExceptionTestCase _enclosing, Db4oUUID uuid)
            {
                this._enclosing = _enclosing;
                this.uuid = uuid;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.TriggerException(true);
                _enclosing.Db().GetByUUID(uuid);
            }
        }

        public class Item
        {
            public DeepMemeber member;

            public Item(int depth)
            {
                member = new DeepMemeber(depth);
            }
        }

        public class DeepMemeber
        {
            public DeepMemeber member;

            public DeepMemeber(int depth)
            {
                if (depth > 0)
                {
                    member = new DeepMemeber(--depth);
                }
            }
        }
    }
}