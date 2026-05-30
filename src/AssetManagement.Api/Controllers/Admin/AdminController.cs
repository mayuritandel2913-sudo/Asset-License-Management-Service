using AssetManagement.AppService.Contracts;
using AssetManagement.AppService.DTOs;
using System;
using AssetManagement.Utility;
using AssetManagement.Utility.Resource;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AssetManagement.Api.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "ITAdmin")]
    public class AdminController : BaseApiController
    {
        private readonly IAdminService _adminService;
        private readonly IAssetService _assetService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAdminService adminService, IAssetService assetService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _assetService = assetService;
            _logger = logger;
        }

        [HttpGet("Assetreportlist")]
        public async Task<IActionResult> GetAssetReportList([FromQuery] AssetReportFilterRequest filter)
        {
            _logger.LogInformation("GetReportList called.");
            var result = await _adminService.GetAssetReportListAsync(filter);
            return Ok(result, CommonResource.DataFetchedSuccessfully);
        }

        [HttpGet("Assetreportlist/pdf")]
        public async Task<IActionResult> GetReportPdf([FromQuery] AssetReportFileRequest filter)
        {
            _logger.LogInformation("GetReportPdf called.");
            var pdfData = await _adminService.GetAssetReportPdfAsync(filter);
            return File(pdfData, "application/pdf", CommonResource.AssetReportPdfFileName);
        }

        [HttpGet("Assetreportlist/excel")]
        public async Task<IActionResult> GetReportExcel([FromQuery] AssetReportFileRequest filter)
        {
            _logger.LogInformation("GetReportExcel called. Format: {Format}, Fields: {Fields}", filter.Format, filter.Fields ?? "default");
            var (data, fileName, contentType) = await _adminService.GetAssetReportFileAsync(filter);
            return File(data, contentType, fileName);
        }

        [Authorize(Roles = "ITAdmin")]
        [HttpPost("createasset")]
        public async Task<IActionResult> CreateAssetRequest([FromBody] CreateAssetRequest createAssetRequest)
        {
            var createdById = GetCurrentUserId();
            var result = await _assetService.CreateAssetRequestAsync(createAssetRequest, createdById);
            _logger.LogInformation("Asset created successfully.");

            var responseData = new { AssetId = result.AssetID };
            return StatusCode(201, Envelope.Ok(responseData, CommonResource.AssetCreatedSuccess, 201));
        }

        private int GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdValue, out var userId) ? userId : 0;
        }

        [Authorize(Roles = "ITAdmin")]
        [HttpGet("getasset/{id}")]
        public async Task<IActionResult> GetAssetById(int id)
        {
            var result = await _assetService.GetAssetByIdAsync(id);
            _logger.LogInformation("Asset fetched successfully.");

            return Ok(result, CommonResource.AssetFetchedSuccessfully);
        }

        [Authorize(Roles = "ITAdmin")]
        [HttpGet("category/getProperty/{categoryId}")]
        public async Task<IActionResult> GetPropertyByCategoryId([FromRoute] byte categoryId)
        {
            _logger.LogInformation("GetPropertyByCategoryId | CategoryId: {CategoryId}", categoryId);
            var properties = await _adminService.GetPropertiesByCategoryIdAsync(categoryId);
            return StatusCode(200, Envelope.Ok(properties, CommonResource.DataFetchedSuccessfully, 200));
        }

        [Authorize(Roles = "ITAdmin")]
        [HttpGet("getassets/dashboard")]
        public async Task<IActionResult> GetAllAssets(
            [FromQuery] int pageNo = 1,
            [FromQuery] string? search = null)
        {
            var result = await _assetService.GetAllAssetsAsync(search, pageNo);
            if (result.TotalRecords == 0 && !string.IsNullOrWhiteSpace(search))
            {
                _logger.LogInformation("No assets found for search term '{SearchTerm}'", search);
                return Ok(result, CommonResource.AssetNotFoundForSearch);
            }

            _logger.LogInformation("Assets fetched successfully.");
            return Ok(result, CommonResource.AssetFetchedSuccessfully);
        }

        [Authorize(Roles = "ITAdmin")]
        [HttpGet("dashboard/assets/utilization")]
        public async Task<IActionResult> GetAssetUtilization()
        {
            _logger.LogInformation("GetAssetUtilization called.");

            var result = await _adminService.GetAssetUtilizationAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("dashboard/assets/downtime-rate")]
        public async Task<IActionResult> GetAssetDowntimeRate()
        {
            _logger.LogInformation("GetAssetDowntimeRate called.");

            var result = await _adminService.GetAssetDowntimeRateAsync();
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "ITAdmin")]
        [HttpGet("dashboard/assets/by-category")]
        public async Task<IActionResult> GetAssetsByCategory()
        {
            _logger.LogInformation("GetAssetsByCategory called.");

            var result = await _adminService.GetAssetsByCategoryAsync();
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "ITAdmin")]
        [HttpGet("assets/audit-logs/{assetId}")]
        public async Task<IActionResult> GetAssetAuditLogs(
            [FromRoute] int assetId,
            [FromQuery] int pageNo = 1)
        {
            _logger.LogInformation("GetAssetAuditLogs called for AssetID: {AssetId}", assetId);

            var result = await _assetService.GetAssetAuditLogsAsync(assetId, pageNo, CommonResource.PageSize);
            return StatusCode(200, result);
        }

        [Authorize(Roles = "ITAdmin")]
        [HttpGet("assets/audit-logs-details/{logId}")]
        public async Task<IActionResult> GetAssetAuditLogDetail([FromRoute] int logId)
        {
            _logger.LogInformation("GetAssetAuditLogDetail called for LogID: {LogId}", logId);

            var result = await _assetService.GetAssetAuditLogDetailAsync(logId);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "ITAdmin")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateAssetRequest(int id, [FromBody] UpdateAssetRequest updateAssetRequest)
        {
            var modifiedById = GetCurrentUserId();
            var result = await _assetService.UpdateAssetRequestAsync(id, updateAssetRequest, modifiedById);
            _logger.LogInformation("Asset updated successfully.");
            var responseData = new { AssetId = result!.AssetID };
            return Ok(responseData, CommonResource.AssetUpdatedSuccessfully);
        }
        [Authorize(Roles = "ITAdmin")]
        [HttpPost("assign/{id}")]
        public async Task<IActionResult> AssignAssetRequest(int id, [FromBody] AssignAssetRequest assignAssetRequest)
        {
            var currentUserId = GetCurrentUserId();
            await _assetService.AssignAssetRequestAsync(id, assignAssetRequest, currentUserId);
            _logger.LogInformation("Asset assigned successfully.");
            return Ok(new { AssetId = id }, CommonResource.AssetAssigned);
        }

        [Authorize(Roles = "ITAdmin")]
        [HttpPost("unassign/{id}")]
        public async Task<IActionResult> UnassignAssetRequest(int id, [FromBody] UnassignAssetRequest unassignAssetRequest)
        {
            var currentUserId = GetCurrentUserId();
            await _assetService.UnassignAssetRequestAsync(id, unassignAssetRequest, currentUserId);
            _logger.LogInformation("Asset unassigned successfully.");
            return Ok(new { AssetId = id }, CommonResource.AssetUnassigned);
        }

        [HttpGet("dashboard/assets/status-distribution")]
        public async Task<IActionResult> GetAssetStatusDistribution([FromQuery] int? categoryId = null, [FromQuery] string? categoryName = null)
        {
            _logger.LogInformation("GetAssetStatusDistribution called. CategoryId: {CategoryId}, CategoryName: {CategoryName}", categoryId, categoryName);
            var result = await _assetService.GetAssetStatusDistributionAsync(categoryId, categoryName);
            return StatusCode(200, Envelope.Ok(result, CommonResource.AssetStatusDistributionFetchedSuccessfully, 200));
        }
    }
}