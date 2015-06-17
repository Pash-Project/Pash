Intrinsics
==========
'Intrinsic classes' are all those whose name end with 'Intrinsics'.  They
are mostly used to provide  easy API to to manipulate data, without
actually containing that data (in contrast to other classes).  This means
Instrinsic objects are cheap to construct whenever you need comfortable
access to some data.

SessionState Intrinsics
-----------------------
The [SessionState](SessionState.md) provides some Intrinsic objects to
easily manipulate the data related to the session's state.  For example
the `PSVariableIntrinsics` class provides functions to easily add, get,
set, and remove variables in *all* [Scopes](Scopes.md) that are accessible
in the current SessionState. However, it does not contain these variables,
but only needs a reference to the `SessionStateScope` hierarchy which
holds all variable data.

For other scoped data there are also Intrinsics available. Some are
for also available for public use (e.g. `DriveManagementIntrinsics` by
using `$ExecutionContext.SessionState.Drive` in Pash), and some are
for comfortable internal use (e.g. `CmdletIntrinsics`).

Without the Intrinsics we could not provide both a comfortable, data
specific API while maintaining a unique way to handle scoped items.

But it's not only good for scoped data, but also for global data. For
example loaded Providers aren't scope specific, but always global in the
context of a Pash session.  It's the same with Paths (and locations).
To avoid to implement all API in the `SessionStateGlobal` class, it's
implemented in the `PathIntrinsics` and 
`CmdletProviderManagementIntrinsics` while the data stays in the
`SessionStateGlobal`.  So if you need comfortable path related
functionality anywhere in your code, you can quickly instantiated some
`PathIntrinsics` and start your work.


Provider Intrinsics
-------------------
While [Providers](Providers.md) all have there own core implementations,
they share some common functionality.  The most basic provider related
functionality is to decide which provider should actually be invoked,
to glob paths with wildcards and make use of include/exclude filters.

Because all that work is tedious, you wouldn't want to do this every time
you need access to provider functionality.  For this reason Intrinsics
like the `ItemCmdletProviderIntrinsics` exist.  You can cheaply
instantiate that class and for example call `Exists` to check if there
is any provider that knows if there exists some data at that path.

So, similar to the SessionState intrinsics, they provide a cheap and
useful API without actually containing data, or a provider specific
implementation.
