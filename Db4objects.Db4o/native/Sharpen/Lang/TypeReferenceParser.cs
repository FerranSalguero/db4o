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
using System.Globalization;
using System.Reflection;

namespace Sharpen.Lang
{
    internal class TypeReferenceParser
    {
        private readonly TypeReferenceLexer _lexer;
        private readonly Stack<Token> _stack = new Stack<Token>();

        public TypeReferenceParser(string input)
        {
            _lexer = new TypeReferenceLexer(input);
        }

        public TypeReference Parse()
        {
            var str = ParseSimpleTypeReference();
            var returnValue = ParseQualifiedTypeReference(str);
            var token = NextToken();
            if (null != token)
            {
                switch (token.Kind)
                {
                    case TokenKind.Comma:
                        str.SetAssemblyName(ParseAssemblyName());
                        break;
                    default:
                        UnexpectedToken(TokenKind.Comma, token);
                        break;
                }
            }
            return returnValue;
        }

        private TypeReference ParseQualifiedTypeReference(TypeReference elementType)
        {
            var returnValue = elementType;

            Token token;
            while (null != (token = NextToken()))
            {
                switch (token.Kind)
                {
                    case TokenKind.LBrack:
                        returnValue = ParseArrayTypeReference(returnValue);
                        break;
                    case TokenKind.PointerQualifier:
                        returnValue = new PointerTypeReference(returnValue);
                        break;
                    default:
                        Push(token);
                        return returnValue;
                }
            }

            return returnValue;
        }

        private TypeReference ParseArrayTypeReference(TypeReference str)
        {
            var rank = 1;
            var token = NextToken();
            while (null != token && token.Kind == TokenKind.Comma)
            {
                ++rank;
                token = NextToken();
            }
            AssertTokenKind(TokenKind.RBrack, token);

            return new ArrayTypeReference(str, rank);
        }

        private SimpleTypeReference ParseSimpleTypeReference()
        {
            var id = Expect(TokenKind.Id);

            var t = NextToken();
            if (null == t) return new SimpleTypeReference(id.Value);

            while (TokenKind.NestedQualifier == t.Kind)
            {
                var nestedId = Expect(TokenKind.Id);
                id.Value += "+" + nestedId.Value;

                t = NextToken();
                if (null == t) return new SimpleTypeReference(id.Value);
            }

            if (t.Kind == TokenKind.GenericQualifier)
            {
                return ParseGenericTypeReference(id);
            }

            Push(t);
            return new SimpleTypeReference(id.Value);
        }

        private SimpleTypeReference ParseGenericTypeReference(Token id)
        {
            return InternalParseGenericTypeReference(id, 0);
        }

        private SimpleTypeReference InternalParseGenericTypeReference(Token id, int count)
        {
            var argcToken = Expect(TokenKind.Number);
            id.Value += "`" + argcToken.Value;

            var argc = int.Parse(argcToken.Value);

            var t = NextToken();
            while (TokenKind.NestedQualifier == t.Kind)
            {
                var nestedId = Expect(TokenKind.Id);
                id.Value += "+" + nestedId.Value;

                t = NextToken();
            }

            if (IsInnerGenericTypeReference(t))
            {
                return InternalParseGenericTypeReference(id, argc + count);
            }

            var args = new TypeReference[0];
            if (!IsOpenGenericTypeDefinition(t))
            {
                args = new TypeReference[argc + count];
                AssertTokenKind(TokenKind.LBrack, t);
                for (var i = 0; i < args.Length; ++i)
                {
                    if (i > 0) Expect(TokenKind.Comma);
                    Expect(TokenKind.LBrack);
                    args[i] = Parse();
                    Expect(TokenKind.RBrack);
                }
                Expect(TokenKind.RBrack);
            }
            else
            {
                Push(t);
            }

            return new GenericTypeReference(id.Value, args);
        }

        private static bool IsOpenGenericTypeDefinition(Token t)
        {
            return t.Kind != TokenKind.LBrack;
        }

        private static bool IsInnerGenericTypeReference(Token t)
        {
            return TokenKind.GenericQualifier == t.Kind;
        }

        public AssemblyName ParseAssemblyName()
        {
            var simpleName = _lexer.SimpleName();

            var assemblyName = new AssemblyName();
            assemblyName.Name = simpleName.Value;

            if (!CommaIdEquals()) return assemblyName;

            var version = Expect(TokenKind.VersionNumber);
            assemblyName.Version = new Version(version.Value);

            if (!CommaIdEquals()) return assemblyName;

            var culture = Expect(TokenKind.Id);
            if ("neutral" == culture.Value)
            {
                assemblyName.CultureInfo = CultureInfo.InvariantCulture;
            }
            else
            {
#if SILVERLIGHT
                assemblyName.CultureInfo = CultureInfo.InvariantCulture;
#else
                assemblyName.CultureInfo = CultureInfo.CreateSpecificCulture(culture.Value);
#endif
            }

            if (!CommaIdEquals()) return assemblyName;

            var token = NextToken();
            if ("null" != token.Value)
            {
                assemblyName.SetPublicKeyToken(ParsePublicKeyToken(token.Value));
            }

            return assemblyName;
        }

        private static byte[] ParsePublicKeyToken(string token)
        {
            var len = token.Length/2;
            var bytes = new byte[len];
            for (var i = 0; i < len; ++i)
            {
                bytes[i] = byte.Parse(token.Substring(i*2, 2), NumberStyles.HexNumber);
            }
            return bytes;
        }

        private bool CommaIdEquals()
        {
            var token = NextToken();
            if (null == token) return false;
            if (token.Kind != TokenKind.Comma)
            {
                Push(token);
                return false;
            }

            AssertTokenKind(TokenKind.Comma, token);
            Expect(TokenKind.Id);
            Expect(TokenKind.Equals);
            return true;
        }

        private Token Expect(TokenKind expected)
        {
            var actual = NextToken();
            AssertTokenKind(expected, actual);
            return actual;
        }

        private static void AssertTokenKind(TokenKind expected, Token actual)
        {
            if (null == actual || actual.Kind != expected)
            {
                UnexpectedToken(expected, actual);
            }
        }

        private static void UnexpectedToken(TokenKind expectedKind, Token actual)
        {
            throw new ArgumentException(string.Format("Unexpected Token: '{0}' (Expected kind: '{1}')", actual,
                expectedKind));
        }

        private void Push(Token token)
        {
            _stack.Push(token);
        }

        private Token NextToken()
        {
            return _stack.Count > 0
                ? _stack.Pop()
                : _lexer.NextToken();
        }
    }
}