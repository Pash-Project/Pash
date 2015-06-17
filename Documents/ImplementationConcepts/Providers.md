Providers
=========
This document doesn't cover the detailed concept of providers itself,
but the ideas behind Pash's implementation of these concepts.  So check
out official Powershell documentation if you want to learn more about
providers in general.

The basic idea of Providers is to abstract data related cmdlets like
`Get-Item` from a specific data source, like the filesystem.  Doing so,
the Pash user can access all data sources in a unique, familiar way,
while the developers don't need to implement similar cmdlets for each
data source.

Core Components
---------------
Let's start with a short description of the core components involved in
the provider concept, before there connection is shown in a concrete
example.

### Provider Core Classes
If you want to implement a new provider, you need to derive from core
classes `NavigationCmdletProvider`, `ContainerCmdletProvider`, or others.
They don't contain much logic and should only serve as a base class to
derive from.  It would't make much sense to implement common logic here,
because some behavior, e.g. for `GetChildItems`, can be the same for e.g.
`ContainerCmdletProvider` and `NavigationCmdletProvider`, with some extra
logic for the latter one.  So where should that be implemented?  This would
get very nasty.

### Provider Cmdlets
Starting with the provider cmdlets, you can see that they share some
common parameters like `Include`, `Exclude`, `Filter`, `Force`,
`Credential`, and maybe `Path` and `LiteralPath`.  As you can already
imagine, it would be quite tedious to pass and check process all these
parameters in each of the cmdlets.  Instead, they simply override a
protected base attribute in `CoreCommandBase` (and derivatives) and can
be accessed as part of the `ProviderRuntime` member.

### Provider Intrinsics
The Intrinsics implement the common logic of the cmdlets, so it doesn't
need to be implemented in the cmdlets or the provider core classes.  This
has two advantages:
1.  The provider's functionality is not only available for cmdlets as a
    user interface in the Pash language, but also as an API.
2.  It is very easy to access this provider functionality easly at low
    cost from everywhere in your code (see [Intrinsics](Intrinsics.md))

### ProviderRuntime
The connecting piece between all components is the `ProviderRuntime`.
It serves as a general context of the operations and abstracts provider's
output mechanism, so it can either be captured or directly be forwarded
to a cmdlet's output pipe.

### PathGlobber
It's responsible for resolving paths containing wildcards, finding
the drive and provider for a given path, while applying the `Include`
and `Exclude` filters if provided.

It has three main tasks:
1. Format the path.  A path can be in different forms: relative or
   absolute; provider qualified, drive qualified, or provider direct.
   The provider implementation shouldn't bother about these differences,
   but get something they can rely on (see below).
   When a path is formatted, information about the affected provider and
   drive of this path will be saved in the `ProviderRuntime` (and sometimes
   returned).

2. Resolve wildcards.  Many cmdlets allow wildcards in the paths passed.
   The globber's work is to resolve them by splitting paths and querying
   child items, so all providers that support these operations can take
   advantage of this built-in globbing mechanism.

3. Regard the filters.  By using the `ProviderRuntime`, the `PathGlobber`
   knows about the `Include` and `Exclude` filters that may have been
   set.  It can therefore directly apply them when resolving the wildcards,
   so any not-matching path get directly discarded.



Example
-------
This example should show the actual purpose of the components and how
they work together.  We assume that a user wants to get a provider 
specific item with the `Get-Item` cmdlet.

The `GetItemCommand` class implements this cmdlet.  This cmdlet can take
the parameters `Include`, `Exclude`, `Filter`, `Credential`, `Path` or
`LiteralPath`.  This is quite common for provider cmdlets, so the class
is simply derived from `CoreCommandWithFilteredPathsBase`.  It also takes
the `Force` parameter, so it defines it by overriding the protected
`Force` property and decorating it with a `ParameterAttribute`.

In the `ProcessRecord` method of the command, we need to actually get the
item. But instead of caring about all the parameters, we can just use the
`ProviderRuntime` property, which creates an identically named object
with all these information set.  Another userful property that should be
used is `InternalPaths`, which either includes the content of the `Path`
or `LiteralPath` parameter.  Depending on which of them was actually used,
the runtime's `AvoidGlobbing` property will be set, because this is what
these parameters are all about.  Finally, the cmdlet only needs to call
the Intrinsics:
    InvokeProvider.Item.Get(InternalPaths, ProviderRuntime);


