using AssetManagement.AppService.Contracts;
using AssetManagement.Utility;
using AssetManagement.Utility.Resource;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Api.Controllers
{
    [Route("api/master")]
    [ApiController]
    public class MasterServiceController : BaseApiController
    {
        private readonly IMasterService _masterService;
        private readonly ILogger<MasterServiceController> _logger;

        public MasterServiceController(IMasterService masterService, ILogger<MasterServiceController> logger)
        {
            _masterService = masterService;
            _logger = logger;
        }

        [Authorize(Roles = "ITAdmin")]
        [HttpGet("getUser")]
        public async Task<IActionResult> GetUser([FromQuery] string? search = null)
        {
            _logger.LogInformation("GetUser called.");
            var users = await _masterService.GetAllUsersAsync(search);
            return StatusCode(200, Envelope.Ok(users, CommonResource.DataFetchedSuccessfully, 200));
        }
        [Authorize(Roles = "ITAdmin,Employee")]
        [HttpGet("getstatus")]
        public async Task<IActionResult> GetStatus()
        {
            _logger.LogInformation("GetStatus called.");
            var statuses = await _masterService.GetStatusesAsync();
            return StatusCode(200, Envelope.Ok(statuses, CommonResource.DataFetchedSuccessfully, 200));
        }
        [Authorize(Roles = "ITAdmin")]
        [HttpGet("getRole")]
        public async Task<IActionResult> GetRole()
        {
            _logger.LogInformation("GetRole called.");
            var roles = await _masterService.GetRolesAsync();
            return StatusCode(200, Envelope.Ok(roles, CommonResource.DataFetchedSuccessfully, 200));
        }
        [Authorize(Roles = "ITAdmin")]
        [HttpGet("getDataType")]
        public async Task<IActionResult> GetDataType()
        {
            _logger.LogInformation("GetDataType called.");
            var dataTypes = await _masterService.GetDataTypesAsync();
            return StatusCode(200, Envelope.Ok(dataTypes, CommonResource.DataFetchedSuccessfully, 200));
        }
        [Authorize(Roles = "ITAdmin")]
        [HttpGet("getresourceType")]
        public async Task<IActionResult> GetResourceType()
        {
            _logger.LogInformation("GetResourceType called.");
            var resourceTypes = await _masterService.GetResourceTypesAsync();
            return StatusCode(200, Envelope.Ok(resourceTypes, CommonResource.DataFetchedSuccessfully, 200));
        }

        [Authorize(Roles = "ITAdmin")]
        [HttpGet("getLicenseStatus")]
        public async Task<IActionResult> GetLicenseStatus()
        {
            _logger.LogInformation("GetLicenseStatus called.");
            var data = await _masterService.GetLicenseStatusesAsync();
            return StatusCode(200, Envelope.Ok(data, CommonResource.LicenseStatusFetchedSuccessfully, 200));
        }

        [Authorize(Roles = "ITAdmin")]
        [HttpGet("getLicenseType")]
        public async Task<IActionResult> GetLicenseType()
        {
            _logger.LogInformation("GetLicenseType called.");
            var data = await _masterService.GetLicenseTypesAsync();
            return StatusCode(200, Envelope.Ok(data, CommonResource.LicenseTypeFetchedSuccessfully, 200));
        }

        [Authorize(Roles = "ITAdmin")]
        [HttpGet("getLicensePurchaseType")]
        public async Task<IActionResult> GetLicensePurchaseType()
        {
            _logger.LogInformation("GetLicensePurchaseType called.");
            var data = await _masterService.GetLicensePurchaseTypesAsync();
            return StatusCode(200, Envelope.Ok(data, CommonResource.LicensePurchaseTypeFetchedSuccessfully, 200));
        }

        [Authorize(Roles = "ITAdmin")]
        [HttpGet("getLicenseReminderDays")]
        public async Task<IActionResult> GetReminderDays()
        {
            _logger.LogInformation("GetLicenseReminderDays called.");
            var data = await _masterService.GetReminderDaysAsync();
            return StatusCode(200, Envelope.Ok(data, CommonResource.ReminderDaysFetchedSuccessfully, 200));
        }

        [Authorize(Roles = "ITAdmin")]
        [HttpGet("getActionTypes")]
        public async Task<IActionResult> GetActionTypes()
        {
            _logger.LogInformation("GetActionTypes called.");
            var data = await _masterService.GetActionTypesAsync();
            return StatusCode(200, Envelope.Ok(data, CommonResource.ActionTypesFetchedSuccessfully, 200));
        }

        [Authorize(Roles = "ITAdmin")]
        [HttpGet("getHealthStatuses")]
        public async Task<IActionResult> GetAssetHealthStatuses()
        {
            _logger.LogInformation("GetAssetHealthStatuses called.");
            var data = await _masterService.GetAssetHealthStatusesAsync();
            return StatusCode(200, Envelope.Ok(data, CommonResource.AssetHealthStatusesFetchedSuccessfully, 200));
        }
    }
}
