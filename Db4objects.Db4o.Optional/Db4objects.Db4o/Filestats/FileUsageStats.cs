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
using System.Collections;
using System.Text;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal.Slots;

namespace Db4objects.Db4o.Filestats
{
    /// <summary>Byte usage statistics for a db4o database file</summary>
    public class FileUsageStats
    {
        private readonly long _classMetadata;
        private readonly long _commitTimestampUsage;
        private readonly long _fileHeader;
        private readonly long _fileSize;
        private readonly long _freespace;
        private readonly long _freespaceUsage;
        private readonly long _idSystem;
        private readonly ISlotMap _slots;
        private readonly long _uuidUsage;
        private TreeStringObject _classUsageStats;

        internal FileUsageStats(long fileSize, long fileHeader, long idSystem, long freespace
            , long classMetadata, long freespaceUsage, long uuidUsage, ISlotMap slots, long
                commitTimestampUsage)
        {
            _fileSize = fileSize;
            _fileHeader = fileHeader;
            _idSystem = idSystem;
            _freespace = freespace;
            _classMetadata = classMetadata;
            _freespaceUsage = freespaceUsage;
            _uuidUsage = uuidUsage;
            _slots = slots;
            _commitTimestampUsage = commitTimestampUsage;
        }

        /// <returns>bytes used by the db4o file header (static and variable parts)</returns>
        public virtual long FileHeader()
        {
            return _fileHeader;
        }

        /// <returns>total number of bytes registered as freespace, available for reuse</returns>
        public virtual long Freespace()
        {
            return _freespace;
        }

        /// <returns>bytes used by the id system indices</returns>
        public virtual long IdSystem()
        {
            return _idSystem;
        }

        /// <returns>
        ///     number of bytes used for class metadata (class metadata repository and schema definitions)
        /// </returns>
        public virtual long ClassMetadata()
        {
            return _classMetadata;
        }

        /// <returns>number of bytes used for the bookkeeping of the freespace system itself</returns>
        public virtual long FreespaceUsage()
        {
            return _freespaceUsage;
        }

        /// <returns>number of bytes used for the uuid index</returns>
        public virtual long UuidUsage()
        {
            return _uuidUsage;
        }

        /// <returns>number of bytes used for the commit timestamp indexes</returns>
        public virtual long CommitTimestampUsage()
        {
            return _commitTimestampUsage;
        }

        /// <returns>total file size in bytes</returns>
        public virtual long FileSize()
        {
            return _fileSize;
        }

        /// <returns>
        ///     number of bytes used aggregated from all categories - should always be equal to
        ///     <see cref="FileSize()">FileSize()</see>
        /// </returns>
        public virtual long TotalUsage()
        {
            var total = new LongByRef(_fileHeader + _freespace + _idSystem + _classMetadata
                                      + _freespaceUsage + _uuidUsage + _commitTimestampUsage);
            Tree.Traverse(_classUsageStats, new _IVisitor4_98(total));
            return total.value;
        }

        /// <returns>the statistics for each persisted class</returns>
        public virtual IEnumerator ClassUsageStats()
        {
            return new TreeNodeIterator(_classUsageStats);
        }

        /// <param name="name">a fully qualified class name</param>
        /// <returns>the statistics for the class with the given name</returns>
        public virtual ClassUsageStats ClassStats(string name)
        {
            var found = (TreeStringObject) Tree.Find(_classUsageStats, new TreeStringObject
                (name, null));
            return found == null
                ? null
                : ((ClassUsageStats) found._value
                    );
        }

        public override string ToString()
        {
            var str = new StringBuilder();
            Tree.Traverse(_classUsageStats, new _IVisitor4_124(str));
            str.Append("\n");
            str.Append(FileUsageStatsUtil.FormatLine("File header", FileHeader()));
            str.Append(FileUsageStatsUtil.FormatLine("Freespace", Freespace()));
            str.Append(FileUsageStatsUtil.FormatLine("ID system", IdSystem()));
            str.Append(FileUsageStatsUtil.FormatLine("Class metadata", ClassMetadata()));
            str.Append(FileUsageStatsUtil.FormatLine("Freespace usage", FreespaceUsage()));
            str.Append(FileUsageStatsUtil.FormatLine("UUID usage", UuidUsage()));
            str.Append(FileUsageStatsUtil.FormatLine("Version usage", CommitTimestampUsage())
                );
            str.Append("\n");
            var totalUsage = TotalUsage();
            str.Append(FileUsageStatsUtil.FormatLine("Total", totalUsage));
            str.Append(FileUsageStatsUtil.FormatLine("Unaccounted", FileSize() - totalUsage));
            str.Append(FileUsageStatsUtil.FormatLine("File", FileSize()));
            str.Append(_slots);
            return str.ToString();
        }

        internal virtual void AddClassStats(ClassUsageStats classStats
            )
        {
            _classUsageStats = ((TreeStringObject) Tree.Add(_classUsageStats, new TreeStringObject
                (classStats.ClassName(), classStats)));
        }

        internal virtual void AddSlot(Slot slot)
        {
            _slots.Add(slot);
        }

        private sealed class _IVisitor4_98 : IVisitor4
        {
            private readonly LongByRef total;

            public _IVisitor4_98(LongByRef total)
            {
                this.total = total;
            }

            public void Visit(object node)
            {
                total.value += ((ClassUsageStats) ((TreeStringObject) node
                    )._value).TotalUsage();
            }
        }

        private sealed class _IVisitor4_124 : IVisitor4
        {
            private readonly StringBuilder str;

            public _IVisitor4_124(StringBuilder str)
            {
                this.str = str;
            }

            public void Visit(object node)
            {
                ((ClassUsageStats) ((TreeStringObject) node)._value).ToString
                    (str);
            }
        }
    }
}

#endif // !SILVERLIGHT