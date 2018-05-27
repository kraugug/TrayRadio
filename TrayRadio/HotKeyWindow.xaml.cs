using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TrayRadio
{
	/// <summary>
	/// Interaction logic for HotKeyWindow.xaml
	/// </summary>
	public partial class HotKeyWindow : Window
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

		#region Methos

		private void Window_Deactivated(object sender, EventArgs e)
		{
			Close();
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
				Close();
		}

		#endregion

		#region Constructor

		public HotKeyWindow()
		{
			DataContext = this;
			InitializeComponent();
		}

		#endregion
	}
}
