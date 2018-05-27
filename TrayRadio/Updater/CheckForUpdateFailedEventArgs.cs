using System;

namespace TrayRadio.Updater
{
    public class CheckForUpdateFailedEventArgs : EventArgs
    {
		#region Properties

		public Exception Exception { get; }

		#endregion

		#region Constructor

		public CheckForUpdateFailedEventArgs(Exception exception)
		{
			Exception = exception;
		}

		#endregion
	}
}
