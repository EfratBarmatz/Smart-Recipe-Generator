using Smart_Recipe_Generator.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Smart_Recipe_Generator.Services
{
    // Simple placeholder implementation — replace with real AI client later
    public class RecipeService : IRecipeService
    {
        public Task<RecipeResponse> GenerateRecipeAsync(RecipeRequest request)
        {
            // Very simple generation logic for MVP/demo purposes
            var title = request.Ingredients != null && request.Ingredients.Count > 0
                ? "Quick " + string.Join(" & ", request.Ingredients.Take(2)) + " Dish"
                : "Quick Recipe";

            var ingredients = request.Ingredients.Select(i => i.Trim()).Where(i => !string.IsNullOrEmpty(i)).ToList();

            var steps = new System.Collections.Generic.List<string>();
            steps.Add($"Prepare the following ingredients: {string.Join(", ", ingredients)}.");
            steps.Add("Combine ingredients in a pan and cook for 8-12 minutes, adjust seasoning to taste.");
            steps.Add("Serve hot.");

            // Very naive nutrition estimation: 120 kcal per ingredient (placeholder)
            var calories = ingredients.Count * 120;

            var response = new RecipeResponse
            {
                Title = title,
                Ingredients = ingredients,
                Steps = steps,
                Nutrition = new NutritionInfo
                {
                    Calories = calories,
                    ProteinGrams = System.Math.Round(ingredients.Count * 3.5, 1),
                    FatGrams = System.Math.Round(ingredients.Count * 5.0, 1),
                    CarbsGrams = System.Math.Round(ingredients.Count * 12.0, 1),
                },
                ImageUrl = null,
                Servings = request.Servings <= 0 ? 1 : request.Servings
            };

            return Task.FromResult(response);
        }
    }
}
