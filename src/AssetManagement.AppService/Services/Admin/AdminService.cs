
using AssetManagement.AppService.Contracts;
using AssetManagement.AppService.DTOs;
using AssetManagement.AppService.DTOs.asset;
using AssetManagement.Infrastructure.Contracts;
using AssetManagement.Infrastructure.Entities;
using AssetManagement.Utility.Exceptions;
using AssetManagement.Utility;
using AssetManagement.Utility.Resource;
using AssetManagement.Utility.Report.Excel;
using AssetManagement.Utility.Report;
using AssetManagement.Utility.Report.Pdf;
using Microsoft.Extensions.Logging;

namespace AssetManagement.AppService.Services.AdminService
{
    public class AdminService : IAdminService
    {
        private readonly IMasterServiceRepository _masterServiceRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAssetRepository _assetRepository;
        private readonly ILogger<AdminService> _logger;

        public AdminService(IMasterServiceRepository masterServiceRepository, ICategoryRepository categoryRepository, IAssetRepository assetRepository, ILogger<AdminService> logger)
        {
            _masterServiceRepository = masterServiceRepository;
            _categoryRepository = categoryRepository;
            _assetRepository = assetRepository;
            _logger = logger;
        }

        public async Task<AssetUtilizationPagedResponse> GetAssetUtilizationAsync()
        {
            _logger.LogInformation("Fetching asset utilization data.");

            var assets = (await _assetRepository.GetALLAsync())
                .Where(asset => asset.DeletedDate == null)
                .ToList();

            var categories = (await _masterServiceRepository.GetCategoriesAsync()).ToList();

            if (!assets.Any() || !categories.Any())
            {
                _logger.LogWarning("No data available for asset utilization.");
                throw new NotFoundException(CommonResource.NoDataAvailable);
            }

            var utilizationCategories = categories
                .Select(category =>
                {
                    var categoryAssets = assets.Where(asset => asset.CategoryID == category.CategoryID).ToList();

                    return new AssetUtilizationCategoryResponse
                    {
                        CategoryName = category.CategoryName,
                        AssignedAsset = categoryAssets.Count(asset => asset.UserID.HasValue),
                        UnassignedAsset = categoryAssets.Count(asset => !asset.UserID.HasValue)
                    };
                })
                .ToList();

            if (!utilizationCategories.Any())
            {
                _logger.LogWarning("No utilization categories were produced.");
                throw new NotFoundException(CommonResource.NoDataAvailable);
            }

            return new AssetUtilizationPagedResponse
            {
                Data = new List<AssetUtilizationChartResponse>
                {
                    new AssetUtilizationChartResponse
                    {
                        Categories = utilizationCategories
                    }
                },
                Message = CommonResource.AssetUtilizationFetchedSuccessfully,
                StatusCode = 200
            };
        }

        public async Task<AssetDowntimePagedResponse> GetAssetDowntimeRateAsync()
        {
            _logger.LogInformation("Fetching asset downtime rate data.");

            var assets = (await _assetRepository.GetALLAsync())
                .Where(asset => asset.DeletedDate == null)
                .ToList();
            var downtimeStatusNames = (await _assetRepository.GetAssetHealthStatusNamesAsync())
                .Select(NormalizeStatus)
                .Where(status => !string.IsNullOrWhiteSpace(status))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var categories = (await _masterServiceRepository.GetCategoriesAsync()).ToList();

            if (!assets.Any() || !categories.Any())
            {
                _logger.LogWarning("No data available for asset downtime rate.");
                throw new NotFoundException(CommonResource.NoDataAvailable);
            }

            // Get health-status names considered downtime (e.g., not 'Good') and normalize them
            var blockedHealthStatuses = (await _assetRepository.GetBlockedHealthStatusNamesAsync())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => new string(n.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant())
                .ToHashSet();

            var downtimeCategories = categories
                .Select(category =>
                {
                    var categoryAssets = assets.Where(asset => asset.CategoryID == category.CategoryID).ToList();
                    var totalAssets = categoryAssets.Count;
                    var downtimeAssets = categoryAssets.Count(asset =>
                        IsDowntimeAsset(asset.AssetStatus?.AssetStatusName, downtimeStatusNames) ||
                        IsDowntimeAsset(asset.AssetHealthStatus?.AssetHealthStatusName, blockedHealthStatuses));

                    _logger.LogDebug("Category {CategoryId}-{CategoryName}: totalAssets={TotalAssets}, downtimeAssets={DowntimeAssets}", category.CategoryID, category.CategoryName, totalAssets, downtimeAssets);

                    var downtimePercentage = totalAssets == 0
                        ? 0m
                        : Math.Round((downtimeAssets / (decimal)totalAssets) * 100m, 2);

                    return new AssetDowntimeCategoryResponse
                    {
                        CategoryName = category.CategoryName,
                        DowntimePercentage = downtimePercentage
                    };
                })
                .ToList();

            return new AssetDowntimePagedResponse
            {
                Data = new List<AssetDowntimeChartResponse>
                {
                    new AssetDowntimeChartResponse
                    {
                        Categories = downtimeCategories
                    }
                },
                Message = CommonResource.AssetDowntimeRateFetchedSuccessfully,
                StatusCode = 200
            };
        }

