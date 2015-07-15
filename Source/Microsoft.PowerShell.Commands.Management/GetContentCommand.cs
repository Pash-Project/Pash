// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Management.Pash.Implementation;

namespace Microsoft.PowerShell.Commands
{
    [CmdletAttribute(VerbsCommon.Get, "Content", DefaultParameterSetName="Path" /*HelpUri="http://go.microsoft.com/fwlink/?LinkID=113310"*/)]
    [OutputType(typeof(byte), typeof(string))]
    public class GetContentCommand : ContentCommandBase
    {
        [ParameterAttribute(ValueFromPipelineByPropertyName = true)]
        public long ReadCount { get; set; }

        [Alias("Last")]
        [ParameterAttribute(ValueFromPipelineByPropertyName = true)]
        public int Tail { get; set; }

        [Alias("First", "Head")]
        [ParameterAttribute(ValueFromPipelineByPropertyName = true)]
        public long TotalCount { get; set; }

        public GetContentCommand()
        {
            ReadCount = 1;
            TotalCount = -1;
        }

        List<object> pendingItems = new List<object>();
        IContentReader contentReader;

        // TODO: #DynamicParameter support

        protected override void ProcessRecord()
        {
            var readers = InvokeProvider.Content.GetReader(InternalPaths, ProviderRuntime);
            foreach (var curContentReader in readers)
            {
                contentReader = curContentReader;
                try
                {
                    if (Tail > 0)
                    {
                        WriteLastItemsToPipeline();
                    }
                    else
                    {
                        WriteItemsToPipeline();
                    }

                    WritePendingItems();
                }
                finally
                {
                    contentReader.Close();
                }
            }
        }

        private void WriteLastItemsToPipeline()
        {
            foreach (object item in ReadItems().Reverse().Take(Tail).Reverse())
            {
                WriteItem(item);
            }
        }

        private void WriteItem(object item)
        {
            if (ReadCount > 1)
            {
                pendingItems.Add(item);

                if (pendingItems.Count >= ReadCount)
                {
                    WritePendingItems();
                }
            }
            else
            {
                WriteObject(item);
            }
        }

        private void WritePendingItems()
        {
            if ((pendingItems != null) && (pendingItems.Count > 0))
            {
                WriteObject(pendingItems.ToArray());
                pendingItems.Clear();
            }
        }

        private void WriteItemsToPipeline()
        {
            int currentItemNumber = 1;
            if (!ReadMoreItems(currentItemNumber))
            {
                return;
            }

            foreach (object item in ReadItems())
            {
                WriteItem(item);

                currentItemNumber++;
                if (!ReadMoreItems(currentItemNumber))
                {
                    return;
                }
            }
        }

        private IEnumerable<object> ReadItems()
        {
            IList items;
            do
            {
                items = contentReader.Read(1);
                if (items.Count > 0)
                {
                    yield return items[0];
                }
            }
            while (items.Count > 0);
        }

        private bool ReadMoreItems(int currentLineNumber)
        {
            return (TotalCount == -1) || (currentLineNumber <= TotalCount);
        }
    }
}
