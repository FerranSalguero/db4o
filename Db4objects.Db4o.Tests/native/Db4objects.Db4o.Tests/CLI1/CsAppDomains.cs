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
using System.Collections;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.CLI1
{
#if !CF && !SILVERLIGHT
    public class ADTask : MarshalByRefObject
    {
        public ADTask(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }

    internal class TaskDatabase : MarshalByRefDatabase, IOptOutMultiSession
    {
        public string[] QueryTaskNames()
        {
            var names = new ArrayList();
            var os = InternalQueryTasks();
            while (os.HasNext())
            {
                names.Add(((ADTask) os.Next()).Name);
            }
            return (string[]) names.ToArray(typeof (string));
        }

        public ADTask[] QueryTasks()
        {
            var tasks = new ArrayList();
            var os = InternalQueryTasks();
            while (os.HasNext())
            {
                tasks.Add(os.Next());
            }
            return (ADTask[]) tasks.ToArray(typeof (ADTask));
        }

        private IObjectSet InternalQueryTasks()
        {
            var query = _container.Query();
            query.Constrain(typeof (ADTask));
            query.Descend("_name").OrderAscending();
            return query.Execute();
        }
    }

    /// <summary>
    ///     Tests the interaction of db4o with multiple AppDomains
    /// </summary>
    public class CsAppDomains : AbstractDb4oTestCase, IOptOutMultiSession, IOptOutInMemory
    {
        // keep task objects alive to check for any identity problems
        private readonly ArrayList _tasks = new ArrayList();

        protected override void Store()
        {
            ADTask task = null;
            Store(task = new ADTask("task 1"));
            _tasks.Add(task);

            Store(task = new ADTask("task 2"));
            _tasks.Add(task);
        }

        public void TestRemoteDomain()
        {
            Fixture().Close();

            var domain = AppDomain.CreateDomain("db4o-remote-domain");
            try
            {
                using (
                    var db =
                        (TaskDatabase)
                            domain.CreateInstanceAndUnwrap(typeof (TaskDatabase).Assembly.GetName().ToString(),
                                typeof (TaskDatabase).FullName))
                {
                    db.Open(CurrentFileName(), false);

                    var taskNames = db.QueryTaskNames();
                    Assert.AreEqual(2, taskNames.Length);
                    Assert.AreEqual("task 1", taskNames[0]);
                    Assert.AreEqual("task 2", taskNames[1]);

                    var tasks = db.QueryTasks();
                    Assert.AreEqual(2, tasks.Length);
                    Assert.AreEqual("task 1", tasks[0].Name);
                    Assert.AreEqual("task 2", tasks[1].Name);
                }
            }
            finally
            {
                AppDomain.Unload(domain);
                Fixture().Open(this);
            }
        }

        public string CurrentFileName()
        {
            return ((Db4oSolo) Fixture()).GetAbsolutePath();
        }
    }
#endif
}