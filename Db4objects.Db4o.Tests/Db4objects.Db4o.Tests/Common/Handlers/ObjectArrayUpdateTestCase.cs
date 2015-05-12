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

using Db4objects.Db4o.Ext;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Handlers
{
    public class ObjectArrayUpdateTestCase : HandlerUpdateTestCaseBase
    {
        private static readonly ParentItem[] childData =
        {
            new ChildItem("one"), new ChildItem
                ("two"),
            null
        };

        private static readonly ParentItem[] mixedData =
        {
            new ParentItem("one"), new ChildItem
                ("two"),
            new ChildItem("three"), null
        };

        protected override object CreateArrays()
        {
            var item = new ItemArrays
                ();
            item._typedChildren = CastToChildItemArray(childData);
            item._typedChildrenInParentArray = childData;
            item._untypedChildren = CastToChildItemArray(childData);
            item._untypedChildrenInParentArray = childData;
            item._untypedChildrenInObject = CastToChildItemArray(childData);
            item._untypedChildrenInParentArrayInObject = childData;
            item._typedMixed = mixedData;
            item._untypedMixed = mixedData;
            item._untypedMixedInObject = mixedData;
            return item;
        }

        protected override void AssertArrays(IExtObjectContainer objectContainer, object
            obj)
        {
            var item = (ItemArrays
                ) obj;
            ArrayAssert.AreEqual(CastToChildItemArray(childData), item._typedChildren);
            ArrayAssert.AreEqual(childData, item._typedChildrenInParentArray);
            ArrayAssert.AreEqual(CastToChildItemArray(childData), item._untypedChildren);
            ArrayAssert.AreEqual(childData, item._untypedChildrenInParentArray);
            ArrayAssert.AreEqual(CastToChildItemArray(childData), (object[]) item._untypedChildrenInObject
                );
            ArrayAssert.AreEqual(childData, (object[]) item._untypedChildrenInParentArrayInObject
                );
            ArrayAssert.AreEqual(mixedData, item._typedMixed);
            ArrayAssert.AreEqual(mixedData, item._untypedMixed);
            ArrayAssert.AreEqual(mixedData, (object[]) item._untypedMixedInObject);
        }

        private ChildItem[] CastToChildItemArray(ParentItem
            [] array)
        {
            var res = new ChildItem
                [array.Length];
            for (var i = 0; i < res.Length; i++)
            {
                res[i] = (ChildItem) array[i];
            }
            return res;
        }

        protected override object[] CreateValues()
        {
            // not used
            return null;
        }

        protected override void AssertValues(IExtObjectContainer objectContainer, object[]
            values)
        {
        }

        // not used
        protected override string TypeName()
        {
            return "object-array";
        }

        public class ItemArrays
        {
            public ChildItem[] _typedChildren;
            public ParentItem[] _typedChildrenInParentArray;
            public ParentItem[] _typedMixed;
            public object[] _untypedChildren;
            public object _untypedChildrenInObject;
            public object[] _untypedChildrenInParentArray;
            public object _untypedChildrenInParentArrayInObject;
            public object[] _untypedMixed;
            public object _untypedMixedInObject;
        }

        public class ParentItem
        {
            public string _name;

            public ParentItem(string name)
            {
                _name = name;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is ParentItem))
                {
                    return false;
                }
                if (obj is ChildItem)
                {
                    return false;
                }
                return HasSameNameAs((ParentItem) obj);
            }

            protected virtual bool HasSameNameAs(ParentItem other)
            {
                if (_name == null)
                {
                    return other._name == null;
                }
                return _name.Equals(other._name);
            }
        }

        public class ChildItem : ParentItem
        {
            public ChildItem(string name) : base(name)
            {
            }

            public override bool Equals(object obj)
            {
                if (!(obj is ChildItem))
                {
                    return false;
                }
                return HasSameNameAs((ParentItem) obj);
            }
        }
    }
}