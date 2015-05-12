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
using Db4objects.Db4o.Events;
using Db4objects.Db4o.Foundation;
using Db4oUnit.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Events
{
    public class ExceptionPropagationInEventsTestVariables
    {
        internal static readonly FixtureVariable EventSelector = new FixtureVariable("event"
            );

        internal static readonly IFixtureProvider EventProvider = new SimpleFixtureProvider
            (EventSelector, new object[]
            {
                new EventInfo("query", new _IProcedure4_14()), new
                    EventInfo("query", new _IProcedure4_23()),
                new EventInfo("delete", false, new _IProcedure4_32
                    ()),
                new EventInfo("delete", false, new _IProcedure4_42()), new EventInfo("insert"
                    , false, new _IProcedure4_51()),
                new EventInfo("insert", false, new _IProcedure4_65
                    ()),
                new EventInfo("insert", new _IProcedure4_79()), new EventInfo("insert", new
                    _IProcedure4_89()),
                new EventInfo("query", new _IProcedure4_98()), new EventInfo
                    ("update", new _IProcedure4_107()),
                new EventInfo("update", new _IProcedure4_116
                    ()),
                new EventInfo("query", new _IProcedure4_126()), new EventInfo("query", new
                    _IProcedure4_135())
            });

        private sealed class _IProcedure4_14 : IProcedure4
        {
            // 0
            public void Apply(object eventRegistry)
            {
                ((IEventRegistry) eventRegistry).Activated += new _IEventListener4_16().OnEvent;
            }

            private sealed class _IEventListener4_16
            {
                public void OnEvent(object sender, ObjectInfoEventArgs args
                    )
                {
                    throw new NotImplementedException();
                }
            }
        }

        private sealed class _IProcedure4_23 : IProcedure4
        {
            // 1
            public void Apply(object eventRegistry)
            {
                ((IEventRegistry) eventRegistry).Activating += new _IEventListener4_25().OnEvent;
            }

            private sealed class _IEventListener4_25
            {
                public void OnEvent(object sender, CancellableObjectEventArgs
                    args)
                {
                    throw new NotImplementedException();
                }
            }
        }

        private sealed class _IProcedure4_32 : IProcedure4
        {
            // 2
            public void Apply(object eventRegistry)
            {
                ((IEventRegistry) eventRegistry).Deleted += new _IEventListener4_34().OnEvent;
            }

            private sealed class _IEventListener4_34
            {
                public void OnEvent(object sender, ObjectInfoEventArgs args
                    )
                {
                    throw new NotImplementedException();
                }
            }
        }

        private sealed class _IProcedure4_42 : IProcedure4
        {
            // 3
            public void Apply(object eventRegistry)
            {
                ((IEventRegistry) eventRegistry).Deleting += new _IEventListener4_44().OnEvent;
            }

            private sealed class _IEventListener4_44
            {
                public void OnEvent(object sender, CancellableObjectEventArgs
                    args)
                {
                    throw new NotImplementedException();
                }
            }
        }

        private sealed class _IProcedure4_51 : IProcedure4
        {
            // 4
            public void Apply(object eventRegistry)
            {
                ((IEventRegistry) eventRegistry).Committing += new _IEventListener4_53().OnEvent;
            }

            private sealed class _IEventListener4_53
            {
                private bool _firstTime;

                public _IEventListener4_53()
                {
                    _firstTime = true;
                }

                public void OnEvent(object sender, CommitEventArgs args)
                {
                    if (_firstTime)
                    {
                        _firstTime = false;
                        throw new NotImplementedException();
                    }
                }
            }
        }

        private sealed class _IProcedure4_65 : IProcedure4
        {
            // 5
            public void Apply(object eventRegistry)
            {
                ((IEventRegistry) eventRegistry).Committed += new _IEventListener4_67().OnEvent;
            }

            private sealed class _IEventListener4_67
            {
                private bool _firstTime;

                public _IEventListener4_67()
                {
                    _firstTime = true;
                }

                public void OnEvent(object sender, CommitEventArgs args)
                {
                    if (_firstTime)
                    {
                        _firstTime = false;
                        throw new NotImplementedException();
                    }
                }
            }
        }

        private sealed class _IProcedure4_79 : IProcedure4
        {
            // 6
            public void Apply(object eventRegistry)
            {
                ((IEventRegistry) eventRegistry).Creating += new _IEventListener4_81().OnEvent;
            }

            private sealed class _IEventListener4_81
            {
                public void OnEvent(object sender, CancellableObjectEventArgs
                    args)
                {
                    throw new NotImplementedException();
                }
            }
        }

        private sealed class _IProcedure4_89 : IProcedure4
        {
            // 7
            public void Apply(object eventRegistry)
            {
                ((IEventRegistry) eventRegistry).Created += new _IEventListener4_91().OnEvent;
            }

            private sealed class _IEventListener4_91
            {
                public void OnEvent(object sender, ObjectInfoEventArgs args
                    )
                {
                    throw new NotImplementedException();
                }
            }
        }

        private sealed class _IProcedure4_98 : IProcedure4
        {
            // 8
            public void Apply(object eventRegistry)
            {
                ((IEventRegistry) eventRegistry).Instantiated += new _IEventListener4_100().OnEvent;
            }

            private sealed class _IEventListener4_100
            {
                public void OnEvent(object sender, ObjectInfoEventArgs args
                    )
                {
                    throw new NotImplementedException();
                }
            }
        }

        private sealed class _IProcedure4_107 : IProcedure4
        {
            // 9
            public void Apply(object eventRegistry)
            {
                ((IEventRegistry) eventRegistry).Updating += new _IEventListener4_109().OnEvent;
            }

            private sealed class _IEventListener4_109
            {
                public void OnEvent(object sender, CancellableObjectEventArgs
                    args)
                {
                    throw new NotImplementedException();
                }
            }
        }

        private sealed class _IProcedure4_116 : IProcedure4
        {
            // 10
            public void Apply(object eventRegistry)
            {
                ((IEventRegistry) eventRegistry).Updated += new _IEventListener4_118().OnEvent;
            }

            private sealed class _IEventListener4_118
            {
                public void OnEvent(object sender, ObjectInfoEventArgs args
                    )
                {
                    throw new NotImplementedException();
                }
            }
        }

        private sealed class _IProcedure4_126 : IProcedure4
        {
            // 11
            public void Apply(object eventRegistry)
            {
                ((IEventRegistry) eventRegistry).QueryStarted += new _IEventListener4_128().OnEvent;
            }

            private sealed class _IEventListener4_128
            {
                public void OnEvent(object sender, QueryEventArgs args)
                {
                    throw new NotImplementedException();
                }
            }
        }

        private sealed class _IProcedure4_135 : IProcedure4
        {
            // 12
            public void Apply(object eventRegistry)
            {
                ((IEventRegistry) eventRegistry).QueryFinished += new _IEventListener4_137().OnEvent;
            }

            private sealed class _IEventListener4_137
            {
                public void OnEvent(object sender, QueryEventArgs args)
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}