# Code Analysis
The Pash-Project makes use of code analysis which is included in later editions of Visual Studio. Code analysis is designed to prevent programers from making common design errors impacting performance, security, internationalization, etc. Unfortunately, this functionality is not yet available for other platforms.

Two code analysis rulesets are defined for the Pash-Project:

1. **PashRules.Warning**
>The warning ruleset is only enabled on debug builds. This serves two purposes: (1.) warnings will not break your local builds which might otherwise interfer with development underway; (2.) new rules will be enabled here first allowing existing violations to be resolved in a piecmeal fashion.

2. **PashRules.Error**
>The error ruleset is a copy of the warning ruleset, but violations will cause errors and break the build. This ruleset is enabled for the release configuraion and violations will break on the build agent.

## Guidelines
Use the following guidelines for developing with code analysis in the Pash-Project:
 * Code analysis should not be enabled on test projects
 * Code analysis should not be enabled on third-party libraries
 * Do not introduce new code analysis warnings in your code
 * If you clear out the final rule violation for a warning, enable that rule in PashRules.error
 * New projects should enable PashRules.Warning for the debug configuration and PashRules.Error for the release configuration
 * False positives should be suppressed in a suppression file and not in source
 * Do not suppress rules you don't understand; an MSDN search for a given rule identifier (e.g. CA1001) will yield great information

## Configuring Code Analysis
From the project properties window...

1. Select Code Analysis
2. Choose the target configuration from the drop-down
3. Check "Enable Code Analysis on Build" and "Suppress results from generated code (managed only)"
4. Choose the correct ruleset from the dropdown: Pash Code Analysis Errors for Release and Pash Code Analysis Warnings for Debug. If you do not see these rulesets in the drop-down, select &lt;Browse...&gt; and locate them in the tools/FxCop folder.
5. Make sure to repeat steps 2 - 4 for all configurations and save the project

## Suppressing False Violations
1. In the Code Analysis pane select the violation you'd like to suppress
2. Expand the Actions link and select Suppress Message &gt; In Suppression File

False positives should be suppressed in a suppression file and not in source. Doing this will add to a GlobalSuppressions.cs file to the current project. Placing all the suppressions within one file makes housekeeping easier later on &mdash; suppressions can easily be reviewed by deleting the global suppression file and reviewing the violations that return. Finally, Do not suppress anything without good reason.