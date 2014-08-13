using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Management.Automation;
using System.IO;

namespace ReferenceTests
{
    [TestFixture]
    public class CsvTests : ReferenceTestBase
    {
        private string[] _commonProperties = new [] { "name", "age", "sex" };
        private Dictionary<string, object>[] _data = new []
        {
            new Dictionary<string, object> {{"name", "John Doe"}, {"age", "45"}, {"sex", "m"}},
            new Dictionary<string, object> {{"name", "Foobar\t"}, {"age", "25"}, {"sex", "f"}, {"randomfield", "blub"}},
            new Dictionary<string, object> {{"name", "Joe,"}, {"age", "23"}, {"sex", "m"}},
            new Dictionary<string, object> {{"name", ""}, {"age", null}, {"sex", null}},
            new Dictionary<string, object> {{"name", "'Jack"}, {"age", "15"}, {"sex", "m'"}},
            new Dictionary<string, object> {{"name", "Any,bo\"\"dy"}, {"age", "8\"9"}, {"sex", ""}},
            new Dictionary<string, object> {{"name", "Ma\"\"ry"}, {"age", null}, {"sex", null}},
            new Dictionary<string, object> {{"name", "Jane" + Environment.NewLine + ",12"}, {"age", "3"}, {"sex", "f" + Environment.NewLine}}
        };

