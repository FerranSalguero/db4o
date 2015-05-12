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

using System.Collections;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;

namespace Db4objects.Db4o.CS.Internal.Messages
{
    /// <summary>Messages with Data for Client/Server Communication</summary>
    public class MsgD : Msg
    {
        internal StatefulBuffer _payLoad;

        internal MsgD()
        {
        }

        internal MsgD(string aName) : base(aName)
        {
        }

        public override ByteArrayBuffer GetByteLoad()
        {
            return _payLoad;
        }

        public override sealed StatefulBuffer PayLoad()
        {
            return _payLoad;
        }

        public virtual void PayLoad(StatefulBuffer writer)
        {
            _payLoad = writer;
        }

        public MsgD GetWriterForByte(Transaction trans
            , byte b)
        {
            var msg = GetWriterForLength(trans, 1);
            msg._payLoad.WriteByte(b);
            return msg;
        }

        public MsgD GetWriterForBuffer(Transaction trans
            , ByteArrayBuffer buffer)
        {
            var writer = GetWriterForLength(trans, buffer
                .Length());
            writer.WriteBytes(buffer._buffer);
            return writer;
        }

        public MsgD GetWriterForLength(Transaction trans
            , int length)
        {
            var message = (MsgD
                ) PublicClone();
            message.SetTransaction(trans);
            message._payLoad = new StatefulBuffer(trans, length + Const4.MessageLength);
            message.WriteInt(_msgID);
            message.WriteInt(length);
            if (trans.ParentTransaction() == null)
            {
                message._payLoad.WriteByte(Const4.SystemTrans);
            }
            else
            {
                message._payLoad.WriteByte(Const4.UserTrans);
            }
            return message;
        }

        public MsgD GetWriter(Transaction trans)
        {
            return GetWriterForLength(trans, 0);
        }

        public MsgD GetWriterForInts(Transaction trans
            , int[] ints)
        {
            var message = GetWriterForLength(trans, Const4
                .IntLength*ints.Length);
            for (var i = 0; i < ints.Length; i++)
            {
                message.WriteInt(ints[i]);
            }
            return message;
        }

        public MsgD GetWriterForIntArray(Transaction
            a_trans, int[] ints, int length)
        {
            return GetWriterForIntSequence(a_trans, length, IntIterators.ForInts(ints, length
                ));
        }

        public virtual MsgD GetWriterForIntSequence(
            Transaction trans, int length, IEnumerator iterator)
        {
            var message = GetWriterForLength(trans, Const4
                .IntLength*(length + 1));
            message.WriteInt(length);
            while (iterator.MoveNext())
            {
                message.WriteInt(((int) iterator.Current));
            }
            return message;
        }

        public MsgD GetWriterForInt(Transaction a_trans
            , int id)
        {
            var message = GetWriterForLength(a_trans, Const4
                .IntLength);
            message.WriteInt(id);
            return message;
        }

        public MsgD GetWriterForIntString(Transaction
            a_trans, int anInt, string str)
        {
            var message = GetWriterForLength(a_trans, Const4
                .stringIO.Length(str) + Const4.IntLength*2);
            message.WriteInt(anInt);
            message.WriteString(str);
            return message;
        }

        public MsgD GetWriterForLong(Transaction a_trans
            , long a_long)
        {
            var message = GetWriterForLength(a_trans, Const4
                .LongLength);
            message.WriteLong(a_long);
            return message;
        }

        public MsgD GetWriterForLongs(Transaction trans
            , long[] longs)
        {
            var message = GetWriterForLength(trans, Const4
                .LongLength*longs.Length);
            for (var i = 0; i < longs.Length; i++)
            {
                message.WriteLong(longs[i]);
            }
            return message;
        }

        public virtual MsgD GetWriterForSingleObject
            (Transaction trans, object obj)
        {
            var serialized = Serializer.Marshall(trans.Container(), obj);
            var msg = GetWriterForLength(trans, serialized
                .MarshalledLength());
            serialized.Write(msg._payLoad);
            return msg;
        }

        public MsgD GetWriterForString(Transaction a_trans
            , string str)
        {
            var message = GetWriterForLength(a_trans, Const4
                .stringIO.Length(str) + Const4.IntLength);
            message.WriteString(str);
            return message;
        }

        public virtual MsgD GetWriter(StatefulBuffer
            bytes)
        {
            var message = GetWriterForLength(bytes.Transaction
                (), bytes.Length());
            message._payLoad.Append(bytes._buffer);
            return message;
        }

        public virtual byte[] ReadBytes()
        {
            return _payLoad.ReadBytes(ReadInt());
        }

        public int ReadInt()
        {
            return _payLoad.ReadInt();
        }

        public long ReadLong()
        {
            return _payLoad.ReadLong();
        }

        public bool ReadBoolean()
        {
            return _payLoad.ReadByte() != 0;
        }

        public virtual object ReadObjectFromPayLoad()
        {
            return Serializer.Unmarshall(Container(), _payLoad);
        }

        internal override sealed Msg ReadPayLoad(IMessageDispatcher messageDispatcher, Transaction
            a_trans, Socket4Adapter sock, ByteArrayBuffer reader)
        {
            var length = reader.ReadInt();
            a_trans = CheckParentTransaction(a_trans, reader);
            var command = (MsgD
                ) PublicClone();
            command.SetTransaction(a_trans);
            command.SetMessageDispatcher(messageDispatcher);
            command._payLoad = ReadMessageBuffer(a_trans, sock, length);
            return command;
        }

        public string ReadString()
        {
            var length = ReadInt();
            return Const4.stringIO.Read(_payLoad, length);
        }

        public virtual object ReadSingleObject()
        {
            return Serializer.Unmarshall(Container(), SerializedGraph.Read(_payLoad));
        }

        public void WriteBytes(byte[] aBytes)
        {
            _payLoad.Append(aBytes);
        }

        public void WriteInt(int aInt)
        {
            _payLoad.WriteInt(aInt);
        }

        public void WriteLong(long l)
        {
            _payLoad.WriteLong(l);
        }

        public void WriteString(string aStr)
        {
            _payLoad.WriteInt(aStr.Length);
            Const4.stringIO.Write(_payLoad, aStr);
        }
    }
}