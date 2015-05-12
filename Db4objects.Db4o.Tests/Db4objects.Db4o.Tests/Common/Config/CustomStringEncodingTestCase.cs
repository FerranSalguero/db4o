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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Config.Encoding;
using Db4objects.Db4o.Internal.Encoding;
using Sharpen;

namespace Db4objects.Db4o.Tests.Common.Config
{
    public class CustomStringEncodingTestCase : StringEncodingTestCaseBase
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.StringEncoding(new _IStringEncoding_12());
        }

        protected override Type StringIoClass()
        {
            return typeof (DelegatingStringIO);
        }

        private sealed class _IStringEncoding_12 : IStringEncoding
        {
            public byte[] Encode(string str)
            {
                var length = str.Length;
                var chars = new char[length];
                Runtime.GetCharsForString(str, 0, length, chars, 0);
                var bytes = new byte[length*4];
                var count = 0;
                for (var i = 0; i < length; i++)
                {
                    bytes[count++] = (byte) (chars[i] & unchecked(0xff));
                    bytes[count++] = (byte) (chars[i] >> 8);
                    bytes[count++] = (byte) i;
                    // bogus bytes, just for testing
                    bytes[count++] = (byte) i;
                }
                return bytes;
            }

            public string Decode(byte[] bytes, int start, int length)
            {
                var stringLength = length/4;
                var chars = new char[stringLength];
                var j = start;
                for (var ii = 0; ii < stringLength; ii++)
                {
                    chars[ii] = (char) ((bytes[j++] & unchecked(0xff)) | ((bytes[j++] & unchecked(
                        0xff)) << 8));
                    j += 2;
                }
                return new string(chars, 0, stringLength);
            }
        }
    }
}