/*
 * Copyright(C) 2019 Michal Heczko
 * All rights reserved.
 *
 * This software may be modified and distributed under the terms
 * of the BSD license.  See the LICENSE file for details.
 */

using System.IO;

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
