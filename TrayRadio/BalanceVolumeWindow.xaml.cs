/*
 * Copyright(C) 2017 Michal Heczko
 * All rights reserved.
 *
 * This software may be modified and distributed under the terms
 * of the BSD license.  See the LICENSE file for details.
 */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TrayRadio
{
	/// <summary>
	/// Interaction logic for BalanceVolumeWindow.xaml
	/// </summary>
	public partial class BalanceVolumeWindow : Window
	{
		#region Fields

		public static readonly DependencyProperty BalanceSliderToolTipProperty = DependencyProperty.Register("BalanceSliderToolTip", typeof(string), typeof(BalanceVolumeWindow), new PropertyMetadata(null));
		public static readonly DependencyProperty VolumeSliderToolTipProperty = DependencyProperty.Register("VolumeSliderToolTip", typeof(string), typeof(BalanceVolumeWindow), new PropertyMetadata(null));

		#endregion

		#region Properties

		public string BalanceSliderToolTip
		{
			get { return (string)GetValue(BalanceSliderToolTipProperty); }
			set { SetValue(BalanceSliderToolTipProperty, value); }
		}

		public string VolumeSliderToolTip
		{
			get { return (string)GetValue(VolumeSliderToolTipProperty); }
			set { SetValue(VolumeSliderToolTipProperty, value); }
		}

		#endregion

		#region Methods

		public new void Focus()
		{
			System.Drawing.Point location = System.Windows.Forms.Cursor.Position;
			Left = location.X - (Width / 2);
			Top = location.Y - Height - 20;
			base.Focus();
		}

		public new void Show()
		{
			System.Drawing.Point location = System.Windows.Forms.Cursor.Position;
			Left = location.X - (Width / 2);
			Top = location.Y - Height - 20;
			base.Show();
		}

		private void sldBalance_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			Slider_MouseWheel(sldBalance, e);
		}

		private void sldBalance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			BalanceSliderToolTip = string.Format("Balance {0}", Math.Truncate(sldBalance.Value));
		}

		private void sldVolume_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			Slider_MouseWheel(sldVolume, e);
		}

		private void sldVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			VolumeSliderToolTip = string.Format("Volume {0}%", Math.Truncate(sldVolume.Value));
		}

		private void Slider_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			Slider slider = sender as Slider;
			if (slider != null)
			{
				if (e.Delta < 0)
				{
					if (slider.Value > slider.Minimum)
						slider.Value -= 1;
				}
				else
				{
					if (slider.Value < slider.Maximum)
						slider.Value += 1;
				}
			}
		}

		private void Window_Deactivated(object sender, EventArgs e)
		{
			Close();
		}
		
		#endregion

		#region Constructor

		public BalanceVolumeWindow()
		{
			DataContext = this;
			InitializeComponent();
			BalanceSliderToolTip = string.Format("Balance {0}", Math.Truncate(sldBalance.Value));
			VolumeSliderToolTip = string.Format("Volume {0}%", Math.Truncate(sldVolume.Value));
		}

		#endregion
	}
}
