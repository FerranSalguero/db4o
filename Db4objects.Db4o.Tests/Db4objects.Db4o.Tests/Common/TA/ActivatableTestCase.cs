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

using Db4objects.Db4o.Ext;
using Db4oUnit;
using Db4oUnit.Mocking;

namespace Db4objects.Db4o.Tests.Common.TA
{
    public class ActivatableTestCase : TransparentActivationTestCaseBase
    {
        public static void Main(string[] args)
        {
            new ActivatableTestCase().RunAll();
        }

        public virtual void TestActivatorIsBoundUponStore()
        {
            var mock = StoreNewMock();
            AssertSingleBindCall(mock);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestActivatorIsBoundUponRetrieval()
        {
            StoreNewMock();
            Reopen();
            AssertSingleBindCall(RetrieveMock());
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestActivatorIsUnboundUponClose()
        {
            var mock = StoreNewMock();
            Reopen();
            AssertBindUnbindCalls(mock);
        }

        public virtual void TestUnbindingIsIsolated()
        {
            if (!IsMultiSession())
            {
                return;
            }
            var mock1 = StoreNewMock();
            Db().Commit();
            var mock2 = RetrieveMockFromNewClientAndClose();
            AssertBindUnbindCalls(mock2);
            // mock1 has only be bound by store so far
            // client.close should have no effect on it
            mock1.Recorder().Verify(new[]
            {
                new MethodCall("bind", new object[]
                {
                    new _IArgumentCondition_50()
                })
            });
        }

        private MockActivatable RetrieveMockFromNewClientAndClose()
        {
            var client = OpenNewSession();
            try
            {
                return RetrieveMock(client);
            }
            finally
            {
                client.Close();
            }
        }

        private void AssertBindUnbindCalls(MockActivatable mock)
        {
            mock.Recorder().Verify(new[]
            {
                new MethodCall("bind", new[]
                {
                    MethodCall
                        .IgnoredArgument
                }),
                new MethodCall("bind", new object[] {null})
            });
        }

        private void AssertSingleBindCall(MockActivatable mock)
        {
            mock.Recorder().Verify(new[]
            {
                new MethodCall("bind", new[]
                {
                    MethodCall
                        .IgnoredArgument
                })
            });
        }

        private MockActivatable RetrieveMock()
        {
            return RetrieveMock(Db());
        }

        private MockActivatable RetrieveMock(IExtObjectContainer container)
        {
            return (MockActivatable) RetrieveOnlyInstance(container, typeof (
                MockActivatable));
        }

        private MockActivatable StoreNewMock()
        {
            var mock = new MockActivatable();
            Store(mock);
            return mock;
        }

        private sealed class _IArgumentCondition_50 : MethodCall.IArgumentCondition
        {
            public void Verify(object argument)
            {
                Assert.IsNotNull(argument);
            }
        }
    }
}