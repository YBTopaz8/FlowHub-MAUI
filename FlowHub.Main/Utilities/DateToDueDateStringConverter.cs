using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowHub.Main.Utilities;

public class DateToDueDateStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime deadline)
        {
            var difference = DateTime.Now.Date - deadline.Date;

            if (difference.TotalDays > 0)
                return $"Due past {difference.TotalDays} day(s)";
            else if (difference.TotalDays < 0)
                return $"Due in {-difference.TotalDays} day(s)";
            else
                return "Due today";
        }
        return "No Deadline Set";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}