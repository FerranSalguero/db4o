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
using System.Text;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal.Marshall;
using Db4objects.Db4o.Internal.Query.Result;
using Db4objects.Db4o.Query;
using Db4objects.Db4o.Reflect;
using Db4objects.Db4o.Types;
using Sharpen.Util;

namespace Db4objects.Db4o.Internal.Query.Processor
{
    /// <summary>QQuery is the users hook on our graph.</summary>
    /// <remarks>
    ///     QQuery is the users hook on our graph.
    ///     A QQuery is defined by it's constraints.
    ///     NOTE: This is just a 'partial' base class to allow for variant implementations
    ///     in db4oj and db4ojdk1.2. It assumes that itself is an instance of QQuery
    ///     and should never be used explicitly.
    /// </remarks>
    /// <exclude></exclude>
    public abstract class QQueryBase : IInternalQuery, IUnversioned
    {
        [NonSerialized] private readonly QQuery _this;

        private readonly Collection4 i_constraints = new Collection4();
        private readonly string i_field;
        private readonly QQuery i_parent;
        private IQueryComparator _comparator;

        [NonSerialized] private QueryEvaluationMode _evaluationMode;

        private int _evaluationModeAsInt;
        private IList _orderings;
        private int _prefetchCount;
        private int _prefetchDepth;

        [NonSerialized] internal Transaction _trans;

        protected QQueryBase()
        {
            // C/S only
            _this = Cast(this);
        }

        protected QQueryBase(Transaction a_trans, QQuery a_parent
            , string a_field)
        {
            _this = Cast(this);
            _trans = a_trans;
            i_parent = a_parent;
            i_field = a_field;
        }

        public virtual IInternalObjectContainer Container
        {
            get { return Stream(); }
        }

        public virtual void CaptureQueryResultConfig()
        {
            var config = _trans.Container().Config();
            _evaluationMode = config.EvaluationMode();
            _prefetchDepth = config.PrefetchDepth();
            _prefetchCount = config.PrefetchObjectCount();
        }

        internal virtual void AddConstraint(QCon a_constraint)
        {
            i_constraints.Add(a_constraint);
        }

        private void AddConstraint(Collection4 col, object obj)
        {
            if (AttachToExistingConstraints(col, obj, true))
            {
                return;
            }
            if (AttachToExistingConstraints(col, obj, false))
            {
                return;
            }
            var newConstraint = new QConObject(_trans, null, null, obj);
            AddConstraint(newConstraint);
            col.Add(newConstraint);
        }

        private bool AttachToExistingConstraints(Collection4 newConstraintsCollector, object
            obj, bool onlyForPaths)
        {
            var found = false;
            var j = IterateConstraints();
            while (j.MoveNext())
            {
                var existingConstraint = (QCon) j.Current;
                var removeExisting = new BooleanByRef(false);
                if (!onlyForPaths || (existingConstraint is QConPath))
                {
                    var newConstraint = existingConstraint.ShareParent(obj, removeExisting);
                    if (newConstraint != null)
                    {
                        newConstraintsCollector.Add(newConstraint);
                        AddConstraint(newConstraint);
                        if (removeExisting.value)
                        {
                            RemoveConstraint(existingConstraint);
                        }
                        found = true;
                        if (!onlyForPaths)
                        {
                            break;
                        }
                    }
                }
            }
            return found;
        }

        /// <summary>Search for slot that corresponds to class.</summary>
        /// <remarks>
        ///     Search for slot that corresponds to class. <br />If not found add it.
        ///     <br />Constrain it. <br />
        /// </remarks>
        public virtual IConstraint Constrain(object example)
        {
            lock (StreamLock())
            {
                var claxx = ReflectClassForClass(example);
                if (claxx != null)
                {
                    return AddClassConstraint(claxx);
                }
                var eval = Platform4.EvaluationCreate(_trans, example);
                if (eval != null)
                {
                    return AddEvaluationToAllConstraints(eval);
                }
                var constraints = new Collection4();
                AddConstraint(constraints, example);
                return ToConstraint(constraints);
            }
        }

        private IConstraint AddEvaluationToAllConstraints(QConEvaluation eval)
        {
            if (i_constraints.Size() == 0)
            {
                _trans.Container().ClassCollection().IterateTopLevelClasses(new _IVisitor4_139(this
                    ));
            }
            var i = IterateConstraints();
            while (i.MoveNext())
            {
                ((QCon) i.Current).AddConstraint(eval);
            }
            // FIXME: should return valid Constraint object
            return null;
        }

