using System;

namespace IntelOrca.Launchpad
{
	public class LaunchpadException : Exception
	{
		public LaunchpadException() : base() { }
		public LaunchpadException(string message) : base(message) { }
	}
}
