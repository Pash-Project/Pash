using System;
using System.Management.Automation.Host;

namespace PashGui
{
	class Host : PSHost
	{
		public override void EnterNestedPrompt ()
		{
			throw new NotImplementedException ();
		}

		public override void ExitNestedPrompt ()
		{
			throw new NotImplementedException ();
		}

		public override void NotifyBeginApplication ()
		{
			throw new NotImplementedException ();
		}

		public override void NotifyEndApplication ()
		{
			throw new NotImplementedException ();
		}

		public override void SetShouldExit (int exitCode)
		{
			throw new NotImplementedException ();
		}

		public override System.Globalization.CultureInfo CurrentCulture {
			get {
				throw new NotImplementedException ();
			}
		}

		public override System.Globalization.CultureInfo CurrentUICulture {
			get {
				throw new NotImplementedException ();
			}
		}

		public override Guid InstanceId {
			get {
				throw new NotImplementedException ();
			}
		}

		public override string Name {
			get {
				throw new NotImplementedException ();
			}
		}

		public override PSHostUserInterface UI {
			get {
				throw new NotImplementedException ();
			}
		}

		public override Version Version {
			get {
				throw new NotImplementedException ();
			}
		}
	}
}