using System;
using NUnit.Framework;
using Pash.Implementation;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.Collections.Generic;
using System.Management.Automation.Language;

namespace TestHost
{
    [TestFixture]
    public class SessionStateScopeTests
    {
        public enum AvailableStates
        {
            Global,
            Script,
            Function,
            Local
        };

        private SessionState globalState;
        private SessionState scriptState;
        private SessionState functionState;
        private SessionState localState;
        private Dictionary<AvailableStates, SessionState> states;
        private CommandManager hostCommandManager;

        [SetUp]
        public void createScopes()
        {
            TestHost testHost = new TestHost(new TestHostUserInterface());
            Runspace hostRunspace = TestHost.CreateRunspace(testHost);

            globalState = hostRunspace.ExecutionContext.SessionState;
            scriptState = new SessionState(globalState);
            scriptState.IsScriptScope = true;
            functionState = new SessionState(scriptState);
            localState = new SessionState(functionState);
            states = new Dictionary<AvailableStates, SessionState>();
            states.Add(AvailableStates.Global, globalState);
            states.Add(AvailableStates.Script, scriptState);
            states.Add(AvailableStates.Function, functionState);
            states.Add(AvailableStates.Local, localState);

            hostCommandManager = new CommandManager(hostRunspace.ExecutionContext);
        }

        #region general scope related

        [TestCase("private:foo", "private", "foo")]
        [TestCase("local:foo", "local", "foo")]
        [TestCase("script:foo", "script", "foo")]
        [TestCase("global:foo", "global", "foo")]
        [TestCase("global:private:foo", "global", "private:foo")]
        [TestCase("0:foo", "", "0:foo")]
        [TestCase("1:foo", "", "1:foo")]
        [TestCase("-1:foo", "", "-1:foo")]
        [TestCase("bar:foo", "", "bar:foo")]
        public void QualifiedNameTest(string name, string specifier, string unqualifiedName)
        {
            var qualName = new SessionStateScope<PSVariable>.QualifiedName(name);
            Assert.AreEqual(specifier, qualName.ScopeSpecifier);
            Assert.AreEqual(unqualifiedName, qualName.UnqualifiedName);
        }

        #endregion

        #region variable related

        [TestCase("x", "f")] //correct x is fetched in general (from function scope)
        [TestCase("local:x", null)] //local scope has no variable x
        [TestCase("script:x", null)] //sx is private in the script scope
        [TestCase("global:x", "g")]
        [TestCase("y", "l")] //the overridden y in the local scope
        [TestCase("local:y", "l")] //also the local one, but explicitly
        [TestCase("script:y", "s")]
        [TestCase("global:y", "g")]
        [TestCase("z", "s")] //ignores the private z in function scope
        public void VariableAccessTest(string name, object expected)
        {
            globalState.PSVariable.Set(new PSVariable("x", "g"));
            globalState.PSVariable.Set(new PSVariable("y", "g"));
            scriptState.PSVariable.Set(new PSVariable("x", "s", ScopedItemOptions.Private));
            scriptState.PSVariable.Set(new PSVariable("y", "s"));
            scriptState.PSVariable.Set(new PSVariable("z", "s"));
            functionState.PSVariable.Set(new PSVariable("x", "f"));
            functionState.PSVariable.Set(new PSVariable("y", "f"));
            functionState.PSVariable.Set(new PSVariable("z", "f", ScopedItemOptions.Private));
            localState.PSVariable.Set(new PSVariable("y", "l"));
            Assert.AreEqual(expected, localState.PSVariable.GetValue(name));
        }


        [TestCase(AvailableStates.Global, "g", true)]
        [TestCase(AvailableStates.Script, "s", true)]
        [TestCase(AvailableStates.Function, "f", true)]
        [TestCase(AvailableStates.Local, "l", true)]
        [TestCase(AvailableStates.Local, "s", false)] //makes sure private setting works
        public void VariableSetTest(AvailableStates sessionState, object value, bool initLocal=true)
        {
            functionState.PSVariable.Set("private:x", "f");
            localState.PSVariable.Set("global:x", "g");
            localState.PSVariable.Set("script:x", "s");
            if (initLocal)
            {
                localState.PSVariable.Set ("local:x", "l");
            }
            Assert.AreEqual(value, states[sessionState].PSVariable.GetValue("x"));
        }

