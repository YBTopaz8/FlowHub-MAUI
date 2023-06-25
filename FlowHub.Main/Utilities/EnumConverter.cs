using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FlowHub.Main.Utilities;

public class EnumConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Enum enumValue)
        {
            return GetDescription(enumValue);
        }
        return null;
    }

    private object GetDescription(Enum enumValue)
    {
        FieldInfo fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
        var attribute = (DescriptionAttribute)fieldInfo.GetCustomAttribute(typeof(DescriptionAttribute));
        return attribute?.Description ?? enumValue.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}