/*
 * Copyright(C) 2017 Michal Heczko
 * All rights reserved.
 *
 * This software may be modified and distributed under the terms
 * of the BSD license.  See the LICENSE file for details.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml.Serialization;
using Un4seen.Bass;

namespace TrayRadio
{
	public class RadioEntry : INotifyPropertyChanged
	{
		#region Fields

		private bool m_Active;
		private double m_Balance;
		private DOWNLOADPROC m_DownloadProc;
		private ShoutcastMetadata m_PreviouseInfo;
		private bool m_IsRecording;
		private bool m_Mute;
		private string m_Name;
		private RecordFileStream m_RecordFileStream;
		private double m_Volume;

		#endregion

		#region Properties

		[XmlIgnore]
		public string ActiveRecordingFileName { get { return m_RecordFileStream?.Name; } }

		internal double Balance
		{
			get { return m_Balance; }
			set
			{
				m_Balance = value;
				SetBalance(m_Balance);
			}
		}

		internal int ChannelHandle { get; set; }

		internal BASS_CHANNELINFO ChannelInfo { get; set; }

		internal ShoutcastMetadata Info { get { return ShoutcastMetadata.From(Marshal.PtrToStringAnsi(Bass.BASS_ChannelGetTags(ChannelHandle, BASSTag.BASS_TAG_META))); } }

		internal bool IsActive
		{
			get { return m_Active; }
			private set
			{
				m_Active = value;
				MenuStripItem.Font = new System.Drawing.Font(MenuStripItem.Font, m_Active ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular);
			}
		}

		internal bool IsNewSong
		{
			get
			{
				ShoutcastMetadata info = Info;

				if ((m_PreviouseInfo == null && info == null) || (m_PreviouseInfo != null && info == null))
					return false;
				if (m_PreviouseInfo == null && info != null)
				{
					m_PreviouseInfo = info;
					return true;
				}
				if (m_PreviouseInfo.Title.Equals(info.Title))
					return false;
				m_PreviouseInfo = info;
				return true;
			}
		}

        [XmlIgnore]
		public bool IsRecording
		{
			get { return m_IsRecording; }
			private set
			{
				m_IsRecording = value;
				FirePropertyChangedEvent(nameof(IsRecording));
			}
		}

		internal ToolStripItem MenuStripItem { get; }

		internal bool Mute
		{
			get { return m_Mute; }
			set
			{
				m_Mute = value;
				SetVolume(m_Mute ? 0 : Properties.Settings.Default.Volume);
			}
		}

		public string Name
		{
			get { return m_Name; }
			set
			{
				m_Name = value;
				MenuStripItem.Text = m_Name;
			}
		}

		internal BASSActive Status
		{
			get
			{
                BASSActive status = ChannelHandle != 0 ? Bass.BASS_ChannelIsActive(ChannelHandle) : BASSActive.BASS_ACTIVE_STOPPED;
				IsActive = status != BASSActive.BASS_ACTIVE_STOPPED;
				return status;
			}
		}

		internal double Volume
		{
			get { return m_Volume; }
			set
			{
				m_Volume = value;
				if (!m_Mute)
					SetVolume(m_Volume);
			}
		}

		public string Url { get; set; }

		#endregion

		#region Methods

		private void DownloadProc(IntPtr buffer, int length, IntPtr user)
		{
			if (m_RecordFileStream != null)
			{
				if ((buffer != null) && (buffer != IntPtr.Zero) && (Status == BASSActive.BASS_ACTIVE_PLAYING))
				{
					byte[] data = new byte[length];
					Marshal.Copy(buffer, data, 0, length);
					if (data != null && data.Length > 0)
						m_RecordFileStream.Write(data, 0, data.Length);
				}
			}
		}

		protected void FirePropertyChangedEvent(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void Play()
		{
			OnBeforePlay?.Invoke(this, EventArgs.Empty);
			ChannelHandle = Bass.BASS_StreamCreateURL(Url, 0, BASSFlag.BASS_DEFAULT, m_DownloadProc, IntPtr.Zero);
			if (ChannelHandle != 0)
			{
				if (!(IsActive = Bass.BASS_ChannelPlay(ChannelHandle, false)))
					App.Instance.ShowBallonTip(string.Format("BASS_ChannelPlay: {0}", Bass.BASS_ErrorGetCode().ToString()));
				ChannelInfo = Bass.BASS_ChannelGetInfo(ChannelHandle);
			}
			else
				App.Instance.ShowBallonTip(string.Format("BASS_StreamCreateURL: {0}", Bass.BASS_ErrorGetCode().ToString()));
			OnAfterPlay?.Invoke(this, EventArgs.Empty);
		}

		public void StartRecording()
		{
            OnBeforeStartRecording?.Invoke(this, EventArgs.Empty);
            string radioRecordFolder = Path.Combine(Properties.Settings.Default.RecordsFolder, App.Instance.ActiveRadio.Name);
			if (!Directory.Exists(radioRecordFolder))
				Directory.CreateDirectory(radioRecordFolder);
			string fileTitle;
			if (!string.IsNullOrEmpty(App.Instance.ActiveRadio.Info.Title))
				fileTitle = App.Instance.ActiveRadio.Info.Title;
			else
				fileTitle = DateTime.Now.ToString("d/M/yyyy HH/mm/ss");
			string fileToSave = Path.Combine(radioRecordFolder, string.Format("{0}.mp3", fileTitle));
			foreach (char ch in System.IO.Path.GetInvalidFileNameChars())
				if ((ch != '\\') && (ch != ':'))
					fileToSave = fileToSave.Replace(ch, '-');
			m_RecordFileStream = new RecordFileStream(fileToSave, FileMode.Create);
			if (m_RecordFileStream != null)
			{
				m_RecordFileStream.ParseInfo(App.Instance.ActiveRadio.Info.Title);
                IsRecording = true;
                PreferencesWindow.Instance?.RefreshRecordingsList();
			}
            OnAfterStartRecording?.Invoke(this, EventArgs.Empty);
        }

		public void StopRecording()
		{
            OnBeforeStopRecording?.Invoke(this, EventArgs.Empty);
			if (IsRecording)
			{
				IsRecording = false;
				m_RecordFileStream.Flush();
				m_RecordFileStream.Close();
				m_RecordFileStream.Dispose();
				m_RecordFileStream = null;
                PreferencesWindow.Instance?.RefreshRecordingsList();
            }
            OnAfterStopRecording?.Invoke(this, EventArgs.Empty);
        }

		protected void SetBalance(double balance)
		{
			if (ChannelHandle != 0)
				if (!Bass.BASS_ChannelSetAttribute(ChannelHandle, BASSAttribute.BASS_ATTRIB_PAN, (float)balance / 100))
					App.Instance.ShowBallonTip(string.Format("BASS_ChannelSetAttribute: {0}", Bass.BASS_ErrorGetCode().ToString()));
		}

		protected void SetVolume(double volume)
		{
			if (ChannelHandle != 0)
				if (!Bass.BASS_ChannelSetAttribute(ChannelHandle, BASSAttribute.BASS_ATTRIB_VOL, (float)volume / 100))
					App.Instance.ShowBallonTip(string.Format("BASS_ChannelSetAttribute: {0}", Bass.BASS_ErrorGetCode().ToString()));
		}

		public void Stop()
		{
			OnBeforeStop?.Invoke(this, EventArgs.Empty);
			if (Bass.BASS_ChannelStop(ChannelHandle))
			{
				Bass.BASS_StreamFree(ChannelHandle);
				ChannelHandle = 0;
				StopRecording();
				IsActive = false;
			}
			OnAfterStop?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		#region Constructor(s)

		public RadioEntry()
		{
			ChannelHandle = 0;
			MenuStripItem = new ToolStripMenuItem();
			MenuStripItem.Click += (object sender, EventArgs args) => { Play(); };
			m_DownloadProc = new DOWNLOADPROC(DownloadProc);
		}

		public RadioEntry(string name, string url) : this()
		{
			MenuStripItem.Text = name;
			Name = name;
			Url = url;
		}

		#endregion

		#region Events

		public event EventHandler OnAfterPlay;
		public event EventHandler OnAfterStop;
        public event EventHandler OnAfterStartRecording;
        public event EventHandler OnAfterStopRecording;
        public event EventHandler OnBeforePlay;
		public event EventHandler OnBeforeStop;
        public event EventHandler OnBeforeStartRecording;
        public event EventHandler OnBeforeStopRecording;
        public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}