        private IConstraint AddClassConstraint(IReflectClass claxx)
        {
            if (IsTheObjectClass(claxx))
            {
                return null;
            }
            if (claxx.IsInterface())
            {
                return AddInterfaceConstraint(claxx);
            }
            var newConstraints = IntroduceClassConstrain(claxx);
            if (newConstraints.IsEmpty())
            {
                var qcc = new QConClass(_trans, claxx);
                AddConstraint(qcc);
                return qcc;
            }
            return ToConstraint(newConstraints);
        }

        private Collection4 IntroduceClassConstrain(IReflectClass claxx)
        {
            var newConstraints = new Collection4();
            var existingConstraints = IterateConstraints();
            while (existingConstraints.MoveNext())
            {
                QCon existingConstraint = (QConObject) existingConstraints.Current;
                var removeExisting = new BooleanByRef(false);
                QCon newConstraint = existingConstraint.ShareParentForClass(claxx, removeExisting
                    );
                if (newConstraint != null)
                {
                    newConstraints.Add(newConstraint);
                    AddConstraint(newConstraint);
                    if (removeExisting.value)
                    {
                        RemoveConstraint(existingConstraint);
                    }
                }
            }
            return newConstraints;
        }

        private bool IsTheObjectClass(IReflectClass claxx)
        {
            return claxx.Equals(Stream()._handlers.IclassObject);
        }

        private IConstraint AddInterfaceConstraint(IReflectClass claxx)
        {
            var classes = Stream().ClassCollection().ForInterface(claxx);
            if (classes.Size() == 0)
            {
                var qcc = new QConClass(_trans, null, null, claxx);
                AddConstraint(qcc);
                return qcc;
            }
            var i = classes.GetEnumerator();
            IConstraint constr = null;
            while (i.MoveNext())
            {
                var classMetadata = (ClassMetadata) i.Current;
                var classMetadataClaxx = classMetadata.ClassReflector();
                if (classMetadataClaxx != null)
                {
                    if (!classMetadataClaxx.IsInterface())
                    {
                        if (constr == null)
                        {
                            constr = Constrain(classMetadataClaxx);
                        }
                        else
                        {
                            constr = constr.Or(Constrain(classMetadata.ClassReflector()));
                        }
                    }
                }
            }
            return constr;
        }

        private IReflectClass ReflectClassForClass(object example)
        {
            if (example is IReflectClass)
            {
                return (IReflectClass) example;
            }
            if (example is Type)
            {
                return _trans.Reflector().ForClass((Type) example);
            }
            return null;
        }

        public virtual IConstraints Constraints()
        {
            lock (StreamLock())
            {
                var constraints = new IConstraint[i_constraints.Size()];
                i_constraints.ToArray(constraints);
                return new QConstraints(_trans, constraints);
            }
        }

        public virtual IQuery Descend(string a_field)
        {
            lock (StreamLock())
            {
                var query = new QQuery(_trans, _this, a_field);
                var run = new IntByRef(1);
                if (!Descend1(query, a_field, run))
                {
                    // try to add unparented nodes on the second run,
                    // if not added in the first run and a descendant
                    // was not found
                    if (run.value == 1)
                    {
                        run.value = 2;
                        if (!Descend1(query, a_field, run))
                        {
                            new QConUnconditional(_trans, false).Attach(query, a_field);
                        }
                    }
                }
                return query;
            }
        }

        private bool Descend1(QQuery query, string fieldName, IntByRef run)
        {
            if (run.value == 2 || i_constraints.Size() == 0)
            {
                // On the second run we are really creating a second independant
                // query network that is not joined to other higher level
                // constraints.
                // Let's see how this works out. We may need to join networks.
                run.value = 0;
                // prevent a double run of this code
                Stream().ClassCollection().AttachQueryNode(fieldName, new _IVisitor4_275(this));
            }
            CheckConstraintsEvaluationMode();
            var foundClass = new BooleanByRef(false);
            var i = IterateConstraints();
            while (i.MoveNext())
            {
                if (((QCon) i.Current).Attach(query, fieldName))
                {
                    foundClass.value = true;
                }
            }
            return foundClass.value;
        }

