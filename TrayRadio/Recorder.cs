using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;

namespace TrayRadio
{
	public class Recorder : INotifyPropertyChanged
	{
		#region Fileds

		private int handle_;
		private bool isReady_;
		private bool isReccording_;
		private FileStream recordFileStream_;
		private RECORDPROC _recordProc;

		#endregion

		#region Properties

		private int Handle
		{
			get { return handle_; }
			set
			{
				handle_ = value;
				FirePropertyChangedEvent("Handle");
			}
		}

		public bool IsReady
		{
			get { return isReady_; }
			private set
			{
				isReady_ = value;
				FirePropertyChangedEvent("IsReady");
			}
		}

		public bool IsRecording
		{
			get { return isReccording_; }
			private set
			{
				isReccording_ = value;
				FirePropertyChangedEvent("IsRecording");
			}
		}

		#endregion

		#region Methods

		protected void FirePropertyChangedEvent(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private bool RecordProc(int handle, IntPtr buffer, int length, IntPtr user)
		{
			byte[] byteBuffer = new byte[length];
			Marshal.Copy(buffer, byteBuffer, 0, length);
			recordFileStream_.Write(byteBuffer, 0, length);	
			return true;
		}

		public void Start()
		{
			if (IsReady = Bass.BASS_RecordInit(/*Properties.Settings.Default.BassRecordDevice*/-1))
			{
				string fileTitle = string.IsNullOrEmpty(App.Instance.ActiveRadio.Info.Title) ? App.Instance.ActiveRadio.Name : App.Instance.ActiveRadio.Info.Title;
				string fileToSave = Path.Combine(Properties.Settings.Default.RecordsFolder, string.Format("{0}.{1}.wav", fileTitle, DateTime.Now.ToString("yyyy-MM-dd.HH-mm-ss")));
				if (!Directory.Exists(Properties.Settings.Default.RecordsFolder))
					Directory.CreateDirectory(Properties.Settings.Default.RecordsFolder);
				recordFileStream_ = new FileStream(fileToSave, FileMode.Create);
				_recordProc = new RECORDPROC(RecordProc);
				IsRecording = (Handle = Bass.BASS_RecordStart(Properties.Settings.Default.BassRecordFrequency, Properties.Settings.Default.BassRecordChannels, BASSFlag.BASS_RECORD_PAUSE, 50, _recordProc, IntPtr.Zero)) != 0;
				if (IsRecording)
					Bass.BASS_ChannelPlay(Handle, false);
			}
		}

		public void Stop()
		{
			if (IsReady)
			{
				if (IsRecording)
					Bass.BASS_ChannelStop(Handle);
				Bass.BASS_RecordFree();
				recordFileStream_.Flush();
				recordFileStream_.Close();
				IsRecording = false;
				IsReady = false;
			}
		}

		#endregion

		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}
