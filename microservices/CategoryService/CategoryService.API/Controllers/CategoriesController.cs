using CategoryService.Application.Interfaces;
using CategoryService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CategoryService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null) return NotFound();
        return Ok(category);
    }

    [HttpPost]
    [Authorize] // Only authenticated users can create categories (Admin/Instructor logic can be added)
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        var category = await _categoryService.CreateCategoryAsync(request.Name, request.Description);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        await _categoryService.UpdateCategoryAsync(id, request.Name, request.Description);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _categoryService.DeleteCategoryAsync(id);
        return NoContent();
    }
}

public record CreateCategoryRequest(string Name, string Description);
public record UpdateCategoryRequest(string Name, string Description);