        [Test]
        public void VariableNoPrivacyChangeTest()
        {
            globalState.PSVariable.Set("private:y", "py0");
            globalState.PSVariable.Set("y", "py1");
            globalState.PSVariable.Set("x", "x0");
            globalState.PSVariable.Set("private:x", "x1");
            Assert.IsNull(scriptState.PSVariable.GetValue("y"));
            Assert.AreEqual("x1", scriptState.PSVariable.GetValue("x"));
        }

        [TestCase("global:x", AvailableStates.Global, true)]
        [TestCase("script:x", AvailableStates.Script, true)]
        [TestCase("local:x", AvailableStates.Local, true)]
        [TestCase("x", AvailableStates.Function, false)] //looks in parent scopes and removes the variable
        public void VariableRemoveTest(string variable, AvailableStates affectedState, bool initLocal)
        {
            globalState.PSVariable.Set("x", "g");
            scriptState.PSVariable.Set("x", "s");
            functionState.PSVariable.Set("x", "f");
            if (initLocal)
            {
                localState.PSVariable.Set("x", "l");
            }

            localState.PSVariable.Remove(variable);
            foreach (KeyValuePair<AvailableStates, SessionState> curState in states)
            {
                if (curState.Key == affectedState || (curState.Key == AvailableStates.Local && !initLocal))
                {
                    Assert.IsNull(curState.Value.PSVariable.Get("local:x"));
                }
                else
                {
                    Assert.IsNotNull(curState.Value.PSVariable.Get("local:x"));
                }
            }
        }

        [Test]
        public void VariableRemoveByObjectTest()
        {
            globalState.PSVariable.Set("x", "g");
            scriptState.PSVariable.Set("x", "s");
            var variable = new PSVariable("x");
            scriptState.PSVariable.Remove(variable);
            Assert.IsNull(scriptState.PSVariable.Get("local:x"));
            Assert.IsNotNull(globalState.PSVariable.Get("local:x"));
            try
            {
                scriptState.PSVariable.Remove(variable); //doesn't affect parent scopes (different to passing the name)
                Assert.True(false);
            }
            catch (ItemNotFoundException)
            {
            }
            Assert.IsNotNull(globalState.PSVariable.Get("local:x"));
        }

        #endregion

        #region drive related

        [TestCase("4", null, ExpectedException=typeof(ArgumentOutOfRangeException))]
        [TestCase("foo", null, ExpectedException=typeof(ArgumentException))]
        [TestCase("3", AvailableStates.Global)]
        [TestCase("global", AvailableStates.Global)]
        [TestCase("2", AvailableStates.Script)]
        [TestCase("script", AvailableStates.Script)]
        [TestCase("1", AvailableStates.Function)]
        [TestCase("0", AvailableStates.Local)]
        [TestCase("local", AvailableStates.Local)]
        public void DriveNewTest(string scope, AvailableStates affectedState)
        {
            PSDriveInfo info = createDrive("test");
            localState.Drive.New (info, scope);
            Assert.AreEqual(info, states[affectedState].Drive.Get(info.Name));
        }

        [Test]
        public void DriveNewExistingTest ()
        {
            PSDriveInfo info = createDrive ("test");
            globalState.Drive.New (info, "local");
            scriptState.Drive.New (info, "local"); //overriding should work of course
            try {
                scriptState.Drive.New (info, "global"); //this shouldn't work, as the global scope has that drive
                Assert.True (false);
            } catch (SessionStateException) { }
        }

