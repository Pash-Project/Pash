Contributing to Pash
==============================

I'm so grateful that anyone would consider helping improve Pash. Thanks!

If you like Pash and want to express your gratitude, you could:

- Contribute code to Pash.
- Contribute to some other open source project.
- Edit Wikipedia.
- Create art, music, architecture, boat plans, whatever, and release it under Creative Commons.
- Volunteer in your community.
- Give free hugs!
- If you're making money with Pash, then [send money](http://pledgie.com/campaigns/19268).

Building Pash on Windows
----

I want it to be easy to build this project. You should be able to use:

- Visual Studio 2012 Pro (I use this)

- Visual Studio Express for Desktop 2012

- Visual C# 2010

Building Pash on Mac or Linux
----

- MonoDevelop 3.0 on Windows or Linux. (See https://github.com/JayBazuzi/Pash2/issues/16). Follow the steps in `.travis.yml`.

Travis
----

We're hooked up to Travis-CI. See https://travis-ci.org/JayBazuzi/Pash2

Make these things easy:
----

1. **Merging.**

	Any problem can be fixed, but only if we can merge that change.

	The worst types of changes to merge are gross formatting changes, which ironically have the least impact on the way the code behaves. 

2. **Know that code is correct.**

	If you can read code & know what it does without doubt, that helps. If a function name and its implementation are obviously saying the same thing, that helps, too. Good automated testing also works. 

	LINQ helps here a lot. 

3. **Read the code & understand what it does.**

	Keep code well-factored. Do not hesitate to Extract Method & Extract Class.

4. **Read change history.**

	Some day someone will look at this code and ask "why is it like this? How did it come to be this way? What was the developer thinking at the time?"

	You can engineer the change history as you work.


Tests
----

Tests are great! All tests should pass all the time. See `.travis.yml` for how to run the tests.


Coding Style:
----

- Use the default Visual Studio formatting settings. Keep files formatted that way at all times. 4 spaces, no tabs, open brace on the next line (because that's the VS default).

- Commit **refactorings** (which tend to modify a lot of code but hopefully don't change behavior) **separately from features/bugs** (which tend to modify less code, but deliberately introduce behavior changes). One way I like to work is to make most of my commits be refactorings, reshaping the code to make my new feature trivial to implement, followed by a single, simple commit that introduces the new behavior.

- **Good commit messages** should explain *why* you made this change, what other changes you considered, and why you rejected them, unless those things are obvious or irrelevant.
