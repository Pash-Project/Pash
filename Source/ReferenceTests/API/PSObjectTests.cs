using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using TestParameterizedProperties;

namespace ReferenceTests.API
{
// prevents the compiler from complaining about unused fields, etc
#pragma warning disable 169, 414, 0649
    interface IDummyBase
    {
        int DummyBaseProperty { get; }
    }

    interface IDummy : IDummyBase
    {

    }

    class DummyAncestor : IDummyBase
    {
        private int AncFieldPrivated;
        protected int AncFieldProtected;
        static public int AncFieldPublic;
        public string AncProperty { get; set;  }

        public int DummyBaseProperty { get { return 0; } }

        private void AncMethodPrivate() { }
        protected void AncMethodProtected() { }
        public void AncMethodPublic() { }
        public virtual void VirtualMethod() { }
    }

    class DummyDescendant : DummyAncestor
    {
        private int DescFieldPrivated;
        protected int DescFieldProtected;
        internal int DescFieldInternal;
        public int DescFieldPublic;
        public string DescProperty { get; set; }

        private void DescMethodPrivate() { }
        protected void DescMethodProtected() { }
        static public void DescMethodPublic() { }
        public override void VirtualMethod() { }
    }
#pragma warning restore 169, 414, 0649

    [TestFixture]
    public class PSObjectTests : ReferenceTestBase
    {
        private Collection<string> _dummyDescendantTypeNames = new Collection<string>() {
                typeof(DummyDescendant).FullName,
                typeof(DummyAncestor).FullName,
                typeof(Object).FullName
        };

        private IEnumerable<string> GetPublicInstanceMembers(Type type)
        {
            var members = (from member in
                       type.GetMembers(BindingFlags.Instance | BindingFlags.Public)
                   where (member.MemberType & (MemberTypes.Field | MemberTypes.Property)) != 0
                   select member.Name).ToList();
            type.GetInterfaces().ToList().ForEach(
                i => members.AddRange(
                    from prop
                    in i.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    select prop.Name
                )
            );
            return members.Distinct();
        }

        private IEnumerable<string> GetPublicInstanceMethods(Type type)
        {
            return from method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                   select method.Name;
        }

        [Test]
        public void PSObjectWrapsInstance()
        {
            var instace = new DummyDescendant();
            var psobj = PSObject.AsPSObject(instace);
            Assert.AreSame(instace, psobj.BaseObject);
            Assert.AreSame(instace, psobj.ImmediateBaseObject);
        }

        [Test]
        public void PSObjectReflectsTypeNames()
        {
            var psobj = PSObject.AsPSObject(new DummyDescendant());
            var names = psobj.TypeNames;
            Assert.AreEqual(_dummyDescendantTypeNames, names);
        }

        [Test]
        public void PSObjectReflectsProperties()
        {
            var psobj = PSObject.AsPSObject(new DummyDescendant());
            var propertyNames = from prop in psobj.Properties select prop.Name;
            var realProperyNames = GetPublicInstanceMembers(typeof(DummyDescendant));
            Assert.That(propertyNames, Is.EquivalentTo(realProperyNames));
        }

        [Test]
        public void PSObjectReflectsMethods()
        {
            var psobj = PSObject.AsPSObject(new DummyDescendant());
            var methodNames = from method in psobj.Methods select method.Name;
            var realMethodNames = GetPublicInstanceMethods(typeof(DummyDescendant));
            Assert.That(methodNames, Is.EquivalentTo(realMethodNames));
        }

        [Test]
        public void PSObjectReflectsMembers()
        {
            var psobj = PSObject.AsPSObject(new DummyDescendant());
            var memberNames = from member in psobj.Members select member.Name;
            var type = typeof(DummyDescendant);
            var realMemberNames = GetPublicInstanceMembers(type).Concat(GetPublicInstanceMethods(type));
            Assert.That(memberNames, Is.EquivalentTo(realMemberNames));
        }

        [Test]
        public void AsPSObjectCannotWrapNull()
        {
            // TODO: check for exception type
            Assert.Throws(Is.InstanceOf(typeof(Exception)), delegate
            {
                PSObject.AsPSObject(null);
            });
        }

        [Test]
        public void PSObjectCannotConstructNull()
        {
            // TODO: check for exception type
            Assert.Throws(Is.InstanceOf(typeof(Exception)), delegate
            {
                new PSObject(null);
            });
        }

        [Test]
        [TestCase(new int[] { 1, 2 }, "1 2")]
        [TestCase(new object[] { 1, "foo" }, "1 foo")]
        [TestCase(new object[] { 1, new object[] { 1, 2 } }, "1 System.Object[]")]
        public void PSObjectToStringConvertsArrayCorrectly(object input, string expected)
        {
            Assert.AreEqual(expected, new PSObject(input).ToString());
        }

        [Test, SetCulture("de-DE")]
        public void PSObjectToStringUsesCurrentCulture()
        {
            Assert.AreEqual("1 2,5", new PSObject(new object[] { 1, 2.5 }).ToString());
        }

        [Test]
        public void ObjectWithReadOnlyParameterizedProperty()
        {
            var obj = new TestReadOnlyParameterizedProperty();
            var psObject = new PSObject(obj);

            var propertyInfo = psObject.Members.FirstOrDefault(m => m.Name == "FileNames") as PSParameterizedProperty;

            Assert.IsTrue(propertyInfo.IsGettable);
            Assert.IsFalse(propertyInfo.IsSettable);
            Assert.IsTrue(propertyInfo.IsInstance);
            Assert.AreEqual(PSMemberTypes.ParameterizedProperty, propertyInfo.MemberType);
            Assert.AreEqual("FileNames", propertyInfo.Name);
            Assert.AreEqual("System.String", propertyInfo.TypeNameOfValue);
            Assert.AreEqual(propertyInfo, propertyInfo.Value);
            Assert.AreEqual(1, propertyInfo.OverloadDefinitions.Count);
            Assert.AreEqual("string FileNames(int index) {get;}", propertyInfo.OverloadDefinitions[0]);
        }

        [Test]
        public void ObjectWithWriteOnlyParameterizedProperty()
        {
            var obj = new TestWriteOnlyParameterizedProperty();
            var psObject = new PSObject(obj);

            var propertyInfo = psObject.Members.FirstOrDefault(m => m.Name == "FileNames") as PSParameterizedProperty;

            Assert.IsFalse(propertyInfo.IsGettable);
            Assert.IsTrue(propertyInfo.IsSettable);
            Assert.AreEqual("void FileNames(int index) {set;}", propertyInfo.OverloadDefinitions[0]);
        }

        [Test]
        public void ObjectWithReadWriteOnlyParameterizedProperty()
        {
            var obj = new TestParameterizedProperty();
            var psObject = new PSObject(obj);

            var propertyInfo = psObject.Members.FirstOrDefault(m => m.Name == "FileNames") as PSParameterizedProperty;

            Assert.IsTrue(propertyInfo.IsGettable);
            Assert.IsTrue(propertyInfo.IsSettable);
            Assert.AreEqual("string FileNames(int index) {get;set;}", propertyInfo.OverloadDefinitions[0]);
        }
    }
}