        [TestCase("4", null, ExpectedException=typeof(ArgumentOutOfRangeException))]
        [TestCase("foo", null, ExpectedException=typeof(ArgumentException))]
        [TestCase("3", AvailableStates.Global)]
        [TestCase("global", AvailableStates.Global)]
        [TestCase("2", AvailableStates.Script)]
        [TestCase("script", AvailableStates.Script)]
        [TestCase("1", AvailableStates.Function)]
        [TestCase("0", AvailableStates.Local)]
        [TestCase("local", AvailableStates.Local)]
        public void DriveRemoveTest(string scope, AvailableStates affectedState)
        {
            Dictionary<AvailableStates, PSDriveInfo> driveInfos = new Dictionary<AvailableStates, PSDriveInfo>();
            foreach (var curState in states)
            {
                var info = createDrive(curState.Key.ToString());
                curState.Value.Drive.New(info, "local");
                driveInfos[curState.Key] = info;
            }
            localState.Drive.Remove(driveInfos[affectedState].Name, true, scope);
            foreach (var curState in states)
            {
                if (curState.Key == affectedState)
                {
                    Assert.AreEqual(0, curState.Value.Drive.GetAllAtScope("local").Count);
                }
                else
                {
                    Assert.AreEqual(driveInfos[curState.Key], curState.Value.Drive.Get(driveInfos[curState.Key].Name));
                }
            }
        }

        [Test]
        public void DriveRemoveNotExistingTest ()
        {
            PSDriveInfo info = createDrive ("test");
            try {
                globalState.Drive.Remove (info.Name, true, "local");
                Assert.True (false);
            } catch (DriveNotFoundException) { }
        }

        [Test]
        public void DriveGetAllTest()
        {
            globalState.Drive.New(createDrive("override", "first"), "local");
            globalState.Drive.New(createDrive("global"), "local");
            scriptState.Drive.New(createDrive("script"), "local");
            functionState.Drive.New(createDrive("override", "second"), "local");
            var drives = localState.Drive.GetAll();
            Assert.AreEqual(3, drives.Count);
            bool found = false;
            foreach (var curDrive in drives)
            {
                if (curDrive.Name.Equals("override"))
                {
                    if (found) //make sure it's only one time in there
                    {
                        Assert.True(false);
                    }
                    Assert.AreEqual("second", curDrive.Description);
                    found = true;
                }
            }
            Assert.True (found);
        }

        [TestCase("local", new string [] {})]
        [TestCase("0", new string [] {})]
        [TestCase("1", new string [] {"function"})]
        [TestCase("script", new string [] {"script1", "script2"})]
        [TestCase("2", new string [] {"script1", "script2"})]
        [TestCase("global", new string [] {"global1", "global2"})]
        [TestCase("3", new string [] {"global1", "global2"})]
        [TestCase("4", new string [] {}, ExpectedException=typeof(ArgumentOutOfRangeException))]
        public void DriveGetAllAtScopeTest(string scope, string[] expectedDescriptions)
        {
            globalState.Drive.New(createDrive("x", "global1"), "local");
            globalState.Drive.New(createDrive("y", "global2"), "local");
            scriptState.Drive.New(createDrive("x", "script1"), "local");
            scriptState.Drive.New(createDrive("y", "script2"), "local");
            functionState.Drive.New(createDrive("x", "function"), "local");
            var drives = localState.Drive.GetAllAtScope(scope);
            Assert.AreEqual(expectedDescriptions.Length, drives.Count);
            foreach (var curDrive in drives)
            {
                Assert.Contains(curDrive.Description, expectedDescriptions);
            }
        }

        [Test]
        public void DriveGetTest()
        {
            globalState.Drive.New(createDrive("override", "first"), "local");
            globalState.Drive.New(createDrive("global"), "local");
            functionState.Drive.New(createDrive("override", "second"), "local");
            var drive = localState.Drive.Get ("override");
            Assert.AreEqual("override", drive.Name);
            Assert.AreEqual("second", drive.Description);
            Assert.AreEqual("global", localState.Drive.Get("global").Name);
            try 
            {
                localState.Drive.Get("doesnt_exist");
                Assert.True(false);
            }
            catch (MethodInvocationException) { }
        }

