using Microsoft.EntityFrameworkCore;
using Smart_Recipe_Generator.Models;

namespace Smart_Recipe_Generator.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly RecipeDbContext _context;

        public CategoryRepository(RecipeDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.CategoryId)
                .ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<Category> AddAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Categories.AnyAsync(c => c.CategoryId == id);
        }
    }
}
