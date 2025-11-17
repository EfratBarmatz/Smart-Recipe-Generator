using Smart_Recipe_Generator.Models;

namespace Smart_Recipe_Generator.Repository
{
    public interface ICategoryRepository
    {
        Task<Category> AddAsync(Category category);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
    }
}