        private string[] ExecuteGenerateCsvCommand(bool exportToFile, string input, string args)
        {
            var fname = "__tmpCsvExport.csv";
            var cmd = input + " | ";
            cmd += exportToFile ? "Export-Csv -Path " + fname + " " + args : "ConvertTo-Csv " + args;
            var results = ReferenceHost.Execute(cmd).Split(new []{Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            if (exportToFile)
            {
                results = ReadLinesFromFile(fname);
                File.Delete(fname);
            }
            return results;
        }

        [Test]
        public void ImportCsvWorksRobustly()
        {
            var csvFile = CreateFile(NewlineJoin(
                "name, age,\t\tsex", // tabs shouldn't matter, last commas neither
                "John Doe,45,m", // a normal data set
                "Foobar\t,\t\t25,f,randomdata,", // tabs are stripped only at beginning, extra data is ignored and shouldn't cause an error
                "\"Joe,\",23,m", //delimiter is in quotes 
                "", //newlines are also taken as objects
                "'Jack,15,m'", // single quotes don't introduce a string as double quotes
                "\"Any,\"bo\"\"dy, 8\"9,", // escape only if beginning with quote. missing value => empty string
                "\"Ma\"\"\"\"ry\"", // missing fields, early NL => other fields are null. "" in quotes => escaped quote
                "\"Jane", // starting the quotes, include newline; no finishing quote (file end) 
                ",12\",3,\"f"), // rest of "Jane"
                                     "csv");
            var results = ReferenceHost.RawExecute(String.Format("Import-Csv {0}", csvFile));
            Assert.AreEqual(_data.Length, results.Count);
            for (int i = 0; i < _data.Length; i++)
            {
                foreach (var prop in _commonProperties)
                {
                    var resProp = results[i].Properties[prop];
                    Assert.NotNull(resProp);
                    Assert.AreEqual(_data[i][prop], resProp.Value, prop + " doesn't match");
                }
            }
        }

        [Test]
        public void ImportCsvWithDifferentDelimiter()
        {
            var csvFile = CreateFile(NewlineJoin(
                "foo|bar",
                "\"1|2\"|3"
                ), "csv");
            var results = ReferenceHost.RawExecute(String.Format("Import-Csv {0} -Delimiter '|'", csvFile));
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("1|2", results[0].Properties["foo"].Value);
            Assert.AreEqual("3", results[0].Properties["bar"].Value);
        }

        [Test]
        public void ImportCsvWithDifferentHeader()
        {
            var csvFile = CreateFile(NewlineJoin(
                "name, age",
                "John,40"
                ), "csv");
            var results = ReferenceHost.RawExecute(String.Format("Import-Csv {0} -Header 'his name','his age','his sex'", csvFile));
            Assert.AreEqual(2, results.Count); // first line is interpreted as data
            Assert.AreEqual("name", results[0].Properties["his name"].Value);
            Assert.AreEqual("age", results[0].Properties["his age"].Value);
            Assert.NotNull(results[0].Properties["his sex"]); // property should exist, but value has to be null
            Assert.AreEqual(null, results[0].Properties["his sex"].Value);
            Assert.AreEqual("John", results[1].Properties["his name"].Value);
            Assert.AreEqual("40", results[1].Properties["his age"].Value);
            Assert.NotNull(results[1].Properties["his sex"]); // property should exist, but value has to be null
            Assert.AreEqual(null, results[1].Properties["his sex"].Value);
        }

        [TestCase("")] // empty field name
        [TestCase("name")] // twice the same name
        public void ImportCsvNeedsValidHeader(string invalidHeader)
        {
            var csvFile = CreateFile(NewlineJoin(
                "name, age," + invalidHeader,
                "John,40,Doe"
                ), "csv");
            // TODO: need to fix the exception type somewhere in the pipeline processing
            Assert.Throws(Is.InstanceOf(typeof(Exception)), delegate() {
                ReferenceHost.RawExecute(String.Format("Import-Csv {0}", csvFile));
            });
        }

        [Test]
        public void ImportCsvDoesntThrowOnImportingAnEmptyFile()
        {
            var csvFile = CreateFile("", "csv");
            var result = ReferenceHost.RawExecute(String.Format("Import-Csv {0}", csvFile));
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ImportCsvImportsTypeName()
        {
            var csvFile = CreateFile(NewlineJoin(
                "#TYPE My.Custom.Person",
                "name,age",
                "John,40"
                ), "csv");
            var result = ReferenceHost.RawExecute(String.Format("Import-Csv {0}", csvFile));
            Assert.AreEqual(1, result.Count);
            Assert.Contains("CSV:My.Custom.Person", result[0].TypeNames);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GenerateCsvExportsDataCorrectly(bool exportToFile)
        {
            var expected = new string[] {
                "#TYPE System.Management.Automation.PSCustomObject",
                "\"name\",\"age\",\"sex\"",
                "\"John Doe\",\"45\",\"m\"",
                "\"Foobar\t\",\"25\",\"f\"",
                "\"Joe,\",\"23\",\"m\"",
                "\"\",,",
                "\"'Jack\",\"15\",\"m'\"",
                "\"Any,bo\"\"\"\"dy\",\"8\"\"9\",\"\"",
                "\"Ma\"\"\"\"ry\",,",
                "\"Jane",
                ",12\",\"3\",\"f",
                "\""
            };
            var objectCmd = CreateObjectsCommand(_data);
            var result = ExecuteGenerateCsvCommand(true, objectCmd, "");
            Assert.AreEqual(expected, result);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GenerateCsvWithOtherDelimiter(bool exportToFile)
        {
            var expected = new string[] {
                "#TYPE System.Management.Automation.PSCustomObject",
                "\"name\"|\"age\"|\"sex\"",
                "\"John Doe\"|\"45\"|\"m\""
            };
            var objectCmd = CreateObjectsCommand(new[] { _data[0] });
            var result = ExecuteGenerateCsvCommand(true, objectCmd, "-Delimiter '|'");
            Assert.AreEqual(expected, result);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GenerateCsvWithoutTypedata(bool exportToFile)
        {
            var expected = new string[] {
                "\"name\",\"age\",\"sex\"",
                "\"John Doe\",\"45\",\"m\""
            };
            var objectCmd = CreateObjectsCommand(new[] { _data[0] });
            var result = ExecuteGenerateCsvCommand(true, objectCmd, "-NoTypeInformation");
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GenerateCsvFirstDataSetDeterminesHeader()
        {
            var expectedHeaders = new [] {
                "\"randomfield\"","\"name\"","\"age\"","\"sex\""
            };
            var objectCmd = CreateObjectsCommand(new[] { _data[1], _data[0] });
            var result = ExecuteGenerateCsvCommand(true, objectCmd, "");
            var resultHeaders = result[1].Split(new [] { ',' }, StringSplitOptions.None);
            Assert.AreEqual(expectedHeaders.Length, resultHeaders.Length);
            foreach (var h in expectedHeaders)
            {
                Assert.Contains(h, resultHeaders);
            }
        }

        [Test]
        public void ExportCsvOverwritesByDefault()
        {
            var expected = new string[] {
                "#TYPE System.Management.Automation.PSCustomObject",
                "\"name\",\"age\",\"sex\"",
                "\"Joe,\",\"23\",\"m\""
            };
            var exportFileName = "__tmpExportedCsvData.csv";
            var objectCmd = CreateObjectsCommand(new[] { _data[0] });
            ReferenceHost.Execute(String.Format("{0} | Export-Csv {1}", objectCmd, exportFileName));
            objectCmd = CreateObjectsCommand(new[] { _data[2] });
            ReferenceHost.Execute(String.Format("{0} | Export-Csv {1}", objectCmd, exportFileName));
            var lines = ReadLinesFromFile(exportFileName);
            File.Delete(exportFileName);
            Assert.AreEqual(expected, lines);
        }

        [Test]
        public void ExportCsvDoesNotOverwriteWithClobberArg()
        {
            var exportFileName = "__tmpExportedCsvData.csv";
            var objectCmd = CreateObjectsCommand(new[] { _data[0] });
            ReferenceHost.Execute(String.Format("{0} | Export-Csv {1}", objectCmd, exportFileName));
            objectCmd = CreateObjectsCommand(new[] { _data[1] });
            // TODO: need to fix the exception type somewhere in the pipeline processing
            Assert.Throws(Is.InstanceOf(typeof(Exception)), delegate ()  {
                ReferenceHost.Execute(String.Format("{0} | Export-Csv {1} -NoClobber", objectCmd, exportFileName));
            });
        }
    }
}

