using System.ComponentModel;
using AssetManagement.AppService.DTOs.License;
using AssetManagement.Infrastructure.Entities.License;
using System.Security.Claims;
using License = AssetManagement.Infrastructure.Entities.License.License;

namespace AssetManagement.AppService.Contracts;

public interface ILicenseService
{

    Task<LicensePagedResponse> GetAllLicensesAsync(string? search, string? filter, int pageNo);
    Task<GetLicenseDetailsResponse> GetLicenseByIdAsync(int licenseId);

    Task<License> CreateLicenseAsync(CreateLicenseRequest request, ClaimsPrincipal user);
    Task AssignLicenseAsync(int licenseId, AssignLicenseRequest request, ClaimsPrincipal user);
    Task UnassignLicenseAsync(int licenseId, UnassignLicenseRequest request, ClaimsPrincipal user);
    Task UpdateLicenseAsync(int licenseId, UpdateLicenseRequest request, ClaimsPrincipal user);
    Task<LicenseRenewalResponse> RenewLicenseAsync(int licenseId, RenewLicenseRequest request, ClaimsPrincipal user);
    Task<LicenseRenewalResponse> UpdateLicenseRenewalAsync(int renewalId, RenewLicenseRequest request, ClaimsPrincipal user);
    Task<LicenseAuditLogPagedResponse> GetLicenseAuditLogsAsync(int licenseId, int pageNo = 1);
    Task<(byte[] Data, string FileName, string ContentType)> GetLicenseReportFileAsync(LicenseReportFileRequest filter);
    Task<LicenseAuditLogDetailsResponse> GetLicenseAuditLogDetailsAsync(int logId);
    Task<LicenseUtilizationResponse> GetLicenseUtilizationAsync(string? licenseType = null);
    Task<LicenseStatusOverviewResponse> GetLicenseStatusOverviewAsync(string? licenseType = null);
    Task<LicenseCostOvertimeResponse> GetLicenseCostOvertimeAsync(int? year = null, string? licenseType = null);
    Task<UpcomingLicenseExpirationResponse> GetUpcomingLicenseExpirationsAsync(string? licenseType = null);

}