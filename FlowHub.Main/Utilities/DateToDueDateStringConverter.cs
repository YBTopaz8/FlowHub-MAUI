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
        if (value is DebtModel debt)
        {
            if (debt.IsPaidCompletely)
            {
                if (debt.DatePaidCompletely is null)
                {
                    return "Error";
                }
                var DatePaidDiff = DateTime.Now.Date - debt.DatePaidCompletely?.Date;
                if (DatePaidDiff.Value.TotalDays == 1)
                {
                    return "Paid 1 day ago";
                }
                else if (DatePaidDiff.Value.TotalDays == 0)
                {
                    return "Paid today";
                }
                else
                {
                    return $"Paid {DatePaidDiff.Value.TotalDays} days ago";
                }
            }
            else
            {
                if (!debt.Deadline.HasValue)
                {
                    return "No Deadline Provided";
                }
                var diff = DateTime.Now.Date - debt.Deadline.Value.Date;
                if (diff.TotalDays == 1)
                {
                    return $"Due in {-diff.TotalDays} day";
                }
                if (diff.TotalDays > 1)
                {
                    return $"Due past {diff.TotalDays} days!";
                }
                else if (diff.TotalDays < 0)
                {
                    return $"Due in {-diff.TotalDays} days";
                }
                else
                {
                    return "Due today";
                }
            }
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}