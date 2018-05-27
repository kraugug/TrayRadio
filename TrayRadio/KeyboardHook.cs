using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;

namespace TrayRadio
{
	public sealed class KeyboardHook : IDisposable
	{
		#region Fields

		private HookWindow _hookWindow;
		private int _currentId;

		#endregion

		#region Methods

		public void Dispose()
		{
			for (int i = _currentId; i > 0; i--)
				UnregisterHotKey(_hookWindow.Handle, i);
			_hookWindow.Dispose();
		}

		[DllImport("Kernel32")]
		private static extern int GetLastError();

		// Registers a hot key with Windows.
		[DllImport("User32")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

		public void RegisterHotKey(ModifierKeys modifier, Keys key)
		{
			// Increment the counter...
			_currentId = _currentId + 1;
			// Register the hot key...
			if (!RegisterHotKey(_hookWindow.Handle, _currentId, (uint)modifier, (uint)key))
				throw new InvalidOperationException(string.Format("Couldn’t register the hot key ({0}).", GetLastError()));
		}

		// Unregisters the hot key with Windows.
		[DllImport("User32")]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		#endregion

		#region Constructor

		public KeyboardHook()
		{
			_hookWindow = new HookWindow();
			_hookWindow.KeyPressed += (object sender, KeyPressedEventArgs e) =>
			{
				KeyPressed?.Invoke(this, e);
			};
		}

		#endregion

		#region Events

		public event EventHandler<KeyPressedEventArgs> KeyPressed;

		#endregion

		#region Nested types

		private class HookWindow : NativeWindow, IDisposable
		{
			#region Fileds

			private static int WM_HOTKEY = 0x0312;

			#endregion

			#region Methods

			public void Dispose()
			{
				DestroyHandle();
			}

			protected override void WndProc(ref Message m)
			{
				base.WndProc(ref m);
				if (m.Msg == WM_HOTKEY)
					KeyPressed?.Invoke(this, new KeyPressedEventArgs((Keys)(((int)m.LParam >> 16) & 0xFFFF), (ModifierKeys)((int)m.LParam & 0xFFFF)));
			}

			#endregion

			#region Constructor

			public HookWindow()
			{
				CreateHandle(new CreateParams());
			}

			#endregion

			#region Events
			
			public event EventHandler<KeyPressedEventArgs> KeyPressed;

			#endregion
		}

		#endregion
	}
}
