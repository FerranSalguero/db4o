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
using Sharpen;

namespace Db4objects.Db4o.Config
{
    /// <summary>
    ///     Wildcard Alias functionality to create aliases for packages,
    ///     namespaces or multiple similar named classes.
    /// </summary>
    /// <remarks>
    ///     Wildcard Alias functionality to create aliases for packages,
    ///     namespaces or multiple similar named classes. One single '*'
    ///     wildcard character is supported in the names.
    ///     <br /><br />See
    ///     <see cref="IAlias">IAlias</see>
    ///     for concrete examples.
    /// </remarks>
    public class WildcardAlias : IAlias
    {
        private readonly WildcardPattern _runtimePattern;
        private readonly WildcardPattern _storedPattern;

        /// <summary>
        ///     Create a WildcardAlias with two patterns, the
        ///     stored pattern and the pattern that is to be used
        ///     at runtime.
        /// </summary>
        /// <remarks>
        ///     Create a WildcardAlias with two patterns, the
        ///     stored pattern and the pattern that is to be used
        ///     at runtime. One single '*' is allowed as a wildcard
        ///     character.
        /// </remarks>
        public WildcardAlias(string storedPattern, string runtimePattern)
        {
            if (null == storedPattern)
            {
                throw new ArgumentNullException("storedPattern");
            }
            if (null == runtimePattern)
            {
                throw new ArgumentNullException("runtimePattern");
            }
            _storedPattern = new WildcardPattern(storedPattern);
            _runtimePattern = new WildcardPattern(runtimePattern);
        }

        /// <summary>resolving is done through simple pattern matching</summary>
        public virtual string ResolveRuntimeName(string runtimeTypeName)
        {
            return Resolve(_runtimePattern, _storedPattern, runtimeTypeName);
        }

        /// <summary>resolving is done through simple pattern matching</summary>
        public virtual string ResolveStoredName(string storedTypeName)
        {
            return Resolve(_storedPattern, _runtimePattern, storedTypeName);
        }

        private string Resolve(WildcardPattern from, WildcardPattern
            to, string typeName)
        {
            var match = from.Matches(typeName);
            return match != null ? to.Inject(match) : null;
        }

        internal class WildcardPattern
        {
            private readonly string _head;
            private readonly string _tail;

            public WildcardPattern(string pattern)
            {
                var parts = Split(pattern);
                _head = parts[0];
                _tail = parts[1];
            }

            public virtual string Inject(string s)
            {
                return _head + s + _tail;
            }

            public virtual string Matches(string s)
            {
                if (!s.StartsWith(_head) || !s.EndsWith(_tail))
                {
                    return null;
                }
                return Runtime.Substring(s, _head.Length, s.Length - _tail.Length);
            }

            private void InvalidPattern()
            {
                throw new ArgumentException("only one '*' character");
            }

            internal virtual string[] Split(string pattern)
            {
                var index = pattern.IndexOf('*');
                if (-1 == index || index != pattern.LastIndexOf('*'))
                {
                    InvalidPattern();
                }
                return new[]
                {
                    Runtime.Substring(pattern, 0, index), Runtime.Substring
                        (pattern, index + 1)
                };
            }
        }
    }
}