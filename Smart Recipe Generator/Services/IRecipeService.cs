using Smart_Recipe_Generator.Models;
using System.Threading.Tasks;

namespace Smart_Recipe_Generator.Services
{
    public interface IRecipeService
    {
        Task<RecipeResponse> GenerateRecipeAsync(RecipeRequest request);
    }
}
