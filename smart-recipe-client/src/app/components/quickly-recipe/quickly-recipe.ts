import { CommonModule } from '@angular/common';
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { RecipeResponse } from '../../models/recipe-response';
import { PreferencesComponent } from '../preferences/preferences';

@Component({
  selector: 'app-quickly-recipe',
  templateUrl: './quickly-recipe.html',
  styleUrls: ['./quickly-recipe.css'],
  standalone: true,
  imports: [CommonModule]

})
export class QuicklyRecipe {
  @Input() isLoading: boolean = false;
  @Input() error: string | null = null;
  @Input() selectedProducts: string[] = [];
  @Input() servings: number = 2;
  @Input() recipe: RecipeResponse | null = null;
  
  @Output() backToProducts = new EventEmitter<void>();
  @Output() createNewRecipe = new EventEmitter<void>();

  goBack(): void {
    this.backToProducts.emit();
  }

  createNew(): void {
    this.createNewRecipe.emit();
  }

  // Getters לתצוגה
  get displayTitle(): string {
    return this.recipe?.title || ;
  }

  get displayDescription(): string {
    return this.recipe?.description || ;
  }

  get displayIngredients(): string[] {
    return this.recipe?.ingredients || ;
  }

  get displaySteps(): string[] {
    if (this.recipe?.steps && this.recipe.steps.length > 0) {
      return this.recipe.steps;
    }
   
  }

  get displayServings(): number {
    return this.recipe?.servings || ;
  }
}