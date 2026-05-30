using AssetManagement.Utility.Report;
using AssetManagement.Utility.Resource;
using ClosedXML.Excel;
using System.IO;
using System.Text;

namespace AssetManagement.Utility.Report.Excel
{
    public static class ExcelManager
    {
        public static byte[] GenerateAssetReportExcel(
            IEnumerable<AssetReportItem> reports,
            IReadOnlyList<string> selectedFields,
            string dateHeader,
            bool filterByAssignmentDate)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(CommonResource.AssetReportWorksheetName);

            var headers = BuildAssetHeaders(selectedFields, dateHeader);
            WriteHeaders(worksheet, headers);

            var row = 2;
            foreach (var report in reports)
            {
                var values = BuildAssetRowValues(report, selectedFields, filterByAssignmentDate);
                for (var col = 0; col < values.Count; col++)
                    worksheet.Cell(row, col + 1).Value = values[col];
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public static byte[] GenerateAssetReportCsv(
            IEnumerable<AssetReportItem> reports,
            IReadOnlyList<string> selectedFields,
            string dateHeader,
            bool filterByAssignmentDate)
        {
            var headers = BuildAssetHeaders(selectedFields, dateHeader);
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", headers.Select(EscapeCsv)));

            foreach (var report in reports)
            {
                var values = BuildAssetRowValues(report, selectedFields, filterByAssignmentDate);
                sb.AppendLine(string.Join(",", values.Select(EscapeCsv)));
            }

          
            return Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(sb.ToString()))
                .ToArray();
        }

        
        public static byte[] GenerateLicenseReportExcel(
            IEnumerable<LicenseReportItem> reports,
            IReadOnlyList<string> selectedFields)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(CommonResource.LicenseReportWorksheetName);

            var headers = BuildLicenseHeaders(selectedFields);
            WriteHeaders(worksheet, headers);

            var row = 2;
            foreach (var report in reports)
            {
                var values = BuildLicenseRowValues(report, selectedFields);
                for (var col = 0; col < values.Count; col++)
                    worksheet.Cell(row, col + 1).Value = values[col];
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

               public static byte[] GenerateLicenseReportCsv(
            IEnumerable<LicenseReportItem> reports,
            IReadOnlyList<string> selectedFields)
        {
            var headers = BuildLicenseHeaders(selectedFields);
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", headers.Select(EscapeCsv)));

            foreach (var report in reports)
            {
                var values = BuildLicenseRowValues(report, selectedFields);
                sb.AppendLine(string.Join(",", values.Select(EscapeCsv)));
            }

            return Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(sb.ToString()))
                .ToArray();
        }

        
        private static void WriteHeaders(IXLWorksheet worksheet, IReadOnlyList<string> headers)
        {
            for (var col = 0; col < headers.Count; col++)
                worksheet.Cell(1, col + 1).Value = headers[col];
        }

        private static IReadOnlyList<string> BuildAssetHeaders(IReadOnlyList<string> selectedFields, string dateHeader)
        {
         
            var headers = new List<string> { "Asset ID", "Asset Name", "Category" };

          
            var optionalMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "SerialNumber",     "Serial Number" },
                { "AssignmentStatus", "Assignment Status" },
                { "HealthStatus",     "Health Status" },
                { "PurchaseDate",     dateHeader },      // label switches per Assigned/All filter
                { "AssetCost",        "Asset Cost" },
                { "Vendor",           "Vendor" },
                { "AssignedEmployee", "Assigned Employee" },
                { "AssignmentDate",   "Assignment Date" },
                { "PropertyNames",    "Property Names and Values" },
            };

            foreach (var key in optionalMap.Keys)
                if (selectedFields.Any(f => f.Equals(key, StringComparison.OrdinalIgnoreCase)))
                    headers.Add(optionalMap[key]);

