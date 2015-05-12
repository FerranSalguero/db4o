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

namespace Db4objects.Db4o.Tests.CLI1.NativeQueries
{
    public enum Priority
    {
        None,
        Low,
        Normal,
        High
    }

    public class Base
    {
        public int id;

        public virtual int Id
        {
            get { return id; }
        }
    }

    public class Data : Base
    {
        public string name;
        public Data previous;

        public Data(int id, string name, Data previous, DateTime expires, Priority priority)
        {
            this.id = id;
            this.name = name;
            this.previous = previous;
            Expires = expires;
            Priority = priority;
        }

        public string Name
        {
            get { return name; }
        }

        public Data Previous
        {
            get { return previous; }
        }

        public bool HasPrevious
        {
            get { return Previous != null; }
        }

        public DateTime Expires { get; private set; }
        public Priority Priority { get; private set; }

        public override string ToString()
        {
            return string.Format("Data(id={0}, name={1}, previous={2})",
                id, name, previous == null ? "null" : previous.id.ToString());
        }
    }

    internal class ReturnTruePredicate
    {
        public bool Match(Data candidate)
        {
            return true;
        }
    }

    internal class RightSideField
    {
        public bool Match(Data candidate)
        {
            return "Bb" == candidate.Name;
        }
    }

    internal class DescendRightSideField
    {
        public bool Match(Data candidate)
        {
            return null != candidate.Previous && "Aa" == candidate.Previous.Name;
        }
    }

    internal class WithPriority
    {
        private readonly Priority _priority;

        public WithPriority(Priority priority)
        {
            _priority = priority;
        }

        public bool Match(Data candidate)
        {
            return candidate.Priority == _priority;
        }
    }

    internal class DateRange
    {
        private readonly DateTime _begin;
        private readonly DateTime _end;

        public DateRange(DateTime begin, DateTime end)
        {
            _begin = begin;
            _end = end;
        }

        public bool Match(Data candidate)
        {
            return (candidate.Expires >= _begin) && (candidate.Expires <= _end);
        }
    }

    internal class ConstStringFieldPredicate
    {
        public bool Match(Data candidate)
        {
            return candidate.name == "Cc";
        }
    }

    internal class ConstStringFieldNotEqual
    {
        public bool Match(Data candidate)
        {
            return candidate.name != "Cc";
        }
    }

    internal class ConstStringFieldPredicateEmpty
    {
        public bool Match(Data candidate)
        {
            return candidate.name == "ABBA";
        }
    }

    internal class ConstStringFieldOrPredicate
    {
        public bool Match(Data candidate)
        {
            return candidate.name == "Aa" || candidate.name == "Bb";
        }
    }

    internal class ConstStringFieldOrPredicateEmpty
    {
        public bool Match(Data candidate)
        {
            return candidate.name == "ABBA" || candidate.name == "MILI";
        }
    }

    internal class ConstIntFieldPredicate1
    {
        public bool Match(Data candidate)
        {
            return candidate.id == 1;
        }
    }

    internal class ConstIntFieldPredicate2
    {
        public bool Match(Data candidate)
        {
            return candidate.id == 2;
        }
    }

    internal class ConstIntFieldOrPredicate
    {
        public bool Match(Data candidate)
        {
            return candidate.id == 1 || candidate.id == 2;
        }
    }

    internal class IntFieldLessThanConst
    {
        public bool Match(Data candidate)
        {
            return candidate.id < 2;
        }
    }

    internal class IntFieldGreaterThanConst
    {
        public bool Match(Data candidate)
        {
            return candidate.id > 2;
        }
    }

    internal class IntFieldLessThanOrEqualConst
    {
        public bool Match(Data candidate)
        {
            return candidate.id <= 2;
        }
    }

    internal class IntFieldGreaterThanOrEqualConst
    {
        public bool Match(Data candidate)
        {
            return candidate.id >= 2;
        }
    }

    internal class IntGetterEqual
    {
        public bool Match(Data candidate)
        {
            return candidate.Id == 2;
        }
    }

    internal class IntGetterNotEqual
    {
        public bool Match(Data candidate)
        {
            return candidate.Id != 2;
        }
    }

    internal class IntGetterGreaterThan
    {
        public bool Match(Data candidate)
        {
            return candidate.Id > 2;
        }
    }

    internal class IntGetterLessThan
    {
        public bool Match(Data candidate)
        {
            return candidate.Id < 2;
        }
    }

    internal class IntGetterLessThanOrEqual
    {
        public bool Match(Data candidate)
        {
            return candidate.Id <= 2;
        }
    }

    internal class IntGetterGreaterThanOrEqual
    {
        public bool Match(Data candidate)
        {
            return candidate.Id >= 2;
        }
    }

    internal class StringGetterEqual
    {
        public bool Match(Data candidate)
        {
            return candidate.Name == "Cc";
        }
    }

