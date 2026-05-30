using AssetManagement.AppService.Contracts;
using AssetManagement.AppService.DTOs;
using AssetManagement.Infrastructure.Contracts;
using AssetManagement.Infrastructure.Entities;
using AssetManagement.Utility.Resource;
using AssetManagement.Utility.Exceptions;
using Microsoft.Extensions.Logging;
using System.Globalization;
using AssetManagement.AppService.DTOs.License;
using AssetManagement.AppService.DTOs.asset;

namespace AssetManagement.AppService.Services;

public class AssetService : IAssetService
{
    private readonly IAssetRepository _assetRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMasterServiceRepository _masterRepository;
    private readonly ILogger<AssetService> _logger;

    public AssetService(IAssetRepository assetRepository, ICategoryRepository categoryRepository, IMasterServiceRepository masterRepository, ILogger<AssetService> logger)
    {
        _assetRepository = assetRepository;
        _categoryRepository = categoryRepository;
        _masterRepository = masterRepository;
        _logger = logger;
    }

    public async Task<AssetPropertyValue> AddAssetPropertyValueAsync(CreateAssetRequestProperty createAssetRequestProperty, int assetId)
    {
        var asset = await _assetRepository.GetByIdAsync(assetId);
        if (asset == null)
        {
            _logger.LogWarning("Asset not found. ID {AssetId}", assetId);
            throw new NotFoundException($"{CommonResource.AssetNotFound} ID {assetId}.");
        }

        if (createAssetRequestProperty.PropertyID <= 0 || createAssetRequestProperty.PropertyID > byte.MaxValue)
        {
            _logger.LogWarning("Property not found in database. PropertyID: {PropertyId}", createAssetRequestProperty.PropertyID);
            throw new BadRequestException("Property id not found.");
        }

        var propertyId = (byte)createAssetRequestProperty.PropertyID;

        var existingProperty = await _assetRepository.GetAssetPropertyByIdAsync(propertyId);
        if (existingProperty == null)
        {
            _logger.LogWarning("Property not found in database. PropertyID: {PropertyId}", createAssetRequestProperty.PropertyID);
            throw new BadRequestException("Property id not found.");
        }

        if (existingProperty.CategoryID != asset.CategoryID)
        {
            _logger.LogWarning("Property not found for this asset. PropertyID: {PropertyId}, AssetID: {AssetId}", createAssetRequestProperty.PropertyID, assetId);
            throw new NotFoundException($"Property ID {createAssetRequestProperty.PropertyID} is not associated with the selected category.");
        }

        var existingValue = asset.PropertyValues.FirstOrDefault(p => p.PropertyID == propertyId);

        if (existingValue != null)
        {
            existingValue.Value = createAssetRequestProperty.PropertyValue?.Trim();
            existingValue.ModifiedByID = createAssetRequestProperty.UserId > 0 ? createAssetRequestProperty.UserId : (asset.ModifiedByID ?? 0);
            existingValue.ModifiedDate = DateTime.UtcNow;

            if (existingValue.ModifiedByID <= 0)
            {
                throw new UnAuthServiceorizedException("Authenticated user not found.");
            }

            _logger.LogInformation("Updating existing AssetPropertyValue. PropertyID: {PropertyId}, AssetID: {AssetId}", createAssetRequestProperty.PropertyID, assetId);
            return await _assetRepository.UpdateAsyncAssetPropertyValue(existingValue);
        }

        var assetPropertyValue = new AssetPropertyValue
        {
            AssetID = assetId,
            CategoryID = asset.CategoryID,
            PropertyID = propertyId,
            Value = createAssetRequestProperty.PropertyValue?.Trim(),
            CreatedByID = createAssetRequestProperty.UserId > 0 ? createAssetRequestProperty.UserId : (asset.CreatedByID ?? 0),
            CreatedDate = DateTime.UtcNow
        };

        if (assetPropertyValue.CreatedByID <= 0)
        {
            throw new UnAuthServiceorizedException("Authenticated user not found.");
        }

        _logger.LogInformation("Asset property value prepared successfully. PropertyID: {PropertyId}, AssetID: {AssetId}", createAssetRequestProperty.PropertyID, assetId);
        return await _assetRepository.AddAsyncAssetPropertyValue(assetPropertyValue);
    }

