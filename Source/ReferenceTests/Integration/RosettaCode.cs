// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using NUnit.Framework;

namespace ReferenceTests.Integration
{
    /// <summary>
    /// A series of integration tests taken from the author's own contributions to Rosetta Code.
    /// Many of those should be fairly typical of what features might be used in the real world.
    /// </summary>
    [TestFixture]
    public class RosettaCode : ReferenceTestBase
    {
        [Test, Explicit("Wrong output because arrays are not converted correctly to strings")]
        // Taken from http://rosettacode.org/wiki/Array_concatenation#PowerShell
        public void ArrayConcatenation()
        {
            var code = NewlineJoin(
                "$a = 1,2,3",
                "$b = 4,5,6",
                "",
                "$c = $a + $b",
                "\"$c\"");
            var expected = NewlineJoin("1 2 3 4 5 6");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        // Taken from http://rosettacode.org/wiki/Associative_arrays/Creation#PowerShell
        public void AssociativeArraysCreation()
        {
            var code = NewlineJoin(
                "# empty hash table",
                "$hashtable = @{}",
                "# initializer",
                "$hashtable = @{",
                "    \"key1\" = \"value 1\"",
                "    \"key2\" = 5",
                "}",
                "# assignment",
                "$hashtable.foo    = \"bar\"",
                "$hashtable['bar'] = 42",
                "$hashtable.\"a b\"  = 3.14  # keys can contain spaces, property-style access needs quotation marks, then",
                "$hashtable[5]     = 8     # keys don't need to be strings",
                "# access",
                "$hashtable.key1     # value 1",
                "$hashtable['key2']  # 5");
            var expected = NewlineJoin("value 1", "5");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("NullReferenceException in Ast.cs:44")]
        [SetCulture("en-US")]
        // Taken from http://rosettacode.org/wiki/Averages/Arithmetic_mean#PowerShell
        public void AverageArithmeticMean1()
        {
            var code = NewlineJoin(
                "function mean ($x) {",
                "    if ($x.Count -eq 0) {",
                "        return 0",
                "    } else {",
                "        $sum = 0",
                "        foreach ($i in $x) {",
                "            $sum += $i",
                "        }",
                "        return $sum / $x.Count",
                "    }",
                "}",
                "mean 3,6,9,4");
            var expected = NewlineJoin("5.5");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("NullReferenceException in Ast.cs:44")]
        [SetCulture("en-US")]
        // Taken from http://rosettacode.org/wiki/Averages/Arithmetic_mean#PowerShell
        public void AverageArithmeticMean2()
        {
            var code = NewlineJoin(
                "function mean ($x) {",
                "    if ($x.Count -eq 0) {",
                "        return 0",
                "    } else {",
                "        return ($x | Measure-Object -Average).Average",
                "    }",
                "}",
                "mean 3,6,9,4");
            var expected = NewlineJoin("5.5");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Assignment to multiple variables")]
        // Taken from http://rosettacode.org/wiki/Sorting_algorithms/Bubble_sort#PowerShell
        public void BubbleSort()
        {
            var code = NewlineJoin(
                "function bubblesort ($a) {",
                "    $l = $a.Length",
                "    $hasChanged = $true",
                "    while ($hasChanged) {",
                "        $hasChanged = $false",
                "        $l--",
                "        for ($i = 0; $i -lt $l; $i++) {",
                "            if ($a[$i] -gt $a[$i+1]) {",
                "                $a[$i], $a[$i+1] = $a[$i+1], $a[$i]",
                "                $hasChanged = $true",
                "            }",
                "        }",
                "    }",
                "}",
                "$a = 6,1,7",
                "bubblesort $a",
                "$a");
            var expected = NewlineJoin("1", "6", "7");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CharacterCode()
        {
            var code = NewlineJoin(
                "# Since PowerShell allows no character literals, first we need a char",
                "$char = [char] 'a'",
                "# just cast to int then",
                "$charcode = [int] $char       # => 97",
                "",
                "# works with Unicode, too:",
                "$charcode = [int] [char] '☺'  # => 9786",
                "",
                "# in reverse, just cast to char",
                "[char] 97    # a",
                "[char] 9786  # ☺");
            var expected = NewlineJoin("a", "☺");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Format operator not implemented")]
        [SetCulture("en-US")]
        // Taken from http://rosettacode.org/wiki/Date_format#PowerShell
        public void DateFormat()
        {
            var code = NewlineJoin(
                "\"{0:yyyy-MM-dd}\" -f (Get-Date 2007-11-10)",
                "\"{0:dddd, MMMM d, yyyy}\" -f (Get-Date 2007-11-10)");
            var expected = NewlineJoin("2007-11-10", "Saturday, November 10, 2007");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Where-Object not implemented")]
        // Taken from http://rosettacode.org/wiki/Day_of_the_week#PowerShell
        public void DayOfWeek()
        {
            var code = "2008..2121 | Where-Object { (Get-Date $_-12-25).DayOfWeek -eq \"Sunday\" }";
            var expected = NewlineJoin("2011", "2016", "2022", "2033", "2039", "2044", "2050", "2061", "2067", "2072", "2078", "2089", "2095", "2101", "2107", "2112", "2118");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        // Taken from http://rosettacode.org/wiki/Dynamic_variable_names#PowerShell
        public void DynamicVariableName()
        {
            var code = NewlineJoin(
                "$variableName = 'x'",
                "New-Variable $variableName 'Foo'",
                "(Get-Variable $variableName).Value");
            var expected = NewlineJoin("Foo");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Type declaration for parameter")]
        [SetCulture("de-DE")]
        // Adapted from http://rosettacode.org/wiki/Exponentiation_operator#PowerShell
        public void ExponentiationOperator()
        {
            var code = NewlineJoin(
                "# works for both int^int and float^int",
                "function pow($a, [int]$b) {",
                "    if ($b -eq -1) { return 1/$a }",
                "    if ($b -eq 0)  { return 1 }",
                "    if ($b -eq 1)  { return $a }",
                "    if ($b -lt 0) {",
                "        $rec = $true # reciprocal needed",
                "        $b = -$b",
                "    }",
                "    $result = $a",
                "    2..$b | ForEach-Object {",
                "        $result *= $a",
                "    }",
                "    if ($rec) {",
                "        return 1/$result",
                "    } else {",
                "        return $result",
                "    }        ",
                "}",
                "pow (2.4) 1",
                "pow (2.4) 0",
                "pow 2 3",
                "pow 2 4",
                "pow 2 5");
            var expected = NewlineJoin("2,4", "1", "8", "16", "32");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("NullReferenceException in Ast.cs:44")]
        // Adapted from http://rosettacode.org/wiki/Factorial#PowerShell
        public void Factorial1()
        {
            var code = NewlineJoin(
                "function Get-Factorial ($x) {",
                "    if ($x -eq 0) {",
                "        return 1",
                "    }",
                "    return $x * (Get-Factorial ($x - 1))",
                "}",
                "Get-Factorial 13");
            var expected = NewlineJoin("6227020800");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("NullReferenceException in Ast.cs:44")]
        // Adapted from http://rosettacode.org/wiki/Factorial#PowerShell
        public void Factorial2()
        {
            var code = NewlineJoin(
                "function Get-Factorial ($x) {",
                "    if ($x -eq 0) {",
                "        return 1",
                "    } else {",
                "        $product = 1",
                "        1..$x | ForEach-Object { $product *= $_ }",
                "        return $product",
                "    }",
                "}",
                "Get-Factorial 8");
            var expected = NewlineJoin("40320");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("NullReferenceException in Ast.cs:44")]
        // Adapted from http://rosettacode.org/wiki/Factorial#PowerShell
        public void Factorial3()
        {
            var code = NewlineJoin(
                "function Get-Factorial ($x) {",
                "    if ($x -eq 0) {",
                "        return 1",
                "    }",
                "    return (Invoke-Expression (1..$x -join '*'))",
                "}",
                "Get-Factorial 10");
            var expected = NewlineJoin("3628800");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("NullReferenceException in Ast.cs:44")]
        // Adapted from http://rosettacode.org/wiki/Factors_of_an_integer#PowerShell
        public void FactorsOfAnInteger1()
        {
            var code = NewlineJoin(
                "function Get-Factor ($a) {",
                "    1..$a | Where-Object { $a % $_ -eq 0 }",
                "}",
                "Get-Factor 18");
            var expected = NewlineJoin("1", "2", "3", "6", "9", "18");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("NullReferenceException in Ast.cs:44")]
        // Adapted from http://rosettacode.org/wiki/Factors_of_an_integer#PowerShell
        public void FactorsOfAnInteger2()
        {
            var code = NewlineJoin(
                "function Get-Factor ($a) {",
                "    1..[Math]::Sqrt($a) `",
                "        | Where-Object { $a % $_ -eq 0 } `",
                "        | ForEach-Object { $_; $a / $_ } `",
                "        | Sort-Object -Unique",
                "}",
                "Get-Factor 22");
            var expected = NewlineJoin("1", "2", "11", "22");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Wrong result of 1. Perhaps parameter passing does not work correctly.")]
        public void Fibonacci1()
        {
            var code = NewlineJoin(
                "function fib ($n) {",
                "    if ($n -eq 0) { return 0 }",
                "    if ($n -eq 1) { return 1 }",
                "",
                "    $a = 0",
                "    $b = 1",
                "",
                "    for ($i = 1; $i -lt $n; $i++) {",
                "        $c = $a + $b",
                "        $a = $b",
                "        $b = $c",
                "    }",
                "    ",
                "    return $b",
                "}",
                "fib 14");
            var expected = NewlineJoin("377");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Switch not yet implemented")]
        public void Fibonacci2()
        {
            var code = NewlineJoin(
                "function fib($n) {",
                "    switch ($n) {",
                "        0            { return 0 }",
                "        1            { return 1 }",
                "        { $_ -lt 0 } { return [Math]::Pow(-1, -$n + 1) * (fib (-$n)) }",
                "        default      { return (fib ($n - 1)) + (fib ($n - 2)) }",
                "    }",
                "}",
                "fib 8");
            var expected = NewlineJoin("21");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        // Taken from http://rosettacode.org/wiki/FizzBuzz#PowerShell
        public void FizzBuzz1()
        {
            var code = NewlineJoin(
                "for ($i = 1; $i -le 100; $i++) {",
                "    if ($i % 15 -eq 0) {",
                "        \"FizzBuzz\"",
                "    } elseif ($i % 5 -eq 0) {",
                "        \"Buzz\"",
                "    } elseif ($i % 3 -eq 0) {",
                "        \"Fizz\"",
                "    } else {",
                "        $i",
                "    }",
                "}");
            var expected = NewlineJoin(
                "1", "2", "Fizz", "4", "Buzz", "Fizz", "7", "8", "Fizz", "Buzz", "11", "Fizz", "13", "14", "FizzBuzz", "16", "17", "Fizz", "19", "Buzz",
                "Fizz", "22", "23", "Fizz", "Buzz", "26", "Fizz", "28", "29", "FizzBuzz", "31", "32", "Fizz", "34", "Buzz", "Fizz", "37", "38", "Fizz", "Buzz",
                "41", "Fizz", "43", "44", "FizzBuzz", "46", "47", "Fizz", "49", "Buzz", "Fizz", "52", "53", "Fizz", "Buzz", "56", "Fizz", "58", "59", "FizzBuzz",
                "61", "62", "Fizz", "64", "Buzz", "Fizz", "67", "68", "Fizz", "Buzz", "71", "Fizz", "73", "74", "FizzBuzz", "76", "77", "Fizz", "79", "Buzz",
                "Fizz", "82", "83", "Fizz", "Buzz", "86", "Fizz", "88", "89", "FizzBuzz", "91", "92", "Fizz", "94", "Buzz", "Fizz", "97", "98", "Fizz", "Buzz");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Switch not yet implemented")]
        // Taken from http://rosettacode.org/wiki/FizzBuzz#PowerShell
        public void FizzBuzz2()
        {
            var code = NewlineJoin(
                "1..100 | ForEach-Object {",
                "    switch ($_) {",
                "        { $_ % 15 -eq 0 } { \"FizzBuzz\"; break }",
                "        { $_ % 5 -eq 0 }  { \"Buzz\" }",
                "        { $_ % 3 -eq 0 }  { \"Fizz\" }",
                "        default           { $_ }",
                "    }",
                "}");
            var expected = NewlineJoin(
                "1", "2", "Fizz", "4", "Buzz", "Fizz", "7", "8", "Fizz", "Buzz", "11", "Fizz", "13", "14", "FizzBuzz", "16", "17", "Fizz", "19", "Buzz",
                "Fizz", "22", "23", "Fizz", "Buzz", "26", "Fizz", "28", "29", "FizzBuzz", "31", "32", "Fizz", "34", "Buzz", "Fizz", "37", "38", "Fizz", "Buzz",
                "41", "Fizz", "43", "44", "FizzBuzz", "46", "47", "Fizz", "49", "Buzz", "Fizz", "52", "53", "Fizz", "Buzz", "56", "Fizz", "58", "59", "FizzBuzz",
                "61", "62", "Fizz", "64", "Buzz", "Fizz", "67", "68", "Fizz", "Buzz", "71", "Fizz", "73", "74", "FizzBuzz", "76", "77", "Fizz", "79", "Buzz",
                "Fizz", "82", "83", "Fizz", "Buzz", "86", "Fizz", "88", "89", "FizzBuzz", "91", "92", "Fizz", "94", "Buzz", "Fizz", "97", "98", "Fizz", "Buzz");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("-join and Switch not yet implemented")]
        // Adapted from http://rosettacode.org/wiki/FizzBuzz#PowerShell
        public void FizzBuzz3()
        {
            var code = NewlineJoin(
                "1..100 | ForEach-Object {",
                "    -join $( switch ($_) {",
                "        { $_ % 3 -eq 0 }  { \"Fizz\" }",
                "        { $_ % 5 -eq 0 }  { \"Buzz\" }",
                "        default           { $_ }",
                "    } )",
                "}");
            var expected = NewlineJoin(
                "1", "2", "Fizz", "4", "Buzz", "Fizz", "7", "8", "Fizz", "Buzz", "11", "Fizz", "13", "14", "FizzBuzz", "16", "17", "Fizz", "19", "Buzz",
                "Fizz", "22", "23", "Fizz", "Buzz", "26", "Fizz", "28", "29", "FizzBuzz", "31", "32", "Fizz", "34", "Buzz", "Fizz", "37", "38", "Fizz", "Buzz",
                "41", "Fizz", "43", "44", "FizzBuzz", "46", "47", "Fizz", "49", "Buzz", "Fizz", "52", "53", "Fizz", "Buzz", "56", "Fizz", "58", "59", "FizzBuzz",
                "61", "62", "Fizz", "64", "Buzz", "Fizz", "67", "68", "Fizz", "Buzz", "71", "Fizz", "73", "74", "FizzBuzz", "76", "77", "Fizz", "79", "Buzz",
                "Fizz", "82", "83", "Fizz", "Buzz", "86", "Fizz", "88", "89", "FizzBuzz", "91", "92", "Fizz", "94", "Buzz", "Fizz", "97", "98", "Fizz", "Buzz");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("joined unary operator expression not yet implemented")]
        // Taken from http://rosettacode.org/wiki/FizzBuzz#PowerShell
        public void FizzBuzz4()
        {
            var code = NewlineJoin(
                "1..100 | ForEach-Object {",
                "    $s = ''",
                "    if ($_ % 3 -eq 0) { $s += \"Fizz\" }",
                "    if ($_ % 5 -eq 0) { $s += \"Buzz\" }",
                "    if (!$s) { $s = $_ }",
                "    $s",
                "}");
            var expected = NewlineJoin(
                "1", "2", "Fizz", "4", "Buzz", "Fizz", "7", "8", "Fizz", "Buzz", "11", "Fizz", "13", "14", "FizzBuzz", "16", "17", "Fizz", "19", "Buzz",
                "Fizz", "22", "23", "Fizz", "Buzz", "26", "Fizz", "28", "29", "FizzBuzz", "31", "32", "Fizz", "34", "Buzz", "Fizz", "37", "38", "Fizz", "Buzz",
                "41", "Fizz", "43", "44", "FizzBuzz", "46", "47", "Fizz", "49", "Buzz", "Fizz", "52", "53", "Fizz", "Buzz", "56", "Fizz", "58", "59", "FizzBuzz",
                "61", "62", "Fizz", "64", "Buzz", "Fizz", "67", "68", "Fizz", "Buzz", "71", "Fizz", "73", "74", "FizzBuzz", "76", "77", "Fizz", "79", "Buzz",
                "Fizz", "82", "83", "Fizz", "Buzz", "86", "Fizz", "88", "89", "FizzBuzz", "91", "92", "Fizz", "94", "Buzz", "Fizz", "97", "98", "Fizz", "Buzz");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Format operator not yet implemented")]
        [SetCulture("en-US")]
        // Taken from http://rosettacode.org/wiki/Formatted_numeric_output#PowerShell
        public void FormattedNumericOutput()
        {
            var code = NewlineJoin("'{0:00000.000}' -f 7.125", "7.125.ToString('00000.000')");
            var expected = NewlineJoin("00007.125", "00007.125");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("NullReferenceException in Ast.cs:44")]
        // Taken from http://rosettacode.org/wiki/Function_definition#PowerShell
        public void FunctionDefinition1()
        {
            var code = NewlineJoin(
                "function multiply {",
                "    return $args[0] * $args[1]",
                "}",
                "multiply 3 4");
            var expected = NewlineJoin("12");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("NullReferenceException in Ast.cs:44")]
        // Taken from http://rosettacode.org/wiki/Function_definition#PowerShell
        public void FunctionDefinition2()
        {
            var code = NewlineJoin(
                "function multiply {",
                "    $args[0] * $args[1]",
                "}",
                "multiply 3 4");
            var expected = NewlineJoin("12");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("NullReferenceException in Ast.cs:44")]
        // Taken from http://rosettacode.org/wiki/Function_definition#PowerShell
        public void FunctionDefinition3()
        {
            var code = NewlineJoin(
                "function multiply ($a, $b) {",
                "    return $a * $b",
                "}",
                "multiply 3 4");
            var expected = NewlineJoin("12");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Tries to execute param as a command")]
        // Taken from http://rosettacode.org/wiki/Function_definition#PowerShell
        public void FunctionDefinition4()
        {
            var code = NewlineJoin(
                "function multiply {",
                "    param ($a, $b)",
                "    return $a * $b",
                "}",
                "multiply 3 4");
            var expected = NewlineJoin("12");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Parse error at typed parameter")]
        // Taken from http://rosettacode.org/wiki/Function_definition#PowerShell
        public void FunctionDefinition5()
        {
            var code = NewlineJoin(
                "function multiply ([int]$a, [int]$b) {",
                "    return $a * $b",
                "}",
                "multiply 3 4");
            var expected = NewlineJoin("12");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Parse error at assignment to multiple variables")]
        // Taken from http://rosettacode.org/wiki/Generic_swap#PowerShell
        public void GenericSwap1()
        {
            var code = NewlineJoin(
                "$a = 5",
                "$b = 10",
                "\"`$a = $a, `$b = $b\"",
                "$b, $a = $a, $b",
                "\"`$a = $a, `$b = $b\"");
            var expected = NewlineJoin(
                "$a = 5, $b = 10",
                "$a = 10, $b = 5");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Parse error at typed parameters")]
        // Taken from http://rosettacode.org/wiki/Generic_swap#PowerShell
        public void GenericSwap2()
        {
            var code = NewlineJoin(
                "function swap ([ref] $a, [ref] $b) {",
                "    $a.Value, $b.Value = $b.Value, $a.Value",
                "}",
                "$a = 5",
                "$b = 10",
                "\"`$a = $a, `$b = $b\"",
                "swap ([ref] $a) ([ref] $b)",
                "\"`$a = $a, `$b = $b\"");
            var expected = NewlineJoin(
                "$a = 5, $b = 10",
                "$a = 10, $b = 5");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Measure-Object doesn't yet exist")]
        // Taken from http://rosettacode.org/wiki/Greatest_element_of_a_list#PowerShell
        public void GreatestElementOfAList()
        {
            var code = NewlineJoin(
                "function Get-Maximum ($a) {",
                "    return ($a | Measure-Object -Maximum).Maximum",
                "}",
                "Get-Maximum (1..8)");
            var expected = NewlineJoin("8");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Parse error at typed parameters")]
        // Taken from http://rosettacode.org/wiki/Hash_from_two_arrays#PowerShell
        public void HashFromTwoArrays()
        {
            var code = NewlineJoin(
                "function create_hash ([array] $keys, [array] $values) {",
                "    $h = @{}",
                "    if ($keys.Length -ne $values.Length) {",
                "        Write-Error -Message \"Array lengths do not match\" `",
                "                    -Category InvalidData `",
                "                    -TargetObject $values",
                "    } else {",
                "        for ($i = 0; $i -lt $keys.Length; $i++) {",
                "            $h[$keys[$i]] = $values[$i]",
                "        }",
                "    }",
                "    return $h",
                "}",
                "$h = create_hash (1..5) (6..10)",
                "$h[3]");
            var expected = NewlineJoin("8");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        // Taken from http://rosettacode.org/wiki/Increment_a_numerical_string#PowerShell
        public void IncrementANumericalString1()
        {
            var code = NewlineJoin(
                "$s = \"12345\"",
                "$t = [string] ([int] $s + 1)",
                "$t");
            var expected = NewlineJoin("12346");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        // Taken from http://rosettacode.org/wiki/Increment_a_numerical_string#PowerShell
        public void IncrementANumericalString2()
        {
            var code = NewlineJoin(
                "$s = \"12345\"",
                "$t = [string] (1 + $s)",
                "$t");
            var expected = NewlineJoin("12346");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        // Taken from http://rosettacode.org/wiki/IsNumeric#PowerShell
        public void IsNumeric()
        {
            var code = NewlineJoin(
                "function isNumeric ($x) {",
                "    try {",
                "        0 + $x | Out-Null",
                "        return $true",
                "    } catch {",
                "        return $false",
                "    }",
                "}",
                "isNumeric a");
            var expected = NewlineJoin("False");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Decimal type suffix not yet implemented")]
        // Taken from http://rosettacode.org/wiki/Literals/Integer#PowerShell
        public void LiteralsInteger()
        {
            var code = NewlineJoin(
                "727     # base 10",
                "0x2d7   # base 16",
                "3KB  # 3072",
                "3MB  # 3145728",
                "3GB  # 3221225472",
                "3TB  # 3298534883328",
                "4d.GetType().ToString() # returns System.Decimal");
            var expected = NewlineJoin("727", "727", "3072", "3145728", "3221225472", "3298534883328", "System.Decimal");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        // Taken from http://rosettacode.org/wiki/Logical_operations#PowerShell
        public void LogicalOperations()
        {
            var code = NewlineJoin(
                "$a = $false; $b = $true",
                "\"A and B:   \" + ($a -and $b)",
                "\"A or B:    \" + ($a -or $b)",
                "\"not A:     \" + (-not $a)",
                "\"not A:     \" + (!$a)",
                "\"A xor B:   \" + ($a -xor $b)");
            var expected = NewlineJoin(
                "A and B:   False",
                "A or B:    True",
                "not A:     True",
                "not A:     True",
                "A xor B:   True");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Wrong answer, perhaps because parameter passing doesn't work")]
        // Taken from http://rosettacode.org/wiki/Look-and-say_sequence#PowerShell
        public void LookAndSaySequence()
        {
            var code = NewlineJoin(
                "function Get-LookAndSay ($n = 1) {",
                "    $re = [regex] '(.)\\1*'",
                "    $ret = \"\"",
                "    foreach ($m in $re.Matches($n)) {",
                "        $ret += [string] $m.Length + $m.Value[0]",
                "    }",
                "    return $ret",
                "}",
                "function Get-MultipleLookAndSay ($n) {",
                "    if ($n -eq 0) {",
                "        return @()",
                "    } else {",
                "        $a = 1",
                "        $a",
                "        for ($i = 1; $i -lt $n; $i++) {",
                "            $a = Get-LookAndSay $a",
                "            $a",
                "        }",
                "    }",
                "}",
                "Get-MultipleLookAndSay 8");
            var expected = NewlineJoin(
                "1",
                "11",
                "21",
                "1211",
                "111221",
                "312211",
                "13112221",
                "1113213211");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Infinite loop currently, as break doesn't work")]
        // Adapted from http://rosettacode.org/wiki/Loops/Break#PowerShell
        public void LoopsBreak()
        {
            var code = NewlineJoin(
                "$n = 1",
                "for () {",
                "    $n += 1",
                "    $n",
                "    if ($n -eq 10) {",
                "        break",
                "    }",
                "}");
            var expected = NewlineJoin("2", "3", "4", "5", "6", "7", "8", "9", "10");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Lots of additional newlines in the output")]
        // Adapted from http://rosettacode.org/wiki/Loops/Continue#PowerShell
        public void LoopsContinue()
        {
            var code = NewlineJoin(
                "$s = ''",
                "for ($i = 1; $i -le 10; $i++) {",
                "    $s += $i",
                "    if ($i % 5 -eq 0) {",
                "        $s",
                "        $s = ''",
                "        continue",
                "    }",
                "    $s += \", \"",
                "}");
            var expected = NewlineJoin("1, 2, 3, 4, 5", "6, 7, 8, 9, 10");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("NotImplementedException")]
        // Taken from http://rosettacode.org/wiki/Loops/Do-while#PowerShell
        public void LoopsDoWhile()
        {
            var code = NewlineJoin(
                "$n = 0",
                "do {",
                "    $n++",
                "    $n",
                "} while ($n % 6 -ne 0)");
            var expected = NewlineJoin("1", "2", "3", "4", "5", "6");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("NotImplementedException for decrement expression")]
        // Taken from http://rosettacode.org/wiki/Loops/Downward_for#PowerShell
        public void LoopsDownwardFor()
        {
            var code = NewlineJoin(
                "for ($i = 10; $i -ge 0; $i--) {",
                "    $i",
                "}");
            var expected = NewlineJoin("10", "9", "8", "7", "6", "5", "4", "3", "2", "1", "0");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        // Adapted from http://rosettacode.org/wiki/Loops/For#PowerShell
        public void LoopsFor1()
        {
            var code = NewlineJoin(
                "for ($i = 1; $i -le 5; $i++) {",
                "    for ($j = 1; $j -le $i; $j++) {",
                "        '*'",
                "    }",
                "    '--'",
                "}");
            var expected = NewlineJoin("*", "--", "*", "*", "--", "*", "*", "*", "--", "*", "*", "*", "*", "--", "*", "*", "*", "*", "*", "--");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        // Adapted from http://rosettacode.org/wiki/Loops/For#PowerShell
        public void LoopsFor2()
        {
            var code = NewlineJoin(
                "1..5 | ForEach-Object {",
                "    1..$_ | ForEach-Object {",
                "        '*'",
                "    }",
                "    '--'",
                "}");
            var expected = NewlineJoin("*", "--", "*", "*", "--", "*", "*", "*", "--", "*", "*", "*", "*", "--", "*", "*", "*", "*", "*", "--");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        // Taken from http://rosettacode.org/wiki/Loops/For_with_a_specified_step#PowerShell
        public void LoopsForWithASpecifiedStep()
        {
            var code = NewlineJoin(
                "for ($i = 0; $i -lt 10; $i += 2) {",
                "    $i",
                "}");
            var expected = NewlineJoin("0", "2", "4", "6", "8");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        // Taken from http://rosettacode.org/wiki/Loops/Foreach#PowerShell
        public void LoopsForeach()
        {
            var code = NewlineJoin(
                "foreach ($x in 1..5) {",
                "    $x",
                "}");
            var expected = NewlineJoin("1", "2", "3", "4", "5");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Wrong output, as break doesn't work yet")]
        // Adapted from http://rosettacode.org/wiki/Loops/N_plus_one_half#PowerShell
        public void LoopsNPlusOneHalf1()
        {
            var code = NewlineJoin(
                "for ($i = 1; $i -le 10; $i++) {",
                "    $i",
                "    if ($i -eq 10) {",
                "        '*'",
                "        break",
                "    }",
                "    \"-\"",
                "}");
            var expected = NewlineJoin("1", "-", "2", "-", "3", "-", "4", "-", "5", "-", "6", "-", "7", "-", "8", "-", "9", "-", "10", "*");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Switch not yet implemented")]
        // Adapted from http://rosettacode.org/wiki/Loops/N_plus_one_half#PowerShell
        public void LoopsNPlusOneHalf2()
        {
            var code = NewlineJoin(
                "switch (1..10) {",
                "    { $true }     { $_ }",
                "    { $_ -lt 10 } { \"-\" }",
                "    { $_ -eq 10 } { '*' }",
                "}");
            var expected = NewlineJoin("1", "-", "2", "-", "3", "-", "4", "-", "5", "-", "6", "-", "7", "-", "8", "-", "9", "-", "10", "*");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Parse error at assignment to typed variable")]
        // Taken from http://rosettacode.org/wiki/Loops/While#PowerShell
        public void LoopsWhile()
        {
            var code = NewlineJoin(
                "[int]$i = 1024",
                "while ($i -gt 0) {",
                "    $i",
                "    $i /= 2",
                "}");
            var expected = NewlineJoin("1024", "512", "256", "128", "64", "32", "16", "8", "4", "2", "1");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Replace not yet implemented")]
        // Taken from http://rosettacode.org/wiki/MD5#PowerShell
        public void MD5()
        {
            var code = NewlineJoin(
                "$string = \"The quick brown fox jumped over the lazy dog's back\"",
                "$data = [Text.Encoding]::UTF8.GetBytes($string)",
                "$hash = [Security.Cryptography.MD5]::Create().ComputeHash($data)",
                "([BitConverter]::ToString($hash) -replace '-').ToLower()");
            var expected = NewlineJoin("e38ca1d920c4b8b8d3946b2c72f01680");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("NullReferenceException in Ast.cs:44")]
        // Taken from http://rosettacode.org/wiki/Monte_Carlo_Simulation#PowerShell
        public void MonteCarlo()
        {
            var code = NewlineJoin(
                "function Get-Pi ($Iterations = 10000) {",
                "    $InCircle = 0",
                "    for ($i = 0; $i -lt $Iterations; $i++) {",
                "        $x = Get-Random 1.0",
                "        $y = Get-Random 1.0",
                "        if ([Math]::Sqrt($x * $x + $y * $y) -le 1) {",
                "            $InCircle++",
                "        }",
                "    }",
                "    $Pi = [decimal] $InCircle / $Iterations * 4",
                "    $RealPi = [decimal] \"3.141592653589793238462643383280\"",
                "    $Diff = [Math]::Abs(($Pi - $RealPi) / $RealPi * 100)",
                "    New-Object PSObject `",
                "        | Add-Member -PassThru NoteProperty Iterations $Iterations `",
                "        | Add-Member -PassThru NoteProperty Pi $Pi `",
                "        | Add-Member -PassThru NoteProperty \"% Difference\" $Diff",
                "}",
                "Get-Pi 10");
            var result = ReferenceHost.RawExecute(code)[0];
            var properties = result.Properties;
            Assert.IsNotNull(properties.SingleOrDefault(p => p.Name == "Iterations"));
            Assert.IsNotNull(properties.SingleOrDefault(p => p.Name == "Pi"));
            Assert.IsNotNull(properties.SingleOrDefault(p => p.Name == "% Difference"));
            Assert.AreEqual(10, properties["Iterations"]);
        }

        [Test, Explicit("NullReferenceException in Ast.cs:44")]
        // Taken from http://rosettacode.org/wiki/Mutual_recursion#PowerShell
        public void MutualRecursion()
        {
            var code = NewlineJoin(
                "function F($n) {",
                "    if ($n -eq 0) { return 1 }",
                "    return $n - (M (F ($n - 1)))",
                "}",
                "function M($n) {",
                "    if ($n -eq 0) { return 0 }",
                "    return $n - (F (M ($n - 1)))",
                "}",
                "F 5");
            var expected = NewlineJoin("3");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Parameter passing doesn't work yet")]
        // Taken from http://rosettacode.org/wiki/Named_parameters#PowerShell
        public void NamedParameters()
        {
            var code = NewlineJoin(
                "function Test ($SomeArgument, $AnotherArgument, $ThirdArgument) {",
                "    \"Some argument:    $SomeArgument\"",
                "    \"Another argument: $AnotherArgument\"",
                "    \"Third argument:   $ThirdArgument\"",
                "}",
                "Test foo bar baz",
                "Test -ThirdArgument foo -AnotherArgument bar -SomeArgument baz",
                "Test -ThirdArgument foo -AnotherArgument bar");
            var expected = NewlineJoin(
                "Some argument:    foo",
                "Another argument: bar",
                "Third argument:   baz",
                "Some argument:    baz",
                "Another argument: bar",
                "Third argument:   foo",
                "Some argument:    ",
                "Another argument: bar",
                "Third argument:   foo");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Format operator not implemented yet")]
        // Taken from http://rosettacode.org/wiki/Non-decimal_radices/Output#PowerShell
        public void NonDecimalRadicesOutput()
        {
            var code = NewlineJoin(
                "foreach ($n in 0..5    ) {",
                "    \"Base 2:  \" + [Convert]::ToString($n, 2)",
                "    \"Base 8:  \" + [Convert]::ToString($n, 8)",
                "    \"Base 10: \" + $n",
                "    \"Base 10: \" + [Convert]::ToString($n, 10)",
                "    \"Base 10: \" + (\"{0:D}\" -f $n)",
                "    \"Base 16: \" + [Convert]::ToString($n, 16)",
                "    \"Base 16: \" + (\"{0:X}\" -f $n)",
                "}");
            var expected = NewlineJoin(
                "Base 2:  0", "Base 8:  0", "Base 10: 0", "Base 10: 0", "Base 10: 0", "Base 16: 0", "Base 16: 0",
                "Base 2:  1", "Base 8:  1", "Base 10: 1", "Base 10: 1", "Base 10: 1", "Base 16: 1", "Base 16: 1",
                "Base 2:  10", "Base 8:  2", "Base 10: 2", "Base 10: 2", "Base 10: 2", "Base 16: 2", "Base 16: 2",
                "Base 2:  11", "Base 8:  3", "Base 10: 3", "Base 10: 3", "Base 10: 3", "Base 16: 3", "Base 16: 3",
                "Base 2:  100", "Base 8:  4", "Base 10: 4", "Base 10: 4", "Base 10: 4", "Base 16: 4", "Base 16: 4",
                "Base 2:  101", "Base 8:  5", "Base 10: 5", "Base 10: 5", "Base 10: 5", "Base 16: 5", "Base 16: 5");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("NullReferenceException in Ast.cs:44")]
        // Taken from http://rosettacode.org/wiki/Primality_by_trial_division#PowerShell
        public void PrimalityByTrialDivision()
        {
            var code = NewlineJoin(
                "function isPrime ($n) {",
                "    if ($n -eq 1) {",
                "        return $false",
                "    } else {",
                "        return (@(1..[Math]::Sqrt($n) | Where-Object { $n % $_ -eq 0 }).Length -eq 1)",
                "    }",
                "}",
                "isPrime 2",
                "isPrime 18");
            var expected = NewlineJoin("True", "False");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Format operator not implemented yet")]
        // Adapted from http://rosettacode.org/wiki/Quine#PowerShell
        public void Quine()
        {
            var code = NewlineJoin(
                "$d='$d={0}{1}{0}{2}$d -f [char]39,$d,[Environment]::NewLine'",
                "$d -f [char]39,$d,[Environment]::NewLine");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(code, actual);
        }

        [Test, Explicit("Replace operator not yet implemented")]
        // Taken from http://rosettacode.org/wiki/Regular_expression_matching#PowerShell
        public void RegularExpressionMatching()
        {
            var code = NewlineJoin(
               "\"I am a string\" -match '\\bstr'       # true",
                "\"I am a string\" -replace 'a\\b','no'  # I am no string");
            var expected = NewlineJoin("True", "I am no string");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        // Taken from http://rosettacode.org/wiki/Repeating_a_string#PowerShell
        public void RepeatingAString()
        {
            var code = "\"ha\" * 5  # ==> \"hahahahaha\"";
            var expected = NewlineJoin("hahahahaha");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Parse error at assignment to multiple variables")]
        // Taken from http://rosettacode.org/wiki/Retrieve_a_substring#PowerShell
        public void RetrieveASubstring()
        {
            var code = NewlineJoin(
                "# test string",
                "$s = \"abcdefgh\"",
                "# test parameters",
                "$n, $m, $c, $s2 = 2, 3, [char]'d', 'cd'",
                "# starting from n characters in and of m length",
                "# n = 2, m = 3",
                "$s.Substring($n-1, $m)              # returns 'bcd'",
                "# starting from n characters in, up to the end of the string",
                "# n = 2",
                "$s.Substring($n-1)                  # returns 'bcdefgh'",
                "# whole string minus last character",
                "$s.Substring(0, $s.Length - 1)      # returns 'abcdefg'",
                "# starting from a known character within the string and of m length",
                "# c = 'd', m =3",
                "$s.Substring($s.IndexOf($c), $m)    # returns 'def'",
                "# starting from a known substring within the string and of m length",
                "# s2 = 'cd', m = 3",
                "$s.Substring($s.IndexOf($s2), $m)   # returns 'cde'");
            var expected = NewlineJoin("bcd", "bcdefgh", "abcdefg", "def", "cde");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        // Taken from http://rosettacode.org/wiki/Reversing_a_string#PowerShell
        public void ReversingAString1()
        {
            var code = NewlineJoin(
                "$s = \"asdf\"",
                "[string]::Join('', $s[$s.Length..0])");
            var expected = NewlineJoin("fdsa");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("joined unary operator expression not yet implemented")]
        // Taken from http://rosettacode.org/wiki/Reversing_a_string#PowerShell
        public void ReversingAString2()
        {
            var code = NewlineJoin(
                "$s = \"asdf\"",
                "-join ($s[$s.Length..0])");
            var expected = NewlineJoin("fdsa");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Replace operator not yet implemented")]
        // Taken from http://rosettacode.org/wiki/Reversing_a_string#PowerShell
        public void ReversingAString3()
        {
            var code = NewlineJoin(
                "$s = \"asdf\"",
                "$s -replace",
                "      ('(.)' * $s.Length),",
                "      [string]::Join('', ($s.Length..1 | ForEach-Object { \"`$$_\" }))");
            var expected = NewlineJoin("fdsa");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("joined unary operator expression not yet implemented")]
        // Taken from http://rosettacode.org/wiki/Reversing_a_string#PowerShell
        public void ReversingAString4()
        {
            var code = NewlineJoin(
                "$s = \"asdf\"",
                "$s -replace",
                "      ('(.)' * $s.Length),",
                "      -join ($s.Length..1 | ForEach-Object { \"`$$_\" } )");
            var expected = NewlineJoin("fdsa");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Cannot cast string to regex")]
        // Taken from http://rosettacode.org/wiki/Run-length_encoding#PowerShell
        public void RunLengthEncoding()
        {
            var code = NewlineJoin(
                "function Compress-RLE ($s) {",
                "    $re = [regex] '(.)\\1*'",
                "    $ret = \"\"",
                "    foreach ($m in $re.Matches($s)) {",
                "        $ret += $m.Length",
                "        $ret += $m.Value[0]",
                "    }",
                "    return $ret",
                "}",
                "function Expand-RLE ($s) {",
                "    $re = [regex] '(\\d+)(.)'",
                "    $ret = \"\"",
                "    foreach ($m in $re.Matches($s)) {",
                "        $ret += [string] $m.Groups[2] * [int] [string] $m.Groups[1]",
                "    }",
                "    return $ret",
                "}",
                "Compress-RLE \"WWWWWWWWWWWWBWWWWWWWWWWWWBBBWWWWWWWWWWWWWWWWWWWWWWWWBWWWWWWWWWWWWWW\"",
                "Expand-RLE \"12W1B12W3B24W1B14W\"");
            var expected = NewlineJoin(
                "12W1B12W3B24W1B14W",
                "WWWWWWWWWWWWBWWWWWWWWWWWWBBBWWWWWWWWWWWWWWWWWWWWWWWWBWWWWWWWWWWWWWW");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Scope modifiers don't work in strings yet")]
        // Taken from http://rosettacode.org/wiki/Scope_modifiers#PowerShell
        public void ScopeModifiers1()
        {
            var code = NewlineJoin(
                "$a = \"foo\"                        # global scope",
                "function test {",
                "    $a = \"bar\"                    # local scope",
                "    \"Local: $a\"          # \"bar\"",
                "    \"Global: $global:a\"  # \"foo\"",
                "}",
                "test");
            var expected = NewlineJoin("Local: bar", "Global: foo");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("NullReferenceException in Ast.cs:44")]
        // Taken from http://rosettacode.org/wiki/Scope_modifiers#PowerShell
        public void ScopeModifiers2()
        {
            var code = NewlineJoin(
                "$a = \"foo\"                        # global scope",
                "function test {",
                "    $a = \"bar\"                    # local scope",
                "    \"Local: \" + $a          # \"bar\"",
                "    \"Global: \" + $global:a  # \"foo\"",
                "}",
                "test");
            var expected = NewlineJoin("Local: bar", "Global: foo");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Where-Object not yet implemented")]
        // Taken from http://rosettacode.org/wiki/Select_from_Array#PowerShell
        public void SelectFromArray()
        {
            var code = NewlineJoin(
                "$array = -15..37",
                "$array | Where-Object { $_ % 2 -eq 0 }");
            var expected = NewlineJoin(
                "-14", "-12", "-10", "-8", "-6", "-4", "-2", "0", "2", "4", "6", "8", "10",
                "12", "14", "16", "18", "20", "22", "24", "26", "28", "30", "32", "34", "36");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("NullReferenceException in Ast.cs:44")]
        // Taken from http://rosettacode.org/wiki/Sequence_of_non-squares#PowerShell
        public void SequenceOfNonSquares()
        {
            var code = NewlineJoin(
                "# a filter can be used directly on the pipeline",
                "filter Get-NonSquare {",
                "    return $_ + [Math]::Floor(1/2 + [Math]::Sqrt($_))",
                "}",
                "# print out the values for n in the range 1 to 22",
                "1..22 | Get-NonSquare");
            var expected = NewlineJoin(
                "2", "3", "5", "6", "7", "8", "10", "11", "12", "13", "14", "15",
                "17", "18", "19", "20", "21", "22", "23", "24", "26", "27");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("NullReferenceException in Ast.cs:44")]
        // Adapted from http://rosettacode.org/wiki/Sierpinski_carpet#PowerShell
        public void SierpińskiCarpet()
        {
            var code = NewlineJoin(
                "function inCarpet($x, $y) {",
                "    while ($x -ne 0 -and $y -ne 0) {",
                "        if ($x % 3 -eq 1 -and $y % 3 -eq 1) {",
                "            return \" \"",
                "        }",
                "        $x = [Math]::Truncate($x / 3)",
                "        $y = [Math]::Truncate($y / 3)",
                "    }",
                "    return \"█\"",
                "}",
                "function carpet($n) {",
                "    for ($y = 0; $y -lt [Math]::Pow(3, $n); $y++) {",
                "        $s = ''",
                "        for ($x = 0; $x -lt [Math]::Pow(3, $n); $x++) {",
                "            $s += inCarpet $x $y",
                "        }",
                "        $s",
                "    }",
                "}",
                "carpet 3");
            var expected = NewlineJoin(
                "███████████████████████████",
                "█ ██ ██ ██ ██ ██ ██ ██ ██ █",
                "███████████████████████████",
                "███   ██████   ██████   ███",
                "█ █   █ ██ █   █ ██ █   █ █",
                "███   ██████   ██████   ███",
                "███████████████████████████",
                "█ ██ ██ ██ ██ ██ ██ ██ ██ █",
                "███████████████████████████",
                "█████████         █████████",
                "█ ██ ██ █         █ ██ ██ █",
                "█████████         █████████",
                "███   ███         ███   ███",
                "█ █   █ █         █ █   █ █",
                "███   ███         ███   ███",
                "█████████         █████████",
                "█ ██ ██ █         █ ██ ██ █",
                "█████████         █████████",
                "███████████████████████████",
                "█ ██ ██ ██ ██ ██ ██ ██ ██ █",
                "███████████████████████████",
                "███   ██████   ██████   ███",
                "█ █   █ ██ █   █ ██ █   █ █",
                "███   ██████   ██████   ███",
                "███████████████████████████",
                "█ ██ ██ ██ ██ ██ ██ ██ ██ █",
                "███████████████████████████");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("NullReferenceException in Ast.cs:44")]
        // Taken from http://rosettacode.org/wiki/Sierpinski_triangle#PowerShell
        public void SierpińskiTriangle()
        {
            var code = NewlineJoin(
                "function triangle($o) {",
                "    $n = [Math]::Pow(2, $o)",
                "    $line = ,' '*(2*$n+1)",
                "    $line[$n] = '█'",
                "    $OFS = ''",
                "    for ($i = 0; $i -lt $n; $i++) {",
                "        \"$line\"",
                "        $u = '█'",
                "        for ($j = $n - $i; $j -lt $n + $i + 1; $j++) {",
                "            if ($line[$j-1] -eq $line[$j+1]) {",
                "                $t = ' '",
                "            } else {",
                "                $t = '█'",
                "            }",
                "            $line[$j-1] = $u",
                "            $u = $t",
                "        }",
                "        $line[$n+$i] = $t",
                "        $line[$n+$i+1] = '█'",
                "    }",
                "}",
                "triangle 3");
            var expected = NewlineJoin(
                "        █        ",
                "       █ █       ",
                "      █   █      ",
                "     █ █ █ █     ",
                "    █       █    ",
                "   █ █     █ █   ",
                "  █   █   █   █  ",
                " █ █ █ █ █ █ █ █ ");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        // Taken from http://rosettacode.org/wiki/Sorting_an_Array_of_Integers#PowerShell
        public void SortingAnArrayOfIntegers()
        {
            var code = "34,12,23,56,1,129,4,2,73 | Sort-Object";
            var expected = NewlineJoin("1", "2", "4", "12", "23", "34", "56", "73", "129");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("NullReferenceException in BuildUnaryDashExpressionAst")]
        // Taken from http://rosettacode.org/wiki/Sorting_Using_a_Custom_Comparator#PowerShell
        public void SortingUsingACustomComparator()
        {
            var code = NewlineJoin(
                "$list = \"Here\", \"are\", \"some\", \"sample\", \"strings\", \"to\", \"be\", \"sorted\"",
                "$list | Sort-Object {-$_.Length},{$_}");
            var expected = NewlineJoin("strings", "sample", "sorted", "Here", "some", "are", "be", "to");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        // Adapted from http://rosettacode.org/wiki/String_case#PowerShell
        public void StringCase()
        {
            var code = NewlineJoin(
                "$string = 'alphaBETA'",
                "$lower  = $string.ToLower()",
                "$upper  = $string.ToUpper()",
                "$lower",
                "$upper");
            var expected = NewlineJoin(
                "alphabeta",
                "ALPHABETA");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        // Adapted from http://rosettacode.org/wiki/String_concatenation#PowerShell
        public void StringConcat1()
        {
            var code = NewlineJoin(
                "$s = \"Hello\"",
                "\"$s World.\"");
            var expected = NewlineJoin("Hello World.");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        // Adapted from http://rosettacode.org/wiki/String_concatenation#PowerShell
        public void StringConcat2()
        {
            var code = NewlineJoin(
                "$s = \"Hello\"",
                "$s2 = $s + \" World.\"",
                "$s2");
            var expected = NewlineJoin("Hello World.");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        // Taken from http://rosettacode.org/wiki/String_length#PowerShell
        public void StringLength()
        {
            var code = NewlineJoin(
                "$s = \"Hëlló Wørłð\"",
                "# character length",
                "$s.Length",
                "# byte length",
                "[System.Text.Encoding]::Unicode.GetByteCount($s)",
                "# same for UTF-8",
                "[System.Text.Encoding]::UTF8.GetByteCount($s)");
            var expected = NewlineJoin("11", "22", "16");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Measure-Object not implemented")]
        [SetCulture("en-US")]
        // Adapted from http://rosettacode.org/wiki/Sum_a_series#PowerShell
        public void SumASeries()
        {
            var code = NewlineJoin(
                "$x = 1..1000 `",
                "       | ForEach-Object { 1 / ($_ * $_) } `",
                "       | Measure-Object -Sum",
                "$x.Sum.ToString(\"N3\")");
            var expected = NewlineJoin("1.644");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Measure-Object not implemented")]
        // Taken from http://rosettacode.org/wiki/Sum_and_product_of_an_array#PowerShell
        public void SumAndProductOfArray1()
        {
            var code = NewlineJoin(
                "# using measure-object",
                "function Get-Sum ($a) {",
                "    return ($a | Measure-Object -Sum).Sum",
                "}",
                "Get-Sum 5,9,7,2,3,8,4");
            var expected = NewlineJoin("38");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Wrong result")]
        // Taken from http://rosettacode.org/wiki/Sum_and_product_of_an_array#PowerShell
        public void SumAndProductOfArray2()
        {
            var code = NewlineJoin(
                "# straightforward",
                "function Get-Product ($a) {",
                "    if ($a.Length -eq 0) {",
                "        return 0",
                "    } else {",
                "        $p = 1",
                "        foreach ($x in $a) {",
                "            $p *= $x",
                "        }",
                "        return $p",
                "    }",
                "}",
                "Get-Product 5,9,7,2,3,8,4");
            var expected = NewlineJoin("60480");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Join operator not implemented")]
        // Taken from http://rosettacode.org/wiki/Sum_and_product_of_an_array#PowerShell
        public void SumAndProductOfArray3()
        {
            var code = NewlineJoin(
                "# using Invoke-Expression",
                "function Get-Product ($a) {",
                "    if ($a.Length -eq 0) {",
                "        return 0",
                "    }",
                "    $s = $a -join '*'",
                "    return (Invoke-Expression $s)",
                "}",
                "Get-Product 5,9,7,2,3,8,4");
            var expected = NewlineJoin("60480");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("ParameterBindingException : Positional parameter not found for provided argument 'NoteProperty'")]
        // Taken from http://rosettacode.org/wiki/Sum_and_product_of_an_array#PowerShell
        public void SumAndProductOfArray4()
        {
            var code = NewlineJoin(
                "# returning both at once",
                "function Get-SumAndProduct ($a) {",
                "    $sum = 0",
                "    if ($a.Length -eq 0) {",
                "        $prod = 0",
                "    } else {",
                "        $prod = 1",
                "        foreach ($x in $a) {",
                "            $sum += $x",
                "            $prod *= $x",
                "        }",
                "    }",
                "    $ret = New-Object PSObject",
                "    $ret | Add-Member NoteProperty Sum $sum",
                "    $ret | Add-Member NoteProperty Product $prod",
                "    return $ret",
                "}",
                "Get-SumAndProduct 5,9,7,2,3,8,4");
            var result = ReferenceHost.RawExecute(code)[0];
            Assert.AreEqual(38, result.Properties["Sum"].Value);
            Assert.AreEqual(60480, result.Properties["Product"].Value);
        }

        [Test, Explicit("Measure-Object not implemented")]
        // Taken from http://rosettacode.org/wiki/Sum_of_squares#PowerShell
        public void SumOfSquares()
        {
            var code = NewlineJoin(
                "function Get-SquareSum ($a) {",
                "    if ($a.Length -eq 0) {",
                "        return 0",
                "    } else {",
                "        $x = $a `",
                "             | ForEach-Object { $_ * $_ } `",
                "             | Measure-Object -Sum",
                "        return $x.Sum",
                "    }",
                "}",
                "Get-SquareSum (1..5)");
            var expected = NewlineJoin("55");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        // Taken from http://rosettacode.org/wiki/Tokenize_a_string#PowerShell
        public void TokenizingAString1()
        {
            var code = NewlineJoin(
                "# PowerShell 1",
                "$words = \"Hello,How,Are,You,Today\".Split(',')",
                "[string]::Join('.', $words)");
            var expected = NewlineJoin("Hello.How.Are.You.Today");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Split operator not implemented")]
        // Taken from http://rosettacode.org/wiki/Tokenize_a_string#PowerShell
        public void TokenizingAString2()
        {
            var code = NewlineJoin(
                "# PowerShell 2",
                "$words = \"Hello,How,Are,You,Today\" -split ','",
                "$words -join '.'");
            var expected = NewlineJoin("Hello.How.Are.You.Today");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Format operator not implemented")]
        [SetCulture("de-DE")]
        // Taken from http://rosettacode.org/wiki/Trigonometric_functions#PowerShell
        public void TrigonometricFunctions()
        {
            var code = NewlineJoin(
                "$rad = [Math]::PI / 4",
                "$deg = 45",
                "'{0,10} {1,10}' -f 'Radians','Degrees'",
                "'{0,10:N6} {1,10:N6}' -f [Math]::Sin($rad), [Math]::Sin($deg * [Math]::PI / 180)",
                "'{0,10:N6} {1,10:N6}' -f [Math]::Cos($rad), [Math]::Cos($deg * [Math]::PI / 180)",
                "'{0,10:N6} {1,10:N6}' -f [Math]::Tan($rad), [Math]::Tan($deg * [Math]::PI / 180)",
                "$temp = [Math]::Asin([Math]::Sin($rad))",
                "'{0,10:N6} {1,10:N6}' -f $temp, ($temp * 180 / [Math]::PI)",
                "$temp = [Math]::Acos([Math]::Cos($rad))",
                "'{0,10:N6} {1,10:N6}' -f $temp, ($temp * 180 / [Math]::PI)",
                "$temp = [Math]::Atan([Math]::Tan($rad))",
                "'{0,10:N6} {1,10:N6}' -f $temp, ($temp * 180 / [Math]::PI)");
            var expected = NewlineJoin(
                "   Radians    Degrees",
                "  0,707107   0,707107",
                "  0,707107   0,707107",
                "  1,000000   1,000000",
                "  0,785398  45,000000",
                "  0,785398  45,000000",
                "  0,785398  45,000000");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        // Taken from http://rosettacode.org/wiki/True/False_Values#PowerShell
        public void TrueFalseValues()
        {
            var code = NewlineJoin("$true", "$false");
            var expected = NewlineJoin("True", "False");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Splat operator not implemented")]
        // Taken from http://rosettacode.org/wiki/Variadic_function#PowerShell
        public void VariadicFunction()
        {
            var code = NewlineJoin(
                "# varargs are automatic, the $args array always contains all arguments",
                "function print_all {",
                "    foreach ($x in $args) {",
                "        $x",
                "    }",
                "}",
                "print_all 1 2 'foo'",
                "# v1 way of invoking the function with an array as argument",
                "$array = 1,2,'foo'",
                "Invoke-Expression \"& print_all $array\"",
                "# with splat operator (v2)",
                "print_all @array");
            var expected = NewlineJoin("1", "2", "foo", "1", "2", "foo", "1", "2", "foo");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("NullReferenceException in Ast.cs:44")]
        // Taken from http://rosettacode.org/wiki/Variables#PowerShell
        public void Variables()
        {
            var code = NewlineJoin(
                "# uninitialized variables have no value but don't cause errors",
                "4 + $foo              # yields 4",
                "\"abc\" + $foo + \"def\"  # yields \"abcdef\"");
            var expected = NewlineJoin("4", "abcdef");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }
    }
}