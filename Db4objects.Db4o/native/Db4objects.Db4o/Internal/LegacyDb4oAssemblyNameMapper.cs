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

using System.Text;
using Db4objects.Db4o.Internal.Encoding;

namespace Db4objects.Db4o.Internal
{
    internal class LegacyDb4oAssemblyNameMapper
    {
        private static readonly string[] oldAssemblyNames = {"db4o-4.0-net1", "db4o-4.0-compact1"};
        private static readonly byte[][] oldAssemblies;

        private static readonly string[][] NamespaceRenamings =
        {
            new[] {"com.db4o.ext", "Db4objects.Db4o.Ext"},
            new[] {"com.db4o.inside", "Db4objects.Db4o.Internal"},
            new[] {"com.db4o", "Db4objects.Db4o"}
        };

        static LegacyDb4oAssemblyNameMapper()
        {
            LatinStringIO stringIO = new UnicodeStringIO();
            oldAssemblies = new byte[oldAssemblyNames.Length][];
            for (var i = 0; i < oldAssemblyNames.Length; i++)
            {
                oldAssemblies[i] = stringIO.Write(oldAssemblyNames[i]);
            }
        }

        internal byte[] MappedNameFor(byte[] nameBytes)
        {
            for (var i = 0; i < oldAssemblyNames.Length; i++)
            {
                var assemblyName = oldAssemblies[i];

                var j = assemblyName.Length - 1;
                for (var k = nameBytes.Length - 1; k >= 0; k--)
                {
                    if (nameBytes[k] != assemblyName[j])
                    {
                        break;
                    }
                    j--;
                    if (j < 0)
                    {
                        return UpdateInternalClassName(nameBytes, i);
                    }
                }
            }
            return nameBytes;
        }

        private static byte[] UpdateInternalClassName(byte[] bytes, int candidateMatchingAssemblyIndex)
        {
            var io = new UnicodeStringIO();
            var typeFQN = io.Read(bytes);

            var assemblyNameParts = typeFQN.Split(',');
            if (assemblyNameParts[1].Trim() != oldAssemblyNames[candidateMatchingAssemblyIndex])
            {
                return bytes;
            }

            var typeName = assemblyNameParts[0];
            return io.Write(FullyQualifiedNameFor(typeName).ToString());
        }

        private static StringBuilder FullyQualifiedNameFor(string typeName)
        {
            var typeNameBuffer = new StringBuilder(typeName);
            ApplyNameSpaceRenamings(typeNameBuffer);
            typeNameBuffer.Append(", ");
            typeNameBuffer.Append(GetCurrentAssemblyName());
            return typeNameBuffer;
        }

        private static void ApplyNameSpaceRenamings(StringBuilder typeNameBuffer)
        {
            foreach (var renaming in NamespaceRenamings)
            {
                typeNameBuffer.Replace(renaming[0], renaming[1]);
            }
        }

        private static string GetCurrentAssemblyName()
        {
            return typeof (Platform4).Assembly.GetName().Name;
        }
    }
}