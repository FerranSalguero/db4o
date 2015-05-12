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
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Sharpen;
using Sharpen.IO;

namespace Db4objects.Db4o.Tools
{
    /// <summary>prints statistics about a database file to System.out.</summary>
    /// <remarks>
    ///     prints statistics about a database file to System.out.
    ///     <br /><br />Pass the database file path as an argument.
    ///     <br /><br /><b>This class is not part of db4o.jar!</b><br />
    ///     It is delivered as sourcecode in the
    ///     path ../com/db4o/tools/<br /><br />
    /// </remarks>
    public class Statistics
    {
        private static readonly string Remove = "XXxxREMOVExxXX";

        /// <summary>the main method that runs the statistics.</summary>
        /// <remarks>the main method that runs the statistics.</remarks>
        /// <param name="args">
        ///     a String array of length 1, with the name of the database
        ///     file as element 0.
        /// </param>
        public static void Main(string[] args)
        {
            if (args == null || args.Length != 1)
            {
                Runtime.Out.WriteLine("Usage: java com.db4o.tools.Statistics <database filename>"
                    );
            }
            else
            {
                new Statistics().Run(args[0]);
            }
        }

        public virtual void Run(string filename)
        {
            if (new File(filename).Exists())
            {
                IObjectContainer con = null;
                try
                {
                    var config = Db4oFactory.NewConfiguration();
                    config.MessageLevel(-1);
                    con = Db4oFactory.OpenFile(config, filename);
                    PrintHeader("STATISTICS");
                    Runtime.Out.WriteLine("File: " + filename);
                    PrintStats(con, filename);
                    con.Close();
                }
                catch (Exception e)
                {
                    Runtime.Out.WriteLine("Statistics failed for file: '" + filename + "'");
                    Runtime.Out.WriteLine(e.Message);
                    Runtime.PrintStackTrace(e);
                }
            }
            else
            {
                Runtime.Out.WriteLine("File not found: '" + filename + "'");
            }
        }

        private static bool CanCallConstructor(string className)
        {
            return ReflectPlatform.CreateInstance(className) != null;
        }

        private void PrintStats(IObjectContainer con, string filename)
        {
            Tree unavailable = new TreeString(Remove);
            Tree noConstructor = new TreeString(Remove);
            // one element too many, substract one in the end
            var internalClasses = con.Ext().StoredClasses();
            for (var i = 0; i < internalClasses.Length; i++)
            {
                var internalClassName = internalClasses[i].GetName();
                var clazz = ReflectPlatform.ForName(internalClassName);
                if (clazz == null)
                {
                    unavailable = unavailable.Add(new TreeString(internalClassName));
                }
                else
                {
                    if (!CanCallConstructor(internalClassName))
                    {
                        noConstructor = noConstructor.Add(new TreeString(internalClassName));
                    }
                }
            }
            unavailable = unavailable.RemoveLike(new TreeString(Remove));
            noConstructor = noConstructor.RemoveLike(new TreeString(Remove));
            if (unavailable != null)
            {
                PrintHeader("UNAVAILABLE");
                unavailable.Traverse(new _IVisitor4_80());
            }
            if (noConstructor != null)
            {
                PrintHeader("NO PUBLIC CONSTRUCTOR");
                noConstructor.Traverse(new _IVisitor4_88());
            }
            PrintHeader("CLASSES");
            Runtime.Out.WriteLine("Number of objects per class:");
            var ids = ByRef.NewInstance(new TreeInt(0));
            if (internalClasses.Length > 0)
            {
                Tree all = new TreeStringObject(internalClasses[0].GetName(), internalClasses[0]);
                for (var i = 1; i < internalClasses.Length; i++)
                {
                    all = all.Add(new TreeStringObject(internalClasses[i].GetName(), internalClasses[
                        i]));
                }
                all.Traverse(new _IVisitor4_107(ids));
            }
            PrintHeader("SUMMARY");
            Runtime.Out.WriteLine("File: " + filename);
            Runtime.Out.WriteLine("Stored classes: " + internalClasses.Length);
            if (unavailable != null)
            {
                Runtime.Out.WriteLine("Unavailable classes: " + unavailable.Size());
            }
            if (noConstructor != null)
            {
                Runtime.Out.WriteLine("Classes without public constructors: " + noConstructor
                    .Size());
            }
            Runtime.Out.WriteLine("Total number of objects: " + (((Tree) ids.value).Size
                () - 1));
        }

        private void PrintHeader(string str)
        {
            var stars = (39 - str.Length)/2;
            Runtime.Out.WriteLine("\n");
            for (var i = 0; i < stars; i++)
            {
                Runtime.Out.Write("*");
            }
            Runtime.Out.Write(" " + str + " ");
            for (var i = 0; i < stars; i++)
            {
                Runtime.Out.Write("*");
            }
            Runtime.Out.WriteLine();
        }

        private sealed class _IVisitor4_80 : IVisitor4
        {
            public void Visit(object obj)
            {
                Runtime.Out.WriteLine(((TreeString) obj)._key);
            }
        }

        private sealed class _IVisitor4_88 : IVisitor4
        {
            public void Visit(object obj)
            {
                Runtime.Out.WriteLine(((TreeString) obj)._key);
            }
        }

        private sealed class _IVisitor4_107 : IVisitor4
        {
            private readonly ByRef ids;

            public _IVisitor4_107(ByRef ids)
            {
                this.ids = ids;
            }

            public void Visit(object obj)
            {
                var node = (TreeStringObject) obj;
                var newIDs = ((IStoredClass) node._value).GetIDs();
                for (var j = 0; j < newIDs.Length; j++)
                {
                    if (((Tree) ids.value).Find(new TreeInt((int) newIDs[j])) == null)
                    {
                        ids.value = ((TreeInt) ((Tree) ids.value).Add(new TreeInt((int) newIDs[j])));
                    }
                }
                Runtime.Out.WriteLine(node._key + ": " + newIDs.Length);
            }
        }
    }
}