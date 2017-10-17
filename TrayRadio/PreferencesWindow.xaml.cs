/*
 * Copyright(C) 2017 Michal Heczko
 * All rights reserved.
 *
 * This software may be modified and distributed under the terms
 * of the BSD license.  See the LICENSE file for details.
 */

using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;

namespace TrayRadio
{
	/// <summary>
	/// Interaction logic for PreferencesWindow.xaml
	/// </summary>
	public partial class PreferencesWindow : Window
	{
		#region Fields

		public static readonly DependencyProperty BalanceSliderValueProperty = DependencyProperty.Register("BalanceSliderValue", typeof(string), typeof(PreferencesWindow), new PropertyMetadata(null));
		public static readonly RoutedCommand CommandAddRadio = new RoutedCommand();
		public static readonly RoutedCommand CommandClearRadios = new RoutedCommand();
		public static readonly RoutedCommand CommandExportRadios = new RoutedCommand();
		public static readonly RoutedCommand CommandImportRadios = new RoutedCommand();
		public static readonly RoutedCommand CommandRemoveRadio = new RoutedCommand();
		public static readonly RoutedCommand CommandSaveRadio = new RoutedCommand();
		public static readonly DependencyProperty SelectedRadioProperty = DependencyProperty.Register("SelectedRadio", typeof(RadioEntry), typeof(PreferencesWindow), new PropertyMetadata(null));
		public static readonly DependencyProperty VolumeProperty = DependencyProperty.Register("Volume", typeof(float), typeof(PreferencesWindow), new PropertyMetadata(default(float)));
		public static readonly DependencyProperty VolumeSliderValueProperty = DependencyProperty.Register("VolumeSliderValue", typeof(string), typeof(PreferencesWindow), new PropertyMetadata(null));

		#endregion

		#region Properties

		public string BalanceSliderValue
		{
			get { return (string)GetValue(BalanceSliderValueProperty); }
			set { SetValue(BalanceSliderValueProperty, value); }
		}

		public RadioEntry SelectedRadio
		{
			get { return (RadioEntry)GetValue(SelectedRadioProperty); }
			set { SetValue(SelectedRadioProperty, value); }
		}

		public string VolumeSliderValue
		{
			get { return (string)GetValue(VolumeSliderValueProperty); }
			set { SetValue(VolumeSliderValueProperty, value); }
		}

		#endregion

		#region Methods

		private void CheckBox_Autoplay_Click(object sender, RoutedEventArgs e)
		{
			if ((sender as CheckBox).IsChecked.Value)
				if (string.IsNullOrEmpty(TrayRadio.Properties.Settings.Default.AutoplayRadioName))
					TrayRadio.Properties.Settings.Default.AutoplayRadioName = TrayRadio.Properties.Settings.Default.Radios[0].Name;
		}

		private void CommandAddRadio_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = !string.IsNullOrEmpty(txbRadioName.Text) && !string.IsNullOrEmpty(txbRadioUrl.Text);
			e.CanExecute &= SelectedRadio?.Name.CompareTo(txbRadioName.Text) != 0 || SelectedRadio?.Url.CompareTo(txbRadioUrl.Text) != 0;
		}

		private void CommandAddRadio_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			TrayRadio.Properties.Settings.Default.Radios.Add(new RadioEntry(txbRadioName.Text, txbRadioUrl.Text));
		}

		private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			while(TrayRadio.Properties.Settings.Default.Radios.Count > 0)
				TrayRadio.Properties.Settings.Default.Radios.RemoveAt(0);
		}

		private void CommandExportRadios_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = TrayRadio.Properties.Settings.Default.Radios.Count > 0;
		}

		private void CommandExportRadios_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			SaveFileDialog dlgSaveFile = new SaveFileDialog();
			dlgSaveFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			dlgSaveFile.Filter = Properties.Resources.OpenSaveDialogFilter;
			if (dlgSaveFile.ShowDialog().Value)
			{
				using (StreamWriter writer = new StreamWriter(dlgSaveFile.FileName))
				{
					XmlSerializer serializer = new XmlSerializer(TrayRadio.Properties.Settings.Default.Radios.GetType());
					serializer.Serialize(writer, TrayRadio.Properties.Settings.Default.Radios);
				}
			}
		}

		private void CommandImportRadios_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			OpenFileDialog dlgOpenFile = new OpenFileDialog();
			dlgOpenFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			dlgOpenFile.CheckFileExists = true;
			dlgOpenFile.Filter = Properties.Resources.OpenSaveDialogFilter;
			if (dlgOpenFile.ShowDialog().Value)
			{
				using (StreamReader reader = new StreamReader(dlgOpenFile.FileName))
				{
					XmlSerializer serializer = new XmlSerializer(TrayRadio.Properties.Settings.Default.Radios.GetType());
					RadioCollection collection = (RadioCollection)serializer.Deserialize(reader);
					TrayRadio.Properties.Settings.Default.Radios.Clear();
					foreach (RadioEntry radio in collection)
						TrayRadio.Properties.Settings.Default.Radios.Add(radio); 
				}
			}
		}

		private void CommandRemoveRadio_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = SelectedRadio != null;
		}

		private void CommandRemoveRadio_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			TrayRadio.Properties.Settings.Default.Radios.Remove(SelectedRadio);
		}

		private void CommandSaveRadio_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = SelectedRadio != null && (SelectedRadio?.Name.CompareTo(txbRadioName.Text) != 0 || SelectedRadio?.Url.CompareTo(txbRadioUrl.Text) != 0);
		}

		private void CommandSaveRadio_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			SelectedRadio.Name = txbRadioName.Text;
			SelectedRadio.Url = txbRadioUrl.Text;
			lsvRadios.Items.Refresh();
		}

		private void lsvRadios_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Delete)
				TrayRadio.Properties.Settings.Default.Radios.Remove((sender as ListView).SelectedItem as RadioEntry);
		}

		private void lsvRadios_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			ListView listView = sender as ListView;
			GridView gridView = listView.View as GridView;
			int columnsCount = gridView.Columns.Count;
			Double gridViewWidth = Double.IsNaN(listView.Width) ? listView.ActualWidth : listView.Width;
			Visibility visibile = (Visibility)listView.GetValue(ScrollViewer.ComputedVerticalScrollBarVisibilityProperty);
			if (visibile == Visibility.Visible)
			{
				gridViewWidth -= ((Double)SystemParameters.VerticalScrollBarWidth) * 1.7;
			}
			double lastColumnWidth = gridViewWidth;
			for (int index = 0; index < gridView.Columns.Count; index++)
			{
				lastColumnWidth -= gridView.Columns[index].Width;
			}
		}

		private void sldBalance_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			Slider_MouseWheel(sldBalance, e);
		}

		private void sldBalance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			BalanceSliderValue = string.Format("{0}", Math.Truncate(sldBalance.Value));
		}

		private void sldVolume_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			Slider_MouseWheel(sldVolume, e);
		}

		private void sldVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			VolumeSliderValue = string.Format("Volume {0}%", Math.Truncate(sldVolume.Value));
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
				
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			this.DisableButtons(Extensions.WindowButtons.Maximize | Extensions.WindowButtons.Minimize);
		}

		#endregion

		#region Constructor

		public PreferencesWindow()
		{
			DataContext = this;
			InitializeComponent();
			BalanceSliderValue = string.Format("{0}", Math.Truncate(sldBalance.Value));
			VolumeSliderValue = string.Format("{0}%", Math.Truncate(sldVolume.Value));
		}

		#endregion
	}
}
