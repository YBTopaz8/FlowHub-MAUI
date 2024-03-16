using System.ComponentModel;
using System.Reflection;

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

    public static string GetDescription(Enum enumValue)
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