Contributing to Pash
==============================

I'm so grateful that anyone would consider helping improve Pash. Thanks!


Building Pash on Windows, Linux, or Mac
---------------------------------------

I want it to be easy to build this project. You should be able to use:

- [Visual C# Express 2010](http://www.microsoft.com/express/) or later. Windows only, of course.

- [MonoDevelop 3.0](http://monodevelop.com/) or later, or Xamarin Studio, on any platform.

Open the solution at `Source/Pash.sln` and run. Or at the command line:

<!-- duplication with README.md here; keep them in synch -->

    > xbuild
    > mono Source/PashConsole/bin/Debug/Pash.exe

On Windows, you can use MSBuild instead of xbuild; the `mono` part is unecessary, and you'll probably need to add these to your PATH.


Tests
----

We use NUnit. In `master`, all tests pass all the time.

To run tests

    > xbuild /t:test


Travis
----

We're hooked up to Travis-CI. See https://travis-ci.org/JayBazuzi/Pash2

We don't merge in to `master` if Travis reports an error. So keep an eye on that.


Guidelines
----

1. **Make merging easy.**

	Any problem can be fixed, but only if we can merge that change. When you're preparing changes, keep an eye on making merging easy.

	The worst types of changes to merge are gross formatting changes, which ironically have the least impact on the way the code behaves.

2. **Make it obvious.**

	Write the cleanest code you can, giving your current understanding.

	If you can read code & know what it does without doubt, that's wonderful. If a function name and its implementation are obviously saying the same thing, that helps, too. Good automated testing also helps us know that code is correct.

	LINQ helps here a lot, turning imperative iteration in to declarative code.

	Keep code well-factored. Do not hesitate to Extract Method & Extract Class.

4. **Make change history clean.**

	Some day someone will look at this code and ask "why is it like this? How did it come to be this way? What was the developer thinking at the time?"

	You can engineer the change history as you work. Commit often, in tiny bits.

	Commit **refactorings** (which tend to modify a lot of code but hopefully don't change behavior) **separately from features/bugs** (which tend to modify less code, but deliberately introduce behavior changes). One way I like to work is to make most of my commits be refactorings, reshaping the code to make my new feature trivial to implement, followed by a single, simple commit that introduces the new behavior.

	**Good commit messages** should explain *why* you made this change, what other changes you considered, and why you rejected them, unless those things are obvious or irrelevant.


Coding Style:
----

- Use the default Visual Studio formatting settings. Keep files formatted that way at all times.

Beyond that, use your judgement to write the best code you know how.
