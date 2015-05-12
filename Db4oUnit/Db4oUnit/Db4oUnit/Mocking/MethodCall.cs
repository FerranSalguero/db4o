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
using Db4objects.Db4o.Foundation;

namespace Db4oUnit.Mocking
{
    public class MethodCall
    {
        public static readonly object IgnoredArgument = new _object_9();
        public readonly object[] args;
        public readonly string methodName;

        public MethodCall(string methodName, object[] args)
        {
            this.methodName = methodName;
            this.args = args;
        }

        public override string ToString()
        {
            return methodName + "(" + Iterators.Join(Iterators.Iterate(args), ", ") + ")";
        }

        public override bool Equals(object obj)
        {
            if (null == obj)
            {
                return false;
            }
            if (GetType() != obj.GetType())
            {
                return false;
            }
            var other = (MethodCall) obj;
            if (!methodName.Equals(other.methodName))
            {
                return false;
            }
            if (args.Length != other.args.Length)
            {
                return false;
            }
            for (var i = 0; i < args.Length; ++i)
            {
                var expectedArg = args[i];
                if (expectedArg == IgnoredArgument)
                {
                    continue;
                }
                var actualArg = other.args[i];
                if (expectedArg is IArgumentCondition)
                {
                    ((IArgumentCondition) expectedArg).Verify(actualArg);
                    continue;
                }
                if (!Check.ObjectsAreEqual(expectedArg, actualArg))
                {
                    return false;
                }
            }
            return true;
        }

        private sealed class _object_9 : object
        {
            public override string ToString()
            {
                return "...";
            }
        }

        public interface IArgumentCondition
        {
            void Verify(object argument);
        }

        public class Conditions
        {
            public static IArgumentCondition IsA(Type expectedClass)
            {
                return new _IArgumentCondition_21(expectedClass);
            }

            private sealed class _IArgumentCondition_21 : IArgumentCondition
            {
                private readonly Type expectedClass;

                public _IArgumentCondition_21(Type expectedClass)
                {
                    this.expectedClass = expectedClass;
                }

                public void Verify(object argument)
                {
                    Assert.IsInstanceOf(expectedClass, argument);
                }
            }
        }
    }
}