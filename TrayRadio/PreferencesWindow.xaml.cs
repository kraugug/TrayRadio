/*
 * Copyright(C) 2017 Michal Heczko
 * All rights reserved.
 *
 * This software may be modified and distributed under the terms
 * of the BSD license.  See the LICENSE file for details.
 */

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
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
		public static readonly RoutedCommand CommandBrowseRecordsFolder = new RoutedCommand();
		public static readonly RoutedCommand CommandClearRadios = new RoutedCommand();
        public static readonly RoutedCommand CommandDeleteRecording = new RoutedCommand();
        public static readonly RoutedCommand CommandExportRadios = new RoutedCommand();
		public static readonly RoutedCommand CommandImportRadios = new RoutedCommand();
        public static readonly RoutedCommand CommandOpenRecordingInFolder = new RoutedCommand();
		public static readonly RoutedCommand CommandOpenRecordsFolder = new RoutedCommand();
		public static readonly RoutedCommand CommandPlayRecording = new RoutedCommand();
        public static readonly RoutedCommand CommandRemoveRadio = new RoutedCommand();
		public static readonly RoutedCommand CommandResetRecordsFolder = new RoutedCommand();
		public static readonly RoutedCommand CommandSaveRadio = new RoutedCommand();
        public static readonly DependencyProperty RecordingsProperty = DependencyProperty.Register("Recordings", typeof(ObservableCollection<RecordingInfo>), typeof(PreferencesWindow), new PropertyMetadata(null));
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

        public static PreferencesWindow Instance { get; private set; }

        public ObservableCollection<RecordingInfo> Recordings
        {
            get { return (ObservableCollection<RecordingInfo>)GetValue(RecordingsProperty); }
            set { SetValue(RecordingsProperty, value); }
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

		private void CommandClearRadios_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			while(TrayRadio.Properties.Settings.Default.Radios.Count > 0)
				TrayRadio.Properties.Settings.Default.Radios.RemoveAt(0);
		}

		private void CommandBrowseRecordsFolder_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			System.Windows.Forms.FolderBrowserDialog dlgBrowseFolder = new System.Windows.Forms.FolderBrowserDialog();
			dlgBrowseFolder.SelectedPath = Properties.Settings.Default.RecordsFolder;
			if (dlgBrowseFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				Properties.Settings.Default.RecordsFolder = dlgBrowseFolder.SelectedPath;
				RefreshRecordingsList();
			}
		}

        private void CommandDeleteRecording_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string fileName = e.Parameter as string;
            if (Recordings.Where(i => i.IsActive && i.FullPath.CompareTo(fileName) == 0).Count() > 0)
                if (MessageBox.Show("Do you want to stop and delete recording?", "Recording in progress", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    App.Instance.ActiveRadio.StopRecording();
                else
                    return;
            File.Delete(fileName);
            RefreshRecordingsList();
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
						Properties.Settings.Default.Radios.Add(radio);
					App.Instance.UpdateMenuStrip();
                }
			}
		}

        private void CommandsReconding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter != null && File.Exists(e.Parameter as string);
        }

        private void CommandOpenRecordingInFolder_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Process.Start("explorer.exe", string.Format("/select,\"{0}\"", e.Parameter as string));
        }

		private void CommandOpenRecordsFolder_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = Directory.Exists(Properties.Settings.Default.RecordsFolder);
		}

		private void CommandOpenRecordsFolder_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Process.Start("explorer.exe", string.Format("/select,\"{0}\"", Properties.Settings.Default.RecordsFolder));
		}

		private void CommandPlayRecording_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Process.Start(e.Parameter as string);
        }

        private void CommandRemoveRadio_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = SelectedRadio != null;
		}

		private void CommandRemoveRadio_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			TrayRadio.Properties.Settings.Default.Radios.Remove(SelectedRadio);
		}

		private void CommandResetRecordsFolder_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = Properties.Settings.Default.RecordsFolder.ToUpper().CompareTo(App.DefaultRecordsFolder.ToUpper()) != 0;
		}

		private void CommandResetRecordsFolder_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Properties.Settings.Default.RecordsFolder = App.DefaultRecordsFolder;
			RefreshRecordingsList();
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

        private void lsvRecordings_SizeChanged(object sender, SizeChangedEventArgs e)
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
            double firstColumnWidth = gridViewWidth;
            for (int index = gridView.Columns.Count - 1; index > 0; index--)
            {
                firstColumnWidth -= gridView.Columns[index].Width;
            }
            gridView.Columns[0].Width = firstColumnWidth;
        }

        internal void RefreshRecordingsList()
        {
            Recordings.Clear();
			Task.Factory.StartNew(() =>
			{
				List<RecordingInfo> recordings = new List<RecordingInfo>();
				SearchDirectory(new DirectoryInfo(Properties.Settings.Default.RecordsFolder), "*.MP3", recordings, 1);
				Dispatcher.BeginInvoke(new Action(() => { Recordings = new ObservableCollection<RecordingInfo>(recordings); }));
			});
		}

		internal void SearchDirectory(DirectoryInfo dirInfo, string searchPattern, List<RecordingInfo> files, int level = 0)
		{
			try
			{
				// Search for the directories...
				if (level > 0)
					foreach (DirectoryInfo subDirInfo in dirInfo.GetDirectories())
						try
						{
							SearchDirectory(subDirInfo, searchPattern, files, --level);
						}
						catch (UnauthorizedAccessException uaex)
						{
							Debug.WriteLine("[{0}] {1}: {2}", new object[] { level, uaex.GetType().FullName, uaex.Message });
						}
				// Search for the files...
				FileInfo[] filesInfo = dirInfo.GetFiles(searchPattern);
				foreach (FileInfo fileInfo in filesInfo)
				{
					try
					{
						AuthorizationRuleCollection arc = fileInfo.GetAccessControl().GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));
						bool denyAccess = false;
						foreach (FileSystemAccessRule fsar in arc)
							if (fsar.AccessControlType == AccessControlType.Deny && (fsar.FileSystemRights & FileSystemRights.ListDirectory) == FileSystemRights.ListDirectory)
							{
								denyAccess = true;
								break;
							}
						if (!denyAccess)
							files.Add(new RecordingInfo(fileInfo.FullName) { IsActive = App.Instance.ActiveRadio != null && App.Instance.ActiveRadio.IsRecording && fileInfo.FullName.CompareTo(App.Instance.ActiveRadio.ActiveRecordingFileName) == 0 });
					}
					catch (Exception ex)
					{
						Debug.WriteLine("[{0}] {1}: {2}", new object[] { level, ex.GetType().FullName, ex.Message });
					}
				}
			}
			catch (UnauthorizedAccessException uaex)
			{
				Debug.WriteLine("[{0}] {1}: {2}", new object[] { level, uaex.GetType().FullName, uaex.Message });
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

        private void Window_Closed(object sender, EventArgs e)
        {
            Instance = null;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			this.DisableButtons(Extensions.WindowButtons.Maximize | Extensions.WindowButtons.Minimize);
            RefreshRecordingsList();
			TextBoxRecordsFolder.Focus();
			TextBoxRecordsFolder.SelectAll();
		}

		#endregion

		#region Constructor

		public PreferencesWindow()
		{
            Recordings = new ObservableCollection<RecordingInfo>();
            DataContext = this;
			InitializeComponent();
			BalanceSliderValue = string.Format("{0}", Math.Truncate(sldBalance.Value));
			VolumeSliderValue = string.Format("{0}%", Math.Truncate(sldVolume.Value));
            Instance = this;
		}

		#endregion
	}
}
