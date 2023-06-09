using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using FlowHub.Models;
using iText.Kernel.Colors;
using Color = iText.Kernel.Colors.Color;
using System.Collections.ObjectModel;

namespace FlowHub.Main.PDF_Classes;

public class PrintDetailsMonthlyExpenditure
{

    public Task SaveListDetailMonthlyPlanned(List<ExpendituresModel> expList, string userCurrency, string printDisplayCurrency, string userName, string monthYear)
    {
        throw new NotImplementedException();
    }

    public Task SaveListDetailMonthlyPlanned(List<List<ExpendituresModel>> expLists, string userCurrency, string printDisplayCurrency, string userName, List<string> ListOfTitles)
    {
        throw new NotImplementedException();
    }
}
