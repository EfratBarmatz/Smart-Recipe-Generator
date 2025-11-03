export interface RecipeRequest {
  ingredients: string[];
  preferences?: Preferences;
  servings: number;
}

export interface Preferences {
  vegetarian: boolean;
  vegan: boolean;
  glutenFree: boolean;
  maxCalories?: number;
}