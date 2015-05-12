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
using System.Collections.Generic;
using Db4oTool.Core;
using Mono.GetOptions;

namespace Db4oTool
{
    public delegate ITypeFilter TypeFilterFactory();

    public class ProgramOptions : Options
    {
        public readonly List<TypeFilterFactory> Filters = new List<TypeFilterFactory>();
        private bool _prettyVerbose;

        [Option("Case sensitive queries", "case-sensitive")] public bool CaseSensitive;

        [Option("Run consistency checks on target database.", "check")] public bool CheckDatabase;

        [Option("Instrument native collections for transparent activation/persistence", "collections")] public bool
            Collections;

        public List<string> CustomInstrumentations = new List<string>();

        [Option("Preserve debugging information", "debug")] public bool Debug;

        [Option("Fake operation mode, assembly won't be written", "fake")] public bool Fake;

        [Option("Installs the db4o performance counter category", "install-performance-counters")] public bool
            InstallPerformanceCounters;

        [Option("Optimize all native queries", "nq")] public bool NQ;

        [Option("Displays file usage statistics for target database.", "fileusage")] public bool ShowFileUsageStats;

        public List<string> StatisticsFileNames = new List<string>();

        [Option("Implement Transparent Persistence Support", "tp")] public bool TransparentPersistence;

        [Option("Verbose operation mode", 'v', "verbose")] public bool Verbose;

        public ProgramOptions()
        {
            DontSplitOnCommas = true;
        }

        [Option("Pretty verbose operation mode", "vv")]
        public bool PrettyVerbose
        {
            get { return _prettyVerbose; }

            set { Verbose = _prettyVerbose = value; }
        }

        public string Target
        {
            get
            {
                if (RemainingArguments == null) return null;
                if (RemainingArguments.Length != 1) return null;
                return RemainingArguments[0];
            }

            set { RemainingArguments = new[] {value}; }
        }

        public bool IsValid
        {
            get
            {
                var databaseTarget = CheckDatabase || ShowFileUsageStats;
                var enhancementTarget = NQ || TransparentPersistence || CustomInstrumentations.Count > 0;

                if (databaseTarget && enhancementTarget)
                {
                    return false;
                }

                return StatisticsFileNames.Count > 0 || InstallPerformanceCounters ||
                       (Target != null
                        && (databaseTarget || enhancementTarget));
            }
        }

        [Option("Print statistics for database file PARAM", "statistics")]
        public WhatToDoNext StatisticsFileName(string fileName)
        {
            StatisticsFileNames.Add(fileName);
            return WhatToDoNext.GoAhead;
        }

        [Option("Custom instrumentation type", "instrumentation", MaxOccurs = -1)]
        public WhatToDoNext CustomInstrumentation(string instrumentation)
        {
            CustomInstrumentations.Add(instrumentation);
            return WhatToDoNext.GoAhead;
        }

        [Option("Filter types to be instrumented by attribute", "by-attribute", MaxOccurs = -1)]
        public WhatToDoNext ByAttribute(string attribute)
        {
            Filters.Add(delegate { return new ByAttributeFilter(attribute); });
            return WhatToDoNext.GoAhead;
        }

        [Option("Custom type filter", "by-filter", MaxOccurs = -1)]
        public WhatToDoNext ByFilter(string filterType)
        {
            Filters.Add(delegate { return Factory.Instantiate<ITypeFilter>(filterType); });
            return WhatToDoNext.GoAhead;
        }

        [Option("Filter types by name (with regular expression syntax)", "by-name", MaxOccurs = -1)]
        public WhatToDoNext ByName(string name)
        {
            Filters.Add(delegate { return new ByNameFilter(name); });
            return WhatToDoNext.GoAhead;
        }

        [Option("Negates the last filter.", "not", MaxOccurs = -1)]
        public WhatToDoNext Not()
        {
            if (Filters.Count == 0) throw new InvalidOperationException("'not' must be specified after a filter");

            var lastIndex = Filters.Count - 1;
            var lastFilter = Filters[lastIndex];
            Filters[lastIndex] = delegate { return new NotFilter(lastFilter()); };
            return WhatToDoNext.GoAhead;
        }

        [Option("Same as 'tp'", "ta")]
        public WhatToDoNext TA()
        {
            TransparentPersistence = true;
            return WhatToDoNext.GoAhead;
        }
    }
}