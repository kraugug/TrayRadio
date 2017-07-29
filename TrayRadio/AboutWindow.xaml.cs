/*
 * Copyright(C) 2017 Michal Heczko
 * All rights reserved.
 *
 * This software may be modified and distributed under the terms
 * of the BSD license.  See the LICENSE file for details.
 */

using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace TrayRadio
{
	/// <summary>
	/// Interaction logic for AboutWindow.xaml
	/// </summary>
	public partial class AboutWindow : Window
	{
		#region Fields

		public static readonly RoutedCommand CommandShowLicense = new RoutedCommand();

		#endregion

		#region Properties

		public string LicenseFile { get { return AppDomain.CurrentDomain.BaseDirectory + "\\License.txt"; } }

		public static string Version
		{
			get
			{
				string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
				return version.Substring(0, version.LastIndexOf('.'));
			}
		}

		#endregion

		#region Methods

		private void CommandShowLicense_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = System.IO.File.Exists(LicenseFile);
		}

		private void CommandShowLicense_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Process.Start(LicenseFile);
		}

		#endregion


		#region Constructor

		public AboutWindow()
		{
			InitializeComponent();
			
			lblNameAndVersion.Content = string.Format("Tray Radio {0}", Version);
		}

		#endregion
	}
}
