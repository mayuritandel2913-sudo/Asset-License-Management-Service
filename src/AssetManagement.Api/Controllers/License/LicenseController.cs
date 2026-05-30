using AssetManagement.AppService.Contracts;
using AssetManagement.AppService.DTOs.License;
using AssetManagement.Utility;
using AssetManagement.Utility.Resource;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Api.Controllers.Admin
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "ITAdmin")]
    public class LicenseController : BaseApiController
    {
        private readonly ILicenseService _licenseService;
        private readonly ILogger<LicenseController> _logger;

        public LicenseController(ILicenseService licenseService, ILogger<LicenseController> logger)
        {
            _licenseService = licenseService;
            _logger = logger;
        }

        [HttpPost("createlicense")]

        public async Task<IActionResult> CreateLicense([FromBody] CreateLicenseRequest request)
        {
            var result = await _licenseService.CreateLicenseAsync(request, User);

            _logger.LogInformation("License created successfully. LicenseID: {LicenseID}", result.LicenseID);

            var responseData = new { LicenseId = result.LicenseID };
            return StatusCode(201, Envelope.Ok(responseData, CommonResource.LicenseAddedSuccessfully, 201));
        }

        [HttpPost("license/assign/{licenseId}")]

        public async Task<IActionResult> AssignLicense(int licenseId, [FromBody] AssignLicenseRequest request)
        {
            await _licenseService.AssignLicenseAsync(licenseId, request, User);

            _logger.LogInformation("License assigned successfully. LicenseID: {LicenseID}", licenseId);

            return StatusCode(200, Envelope.Ok(null, "License assigned successfully.", 200));
        }

        [HttpGet("licensereport/excel")]
        public async Task<IActionResult> GetLicenseReportExcel([FromQuery] LicenseReportFileRequest filter)
        {
            _logger.LogInformation(
                "GetLicenseReportExcel called. filter={Filter}, search={Search}, startDate={StartDate}, endDate={EndDate}, format={Format}, fields={Fields}",
                filter.Filter, filter.Search, filter.StartDate, filter.EndDate, filter.Format, filter.Fields ?? "default");

            var (data, fileName, contentType) = await _licenseService.GetLicenseReportFileAsync(filter);
            return File(data, contentType, fileName);
        }

        [HttpGet("getalllicense")]

        public async Task<IActionResult> GetAllLicenses(
            [FromQuery] string? search = null,
            [FromQuery] string filter = "All",
            [FromQuery] int pageNo = 1)
        {
            _logger.LogInformation("GetAllLicenses called with search: {Search}, filter: {Filter}, pageNo: {PageNo}", search ?? "none", filter, pageNo);

            var result = await _licenseService.GetAllLicensesAsync(search, filter, pageNo);

            return Ok(result, CommonResource.LicenseListFetchedSuccessfully);
        }


        [HttpGet("getlicense/{licenseId}")]
        public async Task<IActionResult> GetLicenseById(int licenseId)
        {
            _logger.LogInformation("GetLicenseById called with licenseId: {LicenseId}", licenseId);

            var result = await _licenseService.GetLicenseByIdAsync(licenseId);

            return Ok(result, CommonResource.LicenseFetchedSuccessfully);
        }

        [HttpPost("license/unassign/{licenseId}")]
        public async Task<IActionResult> UnassignLicense(int licenseId, [FromBody] UnassignLicenseRequest request)
        {
            await _licenseService.UnassignLicenseAsync(licenseId, request, User);

            _logger.LogInformation("License unassigned successfully. LicenseID: {LicenseID}, UserId: {UserId}", licenseId, request.UserId);

            return StatusCode(200, Envelope.Ok(null, "License unassigned successfully.", 200));
        }

        [HttpPut("updatelicense/{licenseId}")]

        public async Task<IActionResult> UpdateLicense(int licenseId, [FromBody] UpdateLicenseRequest request)
        {
            await _licenseService.UpdateLicenseAsync(licenseId, request, User);

            _logger.LogInformation("License updated successfully. LicenseID: {LicenseID}", licenseId);

            return StatusCode(200, Envelope.Ok(null, "License updated successfully.", 200));
        }

        [HttpPost("license/renew/{licenseId}")]
        public async Task<IActionResult> RenewLicense(int licenseId, [FromBody] RenewLicenseRequest request)
        {
            var result = await _licenseService.RenewLicenseAsync(licenseId, request, User);

            _logger.LogInformation("License renewed successfully. LicenseID: {LicenseID}, RenewalID: {RenewalID}", licenseId, result.RenewalId);

            return StatusCode(200, Envelope.Ok(new { RenewalId = result.RenewalId }, CommonResource.LicenseRenewedSuccessfully, 200));
        }

        [HttpPut("license/renew/{renewalId}")]
        public async Task<IActionResult> UpdateLicenseRenewal(int renewalId, [FromBody] RenewLicenseRequest request)
        {
            var result = await _licenseService.UpdateLicenseRenewalAsync(renewalId, request, User);

            _logger.LogInformation("License renewal updated successfully. LicenseID: {LicenseID}, RenewalID: {RenewalID}", request.LicenseId, result.RenewalId);

            return StatusCode(200, Envelope.Ok(new { RenewalId = result.RenewalId }, CommonResource.LicenseRenewalUpdatedSuccessfully, 200));
        }

        [HttpGet("licenses/audit-logs/{licenseId}")]
        public async Task<IActionResult> GetAuditLogs(int licenseId, [FromQuery] AuditLogRequest request)
        {
            _logger.LogInformation("GetAuditLogs called for licenseId: {LicenseId}, pageNo: {PageNo}", licenseId, request.PageNo);

            var response = await _licenseService.GetLicenseAuditLogsAsync(licenseId, request.PageNo);

            return StatusCode(200, response);
        }

        [HttpGet("licenses/audit-logs-details/{logId}")]
        public async Task<IActionResult> GetAuditLogDetails(int logId)
        {
            _logger.LogInformation("GetAuditLogDetails called for logId: {LogId}", logId);

            var response = await _licenseService.GetLicenseAuditLogDetailsAsync(logId);

            return StatusCode(200, response);
        }
        [HttpGet("dashboard/licenses/utilization")]
        public async Task<IActionResult> GetLicenseUtilization([FromQuery] string? licenseType = null)
        {
            _logger.LogInformation("GetLicenseUtilization called. LicenseType: {LicenseType}", licenseType ?? "All");
            var result = await _licenseService.GetLicenseUtilizationAsync(licenseType);
            return StatusCode(200, Envelope.Ok(result, CommonResource.LicenseUtilizationFetchedSuccessfully, 200));
        }

        [HttpGet("dashboard/licenses/status-overview")]
        public async Task<IActionResult> GetLicenseStatusOverview([FromQuery] string? licenseType = null)
        {
            _logger.LogInformation("GetLicenseStatusOverview called. LicenseType: {LicenseType}", licenseType ?? "All");
            var result = await _licenseService.GetLicenseStatusOverviewAsync(licenseType);
            return StatusCode(200, Envelope.Ok(result, CommonResource.LicenseStatusOverviewFetchedSuccessfully, 200));
        }

        [HttpGet("dashboard/licenses/cost-overtime")]
        public async Task<IActionResult> GetLicenseCostOvertime(
        [FromQuery] int? year = null,
        [FromQuery] string? licenseType = null)
        {
            _logger.LogInformation("GetLicenseCostOvertime called. Year: {Year}, LicenseType: {LicenseType}",
                year ?? DateTime.UtcNow.Year, licenseType ?? "All");
            var result = await _licenseService.GetLicenseCostOvertimeAsync(year, licenseType);
            return StatusCode(200, Envelope.Ok(result, CommonResource.LicenseCostOvertimeFetchedSuccessfully, 200));
        }


        [HttpGet("dashboard/licenses/upcoming-expiration")]
        public async Task<IActionResult> GetUpcomingLicenseExpiration([FromQuery] string? licenseType = null)
        {
            _logger.LogInformation("GetUpcomingLicenseExpiration called. LicenseType: {LicenseType}", licenseType ?? "All");
            var result = await _licenseService.GetUpcomingLicenseExpirationsAsync(licenseType);
            return StatusCode(200, Envelope.Ok(result, CommonResource.UpcomingLicenseExpirationFetchedSuccessfully, 200));
        }

    }
}