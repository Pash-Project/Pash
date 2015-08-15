using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TestPSSnapIn
{
    public class TestReadOnlyParameterizedProperty
    {
        private List<string> _fileNames;

        public TestReadOnlyParameterizedProperty()
            : this(Enumerable.Empty<string>())
        {
        }

        public TestReadOnlyParameterizedProperty(IEnumerable<string> fileNames)
        {
            _fileNames = fileNames.ToList();
        }

        [IndexerName("FileNames")]
        public string this[int index]
        {
            get { return _fileNames[index]; }
        }
    }

    public class TestWriteOnlyParameterizedProperty
    {
        private Dictionary<int, string> _fileNames;

        public TestWriteOnlyParameterizedProperty()
        {
            _fileNames = new Dictionary<int, string>();
        }

        public string GetFileName(int index)
        {
            return _fileNames[index];
        }

        [IndexerName("FileNames")]
        public string this[int index]
        {
            set { _fileNames[index] = value; }
        }
    }

    public class TestParameterizedProperty
    {
        private List<string> _fileNames;

        public TestParameterizedProperty()
            : this(Enumerable.Empty<string>())
        {
        }

        public TestParameterizedProperty(IEnumerable<string> fileNames)
        {
            _fileNames = fileNames.ToList();
        }

        [IndexerName("FileNames")]
        public string this[int index]
        {
            get
            {
                return _fileNames[index];
            }
            set
            {
                _fileNames[index] = value;
            }
        }
    }

    public class TestOverloadedByTypeParameterizedProperty
    {
        private List<string> _fileNames;

        public TestOverloadedByTypeParameterizedProperty()
            : this(Enumerable.Empty<string>())
        {
        }

        public TestOverloadedByTypeParameterizedProperty(IEnumerable<string> fileNames)
        {
            _fileNames = fileNames.ToList();
        }

        [IndexerName("FileNames")]
        public string this[int index]
        {
            get
            {
                return _fileNames[index];
            }
            set
            {
               _fileNames[index] = value;
            }
        }

        [IndexerName("FileNames")]
        public string this[string fileName]
        {
            get
            {
                return _fileNames.IndexOf(fileName).ToString();
            }
            set
            {
                _fileNames.Add(value);
            }
        }
    }

    public interface ITestParameterizedProperty
    {
        [IndexerName("FileNames")]
        string this[int index] { get; set; }
    }

    public interface ITestInheritedParameterizedProperty : ITestParameterizedProperty
    {
    }

    public class TestInterfaceParameterizedProperty : ITestInheritedParameterizedProperty
    {
        private List<string> _fileNames;

        public TestInterfaceParameterizedProperty()
            : this(Enumerable.Empty<string>())
        {
        }

        public TestInterfaceParameterizedProperty(IEnumerable<string> fileNames)
        {
            _fileNames = fileNames.ToList();
        }

        public string this[int index]
        {
            get
            {
                return _fileNames[index];
            }
            set
            {
                _fileNames[index] = value;
            }
        }
    }

}
