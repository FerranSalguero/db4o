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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Db4objects.Db4o.Config
{
#if !CF && !SILVERLIGHT
    
    /// <summary>
    ///     translator for types that are marked with the Serializable attribute.
    ///     The Serializable translator is provided to allow persisting objects that
    ///     do not supply a convenient constructor. The use of this translator is
    ///     recommended only if:<br />
    ///     - the persistent type will never be refactored<br />
    ///     - querying for type members is not necessary<br />
    /// </summary>
    public class TSerializable : IObjectConstructor
    {
        public object OnStore(IObjectContainer objectContainer, object obj)
        {
            var memoryStream = new MemoryStream();
            new BinaryFormatter().Serialize(memoryStream, obj);
            return memoryStream.GetBuffer();
        }

        public void OnActivate(IObjectContainer objectContainer, object obj, object members)
        {
        }

        public object OnInstantiate(IObjectContainer objectContainer, object obj)
        {
            var memoryStream = new MemoryStream((byte[]) obj);
            return new BinaryFormatter().Deserialize(memoryStream);
        }

        public Type StoredClass()
        {
            return typeof (byte[]);
        }
    }
#endif
}