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

#if !SILVERLIGHT
using System.Collections;
using Db4objects.Db4o.Qlin;
using Db4oUnit;
using Sharpen;

namespace Db4objects.Db4o.Tests.Common.Qlin
{
    /// <summary>
    ///     Syntax and implementation of QLin were inspired by:
    ///     http://www.h2database.com/html/jaqu.html
    /// </summary>
    public class BasicQLinTestCase
    {
        private IQLinable Db()
        {
            // disabled for now, we removed QLinable from the 8.0 ObjectContainer interface
            return null;
        }

        private void StoreAll(IList expected)
        {
            for (var objIter = expected.GetEnumerator(); objIter.MoveNext();)
            {
                var obj = objIter.Current;
            }
        }

        // store(obj);
        public virtual void TestFromSelect()
        {
            StoreAll(OccamAndZora());
            IteratorAssert.SameContent(OccamAndZora(), Db().From(typeof (Cat
                )).Select());
        }

        public virtual void TestWhereFieldNameAsString()
        {
            StoreAll(OccamAndZora());
            IteratorAssert.SameContent(Occam(), Db().From(typeof (Cat)).Where
                ("name").Equal("Occam").Select());
        }

        public virtual void TestWherePrototypeFieldIsString()
        {
            StoreAll(OccamAndZora());
            IteratorAssert.SameContent(Occam(), Db().From(typeof (Cat)).Where
                (((Cat) QLinSupport.P(typeof (Cat))).Name()).Equal
                ("Occam").Select());
        }

        public virtual void TestWherePrototypeFieldStartsWith()
        {
            StoreAll(OccamAndZora());
            IteratorAssert.SameContent(Occam(), Db().From(typeof (Cat)).Where
                (((Cat) QLinSupport.P(typeof (Cat))).Name()).StartsWith
                ("Occ").Select());
        }

        public virtual void TestField()
        {
            StoreAll(OccamAndZora());
            IteratorAssert.SameContent(Occam(), Db().From(typeof (Cat)).Where
                (QLinSupport.Field("name")).Equal("Occam").Select());
        }

        public virtual void TestWherePrototypeFieldIsPrimitiveInt()
        {
            StoreAll(OccamAndZora());
            IteratorAssert.SameContent(Occam(), Db().From(typeof (Cat)).Where
                (((Cat) QLinSupport.P(typeof (Cat))).age).Equal
                (7).Select());
        }

        public virtual void TestWherePrototypeFieldIsSmaller()
        {
            StoreAll(OccamAndZora());
            IteratorAssert.SameContent(Zora(), Db().From(typeof (Cat)).Where
                (((Cat) QLinSupport.P(typeof (Cat))).age).Smaller
                (7).Select());
        }

        public virtual void TestWherePrototypeFieldIsGreater()
        {
            StoreAll(OccamAndZora());
            IteratorAssert.SameContent(OccamAndZora(), Db().From(typeof (Cat
                )).Where(((Cat) QLinSupport.P(typeof (Cat))).age
                ).Greater(5).Select());
        }

        public virtual void TestLimit()
        {
            StoreAll(OccamAndZora());
            Assert.AreEqual(1, Db().From(typeof (Cat)).Limit(1).Select().Count
                );
        }

        public virtual void TestPredefinedPrototype()
        {
            StoreAll(OccamAndZora());
            var cat = ((Cat) QLinSupport.Prototype(typeof (
                Cat)));
            IteratorAssert.SameContent(Occam(), Db().From(typeof (Cat)).Where
                (cat.Name()).StartsWith("Occ").Select());
        }

        public virtual void TestQueryingByInterface()
        {
            StoreAll(OccamAndIsetta());
            var dog = ((Dog) QLinSupport.Prototype(typeof (
                Dog)));
            var cat = ((Cat) QLinSupport.Prototype(typeof (
                Cat)));
            AssertQuery(Isetta(), dog, "Isetta");
            AssertQuery(Occam(), cat, "Occam");
        }

        public virtual void TestTwoLevelField()
        {
            StoreAll(OccamZoraAchatAcrobat());
        }

        public virtual void TestWhereAsNativeQuery()
        {
            StoreAll(OccamAndZora());
            var cat = ((Cat) QLinSupport.Prototype(typeof (
                Cat)));
        }