    public async Task AssignAssetRequestAsync(int assetId, AssignAssetRequest assignAssetRequest, int modifiedById)
    {
        if (assignAssetRequest == null)
        {
            _logger.LogWarning("AssignAssetRequestAsync — not found | AssetID: {AssetId}", assetId);
            throw new ArgumentNullException(nameof(assignAssetRequest), CommonResource.InvalidData);
        }

        if (modifiedById <= 0)
        {
            _logger.LogWarning("AssignAssetRequestAsync — invalid authenticated user id: {ModifiedById}", modifiedById);
            throw new UnAuthServiceorizedException("Invalid authenticated user.");
        }

        var modifierExists = await _assetRepository.UserExistsAsync(modifiedById);
        if (!modifierExists)
        {
            _logger.LogWarning("AssignAssetRequestAsync — authenticated user not found. ID {ModifiedById}", modifiedById);
            throw new UnAuthServiceorizedException("Authenticated user not found.");
        }

        if (!assignAssetRequest.AssignmentDate.HasValue)
        {
            _logger.LogWarning("AssignAssetRequestAsync — assignment date missing or invalid | AssetID: {AssetId}", assetId);
            throw new BadRequestException("Field is mandatory.");
        }

        var sqlMinDate = new DateTime(1753, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        if (assignAssetRequest.AssignmentDate.Value < sqlMinDate)
        {
            _logger.LogWarning("AssignAssetRequestAsync — assignment date out of range: {AssignmentDate}", assignAssetRequest.AssignmentDate.Value);
            throw new BadRequestException("Date is invalid.");
        }

        if (assignAssetRequest.ExpectedReturnDate.HasValue
            && assignAssetRequest.ExpectedReturnDate.Value < sqlMinDate)
        {
            _logger.LogWarning("AssignAssetRequestAsync — expected return date out of range: {ExpectedReturnDate}", assignAssetRequest.ExpectedReturnDate);
            throw new BadRequestException("Date is invalid.");
        }

        if (assignAssetRequest.ExpectedReturnDate.HasValue &&
            assignAssetRequest.ExpectedReturnDate.Value < assignAssetRequest.AssignmentDate)
        {
            _logger.LogWarning("AssignAssetRequestAsync — invalid date range | AssignmentDate: {AssignmentDate} | ExpectedReturnDate: {ExpectedReturnDate}",
                assignAssetRequest.AssignmentDate,
                assignAssetRequest.ExpectedReturnDate);
            throw new BadRequestException("Date is invalid.");
        }

        var asset = await _assetRepository.GetByIdAsync(assetId);
        if (asset == null)
        {
            _logger.LogWarning("Asset not found. ID {AssetId}", assetId);
            throw new NotFoundException(CommonResource.AssetNotFound);
        }

        var currentStatus = asset.AssetStatus?.AssetStatusName ?? $"ID {asset.StatusID}";
        if (!currentStatus.Equals("Available", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "AssignAssetRequestAsync — assignment blocked due to status | AssetID: {AssetId} | CurrentStatus: {CurrentStatus}",
                assetId,
                currentStatus);
            throw new BadRequestException($"Asset cannot be assigned because its current status is '{currentStatus}'. Only assets with status 'Available' can be assigned.");
        }

        if (asset.AssetHealthStatus != null)
        {
            var blockedHealthStatuses = await _assetRepository.GetBlockedHealthStatusNamesAsync();

            var normalizedHealthStatus = asset.AssetHealthStatus.AssetHealthStatusName
                .ToLowerInvariant()
                .Trim()
                .Replace(" ", "");

            var isBlocked = blockedHealthStatuses
                .Select(s => s.ToLowerInvariant().Trim().Replace(" ", ""))
                .Contains(normalizedHealthStatus);

            if (isBlocked)
            {
                _logger.LogWarning(
                    "AssignAssetRequestAsync — assignment blocked due to health status | AssetID: {AssetId} | HealthStatus: {HealthStatus}",
                    assetId,
                    asset.AssetHealthStatus.AssetHealthStatusName);
                throw new BadRequestException($"Asset cannot be assigned because its health status is '{asset.AssetHealthStatus.AssetHealthStatusName}'. Only assets with 'Good' health status can be assigned.");
            }
        }

        if (assignAssetRequest.UserID <= 0)
        {
            _logger.LogWarning("AssignAssetRequestAsync — employee id missing | AssetID: {AssetId}", assetId);
            throw new BadRequestException("Please enter the employee name.");
        }

        var userExists = await _assetRepository.UserExistsAsync(assignAssetRequest.UserID);
        if (!userExists)
        {
            _logger.LogWarning("AssignAssetRequestAsync — employee not found | UserID: {UserId}", assignAssetRequest.UserID);
            throw new NotFoundException("Employee Not Found.");
        }


        if (!assignAssetRequest.AssignmentDate.HasValue || assignAssetRequest.AssignmentDate.Value.Date < asset.PurchaseDate.Date)
        {
            _logger.LogWarning("AssignAssetRequestAsync — assignment date earlier than purchase date | AssetID: {AssetId} | AssignmentDate: {AssignmentDate} | PurchaseDate: {PurchaseDate}", assetId, assignAssetRequest.AssignmentDate, asset.PurchaseDate);
            throw new BadRequestException("Date is invalid.");
        }

        asset.UserID = assignAssetRequest.UserID;
        asset.ModifiedByID = modifiedById;
        asset.ModifiedDate = DateTime.UtcNow;


        var assignedStatusId = await _assetRepository.GetAssetStatusIdByNameAsync("Assigned");
        if (assignedStatusId.HasValue)
        {
            asset.StatusID = assignedStatusId.Value;
        }

        await _assetRepository.UpdateAsync(asset);

        var assignment = new AssignmentDetails
        {
            AssetID = assetId,
            UserID = assignAssetRequest.UserID,
            AssignmentDate = assignAssetRequest.AssignmentDate.Value,
            ExpectedReturnDate = assignAssetRequest.ExpectedReturnDate,
            CreatedByID = modifiedById,
            CreatedDate = DateTime.UtcNow
        };

        await _assetRepository.AddAssignmentAsync(assignment);
        _logger.LogInformation("Asset assigned successfully.");
    }

    public async Task UnassignAssetRequestAsync(int assetId, UnassignAssetRequest unassignAssetRequest, int modifiedById)
    {
        if (unassignAssetRequest == null)
        {
            _logger.LogWarning("UnassignAssetRequestAsync — not found | AssetID: {AssetId}", assetId);
            throw new ArgumentNullException(nameof(unassignAssetRequest), CommonResource.InvalidData);
        }

        if (modifiedById <= 0)
        {
            _logger.LogWarning("UnassignAssetRequestAsync — invalid authenticated user id: {ModifiedById}", modifiedById);
            throw new UnAuthServiceorizedException("Invalid authenticated user.");
        }

        var modifierExists = await _assetRepository.UserExistsAsync(modifiedById);
        if (!modifierExists)
        {
            _logger.LogWarning("UnassignAssetRequestAsync — authenticated user not found. ID {ModifiedById}", modifiedById);
            throw new UnAuthServiceorizedException("Authenticated user not found.");
        }

        if (unassignAssetRequest.UserID <= 0)
        {
            _logger.LogWarning("UnassignAssetRequestAsync — invalid user id: {UserID}", unassignAssetRequest.UserID);
            throw new BadRequestException("User ID is required and must be greater than 0.");
        }

        var userExists = await _assetRepository.UserExistsAsync(unassignAssetRequest.UserID);
        if (!userExists)
        {
            _logger.LogWarning("User not found. ID {UserId}", unassignAssetRequest.UserID);
            throw new NotFoundException($"{CommonResource.UserNotFound} ID {unassignAssetRequest.UserID}.");
        }

        var asset = await _assetRepository.GetByIdAsync(assetId);
        if (asset == null)
        {
            _logger.LogWarning("Asset not found. ID {AssetId}", assetId);
            throw new NotFoundException(CommonResource.AssetNotFound);
        }

        if (!asset.UserID.HasValue)
        {
            _logger.LogWarning(
                "UnassignAssetRequestAsync — asset is not assigned to any user | AssetID: {AssetId}",
                assetId);
            throw new BadRequestException("Asset is not currently assigned to any user.");
        }

        if (asset.UserID.Value != unassignAssetRequest.UserID)
        {
            _logger.LogWarning(
                "UnassignAssetRequestAsync — asset is not assigned to the specified user | AssetID: {AssetId} | CurrentUserID: {CurrentUserID} | RequestedUserID: {RequestedUserID}",
                assetId,
                asset.UserID.Value,
                unassignAssetRequest.UserID);
            throw new BadRequestException($"Asset is not assigned to user ID {unassignAssetRequest.UserID}. It is currently assigned to user ID {asset.UserID.Value}.");
        }

        // Get the "Available" status ID
        var availableStatusId = await _masterRepository.GetAvailableStatusIdAsync();
        if (!availableStatusId.HasValue)
        {
            _logger.LogWarning("UnassignAssetRequestAsync — available status not found in database");
            throw new NotFoundException("Available status not found in the system.");
        }

        // Get the active assignment to update it
        var assignment = await _assetRepository.GetActiveAssignmentByAssetAndUserAsync(assetId, unassignAssetRequest.UserID);
        if (assignment == null)
        {
            _logger.LogWarning(
                "UnassignAssetRequestAsync — no active assignment found | AssetID: {AssetId} | UserID: {UserID}",
                assetId,
                unassignAssetRequest.UserID);
            throw new BadRequestException("No active assignment found for this asset and user.");
        }

        // Update asset: clear UserID and set status to Available
        asset.UserID = null;
        asset.StatusID = availableStatusId.Value;
        asset.ModifiedByID = modifiedById;
        asset.ModifiedDate = DateTime.UtcNow;
        await _assetRepository.UpdateAsync(asset);

        // Update assignment details: mark as inactive and record unassignment info
        assignment.UnassignedByID = modifiedById;
        assignment.UnassignedDate = DateTime.UtcNow;
        assignment.Remark = unassignAssetRequest.Remark;
        assignment.IsActive = false;
        await _assetRepository.UpdateAssignmentAsync(assignment);

        _logger.LogInformation("Asset unassigned successfully from User ID: {UserID}. AssetID: {AssetId}", unassignAssetRequest.UserID, assetId);
    }

    public async Task<Asset> CreateAssetRequestAsync(CreateAssetRequest createAssetRequest, int createdById)
    {
        if (createAssetRequest == null)
        {
            _logger.LogWarning("Invalid data provided.");
            throw new ArgumentNullException(nameof(createAssetRequest), CommonResource.InvalidData);
        }

        if (createAssetRequest.CategoryID > byte.MaxValue)
        {
            _logger.LogWarning("Category not found. ID {CategoryId}", createAssetRequest.CategoryID);
            throw new NotFoundException("CategoryId not found.");
        }

        var category = await _categoryRepository.GetCategoryByIdAsync((byte)createAssetRequest.CategoryID);
        if (category == null)
        {
            _logger.LogWarning("Category not found. ID {CategoryId}", createAssetRequest.CategoryID);
            throw new NotFoundException($"{CommonResource.CategoryNotFound} ID {createAssetRequest.CategoryID}.");
        }

        if (category.DeletedDate != null)
        {
            _logger.LogWarning("Attempted to create asset with a deleted category. ID {CategoryId}", createAssetRequest.CategoryID);
            throw new BadRequestException(CommonResource.CategoryDeleted);
        }

        await ValidateLookupIdsAsync(
            category.ResourceTypeID,
            createAssetRequest.CategoryID,
            createAssetRequest.StatusID);

        if (createdById <= 0)
        {
            _logger.LogWarning("User not found. Invalid ID {UserId}", createdById);
            throw new NotFoundException($"{CommonResource.UserNotFound} ID {createdById}.");
        }

        var creatorExists = await _assetRepository.UserExistsAsync(createdById);
        if (!creatorExists)
        {
            _logger.LogWarning("User not found. ID {UserId}", createdById);
            throw new NotFoundException($"{CommonResource.UserNotFound} ID {createdById}.");
        }

        var serialExists = await _assetRepository.SerialNumberExistsAsync(createAssetRequest.SerialNumber);
        if (serialExists)
        {
            _logger.LogWarning("Serial number already exists. SerialNumber: {SerialNumber}", createAssetRequest.SerialNumber);
            throw new BadRequestException("Serial number must be unique.");
        }

        if (!createAssetRequest.PurchaseDate.HasValue)
        {
            _logger.LogWarning("Purchase date is required.");
            throw new BadRequestException("Purchase date is required.");
        }

        await ValidateAssetPropertiesAsync(
            createAssetRequest.AssetProperties,
            (byte)createAssetRequest.CategoryID);

        var asset = new Asset
        {
            AssetName = createAssetRequest.AssetName,
            SerialNumber = createAssetRequest.SerialNumber,
            PurchaseDate = createAssetRequest.PurchaseDate.Value,
            AssetCost = createAssetRequest.AssetCost,
            VendorName = createAssetRequest.VendorName,
            Description = createAssetRequest.Description,
            ResourceTypeID = category.ResourceTypeID,
            CategoryID = (byte)createAssetRequest.CategoryID,
            StatusID = (byte)createAssetRequest.StatusID,
            CreatedByID = createdById,
            ModifiedByID = createdById,
            CreatedDate = DateTime.UtcNow
        };

        asset.PropertyValues = new List<AssetPropertyValue>();

        var savedAsset = await _assetRepository.AddAsyncAsset(asset);

        if (createAssetRequest.AssetProperties != null && createAssetRequest.AssetProperties.Any())
        {
            foreach (var prop in createAssetRequest.AssetProperties)
            {
                if (prop.PropertyID <= 0 || prop.PropertyID > byte.MaxValue)
                {
                    _logger.LogWarning("CreateAssetRequestAsync - Invalid property ID {PropertyId}", prop.PropertyID);
                    throw new BadRequestException("Property id not found.");
                }
                prop.UserId = createdById;
                await AddAssetPropertyValueAsync(prop, savedAsset.AssetID);
                _logger.LogInformation("Property ID {PropertyId} updated for Asset ID {AssetId}.", prop.PropertyID, savedAsset.AssetID);
            }
        }
        _logger.LogInformation("Asset created successfully.");

        return savedAsset;
    }

    private async Task ValidateAssetPropertiesAsync(IEnumerable<CreateAssetRequestProperty>? properties, byte categoryId, int? excludeAssetId = null)
    {
        var categoryProperties = await _assetRepository.GetAssetPropertiesByCategoryIdAsync(categoryId);
        var categoryPropertiesById = categoryProperties.ToDictionary(p => p.PropertyID);
        var mandatoryProperties = categoryProperties.Where(p => p.IsMandatory).ToList();
        var uniqueProperties = categoryProperties.Where(p => p.IsUnique).ToList();

        var incomingProperties = properties?.ToList() ?? new List<CreateAssetRequestProperty>();

        foreach (var property in incomingProperties)
        {
            if (property.PropertyID <= 0 || property.PropertyID > byte.MaxValue)
            {
                _logger.LogWarning("Invalid property ID {PropertyId}", property.PropertyID);
                throw new BadRequestException("Property id not found.");
            }

            var existingProperty = await _assetRepository.GetAssetPropertyByIdAsync((byte)property.PropertyID);
            if (existingProperty == null)
            {
                _logger.LogWarning("Property not found in database. PropertyID: {PropertyId}", property.PropertyID);
                throw new BadRequestException("Property id not found.");
            }

            if (!categoryPropertiesById.ContainsKey((byte)property.PropertyID))
            {
                _logger.LogWarning("Property not found for category. PropertyID: {PropertyId}, CategoryID: {CategoryId}", property.PropertyID, categoryId);
                throw new NotFoundException("property id not found with this category");
            }

            property.PropertyValue = property.PropertyValue?.Trim();
        }

        foreach (var mandatoryPropertyId in mandatoryProperties.Select(mandatoryProperty => mandatoryProperty.PropertyID))
        {
            var providedMandatoryProperty = incomingProperties.FirstOrDefault(p => p.PropertyID == mandatoryPropertyId);
            if (providedMandatoryProperty == null || string.IsNullOrWhiteSpace(providedMandatoryProperty.PropertyValue))
            {
                _logger.LogWarning(
                    "Mandatory property value missing. PropertyID: {PropertyId}, CategoryID: {CategoryId}",
                    mandatoryPropertyId,
                    categoryId);
                throw new BadRequestException(
                    $"Property ID {mandatoryPropertyId} is mandatory for the selected category. Please provide a value.");
            }
        }

        foreach (var uniqueProperty in uniqueProperties)
        {
            var incomingProperty = incomingProperties.FirstOrDefault(p => p.PropertyID == uniqueProperty.PropertyID);
            if (incomingProperty != null && !string.IsNullOrWhiteSpace(incomingProperty.PropertyValue))
            {
                var valueExists = await _assetRepository.PropertyValueExistsAsync(uniqueProperty.PropertyID, incomingProperty.PropertyValue, excludeAssetId);
                if (valueExists)
                {
                    _logger.LogWarning(
                        "Duplicate value for unique property. PropertyID: {PropertyId}, PropertyName: {PropertyName}, Value: {Value}",
                        uniqueProperty.PropertyID,
                        uniqueProperty.PropertyName,
                        incomingProperty.PropertyValue);
                    throw new BadRequestException(
                        $"The value for property '{uniqueProperty.PropertyName}' must be unique. The value '{incomingProperty.PropertyValue}' already exists.");
                }
            }
        }
    }

    public async Task<AssetPagedResponse> GetAllAssetsAsync(string? search, int pageNo)
    {
        int pageSize = 15;

        var assets = await _assetRepository.GetALLAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            assets = assets.Where(a =>
            a.AssetName.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var totalRecords = assets.Count();
        var pagedAssetEntities = assets
            .Skip((pageNo - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var categoryIds = pagedAssetEntities.Select(a => a.CategoryID).Distinct();
        var propertyPool = await _assetRepository.GetAssetPropertiesByCategoryIdsAsync(categoryIds);
        var propertiesByCategory = propertyPool
            .GroupBy(p => p.CategoryID)
            .ToDictionary(g => g.Key, g => g.Select(property => MapProperty(property)).ToList());

        var pagedAssets = pagedAssetEntities
            .Select(asset => new AssetResponse
            {
                AssetID = asset.AssetID,
                ResourceTypeName = asset.Category?.ResourceType?.TypeName ?? string.Empty,
                CategoryName = asset.Category?.CategoryName ?? string.Empty,
                AssetName = asset.AssetName,
                SerialNumber = asset.SerialNumber,
                PurchaseDateFormatted = asset.PurchaseDate.ToString("MM-dd-yyyy"),
                AssetCost = asset.AssetCost,
                VendorName = asset.VendorName ?? string.Empty,
                AssetStatus = asset.AssetStatus?.AssetStatusName ?? string.Empty,
                AssetHealthStatusName = asset.AssetHealthStatus?.AssetHealthStatusName ?? string.Empty,
                Description = asset.Description ?? string.Empty,
                AssignedEmployee = GetAssignedEmployeeNameOrDefault(asset.User),
                Properties = propertiesByCategory.TryGetValue(asset.CategoryID, out var linkedProperties)
                    ? linkedProperties
                    : new List<ResponseAssetProperty>()
            }).ToList();

        _logger.LogInformation("Assets fetched successfully.");

        return new AssetPagedResponse
        {
            Assets = pagedAssets,
            PageNo = pageNo,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
            Search = search
        };
    }

    public async Task<AssetResponse?> GetAssetByIdAsync(int id)
    {
        var asset = await _assetRepository.GetByIdAsync(id);

        if (asset == null)
        {
            _logger.LogWarning("Asset not found. ID: {AssetId}", id);
            throw new NotFoundException($"{CommonResource.AssetNotFound} ID {id}.");
        }

        string assignedEmployeeName = "Unassigned";
        if (asset.UserID.HasValue && asset.UserID > 0)
        {
            var user = await _assetRepository.GetUserByIdAsync(asset.UserID.Value);
            if (user != null)
            {
                assignedEmployeeName = GetAssignedEmployeeNameOrDefault(user);
            }
        }

        var linkedProperties = await _assetRepository.GetAssetPropertiesByCategoryIdAsync(asset.CategoryID);
        var propertyValuesById = asset.PropertyValues
            .GroupBy(propertyValue => propertyValue.PropertyID)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(propertyValue => propertyValue.CreatedDate)
                    .ThenByDescending(propertyValue => propertyValue.AssetPropertyValueID)
                    .First()
                    .Value);

        _logger.LogInformation("Asset fetched successfully.");
        return new AssetResponse
        {
            AssetID = asset.AssetID,
            ResourceTypeName = asset.Category?.ResourceType?.TypeName ?? string.Empty,
            AssetName = asset.AssetName,
            CategoryName = asset.Category?.CategoryName ?? string.Empty,
            SerialNumber = asset.SerialNumber,
            PurchaseDateFormatted = asset.PurchaseDate.ToString("MM-dd-yyyy"),
            AssetCost = asset.AssetCost,
            VendorName = asset.VendorName ?? string.Empty,
            AssetStatus = asset.AssetStatus?.AssetStatusName ?? string.Empty,
            AssetHealthStatusName = asset.AssetHealthStatus?.AssetHealthStatusName ?? string.Empty,
            Description = asset.Description ?? string.Empty,
            AssignedEmployee = assignedEmployeeName,
            Properties = linkedProperties.Select(property =>
            {
                propertyValuesById.TryGetValue(property.PropertyID, out var propertyValue);
                return MapProperty(property, propertyValue);
            }).ToList()
        };
    }

    public async Task<AssetAuditLogPagedResponse> GetAssetAuditLogsAsync(int assetId, int pageNo, int pageSize)
    {
        if (assetId <= 0)
        {
            _logger.LogWarning("GetAssetAuditLogsAsync called with invalid assetId: {AssetId}", assetId);
            throw new BadRequestException("Asset ID must be greater than zero.");
        }

        var asset = await _assetRepository.GetByIdAsync(assetId);
        if (asset == null)
        {
            _logger.LogWarning("Asset not found for audit logs. ID: {AssetId}", assetId);
            throw new NotFoundException($"{CommonResource.AssetNotFound} ID {assetId}.");
        }

        var (logs, totalRecords) = await _assetRepository.GetAssetAuditLogsAsync(assetId, pageNo, pageSize);

        var responseLogs = logs.Select(log => new AssetAuditLogResponse
        {
            LogId = log.AssetAuditLogID,
            DateTime = log.DateTimestamp.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
            ActionPerformed = log.ActionType?.ActionName ?? string.Empty,
            PerformedBy = log.PerformedBy == null
                ? string.Empty
                : $"{log.PerformedBy.FirstName} {log.PerformedBy.LastName}".Trim(),
            EmployeeName = IsAssignmentAction(log.ActionType?.ActionName) ? log.EmployeeName : null,
            EmployeeEmail = IsAssignmentAction(log.ActionType?.ActionName) ? log.EmployeeEmail : null
        }).ToList();

        return new AssetAuditLogPagedResponse
        {
            Data = new List<AssetAuditLogGroupResponse>
            {
                new AssetAuditLogGroupResponse
                {
                    AssetId = asset.AssetID,
                    AssetName = asset.AssetName,
                    AssetLogs = responseLogs,
                    Pagination = new AssetAuditLogPaginationResponse
                    {
                        Page = pageNo,
                        PageSize = pageSize,
                        TotalRecords = totalRecords,
                        Message = "Audit logs fetched successfully.",
                        StatusCode = 200
                    }
                }
            }
        };
    }

    public async Task<AssetAuditLogDetailPagedResponse> GetAssetAuditLogDetailAsync(int auditLogId)
    {
        if (auditLogId <= 0)
        {
            _logger.LogWarning("GetAssetAuditLogDetailAsync called with invalid auditLogId: {AuditLogId}", auditLogId);
            throw new BadRequestException(CommonResource.AuditLogIdInvalid);
        }

        var auditLog = await _assetRepository.GetAssetAuditLogByIdAsync(auditLogId);
        if (auditLog == null || auditLog.Details == null || !auditLog.Details.Any())
        {
            _logger.LogWarning("Audit log details not found. AuditLogId: {AuditLogId}", auditLogId);
            throw new NotFoundException(CommonResource.AuditLogNoFieldsUpdated);
        }

        var response = new AssetAuditLogDetailItemResponse
        {
            LogId = auditLog.AssetAuditLogID,
            AssetId = auditLog.AssetID,
            AssetName = auditLog.Asset?.AssetName ?? string.Empty,
            UpdatedDetails = auditLog.Details
                .OrderBy(detail => detail.AssetAuditLogDetailID)
                .Select(detail => new AssetAuditLogDetailResponse
                {
                    FieldName = detail.FieldName,
                    OldValue = FormatAuditLogDate(detail.FieldName, detail.OldValue),
                    NewValue = FormatAuditLogDate(detail.FieldName, detail.NewValue)
                })
                .ToList()
        };

        return new AssetAuditLogDetailPagedResponse
        {
            Data = new List<AssetAuditLogDetailItemResponse> { response },
            Message = CommonResource.AuditLogDetailsFetchedSuccessfully,
            StatusCode = 200
        };
    }

    private static string? FormatAuditLogDate(string? fieldName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(fieldName))
            return value;

        if (fieldName.EndsWith("Date", StringComparison.OrdinalIgnoreCase) &&
            DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        }

        return value;
    }



    public async Task<AssetResponse?> UpdateAssetRequestAsync(int id, UpdateAssetRequest updateAssetRequest, int modifiedById)
    {
        if (updateAssetRequest == null)
        {
            _logger.LogWarning("UpdateAssetRequestAsync — not found | AssetID: {AssetId}", id);
            throw new ArgumentNullException(nameof(updateAssetRequest), CommonResource.InvalidData);
        }

        if (modifiedById <= 0)
        {
            _logger.LogWarning("UpdateAssetRequestAsync — invalid authenticated user id: {ModifiedById}", modifiedById);
            throw new UnAuthServiceorizedException("Invalid authenticated user.");
        }

        var modifierExists = await _assetRepository.UserExistsAsync(modifiedById);
        if (!modifierExists)
        {
            _logger.LogWarning("UpdateAssetRequestAsync — authenticated user not found. ID {ModifiedById}", modifiedById);
            throw new UnAuthServiceorizedException("Authenticated user not found.");
        }

        ValidateUpdateTextFields(updateAssetRequest);

        var asset = await GetAssetForUpdateAsync(id);
        EnsureUpdateHasContent(updateAssetRequest);

        await ApplyCoreAssetUpdatesAsync(id, updateAssetRequest, asset, modifiedById);
        await ApplyAssetPropertyUpdatesAsync(id, updateAssetRequest, asset, modifiedById);

        _logger.LogInformation("Asset updated successfully.");

        return await BuildAssetResponseAsync(id);
    }

    private async Task<Asset> GetAssetForUpdateAsync(int id)
    {
        var asset = await _assetRepository.GetByIdAsync(id);
        if (asset == null)
        {
            _logger.LogWarning("Asset not found. ID: {AssetId}", id);
            throw new NotFoundException($"{CommonResource.AssetNotFound} ID {id}.");
        }

        return asset;
    }

    private static void EnsureUpdateHasContent(UpdateAssetRequest updateAssetRequest)
    {
        var hasPropertyUpdates = updateAssetRequest.AssetProperties != null && updateAssetRequest.AssetProperties.Count > 0;
        var hasCoreAssetUpdates =
            updateAssetRequest.AssetName != null ||
            updateAssetRequest.SerialNumber != null ||
            updateAssetRequest.CategoryID.HasValue ||
            updateAssetRequest.StatusID.HasValue ||
            updateAssetRequest.AssetHealthStatusID.HasValue ||
            updateAssetRequest.PurchaseDate.HasValue ||
            updateAssetRequest.AssetCost.HasValue ||
            updateAssetRequest.VendorName != null ||
            updateAssetRequest.Description != null;

        if (!hasCoreAssetUpdates && !hasPropertyUpdates)
        {
            throw new BadRequestException("No fields provided to update.");
        }
    }

    private async Task ApplyCoreAssetUpdatesAsync(int id, UpdateAssetRequest updateAssetRequest, Asset asset, int modifiedById)
    {
        if (!HasCoreAssetUpdates(updateAssetRequest))
        {
            return;
        }

        var resolvedCategoryId = updateAssetRequest.CategoryID ?? asset.CategoryID;
        int resolvedResourceTypeId = asset.Category?.ResourceTypeID ?? 0;
        byte? newResourceTypeId = null;

        if (updateAssetRequest.CategoryID.HasValue && updateAssetRequest.CategoryID.Value != asset.CategoryID)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync((byte)updateAssetRequest.CategoryID.Value);
            if (category == null)
            {
                _logger.LogWarning("Category not found. ID {CategoryId}", updateAssetRequest.CategoryID.Value);
                throw new NotFoundException($"{CommonResource.CategoryNotFound} ID {updateAssetRequest.CategoryID.Value}.");
            }

            if (category.DeletedDate != null)
            {
                _logger.LogWarning("Attempted to update asset to a deleted category. ID {CategoryId}", updateAssetRequest.CategoryID.Value);
                throw new BadRequestException(CommonResource.CategoryDeleted);
            }
            resolvedResourceTypeId = category.ResourceTypeID;
            newResourceTypeId = category.ResourceTypeID;
        }

        var resolvedStatusId = updateAssetRequest.StatusID ?? asset.StatusID;
        var resolvedAssetHealthStatusId = updateAssetRequest.AssetHealthStatusID ?? asset.AssetHealthStatusID;

        await ValidateLookupIdsAsync(
            resolvedResourceTypeId,
            resolvedCategoryId,
            resolvedStatusId,
            resolvedAssetHealthStatusId);

        if (updateAssetRequest.AssetName != null)
        {
            asset.AssetName = updateAssetRequest.AssetName;
        }

        if (updateAssetRequest.SerialNumber != null)
        {
            var serialExists = await _assetRepository.SerialNumberExistsAsync(updateAssetRequest.SerialNumber, id);
            if (serialExists)
            {
                _logger.LogWarning("Serial number already exists. SerialNumber: {SerialNumber}", updateAssetRequest.SerialNumber);
                throw new BadRequestException("Serial number must be unique.");
            }

            asset.SerialNumber = updateAssetRequest.SerialNumber;
        }

        if (updateAssetRequest.CategoryID.HasValue)
        {
            asset.CategoryID = (byte)updateAssetRequest.CategoryID.Value;
            if (newResourceTypeId.HasValue)
            {
                asset.ResourceTypeID = newResourceTypeId.Value;
            }
        }

        if (updateAssetRequest.StatusID.HasValue)
        {
            asset.StatusID = (byte)updateAssetRequest.StatusID.Value;
        }

        if (updateAssetRequest.PurchaseDate.HasValue)
        {
            asset.PurchaseDate = updateAssetRequest.PurchaseDate.Value;
        }

        if (updateAssetRequest.AssetCost.HasValue)
        {
            asset.AssetCost = updateAssetRequest.AssetCost;
        }

        if (updateAssetRequest.VendorName != null)
        {
            asset.VendorName = updateAssetRequest.VendorName;
        }

        if (updateAssetRequest.Description != null)
        {
            asset.Description = updateAssetRequest.Description;
        }

        if (updateAssetRequest.AssetHealthStatusID.HasValue)
        {
            if (updateAssetRequest.AssetHealthStatusID.Value <= 0 || updateAssetRequest.AssetHealthStatusID.Value > byte.MaxValue)
            {
                throw new NotFoundException("Asset health status not found.");
            }

            asset.AssetHealthStatusID = (byte)updateAssetRequest.AssetHealthStatusID.Value;
        }

        if (updateAssetRequest.HealthRemark != null)
        {
            asset.HealthRemark = updateAssetRequest.HealthRemark;
        }

        asset.ModifiedByID = modifiedById;
        asset.ModifiedDate = DateTime.UtcNow;
        await _assetRepository.UpdateAsync(asset);
    }

    private async Task ApplyAssetPropertyUpdatesAsync(int id, UpdateAssetRequest updateAssetRequest, Asset asset, int modifiedById)
    {
        var assetProperties = updateAssetRequest.AssetProperties;
        if (assetProperties == null || assetProperties.Count == 0)
        {
            return;
        }

        await ValidateAssetPropertiesAsync(assetProperties, asset.CategoryID, excludeAssetId: id);

        foreach (var property in assetProperties)
        {
            property.UserId = modifiedById;
            await AddAssetPropertyValueAsync(property, id);
        }
    }

    private async Task<AssetResponse> BuildAssetResponseAsync(int id)
    {
        var updatedAsset = await _assetRepository.GetByIdAsync(id);
        if (updatedAsset == null)
        {
            _logger.LogWarning("Asset not found after update. ID {AssetId}", id);
            throw new NotFoundException($"{CommonResource.AssetNotFound} ID {id}.");
        }

        var propertyValuesById = updatedAsset.PropertyValues
            .GroupBy(propertyValue => propertyValue.PropertyID)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(propertyValue => propertyValue.CreatedDate)
                    .ThenByDescending(propertyValue => propertyValue.AssetPropertyValueID)
                    .First()
                    .Value);

        return new AssetResponse
        {
            AssetID = updatedAsset.AssetID,
            ResourceTypeName = updatedAsset.Category?.ResourceType?.TypeName ?? string.Empty,
            CategoryName = updatedAsset.Category?.CategoryName ?? string.Empty,
            AssetName = updatedAsset.AssetName,
            SerialNumber = updatedAsset.SerialNumber,
            PurchaseDateFormatted = updatedAsset.PurchaseDate.ToString("dd-MM-yyyy"),
            AssetCost = updatedAsset.AssetCost,
            VendorName = updatedAsset.VendorName ?? string.Empty,
            Description = updatedAsset.Description ?? string.Empty,
            AssignedEmployee = GetAssignedEmployeeNameOrDefault(updatedAsset.User),
            Properties = (await _assetRepository.GetAssetPropertiesByCategoryIdAsync(updatedAsset.CategoryID))
                .Select(property =>
                {
                    propertyValuesById.TryGetValue(property.PropertyID, out var propertyValue);
                    return MapProperty(property, propertyValue);
                })
                .ToList()
        };
    }

