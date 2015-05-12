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
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Query;
using Db4objects.Db4o.Reflect;
using Db4objects.Db4o.Types;

namespace Db4objects.Db4o.Internal.Query.Processor
{
    /// <summary>Base class for all constraints on queries.</summary>
    /// <remarks>Base class for all constraints on queries.</remarks>
    /// <exclude></exclude>
    public abstract class QCon : IConstraint, IVisitor4, IUnversioned
    {
        internal static readonly IDGenerator idGenerator = new IDGenerator();
        private readonly int i_id;
        protected List4 _children;

        [NonSerialized] private bool _processedByIndex;

        [NonSerialized] internal QCandidates i_candidates;

        private Collection4 i_childrenCandidates;
        protected QE i_evaluator = QE.Default;
        internal Collection4 i_joins;
        protected QCon i_parent;

        [NonSerialized] internal Transaction i_trans;

        public QCon()
        {
        }

        internal QCon(Transaction a_trans)
        {
            //Used for query debug only.
            // our candidate object tree
            // collection of QCandidates to collect children elements and to
            // execute children. For convenience we hold them in the constraint,
            // so we can do collection and execution in two steps
            // all subconstraints
            // for evaluation
            // ID handling for fast find of QConstraint objects in 
            // pending OR evaluations
            // ANDs and ORs on this constraint
            // the parent of this constraint or null, if this is a root
            // our transaction to get a stream object anywhere
            // whether or not this constraint was used to get the initial set
            // in the FieldIndexProcessor
            // C/S only
            i_id = idGenerator.Next();
            i_trans = a_trans;
        }

        public virtual IConstraint And(IConstraint andWith)
        {
            lock (StreamLock())
            {
                return Join(andWith, true);
            }
        }

        // virtual
        public virtual IConstraint Contains()
        {
            throw NotSupported();
        }

        public virtual IConstraint Equal()
        {
            throw NotSupported();
        }

        public virtual object GetObject()
        {
            throw NotSupported();
        }

        public virtual IConstraint Greater()
        {
            throw NotSupported();
        }

        public virtual IConstraint Identity()
        {
            throw NotSupported();
        }

        public virtual IConstraint ByExample()
        {
            throw NotSupported();
        }

        public virtual IConstraint Like()
        {
            throw NotSupported();
        }

        public virtual IConstraint StartsWith(bool caseSensitive)
        {
            throw NotSupported();
        }

        public virtual IConstraint EndsWith(bool caseSensitive)
        {
            throw NotSupported();
        }

        public virtual IConstraint Not()
        {
            lock (StreamLock())
            {
                if (!(i_evaluator is QENot))
                {
                    i_evaluator = new QENot(i_evaluator);
                }
                return this;
            }
        }

        public virtual IConstraint Or(IConstraint orWith)
        {
            lock (StreamLock())
            {
                return Join(orWith, false);
            }
        }

        public virtual IConstraint Smaller()
        {
            throw NotSupported();
        }

        public virtual void Visit(object obj)
        {
            var qc = (QCandidate) obj;
            Visit1(qc.GetRoot(), this, Evaluate(qc));
        }

        internal virtual QCon AddConstraint(QCon
            a_child)
        {
            _children = new List4(_children, a_child);
            return a_child;
        }

        public virtual ObjectContainerBase Container()
        {
            return Transaction().Container();
        }

        public virtual Transaction Transaction()
        {
            return i_trans;
        }

        internal virtual void AddJoin(QConJoin a_join)
        {
            if (i_joins == null)
            {
                i_joins = new Collection4();
            }
            i_joins.Add(a_join);
        }

        internal virtual QCon AddSharedConstraint
            (QField a_field, object a_object)
        {
            var newConstraint = new QConObject(i_trans, this, a_field, a_object);
            AddConstraint(newConstraint);
            return newConstraint;
        }

        internal virtual bool Attach(QQuery query, string a_field)
        {
            var qcon = this;
            var yc = GetYapClass();
            bool[] foundField = {false};
            ForEachChildField(a_field, new _IVisitor4_101(foundField, query));
            if (foundField[0])
            {
                return true;
            }
            QField qf = null;
            if (yc == null || yc.HoldsAnyClass())
            {
                int[] count = {0};
                FieldMetadata[] yfs = {null};
                i_trans.Container().ClassCollection().AttachQueryNode(a_field, new _IVisitor4_119
                    (yfs, count));
                if (count[0] == 0)
                {
                    return false;
                }
                if (count[0] == 1)
                {
                    qf = yfs[0].QField(i_trans);
                }
                else
                {
                    qf = new QField(i_trans, a_field, null, 0, 0);
                }
            }
            else
            {
                if (yc.IsTranslated())
                {
                    i_trans.Container()._handlers.DiagnosticProcessor().DescendIntoTranslator(yc, a_field
                        );
                }
                var yf = yc.FieldMetadataForName(a_field);
                if (yf != null)
                {
                    qf = yf.QField(i_trans);
                }
                if (qf == null)
                {
                    qf = new QField(i_trans, a_field, null, 0, 0);
                }
            }
            var qcp = new QConPath(i_trans, qcon, qf);
            query.AddConstraint(qcp);
            qcon.AddConstraint(qcp);
            return true;
        }

