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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Internal;
using Db4oUnit.Fixtures;

namespace Db4oUnit.Extensions
{
    public interface IDb4oFixture : ILabeled
    {
        /// <exception cref="System.Exception"></exception>
        void Open(IDb4oTestCase testInstance);

        /// <exception cref="System.Exception"></exception>
        void Close();

        /// <exception cref="System.Exception"></exception>
        void Reopen(IDb4oTestCase testInstance);

        void Clean();
        LocalObjectContainer FileSession();
        IExtObjectContainer Db();
        IConfiguration Config();
        bool Accept(Type clazz);

        /// <exception cref="System.Exception"></exception>
        void Defragment();

        void ConfigureAtRuntime(IRuntimeConfigureAction action);
        void FixtureConfiguration(IFixtureConfiguration configuration);
        void ResetConfig();
        IList UncaughtExceptions();
    }
}