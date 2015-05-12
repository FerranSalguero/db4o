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

using System.Collections;
using Sharpen.IO;

namespace Db4objects.Db4o.Tests.Common.Migration
{
    public class Db4oLibraryEnvironmentProvider
    {
        private readonly File _classPath;
        private readonly IDictionary _environments = new Hashtable();

        public Db4oLibraryEnvironmentProvider(File classPath)
        {
            _classPath = classPath;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual Db4oLibraryEnvironment EnvironmentFor(string path)
        {
            var existing = ExistingEnvironment(path);
            if (existing != null)
            {
                return existing;
            }
            return NewEnvironment(path);
        }

        private Db4oLibraryEnvironment ExistingEnvironment(string path)
        {
            return ((Db4oLibraryEnvironment) _environments[path]);
        }

        /// <exception cref="System.IO.IOException"></exception>
        private Db4oLibraryEnvironment NewEnvironment(string path)
        {
            var env = new Db4oLibraryEnvironment(new File(path)
                , _classPath);
            _environments[path] = env;
            return env;
        }

        public virtual void DisposeAll()
        {
            for (var eIter = _environments.Values.GetEnumerator(); eIter.MoveNext();)
            {
                var e = ((Db4oLibraryEnvironment) eIter.Current);
                e.Dispose();
            }
            _environments.Clear();
        }
    }
}