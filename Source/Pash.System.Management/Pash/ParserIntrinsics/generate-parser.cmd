@IF NOT DEFINED _ECHO ECHO OFF
:::::::::::::::::::::::
:: Consuming the parser grammar, generate the compiled tables, the skeleton program, and the test 
:::::::::::::::::::::::

set PATH=%PATH%;..\..\..\..\GoldBuilder

GOLDBuild.exe -verbose PashGrammar.grm PashGrammar.cgt
IF ERRORLEVEL 1 GOTO :EOF

GOLDProg.exe PashGrammar.cgt ParserTemplate.pgt PashParser.Generated.cs 
IF ERRORLEVEL 1 GOTO :EOF


