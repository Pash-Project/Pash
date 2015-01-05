// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Collections;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Host;
using Pash.Implementation;

namespace System.Management.Automation
{
    internal class CmdletParameterBinder
    {
        private Dictionary<MemberInfo, object> _defaultValues;
        private Collection<MemberInfo> _boundParameters;
        private Dictionary<MemberInfo, object> _commandLineValuesBackup;
        private List<MemberInfo> _commonParameters;
        private CmdletInfo _cmdletInfo;
        private Cmdlet _cmdlet;
        private CommandParameterSetInfo _activeSet;
        private CommandParameterSetInfo _defaultSet;
        private bool _hasDefaultSet;
        private List<CommandParameterSetInfo> _candidateParameterSets;

        private CommandParameterSetInfo ActiveOrDefaultParameterSet
        {
            get
            {
                return _activeSet ?? DefaultParameterSet;
            }
        }

        private CommandParameterSetInfo DefaultParameterSet
        {
            get
            {
                if (_defaultSet == null && _hasDefaultSet)
                {
                    _defaultSet = _cmdletInfo.GetDefaultParameterSet();
                    _hasDefaultSet = _defaultSet != null;
                }
                return _defaultSet;
            }
        }

        public CmdletParameterBinder(CmdletInfo cmdletInfo, Cmdlet cmdlet)
        {
            _cmdletInfo = cmdletInfo;
            _cmdlet = cmdlet;
            _defaultValues = new Dictionary<MemberInfo, object>();
            _boundParameters = new Collection<MemberInfo>();
            _candidateParameterSets = _cmdletInfo.ParameterSets.ToList();
            _commandLineValuesBackup = new Dictionary<MemberInfo, object>();
            _activeSet = null;
            _defaultSet = null;
            _hasDefaultSet = true;
            _commonParameters = (from parameter in CommonCmdletParameters.CommonParameterSetInfo.Parameters
                                 select parameter.MemberInfo).ToList();
        }

        /// <summary>
        /// Binds the parameters from the command line to the command.
        /// </summary>
        /// <remarks>
        /// All abount Cmdlet parameters: http://msdn2.microsoft.com/en-us/library/ms714433(VS.85).aspx
        /// About the lifecycle: http://msdn.microsoft.com/en-us/library/ms714429(v=vs.85).aspx
        /// </remarks>
        public void BindCommandLineParameters(CommandParameterCollection parameters)
        {
            // How command line parameters are bound:
            // In general: We will bind first named parameters, then positionals, and then maybe gather some.
            // While parameters are being bound, a list of _candidateParameterSets is maintained that keeps track
            // of those sets that are eligible for being taken.
            // As we might have an early choice that can be handled, an _activeSet is maintained in addition.
            // The _candidateSet will be restricted to the _activeSet if we found one early

            // 1. Check if one obvious choice for a parameter set to be chosen (e.g. if there is only one)
            _activeSet = SelectObviousActiveParameterSet();

            // 2. Bind all named parameters, fail if some name is bound twice or if the selection is already ambiguos
            BindNamedParameters(parameters);

            // 3. Bind positional parameters per parameter set
            //    To do so, sort parameters of set by position, ignore bound parameters and set the first unbound position
            //    Note: if we don't know the active parameter set, yet, we will bind it by Default, this is PS behavior
            //    Binding will also restrict the candidate set and check for ambiguity 
            BindPositionalParameters(parameters, ActiveOrDefaultParameterSet);

            // 4. This would be the place for common parameters, support of ShouldProcess and dynamic parameters
            // TODO: care about "common" parameters and dynamic parameters

            // 6. There might be parameter sets with parameters that are still unbound but will be set by pipeline.
            //    If not, and if we have an (active or default) parameter set that is still a candidate, then get the
            //    missing parameters.
            if (!HasParameterSetsWithUnboundMandatoryPipelineParameters() &&
                _candidateParameterSets.Contains(ActiveOrDefaultParameterSet))
            {
                HandleMissingMandatoryParameters(ActiveOrDefaultParameterSet, false, true);
            }



            // 7. We finished binding parameters without pipeline. Therefore we can restrict the candidate set to those
            //    sets that have all mandatory parameters set or are able to set them by pipeline
            RestrictCandidatesByBoundParameter(false);

            // 8. Check if we have unbound parameters that can be set by pipeline. If not and we can already do
            // our final choice (and throw an error on problems)
            if (!HasUnboundPipelineParameters())
            {
                DoFinalChoiceOfParameterSet();
            }

            // 8. Back up the bound parameters
            BackupCommandLineParameterValues();

            // 9. For the beginning phase: Tell the cmdlet which parameter set is likely to be used (if we know it)
            // If there is only one candidate, use it at least temporarily. Otherwise the active or default
            SetCmdletParameterSetName(_candidateParameterSets.Count == 1 ? _candidateParameterSets[0]
                                                                         : ActiveOrDefaultParameterSet);
        }

