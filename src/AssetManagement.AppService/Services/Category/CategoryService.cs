using AssetManagement.AppService.Contracts;
using AssetManagement.AppService.DTOs;
using AssetManagement.Infrastructure.Contracts;
using AssetManagement.Infrastructure.Entities;
using AssetManagement.Utility.Exceptions;
using AssetManagement.Utility.Resource;
using Microsoft.Extensions.Logging;

namespace AssetManagement.AppService.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMasterService _masterService;
    private readonly IAssetRepository _assetRepository;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ICategoryRepository categoryRepository, IMasterService masterService, IAssetRepository assetRepository, ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _masterService = masterService;
        _assetRepository = assetRepository;
        _logger = logger;
    }

    public async Task<CategoryDetailsResponse> CreateCategoryAsync(CreateCategoryRequest request, int createdById)
    {
      
        var normalizedCategoryName = System.Text.RegularExpressions.Regex.Replace(request.CategoryName.Trim(), @"\s+", " ");
        
        if (await _categoryRepository.ExistsByNameAsync(normalizedCategoryName))
        {
            _logger.LogWarning("CreateAsync — duplicate name rejected | CategoryName: {CategoryName}", normalizedCategoryName);
            throw new BadRequestException(CommonResource.CategoryAlreadyExists);
        }

        await ValidateResourceTypeIdAsync(request.ResourceTypeID);

        ValidateDuplicatePropertyNames(request.CategoryProperties);

        var category = new Category
        {
            CategoryName = normalizedCategoryName,
            ResourceTypeID = request.ResourceTypeID,
            CreatedDate = DateTime.UtcNow,
            IsActive = true,
            AssetProperties = new List<AssetProperty>()
        };

        if (request.CategoryProperties != null)
        {
            foreach (var prop in request.CategoryProperties)
            {
                await ValidateDataTypeIdAsync(prop.DataTypeID);

                category.AssetProperties.Add(new AssetProperty
                {
                    PropertyName = prop.PropertyName,
                    DataTypeID = prop.DataTypeID,
                    IsUnique = prop.IsUnique,
                    IsMandatory = prop.IsMandatory,
                    CreatedByID = createdById,
                    CreatedDate = DateTime.UtcNow
                });
            }
        }

        var createdCategory = await _categoryRepository.CreateCategoryAsync(category);

        return MapToResponse(createdCategory);
    }

    public async Task<CategoryPagedResponse> GetAllCategoryAsync(string? search, int pageNo, string sortOrder = "asc")
    {
        int pageSize = 15;

        var (items, total) = await _categoryRepository.GetAllCategoryAsync(search, pageNo, pageSize, sortOrder);

        return new CategoryPagedResponse
        {
            Categories = items.Select(MapToListItem).ToList(),
            PageNo = pageNo,
            PageSize = pageSize,
            TotalRecords = total,
            TotalPages = (int)Math.Ceiling((double)total / pageSize),
            Search = search,
        };
    }

    public async Task<CategoryDetailsResponse> GetCategoryByIdAsync(byte categoryId)
    {
        var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
        if (category is null)
        {
            _logger.LogWarning("GetCategoryByIdAsync — not found | CategoryId: {CategoryId}", categoryId);
            throw new NotFoundException(CommonResource.CategoryNotFound);
        }

        return MapToResponse(category);
    }

    public async Task<CategoryDetailsResponse> UpdateCategoryAsync(byte categoryId, UpdateCategoryRequest request, int modifiedById)
    {
        var existingCategory = await _categoryRepository.GetCategoryByIdAsync(categoryId);
        if (existingCategory is null)
        {
            _logger.LogWarning("UpdateCategoryAsync — not found | CategoryId: {CategoryId}", categoryId);
            throw new NotFoundException(CommonResource.CategoryNotFound);
        }

      
        var normalizedCategoryName = System.Text.RegularExpressions.Regex.Replace(request.CategoryName.Trim(), @"\s+", " ");

        if (await _categoryRepository.ExistsByNameAsync(normalizedCategoryName, categoryId))
        {
            _logger.LogWarning("UpdateCategoryAsync — duplicate name rejected | CategoryId: {CategoryId} | CategoryName: {CategoryName}", categoryId, normalizedCategoryName);
            throw new BadRequestException(CommonResource.CategoryAlreadyExists);
        }

        await ValidateResourceTypeIdAsync(request.ResourceTypeID);

        ValidateDuplicatePropertyNames(existingCategory, request.CategoryProperties);

        await ValidatePropertyDataTypesAsync(request.CategoryProperties);

        existingCategory.CategoryName = normalizedCategoryName;
        existingCategory.ResourceTypeID = request.ResourceTypeID;

        var incomingPropertyIds = ApplyIncomingPropertyChanges(existingCategory, request.CategoryProperties, modifiedById, categoryId);
        SoftDeleteMissingProperties(existingCategory, incomingPropertyIds, modifiedById);

        var updatedCategory = await _categoryRepository.UpdateCategoryAsync(existingCategory);

        return MapToResponse(updatedCategory);
    }

    private List<byte> ApplyIncomingPropertyChanges(
        Category existingCategory,
        List<UpdateCategoryPropertyRequest>? categoryProperties,
        int modifiedById,
        byte categoryId)
    {
        var incomingPropertyIds = new List<byte>();
        if (categoryProperties == null)
        {
            return incomingPropertyIds;
        }

        foreach (var propRequest in categoryProperties)
        {
            if (propRequest.PropertyID.HasValue && propRequest.PropertyID.Value > 0)
            {
                UpdateExistingProperty(existingCategory, propRequest, incomingPropertyIds, modifiedById, categoryId);
                continue;
            }

            AddNewProperty(existingCategory, propRequest, modifiedById);
        }

        return incomingPropertyIds;
    }

    private void UpdateExistingProperty(
        Category existingCategory,
        UpdateCategoryPropertyRequest propRequest,
        List<byte> incomingPropertyIds,
        int modifiedById,
        byte categoryId)
    {
        var propertyId = propRequest.PropertyID!.Value;
        incomingPropertyIds.Add(propertyId);

        var existingProp = existingCategory.AssetProperties
            .FirstOrDefault(p => p.PropertyID == propertyId && p.DeletedDate == null);
        if (existingProp == null)
        {
            _logger.LogWarning("UpdateCategoryAsync — Invalid property ID | CategoryId: {CategoryId} | PropertyId: {PropertyId}", categoryId, propRequest.PropertyID);
            throw new BadRequestException(CommonResource.PropertyInvalid);
        }

        existingProp.PropertyName = propRequest.PropertyName;
        existingProp.DataTypeID = propRequest.DataTypeID;
        existingProp.IsUnique = propRequest.IsUnique;
        existingProp.IsMandatory = propRequest.IsMandatory;
        existingProp.ModifiedByID = modifiedById;
        existingProp.ModifiedDate = DateTime.UtcNow;
    }

    private static void AddNewProperty(Category existingCategory, UpdateCategoryPropertyRequest propRequest, int modifiedById)
    {
        existingCategory.AssetProperties.Add(new AssetProperty
        {
            PropertyName = propRequest.PropertyName,
            DataTypeID = propRequest.DataTypeID,
            IsUnique = propRequest.IsUnique,
            IsMandatory = propRequest.IsMandatory,
            CreatedByID = modifiedById,
            CreatedDate = DateTime.UtcNow
        });
    }

    private static void SoftDeleteMissingProperties(Category existingCategory, List<byte> incomingPropertyIds, int modifiedById)
    {
        var propertiesToDelete = existingCategory.AssetProperties
            .Where(p => p.PropertyID > 0 && p.DeletedDate == null && !incomingPropertyIds.Contains(p.PropertyID))
            .ToList();

        foreach (var propToDelete in propertiesToDelete)
        {
            propToDelete.DeletedByID = modifiedById;
            propToDelete.DeletedDate = DateTime.UtcNow;
        }
    }

    public async Task DeleteCategoryAsync(byte categoryId, int deletedById)
    {
        var existingCategory = await _categoryRepository.GetCategoryByIdAsync(categoryId);
        if (existingCategory is null)
        {
            _logger.LogWarning("DeleteCategoryAsync — not found | CategoryId: {CategoryId}", categoryId);
            throw new NotFoundException(CommonResource.CategoryNotFound);
        }

      
        var hasAssets = await _assetRepository.AnyAssetsForCategoryAsync(categoryId);
        if (hasAssets)
        {
            _logger.LogWarning("DeleteCategoryAsync — category has assets and cannot be deleted | CategoryId: {CategoryId}", categoryId);
            throw new BadRequestException(CommonResource.CategoryHasAssets);
        }

        existingCategory.DeletedDate = DateTime.UtcNow;

    
        if (existingCategory.AssetProperties != null)
        {
            foreach (var prop in existingCategory.AssetProperties)
            {
                if (prop.DeletedDate == null)
                {
                    prop.DeletedDate = DateTime.UtcNow;
                    prop.DeletedByID = deletedById;
                }
            }
        }

        await _categoryRepository.DeleteCategoryAsync(existingCategory);
    }

    private static CategoryDetailsResponse MapToResponse(Category category) => new()
    {
        CategoryID = category.CategoryID,
        CategoryName = category.CategoryName,
        ResourceTypeID = category.ResourceTypeID,
        ResourceTypeName = category.ResourceType?.TypeName ?? string.Empty,
        CreatedDate = category.CreatedDate,
        CategoryProperties = category.AssetProperties?
            .Where(p => p.DeletedDate == null)
            .Select(p => new ResponseAssetProperty
            {
                PropertyId = p.PropertyID,
                PropertyName = p.PropertyName,
                DataTypeID = p.DataTypeID,
                DataType = p.DataType?.DataTypeName ?? string.Empty,
                IsUnique = p.IsUnique,
                IsMandatory = p.IsMandatory,
                PropertyValue = null 
            }).ToList() ?? new List<ResponseAssetProperty>()
    };

    private static CategoryListItemResponse MapToListItem(Category category) => new()
    {
        CategoryID = category.CategoryID,
        CategoryName = category.CategoryName,
        ResourceTypeName = category.ResourceType?.TypeName ?? string.Empty,
        IsActive = category.IsActive
    };

    private async Task ValidateResourceTypeIdAsync(byte resourceTypeId)
    {
        var resourceTypes = await _masterService.GetResourceTypesAsync();
        if (!resourceTypes.Any(x => x.ResourceTypeID == resourceTypeId))
        {
            _logger.LogWarning("ValidateResourceTypeIdAsync — resource type not found | ResourceTypeID: {ResourceTypeID}", resourceTypeId);
            throw new NotFoundException(CommonResource.ResourceTypeNotFound);
        }
        
    }
    private async Task ValidateDataTypeIdAsync(byte dataTypeId)
    {
        var dataTypes = await _masterService.GetDataTypesAsync();
        if (!dataTypes.Any(x => x.DataTypeID == dataTypeId))
        {
            _logger.LogWarning("ValidateDataTypeIdAsync — data type not found | DataTypeID: {DataTypeID}", dataTypeId);
            throw new NotFoundException(CommonResource.DataTypeNotFound);
        }
        
    }

    private void ValidateDuplicatePropertyNames(IEnumerable<CreateCategoryPropertyRequest>? properties)
    {
        if (properties == null || !properties.Any())
            return;

        var propertyNames = properties.Select(p => p.PropertyName.Trim().ToLower()).ToList();
        var duplicates = propertyNames.GroupBy(x => x).Where(g => g.Count() > 1).ToList();

        if (duplicates.Any())
        {
            _logger.LogWarning("ValidateDuplicatePropertyNames — duplicate property names found | PropertyNames: {PropertyNames}", 
                string.Join(", ", duplicates.Select(d => d.Key)));
            throw new BadRequestException(CommonResource.DuplicatePropertyName);
        }
    }

    private void ValidateDuplicatePropertyNames(IEnumerable<UpdateCategoryPropertyRequest>? properties)
    {
        if (properties == null || !properties.Any())
            return;

        var propertyNames = properties.Select(p => p.PropertyName.Trim().ToLower()).ToList();
        var duplicates = propertyNames.GroupBy(x => x).Where(g => g.Count() > 1).ToList();

        if (duplicates.Any())
        {
            _logger.LogWarning("ValidateDuplicatePropertyNames — duplicate property names found | PropertyNames: {PropertyNames}", 
                string.Join(", ", duplicates.Select(d => d.Key)));
            throw new BadRequestException(CommonResource.DuplicatePropertyName);
        }
    }

    private void ValidateDuplicatePropertyNames(Category existingCategory, IEnumerable<UpdateCategoryPropertyRequest>? properties)
    {
        if (properties == null || !properties.Any())
            return;

        var normalizedRequestNames = properties
            .Select(p => p.PropertyName.Trim().ToLower())
            .ToList();

        var duplicates = normalizedRequestNames.GroupBy(x => x).Where(g => g.Count() > 1).ToList();
        if (duplicates.Any())
        {
            _logger.LogWarning("ValidateDuplicatePropertyNames — duplicate property names found in request | PropertyNames: {PropertyNames}",
                string.Join(", ", duplicates.Select(d => d.Key)));
            throw new BadRequestException(CommonResource.DuplicatePropertyName);
        }

        var existingProperties = existingCategory.AssetProperties
            .Where(p => p.DeletedDate == null)
            .ToDictionary(p => p.PropertyID, p => p.PropertyName.Trim().ToLower());

        var updatedPropertyIds = properties
            .Where(p => p.PropertyID.HasValue && p.PropertyID.Value > 0)
            .Select(p => p.PropertyID!.Value)
            .ToHashSet();

        var existingNamesToCompare = existingProperties
            .Where(p => !updatedPropertyIds.Contains(p.Key))
            .Select(p => p.Value)
            .ToHashSet();

        var conflicts = normalizedRequestNames.Intersect(existingNamesToCompare).ToList();
        if (conflicts.Any())
        {
            _logger.LogWarning("ValidateDuplicatePropertyNames — duplicate property names found against existing properties | PropertyNames: {PropertyNames}",
                string.Join(", ", conflicts));
            throw new BadRequestException(CommonResource.DuplicatePropertyName);
        }
    }

    private async Task ValidatePropertyDataTypesAsync(IEnumerable<UpdateCategoryPropertyRequest>? properties)
    {
        if (properties == null || !properties.Any())
            return;

        foreach (var prop in properties)
        {
            await ValidateDataTypeIdAsync(prop.DataTypeID);
        }
    }
}