        public async Task<AssetByCategoryPagedResponse> GetAssetsByCategoryAsync()
        {
            _logger.LogInformation("Fetching assets by category data.");

            var assets = (await _assetRepository.GetALLAsync())
                .Where(asset => asset.DeletedDate == null)
                .ToList();

            var categories = (await _masterServiceRepository.GetCategoriesAsync()).ToList();

            if (!assets.Any() || !categories.Any())
            {
                _logger.LogWarning("No data available for assets by category.");
                throw new NotFoundException(CommonResource.NoDataAvailable);
            }

            var totalAssets = assets.Count;

            var categoryBreakdown = categories
                .Select(category =>
                {
                    var assetCount = assets.Count(asset => asset.CategoryID == category.CategoryID);
                    var percentage = totalAssets == 0
                        ? 0m
                        : Math.Round((assetCount / (decimal)totalAssets) * 100m, 2);

                    return new AssetByCategoryResponse
                    {
                        CategoryName = category.CategoryName,
                        AssetCount = assetCount,
                        Percentage = percentage
                    };
                })
                .ToList();

            return new AssetByCategoryPagedResponse
            {
                Data = new List<AssetByCategoryChartResponse>
                {
                    new AssetByCategoryChartResponse
                    {
                        Categories = categoryBreakdown
                    }
                },
                Message = CommonResource.AssetsByCategoryFetchedSuccessfully,
                StatusCode = 200
            };
        }

