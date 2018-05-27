using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;

namespace TrayRadio
{
	public class ObjectToFormatedStringConverter : MarkupExtension, IValueConverter
	{
		#region Properties

		public string Format { get; set; }

		#endregion

		#region Methods

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return string.Format(Format, value.ToString());
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;// _converter ?? (_converter = new T());
		}

		#endregion

		#region Constructors

		public ObjectToFormatedStringConverter()
		{
			Format = "{0}";
		}

		#endregion
	}
}