        /// <summary>
        /// Binds the parameter values provided by pipeline to be processed as a separate record
        /// </summary>
        /// <param name="curInput">Current input object from the pipeline</param>
        /// <param name="isFirstInPipeline">Whether the command is the first in pipeline</param>
        public void BindPipelineParameters(object pipelineInput, bool isFirstInPipeline)
        {
            // First reset to command line parameter values
            RestoreCommandLineParameterValues();

            // How pipeline parameters are bound:
            // 1. If this command is the first in pipeline and there are no input objects:
            //    Then get all left mandatory parameters for the (active or default) parameter set from UI
            if (pipelineInput == null && isFirstInPipeline)
            {
                if (ActiveOrDefaultParameterSet != null &&
                    _candidateParameterSets.Contains(ActiveOrDefaultParameterSet))
                {
                    HandleMissingMandatoryParameters(ActiveOrDefaultParameterSet, true, true);
                }
            }
            // 2. If there is an input object:
            else
            {
                // 1. Try to bind pipeline object properly in all candidate sets: with and without conversion
                bool success = BindPipelineParameters(AllParameters(_candidateParameterSets), pipelineInput);
                // 2. If we had no success, throw an error
                if (!success)
                {
                    throw new ParameterBindingException("The pipeline input cannot be bound to any parameter");
                }
            }

            // 3. This would be the place to process dynamic pipeline parameters
            // TODO: care about dynamic pipeline parameters

            // 4. If there is an active set, make sure all mandatory parameters are set
            if (_activeSet != null)
            {
                HandleMissingMandatoryParameters(_activeSet, true, false);
            }

            // 5. Check for candidate sets, *not* ignoring pipeline related parameters (as we might still have no active)
            //    Note that this set automatically only contain the active set if there is one, so don't worry
            RestrictCandidatesByBoundParameter(true);

            // 6. Do the final choice of our parameter set
            DoFinalChoiceOfParameterSet();
        }

        private void DoFinalChoiceOfParameterSet()
        {
            // If there is only one candidate: Choose that parameter set (This is also the case with an active set)
            if (_candidateParameterSets.Count == 1)
            {
                ChooseParameterSet(_candidateParameterSets[0]);
                return;
            }
            // If there is more than one candidate:
            else if (_candidateParameterSets.Count > 1)
            {
                // If the default set is among the candidates, choose it
                if (DefaultParameterSet != null && _candidateParameterSets.Contains(DefaultParameterSet))
                {
                    ChooseParameterSet(DefaultParameterSet);
                    return;
                }
                // If the default set is the AllParameter set then choose it.
                else if (DefaultParameterSet != null && (DefaultParameterSet.IsAllParameterSets))
                {
                    ChooseParameterSet(DefaultParameterSet);
                    return;
                }
                // Otherwise we could choose multiple sets, so throw an ambigiuous error
                else
                {
                    ThrowAmbiguousParameterSetException();
                }
            }
            // If the candidate set is empty: Throw an error and tell the user what's missing
            else
            {
                ThrowMissingParametersExcpetion(ActiveOrDefaultParameterSet);
            }
        }