        public async Task<AssetReportPagedResponse> GetAssetReportListAsync(AssetReportFilterRequest filter)
        {
            int pageSize = CommonResource.PageSize;
            _logger.LogInformation("Fetching asset report list.");
            (string status, string category, string healthStatus) normalizedFilters = await ValidateAndNormalizeReportFiltersAsync(filter);
            var status = normalizedFilters.status;
            var category = normalizedFilters.category;
            var healthStatus = normalizedFilters.healthStatus;
            var reports = await GetAssetReportDataAsync(status, category, healthStatus, filter.StartDate, filter.EndDate);
            if (!reports.Any())
            {
                _logger.LogWarning("No data found for this specific criteria.");
                throw new NotFoundException(CommonResource.NoDataFound);
            }

            bool filterByAssignmentDate = !string.IsNullOrWhiteSpace(status) && status.Trim().Equals(CommonResource.AssignedStatus, StringComparison.OrdinalIgnoreCase);

            var totalRecords = reports.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var pagedData = reports
                .Skip((filter.PageNo - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new AssetReportListResponse
                {
                    assetId = r.AssetId,
                    assetName = r.AssetName ?? string.Empty,
                    categoryName = r.CategoryName ?? string.Empty,
                    serialNumber = r.SerialNumber ?? string.Empty,
                    purchaseDate = FormatReportDate(r, filterByAssignmentDate),
                    assetCost = r.AssetCost,
                    vendorName = r.VendorName ?? string.Empty,
                    assetStatus = r.AssetStatus ?? string.Empty,
                    healthStatus = r.AssetHealthStatus ?? string.Empty,
                    description = r.Description ?? string.Empty,
                    assignedEmployee = r.EmployeeName,
                    propertyName = r.PropertyName
                });

            return new AssetReportPagedResponse
            {
                data = pagedData,
                pageNo = filter.PageNo,
                pageSize = pageSize,
                totalRecords = totalRecords,
                totalPages = totalPages
            };
        }

        public async Task<IEnumerable<ResponseAssetProperty>> GetPropertiesByCategoryIdAsync(byte categoryId)
        {
            if (categoryId == 0)
            {
                _logger.LogWarning("GetPropertiesByCategoryIdAsync called with invalid CategoryId {CategoryId}", categoryId);
                throw new BadRequestException("CategoryId must be greater than zero.");
            }


            var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if (category == null)
            {
                _logger.LogWarning("Category not found. CategoryId: {CategoryId}", categoryId);
                throw new NotFoundException(CommonResource.CategoryNotFound);
            }

            var properties = await _categoryRepository.GetPropertiesByCategoryIdAsync(categoryId);

            if (properties == null || !properties.Any())
            {
                _logger.LogWarning("No properties found for category id {CategoryId}", categoryId);
                return Enumerable.Empty<ResponseAssetProperty>();
            }

            return properties.Select(p => new ResponseAssetProperty
            {
                PropertyId = p.PropertyID,
                PropertyName = p.PropertyName,
                DataTypeID = p.DataTypeID,
                DataType = p.DataType?.DataTypeName ?? string.Empty,
                IsUnique = p.IsUnique,
                IsMandatory = p.IsMandatory,
                PropertyValue = null
            }).ToList();
        }

        public async Task<byte[]> GetAssetReportPdfAsync(AssetReportFileRequest filter)
        {
            _logger.LogInformation("Generating PDF report.");
            (string status, string category, string healthStatus) normalizedFilters = await ValidateAndNormalizeReportFiltersAsync(filter);
            var status = normalizedFilters.status;
            var category = normalizedFilters.category;
            var healthStatus = normalizedFilters.healthStatus;
            var reports = await GetAssetReportDataAsync(status, category, healthStatus, filter.StartDate, filter.EndDate);

            if (!reports.Any())
            {
                _logger.LogWarning("No data found for PDF report generation.");
                throw new NotFoundException(CommonResource.NoDataFound);
            }

            bool filterByAssignmentDate = IsAssignedStatus(status);
            string dateHeader = GetDateHeader(filterByAssignmentDate);

            var reportItems = reports.Select(r => new AssetReportItem
            {
                AssetId = r.AssetId,
                AssetName = r.AssetName,
                CategoryName = r.CategoryName,
                SerialNumber = r.SerialNumber,
                PurchaseDate = r.PurchaseDate,
                AssetCost = r.AssetCost,
                VendorName = r.VendorName,
                AssetStatus = r.AssetStatus,
                Description = r.Description,
                AssignmentID = r.AssignmentID,
                EmployeeName = r.EmployeeName,
                AssignmentDate = r.AssignmentDate,
                ExpectedReturnDate = r.ExpectedReturnDate,
                PropertyName = r.PropertyName
            });

            var pdfData = PdfManager.GenerateAssetReportPdf(reportItems, dateHeader, filterByAssignmentDate);

            if (pdfData.Length == 0)
            {
                _logger.LogWarning("PDF generation resulted in empty data.");
                throw new InternalServerException("PDF generation failed. No data was produced.");
            }

            _logger.LogInformation("Pdf Generated Successfully.");
            return pdfData;
        }

        public async Task<(byte[] Data, string FileName, string ContentType)> GetAssetReportFileAsync(AssetReportFileRequest filter)
        {
            _logger.LogInformation("Generating asset report. Format: {Format}", filter.Format);
            (string status, string category, string healthStatus) normalizedFilters = await ValidateAndNormalizeReportFiltersAsync(filter);
            var status = normalizedFilters.status;
            var category = normalizedFilters.category;
            var healthStatus = normalizedFilters.healthStatus;
            var reports = await GetAssetReportDataAsync(status, category, healthStatus, filter.StartDate, filter.EndDate);

            if (!reports.Any())
            {
                _logger.LogWarning("No data found for asset report generation.");
                throw new NotFoundException(CommonResource.NoDataFound);
            }

            bool filterByAssignmentDate = IsAssignedStatus(status);
            string dateHeader = GetDateHeader(filterByAssignmentDate);

            var resolvedFields = ResolveAssetFields(ParseFields(filter.Fields));

            var reportItems = reports.Select(r => new AssetReportItem
            {
                AssetId = r.AssetId,
                AssetName = r.AssetName,
                CategoryName = r.CategoryName,
                SerialNumber = r.SerialNumber,
                PurchaseDate = r.PurchaseDate,
                AssetCost = r.AssetCost,
                VendorName = r.VendorName,
                AssetStatus = r.AssetStatus,
                AssignmentStatus = r.EmployeeName != null ? "Assigned" : "Unassigned",
                HealthStatus = r.AssetHealthStatus,
                Description = r.Description,
                AssignmentID = r.AssignmentID,
                EmployeeName = r.EmployeeName,
                AssignmentDate = r.AssignmentDate,
                ExpectedReturnDate = r.ExpectedReturnDate,
                PropertyName = r.PropertyName
            }).ToList();

            bool isCsv = filter.Format.Equals("csv", StringComparison.OrdinalIgnoreCase);
            string ext = isCsv ? "csv" : "xlsx";

            string fileName = CommonResource.BuildReportFileName(filter.StartDate, filter.EndDate, "Asset_Report", ext);

            byte[] fileData;
            string contentType;

            if (isCsv)
            {
                fileData = ExcelManager.GenerateAssetReportCsv(reportItems, resolvedFields, dateHeader, filterByAssignmentDate);
                contentType = "text/csv";
            }
            else
            {
                fileData = ExcelManager.GenerateAssetReportExcel(reportItems, resolvedFields, dateHeader, filterByAssignmentDate);
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }

            if (fileData.Length == 0)
            {
                _logger.LogWarning("Asset report generation resulted in empty data.");
                throw new InternalServerException("Report generation failed. No data was produced.");
            }

            _logger.LogInformation("Asset report generated successfully. FileName: {FileName}", fileName);
            return (fileData, fileName, contentType);
        }

        private static IReadOnlyList<string> ResolveAssetFields(IReadOnlyList<string>? fields)
        {
            if (fields == null || !fields.Any())
                return CommonResource.AssetDefaultOptionalFields;

            return fields
                .Where(f => CommonResource.AssetOptionalFields
                    .Any(allowed => allowed.Equals(f, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        private static void ValidateReportDates(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && startDate.Value.Date > DateTime.Today)
            {
                throw new BadRequestException("startDate cannot be a future date.");
            }

            if (endDate.HasValue && endDate.Value.Date > DateTime.Today)
            {
                throw new BadRequestException("endDate cannot be a future date.");
            }

            if (startDate.HasValue && endDate.HasValue && endDate.Value.Date < startDate.Value.Date)
            {
                throw new BadRequestException("endDate cannot be earlier than startDate.");
            }
        }

        private static string NormalizeValue(string? value) => value?.Trim() ?? string.Empty;

        private Task<(string status, string category, string healthStatus)> ValidateAndNormalizeReportFiltersAsync(AssetReportFilterRequest filter)
            => ValidateAndNormalizeReportFiltersAsync(filter.Status, filter.Category, filter.HealthStatus, filter.StartDate, filter.EndDate);

        private Task<(string status, string category, string healthStatus)> ValidateAndNormalizeReportFiltersAsync(AssetReportFileRequest filter)
            => ValidateAndNormalizeReportFiltersAsync(filter.Status, filter.Category, filter.HealthStatus, filter.StartDate, filter.EndDate);

        private async Task<(string status, string category, string healthStatus)> ValidateAndNormalizeReportFiltersAsync(string? status, string? category, string? healthStatus, DateTime? startDate, DateTime? endDate)
        {
            status = string.IsNullOrWhiteSpace(status) ? CommonResource.AllFilter : NormalizeValue(status);
            category = string.IsNullOrWhiteSpace(category) ? CommonResource.AllFilter : NormalizeValue(category);
            healthStatus = string.IsNullOrWhiteSpace(healthStatus) ? CommonResource.AllFilter : NormalizeValue(healthStatus);
            ValidateReportDates(startDate, endDate);

            if (!status.Equals(CommonResource.AllFilter, StringComparison.OrdinalIgnoreCase))
            {
                var statuses = await _masterServiceRepository.GetStatusesAsync();
                var matchedStatus = statuses.FirstOrDefault(s => string.Equals(NormalizeValue(s.AssetStatusName), status, StringComparison.OrdinalIgnoreCase));

                if (matchedStatus != null)
                {
                    status = NormalizeValue(matchedStatus.AssetStatusName);
                }
                else
                {

                    throw new BadRequestException(CommonResource.StatusNotFound);
                }
            }

            if (!category.Equals(CommonResource.AllFilter, StringComparison.OrdinalIgnoreCase))
            {
                var categories = await _masterServiceRepository.GetCategoriesAsync();
                var matchedCategory = categories.FirstOrDefault(c => string.Equals(NormalizeValue(c.CategoryName), category, StringComparison.OrdinalIgnoreCase));

                if (matchedCategory != null)
                {
                    category = NormalizeValue(matchedCategory.CategoryName);
                }
                else
                {

                    throw new BadRequestException(CommonResource.CategoryNotFound);
                }
            }

            if (!healthStatus.Equals(CommonResource.AllFilter, StringComparison.OrdinalIgnoreCase))
            {
                var healthStatuses = await _assetRepository.GetAllHealthStatusNamesAsync();
                var matchedHealthStatus = healthStatuses.FirstOrDefault(h => string.Equals(NormalizeValue(h), healthStatus, StringComparison.OrdinalIgnoreCase));

                if (matchedHealthStatus != null)
                {
                    healthStatus = NormalizeValue(matchedHealthStatus);
                }
                else
                {
                    throw new BadRequestException("Health status not found.");
                }
            }

            return (status, category, healthStatus);
        }

        private static IReadOnlyList<string>? ParseFields(string? fields)
        {
            if (string.IsNullOrWhiteSpace(fields))
            {
                return null;
            }

            return fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        private async Task<IEnumerable<AssetReportData>> GetAssetReportDataAsync(string status, string category, string healthStatus, DateTime? startDate, DateTime? endDate)
        {
            var reportEntities = (await _masterServiceRepository.GetAllAssetsWithAssignmentsAsync()).ToList();
            _logger.LogInformation("Retrieved {Count} total assets from database", reportEntities.Count);

            bool filterByAssignmentDate = IsAssignedStatus(status);
            var filteredEntities = reportEntities.AsEnumerable();

            if (startDate.HasValue || endDate.HasValue)
            {
                filteredEntities = filteredEntities.Where(x => IsWithinDateRange(x, startDate, endDate, filterByAssignmentDate)).ToList();
            }

            if (!status.Equals(CommonResource.AllFilter, StringComparison.OrdinalIgnoreCase))
            {
                filteredEntities = filteredEntities.Where(x =>
                    !string.IsNullOrEmpty(x.AssetStatus) &&
                    string.Equals(NormalizeValue(x.AssetStatus), status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!category.Equals(CommonResource.AllFilter, StringComparison.OrdinalIgnoreCase))
            {
                filteredEntities = filteredEntities.Where(x =>
                    string.Equals(NormalizeValue(x.CategoryName), category, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!healthStatus.Equals(CommonResource.AllFilter, StringComparison.OrdinalIgnoreCase))
            {
                filteredEntities = filteredEntities.Where(x =>
                    !string.IsNullOrEmpty(x.AssetHealthStatus) &&
                    string.Equals(NormalizeValue(x.AssetHealthStatus), healthStatus, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var finalList = filteredEntities.ToList();
            _logger.LogInformation("Final filtered results: {Count} items", finalList.Count);

            return finalList;
        }

        private static bool IsAssignedStatus(string? status) =>
            !string.IsNullOrWhiteSpace(status) &&
            status.Trim().Equals(CommonResource.AssignedStatus, StringComparison.OrdinalIgnoreCase);

        private static string GetDateHeader(bool filterByAssignmentDate) =>
            filterByAssignmentDate ? CommonResource.AssignmentDateHeader : CommonResource.PurchaseDateHeader;

        private static bool IsDowntimeAsset(string? assetHealthStatus, ISet<string> downtimeStatusNames)
        {
            if (string.IsNullOrWhiteSpace(assetHealthStatus))
            {
                return false;
            }

            var normalizedStatus = new string(assetHealthStatus
                .Where(char.IsLetterOrDigit)
                .ToArray())
                .ToLowerInvariant();

            return downtimeStatusNames.Contains(normalizedStatus);
        }

        private static string NormalizeStatus(string? value) => new string((value ?? string.Empty)
            .Where(char.IsLetterOrDigit)
            .ToArray())
            .ToLowerInvariant();

        private static string FormatReportDate(AssetReportData report, bool filterByAssignmentDate) =>
            filterByAssignmentDate
                ? report.AssignmentDate?.ToString(CommonResource.DateFormat) ?? string.Empty
                : report.PurchaseDate?.ToString(CommonResource.DateFormat) ?? string.Empty;

        private static bool IsWithinDateRange(AssetReportData report, DateTime? startDate, DateTime? endDate, bool filterByAssignmentDate)
        {
            var relevantDate = filterByAssignmentDate ? report.AssignmentDate : report.PurchaseDate;
            if (!relevantDate.HasValue)
            {
                return !startDate.HasValue && !endDate.HasValue;
            }

            var actualDate = relevantDate.Value.Date;
            return (!startDate.HasValue || actualDate >= startDate.Value.Date)
                   && (!endDate.HasValue || actualDate <= endDate.Value.Date);
        }
    }
}