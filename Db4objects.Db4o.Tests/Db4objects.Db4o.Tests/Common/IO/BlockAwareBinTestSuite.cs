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
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.IO;
using Db4oUnit;
using Db4oUnit.Fixtures;
using Db4oUnit.Mocking;
using Sharpen.Lang;

namespace Db4objects.Db4o.Tests.Common.IO
{
    public class BlockAwareBinTestSuite : FixtureTestSuiteDescription
    {
        public BlockAwareBinTestSuite()
        {
            {
                FixtureProviders(new IFixtureProvider[]
                {
                    new SubjectFixtureProvider(new[]
                    {
                        2, 3, 17
                    })
                });
                TestUnits(new[] {typeof (BlockAwareBinTest)});
            }
        }

        public class BlockAwareBinTest : ITestCase, IEnvironment
        {
            private readonly MockBin _mockBin = new MockBin();
            private readonly IBlockSize _mockBlockSize;
            private BlockAwareBin _subject;

            public BlockAwareBinTest()
            {
                _mockBlockSize = new _IBlockSize_20(this);
                Environments.RunWith(this, new _IRunnable_37(this));
            }

            public virtual object Provide(Type service)
            {
                if (service != typeof (IBlockSize))
                {
                    throw new ArgumentException();
                }
                return _mockBlockSize;
            }

            public virtual void TestBlockSize()
            {
                Assert.AreEqual(BlockSize(), _subject.BlockSize());
            }

            public virtual void TestClose()
            {
                _subject.Close();
                Verify(new[] {new MethodCall("close", new object[] {})});
            }

            public virtual void TestSync()
            {
                _subject.Sync();
                Verify(new[] {new MethodCall("sync", new object[] {})});
            }

            public virtual void TestBlockReadReturnsStorageReturnValue()
            {
                _mockBin.ReturnValueForNextCall(-1);
                Assert.AreEqual(-1, _subject.BlockRead(0, new byte[10]));
            }

            public virtual void TestBlockRead()
            {
                var buffer = new byte[10];
                _subject.BlockRead(0, buffer);
                _subject.BlockRead(1, buffer, 5);
                _subject.BlockRead(42, buffer);
                Verify(new[]
                {
                    new MethodCall("read", new object[]
                    {
                        0L, buffer, buffer
                            .Length
                    }),
                    new MethodCall("read", new object[] {(long) BlockSize(), buffer, 5}
                        ),
                    new MethodCall("read", new object[]
                    {
                        42L*BlockSize(), buffer, buffer.Length
                    })
                });
            }

            public virtual void TestBlockReadWithOffset()
            {
                var buffer = new byte[10];
                _subject.BlockRead(0, 1, buffer);
                _subject.BlockRead(1, 3, buffer, 5);
                _subject.BlockRead(42, 5, buffer);
                Verify(new[]
                {
                    new MethodCall("read", new object[]
                    {
                        1L, buffer, buffer
                            .Length
                    }),
                    new MethodCall("read", new object[]
                    {
                        3 + (long) BlockSize(), buffer,
                        5
                    }),
                    new MethodCall("read", new object[]
                    {
                        5 + 42L*BlockSize(), buffer, buffer
                            .Length
                    })
                });
            }

            public virtual void TestBlockWrite()
            {
                var buffer = new byte[10];
                _subject.BlockWrite(0, buffer);
                _subject.BlockWrite(1, buffer, 5);
                _subject.BlockWrite(42, buffer);
                Verify(new[]
                {
                    new MethodCall("write", new object[]
                    {
                        0L, buffer, buffer
                            .Length
                    }),
                    new MethodCall("write", new object[] {(long) BlockSize(), buffer, 5}
                        ),
                    new MethodCall("write", new object[]
                    {
                        42L*BlockSize(), buffer, buffer.Length
                    })
                });
            }

            public virtual void TestBlockWriteWithOffset()
            {
                var buffer = new byte[10];
                _subject.BlockWrite(0, 1, buffer);
                _subject.BlockWrite(1, 3, buffer, 5);
                _subject.BlockWrite(42, 5, buffer);
                Verify(new[]
                {
                    new MethodCall("write", new object[]
                    {
                        1L, buffer, buffer
                            .Length
                    }),
                    new MethodCall("write", new object[]
                    {
                        3 + (long) BlockSize(), buffer
                        , 5
                    }),
                    new MethodCall("write", new object[]
                    {
                        5 + 42L*BlockSize(), buffer, buffer
                            .Length
                    })
                });
            }

            private void Verify(MethodCall[] expectedCalls)
            {
                _mockBin.Verify(expectedCalls);
            }

            private int BlockSize()
            {
                return ((int) SubjectFixtureProvider.Value());
            }

            private sealed class _IBlockSize_20 : IBlockSize
            {
                private readonly BlockAwareBinTest _enclosing;

                public _IBlockSize_20(BlockAwareBinTest _enclosing)
                {
                    this._enclosing = _enclosing;
                }

                public void Register(IListener4 listener)
                {
                    throw new NotImplementedException();
                }

                public void Set(int newValue)
                {
                    Assert.AreEqual(_enclosing.BlockSize(), newValue);
                }

                public int Value()
                {
                    return _enclosing.BlockSize();
                }
            }

            private sealed class _IRunnable_37 : IRunnable
            {
                private readonly BlockAwareBinTest _enclosing;

                public _IRunnable_37(BlockAwareBinTest _enclosing)
                {
                    this._enclosing = _enclosing;
                }

                public void Run()
                {
                    _enclosing._subject = new BlockAwareBin(_enclosing._mockBin);
                }
            }
        }
    }
}