        [TestCase("local", "local")]
        [TestCase("0", "local")]
        [TestCase("1", "function")]
        [TestCase("script", "script")]
        [TestCase("2",  "script")]
        [TestCase("global", "global")]
        [TestCase("3", "global")]
        [TestCase("4", "", ExpectedException=typeof(ArgumentOutOfRangeException))]
        public void DriveGetAtScopeTest(string scope, string expectedDescription)
        {
            globalState.Drive.New(createDrive("x", "global"), "local");
            scriptState.Drive.New(createDrive("x", "script"), "local");
            functionState.Drive.New(createDrive("x", "function"), "local");
            localState.Drive.New(createDrive("x", "local"), "local");
            Assert.AreEqual(expectedDescription, localState.Drive.GetAtScope("x", scope).Description);
        }

        public void DriveGetAllForProviderTest()
        {
            var provider = new ProviderInfo (null, null, "testProvider", "", null);
            globalState.Drive.New(createDrive ("global", "", provider), "local");
            scriptState.Drive.New(createDrive ("script", "", provider), "local");
            functionState.Drive.New(createDrive ("function", "", provider), "local");
            localState.Drive.New (createDrive("local", "", provider), "local");
            var drives = localState.Drive.GetAllForProvider("testProvider");
            Assert.AreEqual(4, drives.Count);
            foreach (var curDrive in drives) {
                Assert.Contains(curDrive.Name, new string[] {"global", "script", "function", "local"});
            }
            drives = scriptState.Drive.GetAllForProvider ("testProvider");
            Assert.AreEqual(2, drives.Count);
            foreach (var curDrive in drives) {
                Assert.Contains(curDrive.Name, new string[] {"global", "script"});
            }
            try {
                globalState.Drive.GetAllForProvider("doesnt_exist");
                Assert.True(false);
            } catch (MethodInvocationException) { }
        }

        private PSDriveInfo createDrive(string name, string descr="", ProviderInfo provider=null)
        {
            return new PSDriveInfo(name, provider, String.Empty, descr, null);
        }

        #endregion
    
        #region function related

        [TestCase("x", "f")] //correct x is fetched in general (from function scope)
        [TestCase("local:x", null)] //local scope has no function x
        [TestCase("script:x", null)] //x is private in the script scope
        [TestCase("global:x", "g")]
        [TestCase("y", "l")] //the overridden y in the local scope
        [TestCase("local:y", "l")] //also the local one, but explicitly
        [TestCase("script:y", "s")]
        [TestCase("global:y", "g")]
        [TestCase("z", "s")] //ignores the private z in function scope
        public void FunctionGetTest(string name, string expectedDescription)
        {
            globalState.Function.Set(createFunction("x", "g"));
            globalState.Function.Set(createFunction("y", "g"));
            scriptState.Function.Set(createFunction("x", "s", ScopedItemOptions.Private));
            scriptState.Function.Set(createFunction("y", "s"));
            scriptState.Function.Set(createFunction("z", "s"));
            functionState.Function.Set(createFunction("x", "f"));
            functionState.Function.Set(createFunction("y", "f"));
            functionState.Function.Set(createFunction("z", "f", ScopedItemOptions.Private));
            localState.Function.Set(createFunction("y", "l"));
            var info = localState.Function.Get(name);
            if (expectedDescription == null)
            {
                Assert.IsNull(info);
            }
            else
            {
                Assert.AreEqual(expectedDescription, info.Description);
            }
        }
  
        [Test]
        public void FunctionGetAllTest()
        {
            globalState.Function.Set(createFunction("override", "first"));
            globalState.Function.Set(createFunction("global"));
            scriptState.Function.Set(createFunction("script"));
            functionState.Function.Set(createFunction("override", "second"));
            var funs = localState.Function.GetAll();
            Assert.AreEqual(3, funs.Count);
            bool found = false;
            foreach (var curFun in funs)
            {
                if (curFun.Value.Name.Equals("override"))
                {
                    if (found) //make sure it's only one time in there
                    {
                        Assert.True(false);
                    }
                    Assert.AreEqual("second", curFun.Value.Description);
                    found = true;
                }
            }
            Assert.True(found);
        }

