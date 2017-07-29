/*
 * Copyright(C) 2017 Michal Heczko
 * All rights reserved.
 *
 * This software may be modified and distributed under the terms
 * of the BSD license.  See the LICENSE file for details.
 */

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Un4seen.Bass;

namespace TrayRadio
{
	public class RadioEntry
	{
		#region Fields

		private bool _active;
		private double _balance;
		private ShoutcastMetadata _previouseInfo;
		private bool _mute;
		private string _name;
		private double _volume;

		#endregion

		#region Properties

		internal double Balance
		{
			get { return _balance; }
			set
			{
				_balance = value;
				SetBalance(_balance);
			}
		}

		internal int ChannelHandle { get; set; }

		internal ShoutcastMetadata Info { get { return ShoutcastMetadata.From(Marshal.PtrToStringAnsi(Bass.BASS_ChannelGetTags(ChannelHandle, BASSTag.BASS_TAG_META))); } }

		internal bool IsActive
		{
			get { return _active; }
			private set { _active = MenuItem.DefaultItem = value; }
		}

		internal bool IsNewSong
		{
			get
			{
				ShoutcastMetadata info = Info;

				if ((_previouseInfo == null && info == null) || (_previouseInfo != null && info == null))
					return false;
				if (_previouseInfo == null && info != null)
				{
					_previouseInfo = info;
					return true;
				}
				if (_previouseInfo.Title.Equals(info.Title))
					return false;
				_previouseInfo = info;
				return true;
			}
		}
		
		internal MenuItem MenuItem { get; }

		internal bool Mute
		{
			get { return _mute; }
			set
			{
				_mute = value;
				SetVolume(_mute ? 0 : Properties.Settings.Default.Volume);
			}
		}

		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				MenuItem.Text = _name;
			}
		}

		internal BASSActive Status
		{
			get
			{
				BASSActive status = ChannelHandle != 0 ? Bass.BASS_ChannelIsActive(ChannelHandle) : BASSActive.BASS_ACTIVE_STOPPED; ;
				IsActive = status != BASSActive.BASS_ACTIVE_STOPPED;
				return status;
			}
		}

		internal double Volume
		{
			get { return _volume; }
			set
			{
				_volume = value;
				if (!_mute)
					SetVolume(_volume);
			}
		}

		public string Url { get; set; }

		#endregion

		#region Methods

		public void Play()
		{
			OnBeforePlay?.Invoke(this, EventArgs.Empty);
			ChannelHandle = Bass.BASS_StreamCreateURL(Url, 0, BASSFlag.BASS_DEFAULT, null, IntPtr.Zero);
			if (ChannelHandle != 0)
			{
				if (!(IsActive = Bass.BASS_ChannelPlay(ChannelHandle, false)))
					App.Instance.ShowBallonTip(string.Format("BASS_ChannelPlay: {0}", Bass.BASS_ErrorGetCode().ToString()));
			}
			else
				App.Instance.ShowBallonTip(string.Format("BASS_StreamCreateURL: {0}", Bass.BASS_ErrorGetCode().ToString()));
			OnAfterPlay?.Invoke(this, EventArgs.Empty);
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
				IsActive = false;
			}
			OnAfterStop?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		#region Constructor(s)

		public RadioEntry()
		{
			ChannelHandle = 0;
			MenuItem = new MenuItem();
			MenuItem.Click += (object sender, EventArgs args) => { Play(); };
		}

		public RadioEntry(string name, string url) : this()
		{
			MenuItem.Text = name;
			Name = name;
			Url = url;
		}

		#endregion

		#region Events

		public event EventHandler OnAfterPlay;
		public event EventHandler OnAfterStop;
		public event EventHandler OnBeforePlay;
		public event EventHandler OnBeforeStop;

		#endregion
	}
}