        public virtual IObjectSet Execute()
        {
            lock (StreamLock())
            {
                return ((IObjectSet) TriggeringQueryEvents(new _IClosure4_331(this)));
            }
        }

        public virtual void ExecuteLocal(IdListQueryResult result)
        {
            CheckConstraintsEvaluationMode();
            var r = CreateCandidateCollection();
            var checkDuplicates = r.checkDuplicates;
            var topLevel = r.topLevel;
            var candidateCollection = r.candidateCollection;
            if (candidateCollection != null)
            {
                var executionPath = topLevel ? null : FieldPathFromTop();
                IEnumerator i = new Iterator4Impl(candidateCollection);
                while (i.MoveNext())
                {
                    ((QCandidates) i.Current).Execute();
                }
                if (candidateCollection._next != null)
                {
                    checkDuplicates = true;
                }
                if (checkDuplicates)
                {
                    result.CheckDuplicates();
                }
                var stream = Stream();
                i = new Iterator4Impl(candidateCollection);
                while (i.MoveNext())
                {
                    var candidates = (QCandidates) i.Current;
                    if (topLevel)
                    {
                        candidates.Traverse(result);
                    }
                    else
                    {
                        candidates.Traverse(new _IVisitor4_374(this, executionPath, stream, result));
                    }
                }
            }
            Sort(result);
        }

        private void TriggerQueryOnFinished()
        {
            Stream().Callbacks().QueryOnFinished(_trans, Cast(this));
        }

        private void TriggerQueryOnStarted()
        {
            Stream().Callbacks().QueryOnStarted(_trans, Cast(this));
        }

        public virtual IEnumerator ExecuteLazy()
        {
            CheckConstraintsEvaluationMode();
            var r = CreateCandidateCollection();
            var executionPath = ExecutionPath(r);
            IEnumerator candidateCollection = new Iterator4Impl(r.candidateCollection);
            MappingIterator executeCandidates = new _MappingIterator_438(executionPath, candidateCollection
                );
            var resultingIDs = new CompositeIterator4(executeCandidates);
            if (!r.checkDuplicates)
            {
                return resultingIDs;
            }
            return CheckDuplicates(resultingIDs);
        }

        public virtual IQueryResult GetQueryResult()
        {
            lock (StreamLock())
            {
                if (i_constraints.Size() == 0)
                {
                    return ExecuteAllObjectsQuery();
                }
                var result = ExecuteClassOnlyQuery();
                if (result != null)
                {
                    return result;
                }
                OptimizeJoins();
                return ExecuteQuery();
            }
        }

        protected IQueryResult ExecuteQuery()
        {
            return Stream().ExecuteQuery(_this);
        }

        private IQueryResult ExecuteAllObjectsQuery()
        {
            return Stream().QueryAllObjects(_trans);
        }

        protected virtual ObjectContainerBase Stream()
        {
            return _trans.Container();
        }

        private IQueryResult ExecuteClassOnlyQuery()
        {
            var clazz = SingleClassConstraint();
            if (null == clazz)
            {
                return null;
            }
            var queryResult = Stream().ClassOnlyQuery(this, clazz);
            Sort(queryResult);
            return queryResult;
        }

        private ClassMetadata SingleClassConstraint()
        {
            if (RequiresSort())
            {
                return null;
            }
            var clazzconstr = ClassConstraint();
            if (clazzconstr == null)
            {
                return null;
            }
            var clazz = clazzconstr._classMetadata;
            if (clazz == null)
            {
                return null;
            }
            if (clazzconstr.HasChildren() || clazz.IsArray())
            {
                return null;
            }
            return clazz;
        }

        private QConClass ClassConstraint()
        {
            if (i_constraints.Size() != 1)
            {
                return null;
            }
            var constr = SingleConstraint();
            if (constr.GetType() != typeof (QConClass))
            {
                return null;
            }
            return (QConClass) constr;
        }

        private IConstraint SingleConstraint()
        {
            return (IConstraint) i_constraints.SingleElement();
        }

