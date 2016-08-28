using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WpfBuiltinDpiTest
{
	[ValueConversion(typeof(DpiScale), typeof(string))]
	public class DpiScaleToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is DpiScale))
				return DependencyProperty.UnsetValue;

			var dpi = (DpiScale)value;
			return $"{dpi.PixelsPerInchX}-{dpi.PixelsPerInchY}";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}