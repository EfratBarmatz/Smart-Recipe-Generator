using Smart_Recipe_Generator.Models;

namespace Smart_Recipe_Generator.Repository
{
    public interface IProductRepository
    {
        Task<Product> AddAsync(Product product);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Product>> GetAllAsync(int? categoryId = null);
        Task<Product?> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
       

    }
}