    private static bool HasCoreAssetUpdates(UpdateAssetRequest updateAssetRequest)
    {
        return updateAssetRequest.AssetName != null ||
               updateAssetRequest.SerialNumber != null ||
               updateAssetRequest.CategoryID.HasValue ||
               updateAssetRequest.StatusID.HasValue ||
               updateAssetRequest.AssetHealthStatusID.HasValue ||
               updateAssetRequest.PurchaseDate.HasValue ||
               updateAssetRequest.AssetCost.HasValue ||
               updateAssetRequest.VendorName != null ||
               updateAssetRequest.Description != null;
    }

    private static void ValidateUpdateTextFields(UpdateAssetRequest updateAssetRequest)
    {
        if (updateAssetRequest.AssetName != null)
        {
            if (string.IsNullOrWhiteSpace(updateAssetRequest.AssetName))
            {
                throw new BadRequestException("Asset name cannot be empty.");
            }

            var trimmedAssetName = updateAssetRequest.AssetName.Trim();
            if (trimmedAssetName.Length <= 2 || trimmedAssetName.Length >= 50)
            {
                throw new BadRequestException("Asset name must be more than 2 and less than 50 characters.");
            }

            updateAssetRequest.AssetName = trimmedAssetName;
        }

        if (updateAssetRequest.SerialNumber != null)
        {
            if (string.IsNullOrWhiteSpace(updateAssetRequest.SerialNumber))
            {
                throw new BadRequestException("Serial number cannot be empty.");
            }

            updateAssetRequest.SerialNumber = updateAssetRequest.SerialNumber.Trim();
        }

        if (updateAssetRequest.VendorName != null)
        {
            var trimmedVendorName = updateAssetRequest.VendorName.Trim();
            if (trimmedVendorName.Length <= 2 || trimmedVendorName.Length >= 50)
            {
                throw new BadRequestException("Vendor name must be more than 2 and less than 50 characters.");
            }

            updateAssetRequest.VendorName = trimmedVendorName;
        }

        if (updateAssetRequest.AssetCost.HasValue && updateAssetRequest.AssetCost.Value <= 0)
        {
            throw new BadRequestException("Asset cost must be a positive value.");
        }
    }