        IEnumerable<CommandParameterInfo> AllParameters(IEnumerable<CommandParameterSetInfo> sources)
        {
            IEnumerable<CommandParameterInfo> allParams = Enumerable.Empty<CommandParameterInfo>();
            foreach (var curParamSet in sources)
            {
                allParams = allParams.Union(curParamSet.Parameters);
            }
            return allParams;
        }

        private void SetCmdletParameterSetName(CommandParameterSetInfo chosenSet)
        {
            if (_cmdlet is PSCmdlet)
            {
                ((PSCmdlet)_cmdlet).ParameterSetName = chosenSet != null ? chosenSet.Name
                    : ParameterAttribute.AllParameterSets;
            }
        }

        private void ChooseParameterSet(CommandParameterSetInfo chosenSet)
        {
            _activeSet = chosenSet;
            _candidateParameterSets.Clear();
            _candidateParameterSets.Add(chosenSet);
            SetCmdletParameterSetName(chosenSet);
        }

        private void ThrowMissingParametersExcpetion(CommandParameterSetInfo paramSet)
        {
            var missing = GetMandatoryUnboundParameters(paramSet, true);
            var msg = "Missing value for mandatory parameter(s): " + String.Join(", ", missing);
            throw new ParameterBindingException(msg, "MissingMandatoryParameter");
        }

        private void BindNamedParameters(CommandParameterCollection parameters)
        {
            var namedParameters = from parameter in parameters
                                      where !String.IsNullOrEmpty(parameter.Name)
                                  select parameter;

            foreach (var curParam in namedParameters)
            {
                string curName = curParam.Name;

                // try to get the parameter from any set. throws an error if the name is ambiguous or doesn't exist
                var paramInfo = _cmdletInfo.LookupParameter(curName);
                BindParameter(paramInfo, curParam.Value, true);
            }
        }

        private void ThrowAmbiguousParameterSetException()
        {
            throw new ParameterBindingException("The parameter set to be used cannot be resolved.",
                "AmbiguousParameterSet");
        }

        private void BindPositionalParameters(CommandParameterCollection parameters, CommandParameterSetInfo parameterSet)
        {
            var parametersWithoutName = from param in parameters
                                            where String.IsNullOrEmpty(param.Name)
                                        select param;
            if (parameterSet == null)
            {
                if (parametersWithoutName.Any())
                {
                    ThrowAmbiguousParameterSetException();
                }
                return;
            }
            var positionals = (from param in parameterSet.Parameters
                where param.Position >= 0
                orderby param.Position ascending
                select param).ToList();
            int i = 0;
            foreach (var curParam in parametersWithoutName)
            {
                if (i < positionals.Count)
                {
                    var affectedParam = positionals[i];
                    BindParameter(affectedParam, curParam.Value, true);
                    i++;
                }
                else
                {
                    var msg = String.Format("Positional parameter not found for provided argument '{0}'", curParam.Value);
                    throw new ParameterBindingException(msg, "PositionalParameterNotFound");
                }
            }
        }

        private void HandleMissingMandatoryParameters(CommandParameterSetInfo parameterSet, bool considerPipeline, bool askUser)
        {
            // select mandatory parametes
            var unsetMandatories = GetMandatoryUnboundParameters(parameterSet, considerPipeline).ToList();
            // check if we got something to do, otherwise we're fine
            if (unsetMandatories.Count == 0)
            {
                return;
            }
            Dictionary<CommandParameterInfo, PSObject> values = null;
            if (askUser)
            {
                // try to get this value from UI
                values = GatherParameterValues(unsetMandatories);
            }
            if (values == null)
            {
                var parameterNames = from cmdInfo in unsetMandatories select "'" + cmdInfo.Name + "'";
                var missingNames = String.Join(", ", parameterNames);
                var msg = String.Format("Missing value for mandatory parameters {0}.", missingNames);
                throw new ParameterBindingException(msg, "MissingMandatoryParameters");
            }
            foreach (var pair in values)
            {
                if (pair.Value == null)
                {
                    // WORKAROUND: PS throws a MethodInvocationException when binding pipeline parameters
                    // that is when a user shouldn't be asked anymore... let's do the same
                    if (askUser)
                    {
                        var msg = String.Format("Missing value for mandatory parameter '{0}'.", pair.Key.Name);
                        throw new ParameterBindingException(msg, "MissingMandatoryParameter");
                    }
                    else
                    {
                        var msg = "The input object cannot be bound as it doesn't provide some mandatory information: "
                            + pair.Key.Name;
                        throw new MethodInvocationException(msg);
                    }
                }
                BindParameter(pair.Key, pair.Value, true);
            }
        }

