using System;

namespace TrayRadio.Updater
{
    public class CheckForUpdateEventArgs : EventArgs
    {
		#region Properties

		public UpdateInfo UpdateInfo { get; }

		public bool IsNewAvailable { get; }

		#endregion

		#region Constructor(s)

		public CheckForUpdateEventArgs(UpdateInfo updateInfo)
		{
			if (updateInfo == null)
				throw new ArgumentNullException(Properties.Resources.Param_UpdateInfo);
			UpdateInfo = updateInfo;
			IsNewAvailable = UpdateInfo.Version.CompareVersion(AboutWindow.Version, 3) > 0;
		}

		#endregion
	}
}
