export interface RecipeResponse {
  title: string;
  description: string;
  ingredients: string[];
  steps: string[];
  nutrition?: NutritionInfo;
  imageUrl?: string;
  servings: number;
  tip: string;
}

export interface NutritionInfo {
  calories: number;
  proteinGrams: number;
  fatGrams: number;
  carbsGrams: number;
}