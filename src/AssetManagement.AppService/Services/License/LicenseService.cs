using System.ComponentModel;
using System.Linq;
using AssetManagement.AppService.Contracts;
using AssetManagement.AppService.DTOs.License;
using AssetManagement.Infrastructure.Contracts;
using AssetManagement.Infrastructure.Entities.License;
using AssetManagement.Utility.Exceptions;
using AssetManagement.Utility.Report;
using AssetManagement.Utility.Report.Excel;
using AssetManagement.Utility.Resource;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using License = AssetManagement.Infrastructure.Entities.License.License;

namespace AssetManagement.AppService.Services;

public class LicenseService : ILicenseService
{
    private const string DateFormat = "MM/dd/yyyy";
    private readonly ILicenseRepository _licenseRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IMasterServiceRepository _masterServiceRepository;
    private readonly ILogger<LicenseService> _logger;
    public LicenseService(
        ILicenseRepository licenseRepository,
        INotificationRepository notificationRepository,
        IMasterServiceRepository masterServiceRepository,
        ILogger<LicenseService> logger)
    {
        _licenseRepository = licenseRepository;
        _notificationRepository = notificationRepository;
        _masterServiceRepository = masterServiceRepository;
        _logger = logger;
    }
    public async Task<LicensePagedResponse> GetAllLicensesAsync(string? search, string? filter, int pageNo)
    {

        if (pageNo < 1)
        {
            _logger.LogWarning("GetAllLicensesAsync - invalid pageNo: {PageNo}", pageNo);
            throw new BadRequestException(CommonResource.PageNumberInvalid);
        }


        string normalizedFilter = string.IsNullOrWhiteSpace(filter) ? CommonResource.LicenseValidFilters[0] : filter.Trim();

        var matchedFilter = CommonResource.LicenseValidFilters.FirstOrDefault(f => f.Equals(normalizedFilter, StringComparison.OrdinalIgnoreCase));
        if (matchedFilter == null)
        {
            _logger.LogWarning("GetAllLicensesAsync - invalid filter: {Filter}", filter);
            throw new NotFoundException("Filter does not exist.");
        }


        string? normalizedSearch = ValidateSearchParameter(search);

        var (licenses, totalRecords) = await _licenseRepository.GetPaginatedLicensesAsync(normalizedSearch, matchedFilter, pageNo, CommonResource.LicensePageSize);


        if (!string.IsNullOrWhiteSpace(normalizedSearch) && !licenses.Any())
        {
            _logger.LogWarning("GetAllLicensesAsync - no results found for search: {Search}", normalizedSearch);
            throw new NotFoundException("Search does not exist.");
        }

        if (!licenses.Any())
        {
            _logger.LogWarning("GetAllLicensesAsync - no licenses found.");
            throw new NotFoundException("License not found.");
        }

        int totalPages = totalRecords > 0 ? (int)Math.Ceiling(totalRecords / (double)CommonResource.LicensePageSize) : 0;

        var licenseDtos = licenses.Select(l => new GetLicenseRequest
        {
            LicenseId = l.LicenseID,
            LicenseName = l.LicenseName,
            Vendor = l.VendorName,
            PurchaseDate = l.PurchaseDate.ToString(CommonResource.DateFormat),
            ExpiryDate = l.ExpiryDate?.ToString(CommonResource.DateFormat),
            StatusId = l.LicenseStatusID,
            Status = l.LicenseStatus?.LicenseStatusName ?? CommonResource.LicenseUnknownStatus
        }).ToList();

        return new LicensePagedResponse
        {
            Data = licenseDtos,
            PageNo = pageNo,
            PageSize = CommonResource.LicensePageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages,
            Search = normalizedSearch,
            Filter = matchedFilter
        };
    }

    private string? ValidateSearchParameter(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return null;
        }

        string trimmedSearch = search.Trim();

        if (trimmedSearch.Length < CommonResource.LicenseMinSearchLength)
        {
            _logger.LogWarning("ValidateSearchParameter - search parameter is too short: {Search}", search);
            throw new BadRequestException(CommonResource.SearchParameterTooShort);
        }

        if (trimmedSearch.Length > CommonResource.LicenseMaxSearchLength)
        {
            _logger.LogWarning("ValidateSearchParameter - search parameter exceeds max length: {Search}", search);
            throw new BadRequestException(CommonResource.SearchParameterExceedsMaxLength);
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(trimmedSearch, CommonResource.LicenseSearchValidationPattern))
        {
            _logger.LogWarning("ValidateSearchParameter - search contains invalid characters: {Search}", search);
            throw new BadRequestException(CommonResource.SearchParameterInvalidCharacters);
        }

