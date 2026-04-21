using CategoryService.Application.Interfaces;
using CategoryService.Domain.Entities;

namespace CategoryService.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid id)
    {
        return await _categoryRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await _categoryRepository.GetAllAsync();
    }

    public async Task<Category> CreateCategoryAsync(string name, string description)
    {
        var category = new Category
        {
            Name = name,
            Description = description
        };
        await _categoryRepository.AddAsync(category);
        return category;
    }

    public async Task UpdateCategoryAsync(Guid id, string name, string description)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category != null)
        {
            category.Name = name;
            category.Description = description;
            category.LastModifiedAt = DateTime.UtcNow;
            await _categoryRepository.UpdateAsync(category);
        }
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        await _categoryRepository.DeleteAsync(id);
    }
}
