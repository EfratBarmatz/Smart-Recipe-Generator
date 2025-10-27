using System.Collections.Generic;

namespace Smart_Recipe_Generator.Models
{
    public class RecipeRequest
    {
        public List<string> Ingredients { get; set; } = new List<string>();

        // Optional preferences (expand later)
        public Preferences? Preferences { get; set; }

        public int Servings { get; set; } = 1;
    }

    public class Preferences
    {
        public bool Vegetarian { get; set; }
        public bool Vegan { get; set; }
        public bool GlutenFree { get; set; }
        public int? MaxCalories { get; set; }
    }
}