        //		IteratorAssert.sameContent(occam(),
        //			db().from(Cat.class)
        //				.where(cat.name().equals("Occam"))
        //				.select());
        public virtual void TestUpdate()
        {
            StoreAll(OccamZoraAchatAcrobat());
            var newAge = 2;
            var cat = ((Cat) QLinSupport.Prototype(typeof (
                Cat)));
            //		db().from(Cat.class)
            //		   .where(cat.father()).equal("Occam")
            //		   .update(cat.age(newAge));
            var updated = Db().From(typeof (Cat)).Where(cat.Name()).Equal
                ("Occam").Select();
            var i = updated.GetEnumerator();
        }

        //		while(i.hasNext()){
        //			Assert.areEqual(newAge, i.next().age());
        //		}
        public virtual void TestExecute()
        {
            StoreAll(OccamZoraAchatAcrobat());
            var cat = ((Cat) QLinSupport.Prototype(typeof (
                Cat)));
        }

        //		db().from(Cat.class)
        //		  .where(cat.name()).startsWith("Zor")
        //		  .execute(cat.feed());
        private IList OccamZoraAchatAcrobat()
        {
            return Family(new Cat("Occam", 7), new Cat("Zora"
                , 6), new[]
                {
                    new Cat("Achat", 1), new Cat
                        ("Acrobat", 1)
                });
        }

        private IList Family(Cat father, Cat mother,
            Cat[] children)
        {
            IList list = new ArrayList();
            list.Add(father);
            list.Add(mother);
            for (var childIndex = 0; childIndex < children.Length; ++childIndex)
            {
                var child = children[childIndex];
                child.father = father;
                child.mother = mother;
                father.children.Add(child);
                mother.children.Add(child);
            }
            father.Spouse(mother);
            return list;
        }

        public virtual void AssertQuery(IList expected, IPet pet, string
            name)
        {
            IteratorAssert.SameContent(expected, Db().From(pet.GetType()).Where(pet.Name()).Equal
                (name).Select());
        }

        private IList OccamAndZora()
        {
            IList list = new ArrayList();
            var occam = new Cat("Occam", 7);
            var zora = new Cat("Zora", 6);
            occam.Spouse(zora);
            list.Add(occam);
            list.Add(zora);
            return list;
        }

        private IList Occam()
        {
            return SingleCat("Occam");
        }

        private IList Zora()
        {
            return SingleCat("Zora");
        }

        private IList Isetta()
        {
            return SingleDog("Isetta");
        }

        private IList OccamAndIsetta()
        {
            IList list = new ArrayList();
            list.Add(new Cat("Occam"));
            list.Add(new Dog("Isetta"));
            return list;
        }

        private IList SingleCat(string name)
        {
            IList list = new ArrayList();
            list.Add(new Cat(name));
            return list;
        }

        private IList SingleDog(string name)
        {
            IList list = new ArrayList();
            list.Add(new Dog(name));
            return list;
        }

        public class Cat : IPet
        {
            public int age;
            public IList children = new ArrayList();
            public Cat father;
            public Cat mother;
            public string name;
            public Cat spouse;

            public Cat()
            {
            }

            public Cat(string name)
            {
                this.name = name;
            }

            public Cat(string name, int age) : this(name)
            {
                this.age = age;
            }

            public virtual string Name()
            {
                return name;
            }

            public virtual void Spouse(Cat spouse)
            {
                this.spouse = spouse;
                spouse.spouse = this;
            }

            public virtual Cat Father()
            {
                return father;
            }

            public virtual Cat Mother()
            {
                return mother;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Cat))
                {
                    return false;
                }
                var other = (Cat) obj;
                if (name == null)
                {
                    return other.name == null;
                }
                return name.Equals(other.name);
            }

            public virtual int Age()
            {
                return age;
            }

            public virtual void Age(int newAge)
            {
                age = newAge;
            }

            public virtual void Feed()
            {
                Runtime.Out.WriteLine(name + ": 'Thanks for all the fish.'");
            }
        }

        public class Dog : IPet
        {
            private readonly string _name;

            public Dog()
            {
            }

            public Dog(string name)
            {
                _name = name;
            }

            public virtual string Name()
            {
                return _name;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Dog))
                {
                    return false;
                }
                var other = (Dog) obj;
                if (_name == null)
                {
                    return other._name == null;
                }
                return _name.Equals(other._name);
            }
        }

        public interface IPet
        {
            string Name();
        }
    }
}

#endif // !SILVERLIGHT