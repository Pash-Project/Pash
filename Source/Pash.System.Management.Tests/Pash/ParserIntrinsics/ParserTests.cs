// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Extensions.String;

namespace Pash.ParserIntrinsics.Tests
{
    [TestFixture]
    public class ParserTests
    {
        [Test]
        public void GrammarErrorsCount()
        {
            // Obviously, we'd rather drive this to 0, but for now, let's lock it down
            Assert.AreEqual(2, PowerShellGrammar.Parser.Language.Errors.Count, PowerShellGrammar.Parser.Language.Errors.JoinString("\r\n"));
        }

        [Test]
        [TestCase(@"if ($true) {}")]
        [TestCase(@"if ($true) { }")] // having whitespace here was causing a parse error.
        [TestCase(@"if ($host.Name -eq ""Console"") {  }")]
        [TestCase(@"if ($host.Name -ne ""Console"") {  }")]
        [TestCase(@"if ($true) {} else {}")]
        [TestCase(@"if ($true) {} elseif ($true) {} ")]
        [TestCase(@"if ($true) {} elseif {$true) {} else {}", Explicit = true)]
        [TestCase(@"if ($true) {} elseif ($true) {} elseif ($true) else {}", Explicit = true)]
        public void IfElseSyntax(string input)
        {
            AssertIsValidInput(input);
        }

        [Test]
        [TestCase(@"[int]")]
        [TestCase(@"[int][string]7")] // double cast is OK
        [TestCase(@"[int]-3"), Description("Cast, not subtraction")]
        [TestCase(@"[int],[string]")]
        [TestCase(@"$x = [int]")]
        [TestCase(@"$x::MaxValue")]
        [TestCase(@"[int]::Parse('7')")]
        [TestCase(@"$x::Parse()")]
        [TestCase(@"$x.Assembly")]
        [TestCase(@"$x.AsType()")]
        [TestCase(@"[char]::IsUpper(""AbC"", 1)", Description = "two parameters")]
        public void TypesAndMembers(string input)
        {
            AssertIsValidInput(input);
        }

        [Test]
        [TestCase(@"$i = 100			# $i designates an int value 100@")]
        [TestCase(@"$j = $i			# $j designates an int value 100, which is a copy@")]
        [TestCase(@"$a = 10,20,30	# $a designates an object[], Length 3, value 10,20,30@")]
        [TestCase(@"$b = $a			# $b designates exactly the same array as does $a, not a copy@")]
        [TestCase(@"$a[1] = 50		# element 1 (which has a value type) is changed from 20 to 50 @")]
        [TestCase(@"$b[1]				# $b refers to the same array as $a, so $b[1] is 50@")]
        public void Section4_Types(string input)
        {
            AssertIsValidInput(input);
        }

