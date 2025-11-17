using Microsoft.EntityFrameworkCore;
using Smart_Recipe_Generator.Models;
using System;

namespace Smart_Recipe_Generator.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly RecipeDbContext _context;

        public ProductRepository(RecipeDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync(int? categoryId = null)
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            return await query.OrderBy(p => p.ProductId).ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task<Product> AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // טען את הקטגוריה
            await _context.Entry(product).Reference(p => p.Category).LoadAsync();

            return product;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
        }



    }

    
}
