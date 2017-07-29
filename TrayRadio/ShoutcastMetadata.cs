/*
 * Copyright(C) 2017 Michal Heczko
 * All rights reserved.
 *
 * This software may be modified and distributed under the terms
 * of the BSD license.  See the LICENSE file for details.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrayRadio
{
	// Example: StreamTitle='Gorefest - Demon Seed * Eth OnAir * Black-, Death- und Thrash-Metal *';StreamUrl='http://www.www.metal-only.de';
	public class ShoutcastMetadata
	{
		#region Properties

		public string Title { get; }

		public string Url { get; }

		#endregion

		#region Methods

		public static ShoutcastMetadata From(string str)
		{
			if (string.IsNullOrWhiteSpace(str))
				return null;
			char[] charsToTrim = new char[] { '\'' };
			string[] peaces = str.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			string title = string.Empty, url = string.Empty;
			if (peaces.Length > 1)
			{
				title = peaces[0].Split('=')[1].TrimStart(charsToTrim).TrimEnd(charsToTrim);
				url = peaces[1].Split('=')[1].TrimStart(charsToTrim).TrimEnd(charsToTrim);
			}
			else
				title = peaces[0].Split('=')[1].TrimStart(charsToTrim).TrimEnd(charsToTrim).TrimStart().TrimEnd();
			return new ShoutcastMetadata(title, url);
		}

		#endregion

		#region Constructor

		public ShoutcastMetadata(string title, string url)
		{
			Title = title;
			Url = url;
		}

		#endregion
	}
}