        public virtual IEnumerator ExecuteSnapshot()
        {
            var r = CreateCandidateCollection();
            var executionPath = ExecutionPath(r);
            IEnumerator candidatesIterator = new Iterator4Impl(r.candidateCollection);
            var snapshots = new Collection4();
            while (candidatesIterator.MoveNext())
            {
                var candidates = (QCandidates) candidatesIterator.Current;
                snapshots.Add(candidates.ExecuteSnapshot(executionPath));
            }
            var snapshotsIterator = snapshots.GetEnumerator();
            var resultingIDs = new CompositeIterator4(snapshotsIterator);
            if (!r.checkDuplicates)
            {
                return resultingIDs;
            }
            return CheckDuplicates(resultingIDs);
        }

        public virtual object TriggeringQueryEvents(IClosure4 closure)
        {
            TriggerQueryOnStarted();
            try
            {
                return closure.Run();
            }
            finally
            {
                TriggerQueryOnFinished();
            }
        }

        private IEnumerator CheckDuplicates(CompositeIterator4 executeAllCandidates)
        {
            return Iterators.Filter(executeAllCandidates, new _IPredicate4_573());
        }

        private Collection4 ExecutionPath(CreateCandidateCollectionResult r)
        {
            return r.topLevel ? null : FieldPathFromTop();
        }

        public virtual void CheckConstraintsEvaluationMode()
        {
            var constraints = IterateConstraints();
            while (constraints.MoveNext())
            {
                ((QConObject) constraints.Current).SetEvaluationMode();
            }
        }

        private Collection4 FieldPathFromTop()
        {
            var q = this;
            var fieldPath = new Collection4();
            while (q.i_parent != null)
            {
                fieldPath.Prepend(q.i_field);
                q = q.i_parent;
            }
            return fieldPath;
        }

        private void LogConstraints()
        {
        }

        public virtual CreateCandidateCollectionResult CreateCandidateCollection
            ()
        {
            var candidatesList = CreateQCandidatesList();
            var checkDuplicates = false;
            var topLevel = true;
            var i = IterateConstraints();
            while (i.MoveNext())
            {
                var constraint = (QCon) i.Current;
                var old = constraint;
                constraint = constraint.GetRoot();
                if (constraint != old)
                {
                    checkDuplicates = true;
                    topLevel = false;
                }
                var classMetadata = constraint.GetYapClass();
                if (classMetadata == null)
                {
                    break;
                }
                AddConstraintToCandidatesList(candidatesList, constraint);
            }
            return new CreateCandidateCollectionResult(candidatesList, checkDuplicates
                , topLevel);
        }

        private void AddConstraintToCandidatesList(List4 candidatesList, QCon qcon)
        {
            if (candidatesList == null)
            {
                return;
            }
            IEnumerator j = new Iterator4Impl(candidatesList);
            while (j.MoveNext())
            {
                var candidates = (QCandidates) j.Current;
                candidates.AddConstraint(qcon);
            }
        }

        private List4 CreateQCandidatesList()
        {
            List4 candidatesList = null;
            var i = IterateConstraints();
            while (i.MoveNext())
            {
                var constraint = (QCon) i.Current;
                constraint = constraint.GetRoot();
                var classMetadata = constraint.GetYapClass();
                if (classMetadata == null)
                {
                    continue;
                }
                if (ConstraintCanBeAddedToExisting(candidatesList, constraint))
                {
                    continue;
                }
                var candidates = new QCandidates((LocalTransaction) _trans, classMetadata,
                    null);
                candidatesList = new List4(candidatesList, candidates);
            }
            return candidatesList;
        }

        private bool ConstraintCanBeAddedToExisting(List4 candidatesList, QCon constraint
            )
        {
            IEnumerator j = new Iterator4Impl(candidatesList);
            while (j.MoveNext())
            {
                var candidates = (QCandidates) j.Current;
                if (candidates.FitsIntoExistingConstraintHierarchy(constraint))
                {
                    return true;
                }
            }
            return false;
        }

        public Transaction Transaction()
        {
            return _trans;
        }

        public virtual IEnumerator IterateConstraints()
        {
            // clone the collection first to avoid
            // InvalidIteratorException as i_constraints might be 
            // modified during the execution of callee
            return new Collection4(i_constraints).GetEnumerator();
        }

        public virtual IQuery OrderAscending()
        {
            if (i_parent == null)
            {
                throw new InvalidOperationException("Cannot apply ordering at top level.");
            }
            lock (StreamLock())
            {
                AddOrdering(SodaQueryComparator.Direction.Ascending);
                return _this;
            }
        }

