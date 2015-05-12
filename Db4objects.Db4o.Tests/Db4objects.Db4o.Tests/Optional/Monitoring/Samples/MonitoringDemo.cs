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

#if !CF && !SILVERLIGHT
using Db4objects.Db4o.Config;
using Db4objects.Db4o.CS;
using Db4objects.Db4o.CS.Config;
using Db4objects.Db4o.Foundation.IO;
using Db4objects.Db4o.Query;
using Sharpen;

namespace Db4objects.Db4o.Tests.Optional.Monitoring.Samples
{
    public class MonitoringDemo
    {
        private const bool ClientServer = true;
        private const int PermanentObjectCount = 10000;
        private const int TemporaryObjectCount = 1000;
        private const int QueryCount = 10;
        private static readonly string DatabaseFileName = "mydb.db4o";
        private IObjectServer _server;

        public static void Main(string[] args)
        {
            new MonitoringDemo().Run();
        }

        public virtual void Run()
        {
            Runtime.Out.WriteLine("MonitoringDemo will run forever to allow you to see JMX/Perfmon statistics."
                );
            Runtime.Out.WriteLine("Cancel running with CTRL + C");
            File4.Delete(DatabaseFileName);
            var objectContainer = OpenContainer();
            StorePermanentObjects(objectContainer);
            try
            {
                while (true)
                {
                    StoreTemporaryObjects(objectContainer);
                    ExecuteQueries(objectContainer);
                    DeleteTemporaryObjects(objectContainer);
                }
            }
            finally
            {
                Close(objectContainer);
            }
        }

        private void Close(IObjectContainer objectContainer)
        {
            objectContainer.Close();
            if (_server != null)
            {
                _server.Close();
                _server = null;
            }
        }

        private IObjectContainer OpenContainer()
        {
            var user = "db4o";
            var password = "db4o";
            _server = Db4oClientServer.OpenServer(((IServerConfiguration) Configure(Db4oClientServer
                .NewServerConfiguration(), "db4o server(" + DatabaseFileName + ")")), DatabaseFileName
                , Db4oClientServer.ArbitraryPort);
            _server.GrantAccess(user, password);
            return Db4oClientServer.OpenClient(((IClientConfiguration) Configure(Db4oClientServer
                .NewClientConfiguration(), "db4o client(localhost:" + _server.Ext().Port() + ")"
                )), "localhost", _server.Ext().Port(), user, password);
            return Db4oEmbedded.OpenFile(((IEmbeddedConfiguration) Configure(Db4oEmbedded.NewConfiguration
                (), "db4o(" + DatabaseFileName + ")")), DatabaseFileName);
        }

        private void ExecuteQueries(IObjectContainer objectContainer)
        {
            for (var i = 0; i < QueryCount; i++)
            {
                ExecuteSodaQuery(objectContainer);
                ExecuteOptimizedNativeQuery(objectContainer);
                ExecuteUnOptimizedNativeQuery(objectContainer);
            }
        }

        private void ExecuteSodaQuery(IObjectContainer objectContainer)
        {
            var query = objectContainer.Query();
            query.Constrain(typeof (Item));
            query.Descend("name").Constrain("1");
            query.Execute();
        }

        private void ExecuteOptimizedNativeQuery(IObjectContainer objectContainer)
        {
            objectContainer.Query(new _Predicate_98());
        }

        private void ExecuteUnOptimizedNativeQuery(IObjectContainer objectContainer)
        {
            objectContainer.Query(new _Predicate_106());
        }

        private void DeleteTemporaryObjects(IObjectContainer objectContainer)
        {
            var query = objectContainer.Query();
            query.Constrain(typeof (Item));
            query.Descend("name").Constrain("temp");
            var objectSet = query.Execute();
            while (objectSet.HasNext())
            {
                objectContainer.Delete(((Item) objectSet.Next()));
            }
            objectContainer.Commit();
        }

        private void StoreTemporaryObjects(IObjectContainer objectContainer)
        {
            for (var i = 0; i < TemporaryObjectCount; i++)
            {
                objectContainer.Store(new Item("temp"));
            }
            objectContainer.Commit();
        }

        private void StorePermanentObjects(IObjectContainer objectContainer)
        {
            for (var i = 0; i < PermanentObjectCount; i++)
            {
                objectContainer.Store(new Item(string.Empty + i));
            }
            objectContainer.Commit();
        }

        private ICommonConfigurationProvider Configure(ICommonConfigurationProvider config
            , string name)
        {
            config.Common.ObjectClass(typeof (Item
                )).ObjectField("name").Indexed(true);
            config.Common.NameProvider(new SimpleNameProvider
                (name));
            new AllMonitoringSupport().Apply(config);
            return config;
        }

        public class Item
        {
            public string name;

            public Item(string name)
            {
                this.name = name;
            }
        }

        private sealed class _Predicate_98 : Predicate
        {
            public bool Match(Item candidate)
            {
                return candidate.name.Equals("name1");
            }
        }

        private sealed class _Predicate_106 : Predicate
        {
            public bool Match(Item candidate)
            {
                return candidate.name[0] == 'q';
            }
        }
    }
}

#endif // !CF && !SILVERLIGHT