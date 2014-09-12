using System;
using NUnit.Framework;
using System.Management.Automation.Runspaces;
using Pash.Implementation;
using System.Management.Automation;
using System.Collections.Generic;
using System.IO;

namespace TestHost
{
    [TestFixture]
    public class TabExpansionProviderTests
    {
        private LocalRunspace _runspace;
        private TabExpansionProvider _tabExp;
        private List<string> _createdFiles;
        private List<string> _createdDirs;
        private string _origDir;
        private string _createdTempDir;

        [TestFixtureSetUp]
        public void SetUpRunspace()
        {
            TestHost host = new TestHost(new TestHostUserInterface());
            // use public static property, so we can access e.g. the ExecutionContext after execution
            _runspace = RunspaceFactory.CreateRunspace(host) as LocalRunspace;
            _runspace.Open();
            var alias = new AliasInfo("gciAlias", "Get-ChildItem", _runspace.CommandManager);
            _runspace.ExecutionContext.SessionState.Alias.New(alias, "global");
            _runspace.ExecutionContext.SessionState.Function.Set("MyFunction", new ScriptBlock(null));
            _runspace.ExecutionContext.SessionState.PSVariable.Set("myvar", 1);
            var newScope = _runspace.ExecutionContext.Clone(ScopeUsages.NewScope);
            newScope.SessionState.PSVariable.Set("myothervar", 2);
            newScope.SessionState.Function.Set("MyFun2", new ScriptBlock(null));
            _runspace.ExecutionContext = newScope;

            _tabExp = new TabExpansionProvider(_runspace);
        }

        [TestFixtureTearDown]
        public void CloseRunspace()
        {
            _runspace.Close();
            _tabExp = null;
        }

        private void CreateFile(string name, bool hidden = false)
        {
            name = CorrectSlash(name);
            File.Create(name).Close();
            if (hidden)
            {
                File.SetAttributes(name, FileAttributes.Hidden);
            }
            _createdFiles.Add(name);
        }

        private void CreateDir(string name, bool hidden = false)
        {
            name = CorrectSlash(name);
            var di = Directory.CreateDirectory(name);
            if (hidden)
            {
                di.Attributes = FileAttributes.Hidden | FileAttributes.Directory;
            }
            _createdDirs.Add(name);
        }

        [SetUp]
        public void SetUp()
        {
            _createdFiles = new List<string>();
            _createdDirs = new List<string>();
            _origDir = Directory.GetCurrentDirectory();
            _createdTempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_createdTempDir);
            Directory.SetCurrentDirectory(_createdTempDir);
        }

        [TearDown]
        public void RemoveCreatedDirsAndFiles()
        {
            foreach (var name in _createdFiles)
            {
                File.Delete(name);
            }
            _createdFiles.Clear();
            foreach (var dir in _createdDirs)
            {
                Directory.Delete(dir);
            }
            _createdDirs.Clear();
            Directory.SetCurrentDirectory(_origDir);
            Directory.Delete(_createdTempDir);
        }

        private string CorrectSlash(string path)
        {
            return path.Replace('/', Path.DirectorySeparatorChar);
        }

