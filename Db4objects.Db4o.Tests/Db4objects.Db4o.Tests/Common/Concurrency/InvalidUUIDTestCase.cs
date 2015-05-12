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
using Sharpen.Lang;

namespace Db4objects.Db4o.Tests.Common.Concurrency
{
    public class InvalidUUIDTestCase : Db4oClientServerTestCase
    {
        public string name;

        public static void Main(string[] args)
        {
            new InvalidUUIDTestCase().RunConcurrency();
        }

        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(GetType()).GenerateUUIDs(true);
        }

        protected override void Store()
        {
            name = "theOne";
            Store(this);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void Conc(IExtObjectContainer oc)
        {
            var os = oc.Query(typeof (InvalidUUIDTestCase));
            if (os.Count == 0)
            {
                // already deleted by other threads
                return;
            }
            Assert.AreEqual(1, os.Count);
            var iu = (InvalidUUIDTestCase) os.Next();
            var myUuid = oc.GetObjectInfo(iu).GetUUID();
            Assert.IsNotNull(myUuid);
            var mySignature = myUuid.GetSignaturePart();
            var myLong = myUuid.GetLongPart();
            var unknownLong = long.MaxValue - 100;
            byte[] unknownSignature = {1, 2, 4, 99, 33, 22};
            var unknownLongPart = new Db4oUUID(unknownLong, mySignature);
            var unknownSignaturePart = new Db4oUUID(myLong, unknownSignature);
            var unknownBoth = new Db4oUUID(unknownLong, unknownSignature);
            Assert.IsNull(oc.GetByUUID(unknownLongPart));
            Assert.IsNull(oc.GetByUUID(unknownSignaturePart));
            Assert.IsNull(oc.GetByUUID(unknownBoth));
            Assert.IsNull(oc.GetByUUID(unknownLongPart));
            Thread.Sleep(500);
            oc.Delete(iu);
            oc.Commit();
            Assert.IsNull(oc.GetByUUID(myUuid));
        }
    }
}

#endif // !SILVERLIGHT