Goals
====

Besides the obvious goal of delivering value to users as quickly as possible, I also like to create a commit history that is easy to understand:

 - Diffs should be easy to read.
 - Commit descriptions should be detailed, and match the diffs.
 - If one commit conflicts during a merge, it should be easy to understand how to recreate that commit from scratch. 

Also, you should be able to checkout any point in the commit history and have a shippable product that passes all tests.

These are the ways I know to do that.

How I work
====

I start by picking some user-visible change I'd like to create. Perhaps a new feature. Then I create a branch:

	PS> git checkout -b work

A more descriptive name is fine, but my branches are short-lived and rarely pushed, so it's not important to me. Also, even though I have a goal when I start, I usually end up somewhere else anyway, so naming the branch after a goal I won't reach is silly.

Next, I ask myself the question "what would it look like if this feature/bug/whatever was trivial to implement/fix/whatever?" Then I refactor to get like that. Finally I make the trivial change.

	# Refactor
	PS> git commit -am "REFACTOR: Extract Method or whatever"
	# Refactor
	PS> git commit -am "REFACTOR: Extract Method or whatever"
	# Fix + test
	PS> git commit -am "Fix Issue:666"

I expect all tests to pass before committing. Always. If I want to commit a failing acceptance test for the work I'm doing, I'll mark it [Ignore] so it's easy available as I work, and then remove the [Ignore] for the final commit.

I may massage the commits a little with cherry-pick/squash/rebase, to make a clean, readable commit log. Then I'll merge to master and push.

With this workflow, each step is visible in the master branch.

The commits should generally be small, but if they are minute, the master branch log becomes tedious to read. Some judgment is required.

----

At my old job, our source control system made it easy to roll up a bunch of changes as one merge commit. You can accomplish something similar with `git merge --squash`. With this workflow, only the composite steps are visible in master, but you can drill down for more detail. 

1. Create a new branch.

	PS> git checkout -b work

2. Make small changes, committing refactorings separately from behavior changes:

	# Create failing acceptance test for the feature/bug/whatever I want to implement
	PS> git commit -am "Test I would like to satisfy"
	# Refactor
	PS> git commit -am "REFACTOR: Extract Method or whatever"
	# fix a bug
	PS> git commit -am "Fix xxx"

Tests can fail along the way, but at this point all tests pass (including the new test).

3. Rename the branch to match what I actually did:

	PS> git branch -m work foo

4. Merge with squash:

	PS> git checkout master
	PS> git merge foo --squash

5. write a summary commit description

	PS> git log ..work --oneline | % { "`t Commit: $_" } | clip
	PS> git commit

The final commit message can look like this:

	Foo to baz the bob. 

	Detailed, public description here.

		Commit: 7b248a6 Behavior change...
		Commit: d5defb4 REFACTOR: ...
		Commit: b25f1da REFACTOR: ...
		Commit: 35c49d1 REFACTOR: ...
		Commit: 70b0a2b REFACTOR: ...

Github will show these commits as links

6. Rename branch to indicate it's merged:

	PS> git branch -m foo merged/foo

7. Push to github

	PS> git push --all
