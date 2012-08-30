How I work
====

1. Create a new branch. I often don't know exactly where I'm going, so I just call the branch `work`:

	PS> git checkout -b work

2. Make small changes, commiting refactorings separately from behavior changes:

	# Refactor
	PS> git commit -am "REFACTOR: Extract Method or whatever"
	# fix a bug
	PS> git commit -am "Fix xxx"

(I have this ideal where most of my edits are refactorings. I look at a bug fix, and imagine "what would it look like if this bug was trivial to fix?". Then I refactor to get like that. Then I make the trivial, obvious fix.)

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
