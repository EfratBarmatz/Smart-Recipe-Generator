using Smart_Recipe_Generator.Models;

namespace Smart_Recipe_Generator.Services
{
    public interface ICategoryService
    {
        Task<Category> AddCategoryAsync(Category cat);
        Task<bool> DeleteCategoryAsync(int id);
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int id);
    }
}