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
using System.IO;
using System.Reflection;

namespace Db4oTool.Tests.Core
{
    public delegate void Action();

    public class ShellUtilities
    {
        public static string WithStdout(Action code)
        {
            var writer = new StringWriter();
            var old = Console.Out;
            try
            {
                Console.SetOut(writer);
                code();
                return writer.ToString().Trim();
            }
            finally
            {
                Console.SetOut(old);
            }
        }

        public static string CopyFileToFolder(string fname, string path)
        {
            var targetFileName = Path.Combine(path, Path.GetFileName(fname));
            Directory.CreateDirectory(path);
            File.Copy(fname, targetFileName, true);
            return targetFileName;
        }

        public static ProcessOutput shellm(string fname, params string[] args)
        {
            var stdout = new StringWriter();
            var stderr = new StringWriter();
            var saved = Console.Out;
            var savedErr = Console.Error;
            try
            {
                Console.SetOut(stdout);
                Console.SetError(stderr);
                Assembly.LoadFrom(fname).EntryPoint.Invoke(null, new object[] {args});
                return new ProcessOutput(0, stdout.ToString(), stderr.ToString());
            }
            finally
            {
                Console.SetOut(saved);
                Console.SetError(savedErr);
            }
        }

        public static ProcessOutput shell(string fname, params string[] args)
        {
            var p = StartProcess(fname, args);

            var stdError = new StringWriter();
            p.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e) { stdError.Write(e.Data); };

            var stdOut = new StringWriter();
            p.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e) { stdOut.Write(e.Data); };

            p.Start();

            p.BeginErrorReadLine();
            p.BeginOutputReadLine();

            p.WaitForExit();
            return new ProcessOutput(p.ExitCode, stdOut.ToString(), stdError.ToString());
        }

        private static Process StartProcess(string filename, params string[] args)
        {
            var p = new Process();
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.FileName = filename;
            p.StartInfo.Arguments = string.Join(" ", quote(args));
            return p;
        }

        private static string[] quote(string[] args)
        {
            for (var i = 0; i < args.Length; ++i)
            {
                args[i] = string.Format("\"{0}\"", args[i]);
            }
            return args;
        }

        public static void CopyParentAssemblyToTemp(Type type)
        {
            CopyAssemblyToTemp(type.Assembly);
        }

        public static void CopyAssemblyToTemp(Assembly assembly)
        {
            CopyToTemp(assembly.ManifestModule.FullyQualifiedName);
        }

        public static string CopyToTemp(string fname)
        {
            return CopyFileToFolder(fname, GetTempPath());
        }

        public static string GetTempPath()
        {
            //			return Path.GetTempPath();

            // for now, debugging information is only
            // preserved when the directory name does not contain
            // UTF character because of some bug, so
            // let's keep it simple
            var tempPath = Path.Combine(
                Directory.GetDirectoryRoot(Directory.GetCurrentDirectory()),
                "tmp");
            Directory.CreateDirectory(tempPath);
            return tempPath;
        }

        public static void DeleteFile(string fname)
        {
            if (File.Exists(fname)) File.Delete(fname);
        }

        public class ProcessOutput
        {
            public int ExitCode;
            public string StdErr;
            public string StdOut;

            public ProcessOutput()
            {
            }

            public ProcessOutput(int exitCode, string stdout, string stderr)
            {
                ExitCode = exitCode;
                StdOut = stdout;
                StdErr = stderr;
            }

            public override string ToString()
            {
                return StdOut + StdErr;
            }
        }
    }
}