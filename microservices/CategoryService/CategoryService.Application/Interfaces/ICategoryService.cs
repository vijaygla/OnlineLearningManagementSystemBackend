using CategoryService.Domain.Entities;

namespace CategoryService.Application.Interfaces;

public interface ICategoryService
{
    Task<Category?> GetCategoryByIdAsync(Guid id);
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task<Category> CreateCategoryAsync(string name, string description);
    Task UpdateCategoryAsync(Guid id, string name, string description);
    Task DeleteCategoryAsync(Guid id);
}
