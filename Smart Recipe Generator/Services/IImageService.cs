using System.Threading.Tasks;

namespace Smart_Recipe_Generator.Services
{
    public interface IImageService
    {
        Task<string> GenerateRecipeImageAsync(string recipeDescription);
    }
}