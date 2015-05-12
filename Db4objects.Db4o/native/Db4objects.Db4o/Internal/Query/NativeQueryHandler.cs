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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Db4objects.Db4o.Internal.Query.Processor;
using Db4objects.Db4o.Query;

namespace Db4objects.Db4o.Internal.Query
{
    public class NativeQueryHandler
    {
        private readonly IObjectContainer _container;
        private INQOptimizer _builder;

        public NativeQueryHandler(IObjectContainer container)
        {
            _container = container;
        }

        public event QueryExecutionHandler QueryExecution;
        public event QueryOptimizationFailureHandler QueryOptimizationFailure;

        public virtual IObjectSet Execute(IQuery query, Predicate predicate, IQueryComparator comparator)
        {
            var q = ConfigureQuery(query, predicate);
            q.SortBy(comparator);
            return q.Execute();
        }

        public virtual IList<Extent> Execute<Extent>(IQuery query, Predicate<Extent> match,
            IQueryComparator comparator)
        {
#if CF
			return ExecuteUnoptimized<Extent>(QueryForExtent<Extent>(query, comparator), match);
#else
            // XXX: check GetDelegateList().Length
            // only 1 delegate must be allowed
            // although we could use it as a filter chain
            // (and)
            return ExecuteImpl(query, match, match.Target, match.Method, match, comparator);
#endif
        }

        public static IList<Extent> ExecuteEnhancedFilter<Extent>(IObjectContainer container,
            IDb4oEnhancedFilter predicate)
        {
            return NQHandler(container).ExecuteEnhancedFilter<Extent>(predicate);
        }

        public IList<T> ExecuteEnhancedFilter<T>(IDb4oEnhancedFilter filter)
        {
            var query = _container.Query();
            query.Constrain(typeof (T));
            filter.OptimizeQuery(query);
            OnQueryExecution(filter, QueryExecutionKind.PreOptimized);
            return WrapQueryResult<T>(query);
        }

        private static NativeQueryHandler NQHandler(IObjectContainer container)
        {
            return ((ObjectContainerBase) container).GetNativeQueryHandler();
        }

        private IList<Extent> ExecuteImpl<Extent>(
            IQuery query,
            object originalPredicate,
            object matchTarget,
            MethodBase matchMethod,
            Predicate<Extent> match,
            IQueryComparator comparator)
        {
            var q = QueryForExtent<Extent>(query, comparator);
            try
            {
                if (OptimizeNativeQueries())
                {
                    OptimizeQuery(q, matchTarget, matchMethod);
                    OnQueryExecution(originalPredicate, QueryExecutionKind.DynamicallyOptimized);

                    return WrapQueryResult<Extent>(q);
                }
            }
            catch (FileNotFoundException fnfe)
            {
                NativeQueryOptimizerNotLoaded(fnfe);
            }
            catch (TargetInvocationException tie)
            {
                NativeQueryOptimizerNotLoaded(tie);
            }
            catch (TypeLoadException tle)
            {
                NativeQueryOptimizerNotLoaded(tle);
            }
            catch (Exception e)
            {
                OnQueryOptimizationFailure(e);

                NativeQueryUnoptimized(e);
            }

            return ExecuteUnoptimized(q, match);
        }

        private void NativeQueryUnoptimized(Exception e)
        {
            var dp = Container()._handlers.DiagnosticProcessor();
            if (dp.Enabled()) dp.NativeQueryUnoptimized(null, e);
        }

        private void NativeQueryOptimizerNotLoaded(Exception exception)
        {
            var dp = Container()._handlers.DiagnosticProcessor();
            if (dp.Enabled())
                dp.NativeQueryOptimizerNotLoaded(Db4o.Diagnostic.NativeQueryOptimizerNotLoaded.NqNotPresent, exception);
        }

        private IList<Extent> ExecuteUnoptimized<Extent>(IQuery q, Predicate<Extent> match)
        {
            q.Constrain(new GenericPredicateEvaluation<Extent>(match));
            OnQueryExecution(match, QueryExecutionKind.Unoptimized);
            return WrapQueryResult<Extent>(q);
        }

        private IQuery QueryForExtent<Extent>(IQuery query, IQueryComparator comparator)
        {
            query.Constrain(typeof (Extent));
            query.SortBy(comparator);
            return query;
        }

        private static IList<Extent> WrapQueryResult<Extent>(IQuery query)
        {
            var queryResult = ((QQuery) query).GetQueryResult();
            return new GenericObjectSetFacade<Extent>(queryResult);
        }

        private IQuery ConfigureQuery(IQuery query, Predicate predicate)
        {
            var filter = predicate as IDb4oEnhancedFilter;
            if (null != filter)
            {
                filter.OptimizeQuery(query);
                OnQueryExecution(predicate, QueryExecutionKind.PreOptimized);
                return query;
            }

            query.Constrain(predicate.ExtentType());

            try
            {
                if (OptimizeNativeQueries())
                {
                    OptimizeQuery(query, predicate, predicate.GetFilterMethod());
                    OnQueryExecution(predicate, QueryExecutionKind.DynamicallyOptimized);
                    return query;
                }
            }
            catch (Exception e)
            {
                OnQueryOptimizationFailure(e);

                if (OptimizeNativeQueries())
                {
                    var dp = Container()._handlers.DiagnosticProcessor();
                    if (dp.Enabled()) dp.NativeQueryUnoptimized(predicate, e);
                }
            }

            query.Constrain(new PredicateEvaluation(predicate));
            OnQueryExecution(predicate, QueryExecutionKind.Unoptimized);
            return query;
        }

        private ObjectContainerBase Container()
        {
            return ((ObjectContainerBase) _container);
        }

        private bool OptimizeNativeQueries()
        {
            return _container.Ext().Configure().OptimizeNativeQueries();
        }

        private void OptimizeQuery(IQuery q, object predicate, MethodBase filterMethod)
        {
            if (_builder == null)
                _builder = NQOptimizerFactory.CreateExpressionBuilder();

            _builder.Optimize(q, predicate, filterMethod);
        }

        private void OnQueryExecution(object predicate, QueryExecutionKind kind)
        {
            if (null == QueryExecution) return;
            QueryExecution(this, new QueryExecutionEventArgs(predicate, kind));
        }

        private void OnQueryOptimizationFailure(Exception e)
        {
            if (null == QueryOptimizationFailure) return;
            QueryOptimizationFailure(this, new QueryOptimizationFailureEventArgs(e));
        }
    }

    internal class GenericPredicateEvaluation<T> : DelegateEnvelope, IEvaluation
    {
        public GenericPredicateEvaluation()
        {
            // for db4o c/s when CallConstructors == true
        }

        public GenericPredicateEvaluation(Predicate<T> predicate)
            : base(predicate)
        {
        }

        public void Evaluate(ICandidate candidate)
        {
            // use starting _ for PascalCase conversion purposes
            var _predicate = (Predicate<T>) GetContent();
            candidate.Include(_predicate((T) candidate.GetObject()));
        }
    }
}