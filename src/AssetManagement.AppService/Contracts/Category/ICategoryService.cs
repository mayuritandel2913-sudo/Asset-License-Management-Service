using AssetManagement.AppService.DTOs;

namespace AssetManagement.AppService.Contracts;

public interface ICategoryService
{
    Task<CategoryDetailsResponse> CreateCategoryAsync(CreateCategoryRequest request, int createdById);
    Task<CategoryPagedResponse> GetAllCategoryAsync(string? search, int pageNo, string sortOrder = "asc");
    Task<CategoryDetailsResponse> GetCategoryByIdAsync(byte categoryId);
    Task<CategoryDetailsResponse> UpdateCategoryAsync(byte categoryId, UpdateCategoryRequest request, int modifiedById);
    Task DeleteCategoryAsync(byte categoryId, int deletedById);
}