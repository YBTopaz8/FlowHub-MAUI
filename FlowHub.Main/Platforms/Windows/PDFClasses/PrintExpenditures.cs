﻿using FlowHub.Main.AdditionalResourcefulApiClasses;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Geom;
using TextAlignment = iText.Layout.Properties.TextAlignment;
using static FlowHub.Main.AdditionalResourcefulApiClasses.ExchangeRateAPI;
using Color = iText.Kernel.Colors.Color;
using Path = System.IO.Path;

namespace FlowHub.Main.PDF_Classes;

//Printing on Windows
public static class PrintExpenditures
{
    public static async Task SaveExpenditureToPDF(ObservableCollection<ExpendituresModel> expList, string userCurrency, string printDisplayCurrency, string userName)
    {
        ConvertedRate ObjectWithRate = new() { result = 1, date = DateTime.UtcNow };

        if (!userCurrency.Equals(printDisplayCurrency))
        {
            ExchangeRateAPI JsonWithRates = new();
            ObjectWithRate = JsonWithRates.GetConvertedRate(userCurrency, printDisplayCurrency);
        }

        string PathFile = EnsureDirectoryAndReturnPath(printDisplayCurrency);

        const string PdfTitle = "Flow Outs Report";

        await Task.Run(() => CreatePdfDoc(expList, PathFile, userCurrency, printDisplayCurrency, ObjectWithRate.result, ObjectWithRate.date, PdfTitle, userName));
    }

    private static string EnsureDirectoryAndReturnPath(string currency)
    {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FlowHub");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        string fileName = $"FlowOutsReport_{currency}_{DateTime.Now:ddd, dd MMMM yyyy}.pdf";
        return $"{path}/{fileName}";
    }

    static void CreatePdfDoc(ObservableCollection<ExpendituresModel> expList, string PathFile, string userCurrency, string printDisplayCurrency, double rate, DateTime dateOfRateUpdate, string pdfTitle, string username)
    {
        Color HeaderTextColor = WebColors.GetRGBColor("darkslateblue");

        using PdfWriter writer = new(PathFile);
        using PdfDocument pdf = new(writer);
        Document document = new(pdf, pageSize: PageSize.A4, immediateFlush: false);
        Paragraph header = new Paragraph(pdfTitle)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontColor(HeaderTextColor)
            .SetBold()
            .SetFontSize(20);
        document.Add(header);
        document.Flush();

        Table table = new Table(4, false).UseAllAvailableWidth();

        table.AddHeaderCell("#")
                .SetTextAlignment(TextAlignment.CENTER);
        table.AddHeaderCell("Description")
                .SetTextAlignment(TextAlignment.CENTER);
        table.AddHeaderCell("Amount Spent")
                .SetTextAlignment(TextAlignment.CENTER);
        table.AddHeaderCell("Date Spent")
                .SetTextAlignment(TextAlignment.CENTER);

        double totall = 0;
        foreach (var item in expList)
        {
            double amount = item.AmountSpent * rate;

            table.AddCell(new Paragraph($"{expList.IndexOf(item) + 1}")
                .SetTextAlignment(TextAlignment.CENTER));

            table.AddCell(new Paragraph($"{item.Reason}")
                .SetTextAlignment(TextAlignment.CENTER));

            table.AddCell(new Paragraph($"{amount:n2} {printDisplayCurrency}")
                .SetTextAlignment(TextAlignment.CENTER));

            table.AddCell(new Paragraph($"{item.DateSpent.ToShortDateString()}")
                .SetTextAlignment(TextAlignment.CENTER));

            totall += amount;
        }

        document.Add(table);
        document.Flush();

        Paragraph footerText = new Paragraph($"Total Spent: {totall:n2} {printDisplayCurrency}")
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontSize(24)
            .SetBold();

        Paragraph waterMarkText = new Paragraph($"Report Generated by FlowHub App for {username}")
            .SetTextAlignment(TextAlignment.LEFT)
            .SetFontSize(15);

        Paragraph bottomNotesText = new Paragraph($"Converted using the rate of 1 {userCurrency} = {rate:n3} {printDisplayCurrency} \nRate updated on {dateOfRateUpdate:D}")
            .SetTextAlignment(TextAlignment.LEFT)
            .SetFontSize(10);

        document.Add(new Paragraph());
        document.Flush();

        document.Add(footerText);
        document.Flush();

        document.Add(new Paragraph());
        document.Flush();

        document.Add(waterMarkText);
        document.Flush();

        document.Add(new Paragraph());
        document.Flush();

        document.Add(bottomNotesText);
        document.Flush();

        int numberPages = pdf.GetNumberOfPages();
        for (int i = 1; i <= numberPages; i++)
        {
            document.ShowTextAligned(new Paragraph(string
               .Format("Page" + i + " of " + numberPages)),
               559, 806, i, TextAlignment.LEFT,
               iText.Layout.Properties.VerticalAlignment.BOTTOM, 0);
        }

        document.Close();
    }
}
