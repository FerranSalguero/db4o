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

namespace Db4objects.Db4o.Diagnostic
{
    /// <summary>Diagnostic to recommend Defragment when needed.</summary>
    /// <remarks>Diagnostic to recommend Defragment when needed.</remarks>
    public class DefragmentRecommendation : DiagnosticBase
    {
        private readonly DefragmentRecommendationReason _reason;

        public DefragmentRecommendation(DefragmentRecommendationReason
            reason)
        {
            _reason = reason;
        }

        public override string Problem()
        {
            return "Database file format is old or database is highly fragmented.";
        }

        public override object Reason()
        {
            return _reason._message;
        }

        public override string Solution()
        {
            return "Defragment the database.";
        }

        public class DefragmentRecommendationReason
        {
            public static readonly DefragmentRecommendationReason DeleteEmbeded
                = new DefragmentRecommendationReason("Delete Embedded not supported on old file format."
                    );

            internal readonly string _message;

            public DefragmentRecommendationReason(string reason)
            {
                _message = reason;
            }
        }
    }
}