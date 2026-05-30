﻿namespace AssetManagement.Utility.Resource;

public static class CommonResource
{

    public const string Success = "Success";

    public const string TokenGenerated = "Token Generated Successfully";

    public const string InternalServerError = "Internal server error";
    public const string NotFound = "Resource not found";
    public const string DataTypeNotFound = "Data type not found";
    public const string DateFormat = "MM-dd-yyyy";

    public const string NoDataFound = "No data found for the specified criteria";

    public const string UnAuthServiceorized = "Unauthorized access";
    public const string InvalidCredentials = "Invalid credentials";
    public const string DataFetchedSuccessfully = "Data fetched successfully";

    public const string UserInactive = "User is inactive";

    public const string AssetReportTitle = "Asset Report";
    public const string AssetReportWorksheetName = "Asset Report";
    public const string Forbidden = "Access forbidden";
    public const string BadRequest = "Bad request";
    public const string InvalidModelState = "Invalid model state";

    public const string CategoryNameRequired = "Category name is required.";
    public const string CategoryNameMinLength = "Category name must be at least 2 characters.";
    public const string CategoryNameMaxLength = "Category name must not exceed 50 characters.";
    public const string ResourceTypeIdRequired = "Resource type is required.";
    public const string CategoryAlreadyExists = "Category name already exists.";
    public const string CategoryCreatedSuccessfully = "Category created successfully.";
    public const string CategoryUpdatedSuccessfully = "Category has been updated successfully.";
    public const string CategoryDeletedSuccessfully = "Category has been deleted successfully.";
    public const string CategoryNotFound = "Category not found.";
    public const string CategoryDeleted = "Selected category has been deleted.";
    public const string PropertyInvalid = "Category is not associated with the specified property.";
    public const string DuplicatePropertyName = "Duplicate property names are not allowed.";
    public const string CategoryNotFoundForSearch = "No records found.";
    public const string ResourceTypeNotFound = "Resource type not found.";
   
    public const string CategoryHasAssets = "Category cannot be deleted because assets are associated with it.";
    public const string UserNotFound = "User not found.";

    public const string StatusNotFound = "Status does not exist.";

    public const string CategoriesFetchedSuccessfully = "Categories fetched successfully.";
    public const string CategoryNameInvalid = "Category name can only contain letters, numbers, spaces, and hyphens.";
    public const string CategoryNameCannotBeOnlyDigits = "Category name cannot be only digits.";

    public const string AssetReportPdfFileName = "AssetReport.pdf";
    public const string AssetReportExcelFileName = "AssetReport.xlsx";
    public const string LicenseReportExcelFileName = "LicenseReport.xlsx";
    public const string LicenseReportWorksheetName = "License Report";

    public const string AssignedStatus = "assigned";
    public const string AssignmentDateHeader = "Assignment Date";
    public const string PurchaseDateHeader = "Purchase Date";
    public const string AllFilter = "all";
    public const int PageSize = 15;

    public const string IndiaTimeZone = "India Standard Time";

    public const string AssetNotFound = "Asset not found.";
    public const string InvalidData = "Invalid data provided.";
    public const string AssetCreatedSuccess = "Asset created successfully.";
    public const string AssetAssigned = "Asset assigned successfully.";
    public const string AssetUnassigned = "Asset unassigned successfully.";
    public const string AssetFetchedSuccessfully = "Asset fetched successfully";
    public const string AssetUtilizationFetchedSuccessfully = "Asset utilization fetched successfully.";
    public const string AssetDowntimeRateFetchedSuccessfully = "Asset downtime rate fetched successfully.";
    public const string AssetsByCategoryFetchedSuccessfully = "Assets by category fetched successfully.";
    public const string NoDataAvailable = "No Data Available.";
    public const string AuditLogDetailsFetchedSuccessfully = "Asset audit log details fetched successfully.";
    public const string AuditLogNoFieldsUpdated = "No Field is Updated.";
    public const string AuditLogIdInvalid = "Invalid audit log ID.";
    public const string SerialNumberAlreadyExists = "An asset with this serial number already exists.";
    public const string AssetNotFoundForSearch = "The requested resource was not found.";


    public const string AssetUpdatedSuccessfully = "Transaction executed successfully";
    public const string VendorNameMinLength = "Vendor name must be at least 2 characters.";
    public const string VendorNameMaxLength = "Vendor name must not exceed 50 characters.";