        private bool HasUnboundPipelineParameters()
        {
            foreach (var curParamSet in _candidateParameterSets)
            {
                var unboundByPipeline = from param in curParamSet.Parameters
                    where !_boundParameters.Contains(param.MemberInfo) &&
                    (param.ValueFromPipeline || param.ValueFromPipelineByPropertyName)
                    select param;
                if (unboundByPipeline.Any())
                {
                    return true;
                }
            }
            return false;
        }

        private bool HasParameterSetsWithUnboundMandatoryPipelineParameters()
        {
            foreach (var curParamSet in _candidateParameterSets)
            {
                var unboundMandatoriesByPipeline = from param in curParamSet.Parameters
                    where param.IsMandatory && !_boundParameters.Contains(param.MemberInfo) && 
                        (param.ValueFromPipeline || param.ValueFromPipelineByPropertyName)
                        select param;
                var unboundMandatoriesWithoutPipeline = from param in curParamSet.Parameters
                    where param.IsMandatory && !_boundParameters.Contains(param.MemberInfo) && 
                        !(param.ValueFromPipeline || param.ValueFromPipelineByPropertyName)
                        select param;
                if (unboundMandatoriesByPipeline.Any() && !unboundMandatoriesWithoutPipeline.Any())
                {
                    return true;
                }
            }
            return false;
        }

        // restricts the candidate parameter sets by the newly bound parameters
        private void RestrictCandidatesByBoundParameter(CommandParameterInfo info)
        {
            var setsContaining = from paramSet in _cmdletInfo.ParameterSets
                where paramSet.Contains(info) select paramSet;
            _candidateParameterSets = _candidateParameterSets.Intersect(setsContaining).ToList();
            if (_candidateParameterSets.Count == 0)
            {
                ThrowAmbiguousParameterSetException();
            }
            else if (_candidateParameterSets.Count == 1)
            {
                _activeSet = _candidateParameterSets[0];
            }
        }

        // restricts candidates by s.t. it only considers those parameter sets that have enough bound parameters
        private void RestrictCandidatesByBoundParameter(bool pipelineProcessed)
        {
            // if we already found an active set, then this should be the only real candidate
            if (_activeSet != null)
            {
                _candidateParameterSets = new List<CommandParameterSetInfo>() { _activeSet };
                return;
            }
            var newCandidates = new List<CommandParameterSetInfo>();
            foreach (var curParamSet in _candidateParameterSets)
            {
                var unboundMandatories = GetMandatoryUnboundParameters(curParamSet, pipelineProcessed);
                if (!unboundMandatories.Any() && !curParamSet.IsAllParameterSets)
                {
                    newCandidates.Add(curParamSet);
                }
            }
            _candidateParameterSets = newCandidates;
        }

        bool BindPipelineParameters(IEnumerable<CommandParameterInfo> parameterSet, object pipelineInput)
        {
            var valueByPipeParams = (from param in parameterSet
                                     where param.ValueFromPipeline
                                     select param);
            var valuesByNamedPipeParams = from param in parameterSet
                                                  where param.ValueFromPipelineByPropertyName
                                                  select param;

            // first try to bind the parameter without conversion, then with conversion
            foreach (var doConvert in new bool[] { false, true })
            {
                // 1. Try to bind the object to the parameter *without* type conversion with "ValueFromPipeline" param
                foreach (var param in valueByPipeParams)
                {
                    if (TryBindParameter(param, pipelineInput, doConvert))
                    {
                        return true;
                    }
                }

                // 2. If this is not sucessfull, try to bind the object to parameters with "ValueFromPipelineByPropertyName"
                //    *without" conversion, if exists
                if (valuesByNamedPipeParams.Any() && 
                    TryBindObjectAsParameters(valuesByNamedPipeParams, pipelineInput, doConvert))
                {
                    return true;
                }
            }
            return false;
        }


