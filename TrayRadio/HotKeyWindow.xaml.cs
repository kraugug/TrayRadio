using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

		public static readonly RoutedCommand CommandPlay = new RoutedCommand();
		public static readonly RoutedCommand CommandPreferences = new RoutedCommand();
		public static readonly RoutedCommand CommandRecord = new RoutedCommand();
		public static readonly RoutedCommand CommandStop = new RoutedCommand();

		#endregion

		#region Methods

		private void CommandPlay_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = App.Instance.ActiveRadio != null && !App.Instance.ActiveRadio.IsActive;
		}

		private void CommandPlay_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			App.Instance.ActiveRadio.Play();
		}

		private void CommandPreferences_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			App.Instance?.ShowPreferences();
		}

		private void CommandRecord_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = App.Instance.ActiveRadio != null && App.Instance.ActiveRadio.IsActive;
		}

		private void CommandRecord_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if ((e.OriginalSource as ToggleButton).IsChecked.Value)
				App.Instance.ActiveRadio.StartRecording();
			else
				App.Instance.ActiveRadio.StopRecording();
		}

		private void CommandStop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = App.Instance.ActiveRadio != null && App.Instance.ActiveRadio.IsActive;
		}

		private void CommandStop_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			App.Instance.ActiveRadio.Stop();
		}

		private void Window_Activated(object sender, EventArgs e)
		{
			ComboBoxRadios.Focus();
		}

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
