@IF NOT DEFINED _ECHO ECHO OFF
:::::::::::::::::::::::
:: Consuming the parser grammar, generate the compiled tables, the skeleton program, and the test 
:::::::::::::::::::::::

set PATH=%PATH%;..\..\..\..\GoldBuilder

:: GOLDBuild.exe doesn't seem to set ERRORLEVEL, so let's check if it actually generates a file, instead.
IF EXIST PashGrammar.cgt DEL PashGrammar.cgt
GOLDBuild.exe PashGrammar.grm PashGrammar.cgt
IF ERRORLEVEL 1 GOTO :EOF
IF NOT EXIST PashGrammar.cgt goto :EOF

GOLDProg.exe PashGrammar.cgt PashParserContext.pgt PashParserContext.Generated.cs
IF ERRORLEVEL 1 GOTO :EOF

GOLDProg.exe PashGrammar.cgt RuleConstants.pgt RuleConstants.Generated.cs
IF ERRORLEVEL 1 GOTO :EOF

GOLDProg.exe PashGrammar.cgt SymbolConstants.pgt SymbolConstants.Generated.cs
IF ERRORLEVEL 1 GOTO :EOF