    public const string LicenseStatusFetchedSuccessfully = "License statuses fetched successfully.";
    public const string LicenseTypeFetchedSuccessfully = "License Type fetched successfully.";
    public const string LicensePurchaseTypeFetchedSuccessfully = "License PurchaseType fetched successfully.";
    public const string ReminderDaysFetchedSuccessfully = "License expiry reminder configurations fetched successfully.";
    public const string LicenseStatusNotFound = "No license statuses found.";
    public const string LicenseTypeNotFound = "No license types found.";
    public const string LicensePurchaseTypeNotFound = "No license purchase types found.";
    public const string LicenseAddedSuccessfully = "License added successfully.";
    public const string LicenseListFetchedSuccessfully = "License list fetched successfully.";
    public const string LicenseFetchedSuccessfully = "License fetched successfully.";
    public const string LicenseKeyAlreadyExists = "License key already exists.";
    public const string LicenseExpiredCannotBeAssigned = "Expired license cannot be assigned.";
    public const string InvalidLicenseType = "Invalid license type provided.";
    public const string InvalidLicensePurchaseType = "Invalid license purchase type provided.";
    public const string InvalidLicenseStatus = "Invalid license status.";
    public const string UserBasedLicenseSeatsError = "User-based license type must have total seats equal to 1.";
    public const string InvalidDateFormat = "Date must be in MM/DD/YYYY format.";
    public const string PageNumberInvalid = "Page number must be greater than 0.";
    public const string InvalidLicenseId = "Invalid license ID.";
    public const string LicenseNotFound = "License not found.";
    public const string NoLicensesFoundForSearch = "No licenses found matching search criteria.";
    public const string NoLicensesFoundForFilter = "No licenses found for the specified filter.";
    public const string InvalidFilterProvided = "Invalid filter provided. Valid filters are: All, New, Active, Expiring3d, Expiring7d, Expiring15d, Expiring30d, Expired";
    public const string SearchParameterTooShort = "Search parameter must be at least 2 characters.";
    public const string SearchParameterExceedsMaxLength = "Search parameter cannot exceed 50 characters.";
    public const string SearchParameterInvalidCharacters = "Search parameter contains invalid characters. Only alphanumeric, spaces, hyphens, dots, underscores, and '@' are allowed.";
    public const string PurchaseDateFormatInvalid = "Purchase date must be in MM/DD/YYYY format.";
    public const string StartDateFormatInvalid = "Start date must be in MM/DD/YYYY format.";
    public const string ExpiryDateFormatInvalid = "Expiry date must be in MM/DD/YYYY format.";
    public const string InvalidAuthenticatedUser = "Invalid authenticated user.";
    public const string UserBasedLicenseType = "User Based";
    public const int LicensePageSize = 15;
    public const int LicenseMinSearchLength = 2;
    public const int LicenseMaxSearchLength = 50;
    public const int LicenseMaskedKeyVisibleLength = 4;
    public const string LicenseDateFormatForParsing = "MM/dd/yyyy";
    public const string LicenseUnknownStatus = "Unknown";
    public const string LicenseSearchValidationPattern = @"^[a-zA-Z0-9\s\-._@]*$";
    public static readonly string[] LicenseValidFilters = { "All", "New", "Active", "Expiring3d", "Expiring7d", "Expiring15d", "Expiring30d", "Expired" };

    public const string LicenseName_Required = "Please enter a name.";
    public const string LicenseName_MinLength = "The minimum length of the name is 2 characters.";
    public const string LicenseName_MaxLength = "The maximum length of the name is 100 characters.";

    public const string LicenseType_Required = "Please select license type.";
    public const string LicenseType_Invalid = "Please select license type.";
    public const string LicensePurchaseType_Required = "Please select license purchase type.";
    public const string LicensePurchaseType_Invalid = "Please select license purchase type.";
    public const string TotalSeats_Required = "Please enter total seats.";
    public const string TotalSeats_Invalid = "Total seats must be greater than 0.";
    public const string VendorName_MinLength = "The minimum length of the name is 2 characters.";
    public const string VendorName_MaxLength = "The maximum length of the name is 50 characters.";
    public const string PurchaseDate_Required = "Please enter the purchase date.";
    public const string PurchaseDate_Format = "Purchase date must be in MM/DD/YYYY format.";
    public const string PurchaseDate_FutureDate = "Purchase date cannot be a future date.";
    public const string PurchaseDate_TooOld = "Purchase date must be within the last 2 years.";
    public const string StartDate_Required = "Please enter the start date.";
    public const string StartDate_Format = "Start date must be in MM/DD/YYYY format.";
    public const string StartDate_BeforePurchase = "Start date must be greater than or equal to purchase date.";
    public const string ExpiryDate_Format = "Expiry date must be in MM/DD/YYYY format.";
    public const string ExpiryDate_BeforePurchase = "Expiry date should be after purchase date.";
    public const string ExpiryDate_BeforeStartDate = "Expiry date should be after start date.";
    public const string ExpiryDate_PastDate = "Expiry date cannot be a past date.";
    public const string ExpiryDate_NotAllowedForPermanentLicense = "Expiry date is not allowed for One-Time License.";
    public const string ReminderDays_NotAllowedForPermanentLicense = "Reminder days configuration is not allowed for One-Time licenses .";
    public const string LicenseKey_Format = "License key can only contain letters, numbers, underscore, dot, and hyphen.";
    public const string LicenseRenewedSuccessfully = "License renewed successfully.";
    public const string LicenseRenewalUpdatedSuccessfully = "License renewal updated successfully.";
    public const string RenewalDateMustBeAfterExpiryDate = "Renewal date must be after the current expiry date.";
    public const string LicenseRenewalWindowExceeded = "License renewal cannot be processed more than two years after expiry.";
    public const string LicenseExpiryDateRequiredForRenewal = "License expiry date is required for renewal.";
    
