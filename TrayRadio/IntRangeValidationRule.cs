/*
 * Copyright(C) 2017 Michal Heczko
 * All rights reserved.
 *
 * This software may be modified and distributed under the terms
 * of the BSD license.  See the LICENSE file for details.
 */

using System.Globalization;
using System.Windows.Controls;

namespace TrayRadio
{
	public class IntRangeValidationRule : ValidationRule
	{
		#region Fields

		private string _message;

		#endregion

		#region Constants

		public const string DefaultErrorMessage = "Value is not in range <{0};{1}>";

		#endregion

		#region Properties

		public int From { get; set; }

		public string Message
		{
			get
			{
				if (string.IsNullOrEmpty(_message))
					return string.Format(DefaultErrorMessage, From, To);
				return _message;
			}
			set { _message = value; }
		}

		public int To { get; set; }

		#endregion

		#region Methods

		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			int intValue;
			if (int.TryParse(value.ToString(), out intValue) && (intValue >= From) && (intValue <= To))
				return ValidationResult.ValidResult;
			return new ValidationResult(false, Message);
		}

		#endregion
	}
}
