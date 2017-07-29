/*
 * Copyright(C) 2017 Michal Heczko
 * All rights reserved.
 *
 * This software may be modified and distributed under the terms
 * of the BSD license.  See the LICENSE file for details.
 */

using System;
using System.Windows;

namespace TrayRadio
{
	/// <summary>
	/// Interaction logic for SongsHistoryWindow.xaml
	/// </summary>
	public partial class SongsHistoryWindow : Window
	{
		#region Methods

		public void Add(RadioEntry radio)
		{
			if (!string.IsNullOrWhiteSpace(radio.Info?.Title))
				txbHistory.AppendText(string.Format("[{0}]: {1} - {2}\n", DateTime.Now.ToString("HH:mm:ss"), radio.Name, radio.Info.Title));
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
			Hide();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			this.DisableButtons(Extensions.WindowButtons.Maximize | Extensions.WindowButtons.Minimize);
		}

		#endregion

		#region Constructor

		public SongsHistoryWindow()
		{
			DataContext = this;
			InitializeComponent();
		}

		#endregion
	}
}
