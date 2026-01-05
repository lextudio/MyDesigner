using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Data;

namespace MyDesigner.XamlDesigner.Converters
{
    public class ActiveDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Document)
                return value;

            return BindingOperations.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Document)
                return value;

            return BindingOperations.DoNothing;
        }
    }
}