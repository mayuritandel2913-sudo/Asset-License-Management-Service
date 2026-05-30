using AssetManagement.Infrastructure.Entities;

namespace AssetManagement.Infrastructure.Contracts;

public interface ICategoryRepository
{
    Task<Category> CreateCategoryAsync(Category category);
    Task<bool> ExistsByNameAsync(string name, byte? excludeId = null);
    Task<Category?> GetCategoryByIdAsync(byte id);
    Task<Category?> GetCategoryByNameAsync(string name);
    Task<List<AssetProperty>> GetPropertiesByCategoryIdAsync(byte categoryId);
    Task<(List<Category> Items, int Total)> GetAllCategoryAsync(string? search, int pageNo, int pageSize, string sortOrder = "asc");
    Task<Category> UpdateCategoryAsync(Category category);
    Task DeleteCategoryAsync(Category category);
}