        public virtual bool CanBeIndexLeaf()
        {
            return false;
        }

        public virtual bool CanLoadByIndex()
        {
            // virtual
            return false;
        }

        internal virtual void CheckLastJoinRemoved()
        {
            if (i_joins.Size() == 0)
            {
                i_joins = null;
            }
        }

        /// <param name="candidates"></param>
        internal virtual void Collect(QCandidates candidates)
        {
        }

        internal virtual void CreateCandidates(Collection4 a_candidateCollection)
        {
            var j = a_candidateCollection.GetEnumerator();
            while (j.MoveNext())
            {
                var candidates = (QCandidates) j.Current;
                if (candidates.TryAddConstraint(this))
                {
                    i_candidates = candidates;
                    return;
                }
            }
            i_candidates = new QCandidates((LocalTransaction) i_trans, GetYapClass(), GetField
                ());
            i_candidates.AddConstraint(this);
            a_candidateCollection.Add(i_candidates);
        }

        internal virtual void DoNotInclude(QCandidate a_root)
        {
            if (DTrace.enabled)
            {
                DTrace.Donotinclude.Log(Id());
            }
            if (i_parent != null)
            {
                i_parent.Visit1(a_root, this, false);
            }
            else
            {
                a_root.DoNotInclude();
            }
        }

        /// <param name="candidate"></param>
        internal virtual bool Evaluate(QCandidate candidate)
        {
            throw Exceptions4.VirtualException();
        }

        internal virtual void EvaluateChildren()
        {
            var i = i_childrenCandidates.GetEnumerator();
            while (i.MoveNext())
            {
                ((QCandidates) i.Current).Evaluate();
            }
        }

        internal virtual void EvaluateCollectChildren()
        {
            if (DTrace.enabled)
            {
                DTrace.CollectChildren.Log(Id());
            }
            var i = i_childrenCandidates.GetEnumerator();
            while (i.MoveNext())
            {
                ((QCandidates) i.Current).Collect(i_candidates);
            }
        }

        internal virtual void EvaluateCreateChildrenCandidates()
        {
            i_childrenCandidates = new Collection4();
            var i = IterateChildren();
            while (i.MoveNext())
            {
                ((QCon) i.Current).CreateCandidates(i_childrenCandidates
                    );
            }
        }

        internal virtual void EvaluateEvaluations()
        {
            var i = IterateChildren();
            while (i.MoveNext())
            {
                ((QCon) i.Current).EvaluateEvaluationsExec
                    (i_candidates, true);
            }
        }

        /// <param name="candidates"></param>
        /// <param name="rereadObject"></param>
        internal virtual void EvaluateEvaluationsExec(QCandidates candidates, bool rereadObject
            )
        {
        }

        // virtual
        internal virtual void EvaluateSelf()
        {
            i_candidates.Filter(this);
        }

        internal virtual void EvaluateSimpleChildren()
        {
            // TODO: sort the constraints for YapFields first,
            // so we stay with the same YapField
            if (_children == null)
            {
                return;
            }
            var i = IterateChildren();
            while (i.MoveNext())
            {
                var qcon = (QCon
                    ) i.Current;
                i_candidates.SetCurrentConstraint(qcon);
                qcon.SetCandidates(i_candidates);
                qcon.EvaluateSimpleExec(i_candidates);
            }
            i_candidates.SetCurrentConstraint(null);
        }

        /// <param name="candidates"></param>
        internal virtual void EvaluateSimpleExec(QCandidates candidates)
        {
        }

        // virtual
        internal virtual void ExchangeConstraint(QCon
            a_exchange, QCon a_with)
        {
            List4 previous = null;
            var current = _children;
            while (current != null)
            {
                if (current._element == a_exchange)
                {
                    if (previous == null)
                    {
                        _children = current._next;
                    }
                    else
                    {
                        previous._next = current._next;
                    }
                }
                previous = current;
                current = current._next;
            }
            _children = new List4(_children, a_with);
        }

