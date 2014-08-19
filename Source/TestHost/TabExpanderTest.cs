using System;
using System.Linq;
using Mono.Terminal;
using NUnit.Framework;

namespace TestHost
{
    [TestFixture]
    public class TabExpanderTest
    {
        private static string[] _possibleCompletions = new string[] {
            "foo" , "foobar", "foobaz", "foobarabaz",
            "bar", "barabar", "baz"
        };

        private TabExpanderUI _tabExpander;

        public static string[] DummyExpansion(string hardPrefix, string replacable)
        {
            var prefixLen = replacable.Length;
            var results = from comp in _possibleCompletions where comp.StartsWith(replacable) select comp;
            return results.ToArray();
        }

        [SetUp]
        public void SetUp()
        {
            _tabExpander = new TabExpanderUI();
            _tabExpander.TabExpansionEvent = DummyExpansion;
        }

        [TestCase("f", "foo")]
        [TestCase("b", "ba")]
        [TestCase("x y z b", "x y z ba")]
        [TestCase("foobar", "foobar")]
        [TestCase("xfoob", "xfoob")]
        [TestCase("x foob", "x fooba")]
        [TestCase("x", "x")]
        public void ExpanderFindsCommonPrefix(string prefix, string foundPrefix)
        {
            _tabExpander.Start(prefix);
            _tabExpander.ChooseNext();
            Assert.AreEqual(true, _tabExpander.Running);
            Assert.AreEqual(foundPrefix, _tabExpander.GetExpandedCommand());
        }

        [Test]
        public void ExpanderCyclesCommands()
        {
            _tabExpander.Start("");
            _tabExpander.ChooseNext(); // shows common prefix
            int numPossibilites = _possibleCompletions.Length;
            for (int i = 0; i < numPossibilites * 2; i++)
            {
                _tabExpander.ChooseNext(); // should select a possibility
                var expected = _possibleCompletions[i % numPossibilites];
                Assert.AreEqual(expected, _tabExpander.GetExpandedCommand());
                Assert.AreEqual(true, _tabExpander.Running);
            }
        }

        [Test]
        public void OneCompletionFinishes()
        {
            _tabExpander.Start("foobara");
            _tabExpander.ChooseNext();
            Assert.AreEqual("foobarabaz", _tabExpander.GetExpandedCommand());
            Assert.AreEqual("foobarabaz", _tabExpander.AcceptedCommand);
            Assert.AreEqual(false, _tabExpander.Running);
        }

        [Test]
        public void StartingWithLastExpansionDoesntReset()
        {
            _tabExpander.Start("f");
            _tabExpander.ChooseNext();
            var commonPrefix = _tabExpander.GetExpandedCommand();
            Assert.AreEqual("foo", commonPrefix);
            _tabExpander.Start(commonPrefix);
            _tabExpander.ChooseNext(); // choose first item
            var firstChoice = _tabExpander.GetExpandedCommand();
            Assert.AreEqual(_possibleCompletions[0], firstChoice);
            _tabExpander.Start(firstChoice);
            _tabExpander.ChooseNext();
            Assert.AreEqual(_possibleCompletions[1], _tabExpander.GetExpandedCommand());
        }

        [Test]
        public void StartingWithDifferentStringResets()
        {
            _tabExpander.Start("f");
            _tabExpander.ChooseNext();
            _tabExpander.Start("foob");
            _tabExpander.ChooseNext(); // chooses common prefix of "foob" completions, no selection
            Assert.AreEqual("fooba", _tabExpander.GetExpandedCommand());
        }

        [Test]
        public void StartingWithAcceptedWordResets()
        {
            _tabExpander.Start("b");
            _tabExpander.ChooseNext(); // common prefix "ba"
            _tabExpander.ChooseNext(); // first choice "bar"
            _tabExpander.Accept();
            _tabExpander.Start(_tabExpander.GetExpandedCommand());
            _tabExpander.ChooseNext(); // was resetted, common prefix is again "bar"
            Assert.AreEqual("bar", _tabExpander.GetExpandedCommand());
        }

        [TestCase(false, "foo")]
        [TestCase(true, "f")]
        public void AbortingWorks(bool reset, string expected)
        {
            _tabExpander.Start("f");
            _tabExpander.ChooseNext();
            _tabExpander.Abort(reset);
            Assert.AreEqual(expected, _tabExpander.GetExpandedCommand());
            Assert.AreEqual(false, _tabExpander.Running);
        }

        [TestCase(1, "foo")] // common prefix
        [TestCase(2, "foo")] // first command
        [TestCase(3, "foobar")] // second command
        public void AcceptingWorks(int numChoose, string expected)
        {
            _tabExpander.Start("f");
            for (int i = 0; i < numChoose; i++)
            {
                _tabExpander.ChooseNext();
            }
            _tabExpander.Accept();
            Assert.AreEqual(false, _tabExpander.Running);
            Assert.AreEqual(expected, _tabExpander.GetExpandedCommand());
            Assert.AreEqual(expected, _tabExpander.AcceptedCommand);
        }
    }
}