    internal class CandidateNestedMethodInvocation
    {
        public bool Match(Data candidate)
        {
            return candidate.HasPrevious;
        }
    }

    internal class NotIntFieldEqual
    {
        public bool Match(Data candidate)
        {
            return !(candidate.id == 1);
        }
    }

    internal class NotIntGetterGreater
    {
        public bool Match(Data candidate)
        {
            return !(candidate.Id > 2);
        }
    }

    internal class NotStringGetterEqual
    {
        public bool Match(Data candidate)
        {
            return !(candidate.Name == "Cc");
        }
    }

    internal class IdGreaterOrEqualThan
    {
        private readonly int _id;

        public IdGreaterOrEqualThan(int id)
        {
            _id = id;
        }

        public bool Match(Data candidate)
        {
            return candidate.id >= _id;
        }
    }

    internal class NameEqualsTo
    {
        private readonly string _name;

        public NameEqualsTo(string name)
        {
            _name = name;
        }

        public bool Match(Data candidate)
        {
            return candidate.Name == _name;
        }
    }

    internal class NameOrId
    {
        private readonly int _id;
        private readonly string _name;

        public NameOrId(string name, int id)
        {
            _name = name;
            _id = id;
        }

        public bool Match(Data candidate)
        {
            return candidate.Name == _name || candidate.Id == _id;
        }
    }

    /*
		 * XXX: what to do?
		class TruePredicate
		{
			public bool Match(Data candidate)
			{
				return true;
			}
		}

		class FalsePredicate
		{
			public bool Match(Data candidate)
			{
				return false;
			}
		}*/

    internal class PreviousIdGreaterOrEqual
    {
        private readonly int _id;

        public PreviousIdGreaterOrEqual(int id)
        {
            _id = id;
        }

        public bool Match(Data candidate)
        {
            return candidate.HasPrevious && candidate.previous.id >= _id;
        }
    }

    internal class HasPreviousWithName
    {
        private readonly string _name;

        public HasPreviousWithName(string name)
        {
            _name = name;
        }

        public bool Match(Data candidate)
        {
            return candidate.HasPrevious && candidate.previous.name == _name;
        }
    }

    internal class GetterHasPreviousWithName
    {
        private readonly string _name;

        public GetterHasPreviousWithName(string name)
        {
            _name = name;
        }

        public bool Match(Data candidate)
        {
            return candidate.HasPrevious && candidate.Previous.name == _name;
        }
    }

    internal class GetterGetterHasPreviousWithName
    {
        private readonly string _name;

        public GetterGetterHasPreviousWithName(string name)
        {
            _name = name;
        }

        public bool Match(Data candidate)
        {
            return candidate.HasPrevious && candidate.Previous.Name == _name;
        }
    }

    internal class FieldGetterHasPreviousWithName
    {
        private readonly string _name;

        public FieldGetterHasPreviousWithName(string name)
        {
            _name = name;
        }

        public bool Match(Data candidate)
        {
            return candidate.HasPrevious && candidate.previous.Name == _name;
        }
    }

    internal class IdGreaterAndNameEqual
    {
        public bool Match(Data candidate)
        {
            return (candidate.id > 1) && candidate.Name == "Cc";
        }
    }

    internal class IdDisjunction
    {
        public bool Match(Data candidate)
        {
            return (candidate.id <= 1) || (candidate.Id >= 3);
        }
    }

    internal class IdRange
    {
        private readonly int _begin;
        private readonly int _end;

        public IdRange(int begin, int end)
        {
            _begin = begin;
            _end = end;
        }

        public bool Match(Data candidate)
        {
            return (candidate.id >= _begin) && (candidate.Id <= _end);
        }
    }

    internal class IdValidRange
    {
        public bool Match(Data candidate)
        {
            return (candidate.id > 1) && (candidate.Id <= 2);
        }
    }

    internal class IdInvalidRange
    {
        public bool Match(Data candidate)
        {
            return (candidate.id > 1) && (candidate.Id < 1);
        }
    }

    internal class HasPreviousWithPrevious
    {
        public bool Match(Data candidate)
        {
            return candidate.HasPrevious && candidate.Previous.HasPrevious;
        }
    }

    internal class NestedOr
    {
        public bool Match(Data candidate)
        {
            return ((candidate.id >= 1) || candidate.Name == "Cc")
                   && candidate.id < 3;
        }
    }

    internal class NestedAnd
    {
        public bool Match(Data candidate)
        {
            return (candidate.id == 1 && candidate.Name == "Bb")
                   || (candidate.id == 3 && candidate.HasPrevious && candidate.Previous.Id == 2);
        }
    }

    internal class Identity
    {
        private readonly Data _identity;

        public Identity(Data identity_)
        {
            _identity = identity_;
        }

        public bool Match(Data candidate)
        {
            return candidate == _identity;
        }
    }
}