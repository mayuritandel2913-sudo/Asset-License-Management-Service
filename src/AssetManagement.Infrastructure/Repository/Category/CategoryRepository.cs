using AssetManagement.Infrastructure.Contracts;
using AssetManagement.Infrastructure.Data;
using AssetManagement.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AssetManagement.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public CategoryRepository(ApplicationDbContext context, ILogger<CategoryRepository> logger)
    {
        _applicationDbContext = context;
    }

    public async Task<bool> ExistsByNameAsync(string name, byte? excludeId = null)
    {
        
        var normalizedName = System.Text.RegularExpressions.Regex.Replace(name.Trim(), @"\s+", " ").ToLowerInvariant();
        
        var query = _applicationDbContext.Category
            .Where(c => c.DeletedDate == null && c.IsActive)
            .AsEnumerable() 
            .Where(c => System.Text.RegularExpressions.Regex.Replace(c.CategoryName.Trim(), @"\s+", " ").ToLowerInvariant() == normalizedName);

        if (excludeId.HasValue)
            query = query.Where(c => c.CategoryID != excludeId.Value);

        return await Task.FromResult(query.Any());
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        _applicationDbContext.Category.Add(category);
        await _applicationDbContext.SaveChangesAsync();
        return category;
    }

    public async Task<Category?> GetCategoryByIdAsync(byte id)
    {
        return await _applicationDbContext.Category
            .Include(c => c.ResourceType)
            .Include(c => c.AssetProperties)
                .ThenInclude(ap => ap.DataType)
            .FirstOrDefaultAsync(c => c.CategoryID == id && c.IsActive);
    }

    public async Task<Category?> GetCategoryByNameAsync(string name)
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(normalizedName))
            return null;

        return await _applicationDbContext.Category
            .FirstOrDefaultAsync(c => c.IsActive && c.CategoryName.ToLower() == normalizedName);
    }

    public async Task<List<AssetProperty>> GetPropertiesByCategoryIdAsync(byte categoryId)
    {
        return await _applicationDbContext.AssetProperty
            .AsNoTracking()
            .Include(ap => ap.DataType)
            .Where(ap => ap.CategoryID == categoryId && ap.DeletedDate == null)
            .ToListAsync();
    }

    public async Task<(List<Category> Items, int Total)> GetAllCategoryAsync(string? search, int pageNo, int pageSize, string sortOrder = "asc")
    {
        var query = _applicationDbContext.Category
            .AsNoTracking()
            .Include(c => c.ResourceType)
            .Where(c => c.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.CategoryName.Contains(search.Trim()));

        query = sortOrder?.Trim().ToLowerInvariant() == "desc"
            ? query.OrderByDescending(c => c.CategoryName)
            : query.OrderBy(c => c.CategoryName);

        var total = await query.CountAsync();

        var items = await query
            .Skip((pageNo - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Category> UpdateCategoryAsync(Category category)
    {
        _applicationDbContext.Category.Update(category);
        await _applicationDbContext.SaveChangesAsync();
        return category;
    }

    public async Task DeleteCategoryAsync(Category category)
    {
        category.IsActive = false;

        _applicationDbContext.Category.Update(category);
        await _applicationDbContext.SaveChangesAsync();
    }
}
