using Smart_Recipe_Generator.Models;
using Smart_Recipe_Generator.Repository;

namespace Smart_Recipe_Generator.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAiRecipeService _aiService;

        public ProductService(
            IProductRepository repository,
            ICategoryRepository categoryRepository,
            IAiRecipeService aiService)
        {
            _repository = repository;
            _categoryRepository = categoryRepository;
            _aiService = aiService;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync(int? categoryId = null)
        {
            return await _repository.GetAllAsync(categoryId);
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Product> AddProductAsync(Product pro,int catId)
        {
            // בדיקה שהקטגוריה קיימת
            if (!await _categoryRepository.ExistsAsync(catId))
            {
                throw new InvalidOperationException("קטגוריה לא קיימת");
            }

            // אימות AI
            bool validation = await _aiService.ValidateProductAsync(pro.Name);
            if (!validation)
            {
                throw new InvalidOperationException("שם המוצר אינו תקין.");
            }

            // בחירת צבע אקראי
            var productColors = new[]
            {
                "from-pink-400", "from-red-400", "from-orange-400", "from-yellow-400",
                "from-green-400", "from-teal-400", "from-blue-400", "from-purple-400",
                "from-gray-400", "from-indigo-400"
            };
            var randomColor = productColors[new Random().Next(productColors.Length)];

            var product = new Product
            {
                Name = pro.Name.Trim(),
                CategoryId = catId,
                Emoji = pro.Emoji.Trim(),
                Color = randomColor
            };

            return await _repository.AddAsync(product);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _repository.GetProductsByCategoryAsync(categoryId);
        }
    }
}
