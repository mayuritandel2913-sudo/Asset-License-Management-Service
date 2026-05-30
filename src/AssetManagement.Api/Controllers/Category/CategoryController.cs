using AssetManagement.AppService.Contracts;
using AssetManagement.AppService.DTOs;
using AssetManagement.Utility;
using AssetManagement.Utility.Resource;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AssetManagement.Api.Controllers;

[ApiController]
[Route("api/admin/")]
public class CategoryController : BaseApiController
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    [Authorize(Roles = "ITAdmin")]
    [HttpPost("createcategory")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var createdById = GetCurrentUserId();
        _logger.LogInformation("CreateCategory | CategoryName: {CategoryName} | ResourceTypeID: {ResourceTypeID}", request.CategoryName, request.ResourceTypeID);

        var result = await _categoryService.CreateCategoryAsync(request, createdById);

        return StatusCode(201, Envelope.Ok(
            new { CategoryID = result.CategoryID },
            CommonResource.CategoryCreatedSuccessfully,
            201
        ));
    }

    [Authorize(Roles = "ITAdmin")]
    [HttpGet("getallcategory")]
    public async Task<IActionResult> GetAllCategories(
        [FromQuery] string? search,
        [FromQuery] int pageNo = 1,
        [FromQuery] string sortOrder = "asc")
    {
        _logger.LogInformation("GetAllCategories | Search: {Search} | PageNo: {PageNo} | SortOrder: {SortOrder}", search ?? "none", pageNo, sortOrder);

        var result = await _categoryService.GetAllCategoryAsync(search, pageNo, sortOrder);

        string message = CommonResource.DataFetchedSuccessfully;
        if (result.TotalRecords == 0 && !string.IsNullOrWhiteSpace(search))
        {
            _logger.LogInformation("No categories found for search term '{SearchTerm}'", search);
            message = CommonResource.CategoryNotFoundForSearch;
        }

        return StatusCode(200, Envelope.Ok(result, message, 200));
    }

    [Authorize(Roles = "ITAdmin")]
    [HttpGet("getcategory/{categoryId}")]
    public async Task<IActionResult> GetCategory(byte categoryId)
    {
        _logger.LogInformation("GetCategory | CategoryId: {CategoryId}", categoryId);

        var result = await _categoryService.GetCategoryByIdAsync(categoryId);

        return StatusCode(200, Envelope.Ok(result, CommonResource.DataFetchedSuccessfully, 200));
    }

    [Authorize(Roles = "ITAdmin")]
    [HttpPut("updatecategory/{categoryId}")]
    public async Task<IActionResult> UpdateCategory(byte categoryId, [FromBody] UpdateCategoryRequest request)
    {
        var modifiedById = GetCurrentUserId();
        _logger.LogInformation("UpdateCategory | CategoryId: {CategoryId} | NewName: {CategoryName}", categoryId, request.CategoryName);

        var result = await _categoryService.UpdateCategoryAsync(categoryId, request, modifiedById);

        return StatusCode(201, Envelope.Ok(
            new { CategoryID = result.CategoryID },
            CommonResource.CategoryUpdatedSuccessfully,
            201
        ));
    }

    [Authorize(Roles = "ITAdmin")]
    [HttpDelete("deletecategory/{categoryId}")]
    public async Task<IActionResult> DeleteCategory(byte categoryId)
    {
        var deletedById = GetCurrentUserId();
        _logger.LogInformation("DeleteCategory | CategoryId: {CategoryId} | DeletedBy: {DeletedById}", categoryId, deletedById);

        await _categoryService.DeleteCategoryAsync(categoryId, deletedById);

        return Ok(new { message = CommonResource.CategoryDeletedSuccessfully });
    }

    private int GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdValue, out var userId) ? userId : 0;
    }
}
