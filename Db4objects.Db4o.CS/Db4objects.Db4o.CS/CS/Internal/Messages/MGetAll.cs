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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.CS.Internal.Objectexchange;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal.Query.Processor;
using Db4objects.Db4o.Internal.Query.Result;

namespace Db4objects.Db4o.CS.Internal.Messages
{
    public sealed class MGetAll : MsgQuery, IMessageWithResponse
    {
        public Msg ReplyFromServer()
        {
            var evaluationMode = QueryEvaluationMode.FromInt(ReadInt());
            var prefetchDepth = ReadInt();
            var prefetchCount = ReadInt();
            lock (ContainerLock())
            {
                return WriteQueryResult(GetAll(evaluationMode), evaluationMode, new ObjectExchangeConfiguration
                    (prefetchDepth, prefetchCount));
            }
        }

        private AbstractQueryResult GetAll(QueryEvaluationMode mode)
        {
            return ((AbstractQueryResult) NewQuery(mode).TriggeringQueryEvents(new _IClosure4_24
                (this, mode)));
        }

        private QQuery NewQuery(QueryEvaluationMode mode)
        {
            var query = (QQuery) LocalContainer().Query();
            query.EvaluationMode(mode);
            return query;
        }

        private sealed class _IClosure4_24 : IClosure4
        {
            private readonly MGetAll _enclosing;
            private readonly QueryEvaluationMode mode;

            public _IClosure4_24(MGetAll _enclosing, QueryEvaluationMode mode)
            {
                this._enclosing = _enclosing;
                this.mode = mode;
            }

            public object Run()
            {
                try
                {
                    return _enclosing.LocalContainer().GetAll(_enclosing.Transaction(), mode
                        );
                }
                catch (Exception e)
                {
                }
                return _enclosing.NewQueryResult(mode);
            }
        }
    }
}