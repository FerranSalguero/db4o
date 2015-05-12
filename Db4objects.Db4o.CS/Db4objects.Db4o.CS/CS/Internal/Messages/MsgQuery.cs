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

using Db4objects.Db4o.Config;
using Db4objects.Db4o.CS.Internal.Objectexchange;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Query.Result;

namespace Db4objects.Db4o.CS.Internal.Messages
{
    public abstract class MsgQuery : MsgObject
    {
        private static int nextID;

        protected MsgD WriteQueryResult(AbstractQueryResult queryResult, QueryEvaluationMode
            evaluationMode, ObjectExchangeConfiguration config)
        {
            if (evaluationMode == QueryEvaluationMode.Immediate)
            {
                return WriteImmediateQueryResult(queryResult, config);
            }
            return WriteLazyQueryResult(queryResult, config);
        }

        private MsgD WriteLazyQueryResult(AbstractQueryResult queryResult, ObjectExchangeConfiguration
            config)
        {
            var queryResultId = GenerateID();
            var maxCount = Config().PrefetchObjectCount();
            var idIterator = queryResult.IterateIDs();
            var message = BuildQueryResultMessage(queryResultId, idIterator, maxCount, config
                );
            var serverThread = ServerMessageDispatcher();
            serverThread.MapQueryResultToID(new LazyClientObjectSetStub(queryResult, idIterator
                ), queryResultId);
            return message;
        }

        private MsgD WriteImmediateQueryResult(AbstractQueryResult queryResult, ObjectExchangeConfiguration
            config)
        {
            var idIterator = queryResult.IterateIDs();
            var message = BuildQueryResultMessage(0, idIterator, queryResult.Size(), config);
            return message;
        }

        private MsgD BuildQueryResultMessage(int queryResultId, IIntIterator4 ids, int maxCount
            , ObjectExchangeConfiguration config)
        {
            var payload = ObjectExchangeStrategyFactory.ForConfig(config).Marshall
                ((LocalTransaction) Transaction(), ids, maxCount);
            var message = QueryResult.GetWriterForLength(Transaction(), Const4.IntLength + payload
                .Length());
            var writer = message.PayLoad();
            writer.WriteInt(queryResultId);
            writer.WriteBytes(payload._buffer);
            return message;
        }

        private static int GenerateID()
        {
            lock (typeof (MsgQuery))
            {
                nextID++;
                if (nextID < 0)
                {
                    nextID = 1;
                }
                return nextID;
            }
        }

        protected virtual AbstractQueryResult NewQueryResult(QueryEvaluationMode mode)
        {
            return Container().NewQueryResult(Transaction(), mode);
        }
    }
}