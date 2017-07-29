/*
 * Copyright(C) 2017 Michal Heczko
 * All rights reserved.
 *
 * This software may be modified and distributed under the terms
 * of the BSD license.  See the LICENSE file for details.
 */

using System.IO;
using System.Xml.Serialization;

namespace TrayRadio.Updater
{
	public class UpdateInfo
	{
		#region Properties

		public string Changes { get; set; }

		public string Name { get; set; }

		public string ReleaseDate { get; set; }

		public string UpdateLink { get; set; }

		public string Version { get; set; }

		#endregion

		#region Methods

		public static UpdateInfo From(string filename)
		{
			using (StreamReader reader = new StreamReader(filename))
				return (UpdateInfo)(new XmlSerializer(typeof(UpdateInfo))).Deserialize(reader);
		}

		public void To(string filename)
		{
			using (StreamWriter writer = new StreamWriter(filename))
				(new XmlSerializer(typeof(UpdateInfo))).Serialize(writer, this);
		}

		#endregion
	}
}
