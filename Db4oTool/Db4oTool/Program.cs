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
using System.Diagnostics;
using Db4objects.Db4o.Consistency;
using Db4objects.Db4o.Filestats;
using Db4objects.Db4o.Monitoring;
using Db4objects.Db4o.Tools;
using Db4oTool.Core;
using Db4oTool.NQ;
using Db4oTool.TA;

namespace Db4oTool
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Error));

            var options = new ProgramOptions();
            try
            {
                options.ProcessArgs(args);
                if (!options.IsValid)
                {
                    options.DoHelp();
                    return -1;
                }

                Run(options);
            }
            catch (Exception x)
            {
                ReportError(options, x);
                return -2;
            }
            return 0;
        }

        public static void Run(ProgramOptions options)
        {
            foreach (var fileName in options.StatisticsFileNames)
            {
                new Statistics().Run(fileName);
            }

            if (options.InstallPerformanceCounters)
            {
                Db4oPerformanceCounters.ReInstall();
            }

            if (options.CheckDatabase)
            {
                Console.Write("\r\nChecking '{0}' : ", options.Target);
                ConsistencyChecker.Main(new[] {options.Target});
            }

            if (options.ShowFileUsageStats)
            {
                FileUsageStatsCollector.Main(new[] {options.Target, "true"});
            }

            if (NoInstrumentationStep(options))
            {
                return;
            }

            using (new CurrentDirectoryAssemblyResolver())
            {
                RunPipeline(options);
            }
        }

        private static bool NoInstrumentationStep(ProgramOptions options)
        {
            return !options.NQ && !options.TransparentPersistence && options.CustomInstrumentations.Count == 0;
        }

        private static void RunPipeline(ProgramOptions options)
        {
            var pipeline = new InstrumentationPipeline(GetConfiguration(options));
            if (options.NQ)
            {
                pipeline.Add(new DelegateOptimizer());
                pipeline.Add(new PredicateOptimizer());
            }

            if (options.TransparentPersistence)
            {
                pipeline.Add(new TAInstrumentation(options.Collections));
            }

            foreach (var instr in Factory.Instantiate<IAssemblyInstrumentation>(options.CustomInstrumentations))
            {
                pipeline.Add(instr);
            }

            if (!options.Fake)
            {
                pipeline.Add(new SaveAssemblyInstrumentation());
                if (pipeline.Context.IsAssemblySigned())
                {
                    pipeline.Context.TraceWarning(
                        "Warning: Assembly {0} has been signed; once instrumented it will fail strong name validation (you will need to sign it again).",
                        pipeline.Context.Assembly.Name.Name);
                }
            }
            pipeline.Run();
        }

        private static void ReportError(ProgramOptions options, Exception x)
        {
            if (options.Verbose)
            {
                Console.WriteLine(x);
            }
            else
            {
                Console.WriteLine(x.Message);
            }
        }

        private static Configuration GetConfiguration(ProgramOptions options)
        {
            var configuration = new Configuration(options.Target);
            configuration.CaseSensitive = options.CaseSensitive;
            configuration.PreserveDebugInfo = options.Debug;
            if (options.Verbose)
            {
                configuration.TraceSwitch.Level = options.PrettyVerbose ? TraceLevel.Verbose : TraceLevel.Info;
            }
            foreach (var factory in options.Filters)
            {
                configuration.AddFilter(factory());
            }
            return configuration;
        }
    }
}