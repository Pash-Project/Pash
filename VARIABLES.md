Pash Variables
==============
This file documents special Pash variables.

## PASHForceSynchronizeProcessOutput
This variable control process execution. Possible values:

* `$false` *(default)* - when starting a new process via _call operator `&`_ or directly, application
  executable file will be checked. If an application uses an OS console subsystem, Pash execution
  will be blocked until process finished. If an application don't use console subsystem (i.e. uses GUI
  subsystem), process will be started in background.
* `$true` - when starting a new process via _call operator `&`_ or directly, Pash execution will be
  blocked until process finished.

_Note:_ This flag effectively has no effect under Unix-like OS (Linux, Mac), because any applications
under Unix counts as console.