using System.Collections.Generic;

namespace Smart_Recipe_Generator.Models
{
    public class RecipeResponse
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; }
        public List<string> Ingredients { get; set; } = new List<string>();
        public List<string> Steps { get; set; } = new List<string>();
        public NutritionInfo? Nutrition { get; set; }
        public string? ImageUrl { get; set; }
        public int Servings { get; set; } = 1;
    }

    public class NutritionInfo
    {
        public int Calories { get; set; }
        public double ProteinGrams { get; set; }
        public double FatGrams { get; set; }
        public double CarbsGrams { get; set; }
    }
}
