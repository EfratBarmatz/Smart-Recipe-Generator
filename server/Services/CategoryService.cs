using Smart_Recipe_Generator.Models;
using Smart_Recipe_Generator.Repository;

namespace Smart_Recipe_Generator.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly IAiRecipeService _aiService;

        public CategoryService(ICategoryRepository repository, IAiRecipeService aiService)
        {
            _repository = repository;
            _aiService = aiService;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Category> AddCategoryAsync(Category cat)
        {
            // אימות AI
            bool validation = await _aiService.ValidateCategoryAsync(cat.Name);
            if(!validation)
            {
                throw new InvalidOperationException("שם הקטגוריה אינו תקין.");
            }
            // בחירת צבע אקראי
            var colors = new[]
            {
                "from-pink-500", "from-green-500", "from-blue-400", "from-orange-500",
                "from-teal-500", "from-red-500", "from-lime-500", "from-yellow-500",
                "from-purple-500", "from-indigo-500", "from-cyan-500", "from-amber-500"
            };
            var randomColor = colors[new Random().Next(colors.Length)];

            var category = new Category
            {
                Name = cat.Name.Trim(),
                Emoji = cat.Emoji.Trim(),
                Color = randomColor
            };

            return await _repository.AddAsync(category);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}
