using Smart_Recipe_Generator.Models;
using System.Threading.Tasks;

namespace Smart_Recipe_Generator.Services
{
    // AI-specific interface. Declares the GenerateRecipeAsync contract independently
    // from the general IRecipeService so callers can inject the AI implementation
    // specifically when needed.
    public interface IAiRecipeService
    {
        Task<RecipeResponse> GenerateRecipeAsync(RecipeRequest request);
        Task<bool> ValidateCategoryAsync(string name);
        Task<bool> ValidateProductAsync(string name);
    }
}
