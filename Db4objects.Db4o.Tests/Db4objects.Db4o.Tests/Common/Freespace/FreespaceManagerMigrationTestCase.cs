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

using System.Collections;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Tests.Common.Handlers;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Freespace
{
    /// <exclude></exclude>
    public class FreespaceManagerMigrationTestCase : FormatMigrationTestCaseBase
    {
        internal int[][] IntArrayData =
        {
            new[] {1, 2}, new[]
            {
                3,
                4
            }
        };

        internal string[][] StringArrayData =
        {
            new[] {"a", "b"},
            new[] {"c", "d"}
        };

        protected override void ConfigureForStore(IConfiguration config)
        {
            CommonConfigure(config);
            config.Freespace().UseIndexSystem();
        }

        protected override bool IsApplicableForDb4oVersion()
        {
            return Db4oMajorVersion() >= 5;
        }

        protected override void ConfigureForTest(IConfiguration config)
        {
            CommonConfigure(config);
            config.Freespace().UseBTreeSystem();
        }

        protected override void DeconfigureForStore(IConfiguration config)
        {
            if (!IsApplicableForDb4oVersion())
            {
                return;
            }
            config.Freespace().UseRamSystem();
        }

        protected override void DeconfigureForTest(IConfiguration config)
        {
            if (!IsApplicableForDb4oVersion())
            {
                return;
            }
            config.Freespace().UseRamSystem();
        }

        private void CommonConfigure(IConfiguration config)
        {
            // config.blockSize(8);
            config.ObjectClass(typeof (StClass)).CascadeOnActivate
                (true);
            config.ObjectClass(typeof (StClass)).CascadeOnUpdate
                (true);
            config.ObjectClass(typeof (StClass)).CascadeOnDelete
                (true);
            config.ObjectClass(typeof (StClass)).MinimumActivationDepth
                (5);
            config.ObjectClass(typeof (StClass)).UpdateDepth
                (10);
        }

        protected override void AssertObjectsAreReadable(IExtObjectContainer objectContainer
            )
        {
            var objectSet = objectContainer.Query(typeof (StClass
                ));
            for (var i = 0; i < 2; i++)
            {
                var cls = (StClass
                    ) objectSet.Next();
                var v = cls.GetVect();
                var intArray = (int[][]) v[0];
                ArrayAssert.AreEqual(IntArrayData[0], intArray[0]);
                ArrayAssert.AreEqual(IntArrayData[1], intArray[1]);
                var stringArray = (string[][]) v[1];
                ArrayAssert.AreEqual(StringArrayData[0], stringArray[0]);
                ArrayAssert.AreEqual(StringArrayData[1], stringArray[1]);
                objectContainer.Delete(cls);
            }
        }

        protected override string FileNamePrefix()
        {
            return "freespace";
        }

        protected override void Store(IObjectContainerAdapter objectContainer)
        {
            for (var i = 0; i < 10; i++)
            {
                var cls = new StClass
                    ();
                var v = new ArrayList(10);
                v.Add(IntArrayData);
                v.Add(StringArrayData);
                cls.SetId(i);
                cls.SetVect(v);
                objectContainer.Store(cls);
            }
        }

        public class StClass
        {
            public int id;
            public ArrayList vect;

            public virtual ArrayList GetVect()
            {
                return vect;
            }

            public virtual void SetVect(ArrayList vect)
            {
                this.vect = vect;
            }

            public virtual int GetId()
            {
                return id;
            }

            public virtual void SetId(int id)
            {
                this.id = id;
            }
        }
    }
}