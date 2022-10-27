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
using TrayRadio.Properties;

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
			App.Instance?.ShowPreferences(() => Close());
			
		}

		private void CommandRecord_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = App.Instance.ActiveRadio != null && App.Instance.ActiveRadio.IsActive;
		}

        private void CommandRecord_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (!(e.OriginalSource as ToggleButton).IsChecked.Value)
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
			//ComboBoxRadios.Focus();
		}

		private void Window_Deactivated(object sender, EventArgs e)
		{
			(sender as HotKeyWindow)?.Hide();
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
            switch (e.Key)
			{
				case Key.Escape:
					Close();
					break;
				case Key.M:
					Settings.Default.Mute = !Properties.Settings.Default.Mute;
                    break;
				case Key.O:
					CommandPreferences.Execute(null, null);
					break;
				case Key.P:
					CommandPlay.Execute(null, null);
                    break;
                case Key.R:
                    CommandRecord.Execute(null, ToogleButtonRecord);
                    break;
                case Key.S:
                    CommandStop.Execute(null, null);
                    break;
            }
			CommandManager.InvalidateRequerySuggested();
		}

        private void Window_LostFocus(object sender, RoutedEventArgs e)
        {
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
