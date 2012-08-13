using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Pash.Configuration
{
    public class ExecutionContextConfigurationSection : IConfigurationSectionHandler
    {
        public IEnumerable<VariableElement> Variables { get; set; }
        public IEnumerable<AliasElement> Aliases { get; set; }
        public IEnumerable<FunctionElement> Functions { get; set; }
        public IEnumerable<PSSnapinElement> PSSnapins { get; set; }

        public object Create(object parent, object configContext, XmlNode section)
        {
            var xDoc = new XDocument();
            using (var xmlWriter = xDoc.CreateWriter())
                section.WriteTo(xmlWriter);
            var ns = xDoc.Root.Name.Namespace;

            Aliases = GetAliases(xDoc.Root.Element(ns + "aliases"));
            Functions = GetFunctions(xDoc.Root.Element(ns + "functions"));
            PSSnapins = GetSnapins(xDoc.Root.Element(ns + "psSnapins"));
            Variables = GetVariables(xDoc.Root.Element(ns + "variables"));

            return this;
        }

        public IEnumerable<AliasElement> GetAliases(XElement aliasSection)
        {
            if (aliasSection == null)
                return null;

            var ns = aliasSection.Name.Namespace;

            var aliases = from aliasElement in aliasSection.Descendants(ns + "alias")
                          let aliasName = aliasElement.Attribute("name")
                          let aliasDefinition = aliasElement.Attribute("definition")
                          let aliasScope = aliasElement.Attribute("scope")
                            select new AliasElement
                            (
                                name: (aliasName != null) ? aliasName.Value : null,
                                definition: (aliasDefinition != null) ? aliasDefinition.Value : null,
                                scope: (aliasScope != null) ? aliasScope.Value : null
                            );

            return aliases;
        }

        public IEnumerable<FunctionElement> GetFunctions(XElement functionSection)
        {
            if (functionSection == null)
                return null;

            var ns = functionSection.Name.Namespace;

            var functions = from functionElement in functionSection.Descendants(ns + "function")
                                      let funcName = functionElement.Attribute("name")
                                      let funcType = functionSection.Attribute("type")
                                      let funcValue = functionElement.Attribute("value")
                                      let funcScope = functionSection.Attribute("scope")
                                      select new FunctionElement
                                                 (
                                                     name: (funcName != null) ? funcName.Value : null,
                                                     type: (funcType != null) ? funcType.Value : null,
                                                     value: (funcValue != null) ? funcValue.Value : null,
                                                     scope: (funcScope != null) ? funcScope.Value : null
                                                 );

            return functions;
        }

        public IEnumerable<PSSnapinElement> GetSnapins(XElement snapinSection)
        {
            if (snapinSection == null)
                return null;

            var ns = snapinSection.Name.Namespace;

            var snapins = from snapinElement in snapinSection.Descendants(ns + "psSnapin")
                          let snpType = snapinElement.Attribute("type")
                                      select new PSSnapinElement
                                      {
                                          type = (snpType != null) ? snpType.Value : null
                                      };

            return snapins;
        }

        public IEnumerable<VariableElement> GetVariables(XElement variablesSection)
        {
            if (variablesSection == null)
                return null;

            var ns = variablesSection.Name.Namespace;

            var functionsCollection = from variableElement in variablesSection.Descendants(ns + "variable")
                                      let varName = variableElement.Attribute("name")
                                      let varType = variableElement.Attribute("type")
                                      let varValue = variableElement.Attribute("value")
                                      let varScope = variableElement.Attribute("scope")
                                      select new VariableElement
                                      (
                                          name: (varName != null) ? varName.Value : null,
                                          type: (varType != null) ? varType.Value : null,
                                          value: (varValue != null) ? varValue.Value : null,
                                          scope: (varScope != null) ? varScope.Value : null
                                      );

            return functionsCollection;
        }

    }
}