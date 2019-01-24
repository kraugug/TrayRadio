/*
 * Copyright(C) 2017 Michal Heczko
 * All rights reserved.
 *
 * This software may be modified and distributed under the terms
 * of the BSD license.  See the LICENSE file for details.
 */

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace TrayRadio
{
	static class Extensions
	{
		#region Constants

		public const int GWL_STYLE = -16;
		public const int WS_MAXIMIZEBOX = 0x00010000;
		public const int WS_MINIMIZEBOX = 0x00020000;
		public const int WS_SYSMENU = 0x00080000;

		#endregion

		#region Methods

		/// <summary>
		/// Compare two version strings and returns if are equal, lower or greater.
		/// Version format: '1.2.3.4..(n)'. Any count of separated numbers.
		/// Separator: '.'.
		/// </summary>
		/// <param name="str">String property.</param>
		/// <param name="str">First version string.</param>
		/// <param name="s">Second version string.</param>
		/// <returns>
		/// Returns...
		///    0 .. 'first' and 'second' string are equal.
		///    1 .. 'first' is greater than 'second'.
		///   -1 .. 'first' is lower than 'second'.
		/// </returns>
		public static int CompareVersion(this string str, string s, int digits = 0)
		{
			if (string.IsNullOrEmpty(str))
				throw new ArgumentException("Version string is null or empty.", "first");
			if (string.IsNullOrEmpty(s))
				throw new ArgumentException("Version string is null or empty.", "second");
			// Lets replace N/A...
			if (str.CompareTo("N/A") == 0)
				str = "0.0.0.0";
			if (s.CompareTo("N/A") == 0)
				s = "0.0.0.0";
			// Check for 'first' version string validity...
			string[] firstVersionParts = str.Split('.');
			var results = firstVersionParts.Where(fvp => fvp.IsValidAsNumber());
			if (results.Count() != ((digits > 0) ? Math.Min(firstVersionParts.Length, digits) : firstVersionParts.Length))
				throw new ArgumentException("Invalid version string.", "first");
			// Check for 'second' version string validity...
			string[] secondVersionParts = s.Split('.');
			results = secondVersionParts.Where(svp => svp.IsValidAsNumber());
			if (results.Count() != secondVersionParts.Length)
				throw new ArgumentException("Invalid version string.", "first");
			// Now finally compare the version...
			int versionDigits = Math.Min(firstVersionParts.Length, secondVersionParts.Length);
			int count = digits == 0 ? versionDigits : Math.Min(versionDigits, digits);
			int[] versionParts = new int[count];
			for (int index = 0; index < count; index++)
			{
				int firstPart = index < firstVersionParts.Length ? int.Parse(firstVersionParts[index]) : 0;
				int secondPart = index < secondVersionParts.Length ? int.Parse(secondVersionParts[index]) : 0;
				if (firstPart == secondPart)
					versionParts[index] = 0;
				else
					versionParts[index] = firstPart > secondPart ? 1 : -1;
			}
			var compares = versionParts.Where(vp => vp != 0);
			return compares.Count() > 0 ? compares.First() : 0;
		}

		public static void DisableButtons(this Window window, WindowButtons buttons)
		{
			IntPtr handle = new System.Windows.Interop.WindowInteropHelper(window).Handle;
			int currentStyle = GetWindowLong(handle, GWL_STYLE);
			if (buttons.HasFlag(WindowButtons.Maximize))
				currentStyle &= ~WS_MAXIMIZEBOX;
			if (buttons.HasFlag(WindowButtons.Minimize))
				currentStyle &= ~WS_MINIMIZEBOX;
			SetWindowLong(handle, GWL_STYLE, currentStyle);
		}

		[DllImport("User32", EntryPoint = "GetWindowLong")]
		private static extern int GetWindowLong(IntPtr hwnd, int index);

		[DllImport("User32", EntryPoint = "SetWindowLong")]
		private static extern int SetWindowLong(IntPtr hwnd, int index, int value);

		/// <summary>
		/// Check if string is valid as a number.
		/// </summary>
		/// <param name="str">String property.</param>
		/// <returns>True if is valid otherwise false.</returns>
		public static bool IsValidAsNumber(this string str)
		{
			int result;
			return IsValidAsNumber(str, out result);
		}

		/// <summary>
		/// Check if string is valid as a number.
		/// </summary>
		/// <param name="str">String property.</param>
		/// <param name="value">Output value.</param>
		/// <returns>True if is valid otherwise false.</returns>
		public static bool IsValidAsNumber(this string str, out int value)
		{
			return int.TryParse(str, out value);
		}

		#endregion

		#region Nested types

		[Flags]
		public enum WindowButtons
		{
			None = 0,
			Maximize = 1,
			Minimize = 2
		}

		#endregion
	}
}
