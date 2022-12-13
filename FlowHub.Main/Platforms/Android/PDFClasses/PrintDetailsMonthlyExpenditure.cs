﻿using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using FlowHub.Models;
using iText.Kernel.Colors;
using Color = iText.Kernel.Colors.Color;
using System.Collections.ObjectModel;
using FlowHub.Main.AdditionalResourcefulApiClasses;
using static FlowHub.Main.AdditionalResourcefulApiClasses.ExchangeRateAPI;

namespace FlowHub.Main.PDF_Classes;

public class PrintDetailsMonthlyExpenditure
{
    public async Task SaveListDetailMonthlyPlanned(List<ExpendituresModel> expList, string userCurrency, string printDisplayCurrency, string userName, string monthYear)
    {
        ConvertedRate ObjectWithRate = new() { result = 1, date = DateTime.UtcNow};        ;

        if (!userCurrency.Equals(printDisplayCurrency))
        {
            ExchangeRateAPI JSONWithRates = new();

            ObjectWithRate = JSONWithRates.GetConvertedRate(userCurrency, printDisplayCurrency);

        }

        string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        string fileName = $"MonthlyPlanned{monthYear}.pdf";
        string PathFile = $"{path}/{fileName}";

        string pdfTitle = $"List Of Estimated Expenditures For {monthYear}";

        await Task.Run(()=> CreatePFDoc(expList, PathFile, userCurrency, printDisplayCurrency, ObjectWithRate.result, ObjectWithRate.date, pdfTitle, userName));


    }

    void CreatePFDoc(List<ExpendituresModel> expList, string pathFile, string userCurrency, string printDisplayCurrency, double rate, DateTime dateOfRateUpdate, string pdfTitle, string username)
    {
        Color HeaderColor = WebColors.GetRGBColor("DarkSlateBlue");

        
        PdfWriter writer = new(pathFile);
        PdfDocument pdf = new PdfDocument(writer);
        Document document = new Document(pdf);

        Paragraph header = new Paragraph(pdfTitle)
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
            .SetFontColor(HeaderColor)
            .SetBold()
            .SetFontSize(20);
        document.Add(header);

        Table table = new Table(4, false).UseAllAvailableWidth();

        table.AddHeaderCell("#")
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
        table.AddHeaderCell("Description")
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
        table.AddHeaderCell("Amount")
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
        table.AddHeaderCell("Comments")
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

        double totalOfAllExp = 0;
        for (int i = 0; i < expList.Count; i++)
        {
            ExpendituresModel item = expList[i];
            double amount = item.AmountSpent * rate;

            table.AddCell(new Paragraph($"{expList.IndexOf(item) + 1}")
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

            table.AddCell(new Paragraph($"{item.Reason}")
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

            table.AddCell(new Paragraph($"{amount:n2} {printDisplayCurrency}")
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
            table.AddCell(new Paragraph($"{item.Comment}")
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

            if (item.IncludeInReport)
            {
                totalOfAllExp += amount;
            }
        }

        document.Add(table);

        Paragraph footerText = new($"Total Amount {totalOfAllExp:n2} {printDisplayCurrency}");

        Paragraph waterMarkText = new($"Report Generated by FlowHub App for {username}");
        Paragraph bottomNotesText= new($"Converted using the rate of 1 {userCurrency} = {rate:n3} {printDisplayCurrency} \nRate updated on {dateOfRateUpdate:d}");

        document.Add(footerText).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetFontSize(16);
       // document.Add(waterMarkText).SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT).SetFontSize(10);
      //  document.Add(bottomNotesText).SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT).SetFontSize(9);
        

        int numberPages = pdf.GetNumberOfPages();
        for (int i = 1; i <= numberPages; i++)
        {
            document.ShowTextAligned(new Paragraph(string
               .Format("Page" + i + " of " + numberPages)),
               559, 806, i, iText.Layout.Properties.TextAlignment.LEFT,
               iText.Layout.Properties.VerticalAlignment.BOTTOM, 0);
        }

        document.Close();

        SharePdfFile(pdfTitle, pathFile);
    }

    async void SharePdfFile(string PdfTitle, string PathFile)
    {
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = PdfTitle,
            File = new ShareFile(PathFile)
        });
    }



}