        public virtual IQuery OrderDescending()
        {
            if (i_parent == null)
            {
                throw new InvalidOperationException("Cannot apply ordering at top level.");
            }
            lock (StreamLock())
            {
                AddOrdering(SodaQueryComparator.Direction.Descending);
                return _this;
            }
        }

        private void AddOrdering(SodaQueryComparator.Direction direction)
        {
            AddOrdering(direction, new ArrayList());
        }

        protected void AddOrdering(SodaQueryComparator.Direction direction, IList path)
        {
            if (i_field != null)
            {
                path.Add(i_field);
            }
            if (i_parent != null)
            {
                i_parent.AddOrdering(direction, path);
                return;
            }
            var fieldPath = ReverseFieldPath(path);
            RemoveExistingOrderingFor(fieldPath);
            Orderings().Add(new SodaQueryComparator.Ordering(direction, fieldPath));
        }

        private void RemoveExistingOrderingFor(string[] fieldPath)
        {
            for (var orderingIter = Orderings().GetEnumerator();
                orderingIter.MoveNext
                    ();)
            {
                var ordering = ((SodaQueryComparator.Ordering) orderingIter
                    .Current);
                if (Arrays.Equals(ordering.FieldPath(), fieldPath))
                {
                    Orderings().Remove(ordering);
                    break;
                }
            }
        }

        /// <summary>Public so it can be used by the LINQ test cases.</summary>
        /// <remarks>Public so it can be used by the LINQ test cases.</remarks>
        public IList Orderings()
        {
            if (null == _orderings)
            {
                _orderings = new ArrayList();
            }
            return _orderings;
        }

        private string[] ReverseFieldPath(IList path)
        {
            var reversedPath = new string[path.Count];
            for (var i = 0; i < reversedPath.Length; i++)
            {
                reversedPath[i] = ((string) path[path.Count - i - 1]);
            }
            return reversedPath;
        }

        public virtual void Marshall()
        {
            CheckConstraintsEvaluationMode();
            _evaluationModeAsInt = _evaluationMode.AsInt();
            var i = IterateConstraints();
            while (i.MoveNext())
            {
                ((QCon) i.Current).GetRoot().Marshall();
            }
        }

        public virtual void Unmarshall(Transaction a_trans)
        {
            _evaluationMode = QueryEvaluationMode.FromInt(_evaluationModeAsInt);
            _trans = a_trans;
            var i = IterateConstraints();
            while (i.MoveNext())
            {
                ((QCon) i.Current).Unmarshall(a_trans);
            }
        }

        internal virtual void RemoveConstraint(QCon a_constraint)
        {
            i_constraints.Remove(a_constraint);
        }

        internal virtual IConstraint ToConstraint(Collection4 constraints)
        {
            if (constraints.Size() == 1)
            {
                return (IConstraint) constraints.SingleElement();
            }
            if (constraints.Size() > 0)
            {
                var constraintArray = new IConstraint[constraints.Size()];
                constraints.ToArray(constraintArray);
                return new QConstraints(_trans, constraintArray);
            }
            return null;
        }

        protected virtual object StreamLock()
        {
            return Stream().Lock();
        }

        public virtual IQuery SortBy(IQueryComparator comparator)
        {
            _comparator = comparator;
            return _this;
        }

        private void Sort(IQueryResult result)
        {
            if (_orderings != null)
            {
                if (result.Size() == 0)
                {
                    return;
                }
                result.SortIds(NewSodaQueryComparator());
            }
            if (_comparator != null)
            {
                if (result.Size() == 0)
                {
                    return;
                }
                result.Sort(_comparator);
            }
        }

        private IIntComparator NewSodaQueryComparator()
        {
            return new SodaQueryComparator((LocalObjectContainer) Transaction().Container
                (), ExtentType(), Sharpen.Collections.ToArray(_orderings
                    , new SodaQueryComparator.Ordering[_orderings.Count]));
        }

        private ClassMetadata ExtentType()
        {
            return ClassConstraint().GetYapClass();
        }

        // cheat emulating '(QQuery)this'
        private static QQuery Cast(QQueryBase obj)
        {
            return (QQuery) obj;
        }

        public virtual bool RequiresSort()
        {
            if (_comparator != null || _orderings != null)
            {
                return true;
            }
            return false;
        }

        public virtual IQueryComparator Comparator()
        {
            return _comparator;
        }

