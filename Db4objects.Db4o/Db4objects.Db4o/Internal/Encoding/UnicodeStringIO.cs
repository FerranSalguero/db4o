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

using Db4objects.Db4o.Marshall;
using Sharpen;

namespace Db4objects.Db4o.Internal.Encoding
{
    /// <exclude></exclude>
    public sealed class UnicodeStringIO : LatinStringIO
    {
        protected override int BytesPerChar()
        {
            return 2;
        }

        public override byte EncodingByte()
        {
            return BuiltInStringEncoding.EncodingByteForEncoding(new UnicodeStringEncoding());
        }

        public override int Length(string str)
        {
            return (str.Length*2) + Const4.ObjectLength + Const4.IntLength;
        }

        public override string Read(IReadBuffer buffer, int length)
        {
            var chars = new char[length];
            for (var ii = 0; ii < length; ii++)
            {
                chars[ii] = (char) ((buffer.ReadByte() & unchecked(0xff)) | ((buffer.ReadByte
                    () & unchecked(0xff)) << 8));
            }
            return new string(chars, 0, length);
        }

        public override string Read(byte[] bytes)
        {
            var length = bytes.Length/2;
            var chars = new char[length];
            var j = 0;
            for (var ii = 0; ii < length; ii++)
            {
                chars[ii] = (char) ((bytes[j++] & unchecked(0xff)) | ((bytes[j++] & unchecked(
                    0xff)) << 8));
            }
            return new string(chars, 0, length);
        }

        public override int ShortLength(string str)
        {
            return (str.Length*2) + Const4.IntLength;
        }

        public override void Write(IWriteBuffer buffer, string str)
        {
            var length = str.Length;
            var chars = new char[length];
            Runtime.GetCharsForString(str, 0, length, chars, 0);
            for (var i = 0; i < length; i++)
            {
                buffer.WriteByte((byte) (chars[i] & unchecked(0xff)));
                buffer.WriteByte((byte) (chars[i] >> 8));
            }
        }

        public override byte[] Write(string str)
        {
            var length = str.Length;
            var chars = new char[length];
            Runtime.GetCharsForString(str, 0, length, chars, 0);
            var bytes = new byte[length*2];
            var j = 0;
            for (var i = 0; i < length; i++)
            {
                bytes[j++] = (byte) (chars[i] & unchecked(0xff));
                bytes[j++] = (byte) (chars[i] >> 8);
            }
            return bytes;
        }
    }
}