        private Dictionary<CommandParameterInfo, PSObject> GatherParameterValues(IEnumerable<CommandParameterInfo> parameters)
        {
            // check first if we have a host to interact with
            if (_cmdlet.PSHostInternal == null)
            {
                return null;
            }

            var fieldDescs = new Collection<FieldDescription>(
                (from param in parameters
                 select new FieldDescription(param.Name, param.Name, param.ParameterType, param.HelpMessage,
                                             null, param.IsMandatory, param.Attributes)
                ).ToList()
            );
            //var caption = String.Format("Cmdlet {0} at position {1} in the pipeline", _cmdletInfo.Name, ???);
            // we don't know the position in the pipeline
            var caption = String.Format("Cmdlet {0} in the pipeline", _cmdletInfo.Name);
            var message = "Please enter values for the following parameters:";
            try
            {
                var values = _cmdlet.PSHostInternal.UI.Prompt(caption, message, fieldDescs);
                var lookupDict = new Dictionary<string, CommandParameterInfo>();
                foreach (var info in parameters)
                {
                    lookupDict[info.Name] = info;
                }
                var returnDict = new Dictionary<CommandParameterInfo, PSObject>();
                foreach (var pair in values)
                {
                    returnDict[lookupDict[pair.Key]] = pair.Value;
                }
                return returnDict;
            }
            catch (HostException)
            {
                return null;
            }
            catch (NotImplementedException)
            {
                return null;
            }
        }

        private IEnumerable<CommandParameterInfo> GetMandatoryUnboundParameters(CommandParameterSetInfo parameterSet,
                                                                                bool considerPipeline)
        {
            if (parameterSet == null)
            {
                return Enumerable.Empty<CommandParameterInfo>();
            }
            return from param in parameterSet.Parameters
                    where param.IsMandatory &&
                !_boundParameters.Contains(param.MemberInfo) && 
                (considerPipeline ||
                    !(param.ValueFromPipeline || param.ValueFromPipelineByPropertyName)
                )
                select param;
        }

        private CommandParameterSetInfo SelectObviousActiveParameterSet()
        {
            // if we only have one parameter set, this is always the active one (e.g. AllParametersSets)
            CommandParameterSetInfo active = null;
            var setCount = _cmdletInfo.ParameterSets.Count;
            if (setCount == 1)
            {
                active = _cmdletInfo.ParameterSets[0];
            }
            // if we have two sets and one is AllParametersSets, then the other one is naturally the default
            else if (setCount == 2)
            {
                var firstSet = _cmdletInfo.ParameterSets[0];
                var secondSet = _cmdletInfo.ParameterSets[1];
                if (firstSet.IsAllParameterSets)
                {
                    active = secondSet;
                }
                else if (secondSet.IsAllParameterSets)
                {
                    active = firstSet;
                }
            }
            if (active != null)
            {
                _candidateParameterSets = new List<CommandParameterSetInfo>() { active };
            }
            return active;
        }

