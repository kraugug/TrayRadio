/*
 * Copyright(C) 2017 Michal Heczko
 * All rights reserved.
 *
 * This software may be modified and distributed under the terms
 * of the BSD license.  See the LICENSE file for details.
 */

using System.Collections.ObjectModel;
using System.Linq;

namespace TrayRadio
{
	public class RadioCollection : ObservableCollection<RadioEntry>
	{
		#region Indexers

		public RadioEntry this[string name]
		{
			get { return this.Where(r => r.Name.CompareTo(name) == 0).FirstOrDefault(); }
		}

		#endregion
	}
}
