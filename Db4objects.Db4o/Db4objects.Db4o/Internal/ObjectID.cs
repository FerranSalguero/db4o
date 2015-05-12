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

using Db4objects.Db4o.Internal.Marshall;

namespace Db4objects.Db4o.Internal
{
    /// <exclude></exclude>
    public class ObjectID
    {
        public static readonly ObjectID IsNull = new _ObjectID_15
            (-1);

        public static readonly ObjectID NotPossible = new _ObjectID_21
            (-2);

        public static readonly ObjectID Ignore = new _ObjectID_27
            (-3);

        public readonly int _id;

        public ObjectID(int id)
        {
            _id = id;
        }

        public virtual bool IsValid()
        {
            return _id > 0;
        }

        public static ObjectID Read(IInternalReadContext context
            )
        {
            var id = context.ReadInt();
            return id == 0
                ? IsNull
                : new ObjectID
                    (id);
        }

        public override string ToString()
        {
            return "ObjectID(" + _id + ")";
        }

        private sealed class _ObjectID_15 : ObjectID
        {
            public _ObjectID_15(int baseArg1) : base(baseArg1)
            {
            }

            public override string ToString()
            {
                return "ObjectID.IS_NULL";
            }
        }

        private sealed class _ObjectID_21 : ObjectID
        {
            public _ObjectID_21(int baseArg1) : base(baseArg1)
            {
            }

            public override string ToString()
            {
                return "ObjectID.NOT_POSSIBLE";
            }
        }

        private sealed class _ObjectID_27 : ObjectID
        {
            public _ObjectID_27(int baseArg1) : base(baseArg1)
            {
            }

            public override string ToString()
            {
                return "ObjectID.IGNORE";
            }
        }
    }
}