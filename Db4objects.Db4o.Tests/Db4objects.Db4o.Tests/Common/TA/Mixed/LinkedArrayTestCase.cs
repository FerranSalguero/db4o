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
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.TA;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.TA.Mixed
{
    public class LinkedArrayTestCase : AbstractDb4oTestCase, IOptOutTA
    {
        internal static int TestedDepth = 7;
        private Db4oUUID _linkedArraysUUID;

        public static void Main(string[] args)
        {
            new LinkedArrayTestCase().RunAll();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.GenerateUUIDs(ConfigScope.Globally);
            config.Add(new TransparentActivationSupport());
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            var linkedArrays = LinkedArrays.NewLinkedArrayRoot(TestedDepth);
            Store(linkedArrays);
            _linkedArraysUUID = Db().GetObjectInfo(linkedArrays).GetUUID();
        }

        public virtual void TestTheTest()
        {
            for (var depth = 1; depth < TestedDepth; depth++)
            {
                var linkedArrays = LinkedArrays.NewLinkedArrays(depth);
                linkedArrays.AssertActivationDepth(depth - 1, false);
            }
        }

        public virtual void TestActivateFixedDepth()
        {
            var linkedArrays = Root();
            for (var depth = 0; depth < TestedDepth; depth++)
            {
                Db().Activate(linkedArrays, depth);
                linkedArrays.AssertActivationDepth(depth, false);
                Db().Deactivate(linkedArrays, int.MaxValue);
            }
        }

        public virtual void TestActivatingActive()
        {
            var linkedArrays = Root();
            for (var secondActivationDepth = 2;
                secondActivationDepth < TestedDepth;
                secondActivationDepth
                    ++)
            {
                for (var firstActivationDepth = 1;
                    firstActivationDepth < secondActivationDepth;
                    firstActivationDepth++)
                {
                    Db().Activate(linkedArrays, firstActivationDepth);
                    Db().Activate(linkedArrays, secondActivationDepth);
                    linkedArrays.AssertActivationDepth(secondActivationDepth, false);
                    Db().Deactivate(linkedArrays, int.MaxValue);
                }
            }
        }

        public virtual void TestActivateDefaultMode()
        {
            var linkedArrays = Root();
            Db().Activate(linkedArrays);
            linkedArrays.AssertActivationDepth(TestedDepth - 1, true);
        }

        public virtual void TestPeekPersisted()
        {
            var linkedArrays = Root();
            for (var depth = 0; depth < TestedDepth; depth++)
            {
                var peeked = (LinkedArrays) Db().PeekPersisted(linkedArrays
                    , depth, true);
                peeked.AssertActivationDepth(depth, false);
            }
        }

        public virtual void TestTransparentActivationQuery()
        {
            var linkedArray = QueryForRoot();
            linkedArray.AssertActivationDepth(TestedDepth - 1, true);
        }

        public virtual void TestTransparentActivationTraversal()
        {
            var root = QueryForRoot();
            var activatableItem = root._activatableItemArray[0];
            activatableItem.Activate(ActivationPurpose.Read);
            var descendant = activatableItem._linkedArrays;
            descendant.AssertActivationDepth(TestedDepth - 3, true);
            Db().Deactivate(activatableItem, 1);
            activatableItem.Activate(ActivationPurpose.Read);
            descendant.AssertActivationDepth(TestedDepth - 3, true);
        }

        private LinkedArrays QueryForRoot()
        {
            var q = Db().Query();
            q.Constrain(typeof (LinkedArrays));
            q.Descend("_isRoot").Constrain(true);
            return (LinkedArrays) q.Execute().Next();
        }

        private LinkedArrays Root()
        {
            return (LinkedArrays) Db().GetByUUID(_linkedArraysUUID);
        }
    }
}