        public virtual QueryEvaluationMode EvaluationMode()
        {
            return _evaluationMode;
        }

        public virtual void EvaluationMode(QueryEvaluationMode mode)
        {
            _evaluationMode = mode;
        }

        private void OptimizeJoins()
        {
            if (!HasOrJoins())
            {
                RemoveJoins();
            }
        }

        private bool HasOrJoins()
        {
            return ForEachConstraintRecursively(new _IFunction4_861());
        }

        private void RemoveJoins()
        {
            ForEachConstraintRecursively(new _IFunction4_877());
        }

        private bool ForEachConstraintRecursively(IFunction4 block)
        {
            IQueue4 queue = new NoDuplicatesQueue(new NonblockingQueue());
            var constrIter = IterateConstraints();
            while (constrIter.MoveNext())
            {
                queue.Add(constrIter.Current);
            }
            while (queue.HasNext())
            {
                var constr = (QCon) queue.Next();
                var cancel = (bool) block.Apply(constr);
                if (cancel)
                {
                    return true;
                }
                var childIter = constr.IterateChildren();
                while (childIter.MoveNext())
                {
                    queue.Add(childIter.Current);
                }
                var joinIter = constr.IterateJoins();
                while (joinIter.MoveNext())
                {
                    queue.Add(joinIter.Current);
                }
            }
            return false;
        }

        public virtual int PrefetchDepth()
        {
            return _prefetchDepth;
        }

        public virtual int PrefetchCount()
        {
            return _prefetchCount;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("QQueryBase\n");
            var i = IterateConstraints();
            while (i.MoveNext())
            {
                var constraint = (QCon) i.Current;
                sb.Append(constraint);
                sb.Append("\n");
            }
            return sb.ToString();
        }

        public virtual QQuery Parent()
        {
            return i_parent;
        }

        private sealed class _IVisitor4_139 : IVisitor4
        {
            private readonly QQueryBase _enclosing;

            public _IVisitor4_139(QQueryBase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Visit(object obj)
            {
                var classMetadata = (ClassMetadata) obj;
                var qcc = new QConClass(_enclosing._trans, classMetadata.ClassReflector
                    ());
                _enclosing.AddConstraint(qcc);
                _enclosing.ToConstraint(_enclosing.i_constraints).Or(qcc);
            }
        }

        private sealed class _IVisitor4_275 : IVisitor4
        {
            private readonly QQueryBase _enclosing;
            internal bool untypedFieldConstraintCollected;

            public _IVisitor4_275(QQueryBase _enclosing)
            {
                this._enclosing = _enclosing;
                untypedFieldConstraintCollected = false;
            }

            public void Visit(object obj)
            {
                var pair = ((object[]) obj);
                var containingClass = (ClassMetadata) pair[0];
                var field = (FieldMetadata) pair[1];
                if (IsTyped(field))
                {
                    AddFieldConstraint(containingClass, field);
                    return;
                }
                if (untypedFieldConstraintCollected)
                {
                    return;
                }
                AddFieldConstraint(containingClass, field);
                untypedFieldConstraintCollected = true;
            }

            private bool IsTyped(FieldMetadata field)
            {
                return !Handlers4.IsUntyped(field.GetHandler());
            }

            private void AddFieldConstraint(ClassMetadata containingClass, FieldMetadata field
                )
            {
                var qcc = new QConClass(_enclosing._trans, null, field.QField(_enclosing
                    ._trans), containingClass.ClassReflector());
                _enclosing.AddConstraint(qcc);
                _enclosing.ToConstraint(_enclosing.i_constraints).Or(qcc);
            }
        }

        private sealed class _IClosure4_331 : IClosure4
        {
            private readonly QQueryBase _enclosing;

            public _IClosure4_331(QQueryBase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public object Run()
            {
                return new ObjectSetFacade(_enclosing.GetQueryResult());
            }
        }

        private sealed class _IVisitor4_374 : IVisitor4
        {
            private readonly QQueryBase _enclosing;
            private readonly Collection4 executionPath;
            private readonly IdListQueryResult result;
            private readonly ObjectContainerBase stream;

            public _IVisitor4_374(QQueryBase _enclosing, Collection4 executionPath, ObjectContainerBase
                stream, IdListQueryResult result)
            {
                this._enclosing = _enclosing;
                this.executionPath = executionPath;
                this.stream = stream;
                this.result = result;
            }