The `ItemCmdletProviderIntrinsics`, exposed by `InvokeProvider.Item`
to the provider cmdlets, contains three definitions for mostly all
methods.  The real work is done by the internal method, as the one called
by the cmdlet.  It takes a `ProviderRuntime` and has therefore all
information about the invocation.  The other two functions are part of
the public Powershell compatible API and are defined for `Get` as follows:

	public Collection<PSObject> Get(string path)
	{
		return Get(new [] { path }, false, false);
	}

	public Collection<PSObject> Get(string[] path, bool force, bool literalPath)
	{
		var runtime = new ProviderRuntime(SessionState, force, literalPath);
		Get(path, runtime);
		return runtime.ThrowFirstErrorOrReturnResults();
	}

As you can see, they only provide a limited, public interface to the
internal method.  It's internal, because the `ProviderRuntime` is an
internal concept and not designed for external use.  In the second
definition of `Get`, you can see that the runtime actually also contains
the results of the whole operation and can either return them or throw an
error.  This is different for runtimes created by the cmdlets: They will
(typically) not capture the results and errors, but directly forward them
to the cmdlet's pipes.  So Neither the cmdlet, nor the provider developer
needs to bother about where the results are going.

The internal method is simple in this example:
	internal void Get(string[] path, ProviderRuntime runtime)
	{
		GlobAndInvoke<ItemCmdletProvider>(path, runtime,
			(curPath, provider) => provider.GetItem(curPath, runtime)
		);
	}

It only calls `GlobAndInvoke`, as it's only necessary to glob the input
paths, call the core function for each resolved path, and check for
errors.  As many operations are simple like this, `GlobAndInvoke` does
all this work, only taking a delegate that can use the concrete
provider instance and a resolved path to invoke the right core function.
However, there are functions that are more elaborate, as `Copy`, because
with filters like `Include` and `Exclude` being set, it needs to
manually do the recursion in order to only affect the correct items.
This is tedious in the Intrinsics' implementation, but keeps all that
work from being done in each provider implementation separately.

When the actual provider's core function is called, you can observe that
it calls `GetItem(curPath, runtime)` although a provider deriving from
`ItemCmdletProvider` only implements
    protected virtual void GetItem(string path);
The internal method that is actually called, does the final trick: It
sets the ProviderRuntime in the provider's instance before invoking it.
By doing this, several context information can be used in the actual
Provider.  For example the `Filter`, originally set as a parameter, now
saved in the runtime, is exposed through the provider's `Filter`
property, so it can be read during the core operation.  Similarly, when
the `PathGlobber` resolved and formated the path, it saved which drive
is affected in the runtime's `PSDriveInfo` property, which is also
exposed to the provider by the same-named property.
Also, the input and output methods like `WriteObject` and `WriteError`
are redirected to the runtime, as already mentioned above.

Remarks about Paths
-------------------
Neither the Powershell "documentation", nor their examples are very clear
about what kind of "path" a provider can expect in it's core functions
that are invoked by the Intrinsics.  Therefore it's clarified it here by
the current state of knowledge.

Provider's function should always get an *absolute* path, never a
relative path.  A prominent exception for this rule is the
`NavigationCmdletProvider`'s `MakePath(string path, string child)` which
might also be called to combine a child to any kind of path.
Otherwise, *absolute* also means, that there is no drive qualifier
or provider qualifier in the beginning of the start.  So it's the
so called "provider internal" path, which might be either a "normal"
unqualified absolute path, or a "provider specific" path (those that
start with `\\`, if your provider supports it).

Last but not least, it's important to mention, that if a drive qualified
path is resolved by the `PathGlobber`, it doesn't only remove the drive
qualification and sets the corresponding `PSDriveInfo` property in the
runtime, but it also prepends the drive's root path *if the provider
supports it*.  Providers that are not derived by
`NavigationCmdletProvider` don't support joining paths, so the paths
root will *not* be prepended, which is also confirmed Powershell
behavior.

Note that the drive's root can also be something that looks like a drive,
but highly depends on the kind of data source.
For example the root of the FileSystem provider's `C:` drive is also
`C:`.  This is, because Window's file system is also a drive-based
concept which is not necessarily identical to the one of Powershell/Pash.
Only in context of the file system provider, they work very similar to
provide a familiar usage for Windows users.
