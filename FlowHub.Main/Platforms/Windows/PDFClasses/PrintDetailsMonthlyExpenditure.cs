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
   

    public async void SharePdfFile(string PdfTitle, string FilePath)
    {
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = PdfTitle,
            File = new ShareFile(FilePath)
        });
    }



}