        [TestCase(AvailableStates.Global, "g", true)]
        [TestCase(AvailableStates.Script, "s", true)]
        [TestCase(AvailableStates.Function, "f", true)]
        [TestCase(AvailableStates.Local, "l", true)]
        [TestCase(AvailableStates.Local, "s", false)] //makes sure private setting works
        public void FunctionSetObjectTest(AvailableStates sessionState, object value, bool initLocal=true)
        {
            functionState.Function.Set("private:x", null, "f");
            localState.Function.Set("global:x", null, "g");
            localState.Function.Set("script:x", null, "s");
            if (initLocal)
            {
                localState.Function.Set ("local:x", null, "l");
            }
            Assert.AreEqual(value, states[sessionState].Function.Get("x").Description);
        }

        [TestCase("global:x", AvailableStates.Global, true)]
        [TestCase("script:x", AvailableStates.Script, true)]
        [TestCase("local:x", AvailableStates.Local, true)]
        [TestCase("x", AvailableStates.Function, false)] //looks in parent scopes and removes the variable
        public void FunctionRemoveTest(string variable, AvailableStates affectedState, bool initLocal)
        {
            globalState.Function.Set("x", null, "g");
            scriptState.Function.Set("x", null, "s");
            functionState.Function.Set("x", null, "f");
            if (initLocal)
            {
                localState.Function.Set("x", null, "l");
            }

            localState.Function.Remove(variable);
            foreach (KeyValuePair<AvailableStates, SessionState> curState in states)
            {
                if (curState.Key == affectedState || (curState.Key == AvailableStates.Local && !initLocal))
                {
                    Assert.IsNull(curState.Value.Function.Get("local:x"));
                }
                else
                {
                    Assert.IsNotNull(curState.Value.Function.Get("local:x"));
                }
            }
        }

        private FunctionInfo createFunction(string name, string description = "",
                                            ScopedItemOptions options = ScopedItemOptions.None)
        {
            var info = new FunctionInfo(name, null, options);
            info.Description = description;
            return info;
        }
        #endregion

        #region alias related

        [TestCase(AvailableStates.Global, true)] //the private one
        [TestCase(AvailableStates.Script, false)] //doesn't see the private one
        [TestCase(AvailableStates.Function, true)] //the non-private
        [TestCase(AvailableStates.Local, true)] //can see the non-private
        public void AliasExistsTest(AvailableStates affectedState, bool exists)
        {
            globalState.Alias.New(createAlias("test", "", ScopedItemOptions.Private), "local");
            functionState.Alias.New(createAlias("test", ""), "local");
            Assert.AreEqual(exists, states[affectedState].Alias.Exists("test"));
        }

        [TestCase("4", null, ExpectedException=typeof(ArgumentOutOfRangeException))]
        [TestCase("foo", null, ExpectedException=typeof(ArgumentException))]
        [TestCase("3", AvailableStates.Global)]
        [TestCase("global", AvailableStates.Global)]
        [TestCase("2", AvailableStates.Script)]
        [TestCase("script", AvailableStates.Script)]
        [TestCase("1", AvailableStates.Function)]
        [TestCase("0", AvailableStates.Local)]
        [TestCase("local", AvailableStates.Local)]
        public void AliasNewTest(string scope, AvailableStates affectedState)
        {
            AliasInfo info = createAlias("test");
            localState.Alias.New(info, scope);
            Assert.AreEqual(info, states[affectedState].Alias.Get(info.Name));
        }

