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

using Db4objects.Db4o.Activation;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Diagnostic;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.TA;
using Db4oUnit;
using Db4oUnit.Extensions.Fixtures;
using Db4oUnit.Extensions.Util;

namespace Db4objects.Db4o.Tests.Common.TA.Diagnostics
{
    public class TransparentActivationDiagnosticsTestCase : TransparentActivationTestCaseBase
        , IOptOutMultiSession, IOptOutDefragSolo
    {
        private readonly IDiagnosticListener _checker;

        private readonly DiagnosticsRegistered _registered
            = new DiagnosticsRegistered();

        private IDiagnosticConfiguration _diagnostic;

        public TransparentActivationDiagnosticsTestCase()
        {
            _checker = new _IDiagnosticListener_60(this);
        }

        protected override void Configure(IConfiguration config)
        {
            base.Configure(config);
            _diagnostic = config.Diagnostic();
            _diagnostic.AddListener(_checker);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Db4oTearDownBeforeClean()
        {
            WorkaroundOsgiConfigCloningBehavior();
            Db().Ext().Configure().Diagnostic().RemoveAllListeners();
            base.Db4oTearDownBeforeClean();
        }

        private void WorkaroundOsgiConfigCloningBehavior()
        {
            // fix for Osgi config cloning behavior - see Db4oOSGiBundleFixture
            _diagnostic.RemoveAllListeners();
        }

        public virtual void TestTADiagnostics()
        {
            Store(new SomeTAAwareData(1));
            Assert.AreEqual(0, _registered._registeredCount);
            Store(new SomeOtherTAAwareData(new SomeTAAwareData
                (2)));
            Assert.AreEqual(0, _registered._registeredCount);
            Store(new NotTAAwareData(new SomeTAAwareData
                (3)));
            Assert.AreEqual(1, _registered._registeredCount);
        }

        public static void Main(string[] args)
        {
            new TransparentActivationDiagnosticsTestCase().RunAll();
        }

        public class SomeTAAwareData
        {
            public int _id;

            public SomeTAAwareData(int id)
            {
                _id = id;
            }
        }

        public class SomeOtherTAAwareData : IActivatable
        {
            public SomeTAAwareData _data;

            public SomeOtherTAAwareData(SomeTAAwareData
                data)
            {
                _data = data;
            }

            public virtual void Bind(IActivator activator)
            {
            }

            public virtual void Activate(ActivationPurpose purpose)
            {
            }
        }

        public class NotTAAwareData
        {
            public SomeTAAwareData _data;

            public NotTAAwareData(SomeTAAwareData data
                )
            {
                _data = data;
            }
        }

        private class DiagnosticsRegistered
        {
            public int _registeredCount;
        }

        private sealed class _IDiagnosticListener_60 : IDiagnosticListener
        {
            private readonly TransparentActivationDiagnosticsTestCase _enclosing;

            public _IDiagnosticListener_60(TransparentActivationDiagnosticsTestCase _enclosing
                )
            {
                this._enclosing = _enclosing;
            }

            public void OnDiagnostic(IDiagnostic diagnostic)
            {
                if (!(diagnostic is NotTransparentActivationEnabled))
                {
                    return;
                }
                var taDiagnostic = (NotTransparentActivationEnabled) diagnostic;
                Assert.AreEqual(CrossPlatformServices.FullyQualifiedName(typeof (NotTAAwareData
                    )), ((ClassMetadata) taDiagnostic.Reason()).GetName());
                _enclosing._registered._registeredCount++;
            }
        }
    }
}