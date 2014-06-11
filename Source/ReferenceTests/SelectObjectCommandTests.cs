using System;
using System.Linq;
using NUnit.Framework;
using System.Text;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Collections.Generic;

namespace ReferenceTests
{
    [TestFixture]
    public class SelectObjectCommandTests
    {
        private string[] _allDataProps = new [] { "name", "age", "sex" };
        private Dictionary<string, object>[] _data = new []
        {
            new Dictionary<string, object>() {{"name", "John Doe"}, { "age", 45}, {"sex", "m" }},
            new Dictionary<string, object>() {{"name", "Derpina"}, { "age", 35}, {"sex", "f" }},
            new Dictionary<string, object>() {{"name", "Foobar"}, { "age", 12}, {"sex", "f" }},
            new Dictionary<string, object>() {{"name", "Barabaz"}, { "age", 89}, {"sex", "m" }},
        };

        private string GenerateObjectArrayCmd()
        {
            var sb = new StringBuilder();
            sb.Append("@(");
            foreach (var person in _data)
            {
                sb.AppendFormat("(new-object psobject -property @{{name='{0}'; age={1}; sex='{2}'}})", person["name"],
                                person["age"], person["sex"]);
                sb.Append(",");
            }
            sb.Remove(sb.Length - 1, 1); // remove last comma
            sb.Append(")");
            return sb.ToString();
        }

        private void AssertPersonObjectsEqual(Dictionary<string, object>[] expected, Collection<PSObject> results,
                                              params string[] propsWithValue)
        {
            var nonNullProps = (propsWithValue.Length == 0) ?
                new List<string>(_allDataProps) : new List<string>(propsWithValue);
            var allDataProps = _allDataProps.Concat(nonNullProps).Distinct().ToList();

            Assert.AreEqual(expected.Length, results.Count);
            for (int i = 0; i < expected.Length; i++)
            {
                foreach (var prop in allDataProps)
                {
                    var ex = nonNullProps.Contains(prop) ? expected[i][prop] : null;
                    var valProp = results[i].Properties[prop];
                    var val = (valProp != null) ? valProp.Value : null;
                    Assert.AreEqual(ex, val);
                }
            }
        }

        private Dictionary<string, object>[] CopyData(Dictionary<string, object>[] orig)
        {
            var copied = new Dictionary<string, object>[orig.Length];
            for (int i = 0; i < orig.Length; i++)
            {
                copied[i] = new Dictionary<string, object>(orig[i]);
            }
            return copied;
        }

        [Test]
        public void SelectObjectWithoutArgsSelectsEverything()
        {
            var cmd = GenerateObjectArrayCmd() + " | Select-Object";
            var result = ReferenceHost.RawExecute(cmd);
            AssertPersonObjectsEqual(_data, result);
            Assert.Contains("Selected." + typeof(PSCustomObject).ToString(), result[0].TypeNames);
        }

        [Test]
        public void SelectObjectByIndex()
        {
            var expected = new [] { _data[1], _data[3] };
            var cmd = GenerateObjectArrayCmd() + " | Select-Object -Index 1,3";
            var result = ReferenceHost.RawExecute(cmd);
            AssertPersonObjectsEqual(expected, result);
        }

        [Test]
        public void SelectObjectByFirstLast()
        {
            var expected = new [] { _data[0], _data[1], _data[3] };
            var cmd = GenerateObjectArrayCmd() + " | Select-Object -First 2 -Last 1";
            var result = ReferenceHost.RawExecute(cmd);
            AssertPersonObjectsEqual(expected, result);
        }

        [Test]
        public void SelectObjectByLastWithSkip()
        {
            var expected = new [] { _data[2] };
            var cmd = GenerateObjectArrayCmd() + " | Select-Object -Skip 1 -Last 1";
            var result = ReferenceHost.RawExecute(cmd);
            AssertPersonObjectsEqual(expected, result);
        }

        [Test]
        public void SelectObjectByFirstLastWithSkip()
        {
            var expected = new [] { _data[1], _data[3] }; // skip first, take one, take last
            var cmd = GenerateObjectArrayCmd() + " | Select-Object -Skip 1 -First 1 -Last 1";
            var result = ReferenceHost.RawExecute(cmd);
            AssertPersonObjectsEqual(expected, result);
        }

        [Test]
        public void SelectObjectByFirstExceeding()
        {
            var cmd = GenerateObjectArrayCmd() + " | Select-Object -First 10";
            var result = ReferenceHost.RawExecute(cmd);
            AssertPersonObjectsEqual(_data, result);
        }

        [Test]
        public void SelectObjectByFirstButSkipAll()
        {
            var cmd = GenerateObjectArrayCmd() + " | Select-Object -Skip 5 -First 4";
            var result = ReferenceHost.RawExecute(cmd);
            AssertPersonObjectsEqual(new Dictionary<string, object>[] { }, result);
        }

        [Test]
        public void SelectObjectByFirstLastOverlappingIgnoresLast()
        {
            var expected = new[] { _data[2], _data[3] }; // skip two, take two
            var cmd = GenerateObjectArrayCmd() + " | Select-Object -Skip 2 -First 2 -Last 4";
            var result = ReferenceHost.RawExecute(cmd);
            AssertPersonObjectsEqual(expected, result);
        }

        [Test]
        public void SelectObjectUniqueOnlyChecksPropertyNames()
        {
            var expected = new [] { _data[0] }; // all have the same property names
            var cmd = GenerateObjectArrayCmd() + " | Select-Object -Unique";
            var result = ReferenceHost.RawExecute(cmd);
            AssertPersonObjectsEqual(expected, result);
        }

        [Test]
        public void SelectObjectCanExcludeProperties()
        {
            var cmd = GenerateObjectArrayCmd() + " | Select-Object -Property age -ExcludeProperty name,sex";
            var result = ReferenceHost.RawExecute(cmd);
            AssertPersonObjectsEqual(_data, result, "age");
        }

        [Test]
        public void SelectObjectCantExcludePropertiesIfIncludedArentNamed()
        {
            var cmd = GenerateObjectArrayCmd() + " | Select-Object -ExcludeProperty name,sex";
            var result = ReferenceHost.RawExecute(cmd);
            AssertPersonObjectsEqual(_data, result); // all properties should still be included
        }

        [Test]
        public void SelectObjectCanTakeCertainProperties()
        {
            var cmd = GenerateObjectArrayCmd() + " | Select-Object -Property name,sex";
            var result = ReferenceHost.RawExecute(cmd);
            AssertPersonObjectsEqual(_data, result, "name", "sex");
        }

        [TestCase("name")]
        [TestCase("label")]
        public void SelectObjectCanCreateNewStaticProperties(string lab)
        {
            var expected = CopyData(_data);
            foreach (var ex in expected)
            {
                ex["foo"] = PSObject.AsPSObject("bar");
            }
            var cmd = GenerateObjectArrayCmd() + " | Select-Object -Property @{" + lab + "='foo'; expression={'bar'}}";
            var result = ReferenceHost.RawExecute(cmd);
            AssertPersonObjectsEqual(expected, result, "foo");
        }

        [Test]
        public void SelectObjectCanCreateNewDynamicProperties()
        {
            var expected = CopyData(_data);
            foreach (var ex in expected)
            {
                ex["greet"] = PSObject.AsPSObject("Hi " + ex["name"]);
            }
            var cmd = GenerateObjectArrayCmd() + " | Select-Object -Property name,@{name='greet'; expression={'Hi ' + $_.name}}";
            var result = ReferenceHost.RawExecute(cmd);
            AssertPersonObjectsEqual(expected, result, "greet", "name");
        }
    }
}