            public void Visit(object a_object)
            {
                var candidate = (QCandidate) a_object;
                if (candidate.Include())
                {
                    var ids = new TreeInt(candidate._key);
                    var idsNew = new ByRef();
                    var itPath = executionPath.GetEnumerator();
                    while (itPath.MoveNext())
                    {
                        idsNew.value = null;
                        var fieldName = (string) (itPath.Current);
                        if (ids != null)
                        {
                            ids.Traverse(new _IVisitor4_385(this, stream, fieldName, idsNew));
                        }
                        ids = (TreeInt) idsNew.value;
                    }
                    if (ids != null)
                    {
                        ids.Traverse(new _IVisitor4_406(result));
                    }
                }
            }

            private sealed class _IVisitor4_385 : IVisitor4
            {
                private readonly _IVisitor4_374 _enclosing;
                private readonly string fieldName;
                private readonly ByRef idsNew;
                private readonly ObjectContainerBase stream;

                public _IVisitor4_385(_IVisitor4_374 _enclosing, ObjectContainerBase stream, string
                    fieldName, ByRef idsNew)
                {
                    this._enclosing = _enclosing;
                    this.stream = stream;
                    this.fieldName = fieldName;
                    this.idsNew = idsNew;
                }

                public void Visit(object treeInt)
                {
                    var id = ((TreeInt) treeInt)._key;
                    var reader = stream.ReadStatefulBufferById(_enclosing._enclosing.
                        _trans, id);
                    if (reader != null)
                    {
                        var oh = new ObjectHeader(stream, reader);
                        var context = new CollectIdContext(_enclosing._enclosing._trans
                            , oh, reader);
                        oh.ClassMetadata().CollectIDs(context, fieldName);
                        Tree.Traverse(context.Ids(), new _IVisitor4_394(idsNew));
                    }
                }

                private sealed class _IVisitor4_394 : IVisitor4
                {
                    private readonly ByRef idsNew;

                    public _IVisitor4_394(ByRef idsNew)
                    {
                        this.idsNew = idsNew;
                    }

                    public void Visit(object node)
                    {
                        idsNew.value = TreeInt.Add(((TreeInt) idsNew.value), ((TreeInt) node)._key);
                    }
                }
            }

            private sealed class _IVisitor4_406 : IVisitor4
            {
                private readonly IdListQueryResult result;

                public _IVisitor4_406(IdListQueryResult result)
                {
                    this.result = result;
                }

                public void Visit(object treeInt)
                {
                    result.AddKeyCheckDuplicates(((TreeInt) treeInt)._key);
                }
            }
        }

        private sealed class _MappingIterator_438 : MappingIterator
        {
            private readonly Collection4 executionPath;

            public _MappingIterator_438(Collection4 executionPath, IEnumerator baseArg1) : base
                (baseArg1)
            {
                this.executionPath = executionPath;
            }

            protected override object Map(object current)
            {
                return ((QCandidates) current).ExecuteLazy(executionPath);
            }
        }

        public class CreateCandidateCollectionResult
        {
            public readonly List4 candidateCollection;
            public readonly bool checkDuplicates;
            public readonly bool topLevel;

            public CreateCandidateCollectionResult(List4 candidateCollection_, bool checkDuplicates_
                , bool topLevel_)
            {
                candidateCollection = candidateCollection_;
                topLevel = topLevel_;
                checkDuplicates = checkDuplicates_;
            }
        }

        private sealed class _IPredicate4_573 : IPredicate4
        {
            private TreeInt ids;

            public _IPredicate4_573()
            {
                ids = new TreeInt(0);
            }

            public bool Match(object current)
            {
                var id = ((int) current);
                if (ids.Find(id) != null)
                {
                    return false;
                }
                ids = (TreeInt) ids.Add(new TreeInt(id));
                return true;
            }
        }

        private sealed class _IFunction4_861 : IFunction4
        {
            public object Apply(object obj)
            {
                var constr = (QCon) obj;
                var joinIter = constr.IterateJoins();
                while (joinIter.MoveNext())
                {
                    var join = (QConJoin) joinIter.Current;
                    if (join.IsOr())
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private sealed class _IFunction4_877 : IFunction4
        {
            public object Apply(object obj)
            {
                var constr = (QCon) obj;
                constr.i_joins = null;
                return false;
            }
        }
    }
}