        return trimmedSearch;
    }

    public async Task<GetLicenseDetailsResponse> GetLicenseByIdAsync(int licenseId)
    {
        if (licenseId <= 0)
        {
            _logger.LogWarning("GetLicenseByIdAsync - invalid licenseId: {LicenseId}", licenseId);
            throw new BadRequestException(CommonResource.InvalidLicenseId);
        }

        var license = await _licenseRepository.GetLicenseByIDAsync(licenseId);
        if (license == null)
        {
            _logger.LogWarning("GetLicenseByIdAsync - license not found: {LicenseId}", licenseId);
            throw new NotFoundException(CommonResource.LicenseNotFound);
        }

        var activeAssignments = license.LicenseAssignments?.Where(a => a.IsActive).ToList() ?? new List<LicenseAssignment>();
        var assignedSeats = activeAssignments.Count;
        var availableSeats = license.TotalSeats - assignedSeats;

        var assignmentDetailsDtos = activeAssignments
            .OrderByDescending(a => a.AssignmentDate)
            .Select(a => new AssignmentDetailsDto
            {
                AssignedTo = a.Assignee != null ? $"{a.Assignee.FirstName} {a.Assignee.LastName}".Trim() : string.Empty,
                AssignmentDate = a.AssignmentDate.ToString(CommonResource.DateFormat),
                Department = a.Assignee?.Department?.DepartmentName ?? string.Empty,
                AssignBy = a.AssignedBy
            })
            .ToList();

        return new GetLicenseDetailsResponse
        {
            LicenseId = license.LicenseID,
            LicenseName = license.LicenseName,
            Vendor = license.VendorName,
            LicenseTypeId = license.LicenseTypeID,
            LicenseType = license.LicenseType?.LicenseTypeName ?? string.Empty,
            LicensePurchaseTypeId = license.LicensePurchaseTypeID,
            LicensePurchaseType = license.LicensePurchaseType?.LicensePurchaseTypeName ?? string.Empty,
            MaskedLicenseKey = MaskLicenseKey(license.LicenseKey),
            UnmaskedLicenseKey = license.LicenseKey,
            PurchaseDate = license.PurchaseDate.ToString(CommonResource.DateFormat),
            StartDate = license.StartDate.ToString(CommonResource.DateFormat),
            ExpiryDate = license.ExpiryDate?.ToString(CommonResource.DateFormat),
            Cost = license.Cost,
            Description = license.Description,
            StatusId = license.LicenseStatusID,
            Status = license.LicenseStatus?.LicenseStatusName ?? string.Empty,
            SeatInfo = new SeatInfoDto
            {
                TotalSeats = license.TotalSeats,
                AssignedSeats = assignedSeats,
                AvailableSeats = availableSeats
            },
            AssignmentDetails = assignmentDetailsDtos
        };
    }

    private string? MaskLicenseKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return key;

        if (key.Length <= CommonResource.LicenseMaskedKeyVisibleLength) return key;

        var masked = key.Substring(0, key.Length - CommonResource.LicenseMaskedKeyVisibleLength);
        var visible = key.Substring(key.Length - CommonResource.LicenseMaskedKeyVisibleLength);

        var maskedArray = masked.Select(c => char.IsLetterOrDigit(c) ? '*' : c).ToArray();
        return new string(maskedArray) + visible;
    }
    public async Task<License> CreateLicenseAsync(CreateLicenseRequest request, ClaimsPrincipal user)
    {
        var createdById = GetCurrentUserId(user);
        if (request == null)
        {
            _logger.LogWarning("CreateLicenseAsync — null request");
            throw new ArgumentNullException(nameof(request), CommonResource.InvalidData);
        }

        if (createdById <= 0)
        {
            _logger.LogWarning("CreateLicenseAsync — invalid authenticated user id: {UserId}", createdById);
            throw new UnAuthServiceorizedException("Invalid authenticated user.");
        }

        var licenseTypeName = await ValidateLicenseTypeAsync(request.LicenseTypeID);
        await ValidateLicensePurchaseTypeAsync(request.LicensePurchaseTypeID);
        ValidateUserBasedLicenseSeats(licenseTypeName, request.TotalSeats);
        var (purchaseDateParsed, startDateParsed, expiryDateParsed) = ParseAndValidateDates(request);

        var isRestrictedPurchaseType = await IsRestrictedLicensePurchaseTypeAsync(request.LicensePurchaseTypeID);
        if (isRestrictedPurchaseType && expiryDateParsed.HasValue)
        {
            _logger.LogWarning("CreateLicenseAsync — expiry date provided for restricted purchase type: {PurchaseTypeId}", request.LicensePurchaseTypeID);
            throw new BadRequestException(CommonResource.ExpiryDate_NotAllowedForPermanentLicense);
        }
        var licenseKeyToUse = NormalizeLicenseKey(request.LicenseKey) ?? GenerateUniqueLicenseKey();

        await ValidateLicenseKeyUniquenessAsync(licenseKeyToUse);
        var newStatusId = await GetNewLicenseStatusAsync();

        var license = BuildLicenseEntity(request, createdById, purchaseDateParsed, startDateParsed, expiryDateParsed, newStatusId.Value, licenseKeyToUse);

        if (request.ReminderDays != null && request.ReminderDays.Count > 0)
        {
            if (isRestrictedPurchaseType)
            {
                _logger.LogWarning("CreateLicenseAsync — reminder days provided for restricted purchase type: {PurchaseTypeId}", request.LicensePurchaseTypeID);
                throw new BadRequestException(CommonResource.ReminderDays_NotAllowedForPermanentLicense);
            }

            await ValidateReminderConfigIdsAsync(request.ReminderDays);

            await AddLicenseRemindersAsync(license, request.ReminderDays, createdById);
        }

        var createdLicense = await _licenseRepository.AddAsyncLicense(license);
        _logger.LogInformation("License created successfully. LicenseID: {LicenseID}", createdLicense.LicenseID);
        return createdLicense;
    }

    public async Task<(byte[] Data, string FileName, string ContentType)> GetLicenseReportFileAsync(LicenseReportFileRequest filter)
    {
        _logger.LogInformation("Generating license report. Format: {Format}", filter.Format);

        ValidateReportDates(filter.StartDate, filter.EndDate);
        var status = NormalizeValue(filter.Filter);
        var search = ValidateSearchParameter(filter.Search);

        var licenses = (await _licenseRepository.GetLicensesWithAssignmentsAsync(filter.StartDate, filter.EndDate, status, search)).ToList();
        if (!licenses.Any())
        {
            _logger.LogWarning("No data found for license report generation.");
            throw new NotFoundException("No data found for license report generation.");
        }

        var resolvedFields = ResolveLicenseFields(ParseFields(filter.Fields));

        var reportItems = licenses.Select(l =>
        {
            var activeAssignments = l.LicenseAssignments
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.AssignmentDate)
                .ToList();

            var assignedEmployees = string.Join(", ", activeAssignments
                .Where(a => a.Assignee != null)
                .Select(a => $"{a.Assignee!.FirstName} {a.Assignee.LastName}".Trim()));

            int availableSeats = l.TotalSeats - activeAssignments.Count;

            var lastAssignment = activeAssignments.FirstOrDefault();

            return new LicenseReportItem
            {
                LicenseId = l.LicenseID,
                LicenseName = l.LicenseName,
                LicenseType = l.LicenseType?.LicenseTypeName ?? string.Empty,
                PurchaseType = l.LicensePurchaseType?.LicensePurchaseTypeName ?? string.Empty,
                TotalSeats = l.TotalSeats,
                AvailableSeats = availableSeats,
                VendorName = l.VendorName ?? string.Empty,
                PurchaseDate = l.PurchaseDate,
                StartDate = l.StartDate,
                ExpiryDate = l.ExpiryDate,
                LicenseStatus = l.LicenseStatus?.LicenseStatusName ?? string.Empty,
                LicenseKey = l.LicenseKey ?? string.Empty,
                Cost = l.Cost,
                Description = l.Description ?? string.Empty,
                AssignedEmployees = assignedEmployees,
                AssignmentDate = lastAssignment?.AssignmentDate,
                AssignedBy = lastAssignment?.AssignedBy ?? string.Empty,
                UnassignedDate = lastAssignment?.UnassignedDate,
                UnassignedBy = lastAssignment?.UnassignedBy ?? string.Empty
            };
        }).ToList();

        bool isCsv = filter.Format.Equals("csv", StringComparison.OrdinalIgnoreCase);
        string ext = isCsv ? "csv" : "xlsx";
        string fileName = CommonResource.BuildReportFileName(filter.StartDate, filter.EndDate, "License_Report", ext);

        byte[] fileData;
        string contentType;

        if (isCsv)
        {
            fileData = ExcelManager.GenerateLicenseReportCsv(reportItems, resolvedFields);
            contentType = "text/csv";
        }
        else
        {
            fileData = ExcelManager.GenerateLicenseReportExcel(reportItems, resolvedFields);
            contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        }

        if (fileData.Length == 0)
        {
            _logger.LogWarning("License report generation resulted in empty data.");
            throw new InternalServerException("Report generation failed. No data was produced.");
        }

        _logger.LogInformation("License report generated successfully. FileName: {FileName}", fileName);
        return (fileData, fileName, contentType);
    }

    private static IReadOnlyList<string> ResolveLicenseFields(IReadOnlyList<string>? fields)
    {
        if (fields == null || !fields.Any())
            return CommonResource.LicenseDefaultOptionalFields;

        return fields
            .Where(f => CommonResource.LicenseOptionalFields
                .Any(allowed => allowed.Equals(f, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    private static IReadOnlyList<string>? ParseFields(string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
        {
            return null;
        }

        return fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }



    private async Task<string> ValidateLicenseTypeAsync(byte licenseTypeId)
    {
        var licenseTypeExists = await _licenseRepository.LicenseTypeExistsAsync(licenseTypeId);
        if (!licenseTypeExists)
        {
            _logger.LogWarning("CreateLicenseAsync — invalid license type: {LicenseTypeID}", licenseTypeId);
            throw new BadRequestException(CommonResource.InvalidLicenseType);
        }

        var licenseTypeName = await _licenseRepository.GetLicenseTypeNameAsync(licenseTypeId);
        if (string.IsNullOrWhiteSpace(licenseTypeName))
        {
            _logger.LogWarning("CreateLicenseAsync — license type name not found: {LicenseTypeID}", licenseTypeId);
            throw new BadRequestException(CommonResource.InvalidLicenseType);
        }

        return licenseTypeName;
    }

    private async Task ValidateLicensePurchaseTypeAsync(byte licensePurchaseTypeId)
    {
        var licensePurchaseTypeExists = await _licenseRepository.LicensePurchaseTypeExistsAsync(licensePurchaseTypeId);
        if (!licensePurchaseTypeExists)
        {
            _logger.LogWarning("CreateLicenseAsync — invalid license purchase type: {LicensePurchaseTypeID}", licensePurchaseTypeId);
            throw new BadRequestException(CommonResource.InvalidLicensePurchaseType);
        }
    }

    private async Task<bool> IsRestrictedLicensePurchaseTypeAsync(byte licensePurchaseTypeId)
    {
        try
        {
            var purchaseTypes = await _masterServiceRepository.GetLicensePurchaseTypesAsync();
            var purchaseType = purchaseTypes.FirstOrDefault(p => p.LicensePurchaseTypeID == licensePurchaseTypeId);
            if (purchaseType == null) return false;

            var normalizedName = new string((purchaseType.LicensePurchaseTypeName ?? string.Empty)
                .Where(char.IsLetterOrDigit)
                .ToArray())
                .ToLowerInvariant();

            return normalizedName == "permanent" || normalizedName == "onetimelicense" || normalizedName == "onetime";
        }
        catch
        {
            return false;
        }
    }

    private async Task ValidateReminderConfigIdsAsync(List<ReminderConfigRequest> reminderConfigs)
    {
        foreach (var config in reminderConfigs)
        {
            var reminderConfigExists = await _licenseRepository.ReminderConfigExistsAsync(config.ReminderConfigTypeID);
            if (!reminderConfigExists)
            {
                _logger.LogWarning("Reminder configuration not found: {ReminderConfigID}", config.ReminderConfigTypeID);
                throw new BadRequestException(CommonResource.InvalidReminderConfigId);
            }
        }
    }

    private void ValidateUserBasedLicenseSeats(string licenseTypeName, double? totalSeats)
    {
        if (licenseTypeName.Equals("User Based", System.StringComparison.OrdinalIgnoreCase) && totalSeats.HasValue && totalSeats.Value != 1)
        {
            _logger.LogWarning("CreateLicenseAsync — User-Based license type with invalid seats: {TotalSeats}", totalSeats);
            throw new BadRequestException(CommonResource.UserBasedLicenseSeatsError);
        }
    }

    private (DateTime purchaseDate, DateTime startDate, DateTime? expiryDate) ParseAndValidateDates(CreateLicenseRequest request)
    {
        if (!DateTime.TryParseExact(request.PurchaseDate, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var purchaseDateParsed))
        {
            _logger.LogWarning("CreateLicenseAsync — invalid purchase date format: {PurchaseDate}", request.PurchaseDate);
            throw new BadRequestException("Purchase date must be in MM/DD/YYYY format.");
        }

        if (!DateTime.TryParseExact(request.StartDate, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var startDateParsed))
        {
            _logger.LogWarning("CreateLicenseAsync — invalid start date format: {StartDate}", request.StartDate);
            throw new BadRequestException("Start date must be in MM/DD/YYYY format.");
        }

        DateTime? expiryDateParsed = null;
        if (!string.IsNullOrWhiteSpace(request.ExpiryDate))
        {
            if (!DateTime.TryParseExact(request.ExpiryDate, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var expiryDate))
            {
                _logger.LogWarning("CreateLicenseAsync — invalid expiry date format: {ExpiryDate}", request.ExpiryDate);
                throw new BadRequestException("Expiry date must be in MM/DD/YYYY format.");
            }
            expiryDateParsed = expiryDate;
        }

        return (purchaseDateParsed, startDateParsed, expiryDateParsed);
    }

    private async Task ValidateLicenseKeyUniquenessAsync(string? licenseKey)
    {
        if (!string.IsNullOrWhiteSpace(licenseKey))
        {
            var normalizedLicenseKey = NormalizeLicenseKey(licenseKey)!;
            var keyExists = await _licenseRepository.LicenseKeyExistsAsync(normalizedLicenseKey);
            if (keyExists)
            {
                _logger.LogWarning("CreateLicenseAsync — duplicate license key: {LicenseKey}", normalizedLicenseKey);
                throw new BadRequestException(CommonResource.LicenseKeyAlreadyExists);
            }
        }
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

    private static string? NormalizeValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
    private async Task<byte?> GetNewLicenseStatusAsync()
    {
        var newStatusId = await _licenseRepository.GetNewStatusIdAsync();
        if (!newStatusId.HasValue)
        {
            _logger.LogWarning("CreateLicenseAsync — New status not found");
            throw new NotFoundException(CommonResource.LicenseStatusNotFound);
        }
        return newStatusId;
    }

    private License BuildLicenseEntity(CreateLicenseRequest request, int createdById, DateTime purchaseDate, DateTime startDate, DateTime? expiryDate, byte statusId, string licenseKey)
    {
        return new License
        {
            LicenseName = request.LicenseName.Trim(),
            LicenseTypeID = request.LicenseTypeID,
            LicensePurchaseTypeID = request.LicensePurchaseTypeID,
            TotalSeats = (byte)request.TotalSeats!.Value,
            VendorName = string.IsNullOrWhiteSpace(request.VendorName) ? null : request.VendorName.Trim(),

            PurchaseDate = purchaseDate,
            StartDate = startDate,
            ExpiryDate = expiryDate,
            LicenseStatusID = statusId,
            LicenseKey = licenseKey,
            Cost = request.Cost!.Value,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            CreatedByID = createdById,
            CreatedDate = DateTime.UtcNow
        };
    }

    private async Task AddLicenseRemindersAsync(License license, List<ReminderConfigRequest> reminderConfigs, int createdById)
    {
        foreach (var config in reminderConfigs)
        {
            license.LicenseReminders.Add(new LicenseReminder
            {
                ReminderConfigID = config.ReminderConfigTypeID,
                CreatedDate = DateTime.UtcNow,
                CreatedByID = createdById
            });
        }
        await Task.CompletedTask;
    }

    public async Task AssignLicenseAsync(int licenseId, AssignLicenseRequest request, ClaimsPrincipal user)
    {
        var assignedById = GetCurrentUserId(user);
        var assigneeIds = request?.Employees?.Select(employee => employee.UserId).ToList() ?? new List<int>();

        if (request == null || assigneeIds.Count == 0)
        {
            _logger.LogWarning("AssignLicenseAsync — invalid request or empty employees list");
            throw new BadRequestException("Select Employee");
        }


        var license = await _licenseRepository.GetLicenseByIdAsync(licenseId);
        if (license == null)
        {
            _logger.LogWarning("AssignLicenseAsync — license not found: {LicenseID}", licenseId);
            throw new NotFoundException("License not found.");
        }

        await EnsureLicenseAssignableAsync(license);

        var hasAvailableSeats = await _licenseRepository.CheckAvailableSeatsAsync(licenseId, assigneeIds.Count);
        if (!hasAvailableSeats)
        {
            _logger.LogWarning("AssignLicenseAsync — insufficient seats: {LicenseID}", licenseId);
            throw new BadRequestException("Employees exceed available seats");
        }


        foreach (var employeeId in assigneeIds)
        {
            await ValidateUserExistsAndActiveAsync(
                employeeId,
                "AssignLicenseAsync — user not found: {UserId}",
                "AssignLicenseAsync — user is inactive: {UserId}",
                new BadRequestException($"User with ID {employeeId} does not exist."),
                new BadRequestException($"User with ID {employeeId} is inactive and cannot be assigned."));


            var alreadyAssigned = await _licenseRepository.IsUserAlreadyAssignedAsync(licenseId, employeeId);
            if (alreadyAssigned)
            {
                _logger.LogWarning("AssignLicenseAsync — user already assigned: {UserId}", employeeId);
                throw new BadRequestException($"User with ID {employeeId} is already assigned to this license.");
            }


            var assignment = new LicenseAssignment
            {
                LicenseID = licenseId,
                AssigneeID = employeeId,
                AssignmentDate = DateTime.UtcNow,
                AssignedBy = assignedById.ToString(),
                IsActive = true,
                CreatedByID = assignedById,
                CreatedDate = DateTime.UtcNow
            };

            await _licenseRepository.AddLicenseAssignmentAsync(assignment);
        }

        _logger.LogInformation("License assigned successfully. LicenseID: {LicenseID}, AssignmentCount: {Count}", licenseId, request.Employees.Count);
    }

    private async Task EnsureLicenseAssignableAsync(License license)
    {
        var expiredStatusId = await _licenseRepository.GetStatusIdByNameAsync(CommonResource.ExpiredLicenseStatus);
        var isExpiredByStatus = expiredStatusId.HasValue && license.LicenseStatusID == expiredStatusId.Value;
        var isExpiredByDate = license.ExpiryDate.HasValue && license.ExpiryDate.Value.Date <= DateTime.UtcNow.Date;

        if (isExpiredByStatus || isExpiredByDate)
        {
            _logger.LogWarning("AssignLicenseAsync — expired license cannot be assigned: {LicenseID}", license.LicenseID);
            throw new BadRequestException(CommonResource.LicenseExpiredCannotBeAssigned);
        }
    }

    private async Task ValidateUserExistsAndActiveAsync(int userId, string notFoundLogMessage, string inactiveLogMessage, Exception notFoundException, Exception inactiveException)
    {
        var userExists = await _licenseRepository.UserExistsAsync(userId);
        if (!userExists)
        {
            _logger.LogWarning(notFoundLogMessage, userId);
            throw notFoundException;
        }

        var isUserActive = await _licenseRepository.IsUserActiveAsync(userId);
        if (!isUserActive)
        {
            _logger.LogWarning(inactiveLogMessage, userId);
            throw inactiveException;
        }
    }

    public async Task UnassignLicenseAsync(int licenseId, UnassignLicenseRequest request, ClaimsPrincipal user)
    {
        var unassignedById = GetCurrentUserId(user);

        _logger.LogInformation("UnassignLicenseAsync called for license ID: {LicenseID}, UserId: {UserId}", licenseId, request.UserId);


        var license = await _licenseRepository.GetLicenseByIdAsync(licenseId);
        if (license == null)
        {
            _logger.LogWarning("UnassignLicenseAsync — license not found: {LicenseID}", licenseId);
            throw new NotFoundException("License not found.");
        }


        var assignment = await _licenseRepository.GetLicenseAssignmentAsync(licenseId, request.UserId);
        if (assignment == null)
        {
            _logger.LogWarning("UnassignLicenseAsync — user is not assigned: {UserId}", request.UserId);
            throw new BadRequestException("User is not assigned to this license.");
        }


        await _licenseRepository.UnassignLicenseAsync(licenseId, request.UserId, unassignedById);

        _logger.LogInformation("License unassigned successfully. LicenseID: {LicenseID}, UserId: {UserId}", licenseId, request.UserId);
    }

    public async Task UpdateLicenseAsync(int licenseId, UpdateLicenseRequest request, ClaimsPrincipal user)
    {
        var modifiedById = GetCurrentUserId(user);

        if (request == null)
        {
            _logger.LogWarning("UpdateLicenseAsync — null request");
            throw new ArgumentNullException(nameof(request), CommonResource.InvalidData);
        }

        if (modifiedById <= 0)
        {
            _logger.LogWarning("UpdateLicenseAsync — invalid authenticated user id: {UserId}", modifiedById);
            throw new UnAuthServiceorizedException("Invalid authenticated user.");
        }


        var license = await _licenseRepository.GetLicenseByIdAsync(licenseId);
        if (license == null)
        {
            _logger.LogWarning("UpdateLicenseAsync — license not found: {LicenseID}", licenseId);
            throw new NotFoundException("License not found.");
        }

        var licenseTypeName = await ValidateLicenseTypeAsync(request.LicenseTypeID);
        await ValidateLicensePurchaseTypeAsync(request.LicensePurchaseTypeID);


        var assignedSeatsCount = await _licenseRepository.GetAssignedSeatsCountAsync(licenseId);
        if (request.TotalSeats.HasValue && request.TotalSeats.Value < assignedSeatsCount)
        {
            _logger.LogWarning("UpdateLicenseAsync — total seats less than assigned: {LicenseID}, TotalSeats: {TotalSeats}, AssignedSeats: {AssignedSeats}",
                licenseId, request.TotalSeats, assignedSeatsCount);
            throw new BadRequestException("Total seats cannot be less than assigned seats.");
        }

        ValidateUserBasedLicenseSeats(licenseTypeName, request.TotalSeats);


        var (purchaseDateParsed, startDateParsed, expiryDateParsed) = ParseAndValidateDatesForUpdate(request);


        var normalizedRequestKey = NormalizeLicenseKey(request.LicenseKey);
        var licenseKeyToUse = normalizedRequestKey ?? license.LicenseKey;


        if (!string.Equals(license.LicenseKey, normalizedRequestKey, System.StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(normalizedRequestKey))
        {
            await ValidateLicenseKeyUniquenessForUpdateAsync(normalizedRequestKey, licenseId);
        }

        byte statusId = await DetermineLicenseStatusAsync(startDateParsed, expiryDateParsed);

        license.LicenseName = request.LicenseName.Trim();
        license.LicenseTypeID = request.LicenseTypeID;
        license.LicensePurchaseTypeID = request.LicensePurchaseTypeID;
        license.TotalSeats = (byte)request.TotalSeats!.Value;
        license.VendorName = string.IsNullOrWhiteSpace(request.VendorName) ? null : request.VendorName.Trim();
        license.PurchaseDate = purchaseDateParsed;
        license.StartDate = startDateParsed;
        license.ExpiryDate = expiryDateParsed;
        license.LicenseStatusID = statusId;
        license.LicenseKey = licenseKeyToUse;
        license.Cost = request.Cost!.Value;
        license.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        license.ModifiedByID = modifiedById;
        license.ModifiedDate = DateTime.UtcNow;


        license.LicenseReminders.Clear();

        if (request.ReminderDays != null && request.ReminderDays.Count > 0)
        {
            await ValidateReminderConfigIdsAsync(request.ReminderDays);
            foreach (var config in request.ReminderDays)
            {
                license.LicenseReminders.Add(new LicenseReminder
                {
                    LicenseID = licenseId,
                    ReminderConfigID = config.ReminderConfigTypeID,
                    CreatedDate = DateTime.UtcNow,
                    CreatedByID = modifiedById
                });
            }
        }

        await _licenseRepository.UpdateLicenseAsync(license);
        _logger.LogInformation("License updated successfully. LicenseID: {LicenseID}", licenseId);
    }

    public async Task<LicenseRenewalResponse> RenewLicenseAsync(int licenseId, RenewLicenseRequest request, ClaimsPrincipal user)
    {
        var renewedById = GetCurrentUserId(user);

        var license = await ValidateRenewLicenseGuardsAsync(licenseId, request, renewedById);

        var (renewalDateParsed, expiryDateParsed) = ParseAndValidateRenewalDates(request);

        ValidateRenewalDatesAgainstLicense(licenseId, renewalDateParsed, license.ExpiryDate!.Value);

        await ValidateUpdatedSeatsAsync(licenseId, request.UpdatedTotalSeats, methodName: nameof(RenewLicenseAsync));

        string? normalizedLicenseKey = NormalizeLicenseKey(request.UpdatedLicenseKey);
        await ValidateRenewalLicenseKeyAsync(license.LicenseKey, normalizedLicenseKey, licenseId);

        var renewal = BuildLicenseRenewal(licenseId, request, renewalDateParsed, expiryDateParsed, normalizedLicenseKey, renewedById);
        await _licenseRepository.AddLicenseRenewalAsync(renewal);
        await ClearQueuedExpiringNotificationsAsync(licenseId, renewedById);

        _logger.LogInformation("License renewed successfully. LicenseID: {LicenseID}, RenewalID: {RenewalID}", licenseId, renewal.LicenseRenewalID);

        return BuildRenewalResponse(renewal.LicenseRenewalID, licenseId, renewalDateParsed, expiryDateParsed, request, normalizedLicenseKey, renewal.RenewalNotes);
    }

    public async Task<LicenseRenewalResponse> UpdateLicenseRenewalAsync(int renewalId, RenewLicenseRequest request, ClaimsPrincipal user)
    {
        var modifiedById = GetCurrentUserId(user);

        var (license, renewal) = await ValidateUpdateRenewalGuardsAsync(renewalId, request, modifiedById);

        var licenseId = request.LicenseId;
        var (renewalDateParsed, expiryDateParsed) = ParseAndValidateRenewalDates(request);
        DateTime? currentExpiry = license.ExpiryDate ?? renewal.ExpiryDate;
        if (!currentExpiry.HasValue)
        {
            throw new BadRequestException(CommonResource.LicenseExpiryDateRequiredForRenewal);
        }

        ValidateRenewalDatesAgainstLicense(licenseId, renewalDateParsed, currentExpiry.Value);

        await ValidateUpdatedSeatsAsync(licenseId, request.UpdatedTotalSeats, methodName: nameof(UpdateLicenseRenewalAsync), renewalId: renewalId);

        string? normalizedLicenseKey = NormalizeLicenseKey(request.UpdatedLicenseKey);
        await ValidateRenewalLicenseKeyAsync(renewal.UpdatedLicenseKey, normalizedLicenseKey, licenseId);

        ApplyRenewalFieldsToRenewalRecord(renewal, request, renewalDateParsed, expiryDateParsed, normalizedLicenseKey, modifiedById);
        await _licenseRepository.UpdateLicenseRenewalAsync(renewal);
        await ClearQueuedExpiringNotificationsAsync(licenseId, modifiedById);

        _logger.LogInformation("License renewal updated successfully. LicenseID: {LicenseID}, RenewalID: {RenewalID}", licenseId, renewalId);

        return BuildRenewalResponse(renewal.LicenseRenewalID, licenseId, renewalDateParsed, expiryDateParsed, request, normalizedLicenseKey, renewal.RenewalNotes);
    }

    private async Task<License> ValidateRenewLicenseGuardsAsync(int licenseId, RenewLicenseRequest request, int renewedById)
    {
        if (request == null)
        {
            _logger.LogWarning("RenewLicenseAsync — null request");
            throw new ArgumentNullException(nameof(request), CommonResource.InvalidData);
        }

        if (renewedById <= 0)
        {
            _logger.LogWarning("RenewLicenseAsync — invalid authenticated user id: {UserId}", renewedById);
            throw new UnAuthServiceorizedException(CommonResource.InvalidAuthenticatedUser);
        }

        var license = await _licenseRepository.GetLicenseByIdAsync(licenseId);
        if (license == null)
        {
            _logger.LogWarning("RenewLicenseAsync — license not found: {LicenseID}", licenseId);
            throw new NotFoundException(CommonResource.LicenseNotFound);
        }

        if (!license.ExpiryDate.HasValue)
        {
            _logger.LogWarning("RenewLicenseAsync — license missing expiry date: {LicenseID}", licenseId);
            throw new BadRequestException(CommonResource.LicenseExpiryDateRequiredForRenewal);
        }

        return license;
    }

    private async Task<(License license, LicenseRenewal renewal)> ValidateUpdateRenewalGuardsAsync(int renewalId, RenewLicenseRequest request, int modifiedById)
    {
        if (request == null)
        {
            _logger.LogWarning("UpdateLicenseRenewalAsync — null request");
            throw new ArgumentNullException(nameof(request), CommonResource.InvalidData);
        }

        if (request.LicenseId <= 0)
        {
            _logger.LogWarning("UpdateLicenseRenewalAsync — invalid license id in request: {LicenseId}", request.LicenseId);
            throw new BadRequestException(CommonResource.InvalidLicenseId);
        }

        if (modifiedById <= 0)
        {
            _logger.LogWarning("UpdateLicenseRenewalAsync — invalid authenticated user id: {UserId}", modifiedById);
            throw new UnAuthServiceorizedException(CommonResource.InvalidAuthenticatedUser);
        }

        var license = await _licenseRepository.GetLicenseByIdAsync(request.LicenseId);
        if (license == null)
        {
            _logger.LogWarning("UpdateLicenseRenewalAsync — license not found: {LicenseID}", request.LicenseId);
            throw new NotFoundException(CommonResource.LicenseNotFound);
        }

        var renewal = await _licenseRepository.GetLicenseRenewalByIdAsync(renewalId);
        if (renewal == null)
        {
            _logger.LogWarning("UpdateLicenseRenewalAsync — renewal not found: {RenewalID}", renewalId);
            throw new NotFoundException("License renewal not found.");
        }

        if (renewal.LicenseID != request.LicenseId)
        {
            _logger.LogWarning("UpdateLicenseRenewalAsync — renewal does not belong to license. LicenseID: {LicenseID}, RenewalID: {RenewalID}", request.LicenseId, renewalId);
            throw new BadRequestException("The renewal record does not belong to the specified license.");
        }

        return (license, renewal);
    }

    private void ValidateRenewalDatesAgainstLicense(int licenseId, DateTime renewalDate, DateTime currentExpiry)
    {
        var today = DateTime.UtcNow.Date;
        var expiryDate = currentExpiry.Date;
        var dayAfterExpiry = expiryDate.AddDays(1);

        if (today <= expiryDate && renewalDate.Date != dayAfterExpiry)
        {
            _logger.LogWarning("RenewLicenseAsync — renewal date must be the day after current expiry date when renewing before expiry. LicenseID: {LicenseID}, CurrentExpiry: {ExpiryDate}, RenewalDate: {RenewalDate}",
                licenseId, expiryDate, renewalDate.Date);
            throw new BadRequestException(CommonResource.RenewalDateMustBeDayAfterExpiryBeforeExpiry);
        }

        if (renewalDate.Date <= expiryDate)
        {
            _logger.LogWarning("RenewLicenseAsync — renewal date must be after current expiry date. LicenseID: {LicenseID}, CurrentExpiry: {ExpiryDate}, RenewalDate: {RenewalDate}",
                 licenseId, expiryDate, renewalDate.Date);
            throw new BadRequestException(CommonResource.RenewalDateMustBeAfterExpiryDate);
        }

        if (renewalDate.Date > expiryDate.AddYears(2))
        {
            _logger.LogWarning("RenewLicenseAsync — renewal date exceeded allowed window. LicenseID: {LicenseID}, CurrentExpiry: {ExpiryDate}, RenewalDate: {RenewalDate}",
                licenseId, expiryDate, renewalDate.Date);
            throw new BadRequestException(CommonResource.LicenseRenewalWindowExceeded);
        }
    }


    private async Task ValidateUpdatedSeatsAsync(int licenseId, int? updatedTotalSeats, string methodName, int? renewalId = null)
    {
        if (!updatedTotalSeats.HasValue) return;

        var assignedSeatsCount = await _licenseRepository.GetAssignedSeatsCountAsync(licenseId);
        if (updatedTotalSeats.Value < assignedSeatsCount)
        {
            if (renewalId.HasValue)
                _logger.LogWarning("{Method} — updated total seats less than assigned seats. LicenseID: {LicenseID}, RenewalID: {RenewalID}, UpdatedTotalSeats: {TotalSeats}, AssignedSeats: {AssignedSeats}",
                    methodName, licenseId, renewalId.Value, updatedTotalSeats.Value, assignedSeatsCount);
            else
                _logger.LogWarning("{Method} — updated total seats less than assigned seats. LicenseID: {LicenseID}, UpdatedTotalSeats: {TotalSeats}, AssignedSeats: {AssignedSeats}",
                    methodName, licenseId, updatedTotalSeats.Value, assignedSeatsCount);

            throw new BadRequestException("Total seats cannot be less than assigned seats.");
        }
    }


    private async Task ValidateRenewalLicenseKeyAsync(string? currentKey, string? newNormalizedKey, int licenseId)
    {
        if (!string.IsNullOrWhiteSpace(newNormalizedKey) &&
            !string.Equals(currentKey, newNormalizedKey, StringComparison.OrdinalIgnoreCase))
        {
            await ValidateLicenseKeyUniquenessForUpdateAsync(newNormalizedKey, licenseId);
        }
    }


    private static LicenseRenewal BuildLicenseRenewal(int licenseId, RenewLicenseRequest request,
        DateTime renewalDate, DateTime expiryDate, string? normalizedLicenseKey, int createdById) =>
        new LicenseRenewal
        {
            LicenseID = licenseId,
            LicenseRenewalDate = renewalDate.Date,
            ExpiryDate = expiryDate.Date,
            UpdatedCost = request.UpdatedCost,
            UpdatedTotalSeats = request.UpdatedTotalSeats,
            UpdatedLicenseKey = normalizedLicenseKey,
            RenewalNotes = string.IsNullOrWhiteSpace(request.RenewalNotes) ? null : request.RenewalNotes.Trim(),
            CreatedByID = createdById,
            CreatedDate = DateTime.UtcNow
        };


    private static void ApplyRenewalFieldsToRenewalRecord(LicenseRenewal renewal, RenewLicenseRequest request,
        DateTime renewalDate, DateTime expiryDate, string? normalizedLicenseKey, int modifiedById)
    {
        renewal.LicenseRenewalDate = renewalDate.Date;
        renewal.ExpiryDate = expiryDate.Date;
        renewal.UpdatedCost = request.UpdatedCost;
        renewal.UpdatedTotalSeats = request.UpdatedTotalSeats;
        renewal.UpdatedLicenseKey = normalizedLicenseKey;
        renewal.RenewalNotes = string.IsNullOrWhiteSpace(request.RenewalNotes) ? null : request.RenewalNotes.Trim();
        renewal.ModifiedByID = modifiedById;
        renewal.ModifiedDate = DateTime.UtcNow;
    }

    private LicenseRenewalResponse BuildRenewalResponse(int renewalId, int licenseId,
        DateTime renewalDate, DateTime expiryDate, RenewLicenseRequest request,
        string? normalizedLicenseKey, string? renewalNotes) =>
        new LicenseRenewalResponse
        {
            RenewalId = renewalId,
            LicenseId = licenseId,
            RenewalDate = renewalDate.ToString(DateFormat),
            ExpiryDate = expiryDate.ToString(DateFormat),
            UpdatedTotalSeats = request.UpdatedTotalSeats,
            UpdatedCost = request.UpdatedCost,
            LicenseKey = normalizedLicenseKey,
            RenewalNotes = renewalNotes
        };


    private (DateTime renewalDate, DateTime expiryDate) ParseAndValidateRenewalDates(RenewLicenseRequest request)
    {
        if (!DateTime.TryParseExact(request.RenewalDate, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var renewalDateParsed))
        {
            _logger.LogWarning("RenewLicenseAsync — invalid renewal date format: {RenewalDate}", request.RenewalDate);
            throw new BadRequestException(CommonResource.InvalidDateFormat);
        }

        if (!DateTime.TryParseExact(request.ExpiryDate, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var expiryDateParsed))
        {
            _logger.LogWarning("RenewLicenseAsync — invalid expiry date format: {ExpiryDate}", request.ExpiryDate);
            throw new BadRequestException(CommonResource.InvalidDateFormat);
        }

        if (expiryDateParsed.Date <= renewalDateParsed.Date)
        {
            _logger.LogWarning("RenewLicenseAsync — expiry date must be after renewal date. RenewalDate: {RenewalDate}, ExpiryDate: {ExpiryDate}", renewalDateParsed.Date, expiryDateParsed.Date);
            throw new BadRequestException("Expiry date must be after renewal date.");
        }

        return (renewalDateParsed.Date, expiryDateParsed.Date);
    }

    private async Task<byte> DetermineLicenseStatusAsync(DateTime startDate, DateTime? expiryDate)
    {
        var today = DateTime.UtcNow.Date;


        if (expiryDate.HasValue && today >= expiryDate.Value.Date)
        {
            var expiredStatusId = await _licenseRepository.GetStatusIdByNameAsync("Expired");
            return expiredStatusId.HasValue ? expiredStatusId.Value : throw new NotFoundException("License status 'Expired' not found.");
        }

        if (today >= startDate.Date)
        {
            var activeStatusId = await _licenseRepository.GetStatusIdByNameAsync("Active");
            return activeStatusId.HasValue ? activeStatusId.Value : throw new NotFoundException("License status 'Active' not found.");
        }

        var newStatusId = await _licenseRepository.GetStatusIdByNameAsync("New");
        return newStatusId.HasValue ? newStatusId.Value : throw new NotFoundException("License status 'New' not found.");
    }

    private async Task ValidateLicenseKeyUniquenessForUpdateAsync(string? licenseKey, int licenseId)
    {
        if (!string.IsNullOrWhiteSpace(licenseKey))
        {
            var normalizedLicenseKey = NormalizeLicenseKey(licenseKey)!;
            var keyExists = await _licenseRepository.LicenseKeyExistsForOtherLicenseAsync(normalizedLicenseKey, licenseId);
            if (keyExists)
            {
                _logger.LogWarning("UpdateLicenseAsync — duplicate license key: {LicenseKey}", normalizedLicenseKey);
                throw new BadRequestException(CommonResource.LicenseKeyAlreadyExists);
            }
        }
    }

    private static string? NormalizeLicenseKey(string? licenseKey)
    {
        return string.IsNullOrWhiteSpace(licenseKey) ? null : licenseKey.Trim().ToUpperInvariant();
    }

    private (DateTime purchaseDate, DateTime startDate, DateTime? expiryDate) ParseAndValidateDatesForUpdate(UpdateLicenseRequest request)
    {
        if (!DateTime.TryParseExact(request.PurchaseDate, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var purchaseDateParsed))
        {
            _logger.LogWarning("UpdateLicenseAsync — invalid purchase date format: {PurchaseDate}", request.PurchaseDate);
            throw new BadRequestException("Purchase date must be in MM/DD/YYYY format.");
        }

        if (!DateTime.TryParseExact(request.StartDate, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var startDateParsed))
        {
            _logger.LogWarning("UpdateLicenseAsync — invalid start date format: {StartDate}", request.StartDate);
            throw new BadRequestException("Start date must be in MM/DD/YYYY format.");
        }

        DateTime? expiryDateParsed = null;
        if (!string.IsNullOrWhiteSpace(request.ExpiryDate))
        {
            if (!DateTime.TryParseExact(request.ExpiryDate, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var expiryDate))
            {
                _logger.LogWarning("UpdateLicenseAsync — invalid expiry date format: {ExpiryDate}", request.ExpiryDate);
                throw new BadRequestException("Expiry date must be in MM/DD/YYYY format.");
            }
            expiryDateParsed = expiryDate;
        }

        if (expiryDateParsed.HasValue && expiryDateParsed.Value.Date <= startDateParsed.Date)
        {
            _logger.LogWarning("UpdateLicenseAsync — expiry date must be after start date: {ExpiryDate} <= {StartDate}", request.ExpiryDate, request.StartDate);
            throw new BadRequestException(CommonResource.ExpiryDate_BeforeStartDate);
        }

        return (purchaseDateParsed, startDateParsed, expiryDateParsed);
    }

    private static string GenerateUniqueLicenseKey()
    {
        var guid = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();
        return $"LIC-{guid}";
    }

    private async Task ClearQueuedExpiringNotificationsAsync(int licenseId, int deletedById)
    {
        var expiringTypeId = await _notificationRepository.GetNotificationTypeIdByNameAsync(CommonResource.LicenseExpiringNotificationTypeName);
        var queueStatusId = await _notificationRepository.GetNotificationStatusIdByNameAsync(CommonResource.QueueNotificationStatusName);

        if (!expiringTypeId.HasValue || !queueStatusId.HasValue)
        {
            return;
        }

        var removedCount = await _notificationRepository.SoftDeleteNotificationsByLicenseTypeAndStatusAsync(licenseId, expiringTypeId.Value, queueStatusId.Value, deletedById);
        if (removedCount > 0)
        {
            _logger.LogInformation(
            "Soft-deleted {Count} queued expiring notifications for LicenseID {LicenseID} after renewal change.",
                removedCount,
                licenseId);
        }
    }

    private static int GetCurrentUserId(ClaimsPrincipal user)
    {
        var userIdValue = user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdValue, out var userId) ? userId : 0;
    }

    public async Task<LicenseAuditLogPagedResponse> GetLicenseAuditLogsAsync(int licenseId, int pageNo = 1)
    {
        int pageSize = 15;

        if (licenseId <= 0) throw new BadRequestException(CommonResource.InvalidLicenseId);
        if (pageNo < 1) pageNo = 1;

        var (logs, totalRecords, licenseName) = await _licenseRepository.GetLicenseAuditLogsAsync(licenseId, pageNo, pageSize);

        if (string.IsNullOrEmpty(licenseName))
        {
            throw new NotFoundException($"License with ID {licenseId} not found.");
        }

        int totalPages = totalRecords > 0 ? (int)Math.Ceiling(totalRecords / (double)pageSize) : 0;

        var logItems = logs.Select(l => new LicenseAuditLogItem
        {
            LogId = l.LicenseAuditLogID,
            DateTime = l.DateTimestamp.ToString("MM/dd/yyyy HH:mm"),
            Action = l.ActionType?.ActionName ?? "Unknown",
            PerformedBy = l.PerformedBy?.Role?.RoleName ?? "Unknown",
            EmployeeName = IsAssignmentAction(l.ActionType?.ActionName) ? l.EmployeeName : null,
            EmployeeEmail = IsAssignmentAction(l.ActionType?.ActionName) ? l.EmployeeEmail : null
        }).ToList();

        var data = new LicenseAuditLogData
        {
            LicenseId = licenseId,
            LicenseName = licenseName,
            LicenseLogs = logItems,
            Pagination = new CustomPagination { Page = pageNo, PageSize = pageSize, TotalRecords = totalRecords, TotalPages = totalPages, Message = "License audit logs fetched successfully.", StatusCode = 200 }
        };

        return new LicenseAuditLogPagedResponse
        {
            Data = new List<LicenseAuditLogData> { data }
        };
    }

    public async Task<LicenseAuditLogDetailsResponse> GetLicenseAuditLogDetailsAsync(int logId)
    {
        if (logId <= 0)
        {
            throw new BadRequestException("Invalid log ID. Log ID must be greater than zero.");
        }

        var log = await _licenseRepository.GetLicenseAuditLogDetailsAsync(logId);
        if (log == null)
        {
            throw new NotFoundException("Audit log not found.");
        }

        var data = new LicenseAuditLogDetailData
        {
            LogId = logId,
            LicenseId = log.LicenseID,
            LicenseName = log.License?.LicenseName ?? string.Empty,
            UpdatedDetails = log.Details.Select(d => new UpdatedDetail
            {
                FieldName = d.FieldName,
                OldValue = d.FieldName.Contains("Date", StringComparison.OrdinalIgnoreCase) && DateTime.TryParse(d.OldValue, out var oldDate) ? oldDate.ToString("MM-dd-yyyy") : d.OldValue,
                NewValue = d.FieldName.Contains("Date", StringComparison.OrdinalIgnoreCase) && DateTime.TryParse(d.NewValue, out var newDate) ? newDate.ToString("MM-dd-yyyy") : d.NewValue
            }).ToList()
        };

        return new LicenseAuditLogDetailsResponse
        {
            Data = new List<LicenseAuditLogDetailData> { data },
            Message = "License audit log detail fetched successfully.",
            StatusCode = 200
        };
    }
    public async Task<LicenseUtilizationResponse> GetLicenseUtilizationAsync(string? licenseType = null)
    {

        ValidateLicenseTypeFilter(licenseType);

        var (assigned, unassigned) = await _licenseRepository.GetLicenseUtilizationAsync(licenseType);

        if (assigned == 0 && unassigned == 0)
            throw new NotFoundException(CommonResource.NoDataAvailable);

        int totalLicenses = assigned + unassigned;
        decimal assignedPercentage = totalLicenses > 0
            ? Math.Round((decimal)assigned / totalLicenses * 100, 2)
            : 0m;
        decimal unassignedPercentage = totalLicenses > 0
            ? Math.Round((decimal)unassigned / totalLicenses * 100, 2)
            : 0m;

        return new LicenseUtilizationResponse
        {
            AssignedCount = assigned,
            UnassignedCount = unassigned,
            TotalLicenses = totalLicenses,
            AssignedPercentage = assignedPercentage,
            UnassignedPercentage = unassignedPercentage
        };
    }

    public async Task<LicenseStatusOverviewResponse> GetLicenseStatusOverviewAsync(string? licenseType = null)
    {
        ValidateLicenseTypeFilter(licenseType);

        var (newCount, active, expired) = await _licenseRepository.GetLicenseStatusOverviewAsync(licenseType);

        if (newCount == 0 && active == 0 && expired == 0)
            throw new NotFoundException(CommonResource.NoDataAvailable);

        int totalLicenses = newCount + active + expired;
        decimal newPercentage = totalLicenses > 0
            ? Math.Round((decimal)newCount / totalLicenses * 100, 2)
            : 0m;
        decimal activePercentage = totalLicenses > 0
            ? Math.Round((decimal)active / totalLicenses * 100, 2)
            : 0m;
        decimal expiredPercentage = totalLicenses > 0
            ? Math.Round((decimal)expired / totalLicenses * 100, 2)
            : 0m;

        return new LicenseStatusOverviewResponse
        {
            New = newCount,
            Active = active,
            Expired = expired,
            TotalLicenses = totalLicenses,
            NewPercentage = newPercentage,
            ActivePercentage = activePercentage,
            ExpiredPercentage = expiredPercentage
        };
    }

    public async Task<LicenseCostOvertimeResponse> GetLicenseCostOvertimeAsync(int? year = null, string? licenseType = null)
    {
        ValidateLicenseTypeFilter(licenseType);

        int selectedYear = year ?? DateTime.UtcNow.Year;

        var monthlyCosts = await _licenseRepository.GetLicenseCostOvertimeAsync(selectedYear, licenseType);
        var list = monthlyCosts.ToList();

        return new LicenseCostOvertimeResponse
        {
            Year = selectedYear,
            MonthlyCosts = list.Select(m => new MonthlyCostItem
            {
                Month = m.Month,
                TotalCost = m.TotalCost
            }),

        };
    }


    public async Task<UpcomingLicenseExpirationResponse> GetUpcomingLicenseExpirationsAsync(string? licenseType = null)
    {
        ValidateLicenseTypeFilter(licenseType);

        var data = await _licenseRepository.GetUpcomingLicenseExpirationsAsync(licenseType);
        var list = data.ToList();

        if (!list.Any())
            throw new NotFoundException(CommonResource.NoDataAvailable);

        return new UpcomingLicenseExpirationResponse
        {
            Licenses = list.Select(l => new UpcomingLicenseExpirationItem
            {
                LicenseName = l.LicenseName,
                ExpiryDate = l.ExpiryDate.ToString("MM-dd-yyyy"),
                DaysRemaining = l.DaysRemaining
            }),

        };
    }


    private static readonly HashSet<string> ValidLicenseTypes =
        new(StringComparer.OrdinalIgnoreCase) { "User Based", "Device Based", "Seat Based" };

    private static void ValidateLicenseTypeFilter(string? licenseType)
    {
        if (!string.IsNullOrWhiteSpace(licenseType) && !ValidLicenseTypes.Contains(licenseType))
            throw new BadRequestException(CommonResource.InvalidLicenseTypeFilter);
    }
    private static bool IsAssignmentAction(string? actionName)
    {
        if (string.IsNullOrWhiteSpace(actionName))
            return false;

        return actionName.Equals("Assigned", StringComparison.OrdinalIgnoreCase) ||
               actionName.Equals("Unassigned", StringComparison.OrdinalIgnoreCase);
    }
}