    private static ResponseAssetProperty MapProperty(AssetProperty property, string? propertyValue = null)
    {
        return new ResponseAssetProperty
        {
            PropertyId = property.PropertyID,
            PropertyName = property.PropertyName,
            DataTypeID = property.DataTypeID,
            DataType = property.DataType?.DataTypeName ?? string.Empty,
            PropertyValue = propertyValue,
            IsUnique = property.IsUnique,
            IsMandatory = property.IsMandatory
        };
    }

    private async Task ValidateLookupIdsAsync(int resourceTypeId, int categoryId, int statusId, int? assetHealthStatusId = null)
    {
        if (resourceTypeId <= 0 || resourceTypeId > byte.MaxValue)
        {
            _logger.LogWarning("Resource type not found. Invalid ID {ResourceTypeId}", resourceTypeId);
            throw new NotFoundException($"{CommonResource.ResourceTypeNotFound} ID {resourceTypeId}.");
        }

        if (categoryId <= 0 || categoryId > byte.MaxValue)
        {
            _logger.LogWarning("Category not found. Invalid ID {CategoryId}", categoryId);
            throw new NotFoundException($"{CommonResource.CategoryNotFound} ID {categoryId}.");
        }

        if (statusId <= 0 || statusId > byte.MaxValue)
        {
            _logger.LogWarning("Status not found. Invalid ID {StatusId}", statusId);
            throw new NotFoundException($"{CommonResource.StatusNotFound} ID {statusId}.");
        }

        var resourceTypeExists = await _assetRepository.ResourceTypeExistsAsync(resourceTypeId);
        if (!resourceTypeExists)
        {
            _logger.LogWarning("Resource type not found. ID {ResourceTypeId}", resourceTypeId);
            throw new NotFoundException($"{CommonResource.ResourceTypeNotFound} ID {resourceTypeId}.");
        }

        var categoryExists = await _assetRepository.CategoryExistsAsync(categoryId);
        if (!categoryExists)
        {
            _logger.LogWarning("Category not found. ID {CategoryId}", categoryId);
            throw new NotFoundException($"{CommonResource.CategoryNotFound} ID {categoryId}.");
        }

        var statusExists = await _assetRepository.StatusExistsAsync(statusId);
        if (!statusExists)
        {
            _logger.LogWarning("Status not found. ID {StatusId}", statusId);
            throw new NotFoundException($"{CommonResource.StatusNotFound} ID {statusId}.");
        }

        if (assetHealthStatusId.HasValue)
        {
            if (assetHealthStatusId.Value <= 0)
            {
                _logger.LogWarning("Asset health status not found. Invalid ID {AssetHealthStatusId}", assetHealthStatusId);
                throw new BadRequestException("Asset health status value should be greater than zero.");
            }
            
            if (assetHealthStatusId.Value > byte.MaxValue)
            {
                _logger.LogWarning("Asset health status not found. Invalid ID {AssetHealthStatusId}", assetHealthStatusId);
                throw new NotFoundException("Asset health status not found.");
            }

            var healthExists = await _assetRepository.AssetHealthStatusExistsAsync(assetHealthStatusId.Value);
            if (!healthExists)
            {
                _logger.LogWarning("Asset health status not found. ID {AssetHealthStatusId}", assetHealthStatusId);
                throw new NotFoundException("Asset health status not found.");
            }
        }
    }

    public async Task<AssetStatusDistributionResponse> GetAssetStatusDistributionAsync(int? categoryId = null, string? categoryName = null)
    {
        var data = await _assetRepository.GetAssetStatusDistributionAsync(categoryId, categoryName);
        var list = data.ToList();

        return new AssetStatusDistributionResponse
        {
            TotalAssets = list.Sum(d => d.AssetCount),
            Statuses = list.Select(d => new AssetStatusDistributionItem
            {
                StatusName = d.StatusName,
                AssetCount = d.AssetCount,
                Percentage = d.Percentage
            }),
        };
    }

    private static bool IsAssignmentAction(string? actionName)
    {
        if (string.IsNullOrWhiteSpace(actionName))
            return false;

        return actionName.Equals("Assigned", StringComparison.OrdinalIgnoreCase) ||
               actionName.Equals("Unassigned", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetAssignedEmployeeNameOrDefault(User? user)
    {
        if (user == null)
        {
            return "Unassigned";
        }

        var employeeName = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrWhiteSpace(employeeName) ? "Unassigned" : employeeName;
    }
}