        internal virtual void ForEachChildField(string name, IVisitor4 visitor)
        {
            var i = IterateChildren();
            while (i.MoveNext())
            {
                var obj = i.Current;
                if (obj is QConObject)
                {
                    if (((QConObject) obj).GetField().Name().Equals(name))
                    {
                        visitor.Visit(obj);
                    }
                }
            }
        }

        public virtual QField GetField()
        {
            return null;
        }

        internal virtual QCon GetRoot()
        {
            if (i_parent != null)
            {
                return i_parent.GetRoot();
            }
            return this;
        }

        internal virtual QCon ProduceTopLevelJoin
            ()
        {
            if (!HasJoins())
            {
                return this;
            }
            var i = IterateJoins();
            if (i_joins.Size() == 1)
            {
                i.MoveNext();
                return ((QCon) i.Current).ProduceTopLevelJoin
                    ();
            }
            var col = new Collection4();
            while (i.MoveNext())
            {
                col.Ensure(((QCon) i.Current).ProduceTopLevelJoin
                    ());
            }
            i = col.GetEnumerator();
            i.MoveNext();
            var qcon = (QCon
                ) i.Current;
            if (col.Size() == 1)
            {
                return qcon;
            }
            while (i.MoveNext())
            {
                qcon = (QCon) qcon.And((IConstraint) i.Current
                    );
            }
            return qcon;
        }

        internal virtual ClassMetadata GetYapClass()
        {
            return null;
        }

        public virtual bool HasChildren()
        {
            return _children != null;
        }

        public virtual bool HasParent()
        {
            return i_parent != null;
        }

        public virtual QCon Parent()
        {
            return i_parent;
        }

        public virtual bool HasJoins()
        {
            if (i_joins == null)
            {
                return false;
            }
            return i_joins.Size() > 0;
        }

        public virtual bool HasObjectInParentPath(object obj)
        {
            if (i_parent != null)
            {
                return i_parent.HasObjectInParentPath(obj);
            }
            return false;
        }

        public virtual int IdentityID()
        {
            return 0;
        }

        internal virtual bool IsNot()
        {
            return i_evaluator is QENot;
        }

        internal virtual bool IsNullConstraint()
        {
            return false;
        }

        public virtual IEnumerator IterateJoins()
        {
            if (i_joins == null)
            {
                return Iterators.EmptyIterator;
            }
            return i_joins.GetEnumerator();
        }

        public virtual IEnumerator IterateChildren()
        {
            if (_children == null)
            {
                return Iterators.EmptyIterator;
            }
            return new Iterator4Impl(_children);
        }

        internal virtual IConstraint Join(IConstraint a_with, bool a_and)
        {
            if (!(a_with is QCon))
            {
                // TODO: one of our STOr test cases somehow carries 
                // the same constraint twice. This may be a result
                // of a funny AND. Check!
                return null;
            }
            if (a_with == this)
            {
                return this;
            }
            return Join1((QCon) a_with, a_and);
        }

        internal virtual IConstraint Join1(QCon
            a_with, bool a_and)
        {
            if (a_with is QConstraints)
            {
                var j = 0;
                var joinHooks = new Collection4();
                var constraints = ((QConstraints) a_with).ToArray();
                for (j = 0; j < constraints.Length; j++)
                {
                    joinHooks.Ensure(((QCon) constraints[j]).
                        JoinHook());
                }
                var joins = new IConstraint[joinHooks.Size()];
                j = 0;
                var i = joinHooks.GetEnumerator();
                while (i.MoveNext())
                {
                    joins[j++] = Join((IConstraint) i.Current, a_and);
                }
                return new QConstraints(i_trans, joins);
            }
            var myHook = JoinHook();
            var otherHook = a_with.JoinHook();
            if (myHook == otherHook)
            {
                // You might like to check out, what happens, if you
                // remove this line. It seems to open a bug in an
                // StOr testcase.
                return myHook;
            }
            var cj = new QConJoin(i_trans, myHook, otherHook, a_and);
            myHook.AddJoin(cj);
            otherHook.AddJoin(cj);
            return cj;
        }

        internal virtual QCon JoinHook()
        {
            return ProduceTopLevelJoin();
        }

        internal virtual void Log(string indent)
        {
        }

        // System.out.println(indent + "JOINS");
        // joins += join.i_id + " ";
        //		System.out.println(joins);
        //		System.out.println(indent + getClass().getName() + " " + i_id + " " + i_debugField + " " + joins );
        // System.out.println(indent + "CONSTRAINTS");
        internal virtual string LogObject()
        {
            return string.Empty;
        }

        internal virtual void Marshall()
        {
            var i = IterateChildren();
            while (i.MoveNext())
            {
                ((QCon) i.Current).Marshall();
            }
        }

        private Exception NotSupported()
        {
            return new Exception("Not supported.");
        }

