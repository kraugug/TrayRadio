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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

		private void Label_MouseEnter(object sender, MouseEventArgs e)
		{
			Label label = sender as Label;
			label.FontWeight = FontWeights.SemiBold;
			Cursor = Cursors.Hand;
		}

		private void Label_MouseLeave(object sender, MouseEventArgs e)
		{
			Label label = sender as Label;
			label.FontWeight = FontWeights.Normal;
			Cursor = Cursors.Arrow;
		}

		private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Process.Start(Properties.Resources.String_TrayRadioURL);
			Close();
		}

        private void Window_LostFocus(object sender, RoutedEventArgs e)
        {
			Close();
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
