/*
 * Copyright(C) 2019 Michal Heczko
 * All rights reserved.
 *
 * This software may be modified and distributed under the terms
 * of the BSD license.  See the LICENSE file for details.
 */

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TrayRadio
{
	public class RecordFileStream : FileStream
	{
		#region Properties

		public string Artist { get; set; }

		internal RadioEntry Radio { get; }

		public string Title { get; set; }

		#endregion

		#region Methods

		public override void Close()
		{
			(new Id3v1Tag(null, Artist, "Recorded by Tray Radio", Id3v1TagGenre.Other, Title, DateTime.Now.Year)).WriteToStream(this);
			base.Close();
		}

		public void ParseInfo(string info)
		{
			string[] pieces = info.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
			if (pieces.Length == 2)
			{
				Artist = pieces[0].Trim();
				Title = pieces[1].Trim();
			}
		}

		#endregion

		#region Constructor

		public RecordFileStream(string path, FileMode mode, RadioEntry radio) : base(path, mode)
		{
			Radio = radio;
		}

		#endregion
	}
}
