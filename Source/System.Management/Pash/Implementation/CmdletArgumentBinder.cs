// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Collections;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
    internal class CmdletArgumentBinder
    {
        private Dictionary<MemberInfo, object> _defaultArguments;
        private Collection<MemberInfo> _boundArguments;
        private Dictionary<MemberInfo, object> _commandLineArgumentsBackup;
        private CmdletInfo _cmdletInfo;
        private Cmdlet _cmdlet;
        private CommandParameterSetInfo _activeSet;
        private CommandParameterSetInfo _defaultSet;
        private bool _hasDefaultSet;
        private Collection<CommandParameterSetInfo> _candidateParameterSets;

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

        public CmdletArgumentBinder(CmdletInfo cmdletInfo, Cmdlet cmdlet)
        {
            _cmdletInfo = cmdletInfo;
            _cmdlet = cmdlet;
            _defaultArguments = new Dictionary<MemberInfo, object>();
            _boundArguments = new Collection<MemberInfo>();
            _candidateParameterSets = new Collection<CommandParameterSetInfo>();
            _commandLineArgumentsBackup = new Dictionary<MemberInfo, object>();
            _activeSet = null;
            _defaultSet = null;
            _hasDefaultSet = true;
        }

        /// <summary>
        /// Binds the parameters from the command line to the command.
        /// </summary>
        /// <remarks>
        /// All abount Cmdlet parameters: http://msdn2.microsoft.com/en-us/library/ms714433(VS.85).aspx
        /// About the lifecycle: http://msdn.microsoft.com/en-us/library/ms714429(v=vs.85).aspx
        /// </remarks>
        public void BindCommandLineArguments(CommandParameterCollection parameters)
        {

            // How command line arguments are bound:
            // 1. Bind all named parameters, fail if some name is bound twice
            BindNamedParameters(parameters);

            // 2. Check if there is an "active" parameter set, yet.
            //    If there is more than one "active" parameter set, fail with ambigous error
            CheckForActiveParameterSet();

            // 3. Bind positional parameters per parameter set
            //    To do so, sort parameters of set by position, ignore bound parameters and set the first unbound position
            //    Note: if we don't know the active parameter set, yet, we will bind it by Default, this is PS behavior
            BindPositionalParameters(parameters, ActiveOrDefaultParameterSet);

            // 4. This would be the place for common parameters, support of ShouldProcess and dynamic parameters
            // TODO: care about "common" parameters and dynamic parameters

            // 5. Check if there is an "active" parameter set, yet, and fail if there are more than one
            CheckForActiveParameterSet();

            // 6. For either the (active or default) parameter set: Gather missing mandatory parameters by UI
            // 7. Check if (active or default) parameter set has unbound mandatory parameters and fail if so
            //    But not pipeline related parameters, they might get passed later on
            HandleMissingMandatoryParameters(ActiveOrDefaultParameterSet, false, true);

            // 8. Check for an active parameter set. If the default set had unique mandatory parameters, we might
            //    know that the default set is the active set
            CheckForActiveParameterSet();

            // 9. Define "candidate" parameter sets: All that have mandatory parameters set, ignoring pipeline related
            //    If one set is already active, this should be the only candidate set
            // NOTE: makes sense if there has been no unique parameter, but multiple sets have their mandatories set
            _candidateParameterSets = GetCandidateParameterSets(false);

            // 10. Back up the bound parameters
            BackupCommandLineArguments();

            // 11. For the beginning phase: Tell the cmdlet which parameter set is likely to be used (if we know it)
            ChooseParameterSet(ActiveOrDefaultParameterSet);
        }

        /// <summary>
        /// Binds the arguments provided by pipeline to be processed as a separate record
        /// </summary>
        /// <param name="curInput">Current input object from the pipeline</param>
        /// <param name="isFirstInPipeline">Whether the command is the first in pipeline</param>
        public void BindPipelineArguments(object pipelineInput, bool isFirstInPipeline)
        {
            // First reset to command line arguments
            RestoreCommandLineArguments();

            // How pipeline parameters are bound
            // 1. If this command is the first in pipeline and there are no input objects:
            //    Then get all left mandatory parameters for the (active or default) parameter set from UI
            if (pipelineInput == null && isFirstInPipeline)
            {
                HandleMissingMandatoryParameters(ActiveOrDefaultParameterSet, true, true);
            }

            // 2. If there is an input object:
            else
            {
                bool success = false;
                // 1. If the default parameter set is still a candidate: Try to bind pipeline object properly
                if (DefaultParameterSet != null && _candidateParameterSets.Contains(DefaultParameterSet))
                {
                    success = BindPipelineParameter(DefaultParameterSet.Parameters, pipelineInput);
                }
                // 2. If binding to the default set was not possible, try the same with all parameters
                if (!success)
                {
                    success = BindPipelineParameter(AllParameters(_candidateParameterSets), pipelineInput);
                }
                // 3. If we still had no success, throw an error
                if (!success)
                {
                    throw new ParameterBindingException("The pipeline input cannot be bound to any parameter", 
                        "PipelineInputCannotBeBound");
                }
            }

            // 3. This would be the place to process dynamic pipeline parameters
            // TODO: care about dynamic pipeline parameters

            // 3. Check for active sets, fail the record (ambiguous) if there are multiples
            CheckForActiveParameterSet();

            // 4. If there is an active set, make sure all mandatory parameters are set
            if (_activeSet != null)
            {
                HandleMissingMandatoryParameters(_activeSet, true, false);
            }

            // 5. Check for candidate sets, not ignoring pipeline related parameters (as we might still have no active)
            //    Note that this set automatically only contain the active set if there is one, so don't worry
            _candidateParameterSets = GetCandidateParameterSets(true);

            // 6. If there is only one candidate: Choose that parameter set (This is also the case with an active set)
            if (_candidateParameterSets.Count == 1)
            {
                ChooseParameterSet(_candidateParameterSets[0]);
                return;
            }
            // 7. If there is more than one candidate:
            else if (_candidateParameterSets.Count > 1)
            {
                // 1. If the default set is among the candidates, choose it
                if (DefaultParameterSet != null && _candidateParameterSets.Contains(DefaultParameterSet))
                {
                    ChooseParameterSet(DefaultParameterSet);
                    return;
                }
                // 2. Otherwise we could choose multiple sets, so throw an ambigiuous error
                else
                {
                    throw new ParameterBindingException("The parameter set to be used cannot be resolved.",
                         "AmbiguousParameterSet");
                }
            }
            // 8. If the candidate set is empty: Throw an error and tell the user what's missing
            else
            {
                ThrowMissingParametersExcpetion(ActiveOrDefaultParameterSet);
            }
        }

        IEnumerable<CommandParameterInfo> AllParameters(Collection<CommandParameterSetInfo> sources)
        {
            IEnumerable<CommandParameterInfo> allParams = Enumerable.Empty<CommandParameterInfo>();
            foreach (var curParamSet in sources)
            {
                allParams = allParams.Union(curParamSet.Parameters);
            }
            return allParams;
        }

        private void ChooseParameterSet(CommandParameterSetInfo chosenSet)
        {
            if (chosenSet != null)
            {
                _cmdlet.ParameterSetName = chosenSet.Name;
            }
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
                BindArgument(paramInfo, curParam.Value, true);
            }
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
                    throw new ParameterBindingException("The parameter set to be used cannot be resolved.",
                         "AmbiguousParameterSet");
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
                    BindArgument(affectedParam, curParam.Value, true);
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
            var unsetMandatories = GetMandatoryUnboundParameters(parameterSet, considerPipeline);
            foreach (var curParam in unsetMandatories)
            {
                object value = null;
                // check if we should try to gather it
                if (askUser)
                {
                    // try to get this value from UI
                    GatherParameterValue(curParam);
                }
                // check if we had success, fail otherwise
                if (value == null)
                {
                    var msg = String.Format("Missing value for mandatory parameter '{0}'.", curParam.Name);
                    throw new ParameterBindingException(msg, "MissingMandatoryParameter");
                }
                BindArgument(curParam, value, true);
            }
        }

        private Collection<CommandParameterSetInfo> GetCandidateParameterSets(bool considerPipeline)
        {
            var candidates = new Collection<CommandParameterSetInfo>();
            // if we already found an active set, then this should be the only real candidate
            if (_activeSet != null)
            {
                candidates.Add(_activeSet);
                return candidates;
            }
            foreach (var curParamSet in _cmdletInfo.ParameterSets)
            {
                // make sure we don't care about the AllParameter set. If it's the only one, it's already active
                if (curParamSet.Name.Equals(ParameterAttribute.AllParameterSets))
                {
                    continue;
                }
                var unboundMandatories = GetMandatoryUnboundParameters(curParamSet, considerPipeline);
                if (!unboundMandatories.Any())
                {
                    candidates.Add(curParamSet);
                }
            }
            return candidates;
        }

        bool BindPipelineParameter(IEnumerable<CommandParameterInfo> parameterSet, object pipelineInput)
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
                    if (TryBindArgument(param, pipelineInput, doConvert))
                    {
                        return true;
                    }
                }

                // 2. If this is not sucessfull, try to bind the object to parameters with "ValueFromPipelineByPropertyName"
                //    *without" conversion, if exists
                if (valuesByNamedPipeParams.Any() && 
                    TryBindObjectAsArguments(valuesByNamedPipeParams, pipelineInput, doConvert))
                {
                    return true;
                }
            }
            return false;
        }


        private object GatherParameterValue(CommandParameterInfo param)
        {
            // TODO: implement asking the user for a value
            return null;
        }

        private IEnumerable<CommandParameterInfo> GetMandatoryUnboundParameters(CommandParameterSetInfo parameterSet, bool considerPipeline)
        {
            if (parameterSet == null)
            {
                return Enumerable.Empty<CommandParameterInfo>();
            }
            return from param in parameterSet.Parameters
                    where param.IsMandatory &&
                !_boundArguments.Contains(param.MemberInfo) && 
                (considerPipeline ||
                    !(param.ValueFromPipeline || param.ValueFromPipelineByPropertyName)
                )
                select param;
        }

        private void CheckForActiveParameterSet()
        {
            // if we only have one parameter set, this is always the active one (e.g. AllParametersSets)
            var setCount = _cmdletInfo.ParameterSets.Count;
            if (setCount == 1)
            {
                if (_activeSet == null)
                {
                    _activeSet = _cmdletInfo.ParameterSets[0];
                }
                return;
            }
            // if we have two sets and one is AllParametersSets, then the other one is naturally the default
            else if (setCount == 2)
            {
                var firstSet = _cmdletInfo.ParameterSets[0];
                var secondSet = _cmdletInfo.ParameterSets[1];
                if (firstSet.Name.Equals(ParameterAttribute.AllParameterSets))
                {
                    _activeSet = secondSet;
                    return;
                }
                else if (secondSet.Name.Equals(ParameterAttribute.AllParameterSets))
                {
                    _activeSet = firstSet;
                    return;
                }

            }
            // otherwise we have more than one parameter set with name
            // even if an activeSet was already chosen, make sure there ist at most one active set
            foreach (var param in _boundArguments)
            {
                string activeSetName;
                if (_cmdletInfo.UniqueSetParameters.TryGetValue(param.Name, out activeSetName))
                {
                    if (_activeSet != null && !_activeSet.Name.Equals(activeSetName))
                    {
                        throw new ParameterBindingException("The parameter set selection is ambiguous!",
                             "AmbiguousParameterSet");
                    }
                    _activeSet = _cmdletInfo.GetParameterSetByName(activeSetName);
                }
            }
        }

        private bool TryBindArgument(CommandParameterInfo info, object value, bool doConvert)
        {
            try
            {
                BindArgument(info, value, doConvert);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void BindArgument(CommandParameterInfo info, object value, bool doConvert)
        {
            var memberInfo = info.MemberInfo;
            if (_boundArguments.Contains(memberInfo))
            {
                var msg = String.Format("Parameter '{0}' has already been bound!", info.Name);
                throw new ParameterBindingException(msg);
            }
            // ConvertTo throws an exception if conversion isn't possible. That's just fine.
            if (doConvert)
            {
                value = LanguagePrimitives.ConvertTo(value, info.ParameterType);
            }
            SetCommandValue(memberInfo, value);
            _boundArguments.Add(memberInfo);
        }

        private bool TryBindObjectAsArguments(IEnumerable<CommandParameterInfo> parameters, object valueObject, bool doConvert)
        {
            var values = PSObject.AsPSObject(valueObject);
            var success = false;
            foreach (var param in parameters)
            {
                var curValue = values.Properties[param.Name];
                if (curValue != null)
                {
                    // important: || success needs to be at the end, because we want to try to bind *every* parameter
                    success = TryBindArgument(param, curValue, doConvert) || success;
                }
            }
            return success;
        }

        private void SetCommandValue(MemberInfo info, object value)
        {
            // make a backup of the default values first, if we don't have one
            if (!_defaultArguments.ContainsKey(info))
            {
                _defaultArguments[info] = GetCommandValue(info);
            }
            // now set the field or property
            try
            {
                if (info.MemberType == MemberTypes.Field)
                {
                    FieldInfo fieldInfo = info as FieldInfo;
                    fieldInfo.SetValue(_cmdlet, value);
                }
                else if (info.MemberType == MemberTypes.Property)
                {
                    PropertyInfo propertyInfo = info as PropertyInfo;
                    propertyInfo.SetValue(_cmdlet, value, null);
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
                return fieldInfo.GetValue(_cmdlet);
            }
            else if (info.MemberType == MemberTypes.Property)
            {
                PropertyInfo propertyInfo = info as PropertyInfo;
                return propertyInfo.GetValue(_cmdlet, null);
            }
            else
            {
                throw new Exception("SetValue only implemented for fields and properties");
            }
        }

        /// <summary>
        /// Makes and internal backup of the command line arguments, so they can be restored
        /// </summary>
        private void BackupCommandLineArguments()
        {
            _commandLineArgumentsBackup.Clear();
            foreach (var info in _boundArguments)
            {
                _commandLineArgumentsBackup[info] = GetCommandValue(info);
            }
        }

        /// <summary>
        /// Restores arguments to those provided by command line, so multiple
        /// records can be processed without influencing each other
        /// </summary>
        public void RestoreCommandLineArguments()
        {
            foreach (var bound in _boundArguments)
            {
                object restoredVaue = _commandLineArgumentsBackup.ContainsKey(bound) ?
                                _commandLineArgumentsBackup[bound] : _defaultArguments[bound];
                if (GetCommandValue(bound) != restoredVaue)
                {
                    SetCommandValue(bound, restoredVaue);
                }
            }
            // reset the bound arguments list
            _boundArguments.Clear();
            foreach (MemberInfo info in _commandLineArgumentsBackup.Keys)
            {
                _boundArguments.Add(info);
            }
        }

    }
}

