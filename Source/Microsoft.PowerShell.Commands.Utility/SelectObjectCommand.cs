using System;
using System.Linq;
using System.Management.Automation.Language;
using System.Management.Automation;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Collections;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Select, "Object", DefaultParameterSetName = "DefaultParameters")]
    public class SelectObjectCommand : CollectForEndProcessingCommandBase
    {
        class ObjectComparer : IEqualityComparer<PSObject>
        {
            public bool Equals(PSObject x, PSObject y)
            {
                if ((x == null) != (y == null))
                {
                    return false;
                }
                if (x == null && y == null)
                {
                    return true;
                }
                if (!x.Equals(y))
                {
                    return false;
                }
                // Powershell somehow seems to only compare the number an names of NoteProperties, not their values
                var propsX = (from member in x.Properties.Match("*", PSMemberTypes.NoteProperty)
                              select member.Name).ToList();
                var propsY = (from member in y.Properties.Match("*", PSMemberTypes.NoteProperty)
                              select member.Name).ToList();
                if (propsX.Count != propsY.Count)
                {
                    return false;
                }
                foreach (var prop in propsX)
                {
                    if (!propsY.Contains(prop))
                    {
                        return false;
                    }
                }
                return true;
            }

            public int GetHashCode(PSObject obj)
            {
                return obj == null ? 0 : obj.GetHashCode();
            }
        }

        [Parameter(ParameterSetName = "DefaultParameters")]
        public string[] ExcludeProperty { get; set; }

        [Parameter(ParameterSetName = "DefaultParameters")]
        public string ExpandProperty { get; set; }

        [Parameter(ParameterSetName = "DefaultParameters")]
        public int First { get; set; }

        [Parameter(ParameterSetName = "IndexParameters")]
        public int[] Index { get; set; }

        [Parameter(ParameterSetName = "DefaultParameters")]
        public int Last { get; set; }

        [Parameter(Position =  1, ParameterSetName = "DefaultParameters")]
        public object[] Property { get; set; }

        [Parameter(ParameterSetName = "DefaultParameters")]
        public int Skip { get; set; }

        [Parameter(ParameterSetName = "DefaultParameters")]
        public SwitchParameter Unique { get; set; }

        protected override void EndProcessing()
        {
            if (ParameterSetName.Equals("IndexParameters"))
            {
                foreach (var idx in Index)
                {
                    if (idx < InputCollection.Count)
                    {
                        WriteObject(InputCollection[idx]);
                    }
                }
                return;
            }

            var inputObjects = FilterByPosition(InputCollection);
            inputObjects = ApplyPropertyParameters(inputObjects);
            if (Unique.IsPresent)
            {
                var comparer = new ObjectComparer();
                inputObjects = inputObjects.Distinct(comparer).ToList();
            }
            WriteObject(inputObjects, true);
        }

        private List<PSObject> ApplyPropertyParameters(IEnumerable<PSObject> objects)
        {
            // TODO: implement support for ExpandProperty
            if (Property == null || Property.Length == 0)
            {
                return objects.ToList();
            }
            List<PSObject> selectedObjects = new List<PSObject>();
            foreach (var curObj in objects)
            {
                if (PSObject.Unwrap(curObj) == null)
                {
                    continue;
                }
                PSObject curSelectedObj = new PSObject();
                curSelectedObj.TypeNames.Insert(0, "Selected." + curObj.BaseObject.GetType().ToString());
                var properties = curObj.SelectProperties(Property, ExecutionContext);
                foreach (var curProp in properties)
                {
                    if (ExcludeProperty == null || !ExcludeProperty.Contains(curProp.Name))
                    {
                        curSelectedObj.Properties.Add(curProp);
                        curSelectedObj.Members.Add(curProp);
                    }
                }
                selectedObjects.Add(curSelectedObj);
            }
            return selectedObjects;
        }

        private List<PSObject> FilterByPosition(IEnumerable<PSObject> objects)
        {
            var count = objects.Count();
            bool skipEnd = (Last != 0 && First == 0);
            var skipFirst = skipEnd ? 0 : Skip;
            var takeFirst = First;
            if (Last == 0 && First == 0)
            {
                takeFirst = count - skipFirst;
            }
            var firstObjects = objects.Skip(skipFirst).Take(takeFirst);
            var takenAndSkipped = (skipFirst + takeFirst);
            if (takenAndSkipped >= count || Last == 0 || (skipEnd && Skip >= (count - takenAndSkipped)))
            {
                return firstObjects.ToList();
            }
            var takeAndSkipAtEnd = Last + (skipEnd ? Skip : 0);
            var skipBeforeEnd = count - takenAndSkipped - takeAndSkipAtEnd;
            int takeLast = Last;
            if (skipBeforeEnd < 0)
            {
                takeLast += skipBeforeEnd;
                skipBeforeEnd = 0;
            }
            return firstObjects.Concat(objects.Skip(takenAndSkipped + skipBeforeEnd).Take(takeLast)).ToList();
        }
    }
}

