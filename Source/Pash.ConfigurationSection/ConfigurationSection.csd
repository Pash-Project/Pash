<?xml version="1.0" encoding="utf-8"?>
<configurationSectionModel dslVersion="1.0.0.0" Id="d0ed9acb-0435-4532-afdd-b5115bc4d562" namespace="Pash.Configuration" xmlSchemaNamespace="http://pash.sourceforge.net/pash/configuration" xmlns="http://schemas.microsoft.com/dsltools/ConfigurationSectionDesigner">
  <configurationElements>
    <configurationElement name="AliasElement">
      <attributeProperties>
        <attributeProperty name="name" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="name" isReadOnly="true">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="definition" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="definition" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="scope" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="scope" isReadOnly="false" defaultValue="&quot;global&quot;">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElement name="VariableElement">
      <attributeProperties>
        <attributeProperty name="name" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="name" isReadOnly="true">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="type" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="type" isReadOnly="false" defaultValue="&quot;System.String&quot;">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="value" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="value" isReadOnly="false" defaultValue="null">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="scope" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="scope" isReadOnly="true" defaultValue="&quot;global&quot;">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationSection name="ExecutionContextConfigurationSection" codeGenOptions="Singleton, XmlnsProperty" xmlSectionName="defaultExecutionContext">
      <elementProperties>
        <elementProperty name="PSSnapins" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="psSnapins" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/PSSnapinCollection" />
          </type>
        </elementProperty>
        <elementProperty name="Functions" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="functions" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/FunctionsCollection" />
          </type>
        </elementProperty>
        <elementProperty name="Aliases" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="aliases" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/AliasesCollection" />
          </type>
        </elementProperty>
        <elementProperty name="Variables" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="variables" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/VariablesCollection" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationSection>
    <configurationElement name="FunctionElement">
      <attributeProperties>
        <attributeProperty name="name" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="name" isReadOnly="true">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="type" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="type" isReadOnly="true" defaultValue="&quot;inline&quot;">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="value" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="value" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="scope" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="scope" isReadOnly="true" defaultValue="&quot;global&quot;">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElementCollection name="FunctionsCollection" collectionType="BasicMap" xmlItemName="function" codeGenOptions="Indexer, AddMethod, RemoveMethod">
      <itemType>
        <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/FunctionElement" />
      </itemType>
    </configurationElementCollection>
    <configurationElementCollection name="AliasesCollection" collectionType="BasicMap" xmlItemName="alias" codeGenOptions="Indexer, AddMethod, RemoveMethod">
      <itemType>
        <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/AliasElement" />
      </itemType>
    </configurationElementCollection>
    <configurationElementCollection name="VariablesCollection" collectionType="BasicMap" xmlItemName="variable" codeGenOptions="Indexer, AddMethod, RemoveMethod">
      <itemType>
        <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/VariableElement" />
      </itemType>
    </configurationElementCollection>
    <configurationElementCollection name="PSSnapinCollection" xmlItemName="psSnapin" codeGenOptions="Indexer, AddMethod, RemoveMethod">
      <itemType>
        <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/PSSnapinElement" />
      </itemType>
    </configurationElementCollection>
    <configurationElement name="PSSnapinElement">
      <attributeProperties>
        <attributeProperty name="type" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="type" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
  </configurationElements>
  <typeDefinitions>
    <externalType name="String" namespace="System" />
    <externalType name="Boolean" namespace="System" />
    <externalType name="Int32" namespace="System" />
    <externalType name="Int64" namespace="System" />
    <externalType name="Single" namespace="System" />
    <externalType name="Double" namespace="System" />
    <externalType name="DateTime" namespace="System" />
    <externalType name="TimeSpan" namespace="System" />
  </typeDefinitions>
</configurationSectionModel>