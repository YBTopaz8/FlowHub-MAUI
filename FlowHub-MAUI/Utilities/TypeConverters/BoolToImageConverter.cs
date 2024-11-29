﻿namespace FlowHub_MAUI.Utilities.TypeConverters;
public class BoolToImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var vm = IPlatformApplication.Current!.Services.GetService<HomePageVM>();
     

        if (value is bool MyBoolValue && MyBoolValue is true)
        {
            return FontAw.Solid.HeartCircleMinus;
            
        }
        else
        {
            
            return Regular.Heart;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
