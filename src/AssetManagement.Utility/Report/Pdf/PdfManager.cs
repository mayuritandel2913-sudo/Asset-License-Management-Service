using AssetManagement.Utility.Report;
using AssetManagement.Utility.Resource;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.IO;
namespace AssetManagement.Utility.Report.Pdf
{
    public static class PdfManager
    {
        public static byte[] GenerateAssetReportPdf(IEnumerable<AssetReportItem> reports, string dateHeader, bool filterByAssignmentDate)
        {
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdfDoc = new PdfDocument(writer);
            using var document = new Document(pdfDoc);

            document.Add(new Paragraph(CommonResource.AssetReportTitle)
                .SetFontSize(20)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(ColorConstants.BLUE));
            document.Add(new Paragraph(string.Empty));

            var table = BuildAssetReportTable(dateHeader);
            foreach (var report in reports)
            {
                AddAssetReportTableRow(table, report, filterByAssignmentDate);
            }

            document.Add(table);
            document.Close();

            return stream.ToArray();
        }

        private static Table BuildAssetReportTable(string dateHeader)
        {
            var table = new Table(12);
            var headers = new[]
            {
                "Asset ID","Asset Name","Category","Serial Number",dateHeader,
                "Asset Cost","Vendor","Status","Description","Assigned To",
                CommonResource.AssignmentDateHeader,"Property Name"
            };

            foreach (var header in headers)
            {
                table.AddHeaderCell(new Cell().Add(new Paragraph(header).SetFontSize(10).SetBackgroundColor(ColorConstants.LIGHT_GRAY)));
            }

            return table;
        }

        private static void AddAssetReportTableRow(Table table, AssetReportItem report, bool filterByAssignmentDate)
        {
            table.AddCell(new Cell().Add(new Paragraph(report.AssetId.ToString()).SetFontSize(9)));
            table.AddCell(new Cell().Add(new Paragraph(report.AssetName ?? string.Empty).SetFontSize(9)));
            table.AddCell(new Cell().Add(new Paragraph(report.CategoryName ?? string.Empty).SetFontSize(9)));
            table.AddCell(new Cell().Add(new Paragraph(report.SerialNumber ?? string.Empty).SetFontSize(9)));
            table.AddCell(new Cell().Add(new Paragraph(filterByAssignmentDate ? report.AssignmentDate?.ToString(CommonResource.DateFormat) ?? string.Empty : report.PurchaseDate?.ToString(CommonResource.DateFormat) ?? string.Empty).SetFontSize(9)));
            table.AddCell(new Cell().Add(new Paragraph(report.AssetCost.ToString("F2")).SetFontSize(9)));
            table.AddCell(new Cell().Add(new Paragraph(report.VendorName ?? string.Empty).SetFontSize(9)));
            table.AddCell(new Cell().Add(new Paragraph(report.AssetStatus ?? string.Empty).SetFontSize(9)));
            table.AddCell(new Cell().Add(new Paragraph(report.Description ?? string.Empty).SetFontSize(9)));
            table.AddCell(new Cell().Add(new Paragraph(report.EmployeeName ?? string.Empty).SetFontSize(9)));
            table.AddCell(new Cell().Add(new Paragraph(report.AssignmentDate?.ToString(CommonResource.DateFormat) ?? string.Empty).SetFontSize(9)));
            table.AddCell(new Cell().Add(new Paragraph(report.PropertyName ?? string.Empty).SetFontSize(9)));
        }
    }
}
