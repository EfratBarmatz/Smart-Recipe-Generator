import { Component } from '@angular/core';
import { ProductCard } from './components/product-card/product-card';
import { Category } from './components/category/category';
import { CommonModule } from '@angular/common';
import { RecipeService } from './services/recipe';
import { RecipeRequest, Preferences } from './models/recipe-request';
import { RecipeResponse } from './models/recipe-response';
import { QuicklyRecipe } from './components/quickly-recipe/quickly-recipe';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrls: ['./app.css'],
  standalone: true,
  imports: [ProductCard, Category, CommonModule, QuicklyRecipe],
})
export class App {
  currentView: 'category' | 'products' | 'recipe' = 'category';
  selectedCategory: string = '';
  selectedProducts: string[] = [];
  servings: number = 2;
  
  // הוספת משתנים חסרים
  preferences: Preferences = {
    vegetarian: false,
    vegan: false,
    glutenFree: false,
    maxCalories: undefined
  };
  
  recipeResponse: RecipeResponse | null = null;
  isLoading: boolean = false;
  error: string | null = null;

  constructor(private recipeService: RecipeService) { }

  onCategorySelected(categoryId: string): void {
    this.selectedCategory = categoryId;
    this.currentView = 'products';
  }

  onBackToCategories(): void {
    this.currentView = 'category';
    this.selectedCategory = '';
  }

  onGenerateRecipe(data: { products: string[], servings: number }): void {
    this.selectedProducts = data.products;
    this.servings = data.servings;
    this.currentView = 'recipe';
    this.isLoading = true;
    this.error = null;

    const request: RecipeRequest = {
      ingredients: this.selectedProducts,
      preferences: this.preferences,
      servings: this.servings
    };

    console.log('Sending request:', request);

    this.recipeService.generateRecipe(request).subscribe({
      next: (response) => {
        this.recipeResponse = response;
        this.isLoading = false;
        console.log('Recipe generated:', response);
      },
      error: (err) => {
        this.error = 'שגיאה ביצירת המתכון. אנא נסה שוב.';
        this.isLoading = false;
        console.error('Error generating recipe:', err);
      }
    });
  }

  onBackToProducts(): void {
    this.currentView = 'products';
  }

  onCreateNewRecipe(): void {
    this.currentView = 'category';
    this.selectedCategory = '';
    this.selectedProducts = [];
    this.servings = 2;
    this.recipeResponse = null;
    this.error = null;
  }

  // פונקציות לעדכון preferences
  updatePreferences(prefs: Partial<Preferences>): void {
    this.preferences = { ...this.preferences, ...prefs };
  }
}