        [Test]
        [TestCase(@"$a = 10,20,30")]
        [TestCase(@"$a.Length							# get instance property")]
        [TestCase(@"(10,20,30).Length")]
        [TestCase(@"$property = ""Length""")]
        [TestCase(@"$a.$property						# property name is a variable")]
        [TestCase(@"$h1 = @{ FirstName = ""James""; LastName = ""Anderson""; IDNum = 123 }")]
        [TestCase(@"$h1.FirstName						# designates the key FirstName")]
        [TestCase(@"$h1.Keys								# gets the collection of keys")]
        [TestCase(@"[int]::MinValue					# get static property")]
        [TestCase(@"[double]::PositiveInfinity		# get static property")]
        [TestCase(@"$property = ""MinValue""")]
        [TestCase(@"[long]::$property					# property name is a variable")]
        [TestCase(@"
            foreach ($t in [byte],[int],[long])
            {
 	            $t::MaxValue					# get static property
            }
            ", Explicit = true)]
        [TestCase(@"$a = @{ID=1},@{ID=2},@{ID=3}")]
        [TestCase(@"$a.ID									# get ID from each element in the array ")]
        public void Section7_1_2_MemberAccess(string input)
        {
            AssertIsValidInput(input);
        }

        [Test]
        [TestCase(@"[math]::Sqrt(2.0)				# call method with argument 2.0")]
        [TestCase(@"[char]::IsUpper(""a"")			# call method")]
        [TestCase(@"$b = ""abc#$%XYZabc"" 
                    $b.ToUpper()					# call instance method", Explicit = true)]
        [TestCase(@"[math]::Sqrt(2) 				# convert 2 to 2.0 and call method")]
        [TestCase(@"[math]::Sqrt(2D) 				# convert 2D to 2.0 and call method")]
        [TestCase(@"[math]::Sqrt($true) 			# convert $true to 1.0 and call method")]
        [TestCase(@"[math]::Sqrt(""20"") 			# convert ""20"" to 20 and call method", Explicit = true)]
        [TestCase(@"$a = [math]::Sqrt				# get method descriptor for Sqrt")]
        [TestCase(@"$a.Invoke(2.0)					# call Sqrt via the descriptor")]
        [TestCase(@"$a = [math]::(""Sq""+""rt"")	# get method descriptor for Sqrt", Explicit = true)]
        [TestCase(@"$a.Invoke(2.0) 				# call Sqrt via the descriptor")]
        [TestCase(@"$a = [char]::ToLower			# get method descriptor for ToLower")]
        [TestCase(@"$a.Invoke(""X"")					# call ToLower via the descriptor", Explicit = true)]
        public void Section7_1_3_InvocationExpressions(string input)
        {
            AssertIsValidInput(input);
        }

        [TestCase(@"$a = [int[]](10,20,30)		# [int[]], Length 3", Explicit = true)]
        [TestCase(@"$a[1]								# returns int 20")]
        [TestCase(@"$a[20]							# no such position, returns $null")]
        [TestCase(@"$a[-1]							# returns int 30, i.e., $a[$a.Length-1]")]
        [TestCase(@"$a[2] = 5						# changes int 30 to int 5")]
        [TestCase(@"$a[20] = 5						# implementation-defined behavior")]
        [TestCase(@"$a = New-Object 'double[,]' 3,2", Explicit = true)]
        [TestCase(@"$a[0,0] = 10.5					# changes 0.0 to 10.5")]
        [TestCase(@"$a[0,0]++						# changes 10.5 to 10.6")]
        [TestCase(@"$list = (""red"",$true,10),20,(1.2, ""yes""", Explicit = true)]
        [TestCase(@"$list[2][1]						# returns string ""yes""")]
        [TestCase(@"$a = @{ A = 10 },@{ B = $true },@{ C = 123.45 }")]
        [TestCase(@"$a[1][""B""]						# $a[1] is a Hashtable, where B is a key", Explicit = true)]
        [TestCase(@"$a = ""red"",""green""")]
        [TestCase(@"$a[1][4]							# returns string ""n"" from string in $a[1]", Explicit = true)]
        public void Section7_1_4_1_SubscriptingAnArray(string input)
        {
            AssertIsValidInput(input);
        }

        [Test]
        [TestCase(@"$s = ""Hello""")]
        [TestCase(@"$s = ""Hello""					# string, Length 5, positions 0-4", Explicit = true)]
        [TestCase(@"$c = $s[1]						# returns ""e"" as a string")]
        [TestCase(@"$c = $s[20]						# no such position, returns $null")]
        [TestCase(@"$c = $s[-1]						# returns ""o"", i.e., $s[$s.Length-1]")]
        public void Section7_1_4_2_SubscriptingAString(string input)
        {
            AssertIsValidInput(input);
        }


        [Test]
        [TestCase(@"[int[]](30,40,50,60,70,80,90)", Explicit = true)]

        [TestCase(@"$a = [int[]](30,40,50,60,70,80,90)", Explicit = true)]
        [TestCase(@"$a[1,3,5]					# slice has Length 3, value 40,60,80")]
        [TestCase(@"++$a[1,3,5][1]				# preincrement 60 in array 40,60,80")]
        [TestCase(@"$a[,5]						# slice with Length 1")]
        [TestCase(@"$a[@()]						# slice with Length 0")]
        [TestCase(@"$a[-1..-3] 					# slice with Length 0, value 90,80,70")]
        [TestCase(@"$a = New-Object 'int[,]' 3,2", Explicit = true)]
        [TestCase(@"$a[0,0] = 10; $a[0,1] = 20; $a[1,0] = 30")]
        [TestCase(@"$a[1,1] = 40; $a[2,0] = 50; $a[2,1] = 60")]
        [TestCase(@"$a[(0,1),(1,0)]			# slice with Length 2, value 20,30, parens needed")]
        [TestCase(@"$h1 = @{ FirstName = ""James""; LastName = ""Anderson""; IDNum = 123 }")]
        [TestCase(@"$h1['FirstName']				# the value associated with key FirstName")]
        [TestCase(@"$h1['BirthDate']				# no such key, returns $null")]
        [TestCase(@"$h1['FirstName','IDNum']	# returns [object[]], Length 2 (James/123)")]
        [TestCase(@"$h1['FirstName','xxx']		# returns [object[]], Length 2 (James/$null)")]
        [TestCase(@"$h1[$null,'IDNum']			# returns [object[]], Length 1 (123)")]
        public void Section7_1_4_5GeneratingArraySlices(string input)
        {
            AssertIsValidInput(input);
        }

        [Test]
        [TestCase(@"$i = 0						# $i = 0")]
        [TestCase(@"$i++							# $i is incremented by 1")]
        [TestCase(@"$j = $i--					# $j takes on the value of $i before the decrement")]
        [TestCase(@"$a = 1,2,3")]
        [TestCase(@"$b = 9,8,7")]
        [TestCase(@"$i = 0")]
        [TestCase(@"$j = 1")]
        [TestCase(@"$b[$j--] = $a[$i++]		# $b[1] takes on the value of $a[0], then $j is")]
        [TestCase(@" 								# decremented, $i incremented")]
        [TestCase(@"$i = 2147483647			# $i holds a value of type int")]
        [TestCase(@"$i++							# $i now holds a value of type double because")]
        [TestCase(@"								# 2147483648 is too big to fit in type int")]
        [TestCase(@"[int]$k = 0					# $k is constrained to int", Explicit = true)]
        [TestCase(@"$k = [int]::MaxValue		# $k is set to 2147483647")]
        [TestCase(@"$k++							# 2147483648 is too big to fit, imp-def bahavior")]
        [TestCase(@"$x = $null					# target is unconstrained, $null goes to [int]0")]
        [TestCase(@"$x++							# value treated as int, 0->1")]
        public void Section7_1_5_Postfix_Increment_And_DecrementOperators(string input)
        {
            AssertIsValidInput(input);
        }

        [Test]
        [TestCase(@"$j = 20")]
        [TestCase(@"$($i = 10)				# pipeline gets nothing")]
        [TestCase(@"$(($i = 10)) 			# pipeline gets int 10")]
        [TestCase(@"$($i = 10; $j) 		# pipeline gets int 20")]
        [TestCase(@"$(($i = 10); $j) 		# pipeline gets [object[]](10,20)")]
        [TestCase(@"$(($i = 10); ++$j) 	# pipeline gets int 10")]
        [TestCase(@"$(($i = 10); (++$j))	# pipeline gets [object[]](10,22)")]
        [TestCase(@"$($i = 10; ++$j) 		# pipeline gets nothing")]
        [TestCase(@"$(2,4,6) 				# pipeline gets [object[]](2,4,6)")]
        public void Section7_1_6_SubexpressionOperator(string input)
        {
            AssertIsValidInput(input);
        }

        [Test, Explicit("Punting for now")]
        public void StringWithDollarSign()
        {
            // This `$%` threw off the tokenizer
            AssertIsValidInput(@"""abc#$%XYZabc""");
        }

        static void AssertIsValidInput(string input)
        {
            var parseTree = PowerShellGrammar.Parser.Parse(input);

            if (parseTree.HasErrors())
            {
                Assert.Fail(parseTree.ParserMessages[0].ToString());
            }
        }
    }
}
