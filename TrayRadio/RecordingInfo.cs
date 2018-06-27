using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace TrayRadio
{
    public class RecordingInfo : INotifyPropertyChanged
    {
        #region Fields

        private bool m_isActive = false;

        #endregion

        #region Properties

        public bool IsActive
        {
            get { return m_isActive; }
            set
            {
                m_isActive = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsActive)));
            }
        }

        public string FileName { get; }

        public string FullPath { get; }

        public string RadioName { get; }

        #endregion

        #region Constructor

        public RecordingInfo(string fileName)
        {
            FileName = Path.GetFileName(fileName);
            FullPath = fileName;
            string folder = Path.GetDirectoryName(fileName);
            int index = folder.LastIndexOf('\\') + 1;
            RadioName = folder.Substring(index, folder.Length - index);
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
