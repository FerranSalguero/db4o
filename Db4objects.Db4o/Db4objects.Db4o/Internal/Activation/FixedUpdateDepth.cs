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

namespace Db4objects.Db4o.Internal.Activation
{
    public abstract class FixedUpdateDepth : IUpdateDepth
    {
        private readonly int _depth;
        private bool _tpMode;

        public FixedUpdateDepth(int depth)
        {
            _depth = depth;
        }

        public virtual bool SufficientDepth()
        {
            return _depth > 0;
        }

        public virtual bool Negative()
        {
            // should never happen?
            return _depth < 0;
        }

        public virtual IUpdateDepth Adjust(ClassMetadata clazz)
        {
            if (clazz.CascadesOnDeleteOrUpdate())
            {
                return AdjustDepthToBorders().Descend();
            }
            return Descend();
        }

        public virtual IUpdateDepth AdjustUpdateDepthForCascade(bool isCollection)
        {
            var minimumUpdateDepth = isCollection ? 2 : 1;
            if (_depth < minimumUpdateDepth)
            {
                return ForDepth(minimumUpdateDepth);
            }
            return this;
        }

        public virtual IUpdateDepth Descend()
        {
            return ForDepth(_depth - 1);
        }

        public abstract bool CanSkip(ObjectReference arg1);

        public virtual void TpMode(bool tpMode)
        {
            _tpMode = tpMode;
        }

        public virtual bool TpMode()
        {
            return _tpMode;
        }

        public override string ToString()
        {
            return GetType().FullName + ": " + _depth;
        }

        public virtual bool IsBroaderThan(FixedUpdateDepth
            other)
        {
            return _depth > other._depth;
        }

        // TODO code duplication in fixed activation/update depth
        public virtual FixedUpdateDepth AdjustDepthToBorders
            ()
        {
            return ForDepth(DepthUtil.AdjustDepthToBorders(_depth));
        }

        public override bool Equals(object other)
        {
            if (this == other)
            {
                return true;
            }
            if (other == null || GetType() != other.GetType())
            {
                return false;
            }
            return _depth == ((FixedUpdateDepth) other)._depth;
        }

        public override int GetHashCode()
        {
            return _depth;
        }

        protected abstract FixedUpdateDepth ForDepth(
            int depth);
    }
}