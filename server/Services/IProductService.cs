using Smart_Recipe_Generator.Models;

namespace Smart_Recipe_Generator.Services
{
    public interface IProductService
    {
        Task<Product> AddProductAsync(Product pro,int catId);
        Task<bool> DeleteProductAsync(int id);
        Task<IEnumerable<Product>> GetAllProductsAsync(int? categoryId = null);
        Task<Product?> GetProductByIdAsync(int id);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
    }
}