using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TrayRadio
{
	public class RecordFileStream : FileStream
	{
		#region Properties

		public string Artist { get; set; }

		public string Title { get; set; }

		#endregion

		#region Methods

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		public void ParseInfo(string info)
		{

		}

		#endregion

		#region Constructor

		public RecordFileStream(string path, FileMode mode) : base(path, mode)
		{ }

		#endregion
	}
}
