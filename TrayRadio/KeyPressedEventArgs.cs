using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;

namespace TrayRadio
{
	public class KeyPressedEventArgs : EventArgs
	{
		#region Properties

		public ModifierKeys Modifier { get; }

		public Keys Key { get; }

		#endregion

		#region Constructor

		public KeyPressedEventArgs(Keys key, ModifierKeys modifier)
		{
			Key = key;
			Modifier = modifier;
		}

		#endregion
	}
}
