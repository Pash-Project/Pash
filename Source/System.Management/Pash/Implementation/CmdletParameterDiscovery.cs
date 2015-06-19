using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Management.Automation;

namespace Pash.Implementation
{
    public class CmdletParameterDiscovery
    {
        private readonly Type _type;

        /// <summary>
        /// A list with all ParameterInfo objects. One parameter member might have multiple associated ParameterInfo
        /// objects.
        /// </summary>
        /// <value>All ParameterInfo objects.</value>
        public List<CommandParameterInfo> AllParameters { get; private set; }

        /// <summary>
        /// A dictionary with one entry for each parameter member. If a member has multiple associtated 
        /// ParameterAttributes, only one representative ParameterAttribute will be stored in the dictionary.
        /// </summary>
        /// <value>Dictionary with parameter member name and a representative ParameterInfo object.</value>
        public Dictionary<string, CommandParameterInfo> NamedParameters { get; private set; }

        public CmdletParameterDiscovery(Type type)
        {
            _type = type;
            AllParameters = new List<CommandParameterInfo>();
            NamedParameters = new Dictionary<string, CommandParameterInfo>();
            DiscoverParameters();
        }

        private void DiscoverParameters()
        {
            DiscoverFieldParameters();
            DiscoverPropertyParameters();
        }

        private void DiscoverFieldParameters()
        {
            foreach (FieldInfo fieldInfo in _type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var paramType = fieldInfo.FieldType;
                var paramAttrs = from attr in fieldInfo.GetCustomAttributes(typeof(ParameterAttribute), false)
                                 select attr as ParameterAttribute;
                // Add the parameter once per ParameterAttribute
                foreach (var paramAttr in paramAttrs)
                {
                    AddParameter(fieldInfo, paramType, paramAttr);
                }
            }
        }

        private void DiscoverPropertyParameters()
        {
            foreach (PropertyInfo propertyInfo in _type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // The property must have a public setter
                MethodInfo setter = propertyInfo.GetSetMethod();
                if (setter == null || !setter.IsPublic)
                {
                    // TODO: we just ignore this here. Should we throw an error/warning?
                    continue;
                }

                // Get getter infos
                MethodInfo getter = propertyInfo.GetAccessors().FirstOrDefault(i => i.IsSpecialName && i.Name.StartsWith("get_"));

                var paramType = propertyInfo.PropertyType;
                // Add the parameter once per ParameterAttribute
                var paramAttrs = from attr in propertyInfo.GetCustomAttributes(typeof(ParameterAttribute), false)
                                 select attr as ParameterAttribute;
                foreach (var paramAttr in paramAttrs)
                {
                    // If the values should come from pipeline, the Getter must be existing and public
                    if ((paramAttr.ValueFromPipeline || paramAttr.ValueFromPipelineByPropertyName) &&
                        (getter == null || !getter.IsPublic))
                    {
                        // TODO: we just ignore this here. Should we throw an error/warning?
                        continue;
                    }
                    AddParameter(propertyInfo, paramType, paramAttr);
                }
            }
        }

        private void AddParameter(MemberInfo info, Type parameterType, ParameterAttribute paramAttribute)
        {
            var paramInfo = new CommandParameterInfo(info, parameterType, paramAttribute);
            if (!NamedParameters.ContainsKey(paramInfo.Name))
            {
                NamedParameters[paramInfo.Name] = paramInfo;
            }
            AllParameters.Add(paramInfo);
        }
    }
}

