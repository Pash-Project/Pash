// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.ObjectModel;
using NUnit.Framework;
using System.Management.Automation.Runspaces;
using System.Management.Automation;

namespace System.Management.Tests.HostTests
{
    [TestFixture]
    public class SessionStateVariableEntryTests
    {
        [Test]
        public void NewVariableEntryWithNameValueAndDescriptionShouldHaveOptionsOfNoneAndPublicVisibility()
        {
            var value = new Object();
            var entry = new SessionStateVariableEntry("name", value, "description");

            Assert.AreEqual(ScopedItemOptions.None, entry.Options);
            Assert.AreEqual(SessionStateEntryVisibility.Public, entry.Visibility);
            Assert.AreEqual("description", entry.Description);
            Assert.AreEqual(value, entry.Value);
            Assert.AreEqual(0, entry.Attributes.Count);
        }

        [Test]
        public void NewVariableEntryCreatedWithSingleAttributeShouldHaveSingleAttribute()
        {
            var value = new Object();
            var attribute = new CredentialAttribute();
            var entry = new SessionStateVariableEntry("name", value, "description", ScopedItemOptions.Constant, attribute);

            Assert.AreEqual(1, entry.Attributes.Count);
            Assert.AreEqual(attribute, entry.Attributes[0]);
            Assert.AreEqual(value, entry.Value);
            Assert.AreEqual("name", entry.Name);
            Assert.AreEqual("description", entry.Description);
            Assert.AreEqual(ScopedItemOptions.Constant, entry.Options);
        }
        
        [Test]
        public void NewVariableEntryCreatedWithTwoAttributesShouldHaveSingleAttributes()
        {
            var value = new Object();
            var attributes = new Collection<Attribute>();
            attributes.Add(new CredentialAttribute());
            attributes.Add(new CredentialAttribute());
            var entry = new SessionStateVariableEntry("name", value, "description", ScopedItemOptions.Constant, attributes);

            CollectionAssert.AreEqual(attributes, entry.Attributes);
            Assert.AreEqual(value, entry.Value);
            Assert.AreEqual("name", entry.Name);
            Assert.AreEqual("description", entry.Description);
            Assert.AreEqual(ScopedItemOptions.Constant, entry.Options);
        }
    }
}
