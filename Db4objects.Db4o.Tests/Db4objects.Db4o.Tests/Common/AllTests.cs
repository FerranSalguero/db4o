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
using Db4objects.Db4o.Tests.Common.TA;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common
{
    public class AllTests : ComposibleTestSuite
    {
        protected override Type[] TestCases()
        {
            return ComposeTests(new[]
            {
                typeof (Acid.AllTests
                    ),
                typeof (Activation.AllTests), typeof (Api.AllTests
                    ),
                typeof (Assorted.AllTests), typeof (Backup.AllTests
                    ),
                typeof (Btree.AllTests), typeof (Classindex.AllTests
                    ),
                typeof (Caching.AllTests), typeof (Config.AllTests
                    ),
                typeof (Constraints.AllTests), typeof (Defragment.AllTests
                    ),
                typeof (Diagnostics.AllTests), typeof (Events.AllTests
                    ),
                typeof (Exceptions.AllTests), typeof (Ext.AllTests
                    ),
                typeof (Fatalerror.AllTests), typeof (Fieldindex.AllTests
                    ),
                typeof (Filelock.AllTests), typeof (Foundation.AllTests
                    ),
                typeof (Freespace.AllTests), typeof (Handlers.AllTests
                    ),
                typeof (Header.AllTests), typeof (Interfaces.AllTests
                    ),
                typeof (Internal.AllTests), typeof (Ids.AllTests
                    ),
                typeof (IO.AllTests), typeof (Querying.AllTests
                    ),
                typeof (Refactor.AllTests), typeof (References.AllTests
                    ),
                typeof (Reflect.AllTests), typeof (Regression.AllTests
                    ),
                typeof (Sessions.AllTests), typeof (Store.AllTests
                    ),
                typeof (Soda.AllTests), typeof (Stored.AllTests
                    ),
                typeof (AllCommonTATests), typeof (TP.AllTests), typeof (
                    Types.AllTests),
                typeof (Updatedepth.AllTests
                    ),
                typeof (Uuid.AllTests), typeof (Optional.AllTests
                    ),
                typeof (Tests.Util.Test.AllTests)
            });
        }

#if !SILVERLIGHT
        protected override Type[] ComposeWith()
        {
            return new[]
            {
                typeof (CS.AllTests), typeof (Qlin.AllTests
                    )
            };
        }
#endif // !SILVERLIGHT
    }
}