            return headers;
        }

        private static List<string> BuildAssetRowValues(
            AssetReportItem report,
            IReadOnlyList<string> selectedFields,
            bool filterByAssignmentDate)
        {
            
            var values = new List<string>
            {
                report.AssetId.ToString(),
                report.AssetName        ?? string.Empty,
                report.CategoryName     ?? string.Empty,
            };

            bool Has(string key) =>
                selectedFields.Any(f => f.Equals(key, StringComparison.OrdinalIgnoreCase));

            if (Has("SerialNumber"))
                values.Add(report.SerialNumber ?? string.Empty);

            if (Has("AssignmentStatus"))
                values.Add(report.AssignmentStatus ?? string.Empty);

            if (Has("HealthStatus"))
                values.Add(report.HealthStatus ?? string.Empty);

            if (Has("PurchaseDate"))
                values.Add(filterByAssignmentDate
                    ? report.AssignmentDate?.ToString(CommonResource.DateFormat) ?? string.Empty
                    : report.PurchaseDate?.ToString(CommonResource.DateFormat)   ?? string.Empty);

            if (Has("AssetCost"))
                values.Add(report.AssetCost.ToString("F2"));

            if (Has("Vendor"))
                values.Add(report.VendorName ?? string.Empty);

            if (Has("AssignedEmployee"))
                values.Add(report.EmployeeName ?? string.Empty);

            if (Has("AssignmentDate"))
                values.Add(report.AssignmentDate?.ToString(CommonResource.DateFormat) ?? string.Empty);

            if (Has("PropertyNames"))
                values.Add(report.PropertyName ?? string.Empty);

            return values;
        }

        private static IReadOnlyList<string> BuildLicenseHeaders(IReadOnlyList<string> selectedFields)
        {
            
            var headers = new List<string> { "License ID", "License Name" };

            var optionalMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "LicenseType",       "License Type" },
                { "PurchaseType",      "Purchase Type" },
                { "TotalSeats",        "Total Seats" },
                { "AvailableSeats",    "Available Seats" },
                { "Vendor",            "Vendor" },
                { "PurchaseDate",      "Purchase Date" },
                { "StartDate",         "Start Date" },
                { "ExpiryDate",        "Expiry Date" },
                { "Status",            "Status" },
                { "LicenseKey",        "License Key" },
                { "Cost",              "Cost" },
                { "AssignedEmployees", "Assigned Employee(s)" },
            };

            foreach (var key in optionalMap.Keys)
                if (selectedFields.Any(f => f.Equals(key, StringComparison.OrdinalIgnoreCase)))
                    headers.Add(optionalMap[key]);

            return headers;
        }

        private static List<string> BuildLicenseRowValues(
            LicenseReportItem report,
            IReadOnlyList<string> selectedFields)
        {
           
            var values = new List<string>
            {
                report.LicenseId.ToString(),
                report.LicenseName ?? string.Empty,
            };

            bool Has(string key) =>
                selectedFields.Any(f => f.Equals(key, StringComparison.OrdinalIgnoreCase));

            if (Has("LicenseType"))
                values.Add(report.LicenseType    ?? string.Empty);

            if (Has("PurchaseType"))
                values.Add(report.PurchaseType   ?? string.Empty);

            if (Has("TotalSeats"))
                values.Add(report.TotalSeats.ToString());

            if (Has("AvailableSeats"))
                values.Add(report.AvailableSeats.ToString());

            if (Has("Vendor"))
                values.Add(report.VendorName     ?? string.Empty);

            if (Has("PurchaseDate"))
                values.Add(report.PurchaseDate.ToString(CommonResource.DateFormat));

            if (Has("StartDate"))
                values.Add(report.StartDate.ToString(CommonResource.DateFormat));

            if (Has("ExpiryDate"))
                values.Add(report.ExpiryDate?.ToString(CommonResource.DateFormat) ?? string.Empty);

            if (Has("Status"))
                values.Add(report.LicenseStatus  ?? string.Empty);

            if (Has("LicenseKey"))
                values.Add(report.LicenseKey     ?? string.Empty);

            if (Has("Cost"))
                values.Add(report.Cost.ToString("F2"));

            if (Has("AssignedEmployees"))
              
                values.Add(report.AssignedEmployees ?? string.Empty);

            return values;
        }

      
        private static string EscapeCsv(string value)
        {
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }
    }
}
