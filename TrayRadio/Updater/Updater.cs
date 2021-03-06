﻿/*
 * Copyright(C) 2017 Michal Heczko
 * All rights reserved.
 *
 * This software may be modified and distributed under the terms
 * of the BSD license.  See the LICENSE file for details.
 */

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Timers;	// Threading;
using System.Xml.Serialization;

namespace TrayRadio.Updater
{
	public class Updater
	{
		#region Fields

		private Timer _timer;

		#endregion

		#region Properties

		public bool Enable
		{
			get { return _timer.Enabled; }
			set { _timer.Enabled = value; }
		}

		public UpdateInfo UpdateInfo { get; internal set; }

		/// <summary>
		/// Update interval in minutes.
		/// </summary>
		public int UpdateInterval
		{
			get { return (int)Math.Truncate(_timer.Interval * 60000); }
			set
			{
				_timer.Enabled = false;
				_timer.Interval = value * 60000;
				_timer.Enabled = true;
			}
		}

		#endregion

		#region Methods

		public void CheckForUpdate(object iniciator)
		{
			Task.Factory.StartNew(() =>
			{
				var webRequest = WebRequest.Create(TrayRadio.Properties.Settings.Default.UpdateLink);
				try
				{
					using (var response = webRequest.GetResponse())
					using (var content = response.GetResponseStream())
					using (var reader = new StreamReader(content))
					{
						UpdateInfo = (UpdateInfo)(new XmlSerializer(typeof(UpdateInfo))).Deserialize(reader);
						if ((UpdateInfo != null)/* && (UpdateInfo.Version.CompareVersion(AboutWindow.Version) >= 0)*/)
							OnCheckForUpdate?.Invoke(iniciator, new CheckForUpdateEventArgs(UpdateInfo));
					}
				}
				catch(Exception ex)
				{
					OnCheckForUpdateFailed?.Invoke(iniciator, new CheckForUpdateFailedEventArgs(ex));
				}
			});
		}

		#endregion

		#region Constructor

		public Updater()
		{
			_timer = new Timer(TrayRadio.Properties.Settings.Default.UpdateCheckInterval);
			_timer.AutoReset = true;
			_timer.Elapsed += (object sender, ElapsedEventArgs e) => { CheckForUpdate(sender); };
		}

		#endregion

		#region

		public event EventHandler<CheckForUpdateFailedEventArgs> OnCheckForUpdateFailed;

		public event EventHandler<CheckForUpdateEventArgs> OnCheckForUpdate;

		#endregion
	}
}