    public const string Cost_Required = "Cost is required.";
    public const string Cost_Invalid = "Cost must not be negative.";
    public const string Description_MaxLength = "Description must not exceed 500 characters.";
    public const string Employees_Required = "Employees list is required.";
    public const string UserId_Required = "User ID is required.";
    public const string UserId_Invalid = "User ID must be greater than 0.";
    public const string InvalidReminderConfigId = "Invalid ReminderDays ID provided.";
    public const string ActiveLicenseStatus = "Active";
    public const string ExpiredLicenseStatus = "Expired";
    public const string ActiveLicenseStatusNotFound = "Active license status not found.";
    public const string ExpiredLicenseStatusNotFound = "Expired license status not found.";
    public const string InvalidLicenseTypeFilter = "Invalid license type. Allowed values: UserBased, SeatBased, DeviceBased.";
    public const string LicenseUtilizationFetchedSuccessfully = "License utilization fetched successfully.";
    public const string LicenseStatusOverviewFetchedSuccessfully = "License status overview fetched successfully.";
    public const string LicenseCostOvertimeFetchedSuccessfully = "License cost overtime fetched successfully.";
    public const string UpcomingLicenseExpirationFetchedSuccessfully = "Upcoming license expiration fetched successfully.";
    public const string ActionTypesFetchedSuccessfully = "Action types fetched successfully.";
    public const string AssetStatusDistributionFetchedSuccessfully = "Asset status distribution fetched successfully.";

    public const string AdminRoleName = "ITAdmin";
    public const string QueueNotificationStatusName = "Queue";
    public const string LicenseExpiringNotificationTypeName = "LicenseExpiring";
    public const string SentNotificationStatusName = "Sent";
    public const string FailedNotificationStatusName = "Failed";

   
    public const string ReportFileDateFormat = "yyyyMMdd";
    public const string ReportFileReadableDateFormat = "dMMMyyyy";
    public const string RenewalDateMustBeDayAfterExpiryBeforeExpiry = "When renewing before expiry, the renewal date must be the day after the current expiry date.";
    public const string AssetHealthStatusesFetchedSuccessfully = "Asset health statuses fetched successfully.";
    
    public static string BuildReportFileName(DateTime? startDate, DateTime? endDate, string reportType, string extension)
    {
        var start = startDate.HasValue ? startDate.Value.ToString(ReportFileReadableDateFormat) : "All";
        var end   = endDate.HasValue   ? endDate.Value.ToString(ReportFileReadableDateFormat)   : "All";
        return $"{reportType}_{start}_{end}.{extension}";
    }

    
    public static readonly string[] AssetMandatoryFields  = { "AssetID", "AssetName", "Category" };

    
    public static readonly string[] AssetOptionalFields =
    {
        "SerialNumber", "AssignmentStatus", "HealthStatus",
        "PurchaseDate", "AssetCost", "Vendor",
        "AssignedEmployee", "AssignmentDate", "PropertyNames"
    };

    
    public static readonly string[] AssetDefaultOptionalFields =
    {
        "SerialNumber", "AssignmentStatus", "HealthStatus"
    };

    
    public static readonly string[] LicenseMandatoryFields = { "LicenseID", "LicenseName" };

   
    public static readonly string[] LicenseOptionalFields =
    {
        "LicenseType", "PurchaseType", "TotalSeats", "AvailableSeats",
        "Vendor", "PurchaseDate", "StartDate", "ExpiryDate",
        "Status", "LicenseKey", "Cost", "AssignedEmployees"
    };

    
    public static readonly string[] LicenseDefaultOptionalFields =
    {
        "PurchaseDate", "ExpiryDate", "Vendor", "Status"
    };

}
