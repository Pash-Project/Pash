Scopes
======
Like other script languages and programming languages, Pash supports scopes.
This means that access to some items, like variables, is limited to certain
environments, like functions or modules.  It also allows these environments
to reuse an item name without modifying their global value.
For detailed information about the concept of scopes, please refer to the
Powershell documentation 'about_Scopes'.

Clarifications about the official documentation
-----------------------------------------------
What can be slightly misleading in the documentation are the named scopes.
At first glance one could think that these names "global", "local", "script",
and "private" are all separately existing scopes.  But in fact, they are only
shortcuts for finding the closest scope associated with the term in the whole
scope *hierarchy*.  Let's have an example:

From the global context, the user invokes a script, which invokes another
script, that contains a function with some code that is just being executed.
We abbreviate this state with
    G > S > S > F
Every "context" has its own scope, while the global context is the "root"
scope with no parent scope.  All other scopes are child scopes of the context
they are created in, as the abbreviation already suggests.
If the user now uses the "script" scope identifier to set a variable, this
variable will be set in the context of the second script, because it's the
*closest* "script" scope to the code where the identifier is used.

Very misleading is the "private" scope, because it suggests that the local
scope and the private scope are completely separate.  But that's not the case.
Instead, scope identifier "private" is just a shortcut for setting items with the
"private" option, meaning that child scopes aren't allowed to access this item.

Scoped Items
------------
In Pash there are in 4 classic types of scoped items:  Aliases, Functions,
Variables, and Drives.  Each of it internally implements the `IScopedItem`
interface which has three purposes:
1. It provides an `ItemName` which is used to identify the item. This is for
   example simply the variable name.
2. It provides `ScopedItemOptions` which are used to set scope related options,
   meaning if the item is ReadOnly, Private, Constant, or AllScope.
3. It serves as a common base between all scoped items, which becomes handy in
   the implementation of scopes.

In addition to the classic scope item types, also Modules and Cmdlets are scoped.
This might not be clear to the user, because they can only live in either a global
scope, or a module scope, but not in script or function scopes. Also, the term
"module scope" might be unknown to the user, as it's not an existing shortcut or
used in the documentation.

However, taking a closer look to the module system, the connection to scopes
become clear.  When you import a module, you can either define the `-Global` or
`-Scope` parameter to tell Pash whether this module should be available globally,
so in the global scope, or just in the parent module.  Also, when using a script
module, you should explicitly name the items that should be *exported*.  In other
words, you define which items are local to the modules scope and which are 
moved to the parent scope.  Using scopes to implement this behavior is therefore
an easy and transparent approach.

Implementation of Scopes
------------------------
In general there exists one [SessionState](SessionState.md) for each existing
scope.  So if Pash executes a function, it will create a new `SessionState` that
is linked to the parent `SessionState`.  The basic operations on scoped items
are basically all the same: Fidning items in the scope hierarchy, getting,
replacing, or removing them with respect to the `ScopedItemOptions`.  However,
the official Microsoft API and internal behavior differ slightly for some
item types (e.g. Modules can only added to module scopes). That's why access
to them is implemented as [Intrinsics](Intrinsics.md), with one Intrinsic class for
each type, and basic data handling is implemented in
`SessionStateScope<T> where T : IScopedItem`.

Doing so, one `SessionStateScope` object represents *one* scope hierarchy level
for *one* specific item type, which is linked to the parent `SessionStateScope`
scope for that type. This means in detail, that whenever a new `SessionState`
is created, the `SessionState` will create one new `SessionStateScope` for
each item type and link them to their respective parent. It will then construct
the respective Intrinsic object to wrap the specific `SessionStateScope` of this
scope hierarchy to enable item access from outside the `SessionState` object.

Although it might looks complicated on the first sight, as a total of
`6*#scopes` `SessionStateScope` and `Intrinsic` objects will exist,
this approach allows both a clear separation of item types in the scopes,
with unified internal scope handling while providing custom APIs.
Also, this concept is easily extensible if there is another item category that
should be scoped in the future.
