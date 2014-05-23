using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.ObjectModel;
using System.Management.Automation;
using Pash.Implementation;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Language;
using System.ComponentModel;

internal static class PSObjectExtensions
{
	public static Collection<PSPropertyInfo> SelectProperties(this PSObject psobj, object[] properties,
	                                                    ExecutionContext executionContext)
	{
        var evaluatedProps = new Collection<PSPropertyInfo>();
        foreach (var curProp in properties)
        {
            evaluatedProps.Add(EvaluateProperty(curProp, psobj, executionContext));
        }
        return evaluatedProps;
	}

    private static PSPropertyInfo EvaluateProperty(object property, PSObject psobj, ExecutionContext executionContext)
	{
        property = PSObject.Unwrap(property);
		var propName = property as string;
		if (propName != null)
		{
			var prop = psobj.Properties[propName];
			return prop ?? new PSNoteProperty(propName, "");
		}
		// check if it's a Hashtable. We can then have expression=<ScriptBlock> and label=<string> or name=<string>
		var hashtable = property as Hashtable;
		if (hashtable != null)
		{
            // we have a strange logice here, but we'll just do it as Powershell
            // if label and name is contained in the hashset, "name" member will be set (and throw, if already existing)
			if (hashtable.ContainsKey("label"))
			{
				hashtable["name"] = hashtable["label"];
                hashtable.Remove("label");
			}
            // also, we really care that we only have "name" and "expression" keys, otherwise we throw an error...
            foreach (var key in hashtable.Keys)
            {
                var keyStr = key.ToString();
                if (keyStr.Equals("name", StringComparison.InvariantCultureIgnoreCase))
                {
                    propName = hashtable[key].ToString();
                }
                else if (keyStr.Equals("expression", StringComparison.InvariantCultureIgnoreCase))
                {
                    // should be a script block, we will check this later
                    property = hashtable[key];
                }
                else
                {
                    throw new NotSupportedException(String.Format("Invalid key '{0}'", keyStr));
                }
            }
		}
		// now it must be a scriptblock that can be evaluated (either from the hastable or the property itself)
		var scriptBlock = property as ScriptBlock;
		if (scriptBlock == null)
		{
			var msg = String.Format("The property '{0}' couldn't be converted to a string, Hashtable, or a ScriptBlock",
			                        property.GetType().Name);
			throw new PSInvalidOperationException(msg);
		}
		if (propName == null)
		{
			propName = scriptBlock.ToString();
		}
        var pipeline = executionContext.CurrentRunspace.CreateNestedPipeline();
		pipeline.Commands.Add(new Command(scriptBlock.Ast as ScriptBlockAst, true));
		// TODO: hack? we need to set the $_ variable
        var oldVar = executionContext.GetVariable("_");
        executionContext.SetVariable("_", psobj);
		Collection<PSObject> results = null;
		try
		{
			results = pipeline.Invoke();
		}
		finally
		{
            if (oldVar != null)
            {
                executionContext.SessionState.PSVariable.Set(oldVar);
            }
            else
            {
                executionContext.SessionState.PSVariable.Set("_", null);
            }
		}
		PSObject value;
		if (results.Count == 0)
		{
			value = PSObject.AsPSObject(null);
		}
		else if (results.Count == 1)
		{
			value = results[0];
		}
		else
		{
			value = PSObject.AsPSObject(new List<PSObject>(results).ToArray());
		}
		return new PSNoteProperty(propName, value);
	}
}

