/*
 * Copyright(C) 2017 Michal Heczko
 * All rights reserved.
 *
 * This software may be modified and distributed under the terms
 * of the BSD license.  See the LICENSE file for details.
 */

using Microsoft.Win32;
using System;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Un4seen.Bass;

namespace TrayRadio
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : System.Windows.Application
	{
		#region Fields

		private static readonly Icon IconAntennaSignal = Icon.FromHandle(TrayRadio.Properties.Resources.Antenna_Signal.GetHicon());
		private static readonly Icon IconAntennaSignalStalled = Icon.FromHandle(TrayRadio.Properties.Resources.Antenna_Signal_Stalled.GetHicon());
		private static readonly Icon IconAntennaNoSignal = Icon.FromHandle(TrayRadio.Properties.Resources.Antenna_No_Signal.GetHicon());

		private BalanceVolumeWindow _balanceVolumeWnd;
		private CancellationTokenSource _cts = new CancellationTokenSource();
		private bool _isInitialised;
		private Mutex _mutex = null;
		private PreferencesWindow _preferencesWnd;
		private BASSActive _radioStatus = BASSActive.BASS_ACTIVE_STOPPED;
		private SongsHistoryWindow _songsHistoryWnd;
		private NotifyIcon _trayIcon;
		private Updater.Updater _updater;

		#endregion

		#region Properties

		public RadioEntry ActiveRadio { get; set; }

		public static App Instance { get; private set; }
		
		#endregion

		#region Methods

		protected void CreateRadioMenu()
		{
			_trayIcon.ContextMenu = new ContextMenu();
			_trayIcon.ContextMenu.MenuItems.Add(new MenuItem("Stop Playing", (object sender, EventArgs args) => { ActiveRadio?.Stop(); }));
			_trayIcon.ContextMenu.MenuItems[0].Enabled = false;
			_trayIcon.ContextMenu.MenuItems.Add(new MenuItem("-"));
			foreach (RadioEntry radio in TrayRadio.Properties.Settings.Default.Radios)
			{
				radio.OnBeforePlay += RadioBeforePlay;
				radio.OnAfterPlay += RadioAfterPlay;
				_trayIcon.ContextMenu.MenuItems.Add(radio.MenuItem);
			}
			if (TrayRadio.Properties.Settings.Default.Radios.Count > 0)
				_trayIcon.ContextMenu.MenuItems.Add(new MenuItem("-"));
			_trayIcon.ContextMenu.MenuItems.Add(new MenuItem("Songs History", (object sender, EventArgs args) =>
			{
				if (_songsHistoryWnd.IsVisible)
					_songsHistoryWnd.Focus();
				else
					_songsHistoryWnd.Show();
			}));
			_trayIcon.ContextMenu.MenuItems.Add(new MenuItem("-"));
			_trayIcon.ContextMenu.MenuItems.Add(new MenuItem("Check for updates", (object sender, EventArgs args) =>
			{
				if (_updater.CheckForUpdate())
					ShowBallonTip(string.Format("New version of {0} {1} is available.", _updater.UpdateInfo.Name, _updater.UpdateInfo.Version), ToolTipIcon.Info);
				else
					System.Windows.MessageBox.Show("Your Tray Radio is up to date.", "Tray Radio Updater", MessageBoxButton.OK, MessageBoxImage.Information);
			}));
			_trayIcon.ContextMenu.MenuItems.Add(new MenuItem("Preferences", (object sender, EventArgs args) =>
			{
				if (_preferencesWnd == null)
				{
					_preferencesWnd = new PreferencesWindow();
					_preferencesWnd.Closed += (object sender2, EventArgs args2) => { _preferencesWnd = null; };
					_preferencesWnd.ShowDialog();
				}
				else
					_preferencesWnd.Focus();
			}));
			_trayIcon.ContextMenu.MenuItems.Add(new MenuItem("-"));
			_trayIcon.ContextMenu.MenuItems.Add(new MenuItem("About", (object sender, EventArgs args) =>
			{
				(new AboutWindow()).ShowDialog();
			}));
			_trayIcon.ContextMenu.MenuItems.Add(new MenuItem("-"));
			_trayIcon.ContextMenu.MenuItems.Add(new MenuItem("Exit", (object sender, EventArgs args) => { Shutdown(0); }));
		}

		protected override void OnExit(ExitEventArgs e)
		{
			if (_isInitialised)
			{
				ActiveRadio?.Stop();
				Bass.BASS_Free();
				_cts.Cancel();
				_trayIcon.Dispose();
				_mutex.ReleaseMutex();
			}
			TrayRadio.Properties.Settings.Default.Save();
			base.OnExit(e);
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			_mutex = new Mutex(false, Current.ToString());
			if (_mutex.WaitOne(500, false))
			{
				if ((_isInitialised = Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero)))
				{
					_trayIcon = new NotifyIcon();
					_trayIcon.BalloonTipClicked += (object sender, EventArgs args) =>
					{
						NotifyIcon ni = sender as NotifyIcon;
						if ((ni != null) && (ni.BalloonTipText.StartsWith("New version of")))
							if (System.Windows.MessageBox.Show("Do you wish to download a new version?", "Tray Radio Updater", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
								System.Diagnostics.Process.Start(_updater.UpdateInfo.UpdateLink);
					};
					_trayIcon.MouseClick += (object sender, MouseEventArgs args) =>
					{
						if (args.Button == MouseButtons.Left)
						{
							if (_balanceVolumeWnd == null)
							{
								_balanceVolumeWnd = new BalanceVolumeWindow();
								_balanceVolumeWnd.Closed += (object sender2, EventArgs args2) => { _balanceVolumeWnd = null; };
								_balanceVolumeWnd.Show();
							}
							else if (_balanceVolumeWnd.IsVisible)
								_balanceVolumeWnd.Hide();
							else
								_balanceVolumeWnd.Show();
						}
					};
					_trayIcon.Icon = IconAntennaNoSignal;
					_trayIcon.Text = TrayRadio.Properties.Resources.TrayRadio;
					_trayIcon.Visible = true;
					CreateRadioMenu();
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
										_trayIcon.ContextMenu.MenuItems[0].Enabled = true;
										_trayIcon.Icon = IconAntennaSignal;
										string text = string.Format("{0} - {1}", TrayRadio.Properties.Resources.TrayRadio, ActiveRadio.Name);
										_trayIcon.Text = text.Length >= 60 ? text.Substring(0, 60) + "..." : text;
										if (ActiveRadio.IsNewSong || (status != _radioStatus))
										{
											System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => { _songsHistoryWnd.Add(ActiveRadio); }));											
											ShowBallonTip(string.Format("{0} - Playing\n\n{1}", ActiveRadio.Name, ActiveRadio.Info.Title), ToolTipIcon.Info);
										}
										break;
									case BASSActive.BASS_ACTIVE_STALLED:
										_trayIcon.Icon = IconAntennaSignalStalled;
										if (status != _radioStatus)
											ShowBallonTip(string.Format("{0} - Stalled", ActiveRadio.Name), ToolTipIcon.Warning);
										break;
									default:
										_trayIcon.ContextMenu.MenuItems[0].Enabled = false;
										_trayIcon.Icon = IconAntennaNoSignal;
										_trayIcon.Text = TrayRadio.Properties.Resources.TrayRadio;
										if (status != _radioStatus)
											ShowBallonTip(string.Format("{0} - Stopped", ActiveRadio.Name), ToolTipIcon.Info);
										break; 
								}
								_radioStatus = status;
							}
							Thread.Sleep(1);
						} while (!_cts.IsCancellationRequested);
					});
					// Songs history...
					_songsHistoryWnd = new SongsHistoryWindow();
					// Autoplay...
					if (TrayRadio.Properties.Settings.Default.AutoplayRadio)
						(ActiveRadio = TrayRadio.Properties.Settings.Default.Radios[TrayRadio.Properties.Settings.Default.AutoplayRadioName]).Play();
					// Updater...
					_updater = new Updater.Updater();
					_updater.UpdateInterval = TrayRadio.Properties.Settings.Default.UpdateCheckInterval;
					_updater.UpdateAvailable += (object sender, EventArgs args) =>
					{
						ShowBallonTip(string.Format("New version of {0} {1} is available.", _updater.UpdateInfo.Name, _updater.UpdateInfo.Version), ToolTipIcon.Info);
					};
					_updater.Enable = TrayRadio.Properties.Settings.Default.EnableUpdates;
					// Check for new update now...
					if (_updater.CheckForUpdate())
						ShowBallonTip(string.Format("New version of {0} {1} is available.", _updater.UpdateInfo.Name, _updater.UpdateInfo.Version), ToolTipIcon.Info);
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
		}

		private void RadioBeforePlay(object sender, EventArgs args)
		{
			ActiveRadio?.Stop();
		}

		public void ShowBallonTip(string message, ToolTipIcon icon = ToolTipIcon.Error)
		{
			_trayIcon.BalloonTipIcon = icon;
			_trayIcon.BalloonTipText = message;
			_trayIcon.BalloonTipTitle = TrayRadio.Properties.Resources.TrayRadio;
			_trayIcon.ShowBalloonTip(100);
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
			}
			TrayRadio.Properties.Settings.Default.Radios.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
				{
					switch (e.Action)
					{
						case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
							foreach (RadioEntry item in e.NewItems)
								_trayIcon.ContextMenu.MenuItems.Add(TrayRadio.Properties.Settings.Default.Radios.Count, item.MenuItem);
							if (_trayIcon.ContextMenu.MenuItems[1].Text.CompareTo("-") != 0)
								_trayIcon.ContextMenu.MenuItems.Add(1, new MenuItem("-"));
							break;
						case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
							RadioEntry radio = e.OldItems[0] as RadioEntry;
							_trayIcon.ContextMenu.MenuItems.Remove(radio.MenuItem);
							if (ActiveRadio?.Name.CompareTo(radio.Name) == 0)
								if (ActiveRadio.IsActive)
								{
									ActiveRadio.Stop();
									ActiveRadio = null;
								}
							if (TrayRadio.Properties.Settings.Default.Radios.Count == 0)
								_trayIcon.ContextMenu.MenuItems.RemoveAt(1);
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
						_updater.Enable = TrayRadio.Properties.Settings.Default.EnableUpdates;
						break;
					case "Mute":
						if (ActiveRadio != null)
							ActiveRadio.Mute = TrayRadio.Properties.Settings.Default.Mute;
						break;
					case "UpdateCheckInterval":
						_updater.UpdateInterval = TrayRadio.Properties.Settings.Default.UpdateCheckInterval;
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
