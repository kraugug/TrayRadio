/*
 * Copyright(C) 2017 Michal Heczko
 * All rights reserved.
 *
 * This software may be modified and distributed under the terms
 * of the BSD license.  See the LICENSE file for details.
 */

using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using TrayRadio.Updater;
using Un4seen.Bass;

namespace TrayRadio
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : System.Windows.Application
	{
		#region Fields

		public static readonly string DefaultRecordsFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Tray Radio", "Records");

		private static readonly Icon IconAntennaSignalRecording = Debugger.IsAttached ? Icon.FromHandle(TrayRadio.Properties.Resources.Debug_Antenna_Recording.GetHicon()) : Icon.FromHandle(TrayRadio.Properties.Resources.Antenna_Signal_Recording.GetHicon());
		private static readonly Icon IconAntennaSignal = Debugger.IsAttached ? Icon.FromHandle(TrayRadio.Properties.Resources.Debug_Antenna_Signal.GetHicon()) : Icon.FromHandle(TrayRadio.Properties.Resources.Antenna_Signal.GetHicon());
		private static readonly Icon IconAntennaSignalStalled = Debugger.IsAttached ? Icon.FromHandle(TrayRadio.Properties.Resources.Debug_Antenna_Signal_Stalled.GetHicon()) : Icon.FromHandle(TrayRadio.Properties.Resources.Antenna_Signal_Stalled.GetHicon());
		private static readonly Icon IconAntennaNoSignal = Debugger.IsAttached ? Icon.FromHandle(TrayRadio.Properties.Resources.Debug_Antenna_No_Signal.GetHicon()) : Icon.FromHandle(TrayRadio.Properties.Resources.Antenna_No_Signal.GetHicon());
		private static readonly Icon IconPreferences = Icon.FromHandle(TrayRadio.Properties.Resources.Preferences.GetHicon());

		private BalanceVolumeWindow m_BalanceVolumeWnd;
		private ToolStripItem m_CheckForUpdate;
		private CancellationTokenSource m_Cts = new CancellationTokenSource();
		private bool m_IsInitialised;
		private KeyboardHook m_KeyboardHook;
		private Mutex m_Mutex = null;
		private PreferencesWindow m_PreferencesWnd;
		private ToolStripItem m_RecordingStart;
		private ToolStripItem m_RecordingStop;
		private BASSActive m_RadioStatus = BASSActive.BASS_ACTIVE_STOPPED;
		private SongsHistoryWindow m_SongsHistoryWnd;
		private NotifyIcon m_TrayIcon;
		private Updater.Updater m_Updater;

		#endregion

		#region Properties

		public RadioEntry ActiveRadio { get; set; }

		public static App Instance { get; private set; }
		
		#endregion

		#region Methods
		
		protected ContextMenuStrip CreateRadioMenuStrip()
		{
			ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
			contextMenuStrip.Opening += (object sender, CancelEventArgs args) =>
			{
				m_RecordingStart.Enabled = ActiveRadio != null ? !ActiveRadio.IsRecording && ActiveRadio.IsActive : false;
				m_RecordingStop.Enabled = ActiveRadio != null ? ActiveRadio.IsRecording : false;
			};
			ToolStripItem item = contextMenuStrip.Items.Add("Stop Playing");
			item.Enabled = false;
			item.Click +=  (object sender, EventArgs args) => { ActiveRadio?.Stop(); };
			contextMenuStrip.Items.Add("-");
			
			foreach (RadioEntry radio in TrayRadio.Properties.Settings.Default.Radios)
			{
				radio.OnBeforePlay += RadioBeforePlay;
				radio.OnAfterPlay += RadioAfterPlay;
				radio.OnAfterStop += RadioAfterStop;
				radio.OnAfterStartRecording += RadioAfterStartRecording;
				radio.OnAfterStopRecording += RadioAfterStopRecording;
				radio.PropertyChanged += (object sender, PropertyChangedEventArgs args) =>
				{
					Dispatcher.BeginInvoke(new Action(() => { m_RecordingStart.Enabled = !(m_RecordingStop.Enabled = ActiveRadio.IsRecording); }));
				};
				contextMenuStrip.Items.Add(radio.MenuStripItem);
			}
			if (TrayRadio.Properties.Settings.Default.Radios.Count > 0)
				contextMenuStrip.Items.Add("-");
			item = contextMenuStrip.Items.Add("Songs History");
			item.Click += (object sender, EventArgs args) =>
			{
				if (m_SongsHistoryWnd.IsVisible)
					m_SongsHistoryWnd.Focus();
				else
					m_SongsHistoryWnd.Show();
			};
			
			contextMenuStrip.Items.Add("-");
			m_RecordingStart = contextMenuStrip.Items.Add("Start Recording");
			m_RecordingStart.Click += (object sender, EventArgs args) =>
			{
				ActiveRadio?.StartRecording();
				//m_TrayIcon.Icon = ActiveRadio.IsRecording ? IconAntennaSignalRecording : IconAntennaSignal;
			};
			m_RecordingStop = contextMenuStrip.Items.Add("Stop Recording");
			m_RecordingStop.Click += (object sender, EventArgs args) =>
			{
				if (ActiveRadio != null)
				{
					ActiveRadio.MenuStripItem.Font = new Font(ActiveRadio.MenuStripItem.Font, System.Drawing.FontStyle.Regular);
					ActiveRadio.StopRecording();
					//m_TrayIcon.Icon = ActiveRadio.IsRecording ? IconAntennaSignalRecording : IconAntennaSignal;
				}
			};
			m_RecordingStop.Enabled = false;				
			contextMenuStrip.Items.Add("-");
			m_CheckForUpdate = contextMenuStrip.Items.Add("Check for updates");
			m_CheckForUpdate.Click += (object sender, EventArgs args) =>
			{
				m_CheckForUpdate.Enabled = false;
				m_Updater.CheckForUpdate(sender);
			};
			item = contextMenuStrip.Items.Add("Preferences");
			item.Image = IconPreferences.ToBitmap();
			item.Click += (object sender, EventArgs args) =>    
			{
				ShowPreferences();
			};
			contextMenuStrip.Items.Add("-");
			item = contextMenuStrip.Items.Add("About");
			item.Click += (object sender, EventArgs args) =>
			{
				(new AboutWindow()).ShowDialog();
			};
			contextMenuStrip.Items.Add("-");
			item = contextMenuStrip.Items.Add("Exit");
			item.Click += (object sender, EventArgs args) =>
			{
				if (ActiveRadio != null)
					ActiveRadio.Stop();
				Shutdown(0);
			};
			return contextMenuStrip;
		}

		protected override void OnExit(ExitEventArgs e)
		{
			if (m_IsInitialised)
			{
				ActiveRadio?.Stop();
				Bass.BASS_Free();
				m_Cts.Cancel();
				m_TrayIcon.Dispose();
				if (!Debugger.IsAttached)
					m_Mutex.ReleaseMutex();
			}
			TrayRadio.Properties.Settings.Default.Save();
			base.OnExit(e);
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			m_Mutex = new Mutex(false, Current.ToString());
			if (m_Mutex.WaitOne(500, false) || Debugger.IsAttached)
			{
				if ((m_IsInitialised = Bass.BASS_Init(TrayRadio.Properties.Settings.Default.BassPlayDevice, TrayRadio.Properties.Settings.Default.BassPlayFrequency,
					BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero)))
				{
					m_TrayIcon = new NotifyIcon();
					m_TrayIcon.BalloonTipClicked += (object sender, EventArgs args) =>
					{
						NotifyIcon ni = sender as NotifyIcon;
						if ((ni != null) && (ni.BalloonTipText.StartsWith("New version of")))
							if (System.Windows.MessageBox.Show("Do you wish to download a new version?", "Tray Radio Updater", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
								System.Diagnostics.Process.Start(m_Updater.UpdateInfo.UpdateLink);
					};
					m_TrayIcon.MouseClick += (object sender, System.Windows.Forms.MouseEventArgs args) =>
					{
						if (args.Button == MouseButtons.Left)
						{
							if (m_BalanceVolumeWnd == null)
							{
								m_BalanceVolumeWnd = new BalanceVolumeWindow();
								m_BalanceVolumeWnd.Closed += (object sender2, EventArgs args2) => { m_BalanceVolumeWnd = null; };
								m_BalanceVolumeWnd.Show();
							}
							else if (m_BalanceVolumeWnd.IsVisible)
								m_BalanceVolumeWnd.Hide();
							else
								m_BalanceVolumeWnd.Show();
						}
					};
					m_TrayIcon.Icon = IconAntennaNoSignal;
					m_TrayIcon.Text = TrayRadio.Properties.Resources.TrayRadio;
					m_TrayIcon.Visible = true;
					m_TrayIcon.ContextMenuStrip = CreateRadioMenuStrip();
					Task.Factory.StartNew(() =>
					{
						do
						{
							if (ActiveRadio != null)
							{
								BASSActive status = ActiveRadio.Status;
								switch (status)
								{
									case BASSActive.BASS_ACTIVE_PLAYING:
										if (ActiveRadio.IsNewSong || (status != m_RadioStatus))
										{
											Dispatcher.Invoke(new Action(() =>
											{
												m_TrayIcon.ContextMenuStrip.Items[0].Enabled = true;
												ActiveRadio.MenuStripItem.Font = new Font(ActiveRadio.MenuStripItem.Font, System.Drawing.FontStyle.Bold);
											}));
											string text = string.Format("{0} - {1}", TrayRadio.Properties.Resources.TrayRadio, ActiveRadio.Name);
											m_TrayIcon.Text = text.Length >= 60 ? text.Substring(0, 60) + "..." : text;
											m_TrayIcon.Icon = ActiveRadio.IsRecording ? IconAntennaSignalRecording : IconAntennaSignal;
											bool wasRecorrding = ActiveRadio.IsRecording;
											if (wasRecorrding)
												ActiveRadio.StopRecording();
											App.Current.Dispatcher.Invoke(new Action(() => { m_SongsHistoryWnd.Add(ActiveRadio); }));											
											ShowBallonTip(string.Format("{0} - Playing\n\n{1}", ActiveRadio.Name, ActiveRadio.Info.Title), ToolTipIcon.Info);
											if (wasRecorrding)
												ActiveRadio.StartRecording();
										}
										break;
									case BASSActive.BASS_ACTIVE_STALLED:
										if (status != m_RadioStatus)
										{
                                            m_TrayIcon.Icon = IconAntennaSignalStalled;
											ShowBallonTip(string.Format("{0} - Stalled", ActiveRadio.Name), ToolTipIcon.Warning);
										}
										break;
									default:
                                        if (m_RadioStatus != status)
                                        {
                                            Dispatcher.Invoke(new Action(() =>
											{
												ActiveRadio.MenuStripItem.Font = new Font(ActiveRadio.MenuStripItem.Font, System.Drawing.FontStyle.Regular);
												m_TrayIcon.ContextMenuStrip.Items[0].Enabled = false;
											}));
                                            m_TrayIcon.Icon = IconAntennaNoSignal;
                                            m_TrayIcon.Text = TrayRadio.Properties.Resources.TrayRadio;
                                            if (status != m_RadioStatus)
                                                ShowBallonTip(string.Format("{0} - Stopped", ActiveRadio.Name), ToolTipIcon.Info);
                                        }
										break; 
								}
								m_RadioStatus = status;
							}
							Thread.Sleep(100);
						} while (!m_Cts.IsCancellationRequested);
					});
					// Songs history...
					m_SongsHistoryWnd = new SongsHistoryWindow();
					// Autoplay...
					if (TrayRadio.Properties.Settings.Default.AutoplayRadio && !string.IsNullOrEmpty(TrayRadio.Properties.Settings.Default.AutoplayRadioName))
						(ActiveRadio = TrayRadio.Properties.Settings.Default.Radios[TrayRadio.Properties.Settings.Default.AutoplayRadioName]).Play();
					// Updater...
					m_Updater = new Updater.Updater();
					m_Updater.UpdateInterval = TrayRadio.Properties.Settings.Default.UpdateCheckInterval;
					m_Updater.OnCheckForUpdate += (object sender, CheckForUpdateEventArgs args) =>
					{
						if (args.IsNewAvailable)
							ShowBallonTip(string.Format("New version of {0} {1} is available.", m_Updater.UpdateInfo.Name, m_Updater.UpdateInfo.Version), ToolTipIcon.Info);
						else if (sender is ToolStripItem)
							System.Windows.MessageBox.Show("Your Tray Radio is up to date.", "Tray Radio Updater", MessageBoxButton.OK, MessageBoxImage.Information);
						Dispatcher.BeginInvoke(new Action(() => { m_CheckForUpdate.Enabled = true; }));
					};
					m_Updater.OnCheckForUpdateFailed += (object sender, CheckForUpdateFailedEventArgs args) =>
					{
						App.Instance.ShowBallonTip("Can not contact update server.", System.Windows.Forms.ToolTipIcon.Warning);
						Dispatcher.BeginInvoke(new Action(() => { m_CheckForUpdate.Enabled = true; }));
					};
					m_Updater.Enable = TrayRadio.Properties.Settings.Default.EnableUpdates;
					// Check for new update now...
					m_CheckForUpdate.Enabled = false;
					m_Updater.CheckForUpdate(this);
					// Create an keyboard hook...
					m_KeyboardHook = new KeyboardHook();
					m_KeyboardHook.KeyPressed += (object sender, KeyPressedEventArgs e2) =>
					{
						(new HotKeyWindow()).ShowDialog();
					};
					m_KeyboardHook.RegisterHotKey(ModifierKeys.Alt | ModifierKeys.Control | ModifierKeys.Shift, Keys.T); // Default hotkey.
					base.OnStartup(e);
				}
				else
					Shutdown(-1);
			}
			else
			{
				System.Windows.MessageBox.Show("Tray Radio already running.", TrayRadio.Properties.Resources.TrayRadio, MessageBoxButton.OK, MessageBoxImage.Warning);
				Shutdown(0);
			}
		}
		
		private void RadioAfterPlay(object sender, EventArgs args)
		{
			ActiveRadio = sender as RadioEntry;
			ActiveRadio.Balance = TrayRadio.Properties.Settings.Default.Balance;
			ActiveRadio.Volume = TrayRadio.Properties.Settings.Default.Volume;
			ActiveRadio.Mute = TrayRadio.Properties.Settings.Default.Mute;
			ActiveRadio.MenuStripItem.Font = new Font(ActiveRadio.MenuStripItem.Font, System.Drawing.FontStyle.Bold);
		}

		private void RadioAfterStartRecording(object sender, EventArgs e)
		{
			m_TrayIcon.Icon = IconAntennaSignalRecording;
			m_RecordingStart.Enabled = false;
			m_RecordingStop.Enabled = true;
		}

		private void RadioAfterStop(object sender, EventArgs e)
		{
			ActiveRadio.MenuStripItem.Font = new Font(ActiveRadio.MenuStripItem.Font, System.Drawing.FontStyle.Regular);
		}

		private void RadioAfterStopRecording(object sender, EventArgs e)
		{
			m_TrayIcon.Icon = ActiveRadio.IsActive ? IconAntennaSignal : IconAntennaNoSignal;
			m_RecordingStart.Enabled = true;
			m_RecordingStop.Enabled = false;
		}

		private void RadioBeforePlay(object sender, EventArgs args)
		{
			if (ActiveRadio != null)
				ActiveRadio.Stop();
		}

		public void ShowBallonTip(string message, ToolTipIcon icon = ToolTipIcon.Error)
		{
			m_TrayIcon.BalloonTipIcon = icon;
			m_TrayIcon.BalloonTipText = message;
			m_TrayIcon.BalloonTipTitle = TrayRadio.Properties.Resources.TrayRadio;
			m_TrayIcon.ShowBalloonTip(100);
		}

		public void ShowPreferences()
		{
			if (m_PreferencesWnd == null)
			{
				m_PreferencesWnd = new PreferencesWindow();
				m_PreferencesWnd.Closed += (object sender2, EventArgs args2) => { m_PreferencesWnd = null; };
				m_PreferencesWnd.ShowDialog();
			}
			else
				m_PreferencesWnd.Focus();
		}
		
		#endregion

		#region Constructor

		public App()
		{
			Instance = this;
			if (TrayRadio.Properties.Settings.Default.Radios == null)
			{
				TrayRadio.Properties.Settings.Default.Radios = new RadioCollection();
				TrayRadio.Properties.Settings.Default.Radios.Add(new RadioEntry("All Irish Radio", "http://192.99.62.212:20142/stream"));
				TrayRadio.Properties.Settings.Default.Radios.Add(new RadioEntry("Impuls Ráááádio", "http://icecast1.play.cz/impuls128.mp3"));
				TrayRadio.Properties.Settings.Default.Radios.Add(new RadioEntry("Irish Radio Canada", "http://167.114.157.212:8068/stream"));
				TrayRadio.Properties.Settings.Default.Radios.Add(new RadioEntry("METAL ONLY - www.metal-only.de - 24h Black Death Heavy", "http://92.222.49.186:9480"));
				TrayRadio.Properties.Settings.Default.Radios.Add(new RadioEntry("MetalRock06", "http://listen.radionomy.com/MetalRock06"));
				TrayRadio.Properties.Settings.Default.Radios.Add(new RadioEntry("PulseRadio - Trance", "http://178.32.98.117:80/pulstranceHD.mp3"));
				TrayRadio.Properties.Settings.Default.Radios.Add(new RadioEntry("Radio BEAT", "http://icecast2.play.cz/radiobeat128.mp3"));
				TrayRadio.Properties.Settings.Default.Radios.Add(new RadioEntry("Jack FM", "http://playerservices.streamtheworld.com/api/livestream-redirect/JACK_FM.mp3"));
				TrayRadio.Properties.Settings.Default.Radios.Add(new RadioEntry("Electro Swing Radio", "http://streamer.radio.co/s2c3cc784b/listen"));
				TrayRadio.Properties.Settings.Default.RecordsFolder = DefaultRecordsFolder;
			}
			TrayRadio.Properties.Settings.Default.Radios.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
			{
				switch (e.Action)
				{
					case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
						foreach (RadioEntry item in e.NewItems)
							m_TrayIcon.ContextMenuStrip.Items.Insert(TrayRadio.Properties.Settings.Default.Radios.Count, item.MenuStripItem);
						if (!(m_TrayIcon.ContextMenuStrip.Items[1] is ToolStripSeparator/*.Text.CompareTo("-") != 0*/))
							m_TrayIcon.ContextMenuStrip.Items.Insert(1, new ToolStripSeparator());
						break;
					case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
						RadioEntry radio = e.OldItems[0] as RadioEntry;
						m_TrayIcon.ContextMenuStrip.Items.Remove(radio.MenuStripItem);
						if (ActiveRadio?.Name.CompareTo(radio.Name) == 0)
							if (ActiveRadio.IsActive)
							{
								ActiveRadio.Stop();
								ActiveRadio = null;
							}
						if (TrayRadio.Properties.Settings.Default.Radios.Count == 0)
							m_TrayIcon.ContextMenuStrip.Items.RemoveAt(1);
						break;
				}
			};
			TrayRadio.Properties.Settings.Default.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
			{
				switch (e.PropertyName)
				{
					case "Autostart":
						Uri uriAppFile = new Uri(Assembly.GetExecutingAssembly().CodeBase);
						RegistryKey runKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run\", true);
						if (TrayRadio.Properties.Settings.Default.Autostart)
							runKey.SetValue("Tray Radio", string.Format("\"{0}\"", uriAppFile.LocalPath), RegistryValueKind.String);
						else
							runKey.DeleteValue("Tray Radio", false);
						runKey.Flush();
						runKey.Close();
						break;
					case "Balance":
						if (ActiveRadio != null)
							ActiveRadio.Balance = TrayRadio.Properties.Settings.Default.Balance;
						break;
					case "EnableUpdates":
						m_Updater.Enable = TrayRadio.Properties.Settings.Default.EnableUpdates;
						break;
					case "Mute":
						if (ActiveRadio != null)
							ActiveRadio.Mute = TrayRadio.Properties.Settings.Default.Mute;
						break;
					case "UpdateCheckInterval":
						m_Updater.UpdateInterval = TrayRadio.Properties.Settings.Default.UpdateCheckInterval;
						break;
					case "Volume":
						if (ActiveRadio != null)
							ActiveRadio.Volume = TrayRadio.Properties.Settings.Default.Volume;
						break;
				}
			};
		}
		
		#endregion
	}
}