        private void CorrectSlashes(string[] paths)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                paths[i] = CorrectSlash(paths[i]);
            }
        }


        [TestCase("", new [] { "./bardir/", "./foodir/", "./bar", "./foo", "./foobar", "./foobaz" } )]
        [TestCase("f", new [] { "./foodir/", "./foo", "./foobar", "./foobaz" } )]
        [TestCase("foob", new [] {"./foobar", "./foobaz"} )]
        [TestCase("food", new [] {"./foodir/"} )]
        [TestCase("b", new [] {"./bardir/", "./bar"} )]
        [TestCase("foodir/", new [] {"./foodir/bar", "./foodir/foo"} )]
        [TestCase("foodir", new [] {"./foodir/"} )]
        public void TabExpansionForFilesWorks(string prefix, string[] expected)
        {
            CreateFile("foo");
            CreateFile("foobar");
            CreateFile("foobaz");
            CreateFile("bar");
            CreateDir("foodir");
            CreateFile("foodir/foo");
            CreateFile("foodir/bar");
            CreateDir("bardir");

            CorrectSlashes(expected);
            var expansions = _tabExp.GetFilesystemExpansions("", prefix);
            expansions.ShouldEqual(expected);
        }

        [TestCase("", new [] { "./bardir/", "./foodir/"} )]
        [TestCase(".", new [] { "./.bardir/", "./.foodir/", "./.bar",  "./.foo" } )]
        [TestCase(".f", new [] { "./.foodir/", "./.foo"} )]
        [TestCase(".foodir/", new [] { "./.foodir/bar", "./.foodir/foo"} )]
        [TestCase(".foodir/.", new [] { "./.foodir/.baz"} )]
        public void TabExpansionExcludesHiddenFilesIfNotMentionedDirectly(string prefix, string[] expected)
        {
            CreateFile(".bar", true);
            CreateFile(".foo", true);
            CreateDir("foodir");
            CreateFile("foodir/foo");
            CreateDir(".foodir", true);
            CreateFile(".foodir/foo");
            CreateFile(".foodir/bar");
            CreateFile(".foodir/.baz", true);
            CreateDir("bardir");
            CreateDir(".bardir", true);

            CorrectSlashes(expected);
            var expansions = _tabExp.GetFilesystemExpansions("", prefix);
            expansions.ShouldEqual(expected);
        }

        [TestCase("", new [] { "Select-Object ", "New-Variable ", "Out-Default ", "Write-Host " }, new [] { "MyFunction " })]
        [TestCase("Ne", new [] {"New-Variable ", "New-Alias " }, new [] { "Select-Object ", "Out-Default ", "Write-Host ", "MyFunction " })]
        [TestCase("Neww", new string[]{}, new [] {"New-Variable ", "New-Alias ", "Select-Object ", "Out-Default ", "Write-Host ", "Prompt " })]
        public void TabExpansionKnowsCommands(string prefix, string[] shouldContain, string[] shouldntContain)
        {
            var expansions = new List<string>(_tabExp.GetCommandExpansions("", prefix));
            foreach (var str in shouldContain)
            {
                Assert.True(expansions.Contains(str), "Expansions doesn't contain " + str);
            }
            foreach (var str in shouldntContain)
            {
                Assert.False(expansions.Contains(str), "Expansions contains " + str);
            }
        }

        // finding cmdlet in line
        [TestCase("", null)] // nothing to find
        [TestCase("new-alias", "New-Alias")] // case insensitive
        [TestCase("new-alias -foo -bar hdsj 'New-Variable'", "New-Alias")] // quoted cmdlet names aren't found
        [TestCase("new-alias -foo -bar hdsj; 'New-Variable'", null)] // cmdlet is in a different command
        [TestCase("new-alias -foo -bar hdsj | function", null)] // we're in a pipeline
        [TestCase("new-alias -foo -bar ('ab' + 'c' ", null)] // we are inside parenthesis, doesn't make sense
        [TestCase("gciAlias baz fooo ", "Get-ChildItem")] // can resolve an alias
        [TestCase("gciAlias baz fooo && MyFunction", null)] // Function "MyFunction" is found first
        public void TabExpansionCanDetermineACmdlet(string hardPrefix, string name)
        {
            var cmdlet = _tabExp.CheckForCommandWithCmdlet(hardPrefix);
            if (name == null)
            {
                Assert.Null(cmdlet);
            }
            else
            {
                Assert.AreEqual(name, cmdlet.Name);
            }
        }

        [TestCase("", new [] { "-LiteralPath", "-PassThru", "-Path", "-PSPath", "-StackName" } )]
        [TestCase("-p", new [] { "-PassThru", "-Path", "-PSPath"} )]
        [TestCase("p", new string[] { } )]
        [TestCase("-ps", new [] { "-PSPath",} )]
        public void TabExpansionCanExpandCmdletParameters(string prefix, string[] expected)
        {
            var cmdlet = _tabExp.CheckForCommandWithCmdlet("Set-Location");
            var expansions = _tabExp.GetCmdletParameterExpansions(cmdlet, prefix);
            expansions.ShouldEqual(expected);
        }

        [TestCase("my", new [] { "MyFun2", "MyFunction" })]
        [TestCase("myfunc", new [] { "MyFunction" })]
        [TestCase("global:my", new [] { "global:MyFunction" })]
        [TestCase("local:my", new [] { "local:MyFun2" })]
        public void TabExpansionForFunctionsWorks(string prefix, string[] expected)
        {
            var expansions = _tabExp.GetFunctionExpansions("", prefix);
            expansions.ShouldEqual(expected);
        }

        [TestCase("$MY", new [] { "$myothervar", "$myvar" })]
        [TestCase("$myv", new [] { "$myvar" })]
        [TestCase("$global:my", new [] { "$global:myvar" })]
        [TestCase("$local:my", new [] { "$local:myothervar" })]
        [TestCase("m", new string [] {})]
        public void TabExpansionForVariablesWorks(string prefix, string[] expected)
        {
            var expansions = _tabExp.GetVariableExpansions("", prefix);
            expansions.ShouldEqual(expected);
        }
    }
}

