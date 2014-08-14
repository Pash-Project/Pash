// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using NUnit.Framework;

namespace ReferenceTests.Integration
{
    /// <summary>
    /// A series of integration tests taken from the author's own contributions to the
    /// Programming Puzzles and Code Golf Stack Exchange site. While none of them would
    /// be typical examples of how to use the language, they should test many intricacies
    /// and corner cases of the grammar, language and behaviour.
    /// </summary>
    [TestFixture]
    public class CodeGolf : ReferenceTestBase
    {
        [Test, Explicit("Parse error on subexpression within string")]
        // Adapted from http://codegolf.stackexchange.com/a/236/15
        public void CubeDrawing()
        {
            var code = NewlineJoin(
                "$t=($w=($s=' ')*($o=($n=\"6\")/2))*4",
                "$r=\"|$t|\"",
                "$s*($a=$o+1)+($q='+'+'--'*$n+'+')",
                "$o..1|%{$s*--$a+\"/$t/$($s*$b++)|\"}",
                "\"$q$w|\"",
                "for(;$o-++$x){\"$r$w|\"}\"$r$w+\"",
                "--$b..0|%{$r+$s*$_+'/'}",
                "$q");
            var expected = NewlineJoin(
                "    +------------+",
                "   /            /|",
                "  /            / |",
                " /            /  |",
                "+------------+   |",
                "|            |   |",
                "|            |   |",
                "|            |   +",
                "|            |  /",
                "|            | /",
                "|            |/",
                "+------------+");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("joined unary operator expression not yet implemented")]
        // Adapted from http://codegolf.stackexchange.com/a/243/15
        public void Reverse()
        {
            var code = "-join($x=[char[]](('Quick brown fox','He jumped over the lazy dog')-join'\n'))[($x.count)..0]";
            var expected = NewlineJoin("god yzal eht revo depmuj eH\nxof nworb kciuQ");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Parse error on subexpression within string")]
        // Taken from http://codegolf.stackexchange.com/a/316/15
        public void HelloWorld()
        {
            var code = "($OFS='')+('x)`x{umQnuu'[-4..6]|%{iex(\"[convert]::\"+([convert]|gm -s *pe).name+\"($($_-9),4)\")})";
            var expected = NewlineJoin("Hello World");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Parse error, assignment to multiple variables")]
        // Adapted from http://codegolf.stackexchange.com/a/770/15
        public void Gradient()
        {
            var code = NewlineJoin(
                "$x,$y,$c=58,14,' .:;+=xX$&'",
                "$l=$c.Length",
                "$c+=\"$($c[-1])\"*90",
                "0..24|%{$r=$_",
                "-join(0..69|%{$c[[math]::truncate([math]::sqrt(($x-$_)*($x-$_)+4*($y-$r)*($y-$r))*$l/70)]})}");
            var expected = NewlineJoin(
                "&&$$$$$$$$XXXXXXXXxxxxxxxxx===========++++++++++++++++++++++++++++++++",
                "&$$$$$$$$XXXXXXXXxxxxxxxxx=========+++++++++++++;;;;;;;;;;;;;;;;;;;;;+",
                "$$$$$$$$XXXXXXXXxxxxxxxx=========+++++++++++;;;;;;;;;;;;;;;;;;;;;;;;;;",
                "$$$$$$$XXXXXXXXxxxxxxxx========++++++++++;;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
                "$$$$$$XXXXXXXXxxxxxxxx========+++++++++;;;;;;;;;;;;;:::::::::::::;;;;;",
                "$$$$$XXXXXXXXxxxxxxxx=======+++++++++;;;;;;;;;;;:::::::::::::::::::::;",
                "$$$$$XXXXXXXxxxxxxxx=======+++++++++;;;;;;;;;:::::::::::::::::::::::::",
                "$$$$XXXXXXXXxxxxxxx=======++++++++;;;;;;;;;:::::::::::::::::::::::::::",
                "$$$$XXXXXXXxxxxxxx========+++++++;;;;;;;;::::::::::...............::::",
                "$$$XXXXXXXXxxxxxxx=======+++++++;;;;;;;;:::::::::...................::",
                "$$$XXXXXXXxxxxxxx=======++++++++;;;;;;;::::::::.......................",
                "$$$XXXXXXXxxxxxxx=======+++++++;;;;;;;::::::::.........       ........",
                "$$$XXXXXXXxxxxxxx=======+++++++;;;;;;;:::::::........           ......",
                "$$$XXXXXXXxxxxxxx=======+++++++;;;;;;;:::::::.......             .....",
                "$$$XXXXXXXxxxxxxx=======+++++++;;;;;;;:::::::.......             .....",
                "$$$XXXXXXXxxxxxxx=======+++++++;;;;;;;:::::::.......             .....",
                "$$$XXXXXXXxxxxxxx=======+++++++;;;;;;;:::::::........           ......",
                "$$$XXXXXXXxxxxxxx=======+++++++;;;;;;;::::::::.........       ........",
                "$$$XXXXXXXxxxxxxx=======++++++++;;;;;;;::::::::.......................",
                "$$$XXXXXXXXxxxxxxx=======+++++++;;;;;;;;:::::::::...................::",
                "$$$$XXXXXXXxxxxxxx========+++++++;;;;;;;;::::::::::...............::::",
                "$$$$XXXXXXXXxxxxxxx=======++++++++;;;;;;;;;:::::::::::::::::::::::::::",
                "$$$$$XXXXXXXxxxxxxxx=======+++++++++;;;;;;;;;:::::::::::::::::::::::::",
                "$$$$$XXXXXXXXxxxxxxxx=======+++++++++;;;;;;;;;;;:::::::::::::::::::::;",
                "$$$$$$XXXXXXXXxxxxxxxx========+++++++++;;;;;;;;;;;;;:::::::::::::;;;;;");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("joined unary operator expression not yet implemented")]
        // Adapted from http://codegolf.stackexchange.com/a/680/15
        public void Factors()
        {
            var code = "2..($x=123)|%{for(;!($x%$_)){$_;$x/=$_}}";
            var expected = NewlineJoin("3", "41");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("joined unary operator expression not yet implemented")]
        [SetCulture("en-US")]
        // Adapted from http://codegolf.stackexchange.com/a/794/15
        public void Snow()
        {
            var code = NewlineJoin(
                "$s='        |        |        |        |* * **  |** * ***|** *  * |*       |--------'",
                "$a=(0..7|%{$x=$_;-join(0..7|%{$s[$_*9+$x]})})",
                "$b=$a-replace' '|%{$_.length}|sort",
                @"'{0}|{1}|{2:.0}|{3}'-f($b[-1..0]+(($b-join'+'|iex)/8),($a|%{($_-replace'^ +|\*').length}|sort)[-1])");
            var expected = NewlineJoin("4|1|1.9|3");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Parse error on subexpression within string")]
        // Adapted from http://codegolf.stackexchange.com/a/695/15
        public void Factorial()
        {
            var code = NewlineJoin(
                "($a=1)..'100'|%{$a=\"$($a*$_)\".trim('0')%1e7}",
                "$a%10");
            var expected = NewlineJoin("4");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("joined unary operator expression not yet implemented")]
        // Adapted from http://codegolf.stackexchange.com/a/1238/15
        public void BinaryPalindrome()
        {
            var code = "('NO','YES')[($a=[Convert]::ToString('5',2))-eq-join$a[64..0]]";
            var expected = NewlineJoin("YES");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("joined unary operator expression not yet implemented")]
        // Adapted from http://codegolf.stackexchange.com/a/738/15
        public void SumIntegersInString1()
        {
            var code = @"'e7rde f ,fe 43 jfj 54f4sD'-split'\D'|%{$s+=+$_};$s";
            var expected = NewlineJoin("108");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("-replace not yet implemented")]
        // Adapted from http://codegolf.stackexchange.com/a/738/15
        public void SumIntegersInString2()
        {
            var code = @"'64 545,5445-32JIFk0ddk'-replace'\D','+0'|iex";
            var expected = NewlineJoin("6086");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("We don't yet allow statements directly after blocks that belong to function, filter, etc.")]
        // Adapted from http://codegolf.stackexchange.com/a/763/15
        public void Symmetry()
        {
            var code = NewlineJoin(
                @"$i='+---','|./.','|/..'",
                @"filter x{$_-split'/'-replace'\\','/'-join'\'}$i|%{$_+-join($_[40..0]|x)}|%{$_",
                @"$s=,($_|x)+$s}",
                @"$s");
            var expected = NewlineJoin(
                @"+------+",
                @"|./..\.|",
                @"|/....\|",
                @"|\..../|",
                @"|.\../.|",
                @"+------+",
                "");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("joined unary operator expression not yet implemented")]
        // Adapted from http://codegolf.stackexchange.com/a/941/15
        public void Count196()
        {
            var code = NewlineJoin(
                "function f($n){",
                "  @($(",
                "    $n % 2",
                "    for(;-join\"$n\"[99..0]-ne$n) {",
                "      $n+=-join\"$n\"[99..0]",
                "      $n % 2",
                "    }",
                "  ) -eq 0).Count",
                "}",
                "f 89");
            var expected = NewlineJoin("13");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("We don't yet allow statements directly after blocks that belong to for, etc.")]
        // Adapted from http://codegolf.stackexchange.com/a/982/15
        public void Friday13th()
        {
            var code = "for($d=\"date 2013-05-09\"|iex;($d+='1').day*$d.dayofweek-65){}'{0:yyy-MM-d}'-f$d";
            var expected = NewlineJoin("2013-09-13");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Missing aliases, split not yet implemented")]
        // Adapted from http://codegolf.stackexchange.com/a/1009/15
        public void SevenSegment()
        {
            var code = NewlineJoin(
                "$i=\"0123456789\"[0..20]",
                "'\u0001\u0000\u0001\u0001\u0000\u0001\u0001\u0001\u0001\u0001','\u0006\u0002\u0003\u0003\u0004\u0005\u0005\u0002\u0004\u0004','\u0004\u0002\u0005\u0003\u0002\u0003\u0004\u0002\u0004\u0003'|%{$c=$_",
                "\"\"+($i|%{('   0 _ 0  |0 _|0|_|0|_ 0| |'-split0)[$c[$_-48]]})}");
            var expected = NewlineJoin(
                " _       _   _       _   _   _   _   _ ",
                "| |   |  _|  _| |_| |_  |_    | |_| |_|",
                "|_|   | |_   _|   |  _| |_|   | |_|  _|");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("We don't yet allow statements directly after blocks that belong to for, etc.")]
        // Adapted from http://codegolf.stackexchange.com/a/1007/15
        public void February29th()
        {
            var code = NewlineJoin(
                "filter f{for($d=date $_;($d+='1').day*$d.month-58){}$d.dayofweek}",
                "'1999-07-06'|f");
            var expected = NewlineJoin("Tuesday");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("joined unary operator expression not yet implemented")]
        // Adapted from http://codegolf.stackexchange.com/q/1159/15
        public void NumbersSumsProducts()
        {
            var code = NewlineJoin(
                "$g=-split'1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 5 5 5 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 5 5 5 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 5 5 5 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1'",
                "(0..359|?{$_%20-le17}|%{$i=$_",
                "(0..2|%{$g[($a=$i+20*$_)..($a+2)]-join'+'|iex})-join'*'|iex}|sort)[-1]");
            var expected = NewlineJoin("3375");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("joined unary operator expression not yet implemented")]
        // Adapted from http://codegolf.stackexchange.com/a/3080/15
        public void SumPositive()
        {
            var code = "($i='5','5','5','-10','-2','-4','8'|%{+$_})[1..$i[0]]-gt0-join'+'|iex";
            var expected = NewlineJoin("10");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Parse error on subexpression within string")]
        // Adapted from http://codegolf.stackexchange.com/a/1200/15
        public void BinaryClock()
        {
            var code = "-join\" .':\n\"[(($d=\"$(date)17:59:20\"[-8..-1]-ne58)|%{($_-band12)/4})+,4+($d|%{$_%4})]";
            var expected = NewlineJoin(" ..'  \n.:..' ");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Aliases are still missing")]
        [SetCulture("en-US")]
        // Adapted from http://codegolf.stackexchange.com/a/1233/15
        public void CoinToss()
        {
            var code = NewlineJoin(
                "($p=1)..($n='8'/2)|%{$p*=(1+$n/$_)/4}",
                "$p");
            var expected = NewlineJoin("0.2734375");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("We don't yet allow statements directly after blocks that belong to for, etc.")]
        // Adapted from http://codegolf.stackexchange.com/a/1267/15
        public void Kaprekar1()
        {
            var code = "for($a=\"211\";$a-495;++$i){$a=+-join($x=\"00$a\"[-1..-3]|sort)[2..0]- -join$x}+$i";
            var expected = NewlineJoin("6");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("InvalidOperation: StandardIn has not been redirected")]
        // Adapted from http://codegolf.stackexchange.com/a/1267/15
        public void Kaprekar2()
        {
            var code = "(7..3+1..5)[($x=($a='467')[0..2]|sort)[2]-$x[0]]*($a-ne495)";
            var expected = NewlineJoin("4");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Assignments to multiple variables")]
        // Adapted from http://codegolf.stackexchange.com/a/1385/15
        public void Rotated()
        {
            var code = NewlineJoin(
                "$a,$b=-split'CodeGolf GolfCode'",
                "('No','Yes')[+!($a.length-$b.length)*\"$b$b\".contains($a)]");
            var expected = NewlineJoin("Yes");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Parse error at the start of the filter body. Maybe caused by the string with subexpression")]
        // Adapted from http://codegolf.stackexchange.com/a/1543/15
        public void Abacus()
        {
            var code = NewlineJoin(
                "$y='='*(2+6*($i=[char[]]'314159').Count)",
                "filter f($v){\"|| $((' (__)','  || ')[($i|%{iex $_%$v})])  ||\"}\"|\\$y/|\"",
                "f 1",
                "f 10-ge5",
                "f 1+1",
                "f 10-lt5",
                "\"|<$y>|\"",
                "'lt','ge'|%{$x=$_",
                "1..5|%{f 5-$x$_}}",
                "\"|/$y\\|\"");
            var expected = NewlineJoin(
                "|\\======================================/|",
                "||  (__)  (__)  (__)  (__)  (__)  (__)  ||",
                "||  (__)  (__)  (__)  (__)   ||    ||   ||",
                "||   ||    ||    ||    ||    ||    ||   ||",
                "||   ||    ||    ||    ||   (__)  (__)  ||",
                "|<======================================>|",
                "||  (__)  (__)  (__)  (__)   ||   (__)  ||",
                "||  (__)   ||   (__)   ||    ||   (__)  ||",
                "||  (__)   ||   (__)   ||    ||   (__)  ||",
                "||   ||    ||   (__)   ||    ||   (__)  ||",
                "||   ||    ||    ||    ||    ||    ||   ||",
                "||   ||    ||    ||    ||   (__)   ||   ||",
                "||   ||   (__)   ||   (__)  (__)   ||   ||",
                "||   ||   (__)   ||   (__)  (__)   ||   ||",
                "||  (__)  (__)   ||   (__)  (__)   ||   ||",
                "||  (__)  (__)  (__)  (__)  (__)  (__)  ||",
                "|/======================================\\|");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("joined unary operator expression not yet implemented")]
        // Taken from http://codegolf.stackexchange.com/a/1539/15
        public void HelloWorld2()
        {
            var code = NewlineJoin(
                "&{-join($args|%{[char]$_.Length})} `",
                "O00000000000000000000000000000000000000000000000000000000000000000000000 `",
                "O0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 `",
                "O00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 `",
                "O00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 `",
                "O00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 `",
                "O0000000000000000000000000000000 `",
                "O00000000000000000000000000000000000000000000000000000000000000000000000000000000000000 `",
                "O00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 `",
                "O00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 `",
                "O00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 `",
                "O000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 `",
                "O00000000000000000000000000000000");
            var expected = NewlineJoin("Hello World!");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Assignment to multiple variables")]
        // Adapted from http://codegolf.stackexchange.com/a/1616/15
        public void Langton()
        {
            var code = NewlineJoin(
                "for($p,$n,$g=144,450+,1*289;$n--){$d+=$g[$p]*=-1",
                "$p+='B0@R'[$d%4]-65}$g[$p]=0",
                "-join'@_#'[$g]-replace'.{17}',\"$&",
                "\"");
            var expected = NewlineJoin(
                "_________________",
                "_________________",
                "___________##____",
                "____##______##___",
                "___#__##___##_#__",
                "__###_#@#__#__#__",
                "__#_#_#_#__#_#___",
                "_____###___#_____",
                "_____#___________",
                "_____#__###______",
                "___#_#_#__#_#_#__",
                "__#__#_#____###__",
                "__#_##__##___#___",
                "___##______##____",
                "____##___________",
                "_________________",
                "_________________");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("We don't yet allow statements directly after blocks that belong to function, filter, etc.")]
        // Reference solution for http://codegolf.stackexchange.com/q/1720/15
        public void Paths()
        {
            var code = NewlineJoin(
                "$g=,0*1764",
                "$h=,0*1764",
                "$i='881 461 792','382 277 406','147 645 930'",
                "$i|%{++$a;$b=0;-split$_|%{$g[$b+$a*42]=$h[++$b*42+$a]=+$_}}",
                "filter f{$g=$_;1..40|%{($x=42*$_)..($x-40)|%{$g[$_+41]+=($g[($_-2)..$_]|sort)[-1]}}}$g,$h|f",
                "\"$(($h|sort)[-1]) $(($g|sort)[-1])\"");
            var expected = NewlineJoin("2134 2128");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Assignment to multiple variables")]
        // Adapted from http://codegolf.stackexchange.com/a/1752/15
        public void Combinations()
        {
            var code = NewlineJoin(
                "$a,$b=iex '5,2'",
                "$p=1",
                "for($a-=$b;$a-ge1){$p*=1+$b/$a--}$p");
            var expected = NewlineJoin("10");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("We don't yet allow statements directly after blocks that belong to if, etc.")]
        // Adapted from http://codegolf.stackexchange.com/a/2414/15
        public void ProductOfDigits()
        {
            var code = "if(($n=15)-le1){$n;exit}(-1,-join(9..2|%{for(;!($n%$_)){$_;$n/=$_}}|sort))[$n-eq1]";
            var expected = NewlineJoin("35");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("NullReferenceException in BuildUnaryDashExpressionAst")]
        [SetCulture("en-US")]
        // Adapted from http://codegolf.stackexchange.com/a/2908/15
        public void Treasure()
        {
            var code = NewlineJoin(
                "'N 3, E 1, N 1, E 3, S 2, W 1'-split','|%{,@{N=0,1",
                "NE=($s=.707106781186548),$s",
                "E=1,0",
                "SE=$s,-$s",
                "S=0,-1",
                "SW=-$s,-$s",
                "W=-1,0",
                "NW=-$s,$s}[($a=-split$_)[0]]*$a[1]}|%{$x+=$_[0]",
                "$y+=$_[1]}",
                "'{0:N3}, {1:N3}'-f$x,$y");
            var expected = NewlineJoin("3.000, 2.000");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Assignment to multiple variables")]
        // Adapted from http://codegolf.stackexchange.com/a/33053/15
        public void Milk()
        {
            var code = NewlineJoin(
                "function date($d) { if ($d){[datetime]$d} else {[datetime]'2006-07-05'} }",
                "[int](100*(($d=@(($a,$b,$c=6,7,5),($c,$a,$b),($c,$b,$a)|%{$_[0]+=1900*($_[0]-le99)+100*($_[0]-le49)",
                ".{date($_-join'-')}2>$x}|sort -u))-ge(date)+'-1').Count/$d.Count)");
            var expected = NewlineJoin("33");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("joined unary operator expression not implemented")]
        // Adapted from http://codegolf.stackexchange.com/a/2661/15
        public void Heightmap()
        {
            var code = NewlineJoin(
                "(($i=@('5321','1456','2105'))-split''|sort)[-1]..1|%{$h=$_",
                "-join(1..$i[0].Length|%{$x=$_-1",
                "@($i|?{\"$h\"-le$_[$x]}).count})}");
            var expected = NewlineJoin(
                "0001",
                "1012",
                "1112",
                "1212",
                "2222",
                "3323");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("joined unary operator expression not implemented")]
        // Adapted from http://codegolf.stackexchange.com/a/2606/15
        public void Dice()
        {
            var code = string.Join("\n", new[] {
                "-join('-----",
                "|{0} {1}|",
                "|{2}{3}'-f'o '[$(!($x=4)",
                "$x-lt3",
                "$x-ne5",
                "$x%2)])[0..14+13..0]"});
            var expected = string.Join("\n", new[] {
                "-----",
                "|o o|",
                "| o |",
                "|o o|",
                "-----"}) + Environment.NewLine;
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("joined unary operator expression not implemented")]
        // Adapted from http://codegolf.stackexchange.com/a/2522/15
        public void Tags()
        {
            var code = NewlineJoin(
                ".{$s=''",
                "-join([char[]]\"$args \"|%{if(90-ge$_){')'*(($x=$s.indexOf(\"$_\".ToLower())+1)+$s.Length*!$x)",
                "$s=$s.substring($x)}else{\"($_\"",
                "$s=\"$_$s\"}})} abBcdDCA");
            var expected = NewlineJoin("(a(b)(c(d)))");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Parse eror on variable declaration with type")]
        // Adapted from http://codegolf.stackexchange.com/a/373/15
        public void IPAddress()
        {
            var code = NewlineJoin(
                "filter I{[int[]]$x=$_-split'\\.'",
                "$x[0]*16MB+$x[1]*64KB+$x[2]*256+$x[3]}",
                "'192.168.1.1'|I");
            var expected = NewlineJoin("3232235777");
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }

        [Test, Explicit("Parse error at subexpression in string")]
        // Taken from http://codegolf.stackexchange.com/a/34486/15
        public void Golf()
        {
            var code = "-join('̠§Üঠ®Ġ®ఠü¾±¸ľРÜܠ®Ҡ§ ®ঠüΠÏľҠ®ࢠ§ïࠠüРÜΠ®ጠüР¯ÜȠ®ᐠüΠ¯ ¯Ġ®§ᒠü êçóϞà᧞'[0..70]|%{\"$([char]($_%128))\"*(+$_-shr7)})";
            var expected = string.Join("\n", new[] {
                @"",
                @"      '\                   .  .                        |>18>>",
                @"        \              .         ' .                   |",
                @"       O>>         .                 'o                |",
                @"        \       .                                      |",
                @"        /\    .                                        |",
                @"       / /  .'                                         |",
                @" jgs^^^^^^^`^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^"}) + Environment.NewLine;
            var actual = ReferenceHost.Execute(code);
            Assert.AreEqual(expected, actual);
        }
    }
}