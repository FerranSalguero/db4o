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

using Db4objects.Db4o.Foundation;
using Sharpen;

namespace Db4objects.Db4o.Internal.Freespace
{
    /// <exclude></exclude>
    public sealed class FreeSlotNode : TreeInt
    {
        internal static int sizeLimit;
        internal FreeSlotNode _peer;

        internal FreeSlotNode(int a_key) : base(a_key)
        {
        }

        public override object ShallowClone()
        {
            var frslot = new FreeSlotNode
                (_key);
            frslot._peer = _peer;
            return ShallowCloneInternal(frslot);
        }

        internal void CreatePeer(int a_key)
        {
            _peer = new FreeSlotNode(a_key);
            _peer._peer = this;
        }

        public override bool Duplicates()
        {
            return true;
        }

        public override int OwnLength()
        {
            return Const4.IntLength*2;
        }

        internal static Tree RemoveGreaterOrEqual(FreeSlotNode
            a_in, TreeIntObject a_finder)
        {
            if (a_in == null)
            {
                return null;
            }
            var cmp = a_in._key - a_finder._key;
            if (cmp == 0)
            {
                a_finder._object = a_in;
                // the highest node in the hierarchy !!!
                return a_in.Remove();
            }
            if (cmp > 0)
            {
                a_in._preceding = RemoveGreaterOrEqual((FreeSlotNode
                    ) a_in._preceding, a_finder);
                if (a_finder._object != null)
                {
                    a_in._size--;
                    return a_in;
                }
                a_finder._object = a_in;
                return a_in.Remove();
            }
            a_in._subsequent = RemoveGreaterOrEqual((FreeSlotNode
                ) a_in._subsequent, a_finder);
            if (a_finder._object != null)
            {
                a_in._size--;
            }
            return a_in;
        }

        public override object Read(ByteArrayBuffer buffer)
        {
            var size = buffer.ReadInt();
            var address = buffer.ReadInt();
            if (size > sizeLimit)
            {
                var node = new FreeSlotNode
                    (size);
                node.CreatePeer(address);
                if (Deploy.debug && Debug4.xbytes)
                {
                    DebugCheckBuffer(buffer, node);
                }
                return node;
            }
            return null;
        }

        private void DebugCheckBuffer(ByteArrayBuffer buffer, FreeSlotNode
            node)
        {
            if (!(buffer is StatefulBuffer))
            {
                return;
            }
            var trans = ((StatefulBuffer) buffer).Transaction();
            if (!(trans.Container() is IoAdaptedObjectContainer))
            {
                return;
            }
            var checker = trans.Container().CreateStatefulBuffer(trans, node._peer
                ._key, node._key);
            checker.Read();
            for (var i = 0; i < node._key; i++)
            {
                if (checker.ReadByte() != (byte) 'X')
                {
                    Runtime.Out.WriteLine("!!! Free space corruption at:" + node._peer._key);
                    break;
                }
            }
        }

        public override void Write(ByteArrayBuffer a_writer)
        {
            // byte order: size, address
            a_writer.WriteInt(_key);
            a_writer.WriteInt(_peer._key);
        }

        // public static final void debug(FreeSlotNode a_node){
        // if(a_node == null){
        // return;
        // }
        // System.out.println("Address:" + a_node.i_key);
        // System.out.println("Length:" + a_node.i_peer.i_key);
        // debug((FreeSlotNode)a_node.i_preceding);
        // debug((FreeSlotNode)a_node.i_subsequent);
        // }
        public override string ToString()
        {
            return base.ToString();
            var str = "FreeSlotNode " + _key;
            if (_peer != null)
            {
                str += " peer: " + _peer._key;
            }
            return str;
        }
    }
}