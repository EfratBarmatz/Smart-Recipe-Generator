import { Component, signal } from '@angular/core';
import { QuicklyRecipe } from './components/quickly-recipe/quickly-recipe';
import { ProductCard } from './components/product-card/product-card';
import { Category } from './components/category/category';
import { RecipeService } from './services/recipe';
import { RecipeRequest, Preferences } from './models/recipe-request';
import { RecipeResponse } from './models/recipe-response';

@Component({
  selector: 'app-root',
  imports: [QuicklyRecipe, ProductCard, Category],
  templateUrl: './app.html',
  styleUrl: './app.css',
  standalone: true
})
export class App {
  protected readonly title = signal('smart-recipe-client');
  selectedCategory = '';
  products = [
    { name: 'Apple', category: 'Fruits', imageUrl: 'https://via.placeholder.com/150', selected: false },
    { name: 'Banana', category: 'Fruits', imageUrl: 'https://via.placeholder.com/150', selected: false },
    { name: 'Carrot', category: 'Vegetables', imageUrl: 'https://via.placeholder.com/150', selected: false },
    { name: 'Milk', category: 'Dairy', imageUrl: 'https://via.placeholder.com/150', selected: false },
    { name: 'Cinnamon', category: 'Spices', imageUrl: 'https://via.placeholder.com/150', selected: false }
  ];

  recipeResponse: RecipeResponse | null = null;
  loading = false;

  preferences: Preferences = {
    vegetarian: false,
    vegan: false,
    glutenFree: false
  };
  servings = 1;

  constructor(private recipeService: RecipeService) {}

  onCategorySelected(category: string) {
    this.selectedCategory = category;
  }

 // app.ts
onToggleProduct(product: any) {
  // עדכון המוצר המקורי במערך
  const originalProduct = this.products.find(p => p.name === product.name);
  if (originalProduct) {
    originalProduct.selected = product.selected;
  }
  console.log('Product toggled:', product.name, 'Selected:', product.selected);
}

  // פונקציה שאוספת את כל המוצרים המסומנים
  getSelectedProducts(): string[] {
    return this.products
      .filter(p => p.selected)
      .map(p => p.name);
  }

  generateRecipe() {
    const selectedIngredients = this.getSelectedProducts();
    
    if (selectedIngredients.length === 0) {
      alert('אנא בחר לפחות מוצר אחד');
      return;
    }

    this.loading = true;
    
    // בניית הבקשה לפי המודל
    const request: RecipeRequest = {
      ingredients: selectedIngredients,
      preferences: this.preferences,
      servings: this.servings
    };

    console.log('Sending request to server:', request);

    this.recipeService.generateRecipe(request).subscribe({
      next: (response: RecipeResponse) => {
        this.recipeResponse = response;
        this.loading = false;
        console.log('Recipe received:', response);
      },
      error: (error) => {
        console.error('Error generating recipe:', error);
        alert('נכשל ביצירת המתכון. נסה שוב.');
        this.loading = false;
      }
    });
  }
}