        private bool TryBindParameter(CommandParameterInfo info, object value, bool doConvert)
        {
            try
            {
                BindParameter(info, value, doConvert);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void BindParameter(CommandParameterInfo info, object value, bool doConvert)
        {
            var memberInfo = info.MemberInfo;
            if (_boundParameters.Contains(memberInfo))
            {
                var msg = String.Format("Parameter '{0}' has already been bound!", info.Name);
                throw new ParameterBindingException(msg);
            }

            // ConvertTo throws an exception if conversion isn't possible. That's just fine.
            if (doConvert)
            {
                value = LanguagePrimitives.ConvertTo(value, info.ParameterType);
            }
            // TODO: validate value with Attributes (ValidateNotNullOrEmpty, etc)
            SetCommandValue(memberInfo, value);
            // make sure to update the candidate set to only consider parameter sets with the newly bound parameter
            RestrictCandidatesByBoundParameter(info);
            _boundParameters.Add(memberInfo);
        }

        private bool TryBindObjectAsParameters(IEnumerable<CommandParameterInfo> parameters, object valueObject, bool doConvert)
        {
            var values = PSObject.AsPSObject(valueObject);
            var success = false;
            foreach (var param in parameters)
            {
                var aliases = new List<string>(param.Aliases);
                aliases.Add(param.Name);
                foreach (var curAlias in aliases)
                {
                    var propertyInfo = values.Properties[curAlias];
                    if (propertyInfo == null)
                    {
                        continue;
                    }
                    var curValue = propertyInfo.Value;
                    bool hadSuccess = TryBindParameter(param, curValue, doConvert);
                    if (hadSuccess)
                    {
                        success = true;
                        break;
                    }
                }

            }
            return success;
        }

        private void SetCommandValue(MemberInfo info, object value)
        {
            // make a backup of the default values first, if we don't have one
            if (!_defaultValues.ContainsKey(info))
            {
                _defaultValues[info] = GetCommandValue(info);
            }
            // now set the field or property
            try
            {
                if (info.MemberType == MemberTypes.Field)
                {
                    FieldInfo fieldInfo = info as FieldInfo;
                    fieldInfo.SetValue(GetCmdlet(info), value);
                }
                else if (info.MemberType == MemberTypes.Property)
                {
                    PropertyInfo propertyInfo = info as PropertyInfo;
                    propertyInfo.SetValue(GetCmdlet(info), value, null);
                }
                else
                {
                    throw new Exception("SetValue only implemented for fields and properties");
                }
            }
            catch (ArgumentException)
            {
                var msg = String.Format("Can't bind value to parameter '{0}'", info.Name);
                throw new ParameterBindingException(msg, "BindingFailed");
            }
        }

        private object GetCommandValue(MemberInfo info)
        {
            if (info.MemberType == MemberTypes.Field)
            {
                FieldInfo fieldInfo = info as FieldInfo;
                return fieldInfo.GetValue(GetCmdlet(info));
            }
            else if (info.MemberType == MemberTypes.Property)
            {
                PropertyInfo propertyInfo = info as PropertyInfo;
                return propertyInfo.GetValue(GetCmdlet(info), null);
            }
            else
            {
                throw new Exception("SetValue only implemented for fields and properties");
            }
        }

        private object GetCmdlet(MemberInfo info)
        {
            if (_commonParameters.Contains(info))
            {
                return _cmdlet.CommonParameters;
            }
            return _cmdlet;
        }

        /// <summary>
        /// Makes and internal backup of the command line parameter values, so they can be restored
        /// </summary>
        private void BackupCommandLineParameterValues()
        {
            _commandLineValuesBackup.Clear();
            foreach (var info in _boundParameters)
            {
                _commandLineValuesBackup[info] = GetCommandValue(info);
            }
        }

        /// <summary>
        /// Restores parameters to those provided by command line, so multiple
        /// records can be processed without influencing each other
        /// </summary>
        public void RestoreCommandLineParameterValues()
        {
            foreach (var bound in _boundParameters)
            {
                object restoredVaue = _commandLineValuesBackup.ContainsKey(bound) ?
                                _commandLineValuesBackup[bound] : _defaultValues[bound];
                if (GetCommandValue(bound) != restoredVaue)
                {
                    SetCommandValue(bound, restoredVaue);
                }
            }
            // reset the bound parameter list
            _boundParameters.Clear();
            foreach (MemberInfo info in _commandLineValuesBackup.Keys)
            {
                _boundParameters.Add(info);
            }
        }

    }
}

