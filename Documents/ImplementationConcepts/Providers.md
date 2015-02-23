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

Provider Core Classes
---------------------
If you want to implement a new provider, you need to derive from core
classes `NavigationCmdletProvider`, `ContainerCmdletProvider`, or others.
They don't contain much logic and should only serve as a base class to
derive from.  It would't make much sense to implement common logic here,
because some behavior, e.g. for `GetChildItems`, can be the same for e.g.
`ContainerCmdletProvider` and `NavigationCmdletProvider`, with some extra
logic for the latter one. So where should that be implemented? This would
get very nasty.

Provider Cmdlets
----------------
Starting with the provider cmdlets, you can see that they share some
common parameters like `Include`, `Exclude`, `Filter`, `Force`,
`Credential`, and maybe `Path` and `LiteralPath`. As you can already
imagine, it would be quite tedious to pass and check process all these
parameters in each of the cmdlets. Instead, they simply override a
protected base attribute in `CoreCommandBase` (and derivatives) and can
be accessed as part of the `ProviderRuntime` member.

Provider Intrinsics
-------------------
The gap between those generic cmdlets and the core functionality
implemented by the specific provider is closed by Pash.
<Just quickly mention what they do>

ProviderRuntime
---------------
The connecting piece between the cmdlets, the Intrinsics and the core
classes is the `ProviderRuntime`.
It serves as a general context of the operations and abstracts provider's
output mechanism, so it can either be captured or directly be forwarded
to a cmdlet's output pipe.

