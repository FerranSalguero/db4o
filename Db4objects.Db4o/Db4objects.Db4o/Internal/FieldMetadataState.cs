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

namespace Db4objects.Db4o.Internal
{
    /// <exclude></exclude>
    internal class FieldMetadataState
    {
        internal static readonly FieldMetadataState NotLoaded =
            new FieldMetadataState("not loaded");

        internal static readonly FieldMetadataState Unavailable =
            new FieldMetadataState("unavailable");

        internal static readonly FieldMetadataState Available =
            new FieldMetadataState("available");

        internal static readonly FieldMetadataState Updating = new
            FieldMetadataState("updating");

        private readonly string _info;

        private FieldMetadataState(string info)
        {
            _info = info;
        }

        public override string ToString()
        {
            return _info;
        }
    }
}