        [TestCase("4", null, ExpectedException=typeof(ArgumentOutOfRangeException))]
        [TestCase("foo", null, ExpectedException=typeof(ArgumentException))]
        [TestCase("3", AvailableStates.Global)]
        [TestCase("global", AvailableStates.Global)]
        [TestCase("2", AvailableStates.Script)]
        [TestCase("script", AvailableStates.Script)]
        [TestCase("1", AvailableStates.Function)]
        [TestCase("0", AvailableStates.Local)]
        [TestCase("local", AvailableStates.Local)]
        public void AliasRemoveTest(string scope, AvailableStates affectedState)
        {
            Dictionary<AvailableStates, AliasInfo> aliasInfos = new Dictionary<AvailableStates, AliasInfo>();
            foreach (var curState in states)
            {
                var info = createAlias(curState.Key.ToString());
                curState.Value.Alias.New(info, "local");
                aliasInfos[curState.Key] = info;
            }
            localState.Alias.Remove(aliasInfos[affectedState].Name, scope);
            foreach (var curState in states)
            {
                if (curState.Key == affectedState)
                {
                    Assert.AreEqual(0, curState.Value.Alias.GetAllAtScope("local").Count);
                }
                else
                {
                    Assert.AreEqual(aliasInfos[curState.Key], curState.Value.Alias.Get(aliasInfos[curState.Key].Name));
                }
            }
        }
        [Test]
        public void AliasGetAllTest()
        {
            globalState.Alias.New(createAlias("override", "first"), "local");
            globalState.Alias.New(createAlias("global"), "local");
            scriptState.Alias.New(createAlias("script"), "local");
            functionState.Alias.New(createAlias("override", "second"), "local");
            var drives = localState.Alias.GetAll();
            Assert.AreEqual(3, drives.Count);
            bool found = false;
            foreach (var curAlias in drives)
            {
                if (curAlias.Value.Name.Equals("override"))
                {
                    if (found) //make sure it's only one time in there
                    {
                        Assert.True(false);
                    }
                    Assert.AreEqual("second", curAlias.Value.Definition);
                    found = true;
                }
            }
            Assert.True (found);
        }

        [TestCase("local", new string [] {})]
        [TestCase("0", new string [] {})]
        [TestCase("1", new string [] {"function"})]
        [TestCase("script", new string [] {"script1", "script2"})]
        [TestCase("2", new string [] {"script1", "script2"})]
        [TestCase("global", new string [] {"global1", "global2"})]
        [TestCase("3", new string [] {"global1", "global2"})]
        [TestCase("4", new string [] {}, ExpectedException=typeof(ArgumentOutOfRangeException))]
        public void AliasGetAllAtScopeTest(string scope, string[] expectedDescriptions)
        {
            globalState.Alias.New(createAlias("x", "global1"), "local");
            globalState.Alias.New(createAlias("y", "global2"), "local");
            scriptState.Alias.New(createAlias("x", "script1"), "local");
            scriptState.Alias.New(createAlias("y", "script2"), "local");
            functionState.Alias.New(createAlias("x", "function"), "local");
            var drives = localState.Alias.GetAllAtScope(scope);
            Assert.AreEqual(expectedDescriptions.Length, drives.Count);
            foreach (var curAlias in drives)
            {
                Assert.Contains(curAlias.Value.Definition, expectedDescriptions);
            }
        }

        [Test]
        public void AliasGetTest()
        {
            globalState.Alias.New(createAlias("override", "first"), "local");
            globalState.Alias.New(createAlias("global"), "local");
            functionState.Alias.New(createAlias("override", "second"), "local");
            var alias = localState.Alias.Get ("override");
            Assert.AreEqual("override", alias.Name);
            Assert.AreEqual("second", alias.Definition);
            Assert.AreEqual("global", localState.Alias.Get("global").Name);
            Assert.IsNull(localState.Alias.Get("doesnt_exist"));
        }

        [TestCase("local", "local")]
        [TestCase("0", "local")]
        [TestCase("1", "function")]
        [TestCase("script", "script")]
        [TestCase("2",  "script")]
        [TestCase("global", "global")]
        [TestCase("3", "global")]
        [TestCase("4", "", ExpectedException=typeof(ArgumentOutOfRangeException))]
        public void AliasGetAtScopeTest(string scope, string expectedDefinition)
        {
            globalState.Alias.New(createAlias("x", "global"), "local");
            scriptState.Alias.New(createAlias("x", "script"), "local");
            functionState.Alias.New(createAlias("x", "function"), "local");
            localState.Alias.New(createAlias("x", "local"), "local");
            Assert.AreEqual(expectedDefinition, localState.Alias.GetAtScope("x", scope).Definition);
        }

        private AliasInfo createAlias(string name, string definition = "",
                                      ScopedItemOptions options = ScopedItemOptions.None)
        {
            return new AliasInfo(name, definition, hostCommandManager, options);
        }

        #endregion
    }
}