        /// <param name="other"></param>
        public virtual bool OnSameFieldAs(QCon other
            )
        {
            return false;
        }

        internal virtual void RemoveNot()
        {
            if (IsNot())
            {
                i_evaluator = ((QENot) i_evaluator).Evaluator();
            }
        }

        public virtual void SetCandidates(QCandidates a_candidates)
        {
            i_candidates = a_candidates;
        }

        internal virtual void SetParent(QCon a_newParent
            )
        {
            i_parent = a_newParent;
        }

        /// <param name="obj"></param>
        /// <param name="removeExisting"></param>
        internal virtual QCon ShareParent(object
            obj, BooleanByRef removeExisting)
        {
            // virtual
            return null;
        }

        /// <param name="claxx"></param>
        /// <param name="removeExisting"></param>
        internal virtual QConClass ShareParentForClass(IReflectClass claxx, BooleanByRef
            removeExisting)
        {
            // virtual
            return null;
        }

        protected virtual object StreamLock()
        {
            return i_trans.Container().Lock();
        }

        internal virtual void Unmarshall(Transaction a_trans)
        {
            if (i_trans != null)
            {
                return;
            }
            i_trans = a_trans;
            UnmarshallParent(a_trans);
            UnmarshallJoins(a_trans);
            UnmarshallChildren(a_trans);
        }

        private void UnmarshallParent(Transaction a_trans)
        {
            if (i_parent != null)
            {
                i_parent.Unmarshall(a_trans);
            }
        }

        private void UnmarshallChildren(Transaction a_trans)
        {
            var i = IterateChildren();
            while (i.MoveNext())
            {
                ((QCon) i.Current).Unmarshall(a_trans);
            }
        }

        private void UnmarshallJoins(Transaction a_trans)
        {
            if (HasJoins())
            {
                var i = IterateJoins();
                while (i.MoveNext())
                {
                    ((QCon) i.Current).Unmarshall(a_trans);
                }
            }
        }

        internal virtual void Visit(QCandidate a_root, bool res)
        {
            Visit1(a_root, this, i_evaluator.Not(res));
        }

        internal virtual void Visit1(QCandidate root, QCon
            reason, bool res)
        {
            // The a_reason parameter makes it eays to distinguish
            // between calls from above (a_reason == this) and below.
            if (HasJoins())
            {
                // this should probably be on the Join
                var i = IterateJoins();
                while (i.MoveNext())
                {
                    root.Evaluate(new QPending((QConJoin) i.Current, this, res));
                }
            }
            else
            {
                if (!res)
                {
                    DoNotInclude(root);
                }
            }
        }

        internal void VisitOnNull(QCandidate a_root)
        {
            // TODO: It may be more efficient to rule out 
            // all possible keepOnNull issues when starting
            // evaluation.
            var i = IterateChildren();
            while (i.MoveNext())
            {
                ((QCon) i.Current).VisitOnNull(a_root);
            }
            if (VisitSelfOnNull())
            {
                Visit(a_root, IsNullConstraint());
            }
        }

        internal virtual bool VisitSelfOnNull()
        {
            return true;
        }

        public virtual QE Evaluator()
        {
            return i_evaluator;
        }

        public virtual void SetProcessedByIndex()
        {
            InternalSetProcessedByIndex();
        }

        protected virtual void InternalSetProcessedByIndex()
        {
            _processedByIndex = true;
            if (i_joins != null)
            {
                var i = i_joins.GetEnumerator();
                while (i.MoveNext())
                {
                    ((QConJoin) i.Current).SetProcessedByIndex();
                }
            }
        }

        public virtual bool ProcessedByIndex()
        {
            return _processedByIndex;
        }

        public virtual int ChildrenCount()
        {
            return List4.Size(_children);
        }

        public virtual int Id()
        {
            return i_id;
        }

        private sealed class _IVisitor4_101 : IVisitor4
        {
            private readonly bool[] foundField;
            private readonly QQuery query;

            public _IVisitor4_101(bool[] foundField, QQuery query)
            {
                this.foundField = foundField;
                this.query = query;
            }

            public void Visit(object obj)
            {
                foundField[0] = true;
                query.AddConstraint((QCon) obj);
            }
        }

        private sealed class _IVisitor4_119 : IVisitor4
        {
            private readonly int[] count;
            private readonly FieldMetadata[] yfs;

            public _IVisitor4_119(FieldMetadata[] yfs, int[] count)
            {
                this.yfs = yfs;
                this.count = count;
            }

            public void Visit(object obj)
            {
                yfs[0] = (FieldMetadata) ((object[]) obj)[1];
                count[0]++;
            }
        }
    }
}